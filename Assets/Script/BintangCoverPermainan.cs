
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class BintangCoverPermainan : MonoBehaviour
{
    [Header("Sprite Bintang")]
    [SerializeField] private Sprite bintangEmas;
    [SerializeField] private Sprite bintangKosong;

    private readonly Image[] bintang = new Image[3];

    private string kunciPenyimpanan;

    private CanvasGroup grupCover;
    private CanvasGroup grupBintang;

    private bool sudahPeringatkanCanvasGroup;

    // ==========================================
    // PERSIAPAN
    // ==========================================

    private void Awake()
    {
        grupBintang = GetComponent<CanvasGroup>();

        if (grupBintang == null)
        {
            grupBintang =
                gameObject.AddComponent<CanvasGroup>();
        }

        grupBintang.ignoreParentGroups = true;
        grupBintang.alpha = 1f;
        grupBintang.interactable = false;
        grupBintang.blocksRaycasts = false;

        CariCanvasGroupCover();
    }

    private void OnEnable()
    {
        PerbaruiBintang();
    }

    // ==========================================
    // MEMBACA CANVAS GROUP COVER
    // ==========================================

    private void CariCanvasGroupCover()
    {
        if (transform.parent == null)
            return;

        grupCover =
            transform.parent.GetComponent<CanvasGroup>();
    }

    // ==========================================
    // MEMBUAT KUNCI OTOMATIS
    // ==========================================

    private string BuatKunciOtomatis()
    {
        if (transform.parent == null)
        {
            Debug.LogError(
                "Baris_Bintang harus menjadi child cover.",
                this
            );

            return null;
        }

        string namaCover = transform.parent.name;

        StringBuilder namaBersih =
            new StringBuilder();

        foreach (char huruf in namaCover)
        {
            if (char.IsLetterOrDigit(huruf))
                namaBersih.Append(huruf);
        }

        return "MathQuest_" +
               namaBersih +
               "_Bintang";
    }

    // ==========================================
    // MENGAMBIL TIGA GAMBAR BINTANG
    // ==========================================

    private bool AmbilGambarBintang()
    {
        for (int i = 0; i < 3; i++)
        {
            string nama = "Bintang_" + (i + 1);

            Transform anak = transform.Find(nama);

            if (anak == null)
            {
                Debug.LogError(
                    nama + " tidak ditemukan.",
                    this
                );

                return false;
            }

            bintang[i] = anak.GetComponent<Image>();

            if (bintang[i] == null)
            {
                Debug.LogError(
                    nama + " tidak memiliki Image.",
                    anak.gameObject
                );

                return false;
            }

            bintang[i].raycastTarget = false;
            bintang[i].preserveAspect = true;
        }

        return true;
    }

    // ==========================================
    // MENAMPILKAN REKOR BINTANG TERTINGGI
    // ==========================================

    public void PerbaruiBintang()
    {
        if (bintangEmas == null ||
            bintangKosong == null)
        {
            Debug.LogWarning(
                "Sprite bintang belum lengkap.",
                this
            );

            return;
        }

        kunciPenyimpanan = BuatKunciOtomatis();

        if (string.IsNullOrEmpty(kunciPenyimpanan))
            return;

        if (!AmbilGambarBintang())
            return;

        // Jika belum bermain, default 0.
        int bintangTertinggi = Mathf.Clamp(
            PlayerPrefs.GetInt(kunciPenyimpanan, 0),
            0,
            3
        );

        for (int i = 0; i < 3; i++)
        {
            bintang[i].sprite =
                i < bintangTertinggi
                ? bintangEmas
                : bintangKosong;

            bintang[i].color = Color.white;
            bintang[i].raycastTarget = false;
            bintang[i].preserveAspect = true;
            bintang[i].enabled = true;
        }
    }

    // ==========================================
    // FADE BINTANG SETIAP FRAME
    // ==========================================

    private void LateUpdate()
    {
        if (grupBintang == null)
            return;

        // Pisahkan pewarisan alpha bintang
        // agar tidak terjadi fade ganda.
        grupBintang.ignoreParentGroups = true;
        grupBintang.alpha = 1f;

        grupBintang.interactable = false;
        grupBintang.blocksRaycasts = false;

        // HorizontalCoverFade membuat
        // CanvasGroup cover saat Start().
        if (grupCover == null)
            CariCanvasGroupCover();

        if (grupCover == null)
        {
            if (!sudahPeringatkanCanvasGroup)
            {
                Debug.LogWarning(
                    "CanvasGroup cover tidak ditemukan. " +
                    "Pastikan HorizontalCoverFade aktif.",
                    this
                );

                sudahPeringatkanCanvasGroup = true;
            }

            return;
        }

        // Baca transparansi cover terbaru.
        float alphaCover =
            Mathf.Clamp01(grupCover.alpha);

        // Terapkan langsung ke ketiga Image.
        for (int i = 0; i < bintang.Length; i++)
        {
            if (bintang[i] == null)
                continue;

            Color warna = bintang[i].color;

            warna.a = alphaCover;

            bintang[i].color = warna;
        }
    }
}
