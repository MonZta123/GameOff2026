using System;
using ECS.Entities;
using ECS.Events;
using UnityEngine;

namespace ECS.Systems
{
    public class HeavySystem
    {
        public HeavySystem(int tickRate)
        {
            _tickRate = tickRate;

            _shootCooldownTicks = (int)MathF.Floor(tickRate * _secondsPrimaryCooldown);
            _secondaryCooldownTicks = (int)MathF.Floor(tickRate * _secondsSecondaryCooldown);
            _secondaryActiveTicks = (int)MathF.Floor(tickRate * _secondaryDuration);
        }

        private int _tickRate;

        private float _secondsPrimaryCooldown = 0.25f;
        private int _shootCooldownTicks;

        private float _secondsSecondaryCooldown = 2.0f;
        private int _secondaryCooldownTicks;

        private float _secondaryDuration = 0.5f;
        private int _secondaryActiveTicks;

        public void Execute(World world, int currentTick)
        {
            var moveEvt = world._moveEvents[0];
            ref var player = ref world.GetSnapshot()._players.GetEntityAt(0);

            if (player._primaryReadyAt <= currentTick)
            {
                if (moveEvt._primary)
                {
                    player._primaryReadyAt = currentTick + _shootCooldownTicks;

                    world.AddShootEvent(new ShootEvent
                    {
                        _type = ProjectileType.Heavy,
                        _position = player._position,
                        _angle = player._angle,
                        _ttl = _tickRate * 3
                    });
                }
            }

            if (player._secondaryReadyAt <= currentTick)
            {
                if (moveEvt._secondary)
                {
                    player._secondaryReadyAt = currentTick + _secondaryCooldownTicks;
                    player._secondaryActivatedTo = currentTick + _secondaryActiveTicks;

                    var radians = player._angle * Mathf.Deg2Rad;

                    var direction = new Vector2(
                        Mathf.Cos(radians),
                        Mathf.Sin(radians)
                    );

                    player._velocity = direction * 2f;

                    return;
                }
            } 

            if (player._secondaryActivatedTo >= currentTick)
            {
                return;
            }

            player._velocity = moveEvt._movement;
            player._angle = moveEvt._angle;

            // if (player._secondaryActive > 0)
            // {
            //     evt._movement = snapshot._players.GetEntityAt(0)._velocity;
            //     evt._angle = snapshot._players.GetEntityAt(0)._angle;
            //     return false;
            // }

            /*
             * if (currentTick < player._cooldownPrimary + _heavyCooldown)
                return;

            var projectile = new Projectile()
            {
                _position = player._position,
                _angle = player._angle,
                _spawnTick = currentTick,
                _ttl = _heavyTtl,
                _type = ProjectileType.Heavy
            };

            world.GetSnapshot()._projectiles.Spawn(projectile);

            player._cooldownPrimary = currentTick;
             */
        }
    }
}
