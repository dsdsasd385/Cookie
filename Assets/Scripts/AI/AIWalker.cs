using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AIWalker : MonoBehaviourPunCallbacks
{
    private static Dictionary<int, AIWalker> _aiList = new();

    public float moveRadius = 10f;
    private NavMeshAgent agent;
    private Animator _animator;
    private Vector3 startPosition;

    private bool _isDead;
    private void Awake()
    {
        var view = gameObject.GetComponent<PhotonView>();

        if (!_aiList.ContainsKey(view.ViewID))
        {
            _aiList.Add(view.ViewID, this);
        }
        else
        {
            // 이미 존재하면 값을 업데이트하거나 무시할 수 있습니다.
            _aiList[view.ViewID] = this;
            // 또는 필요에 따라 _aiList.Remove(view.ViewID) 후 추가할 수도 있습니다.
        }

        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        startPosition = transform.position;
    }
    void Start()
    {
        _isDead = false;
        agent.isStopped = false;

        MoveToRandomPosition();
    }

    void Update()
    {
        if (_isDead == false)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                MoveToRandomPosition();
            }

            bool isWalking = agent.velocity.magnitude > 0.1f;
            if (_animator != null)
            {
                _animator.SetBool("isWalking", isWalking);
            }
        }       
    }

    void MoveToRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * moveRadius;
        randomDirection += startPosition;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, moveRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public void DieRPC(int viewId)
    {
        photonView.RPC("LocalDie", RpcTarget.All, viewId);
    }

    [PunRPC]
    private void LocalDie(int viewId)
    {
        if(_aiList.TryGetValue(viewId, out AIWalker ai))
        {
            if (_isDead == true) return;

            ai._isDead = true;
            ai.OnDie();
        }
    }

    public void Die()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(DieAnim());
        }
    }

    [PunRPC]
    private void PlayDieAnimation()
    {
        _animator.SetTrigger("Die");
    }

    private void OnDie()
    {
        StartCoroutine(DieAnim());
    }

    private IEnumerator DieAnim()
    {
        photonView.RPC("PlayDieAnimation", RpcTarget.All); // 모든 클라이언트에서 애니메이션 실행
        agent.isStopped = true;  // 네비메쉬 즉시 정지

        yield return new WaitForSeconds(3.5f);

        //PhotonNetwork.Destroy(gameObject);
        if (PhotonNetwork.IsMasterClient)
        {
            if (!photonView.IsMine)
            {
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer); // 마스터 클라이언트가 소유권 가져오기
            }

            PhotonNetwork.Destroy(gameObject); // 네트워크에서 AI 삭제
        }
    }
}
