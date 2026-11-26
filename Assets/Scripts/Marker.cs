using TMPro;
using UnityEngine;

public class Marker : MonoBehaviour
{
    [SerializeField]
    private float amplitude = 0.15f;

    [SerializeField]
    private float speed = 4f;

    [SerializeField]
    private float amplitudeBubble = 0.25f;

    [SerializeField]
    private float speedBubble = 2f;

    [SerializeField]
    private SpriteRenderer visualObject;

    [SerializeField]
    private SpriteRenderer bubble;

    [SerializeField]
    private TextMeshPro bubbleText;

    private void OnEnable()
    {
        MarkerManager.Markers.Add(this);
    }

    private void OnDisable()
    {
        MarkerManager.Markers.Remove(this);
    }

    public void SetBaseSize(float size, Color inputColor)
    {
        _baseSize = size;
        _bubbleHeight = size * 0.5f;
        visualObject.color = inputColor;
        MarkerColor = inputColor;
    }
     
    public void SetColor(Color inputColor)
    {
        visualObject.color = inputColor;
        MarkerColor = inputColor;
    }

    private float _bubbleHeight;

    public Color MarkerColor { get; private set; }

    public void SetText(string text, string bText = null)
    {
        Text = text;
        if (bText == null)
        {
            bubble.gameObject.SetActive(false);
            bubbleText.text = string.Empty;
        }
        else
        {
            bubble.gameObject.SetActive(true);
            bubbleText.text = bText;
        }
    }

    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }

    public void SetUsesArrow(bool value)
    {
        UsesArrow = value;
        bubble.gameObject.SetActive(value);
    }

    private float _baseSize = 1.0f;

    public string Text { get; private set; }
    public bool UsesArrow { get; private set; }

    private void Update()
    {
        visualObject.transform.localScale = Vector2.one * (_baseSize * (1 + amplitude * Mathf.Sin(Time.time * speed)));

        var bubbleHeight = Vector2.up * (amplitudeBubble * Mathf.Sin(Time.time * speedBubble + 1f));
        bubbleHeight.y = Mathf.RoundToInt(bubbleHeight.y * 16f) / 16f + _bubbleHeight;

        bubble.transform.localPosition = bubbleHeight;
    }
}
