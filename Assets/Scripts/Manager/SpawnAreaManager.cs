using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SpawnAreaManager : MonoBehaviour
{
    public int spawnCount;  // 생성할 개수

    private BoxCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    public List<Vector3> GetSpawnPositionList(float distance)
    {
        List<Vector3> positionList = new();
        Bounds bounds = _collider.bounds;

        int attempts = 0;  // 무한 루프 방지용 카운터

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos;
            bool isValid;

            do
            {
                if (attempts > spawnCount * 10)  // 무한 루프 방지 (10배수 시도 후 중단)
                {
                    return positionList;
                }

                float posX = Random.Range(bounds.min.x, bounds.max.x);
                float posZ = Random.Range(bounds.min.z, bounds.max.z);
                spawnPos = new Vector3(posX, 0.5f, posZ);

                isValid = true;

                foreach (var pos in positionList)
                {
                    if (Vector3.Distance(spawnPos, pos) < distance)
                    {
                        isValid = false;
                        break;
                    }
                }

                attempts++;  // 시도 횟수 증가
            } while (!isValid);

            positionList.Add(spawnPos);
        }

        return positionList;
    }

}