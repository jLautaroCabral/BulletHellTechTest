using UnityEngine;
using UnityEngine.Serialization;

namespace FusionExamples.Tanknarok
{
	public class MotorShake : MonoBehaviour
	{
		[FormerlySerializedAs("shakeAmountByAxis")] [SerializeField] private Vector3 shakeScaleAmountByAxis = Vector3.zero;
		[SerializeField] private Vector3 shakePositionAmountByAxis = Vector3.zero;
		[SerializeField] private float shakeSpeed = 10f;

		private float offset;
		private Vector3 originScale;
		private Vector3 originPosition;

		void Start()
		{
			originScale = transform.localScale;
			offset = Random.Range(-Mathf.PI, Mathf.PI);
		}

		Vector3 CalculateScaleShake()
		{
			Vector3 shake = new Vector3(Mathf.Sin(Time.time * shakeSpeed + offset), Mathf.Sin(Time.time * shakeSpeed + offset), Mathf.Sin(Time.time * shakeSpeed + offset));
			shake.x *= shakeScaleAmountByAxis.x;
			shake.y *= shakeScaleAmountByAxis.y;
			shake.z *= shakeScaleAmountByAxis.z;
			return shake;
		}
		
		Vector3 CalculatePositiomShake()
		{
			Vector3 shake = new Vector3(Mathf.Sin(Time.time * shakeSpeed + offset), Mathf.Sin(Time.time * shakeSpeed + offset), Mathf.Sin(Time.time * shakeSpeed + offset));
			shake.x *= shakePositionAmountByAxis.x;
			shake.y *= shakePositionAmountByAxis.y;
			shake.z *= shakePositionAmountByAxis.z;
			return shake;
		}

		void Update()
		{
			transform.localScale = originScale + CalculateScaleShake();
			transform.localPosition = originPosition + CalculatePositiomShake();
		}
	}
}