using System;
using UnityEngine;

namespace FusionExamples.Tanknarok
{
	public enum TankPartMaterialType
	{
		Primary = 0,
		Secondary = 1
	}
	public class TankPartMesh : MonoBehaviour
	{
		[SerializeField]
		private TankPartMaterialType materialType;
		
		[Obsolete]
		public void SetMaterial(Material material)
		{
			MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

			meshRenderer.sharedMaterial = material;
		}
		
		public void SetMaterial(PlayerMaterialsSO materialSO)
		{
			MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
			
			switch (materialType)
			{
				case TankPartMaterialType.Primary:
					meshRenderer.sharedMaterial = materialSO.primaryMaterial;
					break;
				case TankPartMaterialType.Secondary:
					meshRenderer.sharedMaterial = materialSO.secundaryMaterial;
					break;
				default:
					meshRenderer.sharedMaterial = materialSO.primaryMaterial;
					break;
			}
		}
	}
}