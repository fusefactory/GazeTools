using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GazeTools
{
	/// <summary>
    /// Provides an interface for "registering" gazers, and privately managing its
	/// Basically a very fancy wrapper for a single boolean value; Gazed At yes/no.
	/// "gazed-at" state is based on if there's at least one registered active gazer.
	/// Invokes Gaze Start/Change/End events.
	/// </summary>
	public class Gazeable : MonoBehaviour
	{
		/// <summary>
		/// A single "gazing" actor which can be deactivated and (re-)activated.
        /// </summary>
		public class Gazer : System.IDisposable
		{
			/// <summary>
            /// The Gazeable this Gazer "gazes at"
            /// </summary>
            /// <value>The gazeable.</value>
			public Gazeable Gazeable { get; private set; }

            /// <summary>
			/// The active state of this gazer; a Gazeable is "gazed at" when it
			/// has at least one _active_ gazer
            /// </summary>
            /// <value><c>true</c> if is active; otherwise, <c>false</c>.</value>
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
         
            /// <summary>
            /// Changes the IsActive state of this Gazer and updated its Gazeable correspondingly
            /// </summary>
            /// <param name="active">new active IsActive state</param>
			public void SetActive(bool active)
			{
				if (active) this.Activate(); else this.Deactivate();
			}

			/// <summary>
            /// Activates this Gazer and updated its Gazeable correspondingly
            /// </summary>
            public void Activate()
			{
				this.activateFunc.Invoke(this);
				this.isActive = true;
			}

			/// <summary>
            /// Deactivates this Gazer and updated its Gazeable correspondingly
            /// </summary>
			public void Deactivate()
			{
				this.deactivateFunc.Invoke(this);
				this.isActive = false;
			}

			/// <summary>
            /// Toggle IsActive state of this Gazer and updated its Gazeable correspondingly
            /// </summary>
			public void Toggle() {
				this.SetActive(!this.isActive);
			}

			/// <summary>
            /// Deactivates this Gazer and updated its Gazeable correspondingly
            /// </summary>
			public void Dispose()
			{
				this.Deactivate();
			}
		}
      
		public class GazeableEvent : UnityEvent<Gazeable> { };
      
		[Header("Events")]
        /// <summary>
        /// Invoked when IsGazedAt state becomes true
        /// </summary>
		public GazeableEvent GazeStartEvent = new GazeableEvent();
		/// <summary>
        /// Invoked when IsGazedAt state becomes false
        /// </summary>      
		public GazeableEvent GazeEndEvent = new GazeableEvent();
		/// <summary>
        /// Invoked when IsGazedAt state changes
        /// </summary>      
		public GazeableEvent GazeChangeEvent = new GazeableEvent();
		/// <summary>
        /// Invoked when IsGazedAt state becomes true
        /// </summary>
		public UnityEvent OnGazeStart;
		/// <summary>
        /// Invoked when IsGazedAt state becomes false
        /// </summary>      
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
      
		/// <summary>
        /// Creates an activated gazer which is managed by the Gazeable
        /// </summary>
		/// <param name="owner">
		/// The Object by which the gazer can be identified
		/// for future operations (see EndGazer(Object) and ToggleGazer(Object)
		/// </param>
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

		/// <summary>
		/// Deactivated (if found) a gazer which is managed by the Gazeable
        /// </summary>
        /// <returns>The gazer instance.</returns>      
        public void EndGazer(Object owner)
        {
			Gazer gazer;
         
            // find existing gazer instance for given owner
            if (hostedGazers.TryGetValue(owner, out gazer))
            {
                gazer.Deactivate();
            }
        }
      
		/// <summary>
		/// Toggle active state of a gazer (if found) which is managed by the Gazeable
        /// </summary>
		/// <param name="owner">The Object by which the gazer can be identifier for future operations (see End</param>
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
      
        /// <summary>
        /// Invoked when a Gazer is activated
        /// </summary>
        /// <param name="gazer">Gazer.</param>
		private void StartGazer(Gazer gazer)
		{
			if (this.activeGazers.Contains(gazer)) return;
			bool gazedAtBefore = this.IsGazedAt;
			this.activeGazers.Add(gazer);
			if (!gazedAtBefore) this.NotifyChange(true);
		}
      
        /// <summary>
        /// Invoked when a gazer is deactivated
        /// </summary>
        /// <param name="gazer">Gazer.</param>
		private void EndGazer(Gazer gazer)
		{
			bool gazedAtBefore = this.IsGazedAt;
			this.activeGazers.Remove(gazer);
			bool gazedAtNow = this.IsGazedAt;
			if (gazedAtBefore && !gazedAtNow) this.NotifyChange(gazedAtNow);
		}
      
        /// <summary>
        /// Invoked whenever the internal isGazedAt state changes value
        /// </summary>
        /// <param name="currentlyGazedAt">If set to <c>true</c> currently gazed at.</param>
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