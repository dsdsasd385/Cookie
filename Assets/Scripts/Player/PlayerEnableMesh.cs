using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerEnableMesh : MonoBehaviourPun
{
    [SerializeField] GameObject myMesh;
    [SerializeField] BoxCollider myCollider;


    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "LobbyScene")
        {
            photonView.RPC("EnableMesh", RpcTarget.All);
            GameManager.Instance.isDie = false;
            GameManager.Instance.isWin = false;
        }

    }


    [PunRPC]
    public void EnableMesh()
    {
        myMesh.SetActive(true);
        myCollider.enabled = true;
    }

}
