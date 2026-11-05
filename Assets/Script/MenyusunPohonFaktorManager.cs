
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenyusunPohonFaktorManager : MonoBehaviour
{
    [Header("Panel Soal")]
    [SerializeField] private TMP_Text txtInstruksi;
    [SerializeField] private TMP_Text txtPetunjuk;
    [SerializeField] private TMP_Text txtProgressSoal;

    [Header("Angka Akar Pohon")]
    [SerializeField] private TMP_Text txtAngkaAkar;

    [Header("Enam Slot pada Area_PohonFaktor")]
    [SerializeField] private RectTransform slotKiri1;
    [SerializeField] private RectTransform slotKanan1;
    [SerializeField] private RectTransform slotKiri2;
    [SerializeField] private RectTransform slotKiri3;
    [SerializeField] private RectTransform slotKanan2;
    [SerializeField] private RectTransform slotKanan3;

    [Header("Sepuluh Kotak Pilihan (01-10)")]
    [SerializeField] private Button[] pilihan = new Button[10];
    [SerializeField] private Sprite ikonSalah;

    [Header("Panel Hasil")]
    [SerializeField] private GameObject panelHasil;
    [SerializeField] private TMP_Text txtNilaiSkor;
    [SerializeField] private GameObject[] bintangEmas =
        new GameObject[3];

    [Header("Tombol Panel Hasil")]
    [SerializeField] private Button btnUlangi;
    [SerializeField] private Button btnMenuBermain;

    [Header("Pengaturan")]
    [SerializeField, Min(0f)]
    private float jedaAntarSoal = 0.8f;

    [SerializeField, Min(0f)]
    private float durasiIkonSalah = 0.5f;

    private const int JUMLAH_SOAL = 10;
    private const int JUMLAH_PILIHAN = 10;
    private const int JUMLAH_SLOT = 6;

    private const string KUNCI_BINTANG =
        "MathQuest_MenyusunPohonFaktor_Bintang";

    // Semua target mempunyai empat faktor prima.
    private readonly int[] bankTarget =
    {
        16, 24, 36, 40, 54, 56,
        60, 81, 84, 88, 90, 100
    };

    private readonly int[] bankPrima =
    {
        2, 3, 5, 7, 11, 13,
        17, 19, 23, 29, 31
    };

    private readonly List<int> urutanSoal =
        new List<int>();

    private readonly List<int> tengahTersisa =
        new List<int>();

    private readonly List<int> kiriTersisa =
        new List<int>();

    private readonly List<int> kananTersisa =
        new List<int>();

    private readonly TMP_Text[] teksPilihan =
        new TMP_Text[JUMLAH_PILIHAN];

    private readonly Image[] gambarPilihan =
        new Image[JUMLAH_PILIHAN];

    private readonly Image[] ikonStatus =
        new Image[JUMLAH_PILIHAN];

    private readonly CanvasGroup[] grupPilihan =
        new CanvasGroup[JUMLAH_PILIHAN];

    private readonly Color[] warnaTeks =
        new Color[JUMLAH_PILIHAN];

    private readonly int[] angkaPilihan =
        new int[JUMLAH_PILIHAN];

    private readonly bool[] pilihanTerpakai =
        new bool[JUMLAH_PILIHAN];

    private readonly bool[] sedangSalah =
        new bool[JUMLAH_PILIHAN];

    private readonly RectTransform[] slotPohon =
        new RectTransform[JUMLAH_SLOT];

    private readonly GameObject[] isiSlot =
        new GameObject[JUMLAH_SLOT];

    private Canvas canvasUtama;
    private RectTransform canvasRect;

    private GameObject pratinjauSeret;
    private int indeksSeret = -1;

    private int nomorSoal;
    private int jumlahTengah;
    private int jumlahDaun;

    private int nilaiTengahKiri;
    private int nilaiTengahKanan;

    private int soalTanpaKesalahan;

    private bool adaKesalahan;
    private bool sedangTransisi;
    private bool permainanSelesai;

    // ==========================================
    // PERSIAPAN
    // ==========================================

    private void Start()
    {
        if (!PeriksaInspector())
        {
            enabled = false;
            return;
        }

        slotPohon[0] = slotKiri1;
        slotPohon[1] = slotKanan1;
        slotPohon[2] = slotKiri2;
        slotPohon[3] = slotKiri3;
        slotPohon[4] = slotKanan2;
        slotPohon[5] = slotKanan3;

        canvasUtama =
            pilihan[0].GetComponentInParent<Canvas>();

        if (canvasUtama == null)
        {
            Debug.LogError(
                "MenyusunPohonFaktor: Canvas tidak ditemukan.",
                this
            );

            enabled = false;
            return;
        }

        canvasUtama = canvasUtama.rootCanvas;

        canvasRect =
            canvasUtama.GetComponent<RectTransform>();

        if (!SiapkanKotakPilihan())
        {
            enabled = false;
            return;
        }

        // Kosongkan teks bawaan slot.
        foreach (RectTransform s in slotPohon)
        {
            Transform anak = s.Find("TXT_Angka");

            if (anak != null)
            {
                TMP_Text tmp =
                    anak.GetComponent<TMP_Text>();

                if (tmp != null)
                {
                    tmp.text = "";
                    tmp.raycastTarget = false;
                }
            }

            Image gambar = s.GetComponent<Image>();

            if (gambar != null)
                gambar.raycastTarget = false;
        }

        HubungkanTombolHasil();
        MulaiPermainan();
    }

    // ==========================================
    // VALIDASI INSPECTOR
    // ==========================================

    private bool PeriksaInspector()
    {
        if (txtInstruksi == null ||
            txtPetunjuk == null ||
            txtProgressSoal == null ||
            txtAngkaAkar == null ||
            slotKiri1 == null ||
            slotKanan1 == null ||
            slotKiri2 == null ||
            slotKiri3 == null ||
            slotKanan2 == null ||
            slotKanan3 == null ||
            pilihan == null ||
            pilihan.Length != JUMLAH_PILIHAN ||
            panelHasil == null ||
            txtNilaiSkor == null ||
            bintangEmas == null ||
            bintangEmas.Length != 3)
        {
            Debug.LogError(
                "MenyusunPohonFaktor: Inspector belum lengkap.",
                this
            );

            return false;
        }

        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            if (pilihan[i] == null)
            {
                Debug.LogError(
                    "Isi Pilihan Element " + i + ".",
                    this
                );

                return false;
            }
        }

        for (int i = 0; i < bintangEmas.Length; i++)
        {
            if (bintangEmas[i] == null)
            {
                Debug.LogError(
                    "Isi Bintang Emas Element " + i + ".",
                    this
                );

                return false;
            }
        }

        return true;
    }

    // ==========================================
    // MENCARI CHILD TERMASUK NONAKTIF
    // ==========================================

    private Transform CariChild(
        Transform induk,
        string nama)
    {
        foreach (Transform anak in
            induk.GetComponentsInChildren<Transform>(true))
        {
            if (anak != induk &&
                anak.name.Trim() == nama)
            {
                return anak;
            }
        }

        return null;
    }

    // ==========================================
    // MENYIAPKAN SEPULUH KOTAK
    // ==========================================

    private bool SiapkanKotakPilihan()
    {
        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            Button tombol = pilihan[i];

            gambarPilihan[i] =
                tombol.GetComponent<Image>();

            Transform angka =
                CariChild(
                    tombol.transform,
                    "TXT_Angka"
                );

            if (gambarPilihan[i] == null ||
                angka == null)
            {
                Debug.LogError(
                    tombol.name +
                    ": membutuhkan Image dan TXT_Angka.",
                    tombol
                );

                return false;
            }

            teksPilihan[i] =
                angka.GetComponent<TMP_Text>();

            if (teksPilihan[i] == null)
            {
                Debug.LogError(
                    tombol.name +
                    ": TXT_Angka harus memakai TextMeshPro.",
                    tombol
                );

                return false;
            }

            teksPilihan[i].gameObject.SetActive(true);
            teksPilihan[i].raycastTarget = false;

            warnaTeks[i] = teksPilihan[i].color;
            warnaTeks[i].a = 1f;

            teksPilihan[i].color = warnaTeks[i];

            // ==================================
            // ICON_STATUS
            // ==================================

            Transform status =
                CariChild(
                    tombol.transform,
                    "Icon_Status"
                );

            if (status == null)
            {
                GameObject baru = new GameObject(
                    "Icon_Status",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

                baru.transform.SetParent(
                    tombol.transform,
                    false
                );

                RectTransform rect =
                    baru.GetComponent<RectTransform>();

                rect.anchorMin =
                    new Vector2(0.5f, 0.5f);

                rect.anchorMax =
                    new Vector2(0.5f, 0.5f);

                rect.pivot =
                    new Vector2(0.5f, 0.5f);

                rect.anchoredPosition =
                    Vector2.zero;

                float sisi = Mathf.Min(
                    ((RectTransform)tombol.transform).rect.width,
                    ((RectTransform)tombol.transform).rect.height
                ) * 0.45f;

                rect.sizeDelta =
                    new Vector2(sisi, sisi);

                status = baru.transform;
            }

            ikonStatus[i] =
                status.GetComponent<Image>();

            if (ikonStatus[i] == null)
            {
                ikonStatus[i] =
                    status.gameObject.AddComponent<Image>();
            }

            RectTransform statusRect =
                status.GetComponent<RectTransform>();

            if (statusRect != null &&
                (statusRect.rect.width < 1f ||
                 statusRect.rect.height < 1f))
            {
                statusRect.anchorMin =
                    new Vector2(0.5f, 0.5f);

                statusRect.anchorMax =
                    new Vector2(0.5f, 0.5f);

                statusRect.pivot =
                    new Vector2(0.5f, 0.5f);

                statusRect.anchoredPosition =
                    Vector2.zero;

                float ukuran = Mathf.Min(
                    ((RectTransform)tombol.transform).rect.width,
                    ((RectTransform)tombol.transform).rect.height
                ) * 0.45f;

                statusRect.sizeDelta =
                    new Vector2(ukuran, ukuran);
            }

            ikonStatus[i].raycastTarget = false;
            ikonStatus[i].preserveAspect = true;
            ikonStatus[i].gameObject.SetActive(false);

            // ==================================
            // CANVAS GROUP
            // ==================================

            grupPilihan[i] =
                tombol.GetComponent<CanvasGroup>();

            if (grupPilihan[i] == null)
            {
                grupPilihan[i] =
                    tombol.gameObject.AddComponent<CanvasGroup>();
            }

            grupPilihan[i].ignoreParentGroups = false;

            gambarPilihan[i].raycastTarget = true;

            tombol.transition =
                Selectable.Transition.None;

            // ==================================
            // DRAG AND DROP
            // ==================================

            EventTrigger eventTrigger =
                tombol.GetComponent<EventTrigger>();

            if (eventTrigger == null)
            {
                eventTrigger =
                    tombol.gameObject.AddComponent<EventTrigger>();
            }

            eventTrigger.triggers.Clear();

            int indeks = i;

            TambahEvent(
                eventTrigger,
                EventTriggerType.BeginDrag,
                data => MulaiSeret(
                    indeks,
                    data as PointerEventData
                )
            );

            TambahEvent(
                eventTrigger,
                EventTriggerType.Drag,
                data => GeserSeret(
                    indeks,
                    data as PointerEventData
                )
            );

            TambahEvent(
                eventTrigger,
                EventTriggerType.EndDrag,
                data => AkhiriSeret(
                    indeks,
                    data as PointerEventData
                )
            );
        }

        return true;
    }

    private void TambahEvent(
        EventTrigger trigger,
        EventTriggerType jenis,
        UnityEngine.Events.UnityAction<BaseEventData> aksi)
    {
        EventTrigger.Entry entry =
            new EventTrigger.Entry();

        entry.eventID = jenis;
        entry.callback.AddListener(aksi);

        trigger.triggers.Add(entry);
    }

    // ==========================================
    // TOMBOL PANEL HASIL
    // ==========================================

    private void HubungkanTombolHasil()
    {
        if (btnUlangi == null)
        {
            btnUlangi = CariTombol("BTN_Ulangi");
        }

        if (btnMenuBermain == null)
        {
            btnMenuBermain =
                CariTombol("BTN_MenuBermain");
        }

        if (btnMenuBermain == null)
        {
            btnMenuBermain =
                CariTombol("BTN_KembaliMenuBermain");
        }

        if (btnUlangi != null)
        {
            btnUlangi.onClick.AddListener(
                UlangiPermainan
            );
        }
        else
        {
            Debug.LogWarning(
                "BTN_Ulangi belum terhubung.",
                this
            );
        }

        if (btnMenuBermain != null)
        {
            btnMenuBermain.onClick.AddListener(
                KembaliMenuBermain
            );
        }
        else
        {
            Debug.LogWarning(
                "BTN_MenuBermain belum terhubung.",
                this
            );
        }
    }

    private Button CariTombol(string nama)
    {
        foreach (Button tombol in
            panelHasil.GetComponentsInChildren<Button>(true))
        {
            if (tombol.name.Trim() == nama)
                return tombol;
        }

        return null;
    }

    // ==========================================
    // PENGACAKAN
    // ==========================================

    private void Acak(List<int> daftar)
    {
        for (int i = daftar.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            int sementara = daftar[i];
            daftar[i] = daftar[j];
            daftar[j] = sementara;
        }
    }

    // ==========================================
    // MENCARI FAKTOR PRIMA
    // ==========================================

    private List<int> FaktorPrima(int bilangan)
    {
        List<int> hasil = new List<int>();

        int sisa = bilangan;

        for (int p = 2; p * p <= sisa; p++)
        {
            while (sisa % p == 0)
            {
                hasil.Add(p);
                sisa /= p;
            }
        }

        if (sisa > 1)
        {
            hasil.Add(sisa);
        }

        return hasil;
    }

    // ==========================================
    // MULAI PERMAINAN
    // ==========================================

    public void MulaiPermainan()
    {
        Time.timeScale = 1f;

        StopAllCoroutines();
        BersihkanSeret();

        // Acak seluruh bank soal.
        urutanSoal.Clear();
        urutanSoal.AddRange(bankTarget);
        Acak(urutanSoal);

        // Ambil 10 soal pertama.
        nomorSoal = 0;
        soalTanpaKesalahan = 0;

        sedangTransisi = false;
        permainanSelesai = false;

        foreach (GameObject b in bintangEmas)
        {
            b.SetActive(false);
        }

        panelHasil.SetActive(false);

        TampilkanSoal();
    }

    public void UlangiPermainan()
    {
        AudioManager.Instance?.PlayButtonClick();

        MulaiPermainan();
    }

    // ==========================================
    // MENAMPILKAN SOAL
    // ==========================================

    private void TampilkanSoal()
    {
        BersihkanSeret();

        jumlahTengah = 0;
        jumlahDaun = 0;

        nilaiTengahKiri = 0;
        nilaiTengahKanan = 0;

        sedangTransisi = false;
        adaKesalahan = false;

        tengahTersisa.Clear();
        kiriTersisa.Clear();
        kananTersisa.Clear();

        int target = urutanSoal[nomorSoal];

        List<int> daun =
            FaktorPrima(target);

        if (daun.Count != 4)
        {
            Debug.LogError(
                "Target " + target +
                " tidak memiliki empat faktor prima.",
                this
            );

            sedangTransisi = true;
            return;
        }

        // ==================================
        // TEKS SOAL
        // ==================================

        txtInstruksi.richText = true;

        txtInstruksi.text =
            "Susun pohon faktor dari " +
            "<color=#258FFC>" +
            target +
            "</color>!";

        txtPetunjuk.text =
            "Isi dua cabang tengah, lalu ujungnya " +
            "dengan bilangan prima.";

        txtProgressSoal.text =
            "Soal " + (nomorSoal + 1) +
            "/" + JUMLAH_SOAL;

        // Angka akar otomatis.
        txtAngkaAkar.text = target.ToString();
        txtAngkaAkar.raycastTarget = false;

        // ==================================
        // MEMILIH DUA FAKTOR TENGAH
        // ==================================

        List<int> acakDaun =
            new List<int>(daun);

        Acak(acakDaun);

        int tengahA =
            acakDaun[0] * acakDaun[1];

        int tengahB =
            acakDaun[2] * acakDaun[3];

        tengahTersisa.Add(tengahA);
        tengahTersisa.Add(tengahB);

        // ==================================
        // MEMBUAT 10 ANGKA PILIHAN
        // ==================================

        List<int> sepuluhPilihan =
            new List<int>();

        // Dua angka tengah.
        sepuluhPilihan.Add(tengahA);
        sepuluhPilihan.Add(tengahB);

        // Empat faktor prima.
        sepuluhPilihan.AddRange(daun);

        // Empat angka pengecoh.
        List<int> pengecoh =
            new List<int>();

        foreach (int p in bankPrima)
        {
            if (!daun.Contains(p) &&
                !sepuluhPilihan.Contains(p))
            {
                pengecoh.Add(p);
            }
        }

        Acak(pengecoh);

        for (int i = 0; i < 4; i++)
        {
            sepuluhPilihan.Add(pengecoh[i]);
        }

        Acak(sepuluhPilihan);

        // ==================================
        // MENGOSONGKAN ENAM SLOT
        // ==================================

        for (int i = 0; i < JUMLAH_SLOT; i++)
        {
            if (isiSlot[i] != null)
            {
                Destroy(isiSlot[i]);
            }

            isiSlot[i] = null;

            Transform label =
                slotPohon[i].Find("TXT_Angka");

            if (label != null)
            {
                TMP_Text teks =
                    label.GetComponent<TMP_Text>();

                if (teks != null)
                    teks.text = "";
            }
        }

        // ==================================
        // MENGISI SEPULUH KOTAK
        // ==================================

        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            angkaPilihan[i] =
                sepuluhPilihan[i];

            pilihanTerpakai[i] = false;
            sedangSalah[i] = false;

            pilihan[i].gameObject.SetActive(true);
            pilihan[i].interactable = true;

            grupPilihan[i].alpha = 1f;
            grupPilihan[i].blocksRaycasts = true;
            grupPilihan[i].interactable = true;

            teksPilihan[i].gameObject.SetActive(true);
            teksPilihan[i].enabled = true;

            teksPilihan[i].text =
                angkaPilihan[i].ToString();

            teksPilihan[i].color =
                warnaTeks[i];

            ikonStatus[i].gameObject.SetActive(false);
        }
    }

    // ==========================================
    // MEMULAI DRAG
    // ==========================================

    private void MulaiSeret(
        int indeks,
        PointerEventData data)
    {
        if (data == null ||
            sedangTransisi ||
            permainanSelesai ||
            indeksSeret >= 0 ||
            pilihanTerpakai[indeks] ||
            sedangSalah[indeks])
        {
            return;
        }

        indeksSeret = indeks;

        RectTransform asal =
            (RectTransform)pilihan[indeks].transform;

        pratinjauSeret = BuatVisualAngka(
            indeks,
            canvasRect,
            asal.rect.size,
            "Pratinjau_Seret"
        );

        if (pratinjauSeret == null)
        {
            indeksSeret = -1;
            return;
        }

        // Kotak asal disembunyikan.
        // Grid Layout tetap tidak bergeser.
        grupPilihan[indeks].alpha = 0f;
        grupPilihan[indeks].blocksRaycasts = false;

        PindahkanVisual(data);
    }

    // ==========================================
    // SAAT ANGKA DISERET
    // ==========================================

    private void GeserSeret(
        int indeks,
        PointerEventData data)
    {
        if (indeks == indeksSeret &&
            data != null &&
            pratinjauSeret != null)
        {
            PindahkanVisual(data);
        }
    }

    private void PindahkanVisual(
        PointerEventData data)
    {
        if (RectTransformUtility
            .ScreenPointToWorldPointInRectangle(
                canvasRect,
                data.position,
                KameraUI(data),
                out Vector3 posisi))
        {
            pratinjauSeret.transform.position =
                posisi;
        }
    }

    private Camera KameraUI(
        PointerEventData data)
    {
        return canvasUtama.renderMode ==
               RenderMode.ScreenSpaceOverlay
            ? null
            : data.pressEventCamera;
    }

    // ==========================================
    // KETIKA ANGKA DILEPASKAN
    // ==========================================

    private void AkhiriSeret(
        int indeks,
        PointerEventData data)
    {
        if (indeks != indeksSeret)
            return;

        int tujuan = -1;

        if (data != null)
        {
            for (int i = 0; i < JUMLAH_SLOT; i++)
            {
                if (isiSlot[i] != null)
                    continue;

                if (RectTransformUtility
                    .RectangleContainsScreenPoint(
                        slotPohon[i],
                        data.position,
                        KameraUI(data)))
                {
                    tujuan = i;
                    break;
                }
            }
        }

        BersihkanSeret();

        if (tujuan < 0 ||
            sedangTransisi ||
            permainanSelesai)
        {
            return;
        }

        // Isi dua cabang tengah lebih dahulu.
        if (jumlahTengah < 2 && tujuan >= 2)
            return;

        // Setelahnya, hanya cabang daun.
        if (jumlahTengah == 2 && tujuan < 2)
            return;

        int angka = angkaPilihan[indeks];

        bool benar;

        if (tujuan < 2)
        {
            benar = tengahTersisa.Contains(angka);
        }
        else if (tujuan < 4)
        {
            benar = kiriTersisa.Contains(angka);
        }
        else
        {
            benar = kananTersisa.Contains(angka);
        }

        AudioManager.Instance?.PlayButtonClick();

        // ==================================
        // JAWABAN SALAH
        // ==================================

        if (!benar)
        {
            adaKesalahan = true;
            sedangSalah[indeks] = true;

            if (ikonSalah != null)
            {
                ikonStatus[indeks].sprite =
                    ikonSalah;

                ikonStatus[indeks].color =
                    Color.white;

                ikonStatus[indeks].gameObject
                    .SetActive(true);
            }

            StartCoroutine(
                HilangkanIkonSalah(
                    indeks,
                    nomorSoal
                )
            );

            return;
        }

        // ==================================
        // JAWABAN BENAR
        // ==================================

        float sisi = Mathf.Min(
            slotPohon[tujuan].rect.width,
            slotPohon[tujuan].rect.height
        ) * 0.83f;

        GameObject visual = BuatVisualAngka(
            indeks,
            slotPohon[tujuan],
            new Vector2(sisi, sisi),
            "IMG_AngkaMasuk"
        );

        if (visual == null)
            return;

        isiSlot[tujuan] = visual;

        pilihanTerpakai[indeks] = true;
        pilihan[indeks].interactable = false;

        grupPilihan[indeks].alpha = 0f;
        grupPilihan[indeks].interactable = false;
        grupPilihan[indeks].blocksRaycasts = false;

        // ==================================
        // MENGISI CABANG TENGAH
        // ==================================

        if (tujuan < 2)
        {
            tengahTersisa.Remove(angka);
            jumlahTengah++;

            if (tujuan == 0)
                nilaiTengahKiri = angka;
            else
                nilaiTengahKanan = angka;

            if (jumlahTengah == 2)
            {
                kiriTersisa.AddRange(
                    FaktorPrima(nilaiTengahKiri)
                );

                kananTersisa.AddRange(
                    FaktorPrima(nilaiTengahKanan)
                );

                txtPetunjuk.text =
                    "Sekarang lengkapi empat ujung " +
                    "dengan bilangan prima.";
            }
        }
        else
        {
            // ==================================
            // MENGISI EMPAT DAUN
            // ==================================

            if (tujuan < 4)
            {
                kiriTersisa.Remove(angka);
            }
            else
            {
                kananTersisa.Remove(angka);
            }

            jumlahDaun++;

            if (jumlahDaun == 4 &&
                kiriTersisa.Count == 0 &&
                kananTersisa.Count == 0)
            {
                sedangTransisi = true;

                if (!adaKesalahan)
                    soalTanpaKesalahan++;

                StartCoroutine(LanjutSoal());
            }
        }
    }

    // ==========================================
    // MENGHILANGKAN IKON SALAH
    // ==========================================

    private IEnumerator HilangkanIkonSalah(
        int indeks,
        int soalSaatSalah)
    {
        yield return new WaitForSeconds(
            durasiIkonSalah
        );

        if (soalSaatSalah != nomorSoal)
            yield break;

        ikonStatus[indeks].gameObject.SetActive(false);
        sedangSalah[indeks] = false;
    }

    // ==========================================
    // MEMBUAT KOTAK ANGKA SAAT DRAG
    // ==========================================

    private GameObject BuatVisualAngka(
        int indeks,
        RectTransform induk,
        Vector2 ukuran,
        string nama)
    {
        if (induk == null)
            return null;

        GameObject objek = new GameObject(
            nama,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform rect =
            objek.GetComponent<RectTransform>();

        rect.SetParent(induk, false);

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = ukuran;
        rect.localScale = Vector3.one;

        Image gambar =
            objek.GetComponent<Image>();

        gambar.sprite =
            gambarPilihan[indeks].sprite;

        gambar.color =
            gambarPilihan[indeks].color;

        gambar.preserveAspect = true;
        gambar.raycastTarget = false;

        // ==================================
        // TEKS ANGKA
        // ==================================

        GameObject teksObjek = new GameObject(
            "TXT_AngkaMasuk",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        RectTransform tr =
            teksObjek.GetComponent<RectTransform>();

        tr.SetParent(rect, false);

        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        TMP_Text sumber =
            teksPilihan[indeks];

        TextMeshProUGUI angka =
            teksObjek.GetComponent<TextMeshProUGUI>();

        angka.text =
            angkaPilihan[indeks].ToString();

        angka.font = sumber.font;
        angka.fontStyle = sumber.fontStyle;
        angka.fontSize = sumber.fontSize;
        angka.color = warnaTeks[indeks];

        angka.alignment =
            TextAlignmentOptions.Center;

        angka.enableAutoSizing = true;
        angka.fontSizeMin = 10f;
        angka.fontSizeMax = Mathf.Max(
            10f,
            sumber.fontSize
        );

        angka.textWrappingMode =
            TextWrappingModes.NoWrap;

        angka.raycastTarget = false;

        rect.SetAsLastSibling();

        return objek;
    }

    // ==========================================
    // MEMBERSIHKAN DRAG SEMENTARA
    // ==========================================

    private void BersihkanSeret()
    {
        if (indeksSeret >= 0 &&
            indeksSeret < JUMLAH_PILIHAN &&
            grupPilihan[indeksSeret] != null)
        {
            grupPilihan[indeksSeret].alpha = 1f;
            grupPilihan[indeksSeret].interactable = true;
            grupPilihan[indeksSeret].blocksRaycasts = true;
        }

        if (pratinjauSeret != null)
        {
            Destroy(pratinjauSeret);
            pratinjauSeret = null;
        }

        indeksSeret = -1;
    }

    // ==========================================
    // LANJUT SOAL
    // ==========================================

    private IEnumerator LanjutSoal()
    {
        yield return new WaitForSeconds(
            jedaAntarSoal
        );

        nomorSoal++;

        if (nomorSoal >= JUMLAH_SOAL)
        {
            TampilkanHasil();
        }
        else
        {
            TampilkanSoal();
        }
    }

    // ==========================================
    // HASIL PERMAINAN
    // ==========================================

    private void TampilkanHasil()
    {
        BersihkanSeret();

        permainanSelesai = true;
        sedangTransisi = true;

        int skor = soalTanpaKesalahan * 10;

        int bintang =
            skor >= 90 ? 3 :
            skor >= 70 ? 2 : 1;

        // Label "Skor:" sudah tersedia.
        txtNilaiSkor.text = skor.ToString();

        for (int i = 0; i < 3; i++)
        {
            bintangEmas[i].SetActive(i < bintang);
        }

        // ==================================
        // REKOR BINTANG TERTINGGI
        // ==================================

        int rekor =
            PlayerPrefs.GetInt(KUNCI_BINTANG, 0);

        if (bintang > rekor)
        {
            PlayerPrefs.SetInt(
                KUNCI_BINTANG,
                bintang
            );

            PlayerPrefs.Save();
        }

        panelHasil.SetActive(true);
    }

    // ==========================================
    // KEMBALI KE MENU BERMAIN
    // ==========================================

    public void KembaliMenuBermain()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (!Application.CanStreamedLevelBeLoaded(
            "MenuBermain"))
        {
            Debug.LogError(
                "Scene MenuBermain belum masuk " +
                "Build Profiles > Scene List.",
                this
            );

            return;
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene("MenuBermain");
    }

    // ==========================================
    // MEMBERSIHKAN LISTENER
    // ==========================================

    private void OnDestroy()
    {
        if (btnUlangi != null)
        {
            btnUlangi.onClick.RemoveListener(
                UlangiPermainan
            );
        }

        if (btnMenuBermain != null)
        {
            btnMenuBermain.onClick.RemoveListener(
                KembaliMenuBermain
            );
        }
    }
}
