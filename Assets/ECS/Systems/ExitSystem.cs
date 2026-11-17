using ECS.Entities;

namespace ECS.Systems
{
    public class ExitSystem
    {
        public void Execute(World world)
        {
            ref var snapshot = ref world.GetSnapshot();

            for (var i = 0; i < snapshot._players.GetNumEntities(); i++)
            {
                ref var player = ref snapshot._players.GetEntityAt(i);
                player._escaped = false;

                for (var j = 0; j < snapshot._triggers.GetNumEntities(); j++)
                {
                    var trigger = snapshot._triggers.GetEntityAt(j);

                    if (trigger._triggerType == TriggerType.Exit &&
                        trigger.IsInBox(player._position, player._colliderRadius, out _))
                    {
                        player._escaped = true;
                        break;
                    }
                }
            }
        }
    }
}
