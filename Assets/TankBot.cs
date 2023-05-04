using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using FusionExamples.Tanknarok;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class TankBot : NetworkBehaviour, ICanTakeDamage
{
    public NavMeshAgent navAgent;
    private TankDamageVisualCustom _damageVisuals;
    
    [Header("Boundaries")]
    [SerializeField] private Vector3 bounds = Vector3.one;

    [Networked]
    public float respawnTimerFloat { get; set; }
    
    [SerializeField]
    private float _respawnDuration = 3f;

    [SerializeField]
    private Weapon[] _weapons;


    private void OnDrawGizmos()
    {
        //Vector3 boundsOffset = this.boundsOffset + (new Vector3(0, 0, boundsMovementMax));
        Gizmos.DrawWireCube(Vector3.zero, bounds * 2);
    }

    private void Awake()
    {
        _damageVisuals = GetComponent<TankDamageVisualCustom>();
    }

    public override void Spawned()
    {
        navAgent.SetDestination(Vector3.zero);
    }

    public void UpdateDestination()
    {
        float x = Random.Range(-bounds.x, bounds.x);
        float z = Random.Range(-bounds.z, bounds.z);
        Vector3 destination = new Vector3(x, transform.position.y, z);
        Debug.Log("Destiny: " + destination);
        navAgent.SetDestination(destination);
    }
    
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        // Update the respawn timer
        respawnTimerFloat = Mathf.Min(respawnTimerFloat + Runner.DeltaTime, _respawnDuration);
        Debug.Log(respawnTimerFloat);
        // Spawn a new powerup whenever the respawn duration has been reached
        if (respawnTimerFloat >= _respawnDuration)
        {
            respawnTimerFloat = 0;
            UpdateDestination();
            
            foreach (var weapon in _weapons)
            {
                weapon.Fire(Runner, Object.InputAuthority, navAgent.velocity);                
            }
        }
    }

    public void ApplyDamage(Vector3 impulse, byte damage, PlayerRef source)
    {
        Debug.Log("Applying damage");
        _damageVisuals.OnDamaged(damage, false);
    }
}
