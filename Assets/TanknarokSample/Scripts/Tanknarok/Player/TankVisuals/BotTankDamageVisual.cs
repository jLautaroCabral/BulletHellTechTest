using System.Collections;
using FusionExamples.Tanknarok;
using UnityEngine;

public class BotTankDamageVisual : MonoBehaviour
{
	[Header("Helpers")]
    [SerializeField] private float _flashTime = 0.1f;

    [Header("Tank prefab references")]
    [SerializeField]
    private MeshRenderer[] _tankBodyParts = null;

    [Header("Audio")]
    [SerializeField] private AudioClipData _damageSnd;
    [SerializeField] private AudioClipData _explosionSnd;
    [SerializeField] private AudioEmitter _audioEmitter;

    private void OnDisable()
    {
    	for (int i = 0; i < _tankBodyParts.Length; i++)
        {
            _tankBodyParts[i].material.SetFloat("_Transition", 0f);
        }
    }

    public void OnDeath()
    {
    	_audioEmitter.PlayOneShot(_explosionSnd);
    }
    
    public void OnDamaged(float damage, bool isDead)
    {
	    if (!isDead)
    	{
    		StartCoroutine(Flash());
    		_audioEmitter.PlayOneShot(_damageSnd);
    	}
    }

    IEnumerator Flash()
    {
	    for (int i = 0; i < _tankBodyParts.Length; i++)
        {
	        _tankBodyParts[i].material.SetFloat("_Transition", 1f); // = _damageMaterial;
        }
    	yield return new WaitForSeconds(_flashTime);
        
        for (int i = 0; i < _tankBodyParts.Length; i++)
        {
            _tankBodyParts[i].material.SetFloat("_Transition", 0f);// = _previousTankBodyPartsMaterials[i];
        }
    }
}
