using System;
using System.Collections;
using Actors;
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
    private Pools pools;

    [SerializeField]
    private CinemachinePositionComposer positionComposer;

    [SerializeField]
    private MapRenderer mapRenderer;

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private Texture2D map;

    [SerializeField]
    private Transform snipeTarget;

    private TickDriver _tickDriver;
    private Timeline _timeline;

    private GameFlowManager _gameFlowManager;

    private int _segmentTicks;

    private void Awake()
    {
        // Show LoadingScreen here
        pools.Initialize();
        _segmentTicks = maxSeconds * stepsPerSecond;
        var mapDefinition = TextureMapBuilder.GetMap(map);

        _tickDriver = new TickDriver(stepsPerSecond);
        _tickDriver._onTick += OnTick;

        _timeline = new Timeline(stepsPerSecond, _segmentTicks);
        _timeline.InitNextSegment(mapDefinition);
        mapRenderer.GenerateMap(mapDefinition);

        _gameFlowManager = new GameFlowManager(_segmentTicks);
        // Hide Loadingscreen here
    }

    private void InitNextCharacter(bool isFirst = false)
    {
        _timeline.SetNextPlayer();

        if (!isFirst)
            Rewind();
        else
            _tickDriver.StartSim();

        var snapshot = _timeline.GetSnapshot(0);

        snapshotRenderer.SetSnapshot(snapshot, 0);
        _activePlayerObject?.SetActivePlayer(false);
        _activePlayerObject = snapshotRenderer.GetPlayerByIndex(snapshot._players.GetNumEntities() - 1);
        _activePlayerObject.SetActivePlayer(true);

        if (snapshot._players.GetNumEntities() < 4)
        {
            positionComposer.VirtualCamera.Follow = _activePlayerObject.transform;
            positionComposer.Damping = Vector3.one;
        }
        else
        {
            positionComposer.VirtualCamera.Follow = snipeTarget;
            positionComposer.Damping = Vector3.one * 0.5f;
        }

        if (snapshot._players.GetNumEntities() > 1)
            inputManager.SetCrossHair(snapshot._players.GetNumEntities() - 1);
        else
            inputManager.RemoveCrossHair();
    }

    private int _lastProcessedTick;

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
                ObjectiveFailed();
                break;
            case GameFlowResult.PlayerDead:
                PlayerDead();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }

    private void PlayerDead()
    {
        Rewind();
    }

    private void ObjectiveFailed()
    {
        Rewind();
    }

    private void NextSegment()
    {
        // mapDefinition fehlt hier noch
        // Ja hier sollte die nächste Map kommen:
        pools.ClearAllNotificationPools();

        var mapDefinition = TextureMapBuilder.GetMap(map);

        _timeline.InitNextSegment(mapDefinition);
        mapRenderer.GenerateMap(mapDefinition);

        InitNextCharacter();
    }

    private void OnDestroy()
    {
        _tickDriver.StopSim();
        _tickDriver._onTick -= OnTick;
        _tickDriver = null;
    }

    public void Start()
    {
        InitNextCharacter(true);
    }

    private void Update()
    {
        _activePlayerObject.SetAngle(inputManager.GetAngle(_activePlayerObject.transform));
        _tickDriver.Update(Time.deltaTime);

        while (_lastProcessedTick + 1 < _tickDriver.CurrentTick)
        {
            snapshotRenderer.RunNotifications(_timeline.GetSnapshot(_lastProcessedTick + 1));
            _lastProcessedTick++;
        }

        if (_tickDriver.CurrentTick != _lastSetTick && _tickDriver.CurrentTick < _timeline.SnapshotLength)
        {
            _lastSetTick = _tickDriver.CurrentTick;

            var snapshot = _timeline.GetSnapshot(_tickDriver.CurrentTick);

            snapshotRenderer.SetSnapshot(snapshot, _tickDriver.CurrentTick);
            snapshotRenderer.RunNotifications(snapshot);

            var result = _gameFlowManager.CheckForLoopBreak(_tickDriver.CurrentTick, snapshot);

            HandleResult(result);
        }
    }

    private Player _activePlayerObject;

    private void Rewind()
    {
        _tickDriver.StopSim();
        _tickDriver.SetToTick(0);
        _lastProcessedTick = 0;

        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }

        _currentCoroutine = StartCoroutine(WaitForStart());
        pools.ClearAllNotificationPools();
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

        _currentCoroutine = null;
    }

    private Coroutine _currentCoroutine;

    private void OnTick(int tick, float deltaTime)
    {
        if (tick + 1 >= _segmentTicks + 1)
        {
            _tickDriver.StopSim();

            // Time Over
            return;
        }

        var intent = inputManager.BuildIntent(_activePlayerObject.transform);

        _timeline.AddIntent(intent, tick);
    }

    private void LateUpdate()
    {
        var screenPos = inputManager.SnipePosition();

        screenPos.x *= (cam.orthographicSize * cam.aspect - 2);
        screenPos.y *= cam.orthographicSize - 2;

        snipeTarget.position = _activePlayerObject.transform.position + screenPos;
    }

    private int _lastSetTick = -1;

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUILayout.Label($"Tick: {_tickDriver.CurrentTick}");
    }
#endif
}
