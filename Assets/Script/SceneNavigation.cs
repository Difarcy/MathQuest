using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneNavigation : MonoBehaviour
{
    public void BukaScene(string sceneName)
    {
        PlayClick();
        SceneManager.LoadScene(sceneName);
    }

    public void BukaMainMenu()
    {
        PlayClick();
        SceneManager.LoadScene("MainMenu");
    }

    public void BukaPengaturan()
    {
        PlayClick();
        SceneManager.LoadScene("Pengaturan");
    }

    public void KeluarGame()
    {
        StartCoroutine(KeluarSetelahSound());
    }

    private IEnumerator KeluarSetelahSound()
    {
        PlayClick();

        yield return new WaitForSecondsRealtime(0.15f);

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}