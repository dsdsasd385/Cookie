using Photon.Pun;
using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // 코루틴 사용을 위해 필요

public class CustomAvatarController : MonoBehaviourPunCallbacks
{
    public bool canMove = true;           // 이동 가능 여부 (공격 중에는 false)
    public float moveSpeed = 5f;          // 걷기 속도
    public float sprintSpeed = 8f;        // 달리기 속도
    public float rotationSpeed = 10f;
    public float cameraDistance = 4f;
    public float cameraHeight = 2f;
    public float attackDuration = 1.8f;     // 공격 애니메이션 지속 시간 (초)
    [SerializeField] private GameObject weapon;
    [SerializeField] private Canvas gameover;
    [SerializeField] private Canvas win;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject myMesh;
    [SerializeField] private BoxCollider myCollider;


    [Header("Attack")]
    [SerializeField] private AudioClip attackClip;
    [Header("Non Attack")]
    [SerializeField] private AudioClip nonAttackClip;

    [Header("Waik")]
    [SerializeField] private AudioClip walkClip;
    
    [Header("Damaged")]
    [SerializeField] private AudioClip damagedClip;
    [Header("Die")]
    [SerializeField] private AudioClip dieClip;


    private PhotonView _pv;
    private AudioSource _audioSource;
    private Animator _animator;
    private bool _isRunning;
    private bool _isAttacking;
    private bool _isWin = false;
    private float _currentYRotation = 0f;
    private bool _isDead = false;
    private int _playerConut;
    private RaycastHit hit;
    
    [Space(25)]
    
    public Vector3 boxSize = new Vector3(0.5f, 0.5f, 0.5f);   // 박스의 크기
    public Vector3 castDirection = Vector3.forward;   // 캐스트 방향
    public float castDistance = 3f;

    // 상태를 참조할 enum 생성
    public enum AvatarState
    {
        Idle,
        Move,
        Attack
    }
    private AvatarState currentState = AvatarState.Idle; // 기본 상태는 Idle

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _pv = GetComponent<PhotonView>();


        if (attackClip == null) attackClip = Resources.Load<AudioClip>("Sound/Attack");
        if (nonAttackClip == null) nonAttackClip = Resources.Load<AudioClip>("Sound/NonAttack");
        if (walkClip == null) walkClip = Resources.Load<AudioClip>("Sound/Walk");
        if (damagedClip == null) damagedClip = Resources.Load<AudioClip>("Sound/Damaged");
        if (dieClip == null) dieClip = Resources.Load<AudioClip>("Sound/Die");
        

        if (photonView.IsMine)
        {
            if (_mainCamera != null)
            {
                GameObject cameraObject = new GameObject("PlayerCamera");
                _mainCamera = cameraObject.AddComponent<Camera>();
                _mainCamera.tag = "MainCamera"; // 카메라의 태그를 "MainCamera"로 설정
            }
            if (_animator == null)
            {
                Debug.LogError("Animator가 없습니다! 프리팹에 Animator를 추가하세요.", gameObject);
            }
        }
        else
        {
            _mainCamera = GetComponentInChildren<Camera>();
            if (_mainCamera != null)
            {
                _mainCamera.gameObject.SetActive(false); // 다른 플레이어의 카메라는 비활성화
                _animator = null;
            }
        }
    }

    private void Start()
    {
        _isDead = false;
        _isWin = false;
        if (photonView.IsMine)
        {
            _animator.SetBool("isDie", false);
        }
    }
    private void Update()
    {
        HandlePlayerController();
        if (SceneManager.GetActiveScene().name != "Map") return;

        // 로컬 플레이어이고 아직 죽지 않았다면 승리 조건 체크
        if (photonView.IsMine && !_isDead)
        {
            // PhotonNetwork.CurrentRoom.PlayerCount가 1이면 마지막 플레이어임
            if (SpawnManager.Instance._playerCount <= 1)
            {
                _isWin = true;
                Win();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Quaternion characterRotation = transform.rotation;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + characterRotation * castDirection * castDistance / 2, boxSize);

        RaycastHit hit;
        if (Physics.BoxCast(transform.position, boxSize / 2, characterRotation * castDirection, out hit, characterRotation, castDistance))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hit.point, 0.2f);
        }
    }

    // 플레이어 제어 (애니메이션 및 기능)
    private void HandlePlayerController()
    {
        if (!photonView.IsMine || _isDead) return;

        FreeCamControl();

        _isRunning = Input.GetKey(KeyCode.LeftShift);
        _animator.SetBool("isRunning", _isRunning);

        if (Input.GetMouseButtonDown(0) && !_isAttacking)
        {
            HandleAttack();
            return;
        }

        if (!_isAttacking && canMove)
        {
            HandleMovement();
        }
    }

    // 플레이어 제어 (이동)
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            Vector3 cameraForward = _mainCamera.transform.forward;
            Vector3 cameraRight = _mainCamera.transform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();
            Vector3 moveDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            float speed = _isRunning ? sprintSpeed : moveSpeed;

            SetState(AvatarState.Move);
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        }
        else
        {
            SetState(AvatarState.Idle);
        }
    }

    // 플레이어의 카메라 제어
    private void FreeCamControl()
    {
        float mouseX = Input.GetAxis("Mouse X");
        _currentYRotation += mouseX * rotationSpeed;
        Quaternion camRotation = Quaternion.Euler(0f, _currentYRotation, 0f);
        _mainCamera.transform.position = transform.position - camRotation * Vector3.forward * cameraDistance + Vector3.up * cameraHeight;
        _mainCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    #region AudioClip

    [PunRPC]
    private void AttackAudio()
    {
        _audioSource.PlayOneShot(attackClip);
    }

    [PunRPC]
    private void NonAttackAudio()
    {
        _audioSource.PlayOneShot(nonAttackClip);
    }
    [PunRPC]
    private void WalkAudio()
    {
        _audioSource.PlayOneShot(walkClip);
    }

    [PunRPC]
    private void DamagedAudio()
    {
        _audioSource.PlayOneShot(damagedClip);
    } 
                
    [PunRPC]
    private void DieAudio()
    {
        _audioSource.PlayOneShot(dieClip);
    }

    #endregion

    // 플레이어 공격 메서드
    private void HandleAttack()
    {
        _isAttacking = true;
        canMove = false;
        SetState(AvatarState.Attack);
        _animator.SetTrigger("Attack");

        _pv.RPC("AttackAudio", RpcTarget.All);

        StartCoroutine(WaitForAttackAnimationToEnd());
    }

    // 플레이어 공격 코루틴 세팅
    private IEnumerator WaitForAttackAnimationToEnd()
    {
        yield return new WaitForSeconds(attackDuration);

        _isAttacking = false;
        canMove = true;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical);
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            SetState(AvatarState.Move);
        }
        else
        {
            SetState(AvatarState.Idle);
        }
    }

    // enum(상태)에 맞춰 애니메이션 Value세팅
    private void SetState(AvatarState newState)
    {
        if (_animator == null) return;

        if (currentState == newState) return;
        currentState = newState;

        switch (currentState)
        {
            case AvatarState.Idle:
                _animator.SetBool("isWalking", false);
                _animator.SetBool("isRunning", false);
                break;
            case AvatarState.Move:
                _animator.SetBool("isWalking", true);
                break;
            case AvatarState.Attack:
                _animator.SetBool("isWalking", false);
                _animator.SetBool("isRunning", false);
                break;
        }
    }

    // 승리 메서드
    private void Win()
    {
        // _isWin가 true이고, 아직 죽지 않았다면
        if (_isWin && !_isDead && photonView.IsMine)
        {
            win.gameObject.SetActive(true);
            UnlockCursor();
            GameManager.Instance.isWin = true;
        }
    }

    // 사망 메서드
    public void Die()
    {
        if (myMesh == null) return;

        // 맞는소리 재생
        _pv.RPC("DamagedAudio", RpcTarget.All);

        if (photonView.IsMine)
        {
            StartCoroutine(DieAnim());
            GameManager.Instance.isDie = true;
        }
    }


    // 사망 애니메이션 코루틴
    private IEnumerator DieAnim()
    {
        _isDead = true;
        _animator.SetBool("isDie", true);
        _animator.SetTrigger("Die");

        yield return new WaitForSeconds(1.5f);

        // 죽는소리 재생
        _pv.RPC("DieAudio", RpcTarget.All);

        yield return new WaitForSeconds(3.5f);

        // 모든 클라이언트에서 메쉬와 콜라이더를 비활성화하도록 RPC 호출
        photonView.RPC("DisableMesh", RpcTarget.All);

        photonView.RPC("DecreasePlayerCountRPC", RpcTarget.All);

        // 게임 오버 UI는 로컬에서만 표시 (원하는 경우 모든 클라이언트에서 활성화하려면 RPC 사용)
        if (photonView.IsMine)
        {
            gameover.gameObject.SetActive(true);
            UnlockCursor();
        }
    }
    // 마우스 잠금메서드
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    // 플레이어 수 체크 메서드(Photon)
    [PunRPC]
    private void DecreasePlayerCountRPC()
    {
        SpawnManager.Instance._playerCount--;
    }

    // 사망시 Mesh 안보이게하는 메서드 (Photon)
    [PunRPC]
    private void DisableMesh()
    {
        // 모든 클라이언트에서 실행되도록 if문 제거
        myMesh.SetActive(false);
        myCollider.enabled = false;
    }

    // pvViewId에 값이 일치하는 인스턴스의 Die 실행
    [PunRPC]
    private void LocalDie(int viewId)
    {
        Dictionary<int, AvatarSetNumber> dictionaryViewId = SpawnManager.Instance.pvViewId;
        if (dictionaryViewId.TryGetValue(viewId, out AvatarSetNumber avatar))
        {
            CustomAvatarController customAvatarController = avatar.GetComponent<CustomAvatarController>();
            customAvatarController.Die();
        }
    }

    private void DieRPC(int viewId)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("LocalDie", RpcTarget.All, viewId);
        }
    }

    // Raycast로 Attack이벤트 실행 메서드
    public void AttackEnemy()
    {
        if (Physics.BoxCast(transform.position, boxSize / 2, transform.forward * 0.5f, out hit, Quaternion.identity, castDistance))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                var pv = hit.collider.gameObject.GetComponent<PhotonView>();
                var id = pv.ViewID;
                if (SceneManager.GetActiveScene().name == "Map")
                {
                    CustomAvatarController customAvatarController = hit.collider.gameObject.GetComponent<CustomAvatarController>();

                    if (customAvatarController._isDead == true)
                    {
                        return;
                    }
                    else
                    {
                        // 공격소리 재생
                        _pv.RPC("Attack", RpcTarget.All);

                        DieRPC(id);
                        Debug.Log("Player Attack");
                    }
                }
            }
            else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ai"))
            {
                var pv = hit.collider.gameObject.GetComponent<PhotonView>();
                var id = pv.ViewID;

                AIWalker aiwalker = hit.collider.gameObject.GetComponent<AIWalker>();

                if (aiwalker == null) { return; }

                if (SceneManager.GetActiveScene().name == "Map")
                {
                    // 공격소리 재생
                    _pv.RPC("Attack", RpcTarget.All);
                    Debug.Log("Ai Attack");
                    aiwalker.DieRPC(id);
                }
            }

            else
            {
                // NonAttack소리 재생
                _pv.RPC("NonAttack", RpcTarget.All);
            }
        }
    }

}


