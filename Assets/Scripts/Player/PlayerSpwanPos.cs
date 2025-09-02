using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerSpwanPos : MonoBehaviourPun
{
    [SerializeField] SpawnManager spawnManager;
    [SerializeField] GameObject playerPrefab;

    public PlayerAvatar MyAvatar { get; private set; }

    private List<Vector3> aiPosition = new List<Vector3>();

    Vector3 playerPosition = new Vector3(0,0,0);


    private int actorNumber;

    void Start()
    {

    }

    public void PlayerSetPosition(List<Vector3> _spawnPositionList)
    {
        int playerCount = PhotonNetwork.CurrentRoom.Players.Count;

        List<int> actorNumbers = new List<int>();

        // 현재 룸에 있는 모든 플레이어의 ActorNumber 가져오기
        foreach (var player in PhotonNetwork.PlayerList)
        {
            actorNumbers.Add(player.ActorNumber);
        }


        for (int i = 0; i < playerCount; i++)
        {
            int randomIndex = Random.Range(0, _spawnPositionList.Count);

            playerPosition = _spawnPositionList[randomIndex];
            //_spawnPositionList.Remove(playerPosition);
            _spawnPositionList.RemoveAt(randomIndex);


            //actorNumber = PhotonNetwork.PlayerList[i].ActorNumber;
            actorNumber = actorNumbers[i];

            // 플레이어 생성
            photonView.RPC("PlayerSpawn", RpcTarget.AllBuffered, actorNumber, playerPosition);
        }
    }

    // 플레이어 생성
    [PunRPC]
    private void PlayerSpawn(int actorNumber, Vector3 position)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            GameObject avatarObject = PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity);
            MyAvatar = avatarObject.GetComponent<PlayerAvatar>();

            MyAvatar.photonView.RPC("SetNickname", RpcTarget.AllBuffered, PhotonNetwork.NickName, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

}
