﻿using System;
using UnityEngine;
using System.Collections;
using InfimaGames.LowPolyShooterPack;
using Random = UnityEngine.Random;
using UnityEngine.ProBuilder.MeshOperations;

public class Projectile : MonoBehaviour {

	[Tooltip("Damage of the Bullet")]
	[SerializeField]
	public float bulletDamage;

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
		
		if (collision.gameObject.layer == LayerMask.NameToLayer("Head"))
        {
			//Debug.Log("Hithhhhhhhh");
			//collision.gameObject.GetComponent<MonsterBase>().TakeDamage((bulletDamage * shooterStatus.GetAttackDamage() / 100) * 2f);
			//EventManager.Instance.TriggerMonsterCriticalDamagedEvent();
			Destroy(gameObject);
		}
		else if (collision.gameObject.layer == LayerMask.NameToLayer("Creature"))
		{
			
			if (collision.gameObject.GetComponent<MonsterBase>() != null)
			{
				collision.gameObject.GetComponent<MonsterBase>().TakeDamage((bulletDamage * shooterStatus.GetAttackDamage() / 100) * shooterStatus.CalculateCriticalHit());
			}
			else if (collision.gameObject.GetComponent<BossBase>() != null)
			{
				collision.gameObject.GetComponent<BossBase>().TakeDamage((bulletDamage * shooterStatus.GetAttackDamage() / 100) * shooterStatus.CalculateCriticalHit());
			}
			else if (collision.gameObject.GetComponent<Dummy>() != null)
			{
				collision.gameObject.GetComponent<Dummy>().TakeDamage((bulletDamage * shooterStatus.GetAttackDamage() / 100) * shooterStatus.CalculateCriticalHit());
			}
			Destroy(gameObject);
		}
		// //Ignore collision if bullet collides with "Player" tag
		// if (collision.gameObject.CompareTag("Player")) 
		// {
		// 	//Physics.IgnoreCollision (collision.collider);
		// 	Debug.LogWarning("Collides with player");
		// 	//Physics.IgnoreCollision(GetComponent<Collider>(), GetComponent<Collider>());
		//
		// 	//Ignore player character collision, otherwise this moves it, which is quite odd, and other weird stuff happens!
		// 	Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
		//
		// 	//Return, otherwise we will destroy with this hit, which we don't want!
		// 	return;
		// }
		//
		//If destroy on impact is false, start 
		//coroutine with random destroy timer
		if (!destroyOnImpact)
		{
			StartCoroutine(DestroyTimer());
		}
		else if (collision.gameObject.layer == LayerMask.NameToLayer("Projectile")) return;
		//Otherwise, destroy bullet on impact
		else
		{
			Destroy(gameObject);
		}

		//If bullet collides with "Target" tag
		if (collision.transform.tag == "Target") 
		{
			//Toggle "isHit" on target object
			collision.transform.gameObject.GetComponent
				<TargetScript>().isHit = true;
			//Destroy bullet object
			Destroy(gameObject);
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