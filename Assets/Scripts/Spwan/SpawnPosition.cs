using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class SpawnPosition : MonoBehaviourPun
{
    [SerializeField] private GameObject prefab;

    [SerializeField] private List<Collider> spawnZone;

    private List<Vector3> _positionList = new List<Vector3>();
    // 스폰 콜라이더 범위
    private List<Bounds> _spawnZonebounds = new List<Bounds>();
    private int _distance = 3;
    private int _prefabCount = 8;
    private bool isValidPosition;
    private SpawnManager _spawnManager;


    private void Start()
    {
        SpwanZone();
    }

    [PunRPC]
    private void SpwanZone()
    {
        for (int i = 0; i < spawnZone.Count; i++)
        {
            // bounds 리스트로 할당
            _spawnZonebounds.Add(spawnZone[i].bounds);

            // bounds 리스트에서 랜덤 포지션 생성
            for (int j = 0; j < _prefabCount; j++)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(_spawnZonebounds[i].min.x, _spawnZonebounds[i].max.x),
                    0,
                    Random.Range(_spawnZonebounds[i].min.z, _spawnZonebounds[i].max.z)
                );

                bool isValidPosition = true;

                // 이전 포지션들과 비교하여 최소 거리 3 이상 떨어져 있는지 체크
                foreach (Vector3 pos in _positionList)
                {
                    float distance = Vector3.Distance(randomPosition, pos);

                    if (distance < _distance) // 만약 3보다 가까운 거리가 있으면
                    {
                        isValidPosition = false;
                        break; // 더 이상 비교할 필요 없으므로 종료
                    }
                }

                // 유효한 위치일 때만 리스트에 추가
                if (isValidPosition)
                {
                    _positionList.Add(randomPosition);
                }
                else
                {
                    j--; // 유효하지 않으면 다시 시도
                }
            }
        }
        Debug.Log(_positionList.Count);
        AiSpwan();
    }

    private void AiSpwan()
    {
        for(int i = 0; i < _positionList.Count; i++)
        {
            Instantiate(prefab,_positionList[i], Quaternion.identity);
        }
    }

}
