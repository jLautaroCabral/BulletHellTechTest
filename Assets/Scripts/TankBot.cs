using Fusion;
using FusionExamples.Tanknarok;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TankBot : NetworkBehaviour, ICanTakeDamage, ITankUnit
{
    public NavMeshAgent navAgent;
    private TankDamageVisualCustom _damageVisuals;
    [SerializeField] private Transform _visualParent;
    
    [SerializeField] private Collider _collider;
    private HitboxRoot _hitBoxRoot;

    [SerializeField] private RotateForeverNetworked _turretRotationController;
    [SerializeField] private GameObject _deathExplosionPrefab;
    
    [Header("Boundaries")]
    [SerializeField] private Vector3 bounds = Vector3.one;


    [FormerlySerializedAs("_respawnDuration")] [SerializeField]
    private float _tankBehaviourTimerDuration = 3f;

    [SerializeField] private Weapon[] _weapons;
    [SerializeField] private TankTeleportInEffect _teleportIn;
    
    [SerializeField] private Transform _hull;
    [SerializeField] private Transform _turret;
    [SerializeField] private Color _tankColor = Color.red;
    
    [Networked(OnChanged = nameof(OnStateChanged))]
    public UnitState BotState { get; set; }
    [Networked]
    public float tankBehaviourTimer { get; set; }
    [Networked]
    private TickTimer respawnTimer { get; set; }
    
    private GameObject _deathExplosionInstance;
    
    public bool isRespawningDone => BotState == UnitState.Spawning && respawnTimer.Expired(Runner);
    public bool isActivated => (gameObject.activeInHierarchy && (BotState == UnitState.Active));
    
    
    [Networked]
    public byte life { get; set; }
    public const byte MAX_HEALTH = 100;
    private void OnDrawGizmos()
    {
        //Vector3 boundsOffset = this.boundsOffset + (new Vector3(0, 0, boundsMovementMax));
        Gizmos.DrawWireCube(Vector3.zero, bounds * 2);
    }

    private void Awake()
    {
        _damageVisuals = GetComponent<TankDamageVisualCustom>();
        _collider = GetComponentInChildren<Collider>();
        _hitBoxRoot = GetComponent<HitboxRoot>();
    }

    private void SetupDeathExplosion()
    {
    	_deathExplosionInstance = Instantiate(_deathExplosionPrefab, this.transform);
    	_deathExplosionInstance.SetActive(false);
    	ColorChanger.ChangeColor(_deathExplosionInstance.transform, _tankColor);
    }
    
    public override void Spawned()
    {
        _teleportIn.Initialize(this);
        SetupDeathExplosion();
        
        if (!Object.HasStateAuthority)
            return;
        
        life = MAX_HEALTH;
        BotState = UnitState.Spawning;
        
        // Start the respawn timer and trigger the teleport in effect
        respawnTimer = TickTimer.CreateFromSeconds(Runner, 1);
        _turretRotationController.ableToRotate = false;
    }

    public void UpdateDestination()
    {
        float x = Random.Range(-bounds.x, bounds.x);
        float z = Random.Range(-bounds.z, bounds.z);
        Vector3 destination = new Vector3(x, transform.position.y, z);
        Debug.Log("Destiny: " + destination);
        navAgent.SetDestination(destination);
    }
    
    public override void Render()
    {
        _visualParent.gameObject.SetActive(BotState == UnitState.Active);
        _collider.enabled = BotState != UnitState.Dead && BotState == UnitState.Active;
        _hitBoxRoot.HitboxRootActive = BotState == UnitState.Active;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;
        
        if (isRespawningDone)
        {
            BotState = UnitState.Active;
        }

        if (isActivated)
        {
            tankBehaviourTimer = Mathf.Min(tankBehaviourTimer + Runner.DeltaTime, _tankBehaviourTimerDuration);
        
            if (tankBehaviourTimer >= _tankBehaviourTimerDuration)
            {
                tankBehaviourTimer = 0;
                Movement();
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
            BotSpawner.BotAmount--;
        }
    }

    public void Movement()
    {
        UpdateDestination();
    }

    public void Fire()
    {
        foreach (var weapon in _weapons)
        {
            weapon.Fire(Runner, Object.InputAuthority, navAgent.velocity);                
        }
    }

    public void ApplyDamage(Vector3 impulse, byte damage, PlayerRef source)
    {
        if (!isActivated)
            return;
        
        Debug.Log("DAMAGED");
        _damageVisuals.OnDamaged(damage, false);

        if (damage >= life)
        {
            life = 0;
            BotState = UnitState.Dead;
        }
        else
        {
            life -= damage;
        }
    }

    public Transform GetTankTurret()
    {
        return _turret;
    }

    public Transform GetTankHull()
    {
        return _hull;
    }

    public Color GetTankColor()
    {
        return _tankColor;
    }
    
    public static void OnStateChanged(Changed<TankBot> changed)
    {
        if(changed.Behaviour)
            changed.Behaviour.OnStateChanged();
    }
    
    public void OnStateChanged()
    {
        switch (BotState)
        {
            case UnitState.Spawning:
                _teleportIn.StartTeleport();
                break;
            case UnitState.Active:
                //_damageVisuals.CleanUpDebris();
                UpdateDestination();
                _teleportIn.EndTeleport();
                _turretRotationController.ableToRotate = true;
                break;
            case UnitState.Dead:
                Debug.Log("TANK DOWN");
                _deathExplosionInstance.transform.position = transform.position;
                _deathExplosionInstance.SetActive(false); // dirty fix to reactivate the death explosion if the particlesystem is still active
                _deathExplosionInstance.SetActive(true);

                _visualParent.gameObject.SetActive(false);
                _damageVisuals.OnDeath();
                break;
        }
    }
}
