using System;
using UnityEngine;

namespace ECS.Systems
{
    public class EnemySystem
    {
        public void Execute(World world, ReadOnlySpan<Intent> intents, int tick)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = 0; i < snapshot.numEnemies; i++)
            {
                ref var enemy = ref snapshot.enemies[i];

                var closestId = -1;
                var closestDistance = float.MaxValue;
                var closestPosition = Vector2.zero;
                for (var j = 0; j < snapshot.numPlayers; j++)
                {
                    ref var player = ref snapshot.players[j];

                    var distance = Vector2.Distance(player._position, enemy._position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestId = player._id;
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
