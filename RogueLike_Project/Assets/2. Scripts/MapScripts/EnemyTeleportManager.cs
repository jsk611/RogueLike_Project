using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTeleportManager : MonoBehaviour
{
    public static EnemyTeleportManager instance;

    [SerializeField] TileManager tileManager;
    [SerializeField] PlayerPositionData playerPositionData;
    [SerializeField] GameObject Beam;
    [SerializeField] float teleportCoolTime = 5f;
 
    CharacterBehaviour player;
    private Stack<MonsterBase> monsterStack;
    private float elapsedTime;

    private void Start()
    {
        instance = this;
        monsterStack = new Stack<MonsterBase>();
        player = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        elapsedTime = 0f;
        StartCoroutine(TeleportEnemy());
    }
 
    IEnumerator TeleportEnemy()
    {
        while (true)
        {
            Debug.Log("enemy teleporting");
            if (monsterStack.Count > 0)
            {
                MonsterBase monster = monsterStack.Pop();
         //       Vector3 newPosition = tileManager.GetTiles[playerPositionData.playerTilePosition.y, playerPositionData.playerTilePosition.x].transform.position+Vector3.up*4;
                Vector3 newPosition = Physics.Raycast(player.transform.position,Vector3.down,8f,LayerMask.GetMask("Floor"))
                Destroy(Instantiate(Beam, newPosition, Quaternion.identity), 2f);
                yield return new WaitForSeconds(2f);
                monster.transform.position = newPosition;
                Debug.Log("enemy teleported");
            }
            
            yield return new WaitForSeconds(teleportCoolTime -2f);
        }
        
    }
    public void GetEnemyToTeleport(MonsterBase monster)
    {
        monsterStack.Push(monster);
    }
}
