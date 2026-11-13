using ECS;
using UnityEngine;

public class GameFlowManager
{
    public GameFlowManager(int ticksPerRun)
    {
        _ticksPerRun = ticksPerRun;
    }

    private readonly int _ticksPerRun;

    public GameFlowResult CheckForLoopBreak(int tick, Snapshot snapshot, bool isLastSegment, bool isReplayMode)
    {
        var currentPlayer = snapshot._players.GetNumEntities() - 1;

        for (var i = 0; i <= currentPlayer; i++)
        {
            if (snapshot._players.GetEntityAt(i)._health <= 0)
            {
                return GameFlowResult.PlayerDead;
            }
        }

        // Round end
        if (tick == _ticksPerRun)
        {
            if (isReplayMode)
            {
                if (isLastSegment)
                    return GameFlowResult.GameOver;
                
                return GameFlowResult.NextReplaySegment;
            }

            if (currentPlayer == 0 && snapshot._damageProcess != 0)
            {
                Debug.Log("Objective failed - damage left: " + snapshot._damageProcess + " enemies");
                return GameFlowResult.ObjectiveFailed;
            }

            if (currentPlayer == 1 && snapshot._hackProcess != 0)
            {
                Debug.Log("Objective failed - hack left: " + snapshot._hackProcess + " seconds");
                return GameFlowResult.ObjectiveFailed;
            }

            if (currentPlayer == 2 && snapshot._bombsLeft != 0)
            {
                Debug.Log("Objective failed - bombs left: " + snapshot._bombsLeft);
                return GameFlowResult.ObjectiveFailed;
            }

            if (currentPlayer == 3)
            {
                if (isLastSegment)
                    return GameFlowResult.StartReplay;
                
                return GameFlowResult.NextSegment;
            }
 
                
            return GameFlowResult.Success;
        }

        return GameFlowResult.Play;
    }
}
