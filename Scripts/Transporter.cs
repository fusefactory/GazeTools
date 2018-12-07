using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RSG;

namespace GazeTools
{
    /// <summary>
    /// Moves its GameObject towards the TargetTransform, using a configurable curve-motion
    /// </summary>
    public class Transporter : MonoBehaviour
    {
        public AnimationCurve curve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        public Transform TargetTransform = null;

        [System.Serializable]
        public class TransporterEvent : UnityEvent<Transporter> { }
        [System.Serializable]
        public class ProgressEvent : UnityEvent<float> { }

        [Header("Events")]
        [SerializeField] public TransporterEvent TransportStartEvent;
        [SerializeField] public TransporterEvent TransportEndEvent;
        [SerializeField] public ProgressEvent TransportProgressEvent;

        private float curvelength;
        private float curvetime = 0.0f;
        private bool isTransporting = false;
        private Vector3 startPos, endPos;
        private bool isLocal = false;
        private List<System.Action> TransportEndCallbacks = new List<System.Action>();


#if UNITY_EDITOR
        [System.Serializable]
        public class Dinfo
        {
            public float duration = 0.0f;
            public float t = 0.0f;
            public float progress = 0.0f;
            public bool finished = false;
        }

        public Dinfo DebugInfo;
#endif

        public float Progress { get { return this.curvetime / this.curvelength; } }

        void Start()
        {
            if (this.curve.keys.Length == 0)
            {
                this.curvelength = 0.1f;
            }
            else
            {
                var kf = this.curve.keys[this.curve.keys.Length - 1];
                this.curvelength = kf.time;
                kf.value = 1.0f;
            }

#if UNITY_EDITOR
            this.DebugInfo.duration = this.curvelength;
#endif
        }

        void Update()
        {
            if (this.TargetTransform != null)
            {
                this.endPos = this.TargetTransform.position;
                if (!isTransporting) this.transform.position = this.endPos;
            }

            if (isTransporting)
            {
                curvetime += Time.fixedDeltaTime;
                bool finished = curvetime >= curvelength;
                if (finished) curvetime = curvelength;

                Vector3 pos = Vector3.Lerp(this.startPos, this.endPos, this.curve.Evaluate(this.curvetime));
                if (this.isLocal)
                    this.transform.localPosition = pos;
                else
                    this.transform.position = pos;

                TransportProgressEvent.Invoke(Progress);

                if (finished) this.FinaliseTransport();

#if UNITY_EDITOR
                this.DebugInfo.t = this.curvetime;
                this.DebugInfo.progress = this.Progress;
                this.DebugInfo.finished = finished;
#endif
            }
        }

        #region Public Action Methods
        public void TransportNow()
        {
            this.TransportTo(this.TargetTransform);
        }

        public void TransportFrom(Vector3 origin)
        {
            this.transform.position = origin;
            if (this.TargetTransform != null) this.TransportTo(this.TargetTransform);
        }

        public void TransportToTransform(Transform transform)
        {
            this.TransportTo(transform.position);
            this.TargetTransform = transform;
        }

        public void TransportToWorldPosition(Vector3 pos)
        {
            this.TransportTo(pos, false);
        }
        #endregion

        public Promise TransportTo(Transform transform)
        {
            var promise = this.TransportTo(transform.position);
            this.TargetTransform = transform;
            return promise;
        }

        public Promise TransportToLocalPosition(Vector3 lpos)
        {
            return this.TransportTo(lpos, true);
        }

        public Promise TransportTo(Vector3 position, bool local = false)
        {
            return new Promise((resolve, reject) =>
            {
                if (isTransporting) this.FinaliseTransport();

                this.TargetTransform = null;
                this.isLocal = local;
                this.startPos = local ? this.transform.localPosition : this.transform.position;
                this.endPos = position;
                this.curvetime = 0.0f;
                this.isTransporting = true;

                this.TransportEndCallbacks.Add(resolve);
                this.TransportStartEvent.Invoke(this);
            });
        }

        public Promise TransportFromTo(Transform p1, Transform p2)
        {
            this.transform.position = p1 == null ? new Vector3(0, 0, 0) : p1.position;
            return (p2 == null ? this.TransportTo(new Vector3(0, 0, 0), true) : this.TransportTo(p2));
        }

        private void FinaliseTransport()
        {
            this.isTransporting = false;
            TransportEndEvent.Invoke(this);

            var funcs = this.TransportEndCallbacks.ToArray();
            this.TransportEndCallbacks.Clear();
            foreach (var func in funcs) func.Invoke();
            this.TransportEndCallbacks.Clear();
        }
    }
}