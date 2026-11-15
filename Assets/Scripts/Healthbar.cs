using System.Collections;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer backdrop;

    [SerializeField]
    private SpriteRenderer animationRed;

    [SerializeField]
    private SpriteRenderer health;

    [SerializeField]
    private float unitsWidth;

    private float _healthPercent;

    private void Awake()
    {
        _healthPercent = 100;
        backdrop.size = new Vector2(unitsWidth, backdrop.size.y);
        health.size = new Vector2(unitsWidth, health.size.y);
        animationRed.size = new Vector2(unitsWidth, animationRed.size.y);

        backdrop.transform.localPosition = new Vector2(-unitsWidth * 0.5f, backdrop.transform.localPosition.y);
        health.transform.localPosition = new Vector2(-unitsWidth * 0.5f, health.transform.localPosition.y);
        animationRed.transform.localPosition = new Vector2(-unitsWidth * 0.5f, animationRed.transform.localPosition.y);
    }

    private Coroutine _scrollDown;

    private float _animationHealthPercent;

    public void SetHealthPercent(float healthPercent)
    {
        healthPercent = Mathf.Clamp(healthPercent, 0, 100);

        if (_healthPercent > healthPercent)
        {
            health.size = new Vector2(unitsWidth * healthPercent / 100, health.size.y);

            if (_scrollDown != null)
                StopCoroutine(_scrollDown);

            _scrollDown = StartCoroutine(ScrollDown());
        }
        else
        {
            health.size = new Vector2(unitsWidth * healthPercent / 100, health.size.y);
        }

        _healthPercent = healthPercent;
    }

    private IEnumerator ScrollDown()
    {
        var startValue = animationRed.size;
        yield return new WaitForSeconds(0.5f);

        var elapsed = 0f;

        var duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            animationRed.size = Vector2.Lerp(startValue, health.size, elapsed / duration);

            yield return null;
        }

        animationRed.size = health.size;
        _scrollDown = null;
    }

    public void ReturnToFullHealth()
    {
        SetHealthPercent(100);
    }
}
