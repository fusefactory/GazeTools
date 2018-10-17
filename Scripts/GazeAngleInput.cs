using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GazeTools
{
	public class GazeAngleInput : MonoBehaviour
	{
		[Tooltip("The \"Gazing\" actor; this transform's 'position' and 'forward' property will be used as \"Gaze Ray\". When left empty, Camera.main.transform will be used.")]
		public Transform Actor;
		[Tooltip("The object that can be \"gazed at\" (only the Transform's position attribute is used), when left empty, this game object's Transform will be used")]
		public Transform Target;
		[Tooltip("When left empty, will look for Gazeable instance on the same gameObject")]
		public Gazeable Gazeable;

		public float MaxAngleBase = 10.0f;
		public float MaxAngleDistanceCorrection = 0.1f;
		public float MaxAngleMinimum = 1.5f;
      
#if UNITY_EDITOR
		[System.Serializable]
		public class Dinfo {
			public float Angle = 0.0f;
			public float MaxAngle = 0.0f;
		}
      
		public Dinfo DebugInfo;
#endif
      
		private Gazeable.Gazer gazer_ = null;
      
		private Transform actor_ { get { return this.Actor != null ? this.Actor : (Camera.main == null ? null : Camera.main.transform); } }

		private void OnDisable()
		{
			if (this.gazer_ != null)
			{
				this.gazer_.Dispose();
				this.gazer_ = null;
			}
		}

		#region Unity Methods
		void Start()
		{
			// gazeable defaults to the gazeable on our game object
			if (this.Gazeable == null) this.Gazeable = this.gameObject.GetComponent<Gazeable>();

			// target default to our own tranform
			if (Target == null) Target = this.transform;
        }

		void Update()
		{
			if (this.actor_ == null) return;

			float angle = GetAngle(this.actor_, this.Target);
			float maxAngle = Mathf.Max(this.MaxAngleMinimum, this.MaxAngleBase - this.MaxAngleDistanceCorrection * (this.Target.position - this.actor_.position).magnitude);
         
			bool isFocused = angle <= maxAngle;
         
			if (isFocused && this.gazer_ == null)
			{
				this.gazer_ = this.Gazeable.StartGazer();
			}

			if (this.gazer_ != null && !isFocused)
			{
				this.gazer_.Dispose();
				this.gazer_ = null;
			}
         
#if UNITY_EDITOR
            DebugInfo.Angle = angle;
			DebugInfo.MaxAngle = maxAngle;
#endif
		}
#endregion

		/// <summary>
        ///  Return angle between the actor's look (forward) vector and the vector from the actor to the target
        ///  0.0 means the actor is exactly facing the target
        ///  180.0 means the actor is facing exactly away from the target
        /// </summary>
        /// <returns>The ange in degrees.</returns>
		public static float GetAngle(Transform viewer, Transform target) {
			if (viewer == null || target == null) return 180.0f;
			Vector3 delta = (target.position - viewer.position).normalized;
			Vector3 forward = viewer.forward.normalized;
			return Vector3.Angle(delta, forward);
		}
	}
}