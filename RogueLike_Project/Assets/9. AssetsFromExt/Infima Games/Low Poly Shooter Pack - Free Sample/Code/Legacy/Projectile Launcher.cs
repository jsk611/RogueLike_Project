using System;
using UnityEngine;
using System.Collections;
using InfimaGames.LowPolyShooterPack;
using Random = UnityEngine.Random;
using static UnityEngine.ParticleSystem;
using UnityEngine.Rendering;

public class ProjectileLauncher : MonoBehaviour {

	[Tooltip("Damage of the Bullet")]
	[SerializeField]
	public float bulletDamage;

	[Tooltip("Radious of Explosion")]
	[SerializeField]
	public float explosionRange;

	[Tooltip("Explosion Particle Prefab")]
	[SerializeField]
	public GameObject explosionPrefab;


	AudioSource explosionSource;
	PlayerStatus shooterStatus;

	[Range(0, 100)]
	[Tooltip("After how long time should the bullet prefab be destroyed?")]
	public float destroyAfter;
	[Tooltip("If enabled the bullet destroys on impact")]
	public bool destroyOnImpact = false;
	[Tooltip("Minimum time after impact that the bullet is destroyed")]
	public float minDestroyTime;
	[Tooltip("Maximum time after impact that the bullet is destroyed")]
	public float maxDestroyTime;

	[Header("Impact Effect Prefabs")]
	public Transform [] bloodImpactPrefabs;
	public Transform [] metalImpactPrefabs;
	public Transform [] dirtImpactPrefabs;
	public Transform []	concreteImpactPrefabs;

	private void Start ()
	{

		//Grab the game mode service, we need it to access the player character!
		var gameModeService = ServiceLocator.Current.Get<IGameModeService>();
		//Ignore the main player character's collision. A little hacky, but it should work.
		Physics.IgnoreCollision(gameModeService.GetPlayerCharacter().GetComponent<Collider>(), GetComponent<Collider>());
		shooterStatus = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter().GetComponent<PlayerStatus>();
		//Start destroy timer
		StartCoroutine (DestroyAfter ());
	}

	//If the bullet collides with anything
	private void OnCollisionEnter (Collision collision)
	{

		//Ignore collisions with other projectiles.
		if (collision.gameObject.GetComponent<Projectile>() != null)
			return;

		Instantiate(explosionPrefab,transform.position,Quaternion.Euler(Vector3.left*90));

		Collider[] hits = Physics.OverlapSphere(transform.position, explosionRange,LayerMask.GetMask("Creature"));
		foreach (Collider hit in hits)
		{
			Debug.Log("grenade hit " + hit.name);
            if (hit.gameObject.GetComponent<MonsterBase>() != null)
            {
                hit.gameObject.GetComponent<MonsterBase>().TakeDamage((bulletDamage * shooterStatus.GetAttackDamage() / 100) * shooterStatus.CalculateCriticalHit());
            }
            else if (hit.gameObject.GetComponent<BossBase>() != null)
            {
                Debug.Log("Boss hit");
                hit.gameObject.GetComponent<BossBase>()?.TakeDamage((bulletDamage * shooterStatus.GetAttackDamage() / 100) * shooterStatus.CalculateCriticalHit());
            }
            else if (hit.gameObject.GetComponent<Dummy>() != null)
            {
                hit.gameObject.GetComponent<Dummy>().TakeDamage((bulletDamage * shooterStatus.GetAttackDamage() / 100) * shooterStatus.CalculateCriticalHit());
            }
        }
		Destroy(gameObject);
		

		if (!destroyOnImpact) 
		{
			StartCoroutine (DestroyTimer ());
		}
		//Otherwise, destroy bullet on impact
		else 
		{
			Destroy (gameObject);
		}
	}

    private IEnumerator DestroyTimer () 
	{
		//Wait random time based on min and max values
		yield return new WaitForSeconds
			(Random.Range(minDestroyTime, maxDestroyTime));
		//Destroy bullet object
		Destroy(gameObject);
	}

	private IEnumerator DestroyAfter () 
	{
		//Wait for set amount of time
		yield return new WaitForSeconds (destroyAfter);
		//Destroy bullet object
		Destroy (gameObject);
	}

}