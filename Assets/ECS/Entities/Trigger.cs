using UnityEngine;

namespace ECS.Entities
{
    public struct Trigger
    {
        public Vector2 _leftBottom;
        public Vector2 _rightTop;
        public TriggerType _triggerType;

        public float _timer;

        public bool IsInBox(Vector2 position, float radius, out Vector2 closestPoint)
        {
            closestPoint = new Vector2(Mathf.Clamp(position.x, _leftBottom.x, _rightTop.x),
                Mathf.Clamp(position.y, _leftBottom.y, _rightTop.y));

            var distanceSqr = (closestPoint - position).sqrMagnitude;

            return distanceSqr < radius * radius;
        }

        public bool IsInCircle(Vector2 position, float myRadius, float otherRadius, out Vector2 direction)
        {
            direction = position - (_leftBottom + _rightTop) / 2f;
            var distanceSqr = direction.sqrMagnitude;

            return distanceSqr <= (myRadius + otherRadius) * (myRadius + otherRadius);
        }
    }
}
