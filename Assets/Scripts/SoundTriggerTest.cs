using UnityEngine;

public class SoundTriggerTest : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event footStepEvent;
    
    [SerializeField]
    private GameObject player;
    
    public void ClickButton()
    {
        footStepEvent.Post(player);
    }
}
