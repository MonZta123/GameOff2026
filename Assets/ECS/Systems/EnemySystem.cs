using System;
using UnityEngine;

namespace ECS.Systems
{
    public class EnemySystem
    {
        public void Execute(World world)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = snapshot._enemies.GetNumEntities() - 1; i >= 0; i--)
            {
                ref var enemy = ref snapshot._enemies.GetEntityAt(i);

                var closestId = -1;
                var closestDistance = float.MaxValue;
                var closestPosition = Vector2.zero;
                for (var j =  snapshot._players.GetNumEntities() - 1; j >= 0; j--)
                {
                    ref var player = ref snapshot._players.GetEntityAt(j);

                    var distance = Vector2.Distance(player._position, enemy._position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestId = snapshot._players.GetIdAt(j);
                        closestPosition = player._position;
                    }
                }

                if (closestId != -1)
                {
                    var direction = closestPosition - enemy._position;

                    enemy._angle = Mathf.Atan2(
                        direction.y,
                        direction.x
                    ) * Mathf.Rad2Deg;
                }
            }
        }
    }
}
