using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MusicToggleButton : MonoBehaviour
{
    [SerializeField] private GameObject musicOnVisual;
    [SerializeField] private GameObject musicOffVisual;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ToggleMusic);
    }

    private void Start()
    {
        RefreshVisual();
    }

    private void ToggleMusic()
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlayButtonClick();
        AudioManager.Instance.ToggleMusic();

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (AudioManager.Instance == null)
            return;

        bool isOn = AudioManager.Instance.MusicOn;

        if (musicOnVisual != null)
            musicOnVisual.SetActive(isOn);

        if (musicOffVisual != null)
            musicOffVisual.SetActive(!isOn);
    }
}