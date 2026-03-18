using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private List<SpawnAreaManager> spawnAreaManager = new List<SpawnAreaManager>();
    [SerializeField] float distance = 3;
    [SerializeField] GameObject aiPrefab;
    [SerializeField] PlayerSpawnPos playerSpawnPos;


    public List<Vector3> _spawnPositionList = new List<Vector3>();
    private List<GameObject> aiList = new List<GameObject>(); // AI 객체 리스트
    public int _playerCount;
    public Dictionary<int, AvatarSetNumber> pvViewId = new Dictionary<int, AvatarSetNumber>();


    #region Singleton
    private static SpawnManager _instance;

    public static SpawnManager Instance
    {
        get
        {
            // 인스턴스가 없으면 씬에서 SpawnManager를 찾아 설정
            if (_instance == null)
                _instance = FindObjectOfType<SpawnManager>();

            return _instance;
        }
    }
    #endregion

    private void Start()
    {
        _playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        // 마스터 클라이언트가 실행
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SetupSpawnPoints", RpcTarget.All, distance);
        }
    }

    [PunRPC]
    private void SetupSpawnPoints(float distanceEach)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        _spawnPositionList.Clear();

        foreach (var teacherarea in spawnAreaManager)
        {
            var spawnPositions = teacherarea.GetSpawnPositionList(distanceEach);
            _spawnPositionList.AddRange(spawnPositions);
        }

        // 플레이어 포지션 할당 및 생성
        playerSpawnPos.PlayerSetPosition(_spawnPositionList);

        // AI 생성
        foreach (var position in _spawnPositionList)
        {
            // Room Object로 생성
            GameObject aiInstance = PhotonNetwork.InstantiateRoomObject(aiPrefab.name, position, Quaternion.identity);
            aiList.Add(aiInstance); // 생성된 AI를 리스트에 추가
        }
    }

    // 마스터 클라이언트 변경 시 호출됨
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            TransferOwnershipToNewMaster();
        }
    }

    // AI의 소유권을 새로운 마스터에게 이전
    private void TransferOwnershipToNewMaster()
    {
        foreach (var ai in aiList)
        {
            if (ai != null)
            {
                PhotonView aiPhotonView = ai.GetComponent<PhotonView>();

                if (aiPhotonView != null && aiPhotonView.Owner != PhotonNetwork.LocalPlayer)
                {
                    aiPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                }
            }
        }
    }
}