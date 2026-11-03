using UnityEngine;

namespace ECS.Entities
{
    public struct Enemy
    {
        public short _id;
        public Vector2 _position;
        public float _angle;
        public float _lastAttackAt;
        public EnemyType _type;
        public float _health;
        public Vector2 _linearVelocity;

        public Enemy(short id)
        {
            _id = id;
            _position = Vector2.zero;
            _angle = 0f;
            _health = 0f;
            _lastAttackAt = 0f;
            _type = EnemyType.Gunman;
            _linearVelocity = Vector2.zero;
        }
    }
}
