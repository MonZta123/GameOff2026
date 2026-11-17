using System;
using ECS;
using ECS.Entities;
using UnityEngine;

public struct GameFlowResult
{
    public GameFlowResultState _state;
    public string _comment;
    public int index;
}

public class GameFlowManager
{
    private string IndexToCharacter(int index)
    {
        switch (index)
        {
            case 0:
                return "Olga";
            case 1:
                return "Dave";
            case 2:
                return "Tien";
            case 3:
                return "Renoir";
            default:
                return "Player";
        }
    }

    public GameFlowManager(int ticksPerRun, int tickRate)
    {
        _ticksPerRun = ticksPerRun;
        _tickRate = tickRate;
    }

    private readonly int _tickRate;
    private readonly int _ticksPerRun;

    public GameFlowResult CheckForLoopBreak(int tick, Snapshot snapshot, bool isLastSegment, bool isReplayMode)
    {
        var currentPlayer = snapshot._players.GetNumEntities() - 1;

        for (var i = 0; i <= currentPlayer; i++)
        {
            if (snapshot._players.GetEntityAt(i)._health <= 0)
            {
                return new GameFlowResult()
                {
                    _state = GameFlowResultState.PlayerDead, _comment = IndexToCharacter(i), index = i
                };
            }
        }

        // Round end
        if (tick == _ticksPerRun)
        {
            if (isReplayMode)
            {
                if (isLastSegment)
                    return new GameFlowResult() { _state = GameFlowResultState.GameOver };

                return new GameFlowResult() { _state = GameFlowResultState.NextReplaySegment };
            }

            var moneyLeft = 0;
            for (var i = 0; i < snapshot._triggers.GetNumEntities(); i++)
                if (snapshot._triggers.GetEntityAt(i)._triggerType == TriggerType.Money)
                    moneyLeft++;

            if (moneyLeft > 0)
            {
                return new GameFlowResult()
                {
                    _state = GameFlowResultState.ObjectiveFailed, _comment = moneyLeft + " cash piles left."
                };
            }

            if (currentPlayer == 0 && snapshot._damageProcess != 0)
            {
                return new GameFlowResult()
                {
                    _state = GameFlowResultState.ObjectiveFailed,
                    _comment = snapshot._damageProcess + " enemies left."
                };
            }

            if (currentPlayer == 1 && snapshot._hackProcess != 0)
            {
                return new GameFlowResult()
                {
                    _state = GameFlowResultState.ObjectiveFailed,
                    _comment = Mathf.CeilToInt(snapshot._hackProcess / (float)_tickRate) + " seconds left to hack."
                };
            }

            if (currentPlayer == 2 && snapshot._bombsLeft != 0)
            {
                return new GameFlowResult()
                {
                    _state = GameFlowResultState.ObjectiveFailed, _comment = snapshot._bombsLeft + " left to place."
                };
            }

            if (currentPlayer == 3)
            {
                if (isLastSegment)
                    return new GameFlowResult() { _state = GameFlowResultState.StartReplay };

                return new GameFlowResult() { _state = GameFlowResultState.NextSegment };
            }

            return new GameFlowResult() { _state = GameFlowResultState.Success };
        }

        return new GameFlowResult() { _state = GameFlowResultState.Play };
    }
}
