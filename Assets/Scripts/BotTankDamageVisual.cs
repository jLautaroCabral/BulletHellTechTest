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

public class BotTankDamageVisual : MonoBehaviour
{
	[Serializable]
	private class DamagedStepData
	{
		public ParticleSystem[] DamagedParticles { get; set; }
		public ParticleSystem[] HitParticles { get; set; }
	    
		public GameObject[] DamagedModules;
		public TankDamageStepAssetsSO DamagedStepAssets;
		public float threshold;

		public void SetDamageStepVisualsVisible(bool areVisible)
		{
			foreach (var particleSystem in DamagedParticles)
			{
				if (particleSystem != null)
				{
					particleSystem.gameObject.SetActive(areVisible);
				}
			}

			foreach (GameObject gmObj in DamagedModules)
			{
				if (gmObj != null)
				{
					gmObj.SetActive(areVisible);
				}
			}
		}
	}
	
	[Header("Helpers")]
    [SerializeField] private float _flashTime = 0.1f;

	[Header("Audio")]
    [SerializeField] private AudioClipData _damageSnd;
    [SerializeField] private AudioClipData _explosionSnd;
    [SerializeField] private AudioEmitter _audioEmitter;
    
    [SerializeField] private DamagedStepData[] _damagedSteps;
    
    [SerializeField] public MeshRenderer[] tankBodyParts;
    
    [SerializeField] private int damagedParticlesToAdd = 2;
    	
	[SerializeField] private Transform[] damagedParticlesPoints;
    
    public Transform hull;
    public Transform turret;
    
    private Transform[] _damagedParticlesPointsSelected;
    
    private int _previousRuntimeDamagedStep;
    private int _currentRuntimeDamagedStep;

    private float _maxHealth;
    private float _currentHealth;
    private float _previousHealth;

    private bool _initialized;

    public void Initialize(float maxTankHealth)
    {
	    if (!_initialized)
	    {
		    _initialized = true;
	    }
	    else
	    {
		    return;
	    }
		    
	    _maxHealth = maxTankHealth;
	    _currentHealth = maxTankHealth;
	    _previousHealth = maxTankHealth;
	    
	    _damagedSteps = _damagedSteps.OrderBy(step => step.threshold).ToArray();
	    
	    _currentRuntimeDamagedStep = _damagedSteps.Length - 1;
        _previousRuntimeDamagedStep = _damagedSteps.Length - 1;
        	    
	    InitParticlesSelectedPoints();
	    InitDamagedParticlesInstances();

	    UpdateVisualsByCurrentDamagedStep();
    }
    
    private void InitParticlesSelectedPoints()
    {
	    _damagedParticlesPointsSelected = new Transform[damagedParticlesToAdd];
	    for (int index = 0, particlesAdded = 0; index < damagedParticlesPoints.Length && particlesAdded < damagedParticlesToAdd; index++)
	    {
		    if (Random.Range(0, 2) != 0) // Random bool is not false
		    {
			    particlesAdded++;
			    _damagedParticlesPointsSelected[particlesAdded - 1] = damagedParticlesPoints[index];
		    }
		    else if ((damagedParticlesPoints.Length - index) <= damagedParticlesToAdd && particlesAdded < damagedParticlesToAdd)
		    {
			    particlesAdded++;
			    _damagedParticlesPointsSelected[particlesAdded - 1] = damagedParticlesPoints[index];
		    }
	    }   
    }
    
    private void InitDamagedParticlesInstances()
    {
	    for (int i = 0; i < _damagedSteps.Length; i++)
	    {
		    _damagedSteps[i].DamagedParticles = new ParticleSystem[damagedParticlesToAdd];
		    
		    for (int j = 0; j < _damagedParticlesPointsSelected.Length; j++)
		    {
			    if (_damagedSteps[i].DamagedStepAssets.SmokeDamageParticles != null)
			    {
				    _damagedSteps[i].DamagedParticles[j] = Instantiate(_damagedSteps[i].DamagedStepAssets.SmokeDamageParticles, 
					    _damagedParticlesPointsSelected[j].position,
					    Quaternion.identity,
					    _damagedParticlesPointsSelected[j].parent
				    ).GetComponent<ParticleSystem>();
				    
				    _damagedSteps[i].DamagedParticles[j].gameObject.name = "DamageParticle(DamageStepIndex: " + i + ")"; 
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
    	_audioEmitter.PlayOneShot(_explosionSnd);
    }
    
    public void OnDamaged(float damage, bool isDead)
    {
	    _currentHealth -= damage;
	    _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
	    
	    if (!isDead)
	    {
		    if (HealthHasChanged())
		    {
			    CalculateCurrentDamagedStep();
			    
			    if (DamagedStepHasChanged())
			    {
				    UpdateVisualsByCurrentDamagedStep();
			    }

			    StartCoroutine(Flash());
			    _audioEmitter.PlayOneShot(_damageSnd);
		    }
	    }
    }

    private void UpdateVisualsByCurrentDamagedStep()
    {
	    for (int i = 0; i < _damagedSteps.Length; i++)
	    {
		    _damagedSteps[i].SetDamageStepVisualsVisible(i == _currentRuntimeDamagedStep);
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
	    for (int i = 0; i < _damagedSteps.Length; i++)
	    {
		    if (_damagedSteps[i].threshold > highestThreshold && _currentHealth > _damagedSteps[i].threshold)
		    {
			    indexOfHighestThreshold = i;
			    highestThreshold = _damagedSteps[i].threshold;
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
        yield return new WaitForSeconds(_flashTime);
        foreach (var tankBodyPart in tankBodyParts)
        {
	        tankBodyPart.material.SetFloat("_Transition", 0f);
        }
    }

    private void OnDrawGizmos()
    {
	    Gizmos.color = Color.red;
	    if (damagedParticlesPoints != null)
	    {
		    for (int i = 0; i < damagedParticlesPoints.Length; i++)
		    {
			    Gizmos.DrawSphere(damagedParticlesPoints[i].position, 0.1f);
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

	        foreach (var meshRenderer in sourceScript.hull.GetComponentsInChildren<MeshRenderer>())
	        {
		        if (meshRenderer.gameObject.CompareTag("TankBodyPart"))
		        {
			        _tankBodyParts.Add(meshRenderer);
			        meshRenderer.gameObject.name = "HullBodyPart";
		        }
	        }
	        
	        foreach (var meshRenderer in sourceScript.turret.GetComponentsInChildren<MeshRenderer>())
	        {
		        if (meshRenderer.gameObject.CompareTag("TankBodyPart"))
		        {
			        _tankBodyParts.Add(meshRenderer);
			        meshRenderer.gameObject.name = "TurretBodyPart";
		        }
	        }

	        Debug.Log("Body parts scanned: " + _tankBodyParts.Count);
	        sourceScript.tankBodyParts = _tankBodyParts.ToArray();
        }
    }
}
#endif
