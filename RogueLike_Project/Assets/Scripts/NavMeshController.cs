using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshController : MonoBehaviour
{
    NavMeshSurface navMeshSurface;
    // Start is called before the first frame update
    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();

        BuildNavMesh();
    }

    public void BuildNavMesh()
    {
        // NavMesh를 빌드합니다.
        navMeshSurface.BuildNavMesh();
    }
}
