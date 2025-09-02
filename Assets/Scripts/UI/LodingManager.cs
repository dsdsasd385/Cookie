using System.Collections;
using Photon.Realtime;  // 이 줄을 추가해야 합니다.
using Photon.Pun;
using UnityEngine;
using TMPro;

public class LoadingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject loadingPanel;  // 로딩 패널
    [SerializeField] private TextMeshProUGUI waitingText;  // Waiting... 텍스트
    private bool isWaiting = true;

    void Start()
    {
        ShowLoading(); // 시작할 때 로딩창 표시
        StartCoroutine(AnimateDots()); // "Waiting..." 애니메이션
        ConnectToPhoton();  // Photon 서버에 연결
    }

    // Photon 서버에 연결
    private void ConnectToPhoton()
    {
        // Photon 서버에 연결되지 않았을 때만 연결 시도
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Photon에 연결 중...");
            PhotonNetwork.ConnectUsingSettings();  // Photon 서버에 연결
        }
        else
        {
            Debug.Log("이미 Photon 서버에 연결되어 있습니다.");
            // 이미 연결된 경우 로비로 바로 접속
            JoinLobby();
        }
    }

    // 로비에 접속
    private void JoinLobby()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("로비에 접속 중...");
            PhotonNetwork.JoinLobby();  // 로비에 접속
        }
        else
        {
            Debug.LogWarning("Photon 서버에 연결되지 않았습니다.");
        }
    }

    // 로딩 패널 표시
    public void ShowLoading()
    {
        loadingPanel.SetActive(true);
    }

    // 로딩 패널 숨기기
    public void HideLoading()
    {
        isWaiting = false; // 애니메이션 정지
        loadingPanel.SetActive(false);
    }

    // Waiting... 애니메이션
    IEnumerator AnimateDots()
    {
        string baseText = "Waiting";
        int dotCount = 0;

        while (isWaiting)
        {
            waitingText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4; // 점 개수 (0~3)
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Photon 서버 연결 후 로비에 접속
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Photon 서버에 연결되었습니다!");
        PhotonNetwork.JoinLobby();  // 로비에 접속
    }

    // 로비 접속 완료 시 호출
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("로비에 접속되었습니다!");
        HideLoading();  // 로딩 화면 숨기기
    }

    // Photon 서버와의 연결 실패 시 호출
    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.LogError("서버와의 연결이 끊어졌습니다: " + cause);
        waitingText.text = "서버와의 연결이 끊어졌습니다!";
        HideLoading();
    }

    // 연결 실패 시 표시할 메시지
    public void OnFailedToConnectToPhoton(string message)
    {
        Debug.LogError("Photon 연결 실패: " + message);
        waitingText.text = "서버 연결 실패!";
        HideLoading();
    }
}
