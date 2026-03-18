using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region SingleTon
    // 싱글톤 패턴 구현: 게임 내에서 GameManager의 단일 인스턴스를 보장
    private static GameManager _instance;


    public bool isWin = false;
    public bool isDie = false;

    public static GameManager Instance
    {
        get
        {
            // 인스턴스가 없으면 씬에서 GameManager를 찾아 설정
            if (_instance == null)
                _instance = FindObjectOfType<GameManager>();

            return _instance;
        }
    }
    #endregion

    // 현재 플레이어의 아바타 정보를 저장할 변수
    public PlayerAvatar MyAvatar { get; set; }
    private string _myNickName;

    // Cinemachine 가상 카메라 참조 변수
    CinemachineVirtualCamera followCamera;

    [SerializeField] public Canvas gameover;

    private void Awake()
    {
        _myNickName = InputField.myNickName;
        // 씬 내에서 Cinemachine 가상 카메라를 찾아 참조 설정
        followCamera = FindObjectOfType<CinemachineVirtualCamera>();
    }

    // 메인 씬이 로드된 후 호출되는 초기화 함수
    public void Start()
    {
        // Wait 씬일 때만 실행
        if (SceneManager.GetActiveScene().name == "Wait")
        {            
            // 플레이어 아바타를 생성
            SpawnAvatar();
        }
    }

    private void SpawnAvatar()
    {
        // -1부터 1 사이의 랜덤한 x, z 좌표를 생성
        // y 좌표는 0으로 고정 (지면 위에 배치)
        float x = Random.Range(-1f, 1f);
        float y = 0f;
        float z = Random.Range(-1f, 1f);

        // 랜덤한 스폰 위치 설정
        Vector3 randomSpwanPosition = new Vector3(x, y, z);

        // PhotonNetwork.Instantiate를 사용하여 네트워크 상에 "PlayerAvatar" 프리팹 생성
        // randomSpawnPosition에서 생성되며 회전 값은 Quaternion.identity(기본값)
        GameObject avatarObject = PhotonNetwork.Instantiate("PlayerAvatar", randomSpwanPosition, Quaternion.identity);

        // 생성된 아바타의 PlayerAvatar 컴포넌트를 가져와 MyAvatar에 저장
        MyAvatar = avatarObject.GetComponent<PlayerAvatar>();

        // 모든 클라이언트에 닉네임과 ActorNumber를 설정하는 RPC 호출
        MyAvatar.photonView.RPC("SetNickname", RpcTarget.AllBuffered, PhotonNetwork.NickName, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public override void OnJoinedRoom()
    {
        if (MyAvatar == null)
        {
            // 아바타가 없으면 새로 생성
            SpawnAvatar();
        }
        else
        {
            Destroy(MyAvatar.gameObject);
            MyAvatar = null;
            SpawnAvatar();
        }
    }
}