using System.Collections;
using ECS.Entities;
using UnityEditor;
using UnityEngine;
using Visual;

namespace Actors
{
    public class Enemy : MonoBehaviour, IEntityView<ECS.Entities.Enemy>
    {
        private static readonly int s_flashAmount = Shader.PropertyToID("_FlashAmount");

        [SerializeField]
        private SpriteRenderer gunRenderer;

        [SerializeField]
        private AnimatorOverrideController[] animatorOverrideControllers;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private MoveBouncer moveBouncer;

        [SerializeField]
        private Healthbar healthbar;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Marker marker;

        public void Init(ECS.Entities.Enemy entity)
        {
            animator.runtimeAnimatorController = animatorOverrideControllers[(int)entity._type];

            _lastPosition = entity._position;
            _incomingEntity = entity;

            marker.gameObject.SetActive(entity._type == EnemyType.Buttler);
            marker.SetUsesArrow(false);
        }

        private void Update()
        {
            var angle = _incomingEntity._angle;

            var velocity = _lastPosition - _incomingEntity._position;
            _lastPosition = _incomingEntity._position;

            var isWalking = velocity != Vector2.zero;

            var isDead = _health <= 0;

            if (isDead)
            {
                animator.Play("Dead");
                return;
            }

            var prefix = "Idle_";
            if (isWalking)
                prefix = "Walk_";

            var postfix = angle switch
            {
                >= 45 and < 135 => "Back",
                >= 135 and < 225 or >= 315 or < 45 => "Side",
                _ => "Front"
            };

            gunRenderer.sortingOrder = angle is > 0 and < 180 ? 4 : 6;
            gunRenderer.flipY = angle is > 90 or < -90;

            animator.Play(prefix + postfix);
        }

        public void SetState(ECS.Entities.Enemy entity, int tick)
        {
            transform.position = entity._position;
            gunRenderer.transform.rotation = Quaternion.Euler(0, 0, entity._angle);

            if (entity._health != _health || entity._maxHealth != _maxHealth)
            {
                if (entity._health < _health)
                {
                    DoFlash();
                }

                healthbar.SetHealthPercent(entity._health / (float)entity._maxHealth * 100);
            }

            _health = entity._health;
            _maxHealth = entity._maxHealth;

            if (entity._linearVelocity.SqrMagnitude() > 0.01f && entity._position != _lastPosition)
            {
                if (!moveBouncer.IsBouncing)
                    moveBouncer.SetIsBouncing(true);
            }
            else
            {
                if (moveBouncer.IsBouncing)
                    moveBouncer.SetIsBouncing(false);
            }

            _incomingEntity = entity;
        }

        private ECS.Entities.Enemy _incomingEntity;

        private Vector2 _lastPosition;

        private int _health;
        private int _maxHealth;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(transform.position, "" + _health);
        }
#endif

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

        public void StopBouncing()
        {
            moveBouncer.SetIsBouncing(false);
        }

        public void Release()
        {
            healthbar.ReturnToFullHealth();
            _health = 100;
            _lastPosition = Vector2.zero;

            spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat(s_flashAmount, 0f);
            spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }
    }
}
