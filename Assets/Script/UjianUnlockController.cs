
using UnityEngine;
using UnityEngine.UI;

// Pasang pada UjianUnlockManager di MenuPilihan.
public class UjianUnlockController : MonoBehaviour
{
    [Header("Dua gambar di bawah BTN_Ujian")]
    [SerializeField]
    private GameObject imageUjianTerkunci;

    [SerializeField]
    private GameObject imageUjianTerbuka;

    [SerializeField]
    private Button tombolUjian;

    private const string KunciUnlock =
        "MathQuest_Ujian_Unlocked";

    private void OnEnable()
    {
        PerbaruiTampilan();
    }

    public void PerbaruiTampilan()
    {
        bool terbuka =
            PlayerPrefs.GetInt(KunciUnlock, 0) == 1;

        if (imageUjianTerkunci != null)
            imageUjianTerkunci.SetActive(!terbuka);

        if (imageUjianTerbuka != null)
            imageUjianTerbuka.SetActive(terbuka);

        if (tombolUjian != null)
            tombolUjian.interactable = terbuka;
    }
}
