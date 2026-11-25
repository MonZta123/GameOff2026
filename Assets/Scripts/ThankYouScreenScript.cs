using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ThankYouScreenScript : MonoBehaviour
{
    [SerializeField]
    private Image fadeImage;

    [SerializeField]
    private Button button;

    private void Awake()
    {
        fadeImage.color = new Color(0, 0, 0, 1);
        _fadeCoroutine = StartCoroutine(FadeIn());
        button.gameObject.SetActive(true);
    }

    private Coroutine _fadeCoroutine;

    public void PlayAgain()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }
        button.gameObject.SetActive(false);

        _fadeCoroutine = StartCoroutine(FadeOut(() => SceneManager.LoadScene("Scenes/Title", LoadSceneMode.Single)));
    }

    private IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(0.5f);

        var elapsed = 0.0f;
        
        var duration = 1.5f;

        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime / duration;
            yield return null;
            fadeImage.color = Color.Lerp(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), elapsed / duration);

            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);
        _fadeCoroutine = null;
    }

    private IEnumerator FadeOut(Action next = null)
    {
        var elapsed = 0.0f;

        var duration = 1.0f;

        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime / duration;
            yield return null;
            fadeImage.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), elapsed / duration);

            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);
        _fadeCoroutine = null;

        next?.Invoke();
    }
}
