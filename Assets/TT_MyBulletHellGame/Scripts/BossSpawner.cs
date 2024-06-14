using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class BossSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject[] _bossesToSpawn;
	[SerializeField] private Transform spawnPoint;
	public NetworkBool ableToSpawnBot { get; set; }
	
	public float respawnTimerFloat { get; set; }

	[SerializeField]
	private float _respawnDurationMin = 2f;
	[SerializeField]
	private float _respawnDurationMax = 5f;

	private float _selectedRespawnTimerDuration;

	private const byte MAX_ALIVE_BOSS_AMOUNT = 1;

	public static byte BossAmount = 0; 

	public override void Spawned()
	{
		if (!Object.HasStateAuthority)
			return;
		
		ableToSpawnBot = false;
		_selectedRespawnTimerDuration = Random.Range(_respawnDurationMin, _respawnDurationMax);
	}

	public override void FixedUpdateNetwork()
	{
		if (!Object.HasStateAuthority)
			return;

		// Update the respawn timer
		respawnTimerFloat = Mathf.Min(respawnTimerFloat + Runner.DeltaTime, _selectedRespawnTimerDuration);

		// Spawn a new powerup whenever the respawn duration has been reached
		if (respawnTimerFloat >= _selectedRespawnTimerDuration)
		{
			_selectedRespawnTimerDuration = Random.Range(_respawnDurationMin, _respawnDurationMax);
			respawnTimerFloat = 0;
			ableToSpawnBot = true;
		}
	}

	// Create a simple scale in effect when spawning
	public override void Render()
	{
		if (!Object.HasStateAuthority)
			return;

		if (ableToSpawnBot)
		{
			if (BossAmount < MAX_ALIVE_BOSS_AMOUNT)
			{
				SpawnBot(Runner, Object.InputAuthority, spawnPoint);
			}
			ableToSpawnBot = false;
			//_renderer.transform.localScale = Vector3.zero;
			// Store the active powerup index for returning
			// int lastIndex = activePowerupIndex;
			// SetRechargeAmount(respawnProgress); Spawn
			// GetComponent<AudioEmitter>().PlayOneShot(_powerupElements[lastIndex].pickupSnd);
			// SetNextPowerup();
			// return lastIndex != -1 ? _powerupElements[lastIndex] : null;
		}
	}

	private void SpawnBot(NetworkRunner runner, PlayerRef owner, Transform locationTransform)
	{
		// Create a key that is unique to this shot on this client so that when we receive the actual NetworkObject
		// Fusion can match it against the predicted local bullet.
		var key = new NetworkObjectPredictionKey {Byte0 = (byte) owner.RawEncoded, Byte1 = (byte) runner.Simulation.Tick};
		BossAmount++;
		runner.Spawn(_bossesToSpawn[Random.Range(0, _bossesToSpawn.Length)], locationTransform.position, locationTransform.rotation, owner, (runner, obj) =>
		{
			if (Object.HasStateAuthority)
				return;
			
			//obj.GetComponent<Projectile>().InitNetworkState(ownerVelocity);
			Debug.Log("Bot Spawn Attempt");
			
		}, key );
	}
}
