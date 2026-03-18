using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class InputField : MonoBehaviourPunCallbacks
{
    [SerializeField] private UnityEngine.UI.InputField inputField;
    [SerializeField] private Button joinButton;
    [SerializeField] private Text connectInfoText;

    public static string myNickName;

    private void Awake()
    {
        // 버튼과 인풋 필드의 이벤트 등록
        joinButton.onClick.AddListener(OnJoinButtonPressed);
        inputField.onSubmit.AddListener(OnEnterPressed);
    }

    private void Start()
    {
        // 입장 버튼 비활성화, 접속 중 메시지 표시
        joinButton.interactable = false;
        connectInfoText.text = "서버에 접속중입니다...";

        // 로비 씬에 진입했을 때 네트워크 상태 정리를 위해 코루틴 실행
        StartCoroutine(ReconnectToServer());
    }
    private IEnumerator ReconnectToServer()
    {
        // 기존 방에 남아있다면 방을 나가기
        if (PhotonNetwork.InRoom)
        {
            connectInfoText.text = "방을 떠나는 중...";
            PhotonNetwork.LeaveRoom();
            while (PhotonNetwork.InRoom)
            {
                yield return null; // 방에서 나갈 때까지 대기
            }
        }

        // 연결이 안 되어 있으면 서버에 자동 연결
        if (!PhotonNetwork.IsConnected)
        {
            connectInfoText.text = "서버에 연결 중...";
            PhotonNetwork.ConnectUsingSettings();

            while (!PhotonNetwork.IsConnected)
            {
                yield return null; // 서버 연결을 기다림
            }
        }

        // Master Server 연결 대기
        while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            yield return null;
        }

        connectInfoText.text = "서버 연결 완료! 방에 입장하려면 버튼을 눌러주세요.";
        joinButton.interactable = true; // 방 입장 버튼 활성화
    }


    private IEnumerator ResetNetworkIfNeeded()
    {

        // 기존 방에 남아있다면 방을 나가기
        if (PhotonNetwork.InRoom)
        {
            connectInfoText.text = "방을 떠나는 중...";
            PhotonNetwork.LeaveRoom();
            while (PhotonNetwork.InRoom)
            {
                yield return null; // 방에서 나갈 때까지 대기
            }
        }

        // 연결이 안 되어 있으면 접속 버튼을 활성화
        if (!PhotonNetwork.IsConnected)
        {
            connectInfoText.text = "서버 접속을 시작하려면 버튼을 눌러주세요.";
            joinButton.interactable = true; // 버튼 활성화
        }
        else
        {
            // 이미 연결된 상태라면 바로 서버 접속 완료 처리
            while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
            {
                yield return null; // Master Server 연결을 기다림
            }

            connectInfoText.text = "서버 접속 완료! 방에 입장할 준비가 되었습니다.";
        }

        JoinRoom();
    }



    // 서버에 성공적으로 연결되었을 때 자동 호출되는 콜백
    public override void OnConnectedToMaster()
    {
        Debug.Log("서버에 성공적으로 연결되었습니다.");
        joinButton.interactable = true;  // 버튼 활성화
        connectInfoText.text = "서버 접속 완료!\nJOIN 버튼을 눌러 방에 입장하세요.";
    }

    // 서버 연결이 끊겼을 때 호출되는 콜백
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"서버 연결이 끊어졌습니다. 원인: {cause}");
        joinButton.interactable = false;  // 버튼 비활성화
        connectInfoText.text = "서버와 연결이 끊겼습니다.\n재접속 시도 중...";
        // 재접속 시도는 OnDisconnected 콜백에서 한 번 호출하도록 처리
        PhotonNetwork.ConnectUsingSettings();
    }

    // Join 버튼 클릭 시 실행되는 함수
    private void OnJoinButtonPressed()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            connectInfoText.text = "닉네임을 입력해주세요.";
            return;
        }

        PhotonNetwork.NickName = inputField.text;
        joinButton.interactable = false; // 중복 클릭 방지

        connectInfoText.text = "서버 연결 확인 중...";
        StartCoroutine(ResetNetworkIfNeeded());
    }


    // 엔터키 입력 시 실행되는 함수
    private void OnEnterPressed(string inputText)
    {
        if (string.IsNullOrEmpty(inputText))
        {
            connectInfoText.text = "닉네임을 입력해주세요";
            return;
        }

        inputField.interactable = false;
        PhotonNetwork.NickName = inputText;
        // JoinRoom() 호출을 OnConnectedToMaster에서만 하도록 수정
    }

    // 룸 입장 시도 함수
    private void JoinRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            connectInfoText.text = "룸에 입장을 시도할게요.";

            // 방에 랜덤 입장하거나, 방이 없으면 새로 생성하기
            PhotonNetwork.JoinRandomRoom();  // 랜덤 입장 시도
        }
        else
        {
            connectInfoText.text = "서버와 접속이 끊겼어요.";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // 랜덤 방 입장에 실패했을 때 호출되는 함수
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connectInfoText.text = "입장 가능한 방이 없습니다. 방을 생성합니다...";
        // 방이 없을 경우 방을 새로 생성
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 8 });
    }

    // 방에 입장한 후 호출되는 콜백
    public override void OnJoinedRoom()
    {
        connectInfoText.text = "방에 입장했어요.\n메인 씬으로 전환할게요.";
        PhotonNetwork.LoadLevel("Wait");
    }
}
