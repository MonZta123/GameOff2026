using TMPro;
using UnityEngine;

public class TaskTextManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI taskText;
    
    public void SetText(string text)
    {
        taskText.text = text;
    }
}
