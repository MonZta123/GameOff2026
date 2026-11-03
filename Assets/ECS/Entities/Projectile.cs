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
        public Projectile(short id)
        {
            _id = id;
            _position = Vector2.zero;
            _angle = 0f;
            _spawnTick = 0f;
            _type = ProjectileType.Heavy;
            _ttl = 0f;
        }
        
        public short _id;
        public Vector2 _position;
        public float _angle;
        public float _spawnTick;
        public ProjectileType _type;
        public float _ttl;
    }
}
