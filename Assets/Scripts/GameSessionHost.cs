using System;
using System.Collections;
using System.Linq;
using Actors;
using Domain;
using ECS;
using ECS.Entities;
using ECS.Notification;
using Notifications;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Door = Actors.Door;
using Player = Actors.Player;
using Trigger = Actors.Trigger;

[Serializable]
public struct Map
{
    public Texture2D _area;
    public Texture2D _enemyMap;
    public MapType _mapType;
}

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
    private HUD hud;

    [SerializeField]
    private Map[] maps;

    [SerializeField]
    private Transform snipeTarget;

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

    [SerializeField]
    private ReplayCredits credits;

    [SerializeField]
    private FlashingText flashingText;

    [SerializeField]
    private string[] cutsceneNames;

    private TickDriver _tickDriver;
    private Timeline _timeline;

    private GameFlowManager _gameFlowManager;

    private int _segmentTicks;

    private GameSessionState _gameSessionState;

    private void Awake()
    {
// #if UNITY_EDITOR
//         maxSeconds = 5;
// #endif

        pools.Initialize();
        _segmentTicks = maxSeconds * stepsPerSecond;
        var mapDefinition = TextureMapBuilder.BuildMap(maps[0]);
        SetObjectives(mapDefinition);
        _gameSessionState = GameSessionState.Playing;
        markerManager.SetVisible(true);

        _tickDriver = new TickDriver(stepsPerSecond);
        _tickDriver._onTick += OnTick;

        _timeline = new Timeline(stepsPerSecond, _segmentTicks, (int)(Time.realtimeSinceStartup * 100), maps.Length);
        _timeline.InitNextSegment(mapDefinition);
        mapRenderer.GenerateMap(mapDefinition);

        taskTextManager.SetText("");
        _gameFlowManager = new GameFlowManager(_segmentTicks, stepsPerSecond);
        flashingText.SetState(FlashingTextState.Recording);

        hud.HideThreatMeter();
        hud.HideRewindMessage();
        hud.HideMissionPanel();
        hud.HideObjectivePanel();
        hud.InitLogoAnimation();
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
        hud.HideThreatMeter();

        StartCoroutine(FadeOut(Next));
        return;

        void Next()
        {
            pools.ClearAllNotificationPools();
            pools.ClearAllObjectPools();

            var mapDefinition =
                TextureMapBuilder.BuildMap(maps[((_timeline.GetCurrentSegment() + 1) % maps.Length)]);
            SetObjectives(mapDefinition);
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

        hud.HideRewindMessage();
        hud.HideObjectivePanel();

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


        var entityNum = snapshot._players.GetNumEntities() - 1;
        var state = _gameSessionState == GameSessionState.ReplayMode ? 4 : _timeline.GetCurrentSegment();

        next?.Invoke();

        if (!isFirst)
        {
            ResetTickTimer();
        }
        else
        {
            soundSystem.PlayCharacterMusic(entityNum, state);
            inputManager.ClearInput();
            _tickDriver.StartSim();
        }

        soundSystem.AnnounceCharacter(entityNum, state);

        Trigger.PlayerTypeHere = (PlayerType)(snapshot._players.GetNumEntities() - 1);
        SetMarkers();

        if (snapshot._players.GetNumEntities() == 1 && !Door.HeavyDestroyedDoor)
        {
            Door.ShowMarkersFor = ShowMarkerFor.Heavy;
        }
        else if (snapshot._players.GetNumEntities() == 3 && !Door.DemolitionDestroyedDoor)
        {
            Door.ShowMarkersFor = ShowMarkerFor.Demolition;
        }
        else
        {
            Door.ShowMarkersFor = ShowMarkerFor.None;
        }
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
                GoToThankYouScreen();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(resultState), resultState, null);
        }
    }

    private void GoToThankYouScreen()
    {
        StartCoroutine(FadeOut(() => SceneManager.LoadScene("Scenes/ThankYouScene", LoadSceneMode.Single)));
    }

    private Coroutine _rewindingCoroutine;

    private void PlayerSuccess()
    {
        AkUnitySoundEngine.StopAll();
        _gameSessionState = GameSessionState.WaitingForRSuccess;
        hud.ShowRewindMessage("Success! Press R to continue.");
        _tickDriver.StopSim();
        StopCharacterBouncing();
    }

    private void PlayerDead(string who, int index)
    {
        AkUnitySoundEngine.StopAll();

        hud.ShowOuchScreen(who + " died!");

        _gameSessionState = GameSessionState.WaitingForR;

        positionComposer.VirtualCamera.Follow = pools._playerPool.Active[index].transform;

        soundSystem.PlayFailMusic();
        soundSystem.PlayDeadSound(index);

        _tickDriver.StopSim();
        StopCharacterBouncing();
    }

    private void DoRewindAnimation()
    {
        hud.HideRewindMessage();

        if (_rewindingCoroutine != null)
            StopCoroutine(_rewindingCoroutine);

        _rewindingCoroutine = StartCoroutine(PlayRewindAnimation(ResetTickTimer));

        soundSystem.StopPauseMusic();
    }

    private void ObjectiveFailed(string reason)
    {
        hud.ShowRewindMessage(reason);
        AkUnitySoundEngine.StopAll();
        _gameSessionState = GameSessionState.WaitingForR;

        soundSystem.PlayFailMusic();

        _tickDriver.StopSim();
    }

    private void InitReplay()
    {
        pools.ClearAllNotificationPools();
        pools.ClearAllObjectPools();
        
        AkUnitySoundEngine.StopAll();
        _tickDriver.StopSim();
        
        StartCoroutine(FadeOut(() => ShowCutscene(3, FinallyDoTheReplay)));
        return;

        void FinallyDoTheReplay()
        {
            Door.ShowMarkersFor = ShowMarkerFor.None;
            flashingText.SetState(FlashingTextState.Playing);
            soundSystem.WinCharacter();
            credits.RunCredits(maxSeconds * 4);
            _gameSessionState = GameSessionState.ReplayMode;
            Trigger.PlayerTypeHere = null;
            markerManager.SetVisible(false);
            if (_activePlayerObject)
                _activePlayerObject.SetActivePlayer(false);
            InitNextReplaySegment();
        }
    }

    private void NextSegment()
    {
        AkUnitySoundEngine.StopAll();
        _tickDriver.StopSim();

        StartCoroutine(FadeOut(() =>
        {
            pools.ClearAllNotificationPools();
            pools.ClearAllObjectPools();

            ShowCutscene(_timeline.GetCurrentSegment(), Action);
        }));
        return;

        void Action()
        {
            var mapDefinition =
                TextureMapBuilder.BuildMap(maps[((_timeline.GetCurrentSegment() + 1) % maps.Length)]);
            SetObjectives(mapDefinition);
            _timeline.InitNextSegment(mapDefinition);
            mapRenderer.GenerateMap(mapDefinition);
            positionComposer.VirtualCamera.PreviousStateIsValid = false;
            BuildNextCharacter(false, () => StartCoroutine(FadeIn()));
        }
    }

    private void ShowCutscene(int index, Action next = null)
    {
        var op = SceneManager.LoadSceneAsync(cutsceneNames[index], LoadSceneMode.Additive);
        if (op == null)
        {
            next?.Invoke();
            return;
        }

        if (index != 3)
            soundSystem.PlayCutsceneSound();
        else
            soundSystem.PlayHelicopterEnding();

        op.completed += OnLoaded;

        return;

        void OnLoaded(AsyncOperation _)
        {
            op.completed -= OnLoaded;
            _gameSessionState = GameSessionState.Cutscene;
            cam.enabled = false;
            hud.gameObject.SetActive(false);
            fade.enabled = false;
            var gos = SceneManager.GetSceneByName(cutsceneNames[index]).GetRootGameObjects();
            var go = gos.FirstOrDefault(go => go.GetComponent<CutsceneManager>());
            
            if (!go)
            {
                cam.enabled = true;
                hud.gameObject.SetActive(true);
                fade.enabled = true;

                var op2 = SceneManager.UnloadSceneAsync(cutsceneNames[index]);

                if (op2 == null)
                {
                    soundSystem.StopCutsceneSound();
                    _gameSessionState = GameSessionState.Playing;
                    next?.Invoke();
                    return;
                }

                op2.completed += OnUnloaded;
                return;

                void OnUnloaded(AsyncOperation _)
                {
                    soundSystem.StopCutsceneSound();
                    op2.completed -= OnUnloaded;
                    _gameSessionState = GameSessionState.Playing;
                    next?.Invoke();
                }
            }

            var csManager = go.GetComponent<CutsceneManager>();

            csManager.OnCutsceneFinished += OnCutsceneFinished;
            return;

            void OnCutsceneFinished()
            {
                soundSystem.StopCutsceneSound();
                csManager.OnCutsceneFinished -= OnCutsceneFinished;
                cam.enabled = true;
                hud.gameObject.SetActive(true);
                fade.enabled = true;

                var op2 = SceneManager.UnloadSceneAsync(cutsceneNames[index]);
                if (op2 == null)
                {
                    _gameSessionState = GameSessionState.Playing;
                    next?.Invoke();
                    return;
                }

                op2.completed += OnUnloaded;
                return;

                void OnUnloaded(AsyncOperation _)
                {
                    op2.completed -= OnUnloaded;
                    _gameSessionState = GameSessionState.Playing;
                    next?.Invoke();
                }
            }
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
        _gameSessionState = GameSessionState.Briefing;

        Invoke(nameof(StartPauseMusic), 0.2f);

        hud.RollBriefingMessage(
            "Listen up, Shift Squad! Those pesky CAPSLOCKERS need to know who's in charge. \nDemolish their security, then smash and grab everything in their vault! \nJust... don't die on me. You lot haven't paid your bar tab yet.",
            () =>
            {
                soundSystem.StopPauseMusic();

                hud.ShowRewindMessage("Press R to understand!");

                _gameSessionState = GameSessionState.WaitingForInitInput;
            });
    }

    private void StartPauseMusic()
    {
        soundSystem.PlayPauseMusic();
    }

    private void StartGame()
    {
        _gameSessionState = GameSessionState.Intro;
        hud.HideBriefingPanel();
        hud.HideRewindMessage();
        hud.PlayLogoAnimation(() => StartCoroutine(FadeIn()));
        StartCoroutine(DoIntroMusic(() => BuildNextCharacter(true)));
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
        if (_gameSessionState == GameSessionState.WaitingForInitInput)
        {
            if (inputManager.IsRKeyDown)
            {
                soundSystem.StopPauseMusic();
                StartGame();
            }
        }

        if (_pauseActive && inputManager.IsEscKeyDown)
        {
            OnResumeGame();
            return;
        }

        if (_gameSessionState
                is GameSessionState.ReplayMode
                or GameSessionState.Playing
                or GameSessionState.Intro
            && inputManager.IsEscKeyDown)
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

        if (_gameSessionState is GameSessionState.WaitingForR or GameSessionState.Playing && inputManager.IsRKeyDown)
        {
            pools.ClearAllNotificationPools();
            _tickDriver.StopSim();
            StopCharacterBouncing();
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
                    _timeline.GetCurrentSegment() == maps.Length - 1,
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

    private void SetObjectives(MapDefinition map)
    {
        _hasHackerMission = map._hackerZone.Length > 0;
        _hasDemolitionMission = map._demolitionZone.Length > 0;
    }

    private bool _hasHackerMission;
    private bool _hasDemolitionMission;

    private void UpdateTaskText()
    {
        var tick = _tickDriver.CurrentTick;

        var text = "Your mission:";

        var snapshot = _timeline.GetSnapshot(tick);

        var currentPlayer = snapshot._players.GetLastEntity();

        var moneyLeft = 0;
        var isEvacuationMission = false;
        var minesLeft = 0;
        for (var i = 0; i < snapshot._triggers.GetNumEntities(); i++)
        {
            var trigger = snapshot._triggers.GetEntityAt(i);

            if (trigger._triggerType == TriggerType.Money)
                moneyLeft++;

            if (trigger._triggerType == TriggerType.Exit)
                isEvacuationMission = true;

            if (trigger._triggerType == TriggerType.DemolitionZone)
                minesLeft++;
        }

        if (moneyLeft > 0)
        {
            hud.SetMissionText("Collect cash piles!");
            hud.SetMissionCount(moneyLeft.ToString());
        }
        else if (isEvacuationMission)
        {
            hud.SetMissionText("Stay in the evacuation point!");
            hud.SetMissionCount(string.Empty);
        }
        else if (currentPlayer._type == PlayerType.Demolition && minesLeft > 0)
        {
            hud.SetMissionText("Optional: Defuse mines!");
            hud.SetMissionCount(string.Empty);
        }

        if (!isEvacuationMission && moneyLeft == 0 && minesLeft == 0)
        {
            hud.SetMissionText("Protect your team!");
            hud.SetMissionCount(string.Empty);
        }

        switch (currentPlayer._type)
        {
            case PlayerType.Heavy:
                hud.SetObjectiveText("Enemies left:");
                hud.SetObjectiveCount(snapshot._damageProcess.ToString());
                hud.SetSuccessState(snapshot._damageProcess == 0);

                hud.HideThreatMeter();

                break;

            case PlayerType.Hacker:

                if (_hasHackerMission)
                {
                    hud.SetObjectiveText("Hack the computer:");
                    hud.SetObjectiveCount((Math.Round(snapshot._hackProcess / (float)_tickDriver.TickRate, 1) +
                                           " s left!"));
                    hud.SetSuccessState(snapshot._hackProcess == 0);
                }
                else
                {
                    hud.HideObjectivePanel();
                }

                break;
            case PlayerType.Demolition:
                if (_hasDemolitionMission)
                {
                    hud.SetObjectiveText("TNT left:");
                    hud.SetObjectiveCount(snapshot._bombsLeft.ToString());
                    hud.SetSuccessState(snapshot._bombsLeft == 0);
                }
                else
                {
                    hud.HideObjectivePanel();
                }

                break;
            case PlayerType.Sniper:
                hud.HideObjectivePanel();

                var buttlerActive = false;

                for (var i = 0; i < snapshot._enemies.GetNumEntities(); i++)
                {
                    if (snapshot._enemies.GetEntityAt(i)._type != EnemyType.Buttler)
                        continue;

                    buttlerActive = true;
                    break;
                }

                hud.SetThreatMeterText(
                    buttlerActive ? "WARNING: Enemy Sniper is active!" : "No immediate threat detected!",
                    buttlerActive);

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
        flashingText.SetState(FlashingTextState.Rewinding);
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
        hud.HideRewindMessage();
        SetAllFootsteps(true);
        scriptableRendererData.rendererFeatures[0].SetActive(false);
        flashingText.SetState(FlashingTextState.Recording);
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

        var entityNum = snapshot._players.GetNumEntities() - 1;
        var state = _gameSessionState == GameSessionState.ReplayMode ? 4 : _timeline.GetCurrentSegment();

        var musicIndex = _gameSessionState == GameSessionState.ReplayMode ? _timeline.GetCurrentSegment() : entityNum;

        soundSystem.PlayCharacterMusic(musicIndex, state);

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
