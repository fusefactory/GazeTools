using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GazeTools
{
	public class GazeRayInput : MonoBehaviour
	{
		[Tooltip("When left empty, will look for Gazeable instance on the same gameObject")]
		public Gazeable Gazeable;
		[Tooltip("When left empty, will use Camera.main")]
		public Camera RayCamera;
		[Tooltip("When left empty, will look for Collider on Gazeable's game object")]
		public UnityEngine.UI.Graphic RaycastReceiver;

		private Gazeable.Gazer gazer_ = null;
      
		void Start()
		{
			if (this.Gazeable == null) this.Gazeable = this.gameObject.GetComponent<Gazeable>();
		}

		void Update()
		{
			var cam = this.RayCamera == null ? Camera.main : this.RayCamera;
			if (cam == null || this.Gazeable == null || this.RaycastReceiver == null) return;
         
			bool hit = RaycastReceiver.Raycast(new Vector2(0.5f, 0.5f), cam);

			// Just started gazing?
			if (hit && this.gazer_ == null)
			{
				this.gazer_ = this.Gazeable.StartGazer();
			}
         
			// Just ended our gaze?      
			if (this.gazer_ != null && !hit)
			{
				this.gazer_.Dispose();
				this.gazer_ = null;
			}
		}
	}
}