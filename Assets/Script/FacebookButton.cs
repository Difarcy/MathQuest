using System.Collections;
using UnityEngine;

public class FacebookButton : MonoBehaviour
{
    private const string FACEBOOK_URL = "https://web.facebook.com/difarcy/";

    public void BukaFacebook()
    {
        StartCoroutine(BukaFacebookSetelahKlik());
    }

    private IEnumerator BukaFacebookSetelahKlik()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        yield return new WaitForSecondsRealtime(0.12f);

        Application.OpenURL(FACEBOOK_URL);
    }
}