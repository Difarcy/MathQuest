using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class EmailButton : MonoBehaviour
{
    private const string EMAIL = "difarcy@gmail.com";

    public void BukaEmail()
    {
        Debug.Log("EMAIL BERHASIL DIKLIK");
        StartCoroutine(BukaEmailSetelahKlik());
    }

    private IEnumerator BukaEmailSetelahKlik()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        yield return new WaitForSecondsRealtime(0.12f);

        string subject = "Kontak MathQuest";
        string body = "";

#if UNITY_ANDROID && !UNITY_EDITOR

        // =============================
        // ANDROID → BUKA GMAIL APP
        // =============================

        string mailto =
            "mailto:" + EMAIL +
            "?subject=" + UnityWebRequest.EscapeURL(subject) +
            "&body=" + UnityWebRequest.EscapeURL(body);

        try
        {
            AndroidJavaClass intentClass =
                new AndroidJavaClass("android.content.Intent");

            AndroidJavaClass uriClass =
                new AndroidJavaClass("android.net.Uri");

            AndroidJavaObject uri =
                uriClass.CallStatic<AndroidJavaObject>("parse", mailto);

            AndroidJavaObject intent =
                new AndroidJavaObject(
                    "android.content.Intent",
                    intentClass.GetStatic<string>("ACTION_SENDTO"),
                    uri
                );

            // Buka Gmail
            intent.Call<AndroidJavaObject>(
                "setPackage",
                "com.google.android.gm"
            );

            AndroidJavaClass unityPlayer =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer");

            AndroidJavaObject activity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("startActivity", intent);
        }
        catch
        {
            // Gmail tidak terpasang → aplikasi email default
            Application.OpenURL(mailto);
        }

#else

        // =============================
        // UNITY EDITOR / WINDOWS
        // → BUKA GMAIL WEB COMPOSE
        // =============================

        string gmailURL =
            "https://mail.google.com/mail/?view=cm&fs=1" +
            "&to=" + UnityWebRequest.EscapeURL(EMAIL) +
            "&su=" + UnityWebRequest.EscapeURL(subject) +
            "&body=" + UnityWebRequest.EscapeURL(body);

        Application.OpenURL(gmailURL);

#endif
    }
}