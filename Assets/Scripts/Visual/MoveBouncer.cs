using System;
using UnityEngine;

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
        
        public void SetIsBouncing(bool isBouncing)
        {
            IsBouncing = isBouncing;

            if (!isBouncing)
            {
                transform.localRotation = Quaternion.identity;
                transform.localPosition = Vector3.zero;
                _count = 0;
            }
        }

        public bool IsBouncing { get; private set; }

        private void Update()
        {
            if (!IsBouncing)
                return;

            _count += Time.deltaTime * bounceSpeed;
            if (_count >= 2 * Mathf.PI)
                _count = 0;

            transform.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(_count)) * bounceHeight, 0);
            transform.localRotation = Quaternion.Euler(0, 0, (Mathf.Sin(_count + Mathf.PI / 2f) * bounceAngle));
        }

        private float _count;
    }
}
