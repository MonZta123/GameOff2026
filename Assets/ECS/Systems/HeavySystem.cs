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
            _blockedInputTicksAfterSecondary = (int)MathF.Floor(tickRate * _blockedInputDurationAfterSecondary);
        }

        private int _tickRate;

        private float _secondsPrimaryCooldown = 0.25f;
        private int _shootCooldownTicks;

        private float _secondsSecondaryCooldown = 2.0f;
        private int _secondaryCooldownTicks;

        private float _secondaryDuration = 0.5f;
        private int _secondaryActiveTicks;

        private float _blockedInputDurationAfterSecondary = 0.125f;
        private int _blockedInputTicksAfterSecondary;

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
                    player._isBlockedTo = currentTick + _secondaryActiveTicks + _blockedInputTicksAfterSecondary;
                    
                    var radians = player._angle * Mathf.Deg2Rad;

                    var direction = new Vector2(
                        Mathf.Cos(radians),
                        Mathf.Sin(radians)
                    );

                    player._velocity = direction * 2f;

                    return;
                }
            }

            if (player._secondaryActivatedTo > currentTick)
            {
                return;
            }

            if (player._secondaryActivatedTo == currentTick && player._secondaryActivatedTo > 0)
            {
                world.AddBlastWaveEvent(new BlastWaveEvent()
                {
                    _position = player._position,
                    _radius = 3f,
                });
            }
            
            if (player._isBlockedTo >= currentTick)
            {
                player._velocity = Vector2.zero;
                player._angle = moveEvt._angle;
                return;
            }

            player._velocity = moveEvt._movement;
            player._angle = moveEvt._angle;
        }
    }
}
