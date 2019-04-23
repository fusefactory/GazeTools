using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GazeTools
{
	/// <summary>
	/// Chargeable animates a float value back-and-forth along a configurable based
	/// on its boolean charging state; it's charging when there is at least one registered
	/// charger, it's discharging (animating backwards) when there are no registered chargers.
	/// </summary>
	public class Chargeable : MonoBehaviour
	{
		public enum State
		{
			IDLE, CHARGING, CHARGED, DECHARGING, MANUAL
		}

		public State state { get; private set; }

		public AnimationCurve chargeCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

#if UNITY_EDITOR
		[System.Serializable]
		public class Dinfo
		{
			public int ChargersCount = 0;
			public State State = State.IDLE;
			public bool IsCharging = false;
			public float Charge = 0.0f;
			public float Percentage = 0.0f;
			public float Time = 0.0f;
			public float Duration = 0.0f;
		}

		public Dinfo DebugInfo;
#endif

		[System.Serializable]
		public class ChargeableEvent : UnityEvent<Chargeable> { };
		[Header("Events")]
		public ChargeableEvent StateChangeEvent = new ChargeableEvent();
		[SerializeField]
		public ChargeableEvent ChargedEvent = new ChargeableEvent();
		public ChargeableEvent IdleEvent = new ChargeableEvent();
		public ChargeableEvent ChargingEvent = new ChargeableEvent();
		public ChargeableEvent DechargingEvent = new ChargeableEvent();
		public ChargeableEvent ChargeChangeEvent = new ChargeableEvent();
		[System.Serializable]
		public class FloatEvent : UnityEvent<float> { };
		[SerializeField]
		public FloatEvent ChargeValueEvent = new FloatEvent();
      
		private float chargeTime = 0.0f;
      
		public float ChargePercentage
		{
			get
			{
				return this.chargeTime / this.ChargeDur;
			}
		}

		public float Charge
		{
			get
			{
				return this.chargeCurve.Evaluate(this.chargeTime);
			}
		}

		public float ChargeDur
		{
			get
			{
				return this.chargeCurve.keys.Length == 0 ? 0.01f : this.chargeCurve.keys[this.chargeCurve.keys.Length - 1].time;
			}
			set
			{
				this.chargeCurve = AnimationCurve.Linear(0.0f, 0.0f, value, 1.0f);
			}
		}

		private List<Object> chargingObjects = new List<Object>();
      
		void Start()
		{
			this.state = State.IDLE;
		}

		private void OnEnable()
		{
			if (this.chargingObjects.Count > 0)
			{
				this.chargeTime = 0.0f;
				this.SetState(State.CHARGING);
			}
		}
      
		private void OnDisable()
		{
			this.SetState(State.IDLE);
		}

		void Update()
		{
#if UNITY_EDITOR
			this.DebugInfo.State = this.state;
#endif
			if (this.state == State.MANUAL) {
				var p = this.ChargePercentage;
				if (p >= 1.0f && this.chargingObjects.Count > 0) this.SetState(State.CHARGED);
				if (p > 0.0f && this.chargingObjects.Count == 0) this.SetState(State.DECHARGING);
				if (p < float.Epsilon && this.chargingObjects.Count == 0) this.SetState(State.IDLE);
				if (p < float.Epsilon && this.chargingObjects.Count > 0) this.SetState(State.CHARGING);
			}

			if (this.state == State.IDLE) return;

			if (this.state == State.CHARGING)
			{
				this.chargeTime += Time.unscaledDeltaTime;
				if (this.chargeTime >= this.ChargeDur)
				{
					this.chargeTime = this.ChargeDur;
					this.ChargeChangeEvent.Invoke(this);
					this.ChargeValueEvent.Invoke(this.Charge);
					this.SetState(State.CHARGED);
				}
				else
				{
					this.ChargeChangeEvent.Invoke(this);
					this.ChargeValueEvent.Invoke(this.Charge);
				}
			}

			if (this.state == State.DECHARGING)
			{
				this.chargeTime -= Time.unscaledDeltaTime;
				if (this.chargeTime <= 0)
				{
					this.chargeTime = 0;
					this.ChargeChangeEvent.Invoke(this);
					this.ChargeValueEvent.Invoke(this.Charge);
					this.SetState(State.IDLE);
				}
				else
				{
					this.ChargeChangeEvent.Invoke(this);
					this.ChargeValueEvent.Invoke(this.Charge);
				}
			}

#if UNITY_EDITOR
			this.DebugInfo.IsCharging = this.state.Equals(State.CHARGING);
			this.DebugInfo.Charge = this.Charge;
			this.DebugInfo.Time = this.chargeTime;
			this.DebugInfo.Duration = this.ChargeDur;
			this.DebugInfo.Percentage = this.ChargePercentage;
#endif
		}

		private void SetState(State newstate)
		{
			bool change = this.state != newstate;
			this.state = newstate;

			if (!change) return;

			this.StateChangeEvent.Invoke(this);
			if (state == State.CHARGED) this.ChargedEvent.Invoke(this);
			if (state == State.IDLE) this.IdleEvent.Invoke(this);
			if (state == State.CHARGING) this.ChargingEvent.Invoke(this);
			if (state == State.DECHARGING) this.DechargingEvent.Invoke(this);
		}

		#region Public Methods
		// AddCharger registers any kind of object as a 'charging actor'
		// (there can be multiple charging actors)
		// When this Chargeable has at least one charging actor; it will charge
		// or stay fully charged when charged.
		// When it has no charging actors, it will decharge or stay idle when
		// fully discharged

		public void AddCharger(Object o)
		{
			if (this.chargingObjects.Contains(o)) return;
			this.chargingObjects.Add(o);
#if UNITY_EDITOR         
			this.DebugInfo.ChargersCount = this.chargingObjects.Count;
#endif

			if (this.enabled && this.chargingObjects.Count == 1) this.SetState(State.CHARGING);
		}

		public void RemoveCharger(Object o)
		{
			if (this.chargingObjects.Remove(o))
			{
#if UNITY_EDITOR
                this.DebugInfo.ChargersCount = this.chargingObjects.Count;
#endif
            
				if (this.enabled && this.chargingObjects.Count == 0) this.SetState(State.DECHARGING);
			}
		}

		public void ToggleCharger(Object o)
		{
			if (this.chargingObjects.Contains(o)) this.RemoveCharger(o);
			else this.AddCharger(o);
		}

        /// <summary>
        /// Sets the current charge in percentage of the total charge
        /// </summary>
		/// <param name="p">The percentage value as a normalized float (0.0-1.0)</param>
		public void SetPercentage(float p) {
			this.chargeTime = this.ChargeDur * p;
			this.state = State.MANUAL;         
		}
      
		public void AddChargePercentage(float p) {
			this.SetPercentage(Mathf.Min(1.0f, ChargePercentage + p));
		}
      
		public void SetChargeDuration(float dur) {
			float percentage = this.ChargePercentage;
			this.ChargeDur = dur;
			this.SetPercentage(percentage);
		}
		#endregion
	}
}