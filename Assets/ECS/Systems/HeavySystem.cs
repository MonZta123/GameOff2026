using System;
using Domain;
using ECS.Events;
using ECS.Notification;
using UnityEngine;

namespace ECS.Systems
{
    public class HeavySystem
    {
        public HeavySystem(int tickRate)
        {
            _shootCooldownTicks = (int)MathF.Floor(tickRate * _secondsPrimaryCooldown);
            _secondaryCooldownTicks = (int)MathF.Floor(tickRate * _secondsSecondaryCooldown);
            _secondaryActiveTicks = (int)MathF.Floor(tickRate * _secondaryDuration);
            _blockedInputTicksAfterSecondary = (int)MathF.Floor(tickRate * _blockedInputDurationAfterSecondary);

            _speed = Constants.HeavyMoveSpeed;
        }

        private readonly float _secondsPrimaryCooldown = 0.25f;
        private readonly int _shootCooldownTicks;

        private readonly float _secondsSecondaryCooldown = 2.0f;
        private readonly int _secondaryCooldownTicks;

        private readonly float _secondaryDuration = 0.5f;
        private readonly int _secondaryActiveTicks;

        private readonly float _blockedInputDurationAfterSecondary = 0.5f;
        private readonly int _blockedInputTicksAfterSecondary;

        private readonly float _speed;

        public void Execute(World world, int currentTick)
        {
            var moveEvt = world._moveEvents[0];
            ref var player = ref world.GetSnapshot()._players.GetEntityAt(0);
            ref var snapshot = ref world.GetSnapshot();

            if (player._primaryReadyAt <= currentTick)
            {
                if (moveEvt._primary)
                {
                    player._primaryReadyAt = currentTick + _shootCooldownTicks;
                    player._primaryBlockingMovementTo = currentTick + _shootCooldownTicks;

                    world.AddHeavyDamageCone(new HeavyDamageConeEvent()
                    {
                        _position = player._position, _angle = player._angle,
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

                    snapshot._soundNotificationPool.Add(new SoundNotification()
                    {
                        _soundType = SoundType.Dash,
                        _switchGroup = "FloorMaterial",
                        _position = player._position
                    });

                    var radians = player._angle * Mathf.Deg2Rad;

                    var direction = new Vector2(
                        Mathf.Cos(radians),
                        Mathf.Sin(radians)
                    );

                    player._velocity = direction * (_speed * 2f);

                    return;
                }
            }

            if (player._secondaryActivatedTo > currentTick)
            {
                return;
            }

            if (player._secondaryActivatedTo == currentTick && player._secondaryActivatedTo > 0)
            {
                snapshot._soundNotificationPool.Add(new SoundNotification()
                {
                    _soundType = SoundType.Slam,
                    _switchGroup = "FloorMaterial",
                    _position = player._position
                });
                
                world.AddBlastWaveEvent(new BlastWaveEvent() { _position = player._position, _radius = 2f });
            }

            if (player._isBlockedTo >= currentTick)
            {
                player._velocity = Vector2.zero;
                player._angle = moveEvt._angle;
                return;
            }

            if (player._primaryBlockingMovementTo < currentTick)
            {
                player._velocity = moveEvt._movement * _speed;
            }
            else
            {
                player._velocity = Vector2.zero;
            }

            player._angle = moveEvt._angle;
        }
    }
}
