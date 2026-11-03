using System;
using Domain;
using ECS.Entities;
using UnityEngine;

namespace ECS.Systems
{
    public class AttackSystem
    {
        public AttackSystem(int tickRate)
        {
            _heavyCooldown = Mathf.CeilToInt(Constants.HeavyCooldown * tickRate);
        }

        private readonly float _heavyCooldown; 

        public void Execute(World world, ReadOnlySpan<Intent> intents, int currentTick)
        {
            var snapshot = world.GetSnapshot();

            for (var i = 0; i < snapshot.numPlayers; i++)
            {
                var intent = intents[i];
                if (!intent.isShooting)
                    continue;
                
                ref var player =ref snapshot.players[i];

                switch (player.type)
                {
                    case PlayerType.Heavy:
                        HandleHeavy(world, intent, currentTick, ref player);
                        break;
                    case PlayerType.Hacker:
                        break;
                    case PlayerType.Demolition:
                        break;
                    case PlayerType.Stealth:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void HandleHeavy(World world, Intent intent,  int currentTick, ref Player player)
        {
            if (currentTick < player.lastAttackTick + _heavyCooldown)
                return;
            
            var projectileId = world.FetchProjectileId();
            
            var projectile = new Projectile(projectileId)
            {
                _position = player.position,
                _angle = player.angle,
                _ttl = 1000
            };
            
            world.SpawnProjectile(projectile);
            
            player.lastAttackTick = currentTick;
        }
    }
}
