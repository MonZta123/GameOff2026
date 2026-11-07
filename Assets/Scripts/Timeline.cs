using System;
using Domain;
using ECS;

public class Timeline
{
    public Timeline(int tickRate, int segmentTicks)
    {
        _segmentTicks = segmentTicks;

        _intents = new Intent[_segmentTicks * Constants.PlayerCount * Constants.Segments];
        _snapshots = new Snapshot[_segmentTicks + 1];

        _world = new World(tickRate);

        for (var i = 0; i < _segmentTicks + 1; i++)
        {
            var snapshot = new Snapshot();
            snapshot.Initialize();
            _snapshots[i] = snapshot;
        }
    }

    private readonly World _world;
    private readonly int _segmentTicks;
    private readonly Snapshot[] _snapshots;
    private readonly Intent[] _intents;

    private int _currentSegment = -1;

    public void AddIntent(Intent intent, int tick)
    {
        var activePlayerIndex = _snapshots[tick]._players.GetNumEntities() - 1;

        _intents[(tick * Constants.PlayerCount + activePlayerIndex) +
                 _currentSegment * _segmentTicks * Constants.PlayerCount] = intent;

        var span = _intents.AsSpan(
            (tick * Constants.PlayerCount) + _currentSegment * _segmentTicks * Constants.PlayerCount,
            Constants.PlayerCount);

        _world.SetState(_snapshots[tick]);
        _world.DoStep(span, tick);

        _snapshots[tick + 1].BuildFrom(_world.GetSnapshot());
    }

    public void InitNextSegment(MapDefinition mapDefinition)
    {
        _currentSegment++;
        _currentSegment %= Constants.Segments;

        _snapshots[0].Initialize();

        _world.BuildScenario(ref _snapshots[0], mapDefinition);
    }

    public Snapshot GetSnapshot(int tickDriverCurrentTick)
    {
        return _snapshots[tickDriverCurrentTick];
    }

    public void SetNextPlayer()
    {
        _world.SetNextPlayer(ref _snapshots[0]);
    }

    public int SnapshotLength => _snapshots.Length;
}
