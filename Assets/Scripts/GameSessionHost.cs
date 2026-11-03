using System;
using Actors;
using Domain;
using ECS;
using Unity.Cinemachine;
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

    [SerializeField]
    private CinemachinePositionComposer positionComposer;

    [SerializeField]
    private Camera cam;

    private TickDriver _tickDriver;
    private World _world;

    private Snapshot[] _snapshots;
    private Intent[] _intents;
    private int _activePlayerIndex;

    private void Awake()
    {
        var ticksTotal = maxSeconds * stepsPerSecond;

        // Show Loadingscreen here

        _world = new World(stepsPerSecond);
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

        _activePlayerObject = snapshotRenderer.GetPlayerByIndex(_activePlayerIndex);
        _activePlayerObject.SetActivePlayer(true);
        positionComposer.VirtualCamera.Follow = _activePlayerObject.transform;

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

        var mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = mousePosition - _activePlayerObject.transform.position;

        _angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        _activePlayerObject.SetAngle(_angle);
        _tickDriver.Update(Time.deltaTime);
    }

    private float _angle;

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

        var intent = new Intent()
        {
            movement = Vector2.ClampMagnitude(new Vector2(xAxis, yAxis), 1),
            isShooting = Input.GetMouseButton(0),
            angle = _angle
        };

        return intent;
    }

    private Player _activePlayerObject;

    private void DoTimeShift(int tick)
    {
        _tickDriver.SetToTick(tick);

        ref var snapshot = ref _snapshots[tick];

        _world.SetNextPlayer(ref snapshot);

        snapshotRenderer.SetSnapshot(_snapshots[tick]);

        _activePlayerIndex++;
        _activePlayerIndex %= Constants.PlayerCount;

        _activePlayerObject.SetActivePlayer(false);
        _activePlayerObject = snapshotRenderer.GetPlayerByIndex(_activePlayerIndex);
        _activePlayerObject.SetActivePlayer(true);

        positionComposer.VirtualCamera.Follow = _activePlayerObject.transform;
    }

    private void OnTick(int tick, float deltaTime)
    {
        if (tick + 1 >= _snapshots.Length)
        {
            _tickDriver.StopSim();
            // Time Over
            return;
        }

        _intents[tick * Constants.PlayerCount + _activePlayerIndex] = BuildIntent();

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
}
