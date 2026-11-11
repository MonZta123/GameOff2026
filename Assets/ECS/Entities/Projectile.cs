using UnityEngine;

namespace ECS.Entities
{
    public enum ProjectileType
    {
        Heavy,
        Hacker,
        Demolition,
        Stealth,
        Dude1,
        Dude2,
        RonWeasly
    }
    
    public struct Projectile
    {
        public Vector2 _position;
        public float _angle;
        public int _spawnTick;
        public ProjectileType _type;
        public float _ttl;
    }
}
