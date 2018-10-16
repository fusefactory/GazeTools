using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GazeTools
{
	public class ChargeableDirector : MonoBehaviour
	{
		public Chargeable Chargeable;
		public PlayableDirector Director;
		public bool UsePercentage = false;
		// Use this for initialization
		void Start()
		{
			if (this.Chargeable == null) this.Chargeable = GetComponent<Chargeable>();
			if (this.Director == null) this.Director = GetComponent<PlayableDirector>();
         
			if (this.Chargeable != null) this.Chargeable.ChargeChangeEvent.AddListener(this.OnChargeChange);
		}

		private void OnDestroy()
		{
			if (this.Chargeable != null) this.Chargeable.ChargeChangeEvent.RemoveListener(this.OnChargeChange);
		}
     
		// Update is called once per frame
		void OnChargeChange(Chargeable c)
		{
			if (!this.isActiveAndEnabled) return;
			float progress = this.UsePercentage ? c.ChargePercentage : c.Charge;
			this.Director.time = this.Director.duration * progress;
			this.Director.Evaluate();
		}
	}
}