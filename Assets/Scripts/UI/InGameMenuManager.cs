using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class InGameMenuManager : MonoBehaviourPunCallbacks
{
    public GameObject pauseMenuUI;  // 게임 일시 정지 UI

    // 게임 종료 또는 메인 화면으로 돌아가는 함수
    public void ReturnToMainMenu()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.LeaveRoom();  // 방을 떠나기 전에 상태 확인
            Invoke(nameof(LoadMainMenu), 1f); // 1초 후 LoadMainMenu 호출
        }
        else
        {
            LoadMainMenu();  // Photon 연결되지 않으면 바로 로드
        }
    }

    // 메인 메뉴 씬 로드
    private void LoadMainMenu()
    {
        SceneManager.LoadScene("LobbyScene");
        PhotonNetwork.ConnectUsingSettings();  // Photon 서버에 다시 연결
    }

    // 로비에 접속된 후 새로운 방을 생성
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();  // 로비에 접속
    }

    // 로비에 접속된 후 새로운 방을 생성
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);  // 랜덤 닉네임 설정
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });  // 새로운 방 생성
    }

    // 게임 종료
    public void QuitGame()
    {
        Debug.Log("게임 종료");
        Application.Quit();  // 게임 종료
    }

    void Update()
    {
        // 스페이스바를 눌렀을 때 게임을 일시 정지하고 메뉴 표시
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.isWin || GameManager.Instance.isDie)
            {
                return;
            }
            else
            {
                bool isMenuActive = pauseMenuUI.activeSelf;  // 현재 메뉴 상태 확인
                TogglePauseMenu(!isMenuActive);  // 상태 반전시켜서 호출
            }
        }
    }

    // PauseMenu UI 활성화/비활성화
    void TogglePauseMenu(bool isActive)
    {
        pauseMenuUI.SetActive(isActive);  // 게임 일시 정지 UI 활성화/비활성화
    }

    // 마스터 클라이언트가 변경될 때 호출되는 콜백
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        Debug.Log("새로운 마스터 클라이언트: " + newMasterClient.NickName);
    }

    // 플레이어가 방을 떠날 때 호출되는 콜백
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (SceneManager.GetActiveScene().name == "Map")
        {
            // SpawnerManager에서 _playerCount를 -1 처리
            SpawnManager.Instance._playerCount--;
        }           
    }
}
