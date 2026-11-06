using System;
using ECS.Entities;
using ECS.Events;
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
        }

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

            if (player._secondaryActivatedTo > currentTick)
            {
                return;
            }

            if (player._secondaryActivatedTo == currentTick && player._secondaryActivatedTo > 0)
            {
            }

            for (var i = snapshot._triggers.GetNumEntities(); i >= 0; i--)
            {
                ref var trigger = ref snapshot._triggers.GetEntityAt(i);

                if (trigger._triggerType == TriggerType.HackerZone &&
                    trigger.IsInBox(player._position, 1f, out var closestPoint) && moveEvt._action)
                {
                    snapshot._hackProcess++;

                    if (snapshot._hackProcess == _tickRate * 5)
                    {
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

            player._velocity = moveEvt._movement;
            player._angle = moveEvt._angle;
        }
    }
}
