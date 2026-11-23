using System.Collections;
using ECS.Entities;
using UnityEngine;

namespace Notifications
{
    public class SplosionNotification : NotificationView<ECS.Notification.SplosionNotification>
    {
        private static readonly int s_progress = Shader.PropertyToID("_Progress");

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AK.Wwise.Event splosionSound;

        [SerializeField]
        private AK.Wwise.Event mineSplosionSound;

        [SerializeField]
        private SpriteRenderer ripple;

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        private MaterialPropertyBlock _materialPropertyBlock;

        public override void Notify(ECS.Notification.SplosionNotification notification)
        {
            transform.position = notification._position;

            if (notification._type == SplosionType.Heavy)
            {
                ripple.gameObject.SetActive(true);
                if (_rippleCoroutine != null)
                {
                    StopCoroutine(_rippleCoroutine);
                    _rippleCoroutine = null;
                }

                _rippleCoroutine = StartCoroutine(DoRipple());
            }
            else
            {
                animator.gameObject.SetActive(true);
                if (notification._type == SplosionType.Granade)
                    splosionSound.Post(gameObject);
                if (notification._type == SplosionType.Mine)
                    mineSplosionSound.Post(gameObject);

                StartCoroutine(ReturnThatStuff());
            }
        }

        private IEnumerator ReturnThatStuff()
        {
            yield return new WaitForSeconds(0.633f);
            
            _returnToPool?.Invoke();
        }

        private Coroutine _rippleCoroutine;

        private IEnumerator DoRipple()
        {
            yield return null;

            var elapsed = 0f;

            var duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                _materialPropertyBlock.SetFloat(s_progress, Mathf.Lerp(0f, 1f, elapsed / duration));
                ripple.SetPropertyBlock(_materialPropertyBlock);

                yield return null;
            }

            _returnToPool?.Invoke();
        }

        public override void OnReturn()
        {
            if (_rippleCoroutine != null)
            {
                StopCoroutine(_rippleCoroutine);
                _rippleCoroutine = null;
            }

            _materialPropertyBlock.SetFloat(s_progress, 0.0f);
            ripple.SetPropertyBlock(_materialPropertyBlock);

            ripple.gameObject.SetActive(false);
            animator.gameObject.SetActive(false);
        }
    }
}
