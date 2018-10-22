using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GazeTools
{
	[AddComponentMenu("GazeTools/ChargeableScaler")]
	public class ChargeableScaler : MonoBehaviour
	{
		[Tooltip("Defaults to any Chargeable found on this GameObject")]
		public Chargeable Chargeable;
        [Tooltip("Defaults to this GameObject's Transform")]
		public Transform Transform;
		public bool UsePercentage = false;

		void Start()
		{
			if (this.Chargeable == null) this.Chargeable = GetComponent<Chargeable>();
			if (this.Transform == null) this.Transform = GetComponent<Transform>();

			if (this.Chargeable != null) this.Chargeable.ChargeChangeEvent.AddListener(this.OnChargeChange);
		}
      
		private void OnDestroy()
		{
			if (this.Chargeable != null) this.Chargeable.ChargeChangeEvent.RemoveListener(this.OnChargeChange);
		}
      
		void OnChargeChange(Chargeable c)
		{
			if (!this.isActiveAndEnabled) return;
			float value = this.UsePercentage ? c.ChargePercentage : c.Charge;
			this.Transform.localScale = new Vector3(value, value, value);
		}
	}
}