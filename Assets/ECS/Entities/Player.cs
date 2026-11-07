using System;
using UnityEngine;

namespace ECS.Entities
{
    public struct Player
    {
        public PlayerType _type;

        public Vector2 _position;
        public Vector2 _velocity;
        public float _angle;
        public float _health;

        public float _colliderRadius;

        public int _primaryReadyAt;
        public int _primaryBlockingMovementTo;
        public int _secondaryReadyAt;
        public int _secondaryActivatedTo;
        public int _isBlockedTo;
        
        public bool _isActionActive;
    }
}
