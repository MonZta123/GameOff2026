using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField]
    private GameObject rewindPanel;

    [SerializeField]
    private Volume globalVolume;

    [SerializeField]
    private GameObject ouchScreen;

    [SerializeField]
    private TextMeshProUGUI wastedText;

    [SerializeField]
    private TextMeshProUGUI ouchText;

    [SerializeField]
    private GameObject briefingPanel;

    [SerializeField]
    private GameObject missionPanel;

    [SerializeField]
    private GameObject objectivePanel;

    // [SerializeField]
    // private GameObject targetPanel;

    [SerializeField]
    private GameObject threatMeter;

    [SerializeField]
    private TextMeshProUGUI threatMeterText;

    [SerializeField]
    private TextMeshProUGUI missionText;

    [SerializeField]
    private TextMeshProUGUI rewindText;

    [SerializeField]
    private TextMeshProUGUI missionCount;

    [SerializeField]
    private TextMeshProUGUI objectiveText;

    [SerializeField]
    private TextMeshProUGUI objectiveCount;

    [SerializeField]
    private TextMeshProUGUI briefingText;

    [SerializeField]
    private Image shiftSquadFlame;

    [SerializeField]
    private Image shiftSquadText;

    private const float LogoSidePx = 1200;

    public void InitLogoAnimation()
    {
        shiftSquadFlame.transform.localPosition = new Vector2(LogoSidePx, 0f);
        shiftSquadText.transform.localPosition = new Vector2(-LogoSidePx, 0f);
    }

    public void PlayLogoAnimation(Action fade)
    {
        StartCoroutine(LogoAnimation(fade));
    }

    private IEnumerator LogoAnimation(Action fade)
    {
        var elapsed = 0f;

        var duration = 0.5f;

        yield return new WaitForSeconds(0.2f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            shiftSquadFlame.transform.localPosition =
                Vector2.Lerp(new Vector2(LogoSidePx, 0f), Vector2.zero, elapsed / duration);

            yield return null;
        }

        elapsed = 0;

        yield return new WaitForSeconds(0.2f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            shiftSquadText.transform.localPosition =
                Vector2.Lerp(new Vector2(-LogoSidePx, 0f), Vector2.zero, elapsed / duration);

            yield return null;
        }

        yield return new WaitForSeconds(4.2f);

        fade();
        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            shiftSquadText.transform.localPosition =
                Vector2.Lerp(Vector2.zero, new Vector2(-LogoSidePx, 0f), elapsed / duration);
            shiftSquadFlame.transform.localPosition =
                Vector2.Lerp(Vector2.zero, new Vector2(LogoSidePx, 0f), elapsed / duration);
            yield return null;
        }
    }

    public void SetThreatMeterText(string text, bool isDangerous = false)
    {
        if (!threatMeter.activeInHierarchy)
        {
            threatMeter.SetActive(true);
        }

        if (threatMeterText.text != text)
            threatMeterText.text = text;

        var color = isDangerous ? Color.red : Color.white;
        if (threatMeterText.color != color)
            threatMeterText.color = color;

        if (isDangerous)
        {
            if (_threatMeterCoroutine == null)
                _threatMeterCoroutine = StartCoroutine(DoFlash());
        }
        else
        {
            if (_threatMeterCoroutine != null)
                StopCoroutine(_threatMeterCoroutine);

            _threatMeterCoroutine = null;

            threatMeterText.gameObject.SetActive(true);
        }
    }

    private Coroutine _threatMeterCoroutine;

    private IEnumerator DoFlash()
    {
        yield return null;

        while (threatMeter.activeInHierarchy)
        {
            threatMeterText.gameObject.SetActive(!threatMeterText.gameObject.activeInHierarchy);

            yield return new WaitForSeconds(0.5f);
        }

        threatMeterText.gameObject.SetActive(true);
        _threatMeterCoroutine = null;
    }

    public void HideThreatMeter()
    {
        if (threatMeter.activeInHierarchy)
            threatMeter.SetActive(false);

        if (_threatMeterCoroutine != null)
        {
            StopCoroutine(_threatMeterCoroutine);
            _threatMeterCoroutine = null;
        }

        threatMeterText.gameObject.SetActive(true);
    }

    public void SetMissionText(string text)
    {
        if (!missionPanel.gameObject.activeInHierarchy)
            missionPanel.SetActive(true);

        missionText.text = text;
    }

    public void SetMissionCount(string count)
    {
        if (!missionPanel.gameObject.activeInHierarchy)
            missionPanel.SetActive(true);

        missionCount.text = count;
    }

    public void HideMissionPanel()
    {
        missionPanel.SetActive(false);
    }

    public void HideObjectivePanel()
    {
        objectivePanel.SetActive(false);
    }

    public void SetObjectiveText(string text)
    {
        if (!objectivePanel.gameObject.activeInHierarchy)
            objectivePanel.SetActive(true);

        objectiveText.text = text;
    }

    public void SetObjectiveCount(string count)
    {
        if (!objectivePanel.gameObject.activeInHierarchy)
            objectivePanel.SetActive(true);

        objectiveCount.text = count;
    }

    public void ShowRewindMessage(string text)
    {
        rewindText.text = text;
        rewindPanel.SetActive(true);
    }

    public void HideRewindMessage()
    {
        rewindPanel.SetActive(false);
        ouchScreen.SetActive(false);

        globalVolume.weight = 0f;

        if (_globalVolumeCoroutine != null)
            StopCoroutine(_globalVolumeCoroutine);
        _globalVolumeCoroutine = null;
    }

    public void ShowOuchScreen(string text, Action next = null)
    {
        ouchText.gameObject.SetActive(false);
        wastedText.gameObject.SetActive(false);
        ouchScreen.SetActive(true);

        if (_globalVolumeCoroutine != null)
            StopCoroutine(_globalVolumeCoroutine);

        _globalVolumeCoroutine = StartCoroutine(DoWastedScreen(text, next));
    }

    [SerializeField]
    private float durationLengthWasted = 1f;

    [SerializeField]
    private AnimationCurve durationCurveWasted;

    private IEnumerator DoWastedScreen(string text, Action next = null)
    {
        yield return null;

        var elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime / durationLengthWasted;

            elapsed = Mathf.Clamp01(elapsed);

            globalVolume.weight = Mathf.Lerp(0f, 1f, durationCurveWasted.Evaluate(elapsed));

            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        wastedText.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(1f);

        ouchText.gameObject.SetActive(true);

        var ouchTextText = string.Empty;

        if (!string.IsNullOrWhiteSpace(text))
        {
            ouchTextText = text + "\n";
        }

        ouchText.text = ouchTextText + "Press R to rewind and retry!";

        next?.Invoke();
        _globalVolumeCoroutine = null;
    }

    private Coroutine _globalVolumeCoroutine;
    private Coroutine _briefingCoroutine;

    public void RollBriefingMessage(string text, Action onComplete = null)
    {
        briefingText.text = string.Empty;
        briefingPanel.SetActive(true);

        if (_briefingCoroutine != null)
            StopCoroutine(_briefingCoroutine);

        _briefingCoroutine = StartCoroutine(RollText(text, onComplete));
    }

    private IEnumerator RollText(string text, Action onComplete = null)
    {
        yield return new WaitForSeconds(0.025f);

        var textLength = text.Length;

        var count = 0;

        while (count <= textLength)
        {
            briefingText.text = text[..count];
            count++;
            yield return new WaitForSeconds(0.035f);
        }

        onComplete?.Invoke();
    }

    public void HideBriefingPanel()
    {
        briefingPanel.SetActive(false);
    }

    [SerializeField]
    private Color successColor =
        Color.green;

    [SerializeField]
    private Color normalColor =
        Color.red;

    public void SetSuccessState(bool p0)
    {
        objectiveCount.color = p0 ? successColor : normalColor;
    }
}
