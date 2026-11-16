using UnityEngine;
using UnityEngine.Events;

namespace Visual
{
    public class MoveBouncer : MonoBehaviour
    {
        [SerializeField]
        private float bounceSpeed = 3f;
        
        [SerializeField]
        private float bounceHeight = 0.5f;
        
        [SerializeField]
        private float bounceAngle = 45f;

        [SerializeField]
        private UnityEvent onTouchFloor;
        
        public void SetIsBouncing(bool isBouncing)
        {
            IsBouncing = isBouncing;

            if (!isBouncing)
            {
                transform.localRotation = Quaternion.identity;
                transform.localPosition = Vector3.zero;
                _count = 0;
                _bounceCount = 0;
            }
        }

        public bool IsBouncing { get; private set; }

        public int _bounceCount;

        private void Update()
        {
            if (!IsBouncing)
                return;

            _count += Time.deltaTime * bounceSpeed;
            if (_count >= 2 * Mathf.PI)
                _count = 0;

            transform.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(_count)) * bounceHeight, 0);
            transform.localRotation = Quaternion.Euler(0, 0, (Mathf.Sin(_count + Mathf.PI / 2f) * bounceAngle));

            var wave = (int)Mathf.Floor(_count / Mathf.PI);
            if (wave != _bounceCount)
            {
                _bounceCount = wave;
                onTouchFloor?.Invoke();
            }
        }

        private float _count;
    }
}
