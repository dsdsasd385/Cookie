using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerList : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text text;

    private void Start()
    {
        UpdatePlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"새로운 플레이어 입장: {newPlayer.NickName}");
        photonView.RPC("UpdatePlayerList", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"플레이어 퇴장: {otherPlayer.NickName}");
        photonView.RPC("UpdatePlayerList", RpcTarget.All);
    }

    [PunRPC]
    private void UpdatePlayerList()
    {
        text.text = $"Player : {PhotonNetwork.CurrentRoom.PlayerCount}";
    }
}
