using TMPro;
using UnityEngine;

public class ObjectiveScript : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI mainText;

    [SerializeField]
    private TextMeshProUGUI subText;

    [SerializeField]
    private TextMeshProUGUI optionalText;

    [SerializeField]
    private TextMeshProUGUI timeText;

    [SerializeField]
    private TextMeshProUGUI subCount;

    [SerializeField]
    private GameObject taskPanel;

    [SerializeField]
    private Transform mainTransform;

    [SerializeField]
    private float amplitudeDegrees = 4f;

    [SerializeField]
    private float speed = 0.75f;

    [SerializeField]
    private float pulseSpeed = 0.6f;

    [SerializeField]
    private float pulseStrength = 0.1f;

    public void SetMain(string text)
    {
        mainText.text = text;
    }

    public void SetTime(string text)
    {
        timeText.text = text;
    }

    public void SetSubCount(string count)
    {
        subCount.text = count;
    }

    public void SetSub(string text)
    {
        subText.text = text;
    }

    public void SetOptional(string text)
    {
        optionalText.text = text;
    }

    [SerializeField]
    private Color successColor =
        Color.green;

    [SerializeField]
    private Color normalColor =
        Color.white;

    public void SetSuccessState(bool p0)
    {
        subText.color = p0 ? successColor : normalColor;
        subCount.color = p0 ? successColor : normalColor;
    }

    public void SetDangerousState(bool p0)
    {
        mainText.color = p0 ? Color.red : normalColor;
    }

    private void Update()
    {
        var angle = Mathf.Sin(Time.unscaledTime * speed * 2f * Mathf.PI) * amplitudeDegrees;

        mainTransform.localRotation = Quaternion.Euler(0, 0, angle);
        mainTransform.localScale = Vector3.one * (1 + Mathf.Sin(Time.unscaledTime * pulseSpeed * 2f * Mathf.PI) * pulseStrength);
    }

    public void HideTaskPanel()
    {
        if (taskPanel.gameObject.activeInHierarchy)
            taskPanel.gameObject.SetActive(false);
    }

    public void ShowTaskPanel()
    {
        if (!taskPanel.gameObject.activeInHierarchy)
            taskPanel.gameObject.SetActive(true);
    }
}
