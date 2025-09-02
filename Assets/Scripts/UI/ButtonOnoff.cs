using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOnoff : MonoBehaviourPunCallbacks
{

    [SerializeField] private GameObject button;
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            button.SetActive(true);
        }
    }
}
