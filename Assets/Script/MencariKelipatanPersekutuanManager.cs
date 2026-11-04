
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MencariKelipatanPersekutuanManager : MonoBehaviour
{
    [Header("Panel Soal dan Deret")]
    [SerializeField] private TMP_Text txtInstruksi;
    [SerializeField] private TMP_Text txtProgressSoal;
    [SerializeField] private TMP_Text txtDeret1;
    [SerializeField] private TMP_Text txtDeret2;
    [SerializeField] private TMP_Text txtPetunjuk;

    [Header("Enam Kotak Pilihan (urut 01-06)")]
    [SerializeField] private Button[] pilihan = new Button[6];
    [SerializeField] private Sprite ikonSalah;

    [Header("Dua Slot Jawaban")]
    [SerializeField] private RectTransform slot01;
    [SerializeField] private RectTransform slot02;

    [Header("Hasil Permainan")]
    [SerializeField] private GameObject panelHasil;
    [SerializeField] private TMP_Text txtNilaiSkor;
    [SerializeField] private GameObject[] bintangEmas =
        new GameObject[3];
    [SerializeField] private Button btnUlangi;
    [SerializeField] private Button btnMenuBermain;

    [Header("Pengaturan")]
    [SerializeField, Min(0.1f)]
    private float jedaAntarSoal = 0.8f;

    private const int JUMLAH_SOAL = 10;

    private readonly Vector2Int[] bankPasangan =
    {
        new Vector2Int(2, 3),
        new Vector2Int(3, 4),
        new Vector2Int(4, 6),
        new Vector2Int(4, 8),
        new Vector2Int(5, 10),
        new Vector2Int(6, 8),
        new Vector2Int(6, 9),
        new Vector2Int(8, 12),
        new Vector2Int(6, 12),
        new Vector2Int(9, 12)
    };

    private readonly List<Vector2Int> urutanPasangan =
        new List<Vector2Int>();

    private readonly List<int> angkaSaatIni =
        new List<int>();

    private TMP_Text[] teksPilihan;
    private Image[] ikonStatus;

    private Sprite[] spriteAwalKotak;
    private Color[] warnaAwalKotak;
    private Color[] warnaAwalTeks;

    private bool[] sudahDitempatkan;
    private bool[] sudahDihukum;
    private bool[] sedangTampilSalah;

    private RectTransform[] slot;
    private Image[] gambarSlot;
    private Sprite[] spriteAwalSlot;
    private Color[] warnaAwalSlot;
    private bool[] slotTerisi;

    private Canvas canvas;
    private GameObject pratinjauSeret;

    private int indeksDiseret = -1;
    private int nomorSoal;
    private int bilanganA;
    private int bilanganB;
    private int kpk;
    private int jumlahTerisi;
    private int totalSalah;

    private bool sedangTransisi;

    // ==========================================
    // START
    // ==========================================

    private void Start()
    {
        if (!SiapkanReferensi())
        {
            enabled = false;
            return;
        }

        PasangEventDrag();

        btnUlangi.onClick.AddListener(MulaiPermainan);
        btnMenuBermain.onClick.AddListener(KembaliMenuBermain);

        MulaiPermainan();
    }

    // ==========================================
    // MENCARI CHILD TERMASUK NONAKTIF
    // ==========================================

    private Transform CariAnak(Transform induk, string nama)
    {
        foreach (Transform anak in
            induk.GetComponentsInChildren<Transform>(true))
        {
            if (anak != induk && anak.name.Trim() == nama)
                return anak;
        }

        return null;
    }

    // ==========================================
    // VALIDASI INSPECTOR
    // ==========================================

    private bool SiapkanReferensi()
    {
        if (txtInstruksi == null ||
            txtProgressSoal == null ||
            txtDeret1 == null ||
            txtDeret2 == null ||
            txtPetunjuk == null ||
            slot01 == null ||
            slot02 == null ||
            slot01 == slot02 ||
            panelHasil == null ||
            txtNilaiSkor == null ||
            btnUlangi == null ||
            btnMenuBermain == null ||
            ikonSalah == null ||
            pilihan == null ||
            pilihan.Length != 6 ||
            bintangEmas == null ||
            bintangEmas.Length != 3)
        {
            Debug.LogError(
                "MencariKelipatanPersekutuan: Inspector belum lengkap.",
                this
            );

            return false;
        }

        if (pilihan[0] == null)
        {
            Debug.LogError("Pilihan_01 belum diisi.", this);
            return false;
        }

        canvas = pilihan[0].GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Canvas tidak ditemukan.", this);
            return false;
        }

        slot = new[] { slot01, slot02 };

        gambarSlot = new Image[2];
        spriteAwalSlot = new Sprite[2];
        warnaAwalSlot = new Color[2];
        slotTerisi = new bool[2];

        for (int i = 0; i < 2; i++)
        {
            gambarSlot[i] = slot[i].GetComponent<Image>();

            if (gambarSlot[i] == null)
            {
                Debug.LogError(
                    "Slot_01 dan Slot_02 harus memiliki Image.",
                    this
                );

                return false;
            }

            spriteAwalSlot[i] = gambarSlot[i].sprite;
            warnaAwalSlot[i] = gambarSlot[i].color;
        }

        teksPilihan = new TMP_Text[6];
        ikonStatus = new Image[6];

        spriteAwalKotak = new Sprite[6];
        warnaAwalKotak = new Color[6];
        warnaAwalTeks = new Color[6];

        sudahDitempatkan = new bool[6];
        sudahDihukum = new bool[6];
        sedangTampilSalah = new bool[6];

        for (int i = 0; i < pilihan.Length; i++)
        {
            if (pilihan[i] == null || pilihan[i].image == null)
            {
                Debug.LogError(
                    "Button pilihan belum lengkap.",
                    this
                );

                return false;
            }

            Transform anakAngka = CariAnak(
                pilihan[i].transform,
                "TXT_Angka"
            );

            Transform anakStatus = CariAnak(
                pilihan[i].transform,
                "Icon_Status"
            );

            if (anakAngka == null || anakStatus == null)
            {
                Debug.LogError(
                    pilihan[i].name +
                    " harus memiliki TXT_Angka dan Icon_Status.",
                    pilihan[i]
                );

                return false;
            }

            teksPilihan[i] =
                anakAngka.GetComponent<TMP_Text>();

            ikonStatus[i] =
                anakStatus.GetComponent<Image>();

            if (teksPilihan[i] == null || ikonStatus[i] == null)
            {
                Debug.LogError(
                    pilihan[i].name +
                    ": Periksa TextMeshPro dan Image.",
                    pilihan[i]
                );

                return false;
            }

            spriteAwalKotak[i] = pilihan[i].image.sprite;
            warnaAwalKotak[i] = pilihan[i].image.color;

            // Simpan warna teks sebelum disembunyikan.
            warnaAwalTeks[i] = teksPilihan[i].color;

            Color warnaTeks = warnaAwalTeks[i];
            warnaTeks.a = 1f;
            warnaAwalTeks[i] = warnaTeks;

            pilihan[i].transition =
                Selectable.Transition.None;

            teksPilihan[i].raycastTarget = false;
            ikonStatus[i].raycastTarget = false;

            ikonStatus[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < bintangEmas.Length; i++)
        {
            if (bintangEmas[i] == null)
            {
                Debug.LogError(
                    "Isi seluruh 3 Bintang Emas di Inspector.",
                    this
                );

                return false;
            }
        }

        return true;
    }

    // ==========================================
    // EVENT DRAG & DROP
    // ==========================================

    private void PasangEventDrag()
    {
        for (int i = 0; i < pilihan.Length; i++)
        {
            int index = i;

            EventTrigger pemicu =
                pilihan[i].GetComponent<EventTrigger>();

            if (pemicu == null)
            {
                pemicu = pilihan[i].gameObject
                    .AddComponent<EventTrigger>();
            }

            pemicu.triggers.Clear();

            TambahEvent(
                pemicu,
                EventTriggerType.BeginDrag,
                data => MulaiSeret(
                    index,
                    (PointerEventData)data
                )
            );

            TambahEvent(
                pemicu,
                EventTriggerType.Drag,
                data => Geser(
                    index,
                    (PointerEventData)data
                )
            );

            TambahEvent(
                pemicu,
                EventTriggerType.EndDrag,
                data => AkhiriSeret(
                    index,
                    (PointerEventData)data
                )
            );
        }
    }

    private void TambahEvent(
        EventTrigger pemicu,
        EventTriggerType jenis,
        UnityEngine.Events.UnityAction<BaseEventData> aksi
    )
    {
        EventTrigger.Entry entri =
            new EventTrigger.Entry
            {
                eventID = jenis
            };

        entri.callback.AddListener(aksi);
        pemicu.triggers.Add(entri);
    }

    // ==========================================
    // MULAI PERMAINAN
    // ==========================================

    public void MulaiPermainan()
    {
        StopAllCoroutines();
        HapusPratinjau();

        indeksDiseret = -1;
        nomorSoal = 1;
        totalSalah = 0;
        sedangTransisi = false;

        panelHasil.SetActive(false);

        foreach (GameObject bintang in bintangEmas)
            bintang.SetActive(false);

        urutanPasangan.Clear();
        urutanPasangan.AddRange(bankPasangan);

        Acak(urutanPasangan);

        SiapkanSoal();
    }

    // ==========================================
    // MENYIAPKAN SOAL
    // ==========================================

    private void SiapkanSoal()
    {
        sedangTransisi = false;
        indeksDiseret = -1;

        Vector2Int pasangan =
            urutanPasangan[nomorSoal - 1];

        bilanganA = pasangan.x;
        bilanganB = pasangan.y;

        kpk = HitungKPK(bilanganA, bilanganB);

        int batasDeret = kpk * 2;

        txtInstruksi.text =
            "Cari kelipatan persekutuan dari " +
            "<color=#258FFC>" +
            bilanganA + " dan " + bilanganB +
            "</color>";

        txtProgressSoal.text =
            "Soal " + nomorSoal + "/" + JUMLAH_SOAL;

        // ======================================
        // PERUBAHAN WARNA ANGKA DERET MENJADI BIRU
        // ======================================

        txtDeret1.text =
            "Kelipatan " + bilanganA + ": " +
            "<color=#258FFC>" +
            BuatTeksDeret(bilanganA, batasDeret) +
            "</color>";

        txtDeret2.text =
            "Kelipatan " + bilanganB + ": " +
            "<color=#258FFC>" +
            BuatTeksDeret(bilanganB, batasDeret) +
            "</color>";

        // ======================================
        // AKHIR PERUBAHAN WARNA DERET
        // ======================================

        txtPetunjuk.text =
            "Seret 2 angka yang sama ke sini";

        BuatEnamAngka(batasDeret);

        jumlahTerisi = 0;

        // Reset kedua slot jawaban.
        for (int i = 0; i < 2; i++)
        {
            slotTerisi[i] = false;

            gambarSlot[i].sprite = spriteAwalSlot[i];
            gambarSlot[i].color = warnaAwalSlot[i];

            Transform angkaMasuk =
                slot[i].Find("IMG_AngkaMasuk");

            if (angkaMasuk != null)
                Destroy(angkaMasuk.gameObject);
        }

        // Reset keenam pilihan.
        for (int i = 0; i < 6; i++)
        {
            sudahDitempatkan[i] = false;
            sudahDihukum[i] = false;
            sedangTampilSalah[i] = false;

            pilihan[i].image.sprite = spriteAwalKotak[i];
            pilihan[i].image.color = warnaAwalKotak[i];
            pilihan[i].interactable = true;

            teksPilihan[i].text =
                angkaSaatIni[i].ToString();

            teksPilihan[i].color = warnaAwalTeks[i];
            teksPilihan[i].alpha = 1f;

            ikonStatus[i].gameObject.SetActive(false);
        }
    }

    // ==========================================
    // HITUNG KPK
    // ==========================================

    private int HitungKPK(int a, int b)
    {
        int x = a;
        int y = b;

        while (y != 0)
        {
            int sisa = x % y;
            x = y;
            y = sisa;
        }

        return a / x * b;
    }

    // ==========================================
    // MEMBUAT TEKS DERET
    // ==========================================

    private string BuatTeksDeret(int angka, int batas)
    {
        List<int> deret = new List<int>();

        for (int nilai = angka;
             nilai <= batas;
             nilai += angka)
        {
            deret.Add(nilai);
        }

        if (deret.Count <= 6)
            return string.Join(", ", deret);

        return deret[0] + ", " +
               deret[1] + ", " +
               deret[2] + ", " +
               deret[3] + ", ..., " +
               deret[deret.Count - 1];
    }

    // ==========================================
    // ENAM ANGKA BERURUTAN
    // ==========================================

    private void BuatEnamAngka(int batasDeret)
    {
        angkaSaatIni.Clear();

        // Dua jawaban benar.
        angkaSaatIni.Add(kpk);
        angkaSaatIni.Add(kpk * 2);

        HashSet<int> pengecoh =
            new HashSet<int>();

        for (int n = bilanganA;
             n <= batasDeret;
             n += bilanganA)
        {
            if (n % kpk != 0)
                pengecoh.Add(n);
        }

        for (int n = bilanganB;
             n <= batasDeret;
             n += bilanganB)
        {
            if (n % kpk != 0)
                pengecoh.Add(n);
        }

        // Tambahan pengecoh jika diperlukan.
        for (int selisih = 1;
             pengecoh.Count < 4 &&
             selisih < batasDeret;
             selisih++)
        {
            int atas = kpk + selisih;
            int bawah = kpk - selisih;

            if (atas > 0 && atas % kpk != 0)
                pengecoh.Add(atas);

            if (bawah > 0 && bawah % kpk != 0)
                pengecoh.Add(bawah);
        }

        List<int> acakPengecoh =
            new List<int>(pengecoh);

        Acak(acakPengecoh);

        for (int i = 0; i < 4; i++)
            angkaSaatIni.Add(acakPengecoh[i]);

        // Urutkan dari kecil ke besar.
        angkaSaatIni.Sort();
    }

    private void Acak<T>(List<T> daftar)
    {
        for (int i = daftar.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            T sementara = daftar[i];
            daftar[i] = daftar[j];
            daftar[j] = sementara;
        }
    }

    // ==========================================
    // MULAI DRAG TANPA DOUBLE
    // ==========================================

    private void MulaiSeret(
        int index,
        PointerEventData data
    )
    {
        if (sedangTransisi ||
            sudahDitempatkan[index] ||
            sedangTampilSalah[index])
        {
            return;
        }

        indeksDiseret = index;

        HapusPratinjau();

        GameObject go = new GameObject(
            "Pratinjau_Seret",
            typeof(RectTransform),
            typeof(Image)
        );

        pratinjauSeret = go;

        RectTransform rect =
            (RectTransform)go.transform;

        rect.SetParent(canvas.transform, false);
        rect.SetAsLastSibling();

        rect.sizeDelta =
            ((RectTransform)pilihan[index].transform)
            .rect.size;

        Image gambar = go.GetComponent<Image>();

        gambar.sprite = pilihan[index].image.sprite;
        gambar.color = warnaAwalKotak[index];
        gambar.preserveAspect = true;
        gambar.raycastTarget = false;

        GameObject goTeks = new GameObject(
            "TXT_Angka",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        RectTransform rectTeks =
            (RectTransform)goTeks.transform;

        rectTeks.SetParent(rect, false);

        rectTeks.anchorMin = Vector2.zero;
        rectTeks.anchorMax = Vector2.one;
        rectTeks.offsetMin = Vector2.zero;
        rectTeks.offsetMax = Vector2.zero;

        TextMeshProUGUI teks =
            goTeks.GetComponent<TextMeshProUGUI>();

        teks.text = angkaSaatIni[index].ToString();
        teks.font = teksPilihan[index].font;
        teks.fontSize = teksPilihan[index].fontSize;
        teks.fontStyle = teksPilihan[index].fontStyle;

        // Warna asli, bukan warna transparan.
        teks.color = warnaAwalTeks[index];
        teks.alpha = 1f;

        teks.alignment = TextAlignmentOptions.Center;
        teks.raycastTarget = false;

        // Kotak asal disembunyikan,
        // tetapi GameObject tetap aktif.
        SembunyikanKotakAsal(index);

        Geser(index, data);
    }

    // ==========================================
    // MENGGERAKKAN KOTAK
    // ==========================================

    private void Geser(
        int index,
        PointerEventData data
    )
    {
        if (index != indeksDiseret ||
            pratinjauSeret == null)
        {
            return;
        }

        RectTransform areaCanvas =
            (RectTransform)canvas.transform;

        Vector2 posisiLokal;

        if (RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                areaCanvas,
                data.position,
                data.pressEventCamera,
                out posisiLokal))
        {
            ((RectTransform)pratinjauSeret.transform)
                .anchoredPosition = posisiLokal;
        }
    }

    // ==========================================
    // SELESAI DRAG
    // ==========================================

    private void AkhiriSeret(
        int index,
        PointerEventData data
    )
    {
        if (index != indeksDiseret)
            return;

        HapusPratinjau();
        indeksDiseret = -1;

        if (sedangTransisi || sudahDitempatkan[index])
        {
            if (!sudahDitempatkan[index])
                PulihkanKotakAsal(index);

            return;
        }

        int tujuan = -1;

        for (int i = 0; i < 2; i++)
        {
            if (!slotTerisi[i] &&
                RectTransformUtility
                .RectangleContainsScreenPoint(
                    slot[i],
                    data.position,
                    data.pressEventCamera))
            {
                tujuan = i;
                break;
            }
        }

        // Di luar slot: kotak kembali.
        if (tujuan < 0)
        {
            PulihkanKotakAsal(index);
            return;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        int nilai = angkaSaatIni[index];

        if (nilai == kpk || nilai == 2 * kpk)
        {
            TerimaJawaban(index, tujuan);
        }
        else
        {
            PulihkanKotakAsal(index);
            TampilkanJawabanSalah(index);
        }
    }

    // ==========================================
    // SEMBUNYIKAN KOTAK ASAL
    // ==========================================

    private void SembunyikanKotakAsal(int index)
    {
        Color warna = warnaAwalKotak[index];
        warna.a = 0f;

        pilihan[index].image.color = warna;
        teksPilihan[index].alpha = 0f;
    }

    // ==========================================
    // KEMBALIKAN KOTAK ASAL
    // ==========================================

    private void PulihkanKotakAsal(int index)
    {
        pilihan[index].image.color =
            warnaAwalKotak[index];

        teksPilihan[index].color =
            warnaAwalTeks[index];

        teksPilihan[index].alpha = 1f;
    }

    // ==========================================
    // JAWABAN BENAR
    // ==========================================

    private void TerimaJawaban(
        int index,
        int tujuan
    )
    {
        slotTerisi[tujuan] = true;
        sudahDitempatkan[index] = true;

        jumlahTerisi++;

        // Buat kotak berangka di dalam slot.
        BuatAngkaPadaSlot(tujuan, index);

        // Kotak asal tetap kosong.
        pilihan[index].interactable = false;
        SembunyikanKotakAsal(index);

        ikonStatus[index].gameObject.SetActive(false);

        if (jumlahTerisi == 2)
        {
            sedangTransisi = true;

            StartCoroutine(
                LanjutKeSoalBerikut()
            );
        }
    }

    // ==========================================
    // ANGKA MASUK KE SLOT
    // ==========================================

    private void BuatAngkaPadaSlot(
        int tujuan,
        int index
    )
    {
        GameObject go = new GameObject(
            "IMG_AngkaMasuk",
            typeof(RectTransform),
            typeof(Image)
        );

        RectTransform rect =
            (RectTransform)go.transform;

        rect.SetParent(slot[tujuan], false);

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition = Vector2.zero;

        // Kotak tetap pas di dalam wadah.
        rect.sizeDelta =
            slot[tujuan].rect.size * 0.86f;

        Image gambar =
            go.GetComponent<Image>();

        gambar.sprite =
            spriteAwalKotak[index];

        gambar.color =
            warnaAwalKotak[index];

        gambar.preserveAspect = true;
        gambar.raycastTarget = false;

        // Buat teks angka di atas gambar kotak.
        GameObject goTeks = new GameObject(
            "TXT_AngkaMasuk",
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        RectTransform rectTeks =
            (RectTransform)goTeks.transform;

        rectTeks.SetParent(rect, false);

        rectTeks.anchorMin = Vector2.zero;
        rectTeks.anchorMax = Vector2.one;
        rectTeks.offsetMin = Vector2.zero;
        rectTeks.offsetMax = Vector2.zero;

        TextMeshProUGUI teks =
            goTeks.GetComponent<TextMeshProUGUI>();

        teks.text =
            angkaSaatIni[index].ToString();

        teks.font =
            teksPilihan[index].font;

        teks.fontSize =
            teksPilihan[index].fontSize;

        teks.fontStyle =
            teksPilihan[index].fontStyle;

        // Menggunakan warna asli teks.
        teks.color = warnaAwalTeks[index];
        teks.alpha = 1f;

        teks.alignment =
            TextAlignmentOptions.Center;

        // Revisi warning CS0618:
        // Tidak menggunakan enableWordWrapping lagi.
        teks.textWrappingMode = TextWrappingModes.NoWrap;
        teks.raycastTarget = false;

        // Pastikan teks terlihat di atas gambar.
        rectTeks.SetAsLastSibling();

        teks.ForceMeshUpdate();
    }

    // ==========================================
    // JAWABAN SALAH
    // ==========================================

    private void TampilkanJawabanSalah(int index)
    {
        // Angka salah yang sama hanya
        // mengurangi skor satu kali.
        if (!sudahDihukum[index])
        {
            sudahDihukum[index] = true;
            totalSalah++;
        }

        ikonStatus[index].sprite = ikonSalah;
        ikonStatus[index].color = Color.white;
        ikonStatus[index].gameObject.SetActive(true);

        sedangTampilSalah[index] = true;

        StartCoroutine(
            HilangkanIkonSalah(index)
        );
    }

    private IEnumerator HilangkanIkonSalah(int index)
    {
        yield return new WaitForSeconds(0.65f);

        ikonStatus[index].gameObject.SetActive(false);

        sedangTampilSalah[index] = false;
    }

    // ==========================================
    // LANJUT OTOMATIS
    // ==========================================

    private IEnumerator LanjutKeSoalBerikut()
    {
        yield return new WaitForSeconds(
            jedaAntarSoal
        );

        if (nomorSoal == JUMLAH_SOAL)
        {
            TampilkanHasil();
        }
        else
        {
            nomorSoal++;
            SiapkanSoal();
        }
    }

    // ==========================================
    // HASIL AKHIR
    // ==========================================

    private void TampilkanHasil()
    {
        int skor = Mathf.Clamp(
            100 - totalSalah * 5,
            0,
            100
        );

        txtNilaiSkor.text =
            skor.ToString();

        int jumlahBintang =
            skor >= 90 ? 3 :
            skor >= 70 ? 2 : 1;

        for (int i = 0; i < bintangEmas.Length; i++)
        {
            bintangEmas[i].SetActive(
                i < jumlahBintang
            );
        }

        string key =
            "MathQuest_MencariKelipatanPersekutuan_Bintang";

        int terbaik =
            PlayerPrefs.GetInt(key, 0);

        if (jumlahBintang > terbaik)
        {
            PlayerPrefs.SetInt(
                key,
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
        SceneManager.LoadScene("MenuBermain");
    }

    // ==========================================
    // HAPUS VISUAL DRAG SEMENTARA
    // ==========================================

    private void HapusPratinjau()
    {
        if (pratinjauSeret != null)
        {
            Destroy(pratinjauSeret);
        }

        pratinjauSeret = null;
    }

    private void OnDisable()
    {
        if (indeksDiseret >= 0 &&
            sudahDitempatkan != null &&
            indeksDiseret < sudahDitempatkan.Length &&
            !sudahDitempatkan[indeksDiseret])
        {
            PulihkanKotakAsal(indeksDiseret);
        }

        indeksDiseret = -1;
        HapusPratinjau();
    }
}
