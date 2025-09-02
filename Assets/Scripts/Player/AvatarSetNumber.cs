using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AvatarSetNumber : MonoBehaviour
{


    private void Awake()
    {
        if (PhotonNetwork.IsConnected && SceneManager.GetActiveScene().name == "Map")
        {
            PhotonView pv = GetComponent<PhotonView>();
            int viewId = pv.ViewID;
            SpawnManager.Instance.pvViewId.Add(viewId, this);
        }
    }
}
