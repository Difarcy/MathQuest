
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenyusunFaktorPrimaManager : MonoBehaviour
{
    [Header("Panel Soal")]
    [SerializeField] private TMP_Text txtInstruksi;
    [SerializeField] private TMP_Text txtPetunjukSoal;
    [SerializeField] private TMP_Text txtProgressSoal;

    [Header("Area Jawaban")]
    [SerializeField] private TMP_Text txtPersamaan;
    [SerializeField] private TMP_Text txtPetunjukJawaban;
    [SerializeField] private RectTransform[] slot =
        new RectTransform[4];

    [Header("Enam Kotak Pilihan (01-06)")]
    [SerializeField] private Button[] pilihan =
        new Button[6];

    [Header("Ikon Jawaban")]
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
    private const int JUMLAH_PILIHAN = 6;
    private const int JUMLAH_SLOT = 4;

    private const string KUNCI_BINTANG =
        "MathQuest_MenyusunFaktorPrima_Bintang";

    // ==========================================
    // BANK SOAL: BILANGAN KECIL DAN BESAR
    // Setiap bilangan mempunyai tepat
    // empat faktor prima dengan pengulangan.
    // Setiap sesi mengambil 10 dari 12.
    // ==========================================

    private readonly int[] bankSoal =
    {
        16, 24, 36, 40, 54, 56,
        60, 81, 84, 88, 90, 100
    };

    private readonly int[] bankPengecohPrima =
    {
        2, 3, 5, 7, 11, 13, 17, 19
    };

    private readonly List<int> urutanSoal =
        new List<int>();

    private readonly List<int> faktorTersisa =
        new List<int>();

    private readonly TMP_Text[] teksPilihan =
        new TMP_Text[JUMLAH_PILIHAN];

    private readonly Image[] gambarPilihan =
        new Image[JUMLAH_PILIHAN];

    private readonly Image[] ikonStatus =
        new Image[JUMLAH_PILIHAN];

    private readonly CanvasGroup[] grupPilihan =
        new CanvasGroup[JUMLAH_PILIHAN];

    private readonly Color[] warnaTeksAwal =
        new Color[JUMLAH_PILIHAN];

    private readonly int[] angkaPilihan =
        new int[JUMLAH_PILIHAN];

    private readonly bool[] sudahDipakai =
        new bool[JUMLAH_PILIHAN];

    private readonly bool[] sedangSalah =
        new bool[JUMLAH_PILIHAN];

    private readonly GameObject[] isiSlot =
        new GameObject[JUMLAH_SLOT];

    private Canvas canvasUtama;
    private RectTransform canvasRect;

    private GameObject pratinjauSeret;
    private int indeksSeret = -1;

    private int nomorSoal;
    private int jumlahBenar;
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

        canvasUtama =
            pilihan[0].GetComponentInParent<Canvas>();

        if (canvasUtama == null)
        {
            Debug.LogError(
                "MenyusunFaktorPrima: Canvas tidak ditemukan.",
                this
            );

            enabled = false;
            return;
        }

        canvasUtama = canvasUtama.rootCanvas;

        canvasRect =
            canvasUtama.GetComponent<RectTransform>();

        if (!SiapkanPilihan())
        {
            enabled = false;
            return;
        }

        HubungkanTombol();
        MulaiPermainan();
    }

    // ==========================================
    // VALIDASI INSPECTOR
    // ==========================================

    private bool PeriksaInspector()
    {
        if (txtInstruksi == null ||
            txtProgressSoal == null ||
            txtPersamaan == null ||
            panelHasil == null ||
            txtNilaiSkor == null ||
            pilihan == null ||
            pilihan.Length != JUMLAH_PILIHAN ||
            slot == null ||
            slot.Length != JUMLAH_SLOT ||
            bintangEmas == null ||
            bintangEmas.Length != 3)
        {
            Debug.LogError(
                "MenyusunFaktorPrima: " +
                "referensi Inspector belum lengkap.",
                this
            );

            return false;
        }

        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            if (pilihan[i] == null)
            {
                Debug.LogError(
                    "Pilihan Element " + i +
                    " belum diisi.",
                    this
                );

                return false;
            }
        }

        for (int i = 0; i < JUMLAH_SLOT; i++)
        {
            if (slot[i] == null)
            {
                Debug.LogError(
                    "Slot Element " + i +
                    " belum diisi.",
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
                    "Bintang Emas Element " + i +
                    " belum diisi.",
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
        Transform[] semua =
            induk.GetComponentsInChildren<Transform>(true);

        foreach (Transform anak in semua)
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
    // MENYIAPKAN ENAM PILIHAN
    // ==========================================

    private bool SiapkanPilihan()
    {
        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            Button tombol = pilihan[i];

            gambarPilihan[i] =
                tombol.GetComponent<Image>();

            if (gambarPilihan[i] == null)
            {
                Debug.LogError(
                    tombol.name + " harus memiliki Image.",
                    tombol
                );

                return false;
            }

            Transform anakAngka =
                CariChild(tombol.transform, "TXT_Angka");

            if (anakAngka == null)
            {
                Debug.LogError(
                    tombol.name + ": TXT_Angka tidak ditemukan.",
                    tombol
                );

                return false;
            }

            teksPilihan[i] =
                anakAngka.GetComponent<TMP_Text>();

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
            teksPilihan[i].enabled = true;
            teksPilihan[i].raycastTarget = false;

            warnaTeksAwal[i] =
                teksPilihan[i].color;

            // ----------------------------------
            // ICON_STATUS
            // ----------------------------------

            Transform anakStatus =
                CariChild(tombol.transform, "Icon_Status");

            if (anakStatus == null)
            {
                GameObject status = new GameObject(
                    "Icon_Status",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image)
                );

                status.transform.SetParent(
                    tombol.transform, false
                );

                RectTransform rect =
                    status.GetComponent<RectTransform>();

                rect.anchorMin =
                    new Vector2(0.5f, 0.5f);

                rect.anchorMax =
                    new Vector2(0.5f, 0.5f);

                rect.pivot =
                    new Vector2(0.5f, 0.5f);

                rect.anchoredPosition = Vector2.zero;

                float ukuran = Mathf.Min(
                    ((RectTransform)tombol.transform).rect.width,
                    ((RectTransform)tombol.transform).rect.height
                ) * 0.45f;

                rect.sizeDelta =
                    new Vector2(ukuran, ukuran);

                anakStatus = status.transform;
            }

            ikonStatus[i] =
                anakStatus.GetComponent<Image>();

            if (ikonStatus[i] == null)
            {
                ikonStatus[i] =
                    anakStatus.gameObject.AddComponent<Image>();
            }

            RectTransform rectIkon =
                anakStatus as RectTransform;

            if (rectIkon != null &&
                (rectIkon.rect.width < 1f ||
                 rectIkon.rect.height < 1f))
            {
                rectIkon.anchorMin =
                    new Vector2(0.5f, 0.5f);

                rectIkon.anchorMax =
                    new Vector2(0.5f, 0.5f);

                rectIkon.pivot =
                    new Vector2(0.5f, 0.5f);

                rectIkon.anchoredPosition = Vector2.zero;

                float ukuran = Mathf.Min(
                    ((RectTransform)tombol.transform).rect.width,
                    ((RectTransform)tombol.transform).rect.height
                ) * 0.45f;

                rectIkon.sizeDelta =
                    new Vector2(ukuran, ukuran);
            }

            ikonStatus[i].preserveAspect = true;
            ikonStatus[i].raycastTarget = false;
            ikonStatus[i].gameObject.SetActive(false);

            // ----------------------------------
            // CANVAS GROUP
            // ----------------------------------

            grupPilihan[i] =
                tombol.GetComponent<CanvasGroup>();

            if (grupPilihan[i] == null)
            {
                grupPilihan[i] =
                    tombol.gameObject.AddComponent<CanvasGroup>();
            }

            grupPilihan[i].ignoreParentGroups = false;
            grupPilihan[i].alpha = 1f;
            grupPilihan[i].interactable = true;
            grupPilihan[i].blocksRaycasts = true;

            gambarPilihan[i].raycastTarget = true;

            tombol.transition =
                Selectable.Transition.None;

            // ----------------------------------
            // EVENT DRAG
            // ----------------------------------

            EventTrigger trigger =
                tombol.GetComponent<EventTrigger>();

            if (trigger == null)
            {
                trigger =
                    tombol.gameObject.AddComponent<EventTrigger>();
            }

            trigger.triggers.Clear();

            int indeks = i;

            TambahEvent(
                trigger,
                EventTriggerType.BeginDrag,
                data => MulaiSeret(
                    indeks,
                    data as PointerEventData
                )
            );

            TambahEvent(
                trigger,
                EventTriggerType.Drag,
                data => GerakkanSeret(
                    indeks,
                    data as PointerEventData
                )
            );

            TambahEvent(
                trigger,
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

    private void HubungkanTombol()
    {
        if (btnUlangi == null)
            btnUlangi = CariTombol("BTN_Ulangi");

        if (btnMenuBermain == null)
            btnMenuBermain = CariTombol("BTN_MenuBermain");

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
        Button[] semua =
            panelHasil.GetComponentsInChildren<Button>(true);

        foreach (Button tombol in semua)
        {
            if (tombol.name.Trim() == nama)
                return tombol;
        }

        return null;
    }

    // ==========================================
    // PENGACAKAN FISHER-YATES
    // ==========================================

    private void AcakDaftar(List<int> daftar)
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
    // ACAK 10 SOAL DARI 12 TARGET
    // ==========================================

    private void AcakSepuluhSoal()
    {
        List<int> kandidat =
            new List<int>(bankSoal);

        AcakDaftar(kandidat);

        urutanSoal.Clear();

        for (int i = 0; i < JUMLAH_SOAL; i++)
        {
            urutanSoal.Add(kandidat[i]);
        }
    }

    // ==========================================
    // MENGHITUNG FAKTOR PRIMA
    // ==========================================

    private List<int> HitungFaktorPrima(int bilangan)
    {
        List<int> hasil = new List<int>();
        int sisa = bilangan;

        for (int pembagi = 2;
             pembagi * pembagi <= sisa;
             pembagi++)
        {
            while (sisa % pembagi == 0)
            {
                hasil.Add(pembagi);
                sisa /= pembagi;
            }
        }

        if (sisa > 1)
            hasil.Add(sisa);

        return hasil;
    }

    // ==========================================
    // MEMBUAT 6 PILIHAN ANGKA SECARA ACAK
    // ==========================================

    private List<int> BuatPilihanAcak(
        List<int> faktorBenar)
    {
        // Empat faktor benar, termasuk
        // pengulangan seperti 2, 2, 2, 3.
        List<int> hasil =
            new List<int>(faktorBenar);

        // Pilih dua pengecoh yang tidak
        // termasuk faktor prima target.
        List<int> pengecoh =
            new List<int>();

        foreach (int prima in bankPengecohPrima)
        {
            if (!faktorBenar.Contains(prima))
            {
                pengecoh.Add(prima);
            }
        }

        AcakDaftar(pengecoh);

        hasil.Add(pengecoh[0]);
        hasil.Add(pengecoh[1]);

        // Posisi keenam kotak ikut diacak.
        AcakDaftar(hasil);

        return hasil;
    }

    // ==========================================
    // MULAI DAN ULANGI PERMAINAN
    // ==========================================

    public void MulaiPermainan()
    {
        Time.timeScale = 1f;

        StopAllCoroutines();
        BersihkanSeret();

        // Acak ulang setiap sesi.
        AcakSepuluhSoal();

        nomorSoal = 0;
        jumlahBenar = 0;
        soalTanpaKesalahan = 0;

        sedangTransisi = false;
        permainanSelesai = false;
        adaKesalahan = false;

        foreach (GameObject bintang in bintangEmas)
        {
            bintang.SetActive(false);
        }

        panelHasil.SetActive(false);

        TampilkanSoal();
    }

    public void UlangiPermainan()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        MulaiPermainan();
    }

    // ==========================================
    // MENAMPILKAN SOAL ACAK
    // ==========================================

    private void TampilkanSoal()
    {
        BersihkanSeret();

        sedangTransisi = false;
        adaKesalahan = false;
        jumlahBenar = 0;

        int target =
            urutanSoal[nomorSoal];

        // Angka soal biru.
        txtInstruksi.richText = true;

        txtInstruksi.text =
            "Susun faktor prima dari " +
            "<color=#258FFC>" +
            target +
            "</color>!";

        if (txtPetunjukSoal != null)
        {
            txtPetunjukSoal.text =
                "Seret bilangan prima untuk " +
                "melengkapi perkalian.";
        }

        txtProgressSoal.text =
            "Soal " + (nomorSoal + 1) +
            "/" + JUMLAH_SOAL;

        // Angka persamaan biru.
        txtPersamaan.richText = true;

        txtPersamaan.text =
            "<color=#258FFC>" +
            target +
            "</color> =";

        if (txtPetunjukJawaban != null)
        {
            txtPetunjukJawaban.text =
                "Pilih angka di bawah dan " +
                "seret ke slot kosong";
        }

        // Hitung empat faktor yang diperlukan.
        faktorTersisa.Clear();

        faktorTersisa.AddRange(
            HitungFaktorPrima(target)
        );

        if (faktorTersisa.Count != JUMLAH_SLOT)
        {
            Debug.LogError(
                "Target " + target +
                " tidak mempunyai tepat empat faktor prima.",
                this
            );

            sedangTransisi = true;
            return;
        }

        // Kosongkan semua slot.
        for (int i = 0; i < JUMLAH_SLOT; i++)
        {
            if (isiSlot[i] != null)
            {
                Destroy(isiSlot[i]);
                isiSlot[i] = null;
            }
        }

        // Acak angka pilihan termasuk pengecoh.
        List<int> enamAngka =
            BuatPilihanAcak(faktorTersisa);

        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            angkaPilihan[i] =
                enamAngka[i];

            sudahDipakai[i] = false;
            sedangSalah[i] = false;

            pilihan[i].gameObject.SetActive(true);
            pilihan[i].interactable = true;

            grupPilihan[i].alpha = 1f;
            grupPilihan[i].interactable = true;
            grupPilihan[i].blocksRaycasts = true;

            teksPilihan[i].gameObject.SetActive(true);
            teksPilihan[i].enabled = true;

            teksPilihan[i].text =
                angkaPilihan[i].ToString();

            teksPilihan[i].color =
                warnaTeksAwal[i];

            ikonStatus[i].gameObject.SetActive(false);
        }
    }

    // ==========================================
    // MULAI SERET ANGKA
    // ==========================================

    private void MulaiSeret(
        int indeks,
        PointerEventData data)
    {
        if (data == null ||
            sedangTransisi ||
            permainanSelesai ||
            indeksSeret != -1 ||
            sudahDipakai[indeks] ||
            sedangSalah[indeks])
        {
            return;
        }

        indeksSeret = indeks;

        RectTransform sumber =
            pilihan[indeks].GetComponent<RectTransform>();

        pratinjauSeret =
            BuatVisualAngka(
                indeks,
                canvasRect,
                sumber.rect.size,
                "Pratinjau_Seret"
            );

        if (pratinjauSeret == null)
        {
            indeksSeret = -1;
            return;
        }

        // Sembunyikan kotak asal sementara.
        // Tetap aktif agar Grid Layout stabil.
        grupPilihan[indeks].alpha = 0f;
        grupPilihan[indeks].blocksRaycasts = false;

        PindahkanVisual(data);
    }

    // ==========================================
    // SAAT ANGKA DISERET
    // ==========================================

    private void GerakkanSeret(
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
        Camera kamera = KameraUI(data);

        if (RectTransformUtility
            .ScreenPointToWorldPointInRectangle(
                canvasRect,
                data.position,
                kamera,
                out Vector3 posisi))
        {
            pratinjauSeret.transform.position =
                posisi;
        }
    }

    // ==========================================
    // SAAT ANGKA DILEPASKAN
    // ==========================================

    private void AkhiriSeret(
        int indeks,
        PointerEventData data)
    {
        if (indeks != indeksSeret)
            return;

        int slotTujuan = -1;

        if (data != null)
        {
            Camera kamera = KameraUI(data);

            for (int i = 0; i < JUMLAH_SLOT; i++)
            {
                if (isiSlot[i] == null &&
                    RectTransformUtility
                    .RectangleContainsScreenPoint(
                        slot[i],
                        data.position,
                        kamera))
                {
                    slotTujuan = i;
                    break;
                }
            }
        }

        BersihkanSeret();

        // Di luar slot atau slot terisi:
        // kembali tanpa penalti.
        if (slotTujuan < 0 ||
            sedangTransisi ||
            permainanSelesai)
        {
            return;
        }

        int angka =
            angkaPilihan[indeks];

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // Harus cocok dengan faktor
        // yang masih dibutuhkan.
        if (!faktorTersisa.Contains(angka))
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
                HilangkanIkonSalah(indeks, nomorSoal)
            );

            return;
        }

        // Jawaban benar masuk ke slot.
        float ukuran = Mathf.Min(
            slot[slotTujuan].rect.width,
            slot[slotTujuan].rect.height
        ) * 0.85f;

        GameObject angkaMasuk =
            BuatVisualAngka(
                indeks,
                slot[slotTujuan],
                new Vector2(ukuran, ukuran),
                "IMG_AngkaMasuk"
            );

        if (angkaMasuk == null)
            return;

        isiSlot[slotTujuan] = angkaMasuk;

        // Hanya hapus satu kemunculan.
        // Contoh: 2,2,2,3 menjadi 2,2,3
        // setelah satu angka 2 dipakai.
        faktorTersisa.Remove(angka);

        sudahDipakai[indeks] = true;
        pilihan[indeks].interactable = false;

        // Kotak asal tetap mengambil tempat
        // dalam Grid Layout.
        grupPilihan[indeks].alpha = 0f;
        grupPilihan[indeks].interactable = false;
        grupPilihan[indeks].blocksRaycasts = false;

        jumlahBenar++;

        if (jumlahBenar == JUMLAH_SLOT &&
            faktorTersisa.Count == 0)
        {
            sedangTransisi = true;

            if (!adaKesalahan)
                soalTanpaKesalahan++;

            StartCoroutine(
                LanjutKeSoalBerikutnya()
            );
        }
    }

    // ==========================================
    // HILANGKAN IKON SALAH
    // ==========================================

    private IEnumerator HilangkanIkonSalah(
        int indeks,
        int soalSaatKesalahan)
    {
        yield return new WaitForSeconds(
            durasiIkonSalah
        );

        if (nomorSoal != soalSaatKesalahan)
            yield break;

        ikonStatus[indeks].gameObject.SetActive(false);
        sedangSalah[indeks] = false;
    }

    // ==========================================
    // MEMBUAT VISUAL KOTAK ANGKA
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

        GameObject objekTeks = new GameObject(
            "TXT_AngkaMasuk",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        RectTransform rectTeks =
            objekTeks.GetComponent<RectTransform>();

        rectTeks.SetParent(rect, false);
        rectTeks.anchorMin = Vector2.zero;
        rectTeks.anchorMax = Vector2.one;
        rectTeks.offsetMin = Vector2.zero;
        rectTeks.offsetMax = Vector2.zero;

        TMP_Text sumber =
            teksPilihan[indeks];

        TextMeshProUGUI teks =
            objekTeks.GetComponent<TextMeshProUGUI>();

        teks.text =
            angkaPilihan[indeks].ToString();

        teks.font = sumber.font;
        teks.fontStyle = sumber.fontStyle;
        teks.color = warnaTeksAwal[indeks];

        teks.alignment =
            TextAlignmentOptions.Center;

        teks.fontSize = sumber.fontSize;
        teks.enableAutoSizing = true;
        teks.fontSizeMin = 10f;
        teks.fontSizeMax = Mathf.Max(
            10f,
            sumber.fontSize
        );

        teks.textWrappingMode =
            TextWrappingModes.NoWrap;

        teks.raycastTarget = false;
        rect.SetAsLastSibling();

        return objek;
    }

    // ==========================================
    // KAMERA UNTUK DRAG UI
    // ==========================================

    private Camera KameraUI(
        PointerEventData data)
    {
        if (canvasUtama.renderMode ==
            RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return data.pressEventCamera;
    }

    // ==========================================
    // MEMBERSIHKAN DRAG
    // ==========================================

    private void BersihkanSeret()
    {
        if (indeksSeret >= 0 &&
            indeksSeret < JUMLAH_PILIHAN &&
            grupPilihan[indeksSeret] != null)
        {
            grupPilihan[indeksSeret].alpha = 1f;
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
    // LANJUT SOAL BERIKUTNYA
    // ==========================================

    private IEnumerator LanjutKeSoalBerikutnya()
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

        int skor =
            soalTanpaKesalahan * 10;

        int jumlahBintang =
            skor >= 90 ? 3 :
            skor >= 70 ? 2 : 1;

        // Hanya angka; label "Skor:" sudah ada.
        txtNilaiSkor.text =
            skor.ToString();

        for (int i = 0; i < bintangEmas.Length; i++)
        {
            bintangEmas[i].SetActive(
                i < jumlahBintang
            );
        }

        // Simpan rekor tertinggi untuk cover.
        int rekorLama =
            PlayerPrefs.GetInt(KUNCI_BINTANG, 0);

        if (jumlahBintang > rekorLama)
        {
            PlayerPrefs.SetInt(
                KUNCI_BINTANG,
                jumlahBintang
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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        if (!Application.CanStreamedLevelBeLoaded(
            "MenuBermain"))
        {
            Debug.LogError(
                "Scene MenuBermain belum terdaftar " +
                "di Build Profiles.",
                this
            );

            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuBermain");
    }

    // ==========================================
    // HAPUS LISTENER
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
