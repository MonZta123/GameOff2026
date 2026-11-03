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
            _heavyTtl = Mathf.CeilToInt(Constants.HeavyProjectileTtl * tickRate);

            _hackerCooldown = Mathf.CeilToInt(Constants.HackerCooldown * tickRate);
            _hackerTtl = Mathf.CeilToInt(Constants.HackerProjectileTtl * tickRate);
        }

        private readonly float _heavyCooldown;
        private readonly float _heavyTtl;

        private readonly float _hackerCooldown;
        private readonly float _hackerTtl;

        public void Execute(World world, ReadOnlySpan<Intent> intents, int currentTick)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = 0; i < snapshot.numPlayers; i++)
            {
                var intent = intents[i];
                if (!intent.isShooting)
                    continue;

                ref var player = ref snapshot.players[i];

                switch (player._type)
                {
                    case PlayerType.Heavy:
                        HandleHeavy(world, intent, currentTick, ref player);
                        break;
                    case PlayerType.Hacker:
                        HandleHacker(world, intent, currentTick, ref player);
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

        private void HandleHeavy(World world, Intent intent, int currentTick, ref Player player)
        {
            if (currentTick < player._lastAttackTick + _heavyCooldown)
                return;

            var projectileId = world.FetchProjectileId();

            var projectile = new Projectile(projectileId)
            {
                _position = player._position,
                _angle = player._angle,
                _spawnTick = currentTick,
                _ttl = _heavyTtl,
                _type = ProjectileType.Heavy
            };

            world.SpawnProjectile(projectile);

            player._lastAttackTick = currentTick;
        }

        private void HandleHacker(World world, Intent intent, int currentTick, ref Player player)
        {
            if (currentTick < player._lastAttackTick + _hackerCooldown)
                return;

            var projectileId = world.FetchProjectileId();

            var projectile = new Projectile(projectileId)
            {
                _position = player._position,
                _angle = player._angle,
                _spawnTick = currentTick,
                _ttl = _hackerTtl,
                _type = ProjectileType.Hacker
            };

            world.SpawnProjectile(projectile);

            player._lastAttackTick = currentTick;
        }
    }
}
