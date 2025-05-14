using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTeleportManager : MonoBehaviour
{
    public static EnemyTeleportManager instance;

    [SerializeField] TileManager tileManager;
    [SerializeField] PlayerPositionData playerPositionData;
    [SerializeField] GameObject Beam;
    [SerializeField] float teleportCoolTime = 5f;
 
    CharacterBehaviour player;
    private Queue<MonsterBase> monsterQueue;
    private HashSet<MonsterBase> monsterSet;
    private float elapsedTime;

    private void Start()
    {
        instance = this;
        monsterQueue = new Queue<MonsterBase>();
        monsterSet = new HashSet<MonsterBase>();
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        elapsedTime = 0f;
        StartCoroutine(TeleportEnemy());
    }
 
    IEnumerator TeleportEnemy()
    {
        while (true) { 
            if (monsterQueue.Count > 0)
            {
                MonsterBase monster = monsterQueue.Peek();
                monsterQueue.Dequeue();
                monsterSet.Remove(monster);
                //       Vector3 newPosition = tileManager.GetTiles[playerPositionData.playerTilePosition.y, playerPositionData.playerTilePosition.x].transform.position+Vector3.up*4;
                RaycastHit hit;
                Physics.Raycast(player.transform.position, Vector3.down, out hit, 8f, LayerMask.GetMask("Wall"));
                Vector3 newPosition = hit.point;
                Destroy(Instantiate(Beam, newPosition, Quaternion.identity), 2f);
                yield return new WaitForSeconds(2f);
                if (monster != null && monster.NmAgent != null)
                {
                    monster.NmAgent.Warp(newPosition);
                    monster.NmAgent.FindClosestEdge(out NavMeshHit h);
                }
                Debug.Log("enemy teleported");
            }
            
            yield return new WaitForSeconds(teleportCoolTime -2f);
        }
        
    }
    public void GetEnemyToTeleport(MonsterBase monster)
    {
        if (monsterSet.Contains(monster))
        {
            return;
        }
        monsterQueue.Enqueue(monster);
        monsterSet.Add(monster);
    }
}
