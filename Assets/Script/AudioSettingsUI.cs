using UnityEngine;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private GameObject musicOnIcon;
    [SerializeField] private GameObject musicOffIcon;

    [Header("Sound")]
    [SerializeField] private GameObject soundOnIcon;
    [SerializeField] private GameObject soundOffIcon;

    private void Start()
    {
        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void ToggleMusic()
    {
        if (AudioManager.Instance == null)
            return;

        // Bunyi klik menggunakan kondisi Sound saat ini
        AudioManager.Instance.PlayButtonClick();

        // Ubah status musik GLOBAL
        AudioManager.Instance.ToggleMusic();

        // Perbarui ikon scene yang sedang dibuka
        RefreshUI();
    }

    public void ToggleSound()
    {
        if (AudioManager.Instance == null)
            return;

        if (AudioManager.Instance.SoundOn)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.SetSound(false);
        }
        else
        {
            AudioManager.Instance.SetSound(true);
            AudioManager.Instance.PlayButtonClick();
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (AudioManager.Instance == null)
            return;

        bool musicOn = AudioManager.Instance.MusicOn;
        bool soundOn = AudioManager.Instance.SoundOn;

        if (musicOnIcon != null)
            musicOnIcon.SetActive(musicOn);

        if (musicOffIcon != null)
            musicOffIcon.SetActive(!musicOn);

        if (soundOnIcon != null)
            soundOnIcon.SetActive(soundOn);

        if (soundOffIcon != null)
            soundOffIcon.SetActive(!soundOn);
    }
}