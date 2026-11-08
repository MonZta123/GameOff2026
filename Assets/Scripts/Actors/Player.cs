#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Visual;

namespace Actors
{
    public class Player : MonoBehaviour, IEntityView<ECS.Entities.Player>
    {
        [SerializeField]
        private PositionSmoother positionSmoother;

        [SerializeField]
        private SpriteRenderer characterRenderer;

        [SerializeField]
        private SpriteRenderer gunRenderer;

        [SerializeField]
        private AnimatorOverrideController[] animatorOverrideController;

        [SerializeField]
        private Sprite[] gunz;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private MoveBouncer moveBouncer;

        private bool _activePlayer;

        public void Init(ECS.Entities.Player entity)
        {
            transform.position = entity._position;

            animator.runtimeAnimatorController = animatorOverrideController[(int)entity._type];

            gunRenderer.sprite = gunz[(int)entity._type];
        }

        public void SetActivePlayer(bool value)
        {
            _activePlayer = value;

            if (!positionSmoother.IsSmoothingPosition)
                positionSmoother.SetSmoothPosition(true);
        }

        public void SetAngle(float angle)
        {
            gunRenderer.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void Update()
        {
            var angle = gunRenderer.transform.rotation.eulerAngles.z;

            var secondaryActive = _lastEntity._secondaryActivatedTo > _tick;
            var primaryActive = _lastEntity._primaryBlockingMovementTo > _tick;
            var isSlam = _lastEntity._isBlockedTo >= _tick && !secondaryActive;
            var actionActive = _lastEntity._isActionActive;
            var isDead = _lastEntity._health <= 0;

            var isBlockingMovementChanged = _lastPrimaryBlockingMovementTo != _lastEntity._primaryBlockingMovementTo;
            _lastPrimaryBlockingMovementTo = _lastEntity._primaryBlockingMovementTo;

            var time = 0f;

            if (isDead)
            {
                animator.Play("Dead");
                return;
            }

            if (isSlam)
            {
                animator.Play("Slam");
                return;
            }

            if (actionActive)
            {
                animator.Play("Action");
                return;
            }

            var walk = false;
            var prefix = "Idle_";

            var isPrimary = false;

            if (primaryActive)
            {
                if (isBlockingMovementChanged)
                {
                    _isLeftHand = !_isLeftHand;
                }
                
                isPrimary = true;
                prefix = "Primary_";

                var start = _lastEntity._primaryBlockingMovementTo - 16;
                var end = _lastEntity._primaryBlockingMovementTo;

                if (end == start)
                    time = 0;
                else
                    time = Mathf.Clamp01((float)(_tick - start) / (float)(end - start));
            }
            else if (secondaryActive)
            {
                prefix = "Secondary_";
            }
            else if (_lastEntity._velocity != Vector2.zero)
            {
                prefix = "Walk_";
                walk = true;
            }

            if (!secondaryActive)
            {
                characterRenderer.flipX = angle is >= 315 or < 45;
            }
            else
            {
                characterRenderer.flipX = _lastEntity._velocity.x > 0;
            }

            var postfix = angle switch
            {
                >= 45 and < 135 => "Back",
                >= 135 and < 225 or >= 315 or < 45 => "Side",
                _ => "Front"
            };

            if (isPrimary)
            {
                animator.Play(prefix + postfix + (_isLeftHand ? "L" : "R"), 0, time);
            }
            else
            {
                animator.Play(prefix + postfix);
            }

            gunRenderer.sortingOrder = angle is > 0 and < 180 ? 4 : 6;
            gunRenderer.flipY = angle is > 90 and < 270;

            if (walk)
            {
                if (_lastEntity._velocity != Vector2.zero)
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
            else
            {
                if (moveBouncer.IsBouncing)
                    moveBouncer.SetIsBouncing(false);
            }
        }
        
        private bool _isLeftHand = false;

        private int _lastPrimaryBlockingMovementTo = 0;

        public void SetState(ECS.Entities.Player entity, int tick)
        {
            transform.position = entity._position;

            if (!_activePlayer)
            {
                gunRenderer.transform.rotation = Quaternion.Euler(0, 0, entity._angle);
            }

            _lastEntity = entity;
            _tick = tick;
        }

        private ECS.Entities.Player _lastEntity;
        private int _tick;

        public void Release()
        {
            positionSmoother.SetSmoothPosition(false);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(transform.position, transform.position.y + " " + transform.position.y);
        }
#endif
    }
}
