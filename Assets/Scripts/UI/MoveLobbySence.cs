using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class MoveLobbySence : MonoBehaviourPunCallbacks
{
    public void LoadScene()
    {
        // 씬 이동을 먼저 수행
        StartCoroutine(MoveToLobby());
    }

    private IEnumerator MoveToLobby()
    {
        if (photonView.IsMine)
        {
            SceneManager.LoadScene("LobbyScene");  // 씬 이동
            yield return null;  // 다음 프레임까지 대기

            // 씬 이동 후 네트워크 연결 해제
            PhotonNetwork.LeaveRoom();
        }      
    }
}



