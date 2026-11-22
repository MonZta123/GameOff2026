using System.Collections;
using TMPro;
using UnityEngine;

public class CreditText : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI role;

    [SerializeField]
    private TextMeshProUGUI namee;

    public void SetText(string creditName, string creditRole)
    {
        role.text = creditRole;
        namee.text = creditName;
    }

    private Coroutine _fadeCoroutine;

    public void FadeIn(float fadeTime)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(DoFadeIn(fadeTime));
    }

    private IEnumerator DoFadeIn(float fadeTime)
    {
        yield return null;

        var elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;

            var alpha = Mathf.Lerp(0f, 1f, elapsed / fadeTime);

            role.color = new Color(role.color.r, role.color.g, role.color.b, alpha);
            namee.color = new Color(namee.color.r, namee.color.g, namee.color.b, alpha);

            yield return null;
        }

        role.color = new Color(role.color.r, role.color.g, role.color.b, 1.0f);
        namee.color = new Color(namee.color.r, namee.color.g, namee.color.b, 1.0f);
    }

    public void Hide()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
        
        _fadeCoroutine = null;
        role.color = new Color(role.color.r, role.color.g, role.color.b, 0);
        namee.color = new Color(namee.color.r, namee.color.g, namee.color.b, 0);
    }
    
    public void FadeOut(float fadeTime)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(DoFadeOut(fadeTime));
    }

    private IEnumerator DoFadeOut(float fadeTime)
    {
        yield return null;

        var elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;

            var alpha = Mathf.Lerp(1.0f, 0.0f, elapsed / fadeTime);

            role.color = new Color(role.color.r, role.color.g, role.color.b, alpha);
            namee.color = new Color(namee.color.r, namee.color.g, namee.color.b, alpha);

            yield return null;
        }

        role.color = new Color(role.color.r, role.color.g, role.color.b, 0.0f);
        namee.color = new Color(namee.color.r, namee.color.g, namee.color.b, 0.0f);
    }
}
