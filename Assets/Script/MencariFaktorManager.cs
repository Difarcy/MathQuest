
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MencariFaktorManager : MonoBehaviour
{
    [Header("Teks Soal")]
    [SerializeField] private TMP_Text txtInstruksi;
    [SerializeField] private TMP_Text txtPetunjukSoal;
    [SerializeField] private TMP_Text txtProgressSoal;
    [SerializeField] private TMP_Text txtPetunjukJawaban;

    [Header("Enam Kotak Pilihan (01-06)")]
    [SerializeField] private Button[] pilihan = new Button[6];

    [Header("Empat Slot Jawaban (01-04)")]
    [SerializeField] private RectTransform[] slot =
        new RectTransform[4];

    [Header("Ikon Jawaban")]
    [SerializeField] private Sprite ikonSalah;

    [Header("Hasil Permainan")]
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

    private const int JUMLAH_SOAL = 10;
    private const int JUMLAH_PILIHAN = 6;
    private const int JUMLAH_SLOT = 4;

    private const string KUNCI_BINTANG =
        "MathQuest_MencariFaktor_Bintang";

    // Setiap bilangan mempunyai 4 faktor positif.
    // Tidak ada target yang berulang.
    private readonly int[] targetSoal =
    {
        6, 8, 10, 14, 15,
        21, 22, 26, 27, 33
    };

    // Empat faktor benar dan dua pengecoh.
    // Keenam angka selalu urut dari kecil ke besar.
    private readonly int[,] opsiSoal =
    {
        { 1, 2, 3, 4, 5, 6 },
        { 1, 2, 3, 4, 7, 8 },
        { 1, 2, 3, 5, 7, 10 },
        { 1, 2, 4, 7, 9, 14 },
        { 1, 2, 3, 5, 7, 15 },
        { 1, 3, 5, 7, 14, 21 },
        { 1, 2, 5, 11, 17, 22 },
        { 1, 2, 4, 13, 17, 26 },
        { 1, 2, 3, 6, 9, 27 },
        { 1, 3, 5, 11, 15, 33 }
    };

    private readonly TMP_Text[] teksPilihan =
        new TMP_Text[JUMLAH_PILIHAN];

    private readonly Image[] gambarPilihan =
        new Image[JUMLAH_PILIHAN];

    private readonly CanvasGroup[] grupPilihan =
        new CanvasGroup[JUMLAH_PILIHAN];

    private readonly Image[] statusPilihan =
        new Image[JUMLAH_PILIHAN];

    private readonly int[] angkaPilihan =
        new int[JUMLAH_PILIHAN];

    private readonly bool[] sudahDigunakan =
        new bool[JUMLAH_PILIHAN];

    private readonly GameObject[] isiSlot =
        new GameObject[JUMLAH_SLOT];

    private Canvas canvasUtama;
    private RectTransform canvasRect;

    private GameObject objekDrag;
    private int indeksDrag = -1;

    private int soalSekarang;
    private int jawabanBenar;
    private int soalTanpaKesalahan;

    private bool adaKesalahan;
    private bool sedangTransisi;
    private bool permainanSelesai;

    // ==========================================
    // PERSIAPAN
    // ==========================================

    private void Start()
    {
        if (!PeriksaReferensi())
        {
            enabled = false;
            return;
        }

        canvasUtama =
            pilihan[0].GetComponentInParent<Canvas>();

        if (canvasUtama != null)
            canvasUtama = canvasUtama.rootCanvas;

        if (canvasUtama == null)
        {
            Debug.LogError(
                "MencariFaktor: Canvas tidak ditemukan.",
                this
            );

            enabled = false;
            return;
        }

        canvasRect =
            canvasUtama.GetComponent<RectTransform>();

        SiapkanPilihan();

        // Jangan memulai permainan jika salah satu kotak
        // tidak memiliki Image atau TXT_Angka.
        if (!enabled)
            return;

        HubungkanTombolHasil();
        MulaiPermainan();
    }

    // ==========================================
    // HUBUNGKAN TOMBOL PANEL HASIL
    // ==========================================

    private void HubungkanTombolHasil()
    {
        // Mencari termasuk child yang nonaktif.
        // Ini penting karena Panel_Hasil biasanya
        // disembunyikan pada awal permainan.
        if (btnUlangi == null)
            btnUlangi = CariTombolHasil("BTN_Ulangi");

        if (btnMenuBermain == null)
            btnMenuBermain = CariTombolHasil("BTN_MenuBermain");

        if (btnUlangi != null)
        {
            btnUlangi.onClick.RemoveListener(UlangiPermainan);
            btnUlangi.onClick.AddListener(UlangiPermainan);
            btnUlangi.interactable = true;
        }
        else
        {
            Debug.LogWarning(
                "MencariFaktor: BTN_Ulangi belum ditemukan. " +
                "Isi field Btn Ulangi di Inspector.", this
            );
        }

        if (btnMenuBermain != null)
        {
            btnMenuBermain.onClick.RemoveListener(KembaliMenuBermain);
            btnMenuBermain.onClick.AddListener(KembaliMenuBermain);
            btnMenuBermain.interactable = true;
        }
        else
        {
            Debug.LogWarning(
                "MencariFaktor: BTN_MenuBermain belum ditemukan. " +
                "Isi field Btn Menu Bermain di Inspector.", this
            );
        }
    }

    private Button CariTombolHasil(string namaTombol)
    {
        if (panelHasil == null)
            return null;

        Button[] semuaTombol =
            panelHasil.GetComponentsInChildren<Button>(true);

        foreach (Button tombol in semuaTombol)
        {
            if (tombol.name == namaTombol)
                return tombol;
        }

        return null;
    }

    private bool PeriksaReferensi()
    {
        if (txtInstruksi == null ||
            txtProgressSoal == null ||
            pilihan == null ||
            pilihan.Length != JUMLAH_PILIHAN ||
            slot == null ||
            slot.Length != JUMLAH_SLOT)
        {
            Debug.LogError(
                "MencariFaktor: Referensi Inspector belum lengkap.",
                this
            );

            return false;
        }

        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            if (pilihan[i] == null)
            {
                Debug.LogError(
                    "Isi semua 6 kotak pilihan.",
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
                    "Isi semua 4 slot jawaban.",
                    this
                );

                return false;
            }
        }

        return true;
    }

    // ==========================================
    // MENYIAPKAN ENAM PILIHAN
    // ==========================================

    private void SiapkanPilihan()
    {
        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            Button tombol = pilihan[i];

            gambarPilihan[i] =
                tombol.GetComponent<Image>();

            Transform anakAngka =
                tombol.transform.Find("TXT_Angka");

            if (gambarPilihan[i] == null ||
                anakAngka == null)
            {
                Debug.LogError(
                    tombol.name +
                    " harus memiliki Image dan TXT_Angka.",
                    tombol
                );

                enabled = false;
                return;
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

                enabled = false;
                return;
            }

            // Teks tidak menghalangi drag.
            teksPilihan[i].raycastTarget = false;

            grupPilihan[i] =
                tombol.GetComponent<CanvasGroup>();

            if (grupPilihan[i] == null)
            {
                grupPilihan[i] =
                    tombol.gameObject.AddComponent<CanvasGroup>();
            }

            Transform status =
                tombol.transform.Find("Icon_Status");

            if (status != null)
            {
                statusPilihan[i] =
                    status.GetComponent<Image>();

                if (statusPilihan[i] != null)
                    statusPilihan[i].raycastTarget = false;

                status.gameObject.SetActive(false);
            }

            // Daftarkan drag langsung ke tombol.
            EventTrigger trigger =
                tombol.GetComponent<EventTrigger>();

            if (trigger == null)
            {
                trigger =
                    tombol.gameObject.AddComponent<EventTrigger>();
            }

            // Hapus event trigger lama pada tombol ini.
            trigger.triggers.Clear();

            int indeks = i;

            PasangEvent(
                trigger,
                EventTriggerType.BeginDrag,
                data => MulaiDrag(
                    indeks,
                    data as PointerEventData
                )
            );

            PasangEvent(
                trigger,
                EventTriggerType.Drag,
                data => GerakkanDrag(
                    indeks,
                    data as PointerEventData
                )
            );

            PasangEvent(
                trigger,
                EventTriggerType.EndDrag,
                data => SelesaiDrag(
                    indeks,
                    data as PointerEventData
                )
            );
        }
    }

    private void PasangEvent(
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
    // MULAI / ULANGI PERMAINAN
    // ==========================================

    public void MulaiPermainan()
    {
        StopAllCoroutines();
        BersihkanDrag();

        soalSekarang = 0;
        jawabanBenar = 0;
        soalTanpaKesalahan = 0;
        adaKesalahan = false;

        sedangTransisi = false;
        permainanSelesai = false;

        if (panelHasil != null)
            panelHasil.SetActive(false);

        // Hasil sesi baru dimulai kosong.
        // Rekor bintang tertinggi di PlayerPrefs TIDAK dihapus.
        if (txtNilaiSkor != null)
            txtNilaiSkor.text = "0";

        if (bintangEmas != null)
        {
            foreach (GameObject bintang in bintangEmas)
            {
                if (bintang != null)
                    bintang.SetActive(false);
            }
        }

        TampilkanSoal();
    }

    public void UlangiPermainan()
    {
        AudioManager.Instance?.PlayButtonClick();
        Time.timeScale = 1f;
        MulaiPermainan();
    }

    // ==========================================
    // MENAMPILKAN SOAL
    // ==========================================

    private void TampilkanSoal()
    {
        BersihkanDrag();

        jawabanBenar = 0;
        adaKesalahan = false;
        sedangTransisi = false;

        int target = targetSoal[soalSekarang];

        txtInstruksi.text =
            "Cari semua faktor dari <color=#268DFF>" +
            target +
            "</color>";

        if (txtPetunjukSoal != null)
        {
            txtPetunjukSoal.text =
                "Pilih bilangan yang dapat membagi " +
                target + " tanpa sisa.";
        }

        txtProgressSoal.text =
            "Soal " + (soalSekarang + 1) +
            "/" + JUMLAH_SOAL;

        if (txtPetunjukJawaban != null)
        {
            txtPetunjukJawaban.text =
                "Seret bilangan faktor ke kotak kosong";
        }

        // Kosongkan keempat slot sebelumnya.
        for (int i = 0; i < JUMLAH_SLOT; i++)
        {
            if (isiSlot[i] != null)
            {
                Destroy(isiSlot[i]);
                isiSlot[i] = null;
            }
        }

        // Tampilkan enam pilihan baru.
        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            angkaPilihan[i] =
                opsiSoal[soalSekarang, i];

            sudahDigunakan[i] = false;

            pilihan[i].gameObject.SetActive(true);
            pilihan[i].interactable = true;

            teksPilihan[i].text =
                angkaPilihan[i].ToString();

            grupPilihan[i].alpha = 1f;
            grupPilihan[i].interactable = true;
            grupPilihan[i].blocksRaycasts = true;

            if (statusPilihan[i] != null)
            {
                statusPilihan[i].gameObject.SetActive(false);
            }
        }
    }

    // ==========================================
    // MULAI MENYERET ANGKA
    // ==========================================

    private void MulaiDrag(
        int indeks,
        PointerEventData data)
    {
        if (data == null ||
            sedangTransisi ||
            permainanSelesai ||
            indeksDrag != -1 ||
            sudahDigunakan[indeks])
        {
            return;
        }

        indeksDrag = indeks;

        // Buat satu tampilan angka yang bergerak.
        objekDrag = BuatVisualAngka(
            indeks,
            canvasRect,
            ((RectTransform)pilihan[indeks].transform)
                .rect.size,
            "Angka_SedangDiseret"
        );

        if (objekDrag == null)
        {
            indeksDrag = -1;
            return;
        }

        // Sembunyikan kotak asal.
        // Layout asal tetap mempertahankan tempatnya.
        grupPilihan[indeks].alpha = 0f;
        grupPilihan[indeks].blocksRaycasts = false;

        PindahkanVisualDrag(data);
    }

    // ==========================================
    // SAAT ANGKA DIGERAKKAN
    // ==========================================

    private void GerakkanDrag(
        int indeks,
        PointerEventData data)
    {
        if (indeks != indeksDrag ||
            objekDrag == null ||
            data == null)
        {
            return;
        }

        PindahkanVisualDrag(data);
    }

    private void PindahkanVisualDrag(
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
            objekDrag.transform.position = posisi;
        }
    }

    // ==========================================
    // KETIKA ANGKA DILEPASKAN
    // ==========================================

    private void SelesaiDrag(
        int indeks,
        PointerEventData data)
    {
        if (indeks != indeksDrag)
            return;

        int slotTujuan = -1;

        if (data != null)
        {
            Camera kamera = KameraUI(data);

            for (int i = 0; i < JUMLAH_SLOT; i++)
            {
                if (RectTransformUtility
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

        BersihkanDrag();

        // Lepas di luar slot: kembali, bukan salah.
        if (slotTujuan == -1)
            return;

        // Slot sudah terisi: kembali ke asal.
        if (isiSlot[slotTujuan] != null)
            return;

        int angka = angkaPilihan[indeks];
        int target = targetSoal[soalSekarang];

        AudioManager.Instance?.PlayButtonClick();

        // Faktor adalah pembagi habis.
        bool benar =
            angka > 0 && target % angka == 0;

        if (!benar)
        {
            adaKesalahan = true;

            if (statusPilihan[indeks] != null &&
                ikonSalah != null)
            {
                statusPilihan[indeks].sprite = ikonSalah;
                statusPilihan[indeks]
                    .gameObject.SetActive(true);

                StartCoroutine(
                    HilangkanStatus(indeks)
                );
            }

            return;
        }

        // Tempatkan satu kotak angka ke slot.
        float ukuran = Mathf.Min(
            slot[slotTujuan].rect.width,
            slot[slotTujuan].rect.height
        ) * 0.9f;

        isiSlot[slotTujuan] = BuatVisualAngka(
            indeks,
            slot[slotTujuan],
            new Vector2(ukuran, ukuran),
            "Angka_Terpasang"
        );

        // Kotak asal baru hilang setelah
        // berhasil masuk ke slot.
        sudahDigunakan[indeks] = true;
        pilihan[indeks].gameObject.SetActive(false);

        jawabanBenar++;

        if (jawabanBenar >= JUMLAH_SLOT)
        {
            sedangTransisi = true;

            if (!adaKesalahan)
                soalTanpaKesalahan++;

            StartCoroutine(LanjutSetelahJeda());
        }
    }

    private IEnumerator HilangkanStatus(int indeks)
    {
        yield return new WaitForSeconds(0.45f);

        if (statusPilihan[indeks] != null)
            statusPilihan[indeks].gameObject.SetActive(false);
    }

    private IEnumerator LanjutSetelahJeda()
    {
        yield return new WaitForSeconds(jedaAntarSoal);

        soalSekarang++;

        if (soalSekarang >= JUMLAH_SOAL)
            TampilkanHasil();
        else
            TampilkanSoal();
    }

    // ==========================================
    // MEMBUAT SATU VISUAL KOTAK ANGKA
    // ==========================================

    private GameObject BuatVisualAngka(
        int indeks,
        RectTransform induk,
        Vector2 ukuran,
        string nama)
    {
        if (gambarPilihan[indeks] == null ||
            teksPilihan[indeks] == null ||
            induk == null)
        {
            return null;
        }

        GameObject kotak = new GameObject(
            nama,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform rect =
            kotak.GetComponent<RectTransform>();

        rect.SetParent(induk, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = ukuran;
        rect.localScale = Vector3.one;

        Image gambar = kotak.GetComponent<Image>();

        gambar.sprite = gambarPilihan[indeks].sprite;
        gambar.color = gambarPilihan[indeks].color;
        gambar.preserveAspect = true;
        gambar.raycastTarget = false;

        // Teks angka menggunakan font yang sama
        // dengan kotak pilihan aslinya.
        GameObject teksObjek = new GameObject(
            "TXT_Angka",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        RectTransform teksRect =
            teksObjek.GetComponent<RectTransform>();

        teksRect.SetParent(rect, false);
        teksRect.anchorMin = Vector2.zero;
        teksRect.anchorMax = Vector2.one;
        teksRect.offsetMin = Vector2.zero;
        teksRect.offsetMax = Vector2.zero;
        teksRect.localScale = Vector3.one;

        TMP_Text sumber = teksPilihan[indeks];

        TextMeshProUGUI teks =
            teksObjek.GetComponent<TextMeshProUGUI>();

        teks.text = angkaPilihan[indeks].ToString();
        teks.font = sumber.font;
        teks.fontStyle = sumber.fontStyle;
        teks.color = sumber.color;
        teks.alignment = TextAlignmentOptions.Center;

        teks.fontSize = sumber.fontSize;
        teks.enableAutoSizing = true;
        teks.fontSizeMin = 10f;
        teks.fontSizeMax = Mathf.Max(
            10f,
            sumber.fontSize
        );

        teks.raycastTarget = false;

        rect.SetAsLastSibling();

        return kotak;
    }

    // ==========================================
    // KAMERA UNTUK INPUT UI
    // ==========================================

    private Camera KameraUI(PointerEventData data)
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

    private void BersihkanDrag()
    {
        if (indeksDrag >= 0 &&
            indeksDrag < JUMLAH_PILIHAN &&
            grupPilihan[indeksDrag] != null)
        {
            grupPilihan[indeksDrag].alpha = 1f;
            grupPilihan[indeksDrag].blocksRaycasts = true;
        }

        if (objekDrag != null)
        {
            Destroy(objekDrag);
            objekDrag = null;
        }

        indeksDrag = -1;
    }

    // ==========================================
    // HASIL 10 SOAL
    // ==========================================

    private void TampilkanHasil()
    {
        BersihkanDrag();

        permainanSelesai = true;
        sedangTransisi = true;

        int nilai = soalTanpaKesalahan * 10;

        int jumlahBintang;

        if (nilai >= 90)
            jumlahBintang = 3;
        else if (nilai >= 70)
            jumlahBintang = 2;
        else
            jumlahBintang = 1;

        // Simpan hanya rekor tertinggi.
        int rekorLama = PlayerPrefs.GetInt(
            KUNCI_BINTANG, 0
        );

        int rekorBaru = Mathf.Max(
            rekorLama,
            jumlahBintang
        );

        if (rekorBaru > rekorLama)
        {
            PlayerPrefs.SetInt(
                KUNCI_BINTANG,
                rekorBaru
            );

            PlayerPrefs.Save();
        }

        Debug.Log(
            "[MENCARI FAKTOR] Nilai: " + nilai +
            " | Bintang: " + jumlahBintang +
            " | Rekor: " + rekorBaru
        );

        // Panel_Hasil sudah memiliki label "Skor:".
        // Tampilkan angkanya saja agar tidak dobel.
        if (txtNilaiSkor != null)
            txtNilaiSkor.text = nilai.ToString();

        for (int i = 0; i < bintangEmas.Length; i++)
        {
            if (bintangEmas[i] != null)
            {
                bintangEmas[i].SetActive(
                    i < jumlahBintang
                );
            }
        }

        if (panelHasil != null)
            panelHasil.SetActive(true);
    }

    // ==========================================
    // KEMBALI KE MENU BERMAIN
    // ==========================================

    public void KembaliMenuBermain()
    {
        AudioManager.Instance?.PlayButtonClick();

        // Pastikan scene tujuan terdaftar di Build Profiles.
        if (!Application.CanStreamedLevelBeLoaded("MenuBermain"))
        {
            Debug.LogError(
                "Scene MenuBermain belum masuk Scene List " +
                "di File > Build Profiles. Tambahkan dahulu.",
                this
            );
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuBermain");
    }

    private void OnDestroy()
    {
        // Hindari listener runtime tertinggal saat scene berganti.
        if (btnUlangi != null)
            btnUlangi.onClick.RemoveListener(UlangiPermainan);

        if (btnMenuBermain != null)
            btnMenuBermain.onClick.RemoveListener(KembaliMenuBermain);
    }
}
