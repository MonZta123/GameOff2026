using TMPro;
using UnityEngine;

public class MarkerArrow : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro arrowText;

    [SerializeField]
    private SpriteRenderer arrowGameObject;
    
    public void SetText(string text)
    {
        arrowText.text = text;
    }
    
    public void SetVector(Vector2 vector)
    { 
        var zRotation = Mathf.Atan2(vector.y, vector.x);
        
        arrowGameObject.transform.rotation = Quaternion.Euler(0, 0, zRotation * Mathf.Rad2Deg);
    }

    public void SetTextPosition(Vector2 newTextPos)
    {
        arrowText.transform.position = newTextPos;
    }

    public void SetPosition(Vector2 viewPortVec)
    {
        arrowGameObject.transform.position = viewPortVec;
    }

    public void SetColor(Color color)
    {
        arrowGameObject.color = (Color)color;
    }
}
