using Domain;
using ECS.Entities;
using ECS.Events;
using ECS.Notification;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Systems
{
    public class DoorSystem
    {
        private readonly float _minDot = math.cos(Constants.HeavyDamageConeAngle * Mathf.Deg2Rad);

        public void Execute(World world, int currentTick)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = 0; i < snapshot._players.GetNumEntities(); i++)
            {
                var player = snapshot._players.GetEntityAt(i);

                if (player._type == PlayerType.Heavy)
                {
                    if (player._secondaryActivatedTo > currentTick)
                    {
                        for (var j = snapshot._doors.GetNumEntities() - 1; j >= 0; j--)
                        {
                            ref var door = ref snapshot._doors.GetEntityAt(j);

                            if (door.IsInDoor(player._position, 0.5f, out var hitpoint))
                            {
                                snapshot._doorDestructionNotificationPool.Add(new DoorDestructionNotification()
                                {
                                    _door = door, _direction = (hitpoint - player._position).normalized
                                });
                                snapshot._doors.DespawnAt(j);
                            }
                        }
                    }
                }
            }

            if (world._hasBlastWaveEvent)
            {
                var blastWaveEvent = world._blastWaveEvent;
                for (var j = snapshot._doors.GetNumEntities() - 1; j >= 0; j--)
                {
                    ref var door = ref snapshot._doors.GetEntityAt(j);

                    if (door.IsInDoor(blastWaveEvent._position, blastWaveEvent._radius, out var hitPoint))
                    {
                        door._health--;
                        if (door._health <= 0)
                        {
                            snapshot._doorDestructionNotificationPool.Add(new DoorDestructionNotification()
                            {
                                _door = door, _direction = (hitPoint - blastWaveEvent._position).normalized
                            });
                            snapshot._doors.DespawnAt(j);
                        }
                    }
                }
            }

            if (world._hasHeavyDamageCone)
            {
                var cone = world._heavyDamageConeEvent;

                var radians = cone._angle * Mathf.Deg2Rad;

                var hitDirection = new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                );

                for (var i = snapshot._doors.GetNumEntities() - 1; i >= 0; i--)
                {
                    ref var door = ref snapshot._doors.GetEntityAt(i);

                    if (!door.IsInDoor(cone._position, Constants.HeavyDamageConeRadius, out var hitPoint))
                        continue;

                    var toTarget = (hitPoint - cone._position).normalized;

                    if (toTarget == Vector2.zero || math.dot(toTarget, hitDirection) > _minDot)
                    {
                        door._health--;
                        if (door._health <= 0)
                        {
                            snapshot._doorDestructionNotificationPool.Add(new DoorDestructionNotification()
                            {
                                _door = door, _direction = hitDirection
                            });
                            snapshot._doors.DespawnAt(i);
                        }
                    }
                }
            }
        }
    }
}
