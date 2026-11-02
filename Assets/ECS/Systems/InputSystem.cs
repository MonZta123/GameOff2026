using System;

namespace ECS.Systems
{
    public class InputSystem
    {
        public void Execute(World world, ReadOnlySpan<Intent> intents)
        {
            for (var i = 0; i < intents.Length; i++)
            {
                ref var moveEvent = ref world._moveEvents[i];
                
                moveEvent.movement = intents[i].movement;
                moveEvent.yaw = intents[i].yaw;
            }
        }
    }
}
