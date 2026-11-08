using UnityEditor;
using UnityEngine;
using Visual;

namespace Actors
{
    public class Enemy : MonoBehaviour, IEntityView<ECS.Entities.Enemy>
    {
        [SerializeField]
        private SpriteRenderer gunRenderer;

        [SerializeField]
        private AnimatorOverrideController[] animatorOverrideControllers;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private MoveBouncer moveBouncer;

        public void Init(ECS.Entities.Enemy entity)
        {
            animator.runtimeAnimatorController = animatorOverrideControllers[(int)entity._type];
            
            _lastPosition = entity._position;
            _incomingEntity = entity;
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

            if (isWalking)
            {
                if (!moveBouncer.IsBouncing)
                    moveBouncer.SetIsBouncing(true);
            }
            else
            {
                if (moveBouncer.IsBouncing)
                    moveBouncer.SetIsBouncing(false);
            }
        }

        public void SetState(ECS.Entities.Enemy entity, int tick)
        {
            transform.position = entity._position;
            gunRenderer.transform.rotation = Quaternion.Euler(0, 0, entity._angle);

            _health = entity._health;
            _incomingEntity = entity;
            _tick = tick;
        }

        private ECS.Entities.Enemy _incomingEntity;
        private int _tick;

        private Vector2 _lastPosition;

        private float _health;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(transform.position, "" + _health);
        }
#endif

        public void Release()
        {
        }
    }
}
