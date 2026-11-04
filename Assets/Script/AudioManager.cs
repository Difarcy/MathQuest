using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip buttonClick;

    private const string MUSIC_KEY = "MusicOn";
    private const string SOUND_KEY = "SoundOn";

    public bool MusicOn { get; private set; }
    public bool SoundOn { get; private set; }

    public event Action<bool> MusicChanged;
    public event Action<bool> SoundChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 1 = ON, 0 = OFF
        // Default pertama kali install = ON
        MusicOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        SoundOn = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;

        ApplyMusicState();
    }

    public void ToggleMusic()
    {
        SetMusic(!MusicOn);
    }

    public void SetMusic(bool value)
    {
        MusicOn = value;

        PlayerPrefs.SetInt(MUSIC_KEY, MusicOn ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMusicState();
        MusicChanged?.Invoke(MusicOn);
    }

    public void ToggleSound()
    {
        SetSound(!SoundOn);
    }

    public void SetSound(bool value)
    {
        SoundOn = value;

        PlayerPrefs.SetInt(SOUND_KEY, SoundOn ? 1 : 0);
        PlayerPrefs.Save();

        SoundChanged?.Invoke(SoundOn);
    }

    public void PlayButtonClick()
    {
        if (!SoundOn)
            return;

        if (sfxSource != null && buttonClick != null)
        {
            sfxSource.PlayOneShot(buttonClick);
        }
    }

    private void ApplyMusicState()
    {
        if (bgmSource == null)
            return;

        if (MusicOn)
        {
            bgmSource.UnPause();

            if (!bgmSource.isPlaying)
            {
                bgmSource.Play();
            }
        }
        else
        {
            bgmSource.Pause();
        }
    }
}