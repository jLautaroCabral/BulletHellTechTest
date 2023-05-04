using System;
using System.Collections;
using System.Collections.Generic;
using FusionExamples.Tanknarok;
using UnityEngine;

public class TankDamageVisualCustom : MonoBehaviour
{
	[Header("Health")]
    [SerializeField] private int _gameMaxHealth = 100;
    private int _previousHealth;

    [Header("Helpers")]
    //[SerializeField] private Transform _damageParticleParent;
    
    [SerializeField] private Material _damageMaterial;
    [SerializeField] private float _flashTime = 0.1f;

    //Remember certain previous values
    [Header("Tank prefab references")]
    [SerializeField]
    private MeshRenderer[] _tankBodyParts = null;
    
    private Material[] _previousTankBodyPartsMaterials = null;

    [Header("Audio")]
    [SerializeField] private AudioClipData _damageSnd;
    [SerializeField] private AudioClipData _explosionSnd;
    [SerializeField] private AudioEmitter _audioEmitter;

    private void Awake()
    {
	    _previousTankBodyPartsMaterials = new Material[_tankBodyParts.Length];
    }

    private void OnDisable()
    {
    	if (_damageMaterial != null)
    	{
    		_damageMaterial.SetFloat("_Transition", 0f);
    	}
    }

    public void OnDeath()
    {
    	_audioEmitter.PlayOneShot(_explosionSnd);
    	CheckHealth(0);
    }
    
    public void OnDamaged(float damage, bool isDead)
    {
    	Debug.Log("Damaged!");
    	if (!isDead && _damageMaterial != null)
    	{
    		StartCoroutine(Flash());
    		_audioEmitter.PlayOneShot(_damageSnd);
    	}
    }
    
    private void OnDestroy()
    {
    	//if (_damageParticleParent != null)
    		//Destroy(_damageParticleParent.gameObject);
    }

    public void CheckHealth(int life)
    {
    	//Check if health has changed
    	if (HealthHasChanged(life))
    	{
    	}
    }

    //Check if the health has changed and return the answer - also sets previous health to current health for later use
    bool HealthHasChanged(int life)
    {
    	if (_previousHealth != life)
    	{
    		_previousHealth = life;
    		return true;
    	}

    	return false;
    }
    
    //Calculate an index based on the value
    int CalculateIndex(int max, float value)
    {
    	return Mathf.FloorToInt(max * value);
    }
    
    
    IEnumerator Flash()
    {
    	//_damageMaterial.SetFloat("_Transition", 1f);
        //_previousTankBodyPartsMaterials = _tankBodyParts[0].GetMaterials();
        for (int i = 0; i < _tankBodyParts.Length; i++)
        {
	        _tankBodyParts[i].material.SetFloat("_Transition", 1f); // = _damageMaterial;
        }
    	yield return new WaitForSeconds(_flashTime);
        
        for (int i = 0; i < _tankBodyParts.Length; i++)
        {
            _tankBodyParts[i].material.SetFloat("_Transition", 0f);// = _previousTankBodyPartsMaterials[i];
        }
    	//_damageMaterial.SetFloat("_Transition", 0f);
    }
}
