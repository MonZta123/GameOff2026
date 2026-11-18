using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuScript : MonoBehaviour
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

    public UnityEvent onMenuClose;

    public void ResumeGame()
    {
        onMenuClose?.Invoke();
    }

  
    private void Awake()
    {
        ResetVolume();
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

    public void Quit()
    {
        Time.timeScale = 1f;
        fade.gameObject.SetActive(true);
        StartCoroutine(FadeOut());
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

        AkUnitySoundEngine.StopAll();
        SceneManager.LoadScene("Title");
    }

    public void CloseAll()
    {
        if (blocker)
            blocker.gameObject.SetActive(false);

        if (confirmQuitPanel)
            confirmQuitPanel.gameObject.SetActive(false);

        if (settingsPanel)
            CancelSettingsPanel();
    }
}
