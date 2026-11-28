using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector timeline;

    [SerializeField]
    private Image fade;

    [SerializeField]
    private Animator camAnimator;

    [SerializeField]
    private AK.Wwise.Event onCutsceneStart;

    public Action OnCutsceneFinished;

    public void PlayCutsceneSound()
    {
        onCutsceneStart.Post(gameObject);
    }

    private void Awake()
    {
        fade.enabled = true;
        timeline.stopped += TimeLineFinished;
        if (camAnimator)
            camAnimator.enabled = false;

        Invoke(nameof(PlayCutsceneSound), 0.2f);
    }

    private void TimeLineFinished(PlayableDirector director)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        if (camAnimator)
            camAnimator.enabled = false;

        if (OnCutsceneFinished != null)
        {
            _fadeCoroutine = StartCoroutine(FadeOut(() => OnCutsceneFinished?.Invoke()));
        }
        else
        {
            _fadeCoroutine = StartCoroutine(FadeOut(() => SceneManager.LoadScene("GameScene")));
        }
    }

    private Coroutine _fadeCoroutine;

    private void Start()
    {
        timeline.time = 0;
        timeline.Evaluate();

        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(FadeIn(() =>
        {
            timeline.Play();
            if (camAnimator)
                camAnimator.enabled = true;
        }));
    }

    private IEnumerator FadeOut(Action next = null)
    {
        fade.enabled = true;

        yield return null;

        fade.color = new Color(0, 0, 0, 0);

        var elapsed = 0f;

        var duration = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            fade.color = new Color(0, 0, 0, elapsed / duration);

            yield return null;
        }

        next?.Invoke();
    }

    private void OnDestroy()
    {
        timeline.stopped -= TimeLineFinished;
    }

    private IEnumerator FadeIn(Action next = null)
    {
        fade.color = new Color(0, 0, 0, 1);

        yield return new WaitForSeconds(0.5f);

        var elapsed = 0f;

        var duration = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            fade.color = new Color(0, 0, 0, 1 - elapsed / duration);

            yield return null;
        }

        fade.color = new Color(0, 0, 0, 0);

        fade.enabled = false;

        next?.Invoke();
    }
}
