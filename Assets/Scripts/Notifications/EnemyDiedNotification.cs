using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Notifications
{
    public class EnemyDiedNotification : NotificationView<ECS.Notification.EnemyDiedNotification>
    {
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AnimatorOverrideController[] overrideControllers;

        private Coroutine _coroutine;

        public override void Notify(ECS.Notification.EnemyDiedNotification notification)
        {
            animator.runtimeAnimatorController = overrideControllers[(int)notification._enemyType];
            if (_coroutine != null)
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(PlayAnimation(notification));

            if (notification._isKnockback)
            {
                var angle = Mathf.Atan2(notification._knockbackDirection.y, notification._knockbackDirection.x) *
                            Mathf.Rad2Deg;

                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            transform.position = notification._position;
        }

        private IEnumerator PlayAnimation(ECS.Notification.EnemyDiedNotification notification)
        {
            if (notification._isKnockback)
            {
                var elapsed = 0f;

                while (elapsed < 1.0f)
                {
                    elapsed += Time.deltaTime * 2f;

                    transform.position = Vector2.Lerp(transform.position,
                        notification._position + notification._knockbackDirection, elapsed);

                    yield return null;
                }
            }

            transform.rotation = quaternion.identity;
            animator.Play("Dead", 0, 0f);
            yield return new WaitForSeconds(3f);

            _returnToPool?.Invoke();
            _coroutine = null;
        }

        public override void OnReturn()
        {
            _coroutine = null;
        }
    }
}
