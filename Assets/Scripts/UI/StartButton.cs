using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class StartButton : MonoBehaviourPunCallbacks
{
    [PunRPC]
    public void Click()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;

            PhotonNetwork.LoadLevel("Map");
        }
        else
        {
            photonView.RPC("LoadMainScene", RpcTarget.MasterClient);
        }
    }
}
