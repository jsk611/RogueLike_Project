using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTeleportManager : MonoBehaviour
{
    public static EnemyTeleportManager instance;

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
    }
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > teleportCoolTime && monsterStack.Peek() != null) {
            elapsedTime = 0f;
            MonsterBase monster = monsterStack.Pop();
            Instantiate(Beam);

            Vector3 newPosition = new Vector3(playerPositionData.playerTilePosition.x, player.transform.position.y, playerPositionData.playerTilePosition.y);
            monster.transform.position = newPosition;
        }
    }

    public void GetEnemyToTeleport(MonsterBase monster)
    {
        monsterStack.Push(monster);
    }
}
