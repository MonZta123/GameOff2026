using UnityEngine;

namespace ECS.Entities
{
    public struct Player
    {
        public Player(short id)
        {
            _id = id;
            _position = Vector2.zero;
            _angle = 0;
            _health = 100;
            _type = PlayerType.Heavy;
            _lastAttackTick = 0;
            _colliderRadius = 1.0f;
        }
        
        public Vector2 _position;
        public float _angle;
        public float _health;
        public PlayerType _type;
        public int _lastAttackTick;
        
        public short _id;
        public float _colliderRadius;
    }
}
