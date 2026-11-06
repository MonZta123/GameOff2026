using UnityEngine;
using ECS.Entities;
using ECS.Events;

namespace ECS.Systems
{
    public class DoorSystem
    {
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

                            if (ShouldBlastDoor(door, world, player._position, 0.5f))
                            {
                                snapshot._doors.DespawnAt(j);
                            }
                        }
                    }
                }
            }

            for (var i = world._numBlastWaveEvents - 1; i >= 0; i--)
            {
                ref var blastWaveEvent = ref world._blastWaveEvents[i];
                for (var j = snapshot._doors.GetNumEntities() - 1; j >= 0; j--)
                {
                    ref var door = ref snapshot._doors.GetEntityAt(j);

                    if (ShouldBlastDoor(door, world, blastWaveEvent._position, blastWaveEvent._radius))
                    {
                        snapshot._doors.DespawnAt(j);
                    }
                }
            }
        }

        private bool ShouldBlastDoor(Door door, World world, Vector2 position, float radius)
        {
            if (door.IsInDoor(position, radius, out var direction))
            {
                world.AddDoorDestructionNotification(new DoorDestructionNotification()
                {
                    _door = door, _direction = direction
                });

                return true;
            }

            return false;
        }

        // private bool CheckShouldBlastDoorDown(Door door, World world, Vector2 position, float radius)
        // {
        //     for (var k = 0; k < door._length; k++)
        //     {
        //         var currentPos = new Vector2(door._position.x, door._position.y + k);
        //
        //         var breakDirection = position - currentPos;
        //         var distanceVectorSqr = (breakDirection).sqrMagnitude;
        //
        //         if (distanceVectorSqr >= radius * radius)
        //             continue;
        //
        //         world.AddDoorDestructionNotification(new DoorDestructionNotification()
        //         {
        //             _door = door, _direction = breakDirection
        //         });
        //
        //         return true;
        //     }
        //
        //     return false;
        // }
        //
        // private bool CheckShouldBlastDoorRight(Door door, World world, Vector2 position, float radius)
        // {
        //     for (var k = 0; k < door._length; k++)
        //     {
        //         var currentPos = new Vector2(door._position.x + k, door._position.y);
        //
        //         var breakDirection = position - currentPos;
        //         var distanceVectorSqr = (breakDirection).sqrMagnitude;
        //
        //         if (distanceVectorSqr >= radius * radius)
        //             continue;
        //
        //         world.AddDoorDestructionNotification(new DoorDestructionNotification()
        //         {
        //             _door = door, _direction = breakDirection
        //         });
        //
        //         return true;
        //     }
        //
        //     return false;
        // }
    }
}
