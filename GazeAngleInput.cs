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

#if UNITY_EDITOR
        [Header("Debug values")]
        public float angle_ = 0.0f;
		public float maxAngle;
#else
        private float angle_ = 0.0f;
		private float maxAngle;
#endif
      
		private Gazeable.Gazer gazer_ = null;
      
		private Transform actor_ { get { return this.Actor != null ? this.Actor : Camera.main.transform; } }
      
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
			this.angle_ = GetAngle();
			this.maxAngle = this.MaxAngleBase - this.MaxAngleDistanceCorrection * (this.Target.position - this.actor_.position).magnitude;

			bool isFocused = angle_ <= maxAngle;
         
			if (isFocused && this.gazer_ == null)
			{
				this.gazer_ = this.Gazeable.StartGazer();
			}
         
			if (this.gazer_ != null && !isFocused)
			{
				this.gazer_.Dispose();
				this.gazer_ = null;
			}
		}
#endregion

#region Custom Private Methods
		/// <summary>
		///  Return angle between the actor's look (forward) vector and the vector from the actor to the target
		///  0.0 means the actor is exactly facing the target
		///  180.0 means the actor is facing exactly away from the target
		/// </summary>
		/// <returns>The focus percentage.</returns>
		private float GetAngle()
		{
			Vector3 targetVector = (this.Target.position - this.actor_.position).normalized;
			Vector3 lookVector = this.actor_.forward.normalized;
			return Vector3.Angle(targetVector, lookVector);         
		}
#endregion
	}
}