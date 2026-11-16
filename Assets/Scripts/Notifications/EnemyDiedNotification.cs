using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Notifications
{
    public class EnemyDiedNotification : NotificationView<ECS.Notification.EnemyDiedNotification>
    {
        private static readonly int s_flashAmount = Shader.PropertyToID("_FlashAmount");
        
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AnimatorOverrideController[] overrideControllers;

        private Coroutine _coroutine;
        
        [SerializeField]
        private SpriteRenderer spriteRenderer;

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
            DoFlash();
            
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
        
        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        private MaterialPropertyBlock _materialPropertyBlock;

        
        private void DoFlash()
        {
            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);

            _flashCoroutine = StartCoroutine(FlashMaterial());
        }
        
        private Coroutine _flashCoroutine;

        private IEnumerator FlashMaterial()
        {
            spriteRenderer.GetPropertyBlock(_materialPropertyBlock);

            _materialPropertyBlock.SetFloat(s_flashAmount, 1f);
            spriteRenderer.SetPropertyBlock(_materialPropertyBlock);

            yield return new WaitForSeconds(0.1f);

            _materialPropertyBlock.SetFloat(s_flashAmount, 0f);
            spriteRenderer.SetPropertyBlock(_materialPropertyBlock);

            _flashCoroutine = null;
        }

        
        public override void OnReturn()
        {
            spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat(s_flashAmount, 0f);
            spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
            _coroutine = null;
        }
    }
}
