using UnityEngine;

public class Marker : MonoBehaviour
{
    [SerializeField]
    private float amplitude = 0.15f;

    [SerializeField]
    private float speed = 4f;

    [SerializeField]
    private SpriteRenderer visualObject;

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
        visualObject.color = inputColor;
    }

    public void SetText(string text)
    {
        Text = text;
    }

    public void SetPosition(Vector2 position)
    {
        transform.position = position;
    }

    public void SetUsesArrow(bool value)
    {
        UsesArrow = value;
    }

    private float _baseSize = 1.0f;
    
    public string Text { get; private set; }
    public bool UsesArrow { get; private set; }

    private void Update()
    {
        visualObject.transform.localScale = Vector2.one * (_baseSize * (1 + amplitude * Mathf.Sin(Time.time * speed)));
    }
}
