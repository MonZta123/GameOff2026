using UnityEngine;

namespace ECS.Entities
{
    public struct Door
    {
        public Direction _direction;
        public Vector2 _leftBottom;
        public Vector2 _rightTop;
        public int _health;

        public bool IsInDoor(Vector2 position, float radius, out Vector2 hitPoint)
        {
            hitPoint = new Vector2(Mathf.Clamp(position.x, _leftBottom.x, _rightTop.x),
                Mathf.Clamp(position.y, _leftBottom.y, _rightTop.y));

            var distanceSqr = (hitPoint - position).sqrMagnitude;

            return distanceSqr < radius * radius;
        }
    }
}
