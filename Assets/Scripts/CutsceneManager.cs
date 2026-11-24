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
    private bool isAdditionalLoaded;

    private void Awake()
    {
        fade.enabled = true;
        timeline.stopped += TimeLineFinished;
    }

    private void TimeLineFinished(PlayableDirector director)
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        camAnimator.enabled = false;
        _fadeCoroutine = StartCoroutine(FadeOut(() => SceneManager.LoadScene("GameScene")));
    }
    
    private Coroutine _fadeCoroutine; 
    
    private void Start()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        _fadeCoroutine = StartCoroutine(FadeIn(() =>
        {
            timeline.Play();
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
