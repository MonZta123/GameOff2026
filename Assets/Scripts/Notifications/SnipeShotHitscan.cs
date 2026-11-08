using System;
using System.Collections;
using ECS.Notification;
using UnityEngine;

namespace Notifications
{
    public class SnipeShotHitscan : NotificationView<SnipeShotNotification>
    {
        [SerializeField]
        private LineRenderer lineRenderer;

        [SerializeField]
        private LineRenderer lineRenderer2;

        private Coroutine _coroutine;

        private Material _material;
        private Material _material2;

        public override void Init(Action returnAction)
        {
            base.Init(returnAction);

            _material = lineRenderer.material;
            _material2 = lineRenderer2.material;
        }

        public override void Notify(SnipeShotNotification notification)
        {
            lineRenderer.SetPosition(0, notification._from);
            lineRenderer.SetPosition(1, notification._to);

            if (_coroutine != null)
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(Renderhitscan(notification));
        }

        private IEnumerator Renderhitscan(SnipeShotNotification notification)
        {
            yield return new WaitForSeconds(0.1f);

            var elapsed = 0f;

            while (elapsed >= 1.0f)
            {
                elapsed += Time.deltaTime;
                var width = 1 - elapsed / 0.15f;

                lineRenderer.widthMultiplier = width;

                _material.color = Color.Lerp(Color.red, new Color(1, 0, 0, 0), elapsed);
                _material2.color = Color.Lerp(Color.white, new Color(1, 0, 0, 0), elapsed * 2);

                yield return null;
            }

            _returnToPool?.Invoke();
        }

        public override void OnReturn()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            lineRenderer.widthMultiplier = 1;
            _material.color = Color.red;
        }
    }
}
