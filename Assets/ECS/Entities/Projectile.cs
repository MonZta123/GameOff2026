using UnityEngine;

namespace ECS.Entities
{
    public enum ProjectileType
    {
        Heavy,
        Hacker,
        Demolition,
        Stealth,
        
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
