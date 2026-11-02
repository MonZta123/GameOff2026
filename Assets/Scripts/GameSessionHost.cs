using System;
using Domain;
using ECS;
using UnityEngine;

public class GameSessionHost : MonoBehaviour
{
    [SerializeField]
    private int stepsPerSecond;

    [SerializeField]
    private int maxSeconds;

    [SerializeField]
    private SnapshotRenderer snapshotRenderer;

    [SerializeField]
    private GameObjectPool pool;

    private TickDriver _tickDriver;
    private World _world;

    private Snapshot[] _snapshots;
    private Intent[] _intents;
    private int _activePlayer;

    private void Awake()
    {
        var ticksTotal = maxSeconds * stepsPerSecond;

        // Show Loadingscreen here

        _world = new World();
        _intents = new Intent[ticksTotal * Constants.PlayerCount];

        _snapshots = new Snapshot[ticksTotal];
        _tickDriver = new TickDriver(stepsPerSecond);

        _tickDriver._onTick += OnTick;
        _tickDriver._onPostTick += OnPostTick;

        pool.Prewarm();

        for (var i = 0; i < ticksTotal; i++)
        {
            var snapshot = new Snapshot();
            snapshot.Initialize();
            _snapshots[i] = snapshot;

            var intent = new Intent();
            _intents[i] = intent;
        }

        _world.BuildScenario(ref _snapshots[0]);

        snapshotRenderer.SetSnapshot(_snapshots[0]);

        // Hide Loadingscreen here
    }

    private void OnDestroy()
    {
        _tickDriver.StopSim();
        _tickDriver._onTick -= OnTick;
        _tickDriver._onPostTick -= OnPostTick;
        _tickDriver = null;
    }

    public void Start()
    {
        _tickDriver.StartSim();
    }

    private void Update()
    {
        WriteInput();
        _tickDriver.Update(Time.deltaTime);
    }

    private void WriteInput()
    {
        // Keep this for a possible rising edge
        if (Input.GetKeyDown(KeyCode.R))
        {
            DoTimeShift(0);
        }
    }

    private bool _timeShift;

    public Intent BuildIntent()
    {
        var xAxis = Input.GetAxis("Horizontal");
        var yAxis = Input.GetAxis("Vertical");

        var intent = new Intent() { movement = Vector2.ClampMagnitude(new Vector2(xAxis, yAxis), 1) };

        return intent;
    }

    private void DoTimeShift(int tick)
    {
        _tickDriver.SetToTick(tick);

        ref var snapshot = ref _snapshots[tick];

        _world.SetNextPlayer(ref snapshot);

        snapshotRenderer.SetSnapshot(_snapshots[tick]);

        _activePlayer++;
        _activePlayer %= Constants.PlayerCount;
    }

    private void OnTick(int tick, float deltaTime)
    {
        if (tick + 1 >= _snapshots.Length)
        {
            _tickDriver.StopSim();
            // Time Over
            return;
        }

        _intents[tick * Constants.PlayerCount + _activePlayer] = BuildIntent();

        var span = _intents.AsSpan(tick * Constants.PlayerCount, Constants.PlayerCount);

        _world.SetState(_snapshots[tick]);
        _world.DoStep(span, tick, deltaTime);

        _snapshots[tick + 1].BuildFrom(_world.GetSnapshot());
    }

    private void OnPostTick(int tick, float deltaTime)
    {
        snapshotRenderer.SetSnapshot(_snapshots[tick + 1]);
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUILayout.Label($"Tick: {_tickDriver.CurrentTick}");
    }
#endif

    public void Rewind()
    {
        _tickDriver.SetToTick(20);
    }
}
