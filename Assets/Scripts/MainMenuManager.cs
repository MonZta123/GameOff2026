using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static readonly int s_unscaledTime = Shader.PropertyToID("_UnscaledTime");

    private void Update()
    {
        Shader.SetGlobalFloat(s_unscaledTime, Time.unscaledTime);
    }

    [SerializeField]
    private Image fade;

    [SerializeField]
    private Image blocker;

    [SerializeField]
    private GameObject confirmQuitPanel;

    [SerializeField]
    private GameObject settingsPanel;

    [SerializeField]
    private GameObject creditsPanel;

    [SerializeField]
    private GameObject howToPlay;

    [SerializeField]
    private Slider masterVolumeSlider;

    [SerializeField]
    private Slider musicVolumeSlider;

    [SerializeField]
    private Slider sfxVolumeSlider;

    [SerializeField]
    private Slider voiceVolumeSlider;

    [SerializeField]
    private AK.Wwise.RTPC masterVolumeRtpc;

    [SerializeField]
    private AK.Wwise.RTPC musicVolumeRtpc;

    [SerializeField]
    private AK.Wwise.RTPC sfxVolumeRtpc;

    [SerializeField]
    private AK.Wwise.RTPC voiceVolumeRtpc;

    [SerializeField]
    private AK.Wwise.Event startMainMenuMusic;

    [SerializeField]
    private AK.Wwise.Event stopMainMenuMusic;

    [SerializeField]
    private AK.Wwise.Event startEnvironmentMusic;

    [SerializeField]
    private AK.Wwise.Event stopEnvironmentMusic;

    public void ShowHowToPlay()
    {
        blocker.gameObject.SetActive(true);
        howToPlay.gameObject.SetActive(true);

        howToPlay.transform.GetChild(0).gameObject.SetActive(true);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void ForceScreenResolution()
    {
        var standardResolution = 16f / 9f;
        
        var width = (float)Display.main.systemWidth;
        var height = (float)Display.main.systemHeight;

        var isBroad = (width / height) > standardResolution;

        if (isBroad)
        {
            var newWidth = height * standardResolution;
            Screen.SetResolution((int)newWidth, (int)height, FullScreenMode.FullScreenWindow);
        }
        else
        {
            var newHeight = width / standardResolution;
            Screen.SetResolution((int)width, (int)newHeight, FullScreenMode.FullScreenWindow);
        }
    }
    
    public void NextStep()
    { 
        for (var i = 0; i < howToPlay.transform.childCount; i++)
        {
            var child = howToPlay.transform.GetChild(i);
            
            if (child.gameObject.activeInHierarchy)
            {
                child.gameObject.SetActive(false);

                if (i < howToPlay.transform.childCount - 1)
                    howToPlay.transform.GetChild(i + 1).gameObject.SetActive(true);
                else
                    HideHowToPlayScreen();

                return;
            }
        }
    }

    private void HideHowToPlayScreen()
    {
        blocker.gameObject.SetActive(false);
        howToPlay.gameObject.SetActive(false);
    }

    private void Awake()
    {
        ResetVolume();

        StartCoroutine(FadeIn());
    }

    private void Start()
    {
        StartCoroutine(Music());
    }

    private IEnumerator Music()
    {
        yield return new WaitForSeconds(0.2f);

        startMainMenuMusic.Post(gameObject);
        startEnvironmentMusic.Post(gameObject);
    }

    private void ResetVolume()
    {
        var masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        var musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        var sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
        var voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);

        masterVolumeSlider.value = masterVolume;
        musicVolumeSlider.value = musicVolume;
        sfxVolumeSlider.value = sfxVolume;
        voiceVolumeSlider.value = voiceVolume;
    }

    private void StopMusic()
    {
        stopMainMenuMusic.Post(gameObject);
        stopEnvironmentMusic.Post(gameObject);
    }

    public void StartGame()
    {
        fade.gameObject.SetActive(true);
        StartCoroutine(FadeOut());
        StopMusic();
    }

    private IEnumerator FadeIn()
    {
        yield return null;

        fade.color = new Color(0, 0, 0, 1);

        var elapsed = 0f;

        var duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            fade.color = new Color(0, 0, 0, 1 - elapsed / duration);

            yield return null;
        }

        fade.color = new Color(0, 0, 0, 0);

        fade.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
        yield return null;

        fade.color = new Color(0, 0, 0, 0);

        var elapsed = 0f;

        var duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            fade.color = new Color(0, 0, 0, elapsed / duration);

            yield return null;
        }

        SceneManager.LoadScene("OpeningCutScene");
    }

    public void ShowCreditsPanel()
    {
        blocker.gameObject.SetActive(true);
        creditsPanel.gameObject.SetActive(true);
    }

    public void CloseCreditsPanel()
    {
        blocker.gameObject.SetActive(false);
        creditsPanel.gameObject.SetActive(false);
    }

    public void ShowSettingsPanel()
    {
        blocker.gameObject.SetActive(true);
        settingsPanel.gameObject.SetActive(true);
    }

    public void CancelSettingsPanel()
    {
        blocker.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(false);

        ResetVolume();
    }

    public void ConfirmSettingsPanel()
    {
        blocker.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(false);

        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetFloat("VoiceVolume", voiceVolumeSlider.value);
    }

    public void OpenQuitPanel()
    {
        blocker.gameObject.SetActive(true);
        confirmQuitPanel.gameObject.SetActive(true);
    }

    public void CloseQuitPanel()
    {
        blocker.gameObject.SetActive(false);
        confirmQuitPanel.gameObject.SetActive(false);
    }

    public void OnMasterChanged(float value)
    {
        masterVolumeRtpc.SetValue(null, value * 100f);
    }

    public void OnMusicChanged(float value)
    {
        musicVolumeRtpc.SetValue(null, value * 100f);
    }

    public void OnSfxChanged(float value)
    {
        sfxVolumeRtpc.SetValue(null, value * 100f);
    }

    public void OnVoiceChanged(float value)
    {
        voiceVolumeRtpc.SetValue(null, value * 100f);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
