using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    //public static MenuManager Instance { get; private set; }
    
    [SerializeField]
    private GameObject pauseMenuPanel;

    [SerializeField]
    private GameObject settingsMenuPanel;

    [SerializeField]
    private GameObject confirmQuitPanel;

    [SerializeField]
    private Slider masterVolumeSlider;

    [SerializeField]
    private Slider musicVolumeSlider;

    [SerializeField]
    private Slider sfxVolumeSlider;

    [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip buttonClip;

    [SerializeField]
    private AudioClip sliderClip;

    public bool PauseIsActive => pauseMenuPanel.activeSelf;

    private float _masterVolume;
    private float _musicVolume;
    private float _sfxVolume;


    private void Awake()
    {
        //Instance = this;
    }

    public void ShowPauseMenu()
    {
        print("openingpausemenu");
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void PlayAudioButton()
    {
        audioSource.PlayOneShot(buttonClip);
    }

    private void PlayAudioSlider()
    {
        audioSource.PlayOneShot(sliderClip);
    }

    public void ContinueGame()
    {
        PlayAudioButton();
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SettingsClicked()
    {
        PlayAudioButton();
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        audioMixer.GetFloat("master", out var db);
        masterVolumeSlider.value = Mathf.Pow(10f, db / 20f);

        audioMixer.GetFloat("music", out var db1);
        musicVolumeSlider.value = Mathf.Pow(10f, db1 / 20f);

        audioMixer.GetFloat("sfx", out var db2);
        sfxVolumeSlider.value = Mathf.Pow(10f, db2 / 20f);

        _masterVolume = masterVolumeSlider.value;
        _musicVolume = musicVolumeSlider.value;
        _sfxVolume = sfxVolumeSlider.value;
    }

    public void SetMasterVolume(float volume)
    {
        _masterVolume = volume;
        audioMixer.SetFloat("master", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = volume;
        audioMixer.SetFloat("music", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = volume;
        audioMixer.SetFloat("sfx", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
    }

    public void CancelSettingsClicked()
    {
        PlayAudioButton();
        pauseMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        var instance = Settings.Instance;

        audioMixer.SetFloat("master", Mathf.Log10(Mathf.Max(instance.MasterVolume, 0.0001f)) * 20f);
        audioMixer.SetFloat("music", Mathf.Log10(Mathf.Max(instance.MusicVolume, 0.0001f)) * 20f);
        audioMixer.SetFloat("sfx", Mathf.Log10(Mathf.Max(instance.SfxVolume, 0.0001f)) * 20f);

        _masterVolume = 0;
        _musicVolume = 0;
        _sfxVolume = 0;
    }

    public void QuitClicked()
    {
        PlayAudioButton();
        pauseMenuPanel.SetActive(false);
        confirmQuitPanel.SetActive(true);
    }

    public void ConfirmQuitClicked()
    {
        PlayAudioButton();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void CancelQuitClicked()
    {
        PlayAudioButton();
        pauseMenuPanel.SetActive(true);
        confirmQuitPanel.SetActive(false);
    }
}
