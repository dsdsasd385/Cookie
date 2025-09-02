using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject button;

    // Update is called once per frame
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // 새로운 마스터가 되었을 때 실행할 로직 추가
        if (PhotonNetwork.IsMasterClient)
        {
            button.SetActive(true);

            Debug.Log("내가 새로운 마스터가 되었습니다.");
        }
    }
}

