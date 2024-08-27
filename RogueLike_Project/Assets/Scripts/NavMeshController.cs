using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshController : MonoBehaviour
{   
    public NavMeshSurface navMeshSurface;
    [SerializeField] NavMeshAgent[] agents;

    void Start()
    {
        // 처음에 NavMesh를 생성
        UpdateNavMesh();
    }

    public void UpdateNavMesh()
    {
        // 이전 NavMesh를 지우고 새롭게 빌드
        navMeshSurface.BuildNavMesh();

    }

    // Wave가 시작되거나 맵이 변화할 때 이 메서드를 호출
   
}
