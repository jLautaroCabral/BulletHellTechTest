using System.Collections;
using System.Collections.Generic;
using FusionExamples.Tanknarok;
using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BotTankDamageVisual : MonoBehaviour
{
	[Header("Helpers")]
    [SerializeField] private float _flashTime = 0.1f;

	[SerializeField]
	public MeshRenderer[] _tankBodyParts2;

	[Header("Audio")]
    [SerializeField] private AudioClipData _damageSnd;
    [SerializeField] private AudioClipData _explosionSnd;
    [SerializeField] private AudioEmitter _audioEmitter;
    
    public Transform hull;
    public Transform turret;
    private void OnDisable()
    {
	    /*
    	for (int i = 0; i < _tankBodyParts.Length; i++)
        {
	        _tankBodyParts[i].material.SetFloat("_Transition", 0f);
        }*/

        foreach (var tankBodyPart in _tankBodyParts2)
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
	    if (!isDead)
    	{
    		StartCoroutine(Flash());
    		_audioEmitter.PlayOneShot(_damageSnd);
    	}
    }

    IEnumerator Flash()
    {
	    /*
	    for (int i = 0; i < _tankBodyParts.Length; i++)
        {
	        _tankBodyParts[i].material.SetFloat("_Transition", 1f); // = _damageMaterial;
        }
    	yield return new WaitForSeconds(_flashTime);
        for (int i = 0; i < _tankBodyParts.Length; i++)
        {
            _tankBodyParts[i].material.SetFloat("_Transition", 0f);// = _previousTankBodyPartsMaterials[i];
        }
        */
	    
        foreach (var tankBodyPart in _tankBodyParts2)
        {
	        tankBodyPart.material.SetFloat("_Transition", 1f);
        }
        yield return new WaitForSeconds(_flashTime);
        foreach (var tankBodyPart in _tankBodyParts2)
        {
	        tankBodyPart.material.SetFloat("_Transition", 0f);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BotTankDamageVisual))]
class BotTankDamageVisualEditor : Editor
{
    bool toggleShowGrassInEditor = true;
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
	        sourceScript._tankBodyParts2 = _tankBodyParts.ToArray();
        }
    }
}
#endif
