using System.Collections;
using System.Collections.Generic;
using Fusion;
using FusionExamples.Tanknarok;
using UnityEngine;
using UnityEngine.Serialization;

public class BossBot : NetworkBehaviour, ICanTakeDamage, ITankUnit
{
    public UnityEngine.AI.NavMeshAgent navAgent;
    private BotTankDamageVisual _damageVisuals;
    
    [SerializeField] private Transform visualParent;
    [SerializeField] private Collider collider;
    private HitboxRoot _hitBoxRoot;

    [SerializeField] private RotateForeverNetworked primaryWeaponsRotationController;
    [SerializeField] private RotateForeverNetworked secundaryWeaponsRotationController;
    [SerializeField] private GameObject deathExplosionPrefab;
    
    [Header("Boundaries")]
    [SerializeField] private Vector3 bounds = Vector3.one;


    [SerializeField] private float tankBehaviourTimerDuration = 3f;

    [SerializeField] private Weapon[] primaryWeapons;
    [SerializeField] private Weapon[] secundaryWeapons;
    [SerializeField] private TankTeleportInEffect teleportIn;
    
    [SerializeField] private Transform hull;
    [SerializeField] private Transform turret;
    [SerializeField] private Color tankColor = Color.red;
    
    [Networked(OnChanged = nameof(OnStateChanged))]
    public UnitState BotState { get; set; }
    
    [Networked]
    public float TankBehaviourTimer { get; set; }
    
    [Networked]
    private TickTimer RespawnTimer { get; set; }
    
    private GameObject _deathExplosionInstance;
    
    public bool IsRespawningDone => BotState == UnitState.Spawning && RespawnTimer.Expired(Runner);
    public bool IsActivated => (gameObject.activeInHierarchy && (BotState == UnitState.Active));
    
    
    [Networked]
    public int Life { get; set; }
    public int maxHealth = 500;
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Vector3.zero, bounds * 2);
    }

    private void Awake()
    {
        _damageVisuals = GetComponent<BotTankDamageVisual>();
        collider = GetComponentInChildren<Collider>();
        _hitBoxRoot = GetComponent<HitboxRoot>();
    }

    private void SetupDeathExplosion()
    {
        _deathExplosionInstance = Instantiate(deathExplosionPrefab, this.transform);
        _deathExplosionInstance.SetActive(false);
        ColorChanger.ChangeColor(_deathExplosionInstance.transform, tankColor);
    }
    
    public override void Spawned()
    {
        _fireState.CurrentFireMode = FireMode.Flower;
        _damageVisuals.Initialize(maxHealth);
        teleportIn.Initialize(this);
        SetupDeathExplosion();
        
        if (!Object.HasStateAuthority)
            return;
        
        Life = maxHealth;
        BotState = UnitState.Spawning;
        
        // Start the respawn timer and trigger the teleport in effect
        RespawnTimer = TickTimer.CreateFromSeconds(Runner, 1);
        primaryWeaponsRotationController.ableToRotate = false;
    }

    public override void Render()
    {
        visualParent.gameObject.SetActive(BotState == UnitState.Active);
        collider.enabled = BotState != UnitState.Dead && BotState == UnitState.Active;
        _hitBoxRoot.HitboxRootActive = BotState == UnitState.Active;
        
        _damageVisuals.CheckHealth(Life);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;
        
        if (IsRespawningDone)
        {
            BotState = UnitState.Active;
        }

        if (IsActivated)
        {
            TankBehaviourTimer = Mathf.Min(TankBehaviourTimer + Runner.DeltaTime, tankBehaviourTimerDuration);
        
            if (TankBehaviourTimer >= tankBehaviourTimerDuration)
            {
                TankBehaviourTimer = 0;
                _fireState.SetNextFireMode();
                Fire();
            }
        }
        else if (BotState == UnitState.Dead)
        {
            Invoke(nameof(DespawnTank), 3f);
        }
    }

    public void DespawnTank()
    {
        if (Object != null)
        {
            Runner.Despawn(Object);
            BossSpawner.BossAmount--;
        }
    }
    public void Fire()
    {
        switch (_fireState.CurrentFireMode)
        {
            case FireMode.Normal: ExecuteFireMode1();
                break;
            case FireMode.Delayed: ExecuteFireMode2();
                break;
            case FireMode.Flower: ExecuteFireMode3();
                break;
        }
    }

    void ExecuteFireMode1()
    {
        primaryWeaponsRotationController.ableToRotate = false;
        secundaryWeaponsRotationController.ableToRotate = false;
        StartCoroutine(FireAllPrimaryWeapons(0f));
    }
    
    void ExecuteFireMode2()
    {
        StartCoroutine(FireAllPrimaryWeapons(0.5f));
        StartCoroutine(FireAllSecundaryWeapons(0.5f));
    }
    
    void ExecuteFireMode3()
    {
        secundaryWeaponsRotationController.ableToRotate = true;
        secundaryWeaponsRotationController._rotationsPerSecond = 0.3f * Mathf.PI;
        StartCoroutine(FireAllPrimaryWeapons(0.5f));
        StartCoroutine(FireAllSecundaryWeapons(0f));
    }

    IEnumerator FireAllPrimaryWeapons(float delayBetweenShoots)
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (var weapon in primaryWeapons)
            {
                if (!IsActivated)
                    break;
                
                weapon.Fire(Runner, Object.InputAuthority, navAgent.velocity);
                yield return new WaitForSeconds(delayBetweenShoots);
            }
        }
    }
    
    IEnumerator FireAllSecundaryWeapons(float delayBetweenShoots)
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (var weapon in secundaryWeapons)
            {
                if (!IsActivated)
                    break;
                
                weapon.Fire(Runner, Object.InputAuthority, navAgent.velocity);
                yield return new WaitForSeconds(0.075f);
            }
        }
    }
    
    public void ApplyDamage(Vector3 impulse, byte damage, PlayerRef source)
    {
        if (!IsActivated)
            return;

        _damageVisuals.OnDamaged(damage, false);

        if (damage >= Life)
        {
            Life = 0;
            BotState = UnitState.Dead;
        }
        else
        {
            Life -= damage;
        }
    }

    public Transform GetTankTurret()
    {
        return turret;
    }

    public Transform GetTankHull()
    {
        return hull;
    }

    public Color GetTankColor()
    {
        return tankColor;
    }
    
    public static void OnStateChanged(Changed<BossBot> changed)
    {
        if(changed.Behaviour)
            changed.Behaviour.OnStateChanged();
    }
    
    public void OnStateChanged()
    {
        switch (BotState)
        {
            case UnitState.Spawning:
                teleportIn.StartTeleport();
                break;
            case UnitState.Active:
                teleportIn.EndTeleport();
                primaryWeaponsRotationController.ableToRotate = true;
                break;
            case UnitState.Dead:
                Debug.Log("TANK DOWN");
                _deathExplosionInstance.transform.position = transform.position;
                _deathExplosionInstance.SetActive(false); // dirty fix to reactivate the death explosion if the particlesystem is still active
                _deathExplosionInstance.SetActive(true);

                visualParent.gameObject.SetActive(false);
                _damageVisuals.OnDeath();
                break;
        }
    }
    
    // Temporal helpers, like this whole script lol
    enum FireMode
    {
        Normal = 0,
        Delayed = 1,
        Flower = 2
    }

    private FireState _fireState;
    private struct FireState
    {
        public FireMode CurrentFireMode;

        public void SetNextFireMode()
        {
            switch (CurrentFireMode)
            {
                case FireMode.Normal:
                    CurrentFireMode = FireMode.Delayed;
                    break;
                case FireMode.Delayed:
                    CurrentFireMode = FireMode.Flower;
                    break;
                case FireMode.Flower:
                    CurrentFireMode = FireMode.Normal;
                    break;
            }
        }
    }
}
