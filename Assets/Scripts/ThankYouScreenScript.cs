using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ThankYouScreenScript : MonoBehaviour
{
    [SerializeField]
    private Image fadeImage;

    private void Awake()
    {
        fadeImage.color = new Color(0, 0, 0, 1);
        _fadeCoroutine = StartCoroutine(FadeOut());
    }

    private Coroutine _fadeCoroutine;

    public void PlayAgain()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        SceneManager.LoadScene("Scenes/Title");
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.5f);

        var elapsed = 0.0f;

        while (elapsed < 1.5f)
        {
            yield return null;
            elapsed += Time.deltaTime;
            yield return null;
            fadeImage.color = Color.Lerp(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), elapsed);

            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);
        _fadeCoroutine = null;
    }
}
