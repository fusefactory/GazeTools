using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GazeTools
{
	public class GazeCharger : MonoBehaviour
	{      
		[Tooltip("When left empty, will look for Gazeable instance on this GameObject")]
		public Gazeable gazeable;
		[Tooltip("When left empty, will look for Chargeable instance on this GameObject")]
		public Chargeable chargeable;
      
		void Start()
		{
			this.gazeable = this.gameObject.GetComponent<Gazeable>();
			this.chargeable = this.gameObject.GetComponent<Chargeable>();

			if (this.gazeable == null || this.chargeable == null)
			{
				this.enabled = false;
				return;
			}

			this.gazeable.GazeStartEvent.AddListener((Gazeable g) =>
			{
				this.chargeable.AddCharger(g);
			});

			this.gazeable.GazeEndEvent.AddListener((Gazeable g) =>
			{
				this.chargeable.RemoveCharger(g);
			});
		}
	}
}