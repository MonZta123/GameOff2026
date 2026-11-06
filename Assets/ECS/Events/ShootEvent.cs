using ECS.Entities;
using UnityEngine;

namespace ECS.Events
{
    public struct ShootEvent
    {
        public ProjectileType _type;
        public Vector2 _position;
        public float _angle;
        public int _ttl;
    }
}
