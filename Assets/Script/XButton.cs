using System.Collections;
using UnityEngine;

public class XButton : MonoBehaviour
{
    private const string X_URL = "https://x.com/difarcy";

    public void BukaX()
    {
        StartCoroutine(BukaXSetelahKlik());
    }

    private IEnumerator BukaXSetelahKlik()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        yield return new WaitForSecondsRealtime(0.12f);

        Application.OpenURL(X_URL);
    }
}