using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FusionExamples.Tanknarok;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class BotTankDamageVisual : MonoBehaviour
{
	[Header("Audio config")]
	[FormerlySerializedAs("_damageSnd")]
    [SerializeField] private AudioClipData damageSnd;
	
    [FormerlySerializedAs("_explosionSnd")]
    [SerializeField] private AudioClipData explosionSnd;
    
    [FormerlySerializedAs("_audioEmitter")]
    [SerializeField] private AudioEmitter audioEmitter;
    
    [Header("Damage visuals config")]
    [SerializeField] private float flashTime = 0.1f;
    [SerializeField] private int damagedParticlesToAdd = 1;
    
    [SerializeField] private DamagedStepData[] damagedSteps;
    [SerializeField] private Transform[] smokeDamagedParticlesPoints;
	[SerializeField] private Transform[] hitParticlesPoints;
	
	[Header("Tank body parts references")]
    public Transform visualParent;
    [SerializeField] public MeshRenderer[] tankBodyParts;
    
    private Transform[] _smokeDamagedParticlesPointsSelected;

    private int _previousRuntimeDamagedStep;
    private int _currentRuntimeDamagedStep;

    private float _maxHealth;
    private float _currentHealth;
    private float _previousHealth;

    private bool _initialized;

    public void Initialize(float maxTankHealth)
    {
	    _maxHealth = maxTankHealth;
	    _currentHealth = maxTankHealth;
	    _previousHealth = maxTankHealth;
	    
	    if (_initialized) // If this is an object reused by Photon Object Pool and not a new one, we want another kind of init
	    {
		    ReInit();
		    return;
	    }
	    
		_initialized = true;
		damagedSteps = damagedSteps.OrderBy(step => step.threshold).ToArray();
	    
	    _currentRuntimeDamagedStep = damagedSteps.Length - 1;
        _previousRuntimeDamagedStep = damagedSteps.Length - 1;
        	    
	    InitDamagedSmokeParticlesSelectedPoints();
	    InitDamagedSmokeParticlesInstances();
	    InitDamagedHitParticlesInstances();

	    CalculateCurrentDamagedStep();
	    UpdateVisualsByCurrentDamagedStep();
    }

    private void ReInit()
    {
	    _currentRuntimeDamagedStep = damagedSteps.Length - 1;
	    _previousRuntimeDamagedStep = damagedSteps.Length - 1;
	    
	    CalculateCurrentDamagedStep();
	    UpdateVisualsByCurrentDamagedStep();
    }

    private void InitDamagedHitParticlesInstances()
    {
	    
	    for (int i = 0; i < damagedSteps.Length; i++)
	    {
		    damagedSteps[i].HitParticles = new ParticleSystem[hitParticlesPoints.Length];
		    
		    for (int j = 0; j < damagedSteps[i].HitParticles.Length; j++)
		    {
			    if (damagedSteps[i].damagedStepAssets.HitParticles != null)
			    {
				    damagedSteps[i].HitParticles[j] = Instantiate(damagedSteps[i].damagedStepAssets.HitParticles, 
					    hitParticlesPoints[j].position,
					    hitParticlesPoints[j].rotation,
					    hitParticlesPoints[j].parent
				    ).GetComponent<ParticleSystem>();
				    
				    damagedSteps[i].HitParticles[j].gameObject.name = "HitParticle(DamageStepIndex: " + i + ")"; 
			    }
		    }
	    }
    }
    
    private void InitDamagedSmokeParticlesSelectedPoints()
    {
	    _smokeDamagedParticlesPointsSelected = new Transform[damagedParticlesToAdd];
	    for (int index = 0, particlesAdded = 0; index < smokeDamagedParticlesPoints.Length && particlesAdded < damagedParticlesToAdd; index++)
	    {
		    if (Random.Range(0, 2) != 0) // Random bool is not false
		    {
			    particlesAdded++;
			    _smokeDamagedParticlesPointsSelected[particlesAdded - 1] = smokeDamagedParticlesPoints[index];
		    }
		    else if ((smokeDamagedParticlesPoints.Length - index) <= damagedParticlesToAdd && particlesAdded < damagedParticlesToAdd)
		    {
			    particlesAdded++;
			    _smokeDamagedParticlesPointsSelected[particlesAdded - 1] = smokeDamagedParticlesPoints[index];
		    }
	    }   
    }
    
    private void InitDamagedSmokeParticlesInstances()
    {
	    for (int i = 0; i < damagedSteps.Length; i++)
	    {
		    damagedSteps[i].DamagedParticles = new ParticleSystem[damagedParticlesToAdd];
		    
		    for (int j = 0; j < _smokeDamagedParticlesPointsSelected.Length; j++)
		    {
			    if (damagedSteps[i].damagedStepAssets.SmokeDamageParticles != null)
			    {
				    damagedSteps[i].DamagedParticles[j] = Instantiate(damagedSteps[i].damagedStepAssets.SmokeDamageParticles, 
					    _smokeDamagedParticlesPointsSelected[j].position,
					    Quaternion.identity,
					    _smokeDamagedParticlesPointsSelected[j].parent
				    ).GetComponent<ParticleSystem>();
				    
				    damagedSteps[i].DamagedParticles[j].gameObject.name = "DamageParticle(DamageStepIndex: " + i + ")"; 
			    }
		    }
	    }
    }
    
    private void OnDisable()
    {
	    foreach (var tankBodyPart in tankBodyParts)
        {
	        tankBodyPart.material.SetFloat("_Transition", 0f);
        }
    }

    public void OnDeath()
    {
    	audioEmitter.PlayOneShot(explosionSnd);
        damagedSteps[_currentRuntimeDamagedStep].SetDamageStepVisualsVisible(false);
    }
    
    public void OnDamaged(float damage, bool isDead)
    {
	    if (!isDead)
	    {
		    StartCoroutine(Flash());
		    audioEmitter.PlayOneShot(damageSnd);
	    }
    }
    
    public void CheckHealth(int life)
    {
	    _currentHealth = life;
	    _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
	    
	    if (HealthHasChanged())
	    {
		    CalculateCurrentDamagedStep();
			    
		    if (DamagedStepHasChanged())
		    {
			    UpdateVisualsByCurrentDamagedStep();
		    }

		    StartCoroutine(Flash());
	    }
    }

    private void UpdateVisualsByCurrentDamagedStep()
    {
	    for (int i = 0; i < damagedSteps.Length; i++)
	    {
		    damagedSteps[i].SetDamageStepVisualsVisible(i == _currentRuntimeDamagedStep);
	    }
    }
    
    private bool DamagedStepHasChanged()
    {
	    if (_previousRuntimeDamagedStep != _currentRuntimeDamagedStep)
	    {
		    _previousRuntimeDamagedStep = _currentRuntimeDamagedStep;
		    return true;
	    }
	    return false;
    }
    
    //Check if the health has changed and return the answer - also sets previous health to current health for later use
    private bool HealthHasChanged()
    {
	    if (_previousHealth != _currentHealth)
	    {
		    _previousHealth = _currentHealth;
		    return true;
	    }
	    return false;
    }
    
    private void CalculateCurrentDamagedStep()
    {
	    int indexOfHighestThreshold = 0;
	    float highestThreshold = 0;
	    for (int i = 0; i < damagedSteps.Length; i++)
	    {
		    if (damagedSteps[i].threshold > highestThreshold && _currentHealth > damagedSteps[i].threshold)
		    {
			    indexOfHighestThreshold = i;
			    highestThreshold = damagedSteps[i].threshold;
		    }
	    }

	    _currentRuntimeDamagedStep = indexOfHighestThreshold;
    }


    IEnumerator Flash()
    {
	    foreach (var tankBodyPart in tankBodyParts)
        {
	        tankBodyPart.material.SetFloat("_Transition", 1f);
        }
        yield return new WaitForSeconds(flashTime);
        foreach (var tankBodyPart in tankBodyParts)
        {
	        tankBodyPart.material.SetFloat("_Transition", 0f);
        }
    }

    private void OnDrawGizmos()
    {
	    
	    if (smokeDamagedParticlesPoints != null)
	    {
		    Gizmos.color = Color.red;
		    for (int i = 0; i < smokeDamagedParticlesPoints.Length; i++)
		    {
			    Gizmos.DrawSphere(smokeDamagedParticlesPoints[i].position, 0.075f);
		    }
		    
		    Gizmos.color = Color.blue;
		    for (int i = 0; i < hitParticlesPoints.Length; i++)
		    {
			    Gizmos.DrawSphere(hitParticlesPoints[i].position, 0.075f);
		    }
	    } 
    }
    
}

#if UNITY_EDITOR
[CustomEditor(typeof(BotTankDamageVisual))]
class BotTankDamageVisualEditor : Editor
{
	public List<MeshRenderer> _tankBodyParts;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BotTankDamageVisual sourceScript = (BotTankDamageVisual)target;

        if (sourceScript == null) return;


        if (GUILayout.Button("Scan Tank's body parts "))
        {
	        _tankBodyParts = new List<MeshRenderer>();

	        
	        foreach (var meshRenderer in sourceScript.visualParent.GetComponentsInChildren<MeshRenderer>())
	        {
		        if (meshRenderer.gameObject.CompareTag("TankBodyPart"))
		        {
			        _tankBodyParts.Add(meshRenderer);
			        meshRenderer.gameObject.name = "BodyPart(Scaned)";
		        }
	        }

	        Debug.Log("Body parts scanned: " + _tankBodyParts.Count);
	        sourceScript.tankBodyParts = _tankBodyParts.ToArray();
        }
    }
}
#endif