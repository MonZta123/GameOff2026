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
        private Sprite[] gunSprites;

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

        [SerializeField]
        private LineRenderer lineRenderer;
        
        [SerializeField]
        private AK.Wwise.Event enemyDamageSound;

        public void Init(ECS.Entities.Enemy entity)
        {
            animator.runtimeAnimatorController = animatorOverrideControllers[(int)entity._type];

            _lastPosition = entity._position;
            _incomingEntity = entity;

            marker.gameObject.SetActive(entity._type == EnemyType.Buttler);
            marker.SetUsesArrow(false);

            switch (entity._type)
            {
                case EnemyType.Buttler:
                    gunRenderer.sprite = gunSprites[1];
                    break;
                case EnemyType.RedHairGuy:
                    gunRenderer.sprite = gunSprites[0];
                    break;
            }
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
                lineRenderer.enabled = false;
                return;
            }

            if (_incomingEntity._shootsAtTick > _tick)
            {
                if (!lineRenderer.enabled)
                    lineRenderer.enabled = true;

                var offset = (_incomingEntity._lockedOnPosition -
                              new Vector2(transform.position.x, transform.position.y)).normalized;

                lineRenderer.SetPosition(0, transform.position + new Vector3(offset.x, offset.y, 0));
                lineRenderer.SetPosition(1, _incomingEntity._lockedOnPosition);

                var timeLeft = (_incomingEntity._shootsAtTick - _tick) / 64f;

                if (timeLeft > 1f)
                {
                    lineRenderer.startColor = new Color(1, 0, 0);
                    lineRenderer.endColor = new Color(1, 0, 0);
                }
                else
                {
                    var val = Color.Lerp(Color.red, Color.white, (Mathf.Sin(_tick * 0.5f) + 1f) / 2f);
                    lineRenderer.startColor = val;
                    lineRenderer.endColor = val;
                }
            }
            else
            {
                if (lineRenderer.enabled)
                    lineRenderer.enabled = false;
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
                    enemyDamageSound.Post(gameObject);
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

            _tick = tick;
        }


        private int _tick;
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
            lineRenderer.enabled = false;
            gunRenderer.sprite = null;

            spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat(s_flashAmount, 0f);
            spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }
    }
}
