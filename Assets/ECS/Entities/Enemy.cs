using UnityEngine;

namespace ECS.Entities
{
    public struct Enemy
    { 
        public Vector2 _position;
        public float _angle; 
        public EnemyType _type;
        public float _health;
        public Vector2 _linearVelocity;
    }
}
