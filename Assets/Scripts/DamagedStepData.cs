using System;
using UnityEngine;
public partial class BotTankDamageVisual
{
    [Serializable]
    private class DamagedStepData
    {
    	public ParticleSystem[] DamagedParticles { get; set; }
    	public ParticleSystem[] HitParticles { get; set; }
        
    	public GameObject[] damagedModules;
    	public TankDamageStepAssetsSO damagedStepAssets;
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
    		
    		foreach (var hitParticle in HitParticles)
    		{
    			if (hitParticle != null)
    			{
    				hitParticle.gameObject.SetActive(areVisible);
    				hitParticle.Play();
    			}
    		}

    		foreach (GameObject gmObj in damagedModules)
    		{
    			if (gmObj != null)
    			{
    				gmObj.SetActive(areVisible);
    			}
    		}
    	}
    }
    	
}
