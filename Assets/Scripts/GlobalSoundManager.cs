using UnityEngine;

public class GlobalSoundManager : MonoBehaviour
{
    [SerializeField]
    public AK.Wwise.Event[] characterMusic;

    [SerializeField]
    public AK.Wwise.Event scratchSound;

    public void PlayCharacterMusic(int index)
    {
        characterMusic[index].Post(gameObject);
    }
}
