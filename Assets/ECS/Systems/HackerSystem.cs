using System;
using Domain;
using ECS.Entities;
using ECS.Events;
using ECS.Notification;
using UnityEngine;

namespace ECS.Systems
{
    public class HackerSystem
    {
        public HackerSystem(int tickRate)
        {
            _tickRate = tickRate;

            _shootCooldownTicks = (int)MathF.Floor(tickRate * _secondsPrimaryCooldown);
            _secondaryCooldownTicks = (int)MathF.Floor(tickRate * _secondsSecondaryCooldown);
            _secondaryActiveTicks = (int)MathF.Floor(tickRate * _secondaryDuration);

            _speed = Constants.HackerMoveSpeed;
        }

        private float _speed;

        private int _tickRate;

        private float _secondsPrimaryCooldown = 0.25f;
        private int _shootCooldownTicks;

        private float _secondsSecondaryCooldown = 1.0f;
        private int _secondaryCooldownTicks;

        private float _secondaryDuration = 0.5f;
        private int _secondaryActiveTicks;

        public void Execute(World world, int currentTick)
        {
            var moveEvt = world._moveEvents[1];
            ref var snapshot = ref world.GetSnapshot();
            ref var player = ref snapshot._players.GetEntityAt(1);

            var wasHacking = player._isActionActive;

            player._isActionActive = false;

            if (player._primaryReadyAt <= currentTick && !wasHacking)
            {
                if (moveEvt._primary)
                {
                    player._primaryReadyAt = currentTick + _shootCooldownTicks;

                    world.AddShootEvent(new ShootEvent
                    {
                        _type = ProjectileType.Hacker,
                        _position = player._position,
                        _angle = moveEvt._angle,
                        _ttl = _tickRate * 3
                    });

                    snapshot._soundNotificationPool.Add(new SoundNotification()
                    {
                        _soundType = SoundType.HackerShoot, _position = player._position
                    });
                }
            }

            if (player._secondaryReadyAt <= currentTick)
            {
                if (moveEvt._secondary)
                {
                    player._secondaryReadyAt = currentTick + _secondaryCooldownTicks;
                    player._secondaryActivatedTo = currentTick + _secondaryActiveTicks;

                    var radians = moveEvt._angle * Mathf.Deg2Rad;

                    snapshot._soundNotificationPool.Add(new SoundNotification()
                    {
                        _soundType = SoundType.HackerDodge, _position = player._position
                    });

                    var direction = new Vector2(
                        Mathf.Cos(radians),
                        Mathf.Sin(radians)
                    );

                    player._velocity = direction * (_speed * 2f);

                    if (wasHacking && snapshot._hackProcess > 0)
                    {
                        snapshot._soundNotificationPool.Add(new SoundNotification()
                        {
                            _soundType = SoundType.InterruptedHacking, _position = player._position
                        });
                    }

                    return;
                }
            }

            if (player._secondaryActivatedTo > currentTick)
            {
                if (wasHacking && snapshot._hackProcess > 0)
                {
                    snapshot._soundNotificationPool.Add(new SoundNotification()
                    {
                        _soundType = SoundType.InterruptedHacking, _position = player._position
                    });
                }

                return;
            }

            for (var i = snapshot._triggers.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var trigger = ref snapshot._triggers.GetEntityAt(i);

                if (trigger._triggerType == TriggerType.HackerZone && snapshot._hackProcess > 0 &&
                    trigger.IsInBox(player._position, 1f, out var closestPoint) && moveEvt._action)
                {
                    snapshot._hackProcess--;

                    player._isActionActive = true;
                    
                    player._velocity = Vector2.zero;

                    if (snapshot._hackProcess == 0)
                    {
                        snapshot._soundNotificationPool.Add(new SoundNotification()
                        {
                            _soundType = SoundType.HackSuccess, _position = closestPoint
                        });
                        snapshot._triggers.DespawnAt(i);
                    }

                    if (!wasHacking)
                    {
                        snapshot._soundNotificationPool.Add(new SoundNotification()
                        {
                            _soundType = SoundType.StartedHacking, _position = closestPoint
                        });
                    }
                }
            }

            if (snapshot._hackProcess > 0 && wasHacking && !player._isActionActive)
            {
                snapshot._soundNotificationPool.Add(new SoundNotification()
                {
                    _soundType = SoundType.InterruptedHacking, _position = player._position
                });
            }

            if (player._isBlockedTo >= currentTick)
            {
                player._velocity = Vector2.zero;
                player._angle = moveEvt._angle;
                return;
            }

            player._velocity = moveEvt._movement * _speed;
            player._angle = moveEvt._angle;
        }
    }
}
