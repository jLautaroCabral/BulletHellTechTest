using UnityEngine;

[CreateAssetMenu(fileName = "TankDamageStep", menuName = "LautaroCabral/TankDamageStepAssetsSO", order = 1)]
public class TankDamageStepAssetsSO : ScriptableObject
{
    [SerializeField] private GameObject hitParticles;
    [SerializeField] private GameObject smokeDamageParticles;
    
    public GameObject HitParticles => hitParticles;
    public GameObject SmokeDamageParticles => smokeDamageParticles;
}