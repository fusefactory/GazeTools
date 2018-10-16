using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GazeTools
{
	/// <summary>
	/// Basically a very fancy wrapper for a single boolean value; Gazed at yes/no.
	/// Provides an interface for "registering" gazers, and privately managing its
	/// "gazed-at" state based on if there's at least one registered gazer.
	/// Invokes Gaze Start, End and Change events.
	/// </summary>
	public class Gazeable : MonoBehaviour
	{
		public class Gazer : System.IDisposable
		{
			public Gazeable Gazeable { get; private set; }
			public bool IsActive { get { return this.isActive; } }
         
			private System.Action<Gazer> activateFunc = null;
			private System.Action<Gazer> deactivateFunc = null;
			private bool isActive = false;
         
			public Gazer(Gazeable g, System.Action<Gazer> activateFunc, System.Action<Gazer> deactivateFunc)
			{
				this.Gazeable = g;
				this.activateFunc = activateFunc;
				this.deactivateFunc = deactivateFunc;
			}

			public void SetActive(bool active)
			{
				if (active) this.Activate(); else this.Deactivate();
			}

			public void Activate()
			{
				this.activateFunc.Invoke(this);
				this.isActive = true;
			}

			public void Deactivate()
			{
				this.deactivateFunc.Invoke(this);
				this.isActive = false;
			}

			public void Toggle() {
				this.SetActive(!this.isActive);
			}

			public void Dispose()
			{
				this.Deactivate();
			}
		}

		public class GazeableEvent : UnityEvent<Gazeable> { };
		[Header("Events")]
		public GazeableEvent GazeStartEvent = new GazeableEvent();
		public GazeableEvent GazeEndEvent = new GazeableEvent();
		public GazeableEvent GazeChangeEvent = new GazeableEvent();
		public UnityEvent OnGazeStart;
		public UnityEvent OnGazeEnd;

#if UNITY_EDITOR
		[Header("Debug-Info")]
		public bool GazedAt = false;
#endif
		public bool IsGazedAt { get { return this.activeGazers.Count > 0; } }

		private List<Gazer> activeGazers = new List<Gazer>();
		private Dictionary<Object, Gazer> hostedGazers = new Dictionary<Object, Gazer>();

		/// <summary>
		/// Provides a gazer instance which is not yet activated
		/// </summary>
		/// <returns>The gazer instance.</returns>
		public Gazer GetGazer()
		{
			var gazer = new Gazer(this, this.StartGazer, this.EndGazer);
			return gazer;
		}

		/// <summary>
		/// Provides a gazer instance which has been activated.
		/// </summary>
		/// <returns>The gazer instance.</returns>
		public Gazer StartGazer()
		{
			var gazer = this.GetGazer();
			gazer.Activate();
			return gazer;
		}

		public void StartGazer(Object owner)
        {
			Gazer gazer;

			// find existing gazer instance for given owner
			if (hostedGazers.TryGetValue(owner, out gazer)) {
				gazer.Activate();
				return;
			}

            // start new gazer and cache it
			gazer = StartGazer();
			hostedGazers.Add(owner, gazer);         
        }
      
        public void EndGazer(Object owner)
        {
			Gazer gazer;
         
            // find existing gazer instance for given owner
            if (hostedGazers.TryGetValue(owner, out gazer))
            {
                gazer.Deactivate();
            }
        }
      
		public void ToggleGazer(Object owner)
        {
            Gazer gazer;
         
            // find existing gazer instance for given owner
            if (hostedGazers.TryGetValue(owner, out gazer))
            {
				gazer.Toggle();
			} else {
				this.StartGazer(owner);
			}
        }
      
		private void StartGazer(Gazer gazer)
		{
			if (this.activeGazers.Contains(gazer)) return;
			bool gazedAtBefore = this.IsGazedAt;
			this.activeGazers.Add(gazer);
			if (!gazedAtBefore) this.NotifyChange(true);
		}

		private void EndGazer(Gazer gazer)
		{
			bool gazedAtBefore = this.IsGazedAt;
			this.activeGazers.Remove(gazer);
			bool gazedAtNow = this.IsGazedAt;
			if (gazedAtBefore && !gazedAtNow) this.NotifyChange(gazedAtNow);
		}

		private void NotifyChange(bool currentlyGazedAt)
		{
			this.GazeChangeEvent.Invoke(this);

			if (currentlyGazedAt)
			{
				this.OnGazeStart.Invoke();
				this.GazeStartEvent.Invoke(this);
			}
			else
			{
				this.OnGazeEnd.Invoke();
				this.GazeEndEvent.Invoke(this);
			}

#if UNITY_EDITOR
			this.GazedAt = currentlyGazedAt;
#endif
		}

		#region Static Methods
        /// <summary>
        /// Convenience method which automates some common init operations for objects that interact with a gazeable.
        /// </summary>
        /// <returns>The gazer.</returns>
        /// <param name="gazeable">Gazeable.</param>
        /// <param name="gameObject">Game object.</param>
		public static Gazer GetGazer(Gazeable gazeable, GameObject gameObject) {
			if (gazeable == null) gazeable = gameObject.GetComponentInChildren<Gazeable>();
			if (gazeable == null) return null;
			return gazeable.GetGazer();
		}
		#endregion
	}
}