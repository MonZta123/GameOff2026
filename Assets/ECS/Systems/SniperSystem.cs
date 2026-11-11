using System;
using Domain;
using ECS.Entities;
using ECS.Events;
using ECS.Notification;
using UnityEngine;

namespace ECS.Systems
{
    public class SniperSystem
    {
        public SniperSystem(int tickRate)
        {
            _tickRate = tickRate;

            _shootCooldownTicks = (int)MathF.Floor(tickRate * _secondsPrimaryCooldown);

            _speed = Constants.SniperMoveSpeed;
        }

        private float _speed;

        private int _tickRate;

        private float _secondsPrimaryCooldown = 0.25f;
        private int _shootCooldownTicks;

        public void Execute(World world, int currentTick)
        {
            var moveEvt = world._moveEvents[3];
            ref var snapshot = ref world.GetSnapshot();
            ref var player = ref snapshot._players.GetEntityAt(3);

            if (player._primaryReadyAt <= currentTick)
            {
                if (moveEvt._primary)
                {
                    // Do Hitscan Shit

                    player._primaryReadyAt = currentTick + _shootCooldownTicks;

                    var radians = moveEvt._angle * Mathf.Deg2Rad;

                    var direction = new Vector2(
                        Mathf.Cos(radians),
                        Mathf.Sin(radians));

                    var step = direction / 16f;

                    var i = 0f;
                    Vector2 currentLazor;
                    do
                    {
                        currentLazor = player._position + step * i;

                        for (var j = snapshot._enemies.GetNumEntities() - 1; j >= 0; j--)
                        {
                            var enmy = snapshot._enemies.GetEntityAt(j);

                            if ((enmy._position - currentLazor).sqrMagnitude <
                                enmy._colliderRadius * enmy._colliderRadius)
                            {
                                snapshot._enemyDiedNotificationPool.Add(new EnemyDiedNotification()
                                {
                                    _enemyType = enmy._type,
                                    _isKnockback = true,
                                    _knockbackDirection = direction.normalized,
                                    _position = enmy._position
                                });

                                snapshot._enemies.DespawnAt(j);
                            }
                        }

                        i++;
                    } while (!world.IsBlocked(currentLazor) && i < 8000);

                    snapshot._snipeShotNotificationPool.Add(new SnipeShotNotification()
                    {
                        _from = player._position, _to = currentLazor,
                    });
                }
            }

            player._velocity = moveEvt._movement * _speed;
            player._angle = moveEvt._angle;
        }
    }
}
