using System;
using Domain;
using ECS.Entities;
using ECS.Events;
using ECS.Notification;
using UnityEngine;

namespace ECS.Systems
{
    public class DemolitionSystem
    {
        public DemolitionSystem(int tickRate)
        {
            _tickRate = tickRate;

            _shootCooldownTicks = (int)MathF.Floor(tickRate * _secondsPrimaryCooldown);

            _speed = Constants.DemolitionMoveSpeed;

            _deltaTime = 1f / tickRate;
        }

        private float _deltaTime;

        private float _speed;

        private int _tickRate;

        private float _secondsPrimaryCooldown = Constants.DemolitionCooldown;
        private int _shootCooldownTicks;

        public void Execute(World world, int currentTick)
        {
            var moveEvt = world._moveEvents[2];
            ref var snapshot = ref world.GetSnapshot();
            ref var player = ref snapshot._players.GetEntityAt(2);

            player._isActionActive = false;

            if (player._primaryReadyAt <= currentTick)
            {
                if (moveEvt._primary)
                {
                    player._primaryReadyAt = currentTick + _shootCooldownTicks;

                    world.AddShootEvent(new ShootEvent
                    {
                        _type = ProjectileType.Demolition,
                        _position = player._position,
                        _angle = moveEvt._angle,
                        _ttl = (int)(_tickRate * Constants.DemolitionProjectileTtl)
                    });

                    snapshot._soundNotificationPool.Add(new SoundNotification()
                    {
                        _soundType = SoundType.DemolitionThrow, _position = player._position
                    });
                }
            }

            for (var i = snapshot._triggers.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var trigger = ref snapshot._triggers.GetEntityAt(i);

                if (trigger._triggerType == TriggerType.DemolitionZone &&
                    trigger.IsInBox(player._position, 1f, out _) && moveEvt._action)
                {
                    player._isActionActive = true;
                    trigger._timer = 0;
                    snapshot._bombsLeft--;
                    snapshot._triggers.DespawnAt(i);

                    snapshot._placedBombs.Spawn(new PlacedBomb() { _position = player._position });
                }
                else if (trigger._triggerType == TriggerType.Mine &&
                         trigger.IsInCircle(player._position, Constants.DemolitionMineReach, 1f, out _) && moveEvt._action)
                {
                    player._isActionActive = true;

                    trigger._timer -= _deltaTime;
                    if (trigger._timer <= 0)
                    {
                        snapshot._soundNotificationPool.Add(new SoundNotification()
                        {
                            _soundType = SoundType.MineDefuse, _position = player._position
                        });
                        snapshot._triggers.DespawnAt(i);
                    }
                }
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
