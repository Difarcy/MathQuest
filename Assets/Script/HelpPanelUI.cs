using UnityEngine;

public class HelpPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelPetunjuk;

    private void Start()
    {
        if (panelPetunjuk != null)
            panelPetunjuk.SetActive(false);
    }

    public void TogglePetunjuk()
    {
        PlayClick();

        if (panelPetunjuk != null)
        {
            bool sedangTerbuka = panelPetunjuk.activeSelf;
            panelPetunjuk.SetActive(!sedangTerbuka);
        }
    }

    public void TutupPetunjuk()
    {
        PlayClick();

        if (panelPetunjuk != null)
            panelPetunjuk.SetActive(false);
    }

    private void PlayClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
}