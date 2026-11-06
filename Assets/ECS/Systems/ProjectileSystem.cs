using System;
using Domain;
using ECS.Entities;
using UnityEngine;

namespace ECS.Systems
{
    public class ProjectileSystem
    {
        public ProjectileSystem(int tickRate)
        {
            _heavyCooldown = Mathf.CeilToInt(Constants.HeavyCooldown * tickRate);
            _heavyTtl = Mathf.CeilToInt(Constants.HeavyProjectileTtl * tickRate);

            _hackerCooldown = Mathf.CeilToInt(Constants.HackerCooldown * tickRate);
            _hackerTtl = Mathf.CeilToInt(Constants.HackerProjectileTtl * tickRate);
            
            _deltaTime = 1f / tickRate;
        }

        private readonly float _deltaTime;
        
        private readonly float _heavyCooldown;
        private readonly float _heavyTtl;

        private readonly float _hackerCooldown;
        private readonly float _hackerTtl;
        
        public void Execute(World world, int tick)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = world._numShootEvents - 1; i >= 0; i--)
            {
                var shootEvent = world._shootEvents[i];
                
                snapshot._projectiles.Spawn(new Projectile()
                {
                    _type = shootEvent._type,
                    _position = shootEvent._position,
                    _angle = shootEvent._angle,
                    _spawnTick = tick,
                    _ttl = shootEvent._ttl
                });
            }
            
            for (var i = snapshot._projectiles.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var projectile = ref snapshot._projectiles.GetEntityAt(i);

                if (projectile._spawnTick + projectile._ttl <= tick)
                {
                    snapshot._projectiles.DespawnAt(i);
                }
            }
        }
    }
}
