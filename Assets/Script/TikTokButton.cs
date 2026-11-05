using System.Collections;
using UnityEngine;

public class TikTokButton : MonoBehaviour
{
    private const string TIKTOK_URL =
        "https://www.tiktok.com/@difarcy";

    public void BukaTikTok()
    {
        StartCoroutine(BukaTikTokSetelahKlik());
    }

    private IEnumerator BukaTikTokSetelahKlik()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        yield return new WaitForSecondsRealtime(0.12f);

        Application.OpenURL(TIKTOK_URL);
    }
}