
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DataJudulMateri
{
    [Min(1)]
    public int mulaiPage = 1;

    public Sprite gambarJudul;
}

public class MateriPageController : MonoBehaviour
{
    [Header("Halaman Materi")]
    [SerializeField] private GameObject[] pages;

    [Header("Root yang Digeser")]
    [SerializeField] private RectTransform slideRoot;

    [Header("Pergantian Judul Materi")]
    [SerializeField] private Image judulPanel;
    [SerializeField] private DataJudulMateri[] daftarJudul;

    [Header("Tombol Navigasi")]
    [SerializeField] private Button btnSebelumnya;
    [SerializeField] private Button btnSelanjutnya;

    [Header("Nomor Halaman")]
    [SerializeField] private TMP_Text txtNomorHalaman;

    [Header("Animasi Slide")]
    [SerializeField] private float slideDistance = 850f;
    [SerializeField] private float duration = 0.32f;

    private int currentPage = 0;
    private bool isAnimating = false;

    private Vector2 centerPosition;
    private CanvasGroup slideCanvasGroup;

    private void Start()
    {
        // Periksa referensi utama
        if (slideRoot == null)
        {
            Debug.LogError(
                "MateriPageController: Slide Root belum diisi!",
                this
            );
            enabled = false;
            return;
        }

        if (pages == null || pages.Length == 0)
        {
            Debug.LogError(
                "MateriPageController: Pages masih kosong!",
                this
            );
            enabled = false;
            return;
        }

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] == null)
            {
                Debug.LogError(
                    $"MateriPageController: Element {i} kosong!",
                    this
                );
                enabled = false;
                return;
            }
        }

        // Simpan posisi awal seluruh panel
        centerPosition = slideRoot.anchoredPosition;

        slideCanvasGroup =
            slideRoot.GetComponent<CanvasGroup>();

        if (slideCanvasGroup == null)
        {
            slideCanvasGroup =
                slideRoot.gameObject.AddComponent<CanvasGroup>();
        }

        currentPage = 0;
        isAnimating = false;

        // Hanya halaman pertama yang aktif
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == currentPage);
        }

        slideRoot.anchoredPosition = centerPosition;
        slideCanvasGroup.alpha = 1f;

        // Atur judul, nomor halaman, dan tombol
        UpdateJudul();
        UpdateNomorHalaman();
        UpdateButtons();
    }

    // ===================================
    // PERGANTIAN JUDUL OTOMATIS
    // ===================================

    private void UpdateJudul()
    {
        if (judulPanel == null || daftarJudul == null)
            return;

        int nomorPage = currentPage + 1;

        int mulaiTerpilih = 0;
        Sprite spriteTerpilih = null;

        foreach (DataJudulMateri data in daftarJudul)
        {
            if (data == null || data.gambarJudul == null)
                continue;

            if (data.mulaiPage <= nomorPage &&
                data.mulaiPage > mulaiTerpilih)
            {
                mulaiTerpilih = data.mulaiPage;
                spriteTerpilih = data.gambarJudul;
            }
        }

        if (spriteTerpilih != null)
        {
            judulPanel.sprite = spriteTerpilih;
        }
    }

    // ===================================
    // NOMOR HALAMAN OTOMATIS
    // ===================================

    private void UpdateNomorHalaman()
    {
        if (txtNomorHalaman == null)
            return;

        txtNomorHalaman.text =
            $"{currentPage + 1}/{pages.Length}";
    }

    // ===================================
    // NAVIGASI HALAMAN
    // ===================================

    public void NextPage()
    {
        if (isAnimating || pages == null)
            return;

        if (currentPage >= pages.Length - 1)
            return;

        StartCoroutine(ChangePage(currentPage + 1, -1));
    }

    public void PreviousPage()
    {
        if (isAnimating || pages == null)
            return;

        if (currentPage <= 0)
            return;

        StartCoroutine(ChangePage(currentPage - 1, 1));
    }

    // ===================================
    // ANIMASI PERGANTIAN HALAMAN
    // ===================================

    private IEnumerator ChangePage(int newPage, int direction)
    {
        isAnimating = true;

        SetNavigationVisible(false);

        Vector2 keluar =
            centerPosition +
            Vector2.right * slideDistance * direction;

        // Seluruh panel lama keluar
        yield return SlideOut(centerPosition, keluar);

        // Nonaktifkan halaman lama
        pages[currentPage].SetActive(false);

        // Pindah ke halaman baru
        currentPage = newPage;

        pages[currentPage].SetActive(true);

        // Perbarui judul dan nomor
        UpdateJudul();
        UpdateNomorHalaman();

        Vector2 masuk =
            centerPosition -
            Vector2.right * slideDistance * direction;

        slideRoot.anchoredPosition = masuk;

        // Seluruh panel baru masuk
        yield return SlideIn(masuk, centerPosition);

        slideRoot.anchoredPosition = centerPosition;
        slideCanvasGroup.alpha = 1f;

        isAnimating = false;

        UpdateButtons();
    }

    private IEnumerator SlideOut(Vector2 from, Vector2 to)
    {
        float timer = 0f;
        float dur = Mathf.Max(0.01f, duration);

        while (timer < dur)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / dur);
            float smooth = EaseInOut(t);

            slideRoot.anchoredPosition =
                Vector2.Lerp(from, to, smooth);

            slideCanvasGroup.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    Mathf.SmoothStep(0.35f, 1f, t)
                );

            yield return null;
        }

        slideRoot.anchoredPosition = to;
        slideCanvasGroup.alpha = 0f;
    }

    private IEnumerator SlideIn(Vector2 from, Vector2 to)
    {
        float timer = 0f;
        float dur = Mathf.Max(0.01f, duration);

        while (timer < dur)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / dur);
            float smooth = EaseInOut(t);

            slideRoot.anchoredPosition =
                Vector2.Lerp(from, to, smooth);

            slideCanvasGroup.alpha =
                Mathf.SmoothStep(0f, 1f, t);

            yield return null;
        }

        slideRoot.anchoredPosition = to;
        slideCanvasGroup.alpha = 1f;
    }

    private float EaseInOut(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    // ===================================
    // TOMBOL NAVIGASI
    // ===================================

    private void SetNavigationVisible(bool visible)
    {
        if (btnSebelumnya != null)
            btnSebelumnya.gameObject.SetActive(visible);

        if (btnSelanjutnya != null)
            btnSelanjutnya.gameObject.SetActive(visible);
    }

    private void UpdateButtons()
    {
        if (btnSebelumnya != null)
        {
            btnSebelumnya.gameObject.SetActive(
                currentPage > 0
            );
        }

        if (btnSelanjutnya != null)
        {
            btnSelanjutnya.gameObject.SetActive(
                currentPage < pages.Length - 1
            );
        }
    }
}
