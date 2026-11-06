using System;
using UnityEngine;
using ECS.Entities;
using ECS.Events;

namespace ECS.Systems
{
    public class DoorSystem
    {
        public void Execute(World world)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = world._numBlastWaveEvents - 1; i >= 0; i--)
            {
                ref var blastWaveEvent = ref world._blastWaveEvents[i];
                for (var j = snapshot._doors.GetNumEntities() - 1; j >= 0; j--)
                {
                    ref var door = ref snapshot._doors.GetEntityAt(j);

                    if (door._direction == Direction.Down && 
                        CheckShouldBlastDoorDown(door, world, blastWaveEvent))
                    {
                        snapshot._doors.DespawnAt(j);
                    }

                    if (door._direction == Direction.Right && 
                        CheckShouldBlastDoorRight(door, world, blastWaveEvent))
                    {
                        snapshot._doors.DespawnAt(j);
                    }
                }
            }
        }

        private bool CheckShouldBlastDoorDown(Door door, World world, BlastWaveEvent blastWaveEvent)
        {
            for (var k = 0; k < door._length; k++)
            {
                var currentPos = new Vector2(door._position.x + k, door._position.y);

                var breakDirection = blastWaveEvent._position - currentPos;
                var distanceVectorSqr = (breakDirection).sqrMagnitude;

                if (distanceVectorSqr >= blastWaveEvent._radius * blastWaveEvent._radius)
                    continue;
                
                world.AddDoorDestructionNotification(new DoorDestructionNotification()
                {
                    _door = door, _direction = breakDirection
                });

                return true;
            }

            return false;
        }

        private bool CheckShouldBlastDoorRight(Door door, World world, BlastWaveEvent blastWaveEvent)
        {
            for (var k = 0; k < door._length; k++)
            {
                var currentPos = new Vector2(door._position.x, door._position.y + k);

                var breakDirection = blastWaveEvent._position - currentPos;
                var distanceVectorSqr = (breakDirection).sqrMagnitude;

                if (distanceVectorSqr >= blastWaveEvent._radius * blastWaveEvent._radius)
                    continue;

                world.AddDoorDestructionNotification(new DoorDestructionNotification()
                {
                    _door = door, _direction = breakDirection
                });

                return true;
            }

            return false;
        }
    }
}
