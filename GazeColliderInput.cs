using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GazeTools
{
	public class GazeColliderInput : MonoBehaviour
	{
		[Tooltip("When left empty, will look for Gazeable instance on the same gameObject")]
		public Gazeable Gazeable;
		[Tooltip("When left empty, will use Camera.main")]
		public Camera GazeCamera;
		[Tooltip("When left empty, will look for Collider on Gazeable's game object")]
		public Collider Collider;
		public float MaxDistance = 100.0f;

		private Gazeable.Gazer gazer_ = null;
      
		void Start()
		{
			if (this.Gazeable == null) this.Gazeable = this.gameObject.GetComponent<Gazeable>();
			if (this.Collider == null) this.Collider = this.GetComponent<Collider>();
			if (this.Collider == null) this.Collider = this.Gazeable.GetComponent<Collider>();         
			// if (this.GazeCamera == null || this.Gazeable == null || this.Collider == null) this.enabled = false;
		}

		void Update()
		{
			var cam = this.GazeCamera == null ? Camera.main : this.GazeCamera;         
			if (cam == null || this.Gazeable == null || this.Collider == null) return;

			Ray gazeRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
			RaycastHit raycastHit;
			bool hit = this.Collider.Raycast(gazeRay, out raycastHit, this.MaxDistance);

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