using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GazeTools
{
	public class GazeFocusInput : MonoBehaviour
	{
		#region Public attributes
		[Tooltip("The \"Gazing\" actor; this transform's 'position' and 'forward' property will be used as \"Gaze Ray\". When left empty, Camera.main.transform will be used.")]
		public Transform actor;
		[Tooltip("The object that can be \"gazed at\" (only the Transform's position attribute is used), when left empty, this game object's Transform will be used")]
		public Transform target;
		[Tooltip("When left empty, will look for Gazeable instance on the same gameObject")]
		public Gazeable gazeable;
		public float minFocusLevel = 0.85f;

		public float FocusPercentage { get { return focusPercentage_; } }
		#endregion

		#region Private properties
		private float focusPercentage_ = 0.0f;
		private Gazeable.Gazer gazer_ = null;

		private Transform actor_ { get { return this.actor == null ? (Camera.main == null ? null : Camera.main.transform) : this.actor; }}
		#endregion

#if UNITY_EDITOR
		[System.Serializable]
		public class Dinfo {
			public float FocusPercentage = 0.0f;
		}

		public Dinfo DebugInfo;
#endif
      
		#region Unity Methods
		void Start()
		{
			// gazeable defaults to the gazeable on our game object
			if (gazeable == null) this.gazeable = this.gameObject.GetComponent<Gazeable>();

			// target default to our own tranform
			if (target == null) target = transform;
		}

		void Update()
		{
			this.focusPercentage_ = GetFocusPercentage(this.actor_, this.target);

#if UNITY_EDITOR
			this.DebugInfo.FocusPercentage = this.focusPercentage_;
#endif
         
			bool isFocused = focusPercentage_ >= minFocusLevel;

			if (isFocused && this.gazer_ == null)
			{
				this.gazer_ = this.gazeable.StartGazer();
			}

			if (this.gazer_ != null && !isFocused)
			{
				this.gazer_.Dispose();
				this.gazer_ = null;
			}
		}
		#endregion
      
		/// <summary>
		///  Return float value in the range from 0.0 to 1.0;
		///  0.0 means the subject is facing 180 degrees is the opposite direction
		///  1.0 means the subject is facing exactly towards the target
		/// </summary>
		/// <returns>The focus percentage.</returns>
		public static float GetFocusPercentage(Transform actor, Transform target) {         
            if (target == null || actor == null) return 0.0f;

            Vector3 targetPoint = (target.position - actor.position).normalized;
            Vector3 lookPoint = actor.forward.normalized;
            return 1.0f - (lookPoint - targetPoint).magnitude / 2.0f;
		}
	}
}