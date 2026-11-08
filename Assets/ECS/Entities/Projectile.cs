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
        GunMan
    }
    
    public struct Projectile
    {
        public Vector2 _position;
        public float _angle;
        public float _spawnTick;
        public ProjectileType _type;
        public float _ttl;
    }
}
