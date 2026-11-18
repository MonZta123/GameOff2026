using System;
using System.Collections;
using Domain;
using ECS;
using ECS.Entities;
using ECS.Notification;
using Notifications;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Player = Actors.Player;

public class GameSessionHost : MonoBehaviour
{
    [SerializeField]
    private PauseMenuScript pauseMenu;

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
    private Texture2D[] maps;

    [SerializeField]
    private Transform snipeTarget;

    [SerializeField]
    private TextMeshProUGUI rewindText;

    [SerializeField]
    private Image fade;

    [SerializeField]
    private SoundSystem soundSystem;

    [SerializeField]
    private MarkerManager markerManager;

    [SerializeField]
    private TaskTextManager taskTextManager;

    [SerializeField]
    private ScriptableRendererData scriptableRendererData;

    [SerializeField]
    private AK.Wwise.Event stopMusic;

    private TickDriver _tickDriver;
    private Timeline _timeline;

    private GameFlowManager _gameFlowManager;

    private int _segmentTicks;

    private GameSessionState _gameSessionState;

    private void Awake()
    {
        // Show LoadingScreen here
        pools.Initialize();
        _segmentTicks = maxSeconds * stepsPerSecond;
        var mapDefinition = TextureMapBuilder.GetMap(maps[0]);
        _gameSessionState = GameSessionState.Playing;
        markerManager.SetVisible(true);

        _tickDriver = new TickDriver(stepsPerSecond);
        _tickDriver._onTick += OnTick;

        _timeline = new Timeline(stepsPerSecond, _segmentTicks, (int)(Time.realtimeSinceStartup * 100));
        _timeline.InitNextSegment(mapDefinition);
        mapRenderer.GenerateMap(mapDefinition);

        rewindText.gameObject.SetActive(false);
        taskTextManager.SetText("");
        _gameFlowManager = new GameFlowManager(_segmentTicks, stepsPerSecond);

        scriptableRendererData.rendererFeatures[0].SetActive(false);
        // Hide Loadingscreen here
    }

    private IEnumerator FadeIn(Action next = null)
    {
        yield return null;
        var elapsed = 0f;

        var duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            fade.color = new Color(0, 0, 0, 1 - elapsed / duration);

            yield return null;
        }

        fade.color = new Color(0, 0, 0, 0);
        next?.Invoke();
    }

    private IEnumerator FadeOut(Action next = null)
    {
        yield return null;
        var elapsed = 0f;

        var duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            fade.color = new Color(0, 0, 0, elapsed / duration);

            yield return null;
        }

        fade.color = new Color(0, 0, 0, 1);
        next?.Invoke();
    }

    private void InitNextReplaySegment()
    {
        StartCoroutine(FadeOut(Next));
        return;

        void Next()
        {
            pools.ClearAllNotificationPools();
            pools.ClearAllObjectPools();

            var mapDefinition =
                TextureMapBuilder.GetMap(maps[((_timeline.GetCurrentSegment() + 1) % Constants.Segments)]);

            _timeline.InitNextSegment(mapDefinition);
            mapRenderer.GenerateMap(mapDefinition);

            _timeline.SetNextPlayer();
            _timeline.SetNextPlayer();
            _timeline.SetNextPlayer();
            _timeline.SetNextPlayer();

            var snapshot = _timeline.GetSnapshot(0);

            snapshotRenderer.SetSnapshot(snapshot, 0);

            _activePlayerObject = snapshotRenderer.GetPlayerByIndex(0);
            positionComposer.VirtualCamera.Follow = _activePlayerObject.transform;
            positionComposer.Damping = Vector3.one;

            inputManager.RemoveCrossHair();

            positionComposer.VirtualCamera.PreviousStateIsValid = false;

            StartCoroutine(FadeIn(ResetTickTimer));
        }
    }

    private void InitNextCharacter()
    {
        pools.ClearAllNotificationPools();

        rewindText.gameObject.SetActive(false);

        if (_rewindingCoroutine != null)
            StopCoroutine(_rewindingCoroutine);

        _rewindingCoroutine = StartCoroutine(PlayRewindAnimation(() => BuildNextCharacter()));
    }

    private void BuildNextCharacter(bool isFirst = false, Action next = null)
    {
        _timeline.SetNextPlayer();

        var snapshot = _timeline.GetSnapshot(0);

        snapshotRenderer.SetSnapshot(snapshot, 0);
        if (_activePlayerObject)
            _activePlayerObject.SetActivePlayer(false);
        _activePlayerObject = snapshotRenderer.GetPlayerByIndex(snapshot._players.GetNumEntities() - 1);
        _activePlayerObject.SetActivePlayer(true);

        UpdateCam();

        if (snapshot._players.GetNumEntities() > 1)
            inputManager.SetCrossHair(snapshot._players.GetNumEntities() - 1);
        else
            inputManager.RemoveCrossHair();

        _gameSessionState = GameSessionState.Playing;
        markerManager.SetVisible(true);

        next?.Invoke();

        if (!isFirst)
        {
            ResetTickTimer();
        }
        else
        {
            soundSystem.PlayCharacterMusic(snapshot._players.GetNumEntities() - 1, _timeline.GetCurrentSegment());
            inputManager.ClearInput();
            _tickDriver.StartSim();
        }

        SetMarkers();
    }

    private void SetMarkers()
    {
        var snapshot = _timeline.GetSnapshot(0);

        for (var i = 0; i < pools._triggerPool.Active.Length; i++)
        {
            var trigger = pools._triggerPool.Active[i];
            if (trigger)
                trigger.SetCurrentPlayerType((PlayerType)(snapshot._players.GetNumEntities() - 1));
        }
    }

    private void UpdateCam()
    {
        var snapshot = _timeline.GetSnapshot(0);

        if (snapshot._players.GetNumEntities() < 4 || _gameSessionState == GameSessionState.ReplayMode)
        {
            positionComposer.VirtualCamera.Follow = _activePlayerObject.transform;
            positionComposer.Damping = Vector3.one;
        }
        else
        {
            positionComposer.VirtualCamera.Follow = snipeTarget;
            positionComposer.Damping = Vector3.one * 0.5f;
        }
    }

    private int _lastProcessedTick;

    private void HandleResult(GameFlowResult resultState)
    {
        switch (resultState._state)
        {
            case GameFlowResultState.Play:
                break;
            case GameFlowResultState.Success:
                PlayerSuccess();
                break;
            case GameFlowResultState.NextSegment:
                NextSegment();
                break;
            case GameFlowResultState.ObjectiveFailed:
                ObjectiveFailed(resultState._comment);
                break;
            case GameFlowResultState.PlayerDead:
                PlayerDead(resultState._comment, resultState.index);
                break;
            case GameFlowResultState.StartReplay:
                InitReplay();
                break;
            case GameFlowResultState.NextReplaySegment:
                InitNextReplaySegment();
                break;
            case GameFlowResultState.GameOver:
                Debug.Log("Game over");
                // Say "thank you" and go back to the main menu
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(resultState), resultState, null);
        }
    }

    private Coroutine _rewindingCoroutine;

    private void PlayerSuccess()
    {
        AkUnitySoundEngine.StopAll();
        _gameSessionState = GameSessionState.WaitingForRSuccess;
        rewindText.gameObject.SetActive(true);
        rewindText.text = "Press R for next Player!";
        _tickDriver.StopSim();
        StopCharacterBouncing();
    }

    private void PlayerDead(string who, int index)
    {
        AkUnitySoundEngine.StopAll();
        _gameSessionState = GameSessionState.WaitingForR;
        rewindText.text = who + " was eliminated! Press R to retry!";
        rewindText.gameObject.SetActive(true);

        positionComposer.VirtualCamera.Follow = pools._playerPool.Active[index].transform;

        soundSystem.PlayFailMusic();

        _tickDriver.StopSim();
        StopCharacterBouncing();
    }

    private void DoRewindAnimation()
    {
        rewindText.gameObject.SetActive(false);

        if (_rewindingCoroutine != null)
            StopCoroutine(_rewindingCoroutine);

        _rewindingCoroutine = StartCoroutine(PlayRewindAnimation(ResetTickTimer));
        
        soundSystem.StopPauseMusic();
    }

    private void ObjectiveFailed(string reason)
    {
        AkUnitySoundEngine.StopAll();
        _gameSessionState = GameSessionState.WaitingForR;
        rewindText.text = reason + " Press R to rewind!";
        rewindText.gameObject.SetActive(true);
        
        soundSystem.PlayFailMusic();
        
        _tickDriver.StopSim();
    }

    private void InitReplay()
    {
        _gameSessionState = GameSessionState.ReplayMode;
        markerManager.SetVisible(false);
        if (_activePlayerObject)
            _activePlayerObject.SetActivePlayer(false);
        InitNextReplaySegment();
    }

    private void NextSegment()
    {
        AkUnitySoundEngine.StopAll();
        _tickDriver.StopSim();

        StartCoroutine(FadeOut(Action));
        return;

        void Action()
        {
            pools.ClearAllNotificationPools();
            pools.ClearAllObjectPools();

            var mapDefinition =
                TextureMapBuilder.GetMap(maps[((_timeline.GetCurrentSegment() + 1) % Constants.Segments)]);

            _timeline.InitNextSegment(mapDefinition);
            mapRenderer.GenerateMap(mapDefinition);
            positionComposer.VirtualCamera.PreviousStateIsValid = false;
            BuildNextCharacter(false, () => StartCoroutine(FadeIn()));
        }
    }

    private void OnDestroy()
    {
        _tickDriver.StopSim();
        _tickDriver._onTick -= OnTick;
        scriptableRendererData.rendererFeatures[0].SetActive(false);
        _tickDriver = null;
        stopMusic.Post(gameObject);
    }

    public void Start()
    {
        _gameSessionState = GameSessionState.Intro;
        StartCoroutine(DoIntroMusic(() => BuildNextCharacter(true, () => StartCoroutine(FadeIn()))));
    }

    private IEnumerator DoIntroMusic(Action next = null)
    {
        yield return new WaitForSeconds(0.2f);
        soundSystem.PlayIntroMusic(() =>
        {
            next?.Invoke();
            _gameSessionState = GameSessionState.Playing;
            markerManager.SetVisible(true);
        });
    }

    public void OnResumeGame()
    {
        soundSystem.ResumeMusic();
        pauseMenu.CloseAll();
        Time.timeScale = 1f;
        _pauseActive = false;
        pauseMenu.gameObject.SetActive(false);
        inputManager.ClearInput();
        soundSystem.StopPauseMusic();
    }

    private void PauseGame()
    {
        soundSystem.PauseMusic();
        Time.timeScale = 0f;
        _pauseActive = true;
        pauseMenu.gameObject.SetActive(true);
        soundSystem.PlayPauseMusic();
    }

    private bool _pauseActive;

    private void Update()
    {
        if (_pauseActive && inputManager.IsEscKeyDown)
        {
            OnResumeGame();
            return;
        }

        if (_gameSessionState is GameSessionState.ReplayMode or GameSessionState.Playing or GameSessionState.Intro &&
            inputManager.IsEscKeyDown)
        {
            PauseGame();
            return;
        }

        if (_gameSessionState == GameSessionState.Playing)
            _activePlayerObject.SetAngle(inputManager.GetAngle(_activePlayerObject.transform));

        _tickDriver.Update(Time.deltaTime);

        while (_lastProcessedTick + 1 <= _tickDriver.CurrentTick)
        {
            var snapshot = _timeline.GetSnapshot(_lastProcessedTick + 1);

            snapshotRenderer.RunNotifications(snapshot);
            snapshotRenderer.RunSounds(snapshot);
            _lastProcessedTick++;
        }

        if (_gameSessionState == GameSessionState.WaitingForR && inputManager.IsRKeyDown)
        {
            pools.ClearAllNotificationPools();
            _gameSessionState = GameSessionState.Rewinding;
            markerManager.SetVisible(false);
            DoRewindAnimation();
        }

        if (_gameSessionState == GameSessionState.WaitingForRSuccess && inputManager.IsRKeyDown)
        {
            pools.ClearAllNotificationPools();
            _gameSessionState = GameSessionState.Rewinding;
            markerManager.SetVisible(false);
            InitNextCharacter();
        }

        if (_gameSessionState is GameSessionState.Playing && _activePlayerObject)
        {
            _activePlayerObject.SetSwitchGroupFloor(1);
        }

        if (_tickDriver.CurrentTick != _lastSetTick && _tickDriver.CurrentTick < _timeline.SnapshotLength)
        {
            _lastSetTick = _tickDriver.CurrentTick;

            var snapshot = _timeline.GetSnapshot(_tickDriver.CurrentTick);
            snapshotRenderer.SetSnapshot(snapshot, _tickDriver.CurrentTick);

            if (_gameSessionState is GameSessionState.Playing or GameSessionState.ReplayMode)
            {
                var result = _gameFlowManager.CheckForLoopBreak(_tickDriver.CurrentTick, snapshot,
                    _timeline.GetCurrentSegment() == Constants.Segments - 1,
                    _gameSessionState == GameSessionState.ReplayMode);

                HandleResult(result);
            }

            if (_gameSessionState == GameSessionState.Playing)
            {
                UpdateTaskText();
            }
            else
            {
                taskTextManager.SetText("");
            }
        }
    }

    private void UpdateTaskText()
    {
        var tick = _tickDriver.CurrentTick;

        var text = "Your mission:";

        var snapshot = _timeline.GetSnapshot(tick);

        var currentPlayer = snapshot._players.GetLastEntity();

        var moneyLeft = 0;
        var isEvacuationMission = false;
        for (var i = 0; i < snapshot._triggers.GetNumEntities(); i++)
        {
            var trigger = snapshot._triggers.GetEntityAt(i);

            if (trigger._triggerType == TriggerType.Money)
                moneyLeft++;

            if (trigger._triggerType == TriggerType.Exit)
                isEvacuationMission = true;
        }

        if (moneyLeft > 0)
            text += "\nCollect " + moneyLeft + " cash piles!";

        if (isEvacuationMission)
            text += "\nStay in the evacuation point!";

        if (moneyLeft == 0 && !isEvacuationMission)
            text += " ";
        else
            text += "\n";

        switch (currentPlayer._type)
        {
            case PlayerType.Heavy:
                text += "Cause havoc! " + snapshot._damageProcess + " enemies left!";
                break;
            case PlayerType.Hacker:
                if (snapshot._hackProcess > 0)
                {
                    text += "Hack the computer - " +
                            (Mathf.CeilToInt(snapshot._hackProcess / (float)_tickDriver.TickRate) + " seconds left!");
                }
                else
                {
                    text += "Protect your team from incoming threats!";
                }

                break;
            case PlayerType.Demolition:
                if (snapshot._bombsLeft > 0)
                {
                    text += "Place detonators - " + snapshot._bombsLeft + " spots left!";
                }
                else
                {
                    var hasMines = false;
                    for (var i = 0; i < snapshot._triggers.GetNumEntities(); i++)
                    {
                        if (snapshot._triggers.GetEntityAt(i)._triggerType == TriggerType.Mine)
                        {
                            hasMines = true;
                            break;
                        }
                    }

                    if (hasMines)
                    {
                        text += "Defuse the mines - protect your Team!";
                    }
                    else
                    {
                        text += "Protect your team from incoming threats!";
                    }
                }

                break;
            case PlayerType.Sniper:
                var buttlerActive = false;

                for (var i = 0; i < snapshot._enemies.GetNumEntities(); i++)
                {
                    if (snapshot._enemies.GetEntityAt(i)._type != EnemyType.Buttler)
                        continue;

                    buttlerActive = true;
                    break;
                }

                text = buttlerActive ? "WARNING: Enemy Sniper is active!" : "No immediate threat detected!";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        text += "\nTime left: " +
                (maxSeconds - _tickDriver.CurrentTick / (float)stepsPerSecond).ToString("0.0") + " seconds";

        taskTextManager.SetText(text);
    }

    private Player _activePlayerObject;

    private void SetAllFootsteps(bool value)
    {
        for (var i = 0; i < pools._playerPool.Active.Length; i++)
        {
            var player = pools._playerPool.Active[i];
            if (player)
                player.SetFootstepsOnOff(value);
        }
    }

    private IEnumerator PlayRewindAnimation(Action next = null)
    {
        scriptableRendererData.rendererFeatures[0].SetActive(true);
        SetAllFootsteps(false);
        yield return null;

        var currentTick = _tickDriver.CurrentTick;

        var elapsed = 0f;

        soundSystem.PlayEvent(new SoundNotification() { _soundType = SoundType.Rewind });

        while (elapsed < Constants.RewindDuration)
        {
            elapsed += Time.deltaTime;

            var t = Mathf.Clamp01(elapsed / Constants.RewindDuration);

            var eased = t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;

            var currentValue = Mathf.RoundToInt(
                Mathf.Lerp(currentTick, 0f, eased)
            );

            _tickDriver.SetToTick(currentValue);

            yield return null;
        }

        _gameSessionState = GameSessionState.Playing;
        markerManager.SetVisible(true);
        rewindText.gameObject.SetActive(false);
        SetAllFootsteps(true);
        scriptableRendererData.rendererFeatures[0].SetActive(false);
        next?.Invoke();
    }

    private void ResetTickTimer()
    {
        _tickDriver.StopSim();

        _tickDriver.SetToTick(0);
        _lastProcessedTick = 0;

        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }

        _currentCoroutine = StartCoroutine(WaitForStart());
    }

    private IEnumerator WaitForStart()
    {
        yield return null;

        var elapsed = 0f;

        UpdateCam();

        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;

            yield return null;
        }

        inputManager.ClearInput();

        var snapshot = _timeline.GetSnapshot(0);

        soundSystem.PlayCharacterMusic(snapshot._players.GetNumEntities() - 1, _timeline.GetCurrentSegment());
        _tickDriver.StartSim();

        _currentCoroutine = null;
    }

    private Coroutine _currentCoroutine;

    private void OnTick(int tick, float deltaTime)
    {
        if (tick + 1 >= _segmentTicks + 1)
        {
            _tickDriver.StopSim();
            StopCharacterBouncing();
            return;
        }

        if (_gameSessionState == GameSessionState.ReplayMode)
        {
            _timeline.PlayIntent(tick);
        }
        else
        {
            var intent = inputManager.BuildIntent(_activePlayerObject.transform);

            _timeline.AddIntent(intent, tick);
        }
    }

    private void StopCharacterBouncing()
    {
        for (var i = 0; i < pools._playerPool.Active.Length; i++)
        {
            var player = pools._playerPool.Active[i];
            if (player)
                player.StopBouncing();
        }

        for (var i = 0; i < pools._enemyPool.Active.Length; i++)
        {
            var enemy = pools._enemyPool.Active[i];
            if (enemy)
                enemy.StopBouncing();
        }
    }

    private void LateUpdate()
    {
        if (!_activePlayerObject)
            return;

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
