using UnityEngine;

public enum FlashingTextState { Recording, Rewinding, Playing }

public class FlashingText : MonoBehaviour
{
    [SerializeField]
    private GameObject redDot;

    [SerializeField]
    private GameObject rewindIcon;

    [SerializeField]
    private GameObject arrowRight;

    [SerializeField]
    private GameObject rec;

    [SerializeField]
    private GameObject rewind;

    [SerializeField]
    private GameObject play;

    private void Update()
    {
        if (((int)Time.realtimeSinceStartup) % 2 == 0)
        {
            if (!redDot.activeInHierarchy)
            {
                redDot.SetActive(true);
                rewindIcon.SetActive(true);
                arrowRight.SetActive(true);
            }
        }
        else
        {
            if (redDot.activeInHierarchy)
            {
                redDot.SetActive(false);
                rewindIcon.SetActive(false);
                arrowRight.SetActive(false);
            }
        }
    }

    public void SetState(FlashingTextState state)
    {
        switch (state)
        {
            case FlashingTextState.Recording:
                rec.SetActive(true);
                rewind.SetActive(false);
                play.SetActive(false);
                break;

            case FlashingTextState.Rewinding:
                rec.SetActive(false);
                rewind.SetActive(true);
                play.SetActive(false);
                break;

            case FlashingTextState.Playing:
                rec.SetActive(false);
                rewind.SetActive(false);
                play.SetActive(true);
                break;
        }
    }
}
