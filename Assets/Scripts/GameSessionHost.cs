using System;
using System.Collections;
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
    private GameObjectPools gameObjectPools;

    [SerializeField]
    private CinemachinePositionComposer positionComposer;

    [SerializeField]
    private MapRenderer mapRenderer;

    [SerializeField]
    private Camera cam;

    private TickDriver _tickDriver;
    private World _world;

    private Snapshot[] _snapshots;
    private Intent[] _intents;
    
    private int _activePlayerIndex;
    private int _activePlayerId;
    
    [SerializeField]
    private Texture2D map;

    private void Awake()
    {
        gameObjectPools.Initialize();

        var ticksTotal = maxSeconds * stepsPerSecond;

        _world = new World(stepsPerSecond);
        // Show Loadingscreen here

        _intents = new Intent[ticksTotal * Constants.PlayerCount];

        _snapshots = new Snapshot[ticksTotal];
        _tickDriver = new TickDriver(stepsPerSecond);

        _tickDriver._onTick += OnTick;
        _tickDriver._onPostTick += OnPostTick;

        for (var i = 0; i < ticksTotal; i++)
        {
            var snapshot = new Snapshot();
            snapshot.Initialize();
            _snapshots[i] = snapshot;

            var intent = new Intent();
            _intents[i] = intent;
        }

        InitWorld();

        _activePlayerObject = snapshotRenderer.GetPlayerByIndex(_activePlayerIndex);
        _activePlayerObject.SetActivePlayer(true);

        positionComposer.VirtualCamera.Follow = _activePlayerObject.transform;
        // Hide Loadingscreen here
    }

    private void InitWorld()
    {
        var mapDefinition = TextureMapBuilder.GetMap(map);

        mapRenderer.GenerateMap(mapDefinition);

        _world.BuildScenario(ref _snapshots[0], mapDefinition);
        snapshotRenderer.SetSnapshot(_snapshots[0]);
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
        var xAxis = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        var yAxis = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

        var intent = new Intent()
        {
            movement = Vector2.ClampMagnitude(new Vector2(xAxis, yAxis), 1),
            primary = Input.GetMouseButton(0),
            secondary = Input.GetKeyDown(KeyCode.Space),
            action = Input.GetKeyDown(KeyCode.E),
            angle = _angle
        };

        return intent;
    }

    private Player _activePlayerObject;

    private void DoTimeShift(int tick)
    {
        _tickDriver.StopSim();
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

        StartCoroutine(WaitForStart());
    }

    private IEnumerator WaitForStart()
    {
        yield return null;

        var elapsed = 0f;

        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;

            yield return null;
        }

        _tickDriver.StartSim();
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
        _world.DoStep(span, tick);

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
