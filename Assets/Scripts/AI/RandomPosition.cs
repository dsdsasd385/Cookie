using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;


public class RandomPosition : MonoBehaviourPunCallbacks
{
    [SerializeField] private NavMeshAgent navMesh;
    private NavMeshSurface _navMeshSurface;

    private Rigidbody _rigidbody;
    private RaycastHit _hit;
    private Bounds _navBounds;
    private float _timer = 5f;
    private void Awake()
    {
        _navMeshSurface = GameObject.FindObjectOfType<NavMeshSurface>();

    }
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _navBounds = _navMeshSurface.navMeshData.sourceBounds;
        if (_navMeshSurface == null) Debug.LogError("NavMeshSurface가 씬에 존재하지 않습니다!");

        //navMesh.avoidancePriority = Random.Range(30, 99);
        //navMesh.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        SetPosition();
    }


    void Update()
    {        
        _timer += Time.deltaTime;

    // 장애물 감지 및 목적지 도달 여부 확인
    if (IsObstacleAhead() && _timer >= 1f)
    {
        _timer = 0f; // 타이머 초기화
        SetPosition(); // 장애물 있으면 회피
    }
    else if (!navMesh.pathPending && navMesh.remainingDistance < 0.5f || navMesh.velocity.magnitude == 0)
    {
        SetPosition(); // 목적지 도달하거나 속도가 0일 때 새 위치 설정
        _rigidbody.velocity = Vector3.zero; // 불필요한 물리 계산 방지
    }

        DebugRays();
    }
    bool IsObstacleAhead()
    {
        float rayDistance = 3f;
        Vector3 origin = transform.position + Vector3.up * 0.5f; // 발사 위치 (캐릭터 중심보다 약간 위)

        // 정면
        if (Physics.Raycast(origin, transform.forward, rayDistance))
            return true;

        // 왼쪽 대각선
        if (Physics.Raycast(origin, Quaternion.Euler(0, -30, 0) * transform.forward, rayDistance))
            return true;

        // 오른쪽 대각선
        if (Physics.Raycast(origin, Quaternion.Euler(0, 30, 0) * transform.forward, rayDistance))
            return true;

        return false;
    }
    void DebugRays()
    {
        float rayDistance = 5f;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        Debug.DrawRay(origin, transform.forward * rayDistance, Color.red);
        Debug.DrawRay(origin, Quaternion.Euler(0, -30, 0) * transform.forward * rayDistance, Color.blue);
        Debug.DrawRay(origin, Quaternion.Euler(0, 30, 0) * transform.forward * rayDistance, Color.green);
    }
    public void SetPosition()
    {
        Vector3 pos = new Vector3(Random.Range(_navBounds.min.x, _navBounds.max.x),
                      0, Random.Range(_navBounds.min.z, _navBounds.max.z));

        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, 1.0f, NavMesh.AllAreas))
        {
            navMesh.destination = hit.position;
        }
        else
        {
            Debug.LogWarning("Invalid position: Unable to find valid spot on the NavMesh.");
        }
    }
}
