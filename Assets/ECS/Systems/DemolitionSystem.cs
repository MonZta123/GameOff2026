using System;
using Domain;
using ECS.Entities;
using ECS.Events;
using UnityEngine;

namespace ECS.Systems
{
    public class DemolitionSystem
    {
        public DemolitionSystem(int tickRate)
        {
            _tickRate = tickRate;

            _shootCooldownTicks = (int)MathF.Floor(tickRate * _secondsPrimaryCooldown);
            _secondaryCooldownTicks = (int)MathF.Floor(tickRate * _secondsSecondaryCooldown);
            _secondaryActiveTicks = (int)MathF.Floor(tickRate * _secondaryDuration);

            _speed = Constants.DemolitionMoveSpeed;

            _deltaTime = 1f / tickRate;
        }

        private float _deltaTime;

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
            var moveEvt = world._moveEvents[2];
            ref var snapshot = ref world.GetSnapshot();
            ref var player = ref snapshot._players.GetEntityAt(2);

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

                    // HIER SECONDARY

                    return;
                }
            }

            if (player._secondaryActivatedTo > currentTick)
            {
                return;
            }

            for (var i = snapshot._triggers.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var trigger = ref snapshot._triggers.GetEntityAt(i);

                if (trigger._triggerType == TriggerType.DemolitionZone &&
                    trigger.IsInBox(player._position, 1f, out _) && moveEvt._action)
                {
                    trigger._timer = 0;
                    snapshot._bombsLeft--;
                    snapshot._triggers.DespawnAt(i);
                }

                if (trigger._triggerType == TriggerType.Mine &&
                    trigger.IsInCircle(player._position, 2f, 1f, out _) && moveEvt._action)
                {
                    trigger._timer -= _deltaTime;
                    if (trigger._timer <= 0)
                        snapshot._triggers.DespawnAt(i);
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
