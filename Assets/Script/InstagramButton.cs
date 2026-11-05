using System.Collections;
using UnityEngine;

public class InstagramButton : MonoBehaviour
{
    private const string INSTAGRAM_URL =
        "https://www.instagram.com/difarcy/";

    public void BukaInstagram()
    {
        StartCoroutine(BukaInstagramSetelahKlik());
    }

    private IEnumerator BukaInstagramSetelahKlik()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        yield return new WaitForSecondsRealtime(0.12f);

        Application.OpenURL(INSTAGRAM_URL);
    }
}