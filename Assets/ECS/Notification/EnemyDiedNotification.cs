using ECS.Entities;
using UnityEngine;

namespace ECS.Notification
{
    public struct EnemyDiedNotification
    {
        public Vector2 _position;
        public EnemyType _enemyType;
        public bool _isKnockback;
        public Vector2 _knockbackDirection;
    }
}
