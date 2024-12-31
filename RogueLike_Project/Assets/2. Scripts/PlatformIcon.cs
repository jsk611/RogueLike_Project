using InfimaGames.LowPolyShooterPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformIcon : MonoBehaviour
{
    // Start is called before the first frame update
    Transform characterTransform;
    void Start()
    {
        characterTransform = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 Direction = characterTransform.position-transform.position;
        Direction.y = 0;
        transform.rotation = Quaternion.LookRotation(Direction);
    }
}
