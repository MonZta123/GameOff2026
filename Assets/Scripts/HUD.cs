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
    private TextMeshProUGUI rewindText;

    [SerializeField]
    private TextMeshProUGUI briefingText;

    [SerializeField]
    private Image shiftSquadFlame;

    [SerializeField]
    private Image shiftSquadText;

    private const float LogoSidePx = 1200;

    [SerializeField]
    private ObjectiveScript objectiveScript;

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

    private bool _flash;

    private IEnumerator DoFlash()
    {
        yield return null;

        bool color = true;

        while (_flash)
        {
            color = !color;

            objectiveScript.SetDangerousState(color);

            if (!_flash)
                yield break;

            yield return new WaitForSeconds(0.5f);
        }

        _threatMeterCoroutine = null;
    }

    private Coroutine _threatMeterCoroutine;

    public void FlashThreatMeter()
    {
        if (_flash)
            return;

        _flash = true;
        if (_threatMeterCoroutine != null)
            StopCoroutine(_threatMeterCoroutine);

        _threatMeterCoroutine = StartCoroutine(DoFlash());
    }

    public void StopFlashing()
    {
        if (!_flash)
            return;
        
        _flash = false;

        if (_threatMeterCoroutine != null)
        {
            StopCoroutine(_threatMeterCoroutine);
        }

        objectiveScript.SetDangerousState(false);
        _threatMeterCoroutine = null;
    }

    public void SetMainText(string text)
    {
        objectiveScript.SetMain(text);
    }

    public void SetOptionalText(string text)
    {
        objectiveScript.SetOptional(text);
    }

    public void SetTaskText(string text)
    {
        objectiveScript.SetSub(text);
    }

    public void SetTaskCount(string count)
    {
        objectiveScript.SetSubCount(count);
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


    public void SetSuccessState(bool p0)
    {
        objectiveScript.SetSuccessState(p0);
    }

    public void HideTaskPanel()
    {
        objectiveScript.HideTaskPanel();
    }

    public void ShowTaskPanel()
    {
        objectiveScript.ShowTaskPanel();
    }

    public void SetTimer(string time)
    {
        objectiveScript.SetTime(time);
    }
}
