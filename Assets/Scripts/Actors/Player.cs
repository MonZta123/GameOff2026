#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

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
        private Animator animator;

        private bool _activePlayer;

        public void Init(ECS.Entities.Player entity)
        {
            transform.position = entity._position;

            animator.runtimeAnimatorController = animatorOverrideController[(int)entity._type];
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

            var secondaryActive = _lastEntity._secondaryActivatedTo >= _tick;
            var primaryActive = _lastEntity._primaryBlockingMovementTo >= _tick;
            var isSlam = _lastEntity._isBlockedTo >= _tick && !secondaryActive;
            var actionActive = _lastEntity._isActionActive;
            var isDead = _lastEntity._health <= 0;

            if (isDead)
            {
                animator.Play("Dead");
                return;
            }

            if (isSlam)
            {
                Debug.Log("SLAM");
                animator.Play("Slam");
                return;
            }

            if (actionActive)
            {
                animator.Play("Action");
                return;
            }

            var prefix = "Idle_";
            var postfix = string.Empty;

            if (primaryActive)
            {
                prefix = "Primary_";
            }
            else if (secondaryActive)
            {
                Debug.Log("SECONDARY");
                prefix = "Secondary_";
            }
            else if (_lastEntity._velocity != Vector2.zero)
            {
                prefix = "Walk_";
            }

            if (angle is >= 315 or < 45)
            {
                postfix = "Side";
                characterRenderer.flipX = true;
            }
            else
                characterRenderer.flipX = false;

            if (angle is >= 45 and < 135)
                postfix = "Back";

            if (angle is >= 135 and < 225)
                postfix = "Side";

            if (angle is >= 225 and < 315)
                postfix = "Front";

            animator.Play(prefix + postfix);
        }

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
