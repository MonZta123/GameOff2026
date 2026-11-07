using ECS.Entities;
using UnityEngine;

namespace ECS.Systems
{
    public class MineSystem
    {
        public void Execute(World world)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = snapshot._players.GetNumEntities() - 1; i >= 0; i--)
            {
                var player = snapshot._players.GetEntityAt(i);
                for (var j = snapshot._triggers.GetNumEntities() - 1; j >= 0; j--)
                {
                    var trigger = snapshot._triggers.GetEntityAt(j);

                    if (trigger._triggerType == TriggerType.Mine)
                    {
                        if (trigger.IsInCircle(player._position, 0.5f, 0f, out _))
                        {
                            snapshot._triggers.DespawnAt(j);
                            player._health -= 40;
                            Debug.Log("boom");
                        }
                    }
                }
            }
        }
    }
}
