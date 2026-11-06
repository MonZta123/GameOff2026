using System;
using ECS.Entities;
using ECS.Events;

namespace ECS.Systems
{
      public class MoveSystem
      {
    //     public MoveSystem()
    //     {
    //         
    //     }
    //     public void Execute(World world)
    //     {
    //         ref var snapshot = ref world.GetSnapshot();
    //
    //         var moveEvents = world._moveEvents;
    //
    //         for (var i = 0; i < moveEvents.Length; i++)
    //         {
    //             ref var player = ref snapshot._players.GetEntityAt(i);
    //
    //             switch (player._type)
    //             {
    //                 case PlayerType.Heavy:
    //                     if (!HandleHeavyMovement(moveEvents[i], ref player, snapshot))
    //                         continue;
    //                     break;
    //                 case PlayerType.Hacker:
    //                     break;
    //                 case PlayerType.Demolition:
    //                     break;
    //                 case PlayerType.Stealth:
    //                     break;
    //                 default:
    //                     throw new ArgumentOutOfRangeException();
    //             }
    //
    //             player._velocity = moveEvents[i]._movement;
    //         }
    //     }
    //
    //     private bool HandleHeavyMovement(MoveEvent evt, ref Player player, Snapshot snapshot)
    //     {
    //
    //         return true;
    //     }
    }
}
