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
    private int stepsPerSecond = 64;

    [SerializeField]
    private int maxSeconds = 30;

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

    [SerializeField]
    private Texture2D map;

    private GameFlowManager _gameFlowManager;

    private int _segmentTicks; 

    private void Awake()
    {
        gameObjectPools.Initialize();
        _segmentTicks = maxSeconds * stepsPerSecond;
        
        _gameFlowManager = new GameFlowManager(_segmentTicks);

        _world = new World(stepsPerSecond);
        // Show Loadingscreen here

        _intents = new Intent[_segmentTicks * Constants.PlayerCount * Constants.Segments];

        _snapshots = new Snapshot[_segmentTicks + 1];
        _tickDriver = new TickDriver(stepsPerSecond);

        _tickDriver._onTick += OnTick;

        for (var i = 0; i < _segmentTicks + 1; i++)
        {
            var snapshot = new Snapshot();
            snapshot.Initialize();
            _snapshots[i] = snapshot;
        }

        InitWorld();

        InitNextCharacter();
        // Hide Loadingscreen here
    }

    private void InitWorld()
    {
        var mapDefinition = TextureMapBuilder.GetMap(map);

        mapRenderer.GenerateMap(mapDefinition);
        _world.BuildScenario(ref _snapshots[0], mapDefinition);
    }

    private void OnDestroy()
    {
        _tickDriver.StopSim();
        _tickDriver._onTick -= OnTick;
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

        if (_tickDriver.CurrentTick != _lastSetTick && _tickDriver.CurrentTick < _snapshots.Length)
        {
            _lastSetTick = _tickDriver.CurrentTick;
            snapshotRenderer.SetSnapshot(_snapshots[_tickDriver.CurrentTick]);
            var result =
                _gameFlowManager.CheckForLoopBreak(_tickDriver.CurrentTick, _snapshots[_tickDriver.CurrentTick]);

            HandleResult(result);
        }
    }

    private void HandleResult(GameFlowResult result)
    {
        switch (result)
        {
            case GameFlowResult.Play:
                break;
            case GameFlowResult.Success:
                InitNextCharacter();
                break;
            case GameFlowResult.NextSegment:
                NextSegment();
                break;
            case GameFlowResult.ObjectiveFailed:
                Rewind();
                break;
            case GameFlowResult.PlayerDead:
                PlayerDead();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        // Ja ich weiß, das fehlt hier alles noch
    }

    private float _angle;

    private void WriteInput()
    {
        // Keep this for a possible rising edge
        if (Input.GetKeyDown(KeyCode.R))
        {
            DoTimeShift(0);
        }

        if (!_secondaryDown && Input.GetKeyDown(KeyCode.Space))
        {
            _secondaryDown = true;
        }

        if (!_actionDown && Input.GetKey(KeyCode.E))
        {
            _actionDown = true;
        }
    }

    private bool _timeShift;

    private bool _secondaryDown;
    private bool _actionDown;

    public Intent BuildIntent()
    {
        var xAxis = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        var yAxis = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

        var intent = new Intent()
        {
            _movement = Vector2.ClampMagnitude(new Vector2(xAxis, yAxis), 1),
            _primary = Input.GetMouseButton(0),
            _secondary = _secondaryDown,
            _action = _actionDown,
            _angle = _angle
        };

        _secondaryDown = false;
        _actionDown = false;

        return intent;
    }

    private Player _activePlayerObject;

    private void InitNextCharacter()
    {
        ref var snapshot = ref _snapshots[0];
        _world.SetNextPlayer(ref snapshot);
        
        snapshotRenderer.SetSnapshot(snapshot);

        _activePlayerObject?.SetActivePlayer(false);
        _activePlayerObject = snapshotRenderer.GetPlayerByIndex(snapshot._players.GetNumEntities() - 1);
        _activePlayerObject.SetActivePlayer(true);

        positionComposer.VirtualCamera.Follow = _activePlayerObject.transform;

        DoTimeShift(0);
    }

    private void NextSegment()
    {
        _currentSegment++;
        _currentSegment %= Constants.Segments;

        DoTimeShift(0);
    }

    private void PlayerDead()
    {
        DoTimeShift(0);
    }

    private void Rewind()
    {
        // Zusätzliche Funktion für visuellen Stuff
        DoTimeShift(0);
    }

    private void DoTimeShift(int tick)
    {
        _tickDriver.StopSim();
        _tickDriver.SetToTick(tick);

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

        var activePlayerIndex = _snapshots[tick]._players.GetNumEntities() - 1;
        
        _intents[
                (tick * Constants.PlayerCount + activePlayerIndex) +
                _currentSegment * _segmentTicks * Constants.PlayerCount] =
            BuildIntent();

        var span = _intents.AsSpan(
            (tick * Constants.PlayerCount) + _currentSegment * _segmentTicks * Constants.PlayerCount,
            Constants.PlayerCount);

        _world.SetState(_snapshots[tick]);
        _world.DoStep(span, tick);

        _snapshots[tick + 1].BuildFrom(_world.GetSnapshot());
    }

    private int _lastSetTick = -1;
    private int _currentSegment = 0;

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUILayout.Label($"Tick: {_tickDriver.CurrentTick}");
    }
#endif
}
