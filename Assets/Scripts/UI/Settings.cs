using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings Instance { get; set; }

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        DontDestroyOnLoad(gameObject);
    }
    
    public float MusicVolume { get; set; } = 1f;
    public float SfxVolume { get; set; } = 1f;
    public float MasterVolume { get; set; } = 1f;
}