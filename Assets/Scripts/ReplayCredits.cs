using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct CreditSlot
{
    public string _name;
    public string _role;
    public Sprite _image;
}

public class ReplayCredits : MonoBehaviour
{
    [SerializeField]
    private GameObject[] replaySpots;

    [SerializeField]
    private CreditSlot[] credits;

    [SerializeField]
    private Canvas target;

    [SerializeField]
    private float visibleDuration = 4f;

    [SerializeField]
    private float waitBeginning = 1f;

    [SerializeField]
    private float endBeforeSeconds = 2f;

    [SerializeField]
    private CreditText creditTextPrefab;

    private CreditText _creditText;

    private bool _isRunning;

    private void Awake()
    {
        _creditText = Instantiate(creditTextPrefab, target.transform);

        _creditText.gameObject.SetActive(false);
    }

    public void RunCredits(float totalSeconds)
    {
        var creditsShowTime = totalSeconds - endBeforeSeconds - waitBeginning;

        var visibleTimeTotal = credits.Length * visibleDuration;

        var nonVisibleTime = creditsShowTime - visibleTimeTotal;

        var timeBetweenCredit = nonVisibleTime / (credits.Length - 1);
        Resume();

        if (_creditsCoroutine != null)
        {
            StopCoroutine(_creditsCoroutine);
        }

        _creditText.Hide();
        _creditsCoroutine = StartCoroutine(DoRunCredits(timeBetweenCredit));
    }

    private Coroutine _creditsCoroutine;

    private IEnumerator DoRunCredits(float timeBetweenCredit)
    {
        yield return new WaitForSeconds(waitBeginning);

        const float FadeTime = 1.0f;

        for (var i = 0; i < credits.Length; i++)
        {
            while (!_isRunning)
                yield return null;

            var spot = replaySpots[Random.Range(0, replaySpots.Length)];
            var credit = credits[i];

            _creditText.transform.position = spot.transform.position;
            _creditText.transform.rotation = spot.transform.rotation;

            _creditText.gameObject.SetActive(true);
            _creditText.SetText(credit._name, credit._role, credit._image);

            _creditText.FadeIn(FadeTime);

            yield return new WaitForSeconds(visibleDuration);

            _creditText.FadeOut(FadeTime);

            if (i < credits.Length - 1)
                yield return new WaitForSeconds(timeBetweenCredit);
        }

        _creditsCoroutine = null;
    }

    public void Pause()
    {
        _isRunning = false;
    }

    public void Resume()
    {
        _isRunning = true;
    }
}
