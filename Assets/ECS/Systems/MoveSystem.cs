using System;

namespace ECS.Systems
{
    public class MoveSystem
    {
        public void Execute(World world, ReadOnlySpan<Intent> intents)
        {
            for (var i = 0; i < intents.Length; i++)
            {
                ref var moveEvent = ref world._moveEvents[i];
                
                moveEvent._movement = intents[i].movement;
                moveEvent._angle = intents[i].angle;
            }
        }
    }
}
