using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    [SerializeField] private int prefabCount;
    [SerializeField] private int maxAttempts = 1000;       // 각 위치를 찾기 위한 최대 시도 횟수


    private Bounds _bounds;
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    public List<Vector3> SetPos(float distance)
    {
        _bounds = _collider.bounds;

        List<Vector3> prefabsPos = new List<Vector3>();

        // prefabCount 만큼 위치를 생성하기 위한 루프
        for (int i = 0; i < prefabCount; i++)
        {
            int attempts = 0;
            bool isValidPosition = false;
            Vector3 randomPosition = Vector3.zero;

            // 최대 maxAttempts번까지 유효한 위치를 찾기 위한 while 루프
            while (attempts < maxAttempts && !isValidPosition)
            {
                // 영역 내 랜덤 위치 생성
                randomPosition = new Vector3(
                    Random.Range(_bounds.min.x, _bounds.max.x),
                    0,
                    Random.Range(_bounds.min.z, _bounds.max.z)
                );

                isValidPosition = true;

                // 이미 저장된 위치들과의 거리 검사
                foreach (Vector3 pos in prefabsPos)
                {
                    if (Vector3.Distance(randomPosition, pos) < distance)
                    {
                        isValidPosition = false;
                        break; // 거리가 너무 가까우면 break
                    }
                }
                attempts++;
            }

            if (isValidPosition)
            {
                // 유효한 위치를 찾았으면 리스트에 추가
                prefabsPos.Add(randomPosition);
            }
            else
            {
                // 최대 시도 횟수를 넘었는데도 유효한 위치를 못 찾은 경우 경고 로그 출력
                Debug.LogWarning("유효한 위치를 찾지 못했습니다. (i = " + i + ")");
                // 상황에 따라 여기서 루프를 종료하거나 다음 로직을 처리할 수 있습니다.
            }
        }

        return prefabsPos;
    }

}
