
using UnityEngine;
using UnityEngine.UI;

public class HorizontalCoverFade : MonoBehaviour
{
    [Header("Referensi UI")]
    [SerializeField]
    private RectTransform viewport;

    [SerializeField]
    private RectTransform content;

    [Header("Pengaturan Fade")]
    [SerializeField, Min(0f)]
    private float fadeWidth = 100f;

    [SerializeField, Range(0f, 1f)]
    private float minimumClickableAlpha = 0.75f;

    private RectTransform[] covers;
    private CanvasGroup[] coverGroups;

    private readonly Vector3[] corners = new Vector3[4];

    // ==========================================
    // INISIALISASI
    // ==========================================

    private void Start()
    {
        InitializeCovers();
    }

    private void InitializeCovers()
    {
        if (viewport == null || content == null)
        {
            Debug.LogError(
                "HorizontalCoverFade: Viewport atau Content belum diisi.",
                this
            );

            enabled = false;
            return;
        }

        int count = content.childCount;

        covers = new RectTransform[count];
        coverGroups = new CanvasGroup[count];

        for (int i = 0; i < count; i++)
        {
            RectTransform cover =
                content.GetChild(i) as RectTransform;

            if (cover == null)
                continue;

            covers[i] = cover;

            // CanvasGroup utama pada cover.
            CanvasGroup group =
                cover.GetComponent<CanvasGroup>();

            if (group == null)
            {
                group = cover.gameObject
                    .AddComponent<CanvasGroup>();
            }

            group.ignoreParentGroups = false;

            coverGroups[i] = group;

            // Siapkan bintang agar mengikuti
            // transparansi dari cover.
            SiapkanBintang(cover);
        }
    }

    // ==========================================
    // SINKRONISASI BINTANG DENGAN COVER
    // ==========================================

    private void SiapkanBintang(RectTransform cover)
    {
        Transform barisBintang =
            cover.Find("Baris_Bintang");

        if (barisBintang == null)
            return;

        // Periksa CanvasGroup tambahan
        // yang mungkin ada dari script lama.
        CanvasGroup[] grupBintang =
            barisBintang.GetComponentsInChildren<
                CanvasGroup
            >(true);

        foreach (CanvasGroup grup in grupBintang)
        {
            // Jangan abaikan CanvasGroup cover.
            grup.ignoreParentGroups = false;

            // Alpha tetap 1 agar tidak
            // menggandakan efek transparansi.
            grup.alpha = 1f;

            grup.interactable = false;
            grup.blocksRaycasts = false;
        }

        // Bintang tidak menghalangi klik
        // dan swipe pada cover permainan.
        Graphic[] gambar =
            barisBintang.GetComponentsInChildren<
                Graphic
            >(true);

        foreach (Graphic item in gambar)
        {
            item.raycastTarget = false;
        }
    }

    // ==========================================
    // EFEK FADE HALUS
    // ==========================================

    private void LateUpdate()
    {
        if (covers == null || viewport == null)
            return;

        float left = viewport.rect.xMin;
        float right = viewport.rect.xMax;

        for (int i = 0; i < covers.Length; i++)
        {
            if (covers[i] == null ||
                coverGroups[i] == null)
            {
                continue;
            }

            RectTransform cover = covers[i];

            // Hitung berdasarkan ukuran cover,
            // bukan ukuran baris bintang.
            cover.GetWorldCorners(corners);

            float xMin =
                viewport.InverseTransformPoint(
                    corners[0]
                ).x;

            float xMax =
                viewport.InverseTransformPoint(
                    corners[2]
                ).x;

            float centerX =
                (xMin + xMax) * 0.5f;

            float halfWidth =
                Mathf.Abs(xMax - xMin) * 0.5f;

            float fadeDistance = Mathf.Max(
                0.001f,
                halfWidth + fadeWidth
            );

            float leftAlpha = Mathf.Clamp01(
                (centerX - left) / fadeDistance
            );

            float rightAlpha = Mathf.Clamp01(
                (right - centerX) / fadeDistance
            );

            float alpha = Mathf.Min(
                leftAlpha,
                rightAlpha
            );

            // Pertahankan kurva fade halus.
            alpha = Mathf.SmoothStep(
                0f,
                1f,
                alpha
            );

            // Satu-satunya pengaturan alpha:
            // cover beserta seluruh child-nya.
            coverGroups[i].alpha = alpha;

            bool clickable =
                alpha >= minimumClickableAlpha;

            coverGroups[i].interactable = clickable;
            coverGroups[i].blocksRaycasts = clickable;
        }
    }
}
