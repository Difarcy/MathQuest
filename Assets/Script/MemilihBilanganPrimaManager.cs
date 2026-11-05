
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MemilihBilanganPrimaManager : MonoBehaviour
{
    [Header("Panel Soal")]
    [SerializeField] private TMP_Text txtInstruksi;
    [SerializeField] private TMP_Text txtPetunjuk;
    [SerializeField] private TMP_Text txtProgressSoal;

    [Header("Sepuluh Kotak Angka (01-10)")]
    [SerializeField] private Button[] pilihan =
        new Button[10];

    [Header("Ikon Jawaban")]
    [SerializeField] private Sprite ikonBenar;
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

    private const string KUNCI_BINTANG =
        "MathQuest_MemilihBilanganPrima_Bintang";

    private readonly TMP_Text[] teksPilihan =
        new TMP_Text[JUMLAH_PILIHAN];

    private readonly Image[] ikonStatus =
        new Image[JUMLAH_PILIHAN];

    private readonly int[] angkaPilihan =
        new int[JUMLAH_PILIHAN];

    private readonly bool[] sudahBenar =
        new bool[JUMLAH_PILIHAN];

    private readonly bool[] sedangSalah =
        new bool[JUMLAH_PILIHAN];

    private readonly UnityAction[] aksiPilihan =
        new UnityAction[JUMLAH_PILIHAN];

    private int nomorSoal;
    private int jumlahPrima;
    private int jumlahBenarDipilih;
    private int soalTanpaKesalahan;

    private bool adaKesalahan;
    private bool sedangTransisi;
    private bool permainanSelesai;

    // ==========================================
    // START
    // ==========================================

    private void Start()
    {
        if (!PeriksaReferensi())
        {
            enabled = false;
            return;
        }

        if (!SiapkanKotakPilihan())
        {
            enabled = false;
            return;
        }

        HubungkanTombolHasil();

        MulaiPermainan();
    }

    // ==========================================
    // VALIDASI INSPECTOR
    // ==========================================

    private bool PeriksaReferensi()
    {
        if (txtInstruksi == null ||
            txtPetunjuk == null ||
            txtProgressSoal == null ||
            panelHasil == null ||
            txtNilaiSkor == null ||
            ikonBenar == null ||
            ikonSalah == null ||
            pilihan == null ||
            pilihan.Length != JUMLAH_PILIHAN ||
            bintangEmas == null ||
            bintangEmas.Length != 3)
        {
            Debug.LogError(
                "MemilihBilanganPrima: " +
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
                    " belum diisi di Inspector.",
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
    // MENYIAPKAN SEPULUH KOTAK
    // ==========================================

    private bool SiapkanKotakPilihan()
    {
        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            Button tombol = pilihan[i];

            // ----------------------------------
            // 1. GAMBAR KOTAK UTAMA
            // ----------------------------------

            Image gambarKotak =
                tombol.GetComponent<Image>();

            if (gambarKotak == null)
            {
                Debug.LogError(
                    tombol.name +
                    " belum memiliki komponen Image " +
                    "pada GameObject utama.",
                    tombol
                );

                return false;
            }

            // Kotak utama harus dapat diklik.
            gambarKotak.raycastTarget = true;

            // Tidak mengubah warna sprite
            // saat tombol ditekan.
            tombol.transition =
                Selectable.Transition.None;

            // ----------------------------------
            // 2. TXT_ANGKA
            // ----------------------------------

            Transform childAngka =
                CariChild(
                    tombol.transform,
                    "TXT_Angka"
                );

            // Jika child belum ada,
            // buat secara otomatis.
            if (childAngka == null)
            {
                GameObject objekTeks =
                    new GameObject(
                        "TXT_Angka",
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(TextMeshProUGUI)
                    );

                objekTeks.transform.SetParent(
                    tombol.transform,
                    false
                );

                RectTransform rect =
                    objekTeks.GetComponent<RectTransform>();

                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                childAngka = objekTeks.transform;
            }

            TMP_Text teks =
                childAngka.GetComponent<TMP_Text>();

            // Jika child ada, tetapi tidak
            // mempunyai TextMeshPro, tambahkan.
            if (teks == null)
            {
                teks = childAngka.gameObject
                    .AddComponent<TextMeshProUGUI>();

                teks.alignment =
                    TextAlignmentOptions.Center;

                teks.fontSize = 48f;
            }

            teksPilihan[i] = teks;

            // Teks boleh kosong di Inspector.
            // Angkanya diisi saat Play Mode.
            teksPilihan[i].gameObject.SetActive(true);
            teksPilihan[i].enabled = true;
            teksPilihan[i].raycastTarget = false;

            // ----------------------------------
            // 3. ICON_STATUS
            // ----------------------------------

            Transform childStatus =
                CariChild(
                    tombol.transform,
                    "Icon_Status"
                );

            // Jika Icon_Status belum ada,
            // buat child UI baru.
            if (childStatus == null)
            {
                GameObject objekStatus =
                    new GameObject(
                        "Icon_Status",
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(Image)
                    );

                objekStatus.transform.SetParent(
                    tombol.transform,
                    false
                );

                childStatus = objekStatus.transform;
            }

            Image gambarStatus =
                childStatus.GetComponent<Image>();

            bool imageBaru = false;

            // Inilah perbaikan penting:
            // Icon_Status bisa ada di Hierarchy
            // tetapi belum memiliki Image.
            if (gambarStatus == null)
            {
                gambarStatus =
                    childStatus.gameObject
                        .AddComponent<Image>();

                imageBaru = true;
            }

            ikonStatus[i] = gambarStatus;

            // Jika Image baru dibuat,
            // posisikan di tengah kotak.
            if (imageBaru)
            {
                RectTransform rectStatus =
                    childStatus.GetComponent<RectTransform>();

                if (rectStatus != null)
                {
                    rectStatus.anchorMin =
                        new Vector2(0.5f, 0.5f);

                    rectStatus.anchorMax =
                        new Vector2(0.5f, 0.5f);

                    rectStatus.pivot =
                        new Vector2(0.5f, 0.5f);

                    rectStatus.anchoredPosition =
                        Vector2.zero;

                    float ukuran = Mathf.Min(
                        ((RectTransform)tombol.transform)
                            .rect.width,
                        ((RectTransform)tombol.transform)
                            .rect.height
                    ) * 0.45f;

                    rectStatus.sizeDelta =
                        new Vector2(ukuran, ukuran);
                }
            }

            ikonStatus[i].enabled = true;
            ikonStatus[i].preserveAspect = true;
            ikonStatus[i].raycastTarget = false;

            // Ikon berada di atas angka.
            ikonStatus[i].transform.SetAsLastSibling();

            // Tidak perlu Sprite atau teks
            // secara manual sebelum Play.
            ikonStatus[i].gameObject.SetActive(false);

            // ----------------------------------
            // 4. EVENT KLIK ANGKA
            // ----------------------------------

            int indeks = i;

            aksiPilihan[i] =
                () => PilihAngka(indeks);

            tombol.onClick.RemoveListener(
                aksiPilihan[i]
            );

            tombol.onClick.AddListener(
                aksiPilihan[i]
            );
        }

        return true;
    }

    // ==========================================
    // MENGHUBUNGKAN TOMBOL HASIL
    // ==========================================

    private void HubungkanTombolHasil()
    {
        if (btnUlangi == null)
        {
            btnUlangi =
                CariTombolHasil("BTN_Ulangi");
        }

        if (btnMenuBermain == null)
        {
            btnMenuBermain =
                CariTombolHasil("BTN_MenuBermain");

            if (btnMenuBermain == null)
            {
                btnMenuBermain =
                    CariTombolHasil(
                        "BTN_KembaliMenuBermain"
                    );
            }
        }

        if (btnUlangi != null)
        {
            btnUlangi.onClick.RemoveListener(
                UlangiPermainan
            );

            btnUlangi.onClick.AddListener(
                UlangiPermainan
            );
        }
        else
        {
            Debug.LogError(
                "BTN_Ulangi tidak ditemukan. " +
                "Hubungkan di Inspector.",
                this
            );
        }

        if (btnMenuBermain != null)
        {
            btnMenuBermain.onClick.RemoveListener(
                KembaliMenuBermain
            );

            btnMenuBermain.onClick.AddListener(
                KembaliMenuBermain
            );
        }
        else
        {
            Debug.LogError(
                "BTN_MenuBermain tidak ditemukan. " +
                "Hubungkan di Inspector.",
                this
            );
        }
    }

    private Button CariTombolHasil(string nama)
    {
        Button[] semuaTombol =
            panelHasil.GetComponentsInChildren<Button>(true);

        foreach (Button tombol in semuaTombol)
        {
            if (tombol.name.Trim() == nama)
                return tombol;
        }

        return null;
    }

    // ==========================================
    // MULAI / ULANGI PERMAINAN
    // ==========================================

    public void MulaiPermainan()
    {
        Time.timeScale = 1f;

        StopAllCoroutines();

        nomorSoal = 0;
        soalTanpaKesalahan = 0;

        jumlahPrima = 0;
        jumlahBenarDipilih = 0;

        adaKesalahan = false;
        sedangTransisi = false;
        permainanSelesai = false;

        foreach (GameObject bintang in bintangEmas)
        {
            if (bintang != null)
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
    // PEMERIKSA BILANGAN PRIMA
    // ==========================================

    private bool ApakahPrima(int bilangan)
    {
        // 1 bukan bilangan prima.
        if (bilangan < 2)
            return false;

        for (int pembagi = 2;
             pembagi * pembagi <= bilangan;
             pembagi++)
        {
            if (bilangan % pembagi == 0)
                return false;
        }

        return true;
    }

    // ==========================================
    // MENAMPILKAN SOAL 1 SAMPAI 100
    // ==========================================

    private void TampilkanSoal()
    {
        sedangTransisi = false;
        adaKesalahan = false;

        jumlahPrima = 0;
        jumlahBenarDipilih = 0;

        int angkaAwal = nomorSoal * 10 + 1;
        int angkaAkhir = angkaAwal + 9;

        txtInstruksi.text =
            "Pilih semua bilangan prima!";

        // Angka petunjuk berwarna biru.
        txtPetunjuk.richText = true;

        txtPetunjuk.text =
            "Ketuk bilangan prima dari " +
            "<color=#258FFC>" +
            angkaAwal +
            "</color> sampai " +
            "<color=#258FFC>" +
            angkaAkhir +
            "</color>.";

        txtProgressSoal.text =
            "Soal " + (nomorSoal + 1) +
            "/" + JUMLAH_SOAL;

        // Angka ditampilkan berurutan.
        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            int angka = angkaAwal + i;

            angkaPilihan[i] = angka;

            sudahBenar[i] = false;
            sedangSalah[i] = false;

            pilihan[i].gameObject.SetActive(true);
            pilihan[i].interactable = true;

            // Angka diisi secara otomatis,
            // meskipun Text Input kosong
            // pada Inspector sebelumnya.
            teksPilihan[i].gameObject.SetActive(true);
            teksPilihan[i].enabled = true;

            teksPilihan[i].text =
                angka.ToString();

            // Reset ikon benar/salah.
            ikonStatus[i].gameObject.SetActive(false);

            if (ApakahPrima(angka))
                jumlahPrima++;
        }
    }

    // ==========================================
    // KETIKA ANGKA DIKETUK
    // ==========================================

    private void PilihAngka(int indeks)
    {
        if (sedangTransisi ||
            permainanSelesai ||
            sudahBenar[indeks] ||
            sedangSalah[indeks])
        {
            return;
        }

        int angka = angkaPilihan[indeks];

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        if (ApakahPrima(angka))
        {
            JawabanBenar(indeks);
        }
        else
        {
            JawabanSalah(indeks);
        }
    }

    // ==========================================
    // JAWABAN BENAR
    // ==========================================

    private void JawabanBenar(int indeks)
    {
        sudahBenar[indeks] = true;

        ikonStatus[indeks].sprite = ikonBenar;
        ikonStatus[indeks].color = Color.white;
        ikonStatus[indeks].enabled = true;

        ikonStatus[indeks].gameObject.SetActive(true);

        // Tidak bisa dihitung dua kali.
        pilihan[indeks].interactable = false;

        jumlahBenarDipilih++;

        // Lanjut jika seluruh bilangan prima
        // pada soal sudah ditemukan.
        if (jumlahBenarDipilih >= jumlahPrima)
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
    // JAWABAN SALAH
    // ==========================================

    private void JawabanSalah(int indeks)
    {
        adaKesalahan = true;
        sedangSalah[indeks] = true;

        ikonStatus[indeks].sprite = ikonSalah;
        ikonStatus[indeks].color = Color.white;
        ikonStatus[indeks].enabled = true;

        ikonStatus[indeks].gameObject.SetActive(true);

        StartCoroutine(
            HilangkanIkonSalah(
                indeks,
                nomorSoal
            )
        );
    }

    private IEnumerator HilangkanIkonSalah(
        int indeks,
        int nomorSaatSalah)
    {
        yield return new WaitForSeconds(
            durasiIkonSalah
        );

        if (nomorSaatSalah != nomorSoal)
            yield break;

        ikonStatus[indeks].gameObject.SetActive(false);

        sedangSalah[indeks] = false;
    }

    // ==========================================
    // LANJUT OTOMATIS
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
    // HASIL AKHIR
    // ==========================================

    private void TampilkanHasil()
    {
        permainanSelesai = true;
        sedangTransisi = true;

        // Setiap soal tanpa kesalahan
        // memperoleh 10 poin.
        int skor = soalTanpaKesalahan * 10;

        int jumlahBintang =
            skor >= 90 ? 3 :
            skor >= 70 ? 2 : 1;

        // Hanya angka karena label
        // Skor: sudah ada di Panel_Hasil.
        txtNilaiSkor.text = skor.ToString();

        // Tampilkan bintang sesi ini.
        for (int i = 0; i < bintangEmas.Length; i++)
        {
            if (bintangEmas[i] != null)
            {
                bintangEmas[i].SetActive(
                    i < jumlahBintang
                );
            }
        }

        // Simpan bintang tertinggi
        // untuk cover MenuBermain.
        int rekorLama =
            PlayerPrefs.GetInt(
                KUNCI_BINTANG, 0
            );

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
                "Scene MenuBermain belum ada " +
                "di Build Profiles > Scene List.",
                this
            );

            return;
        }

        Time.timeScale = 1f;

        SceneManager.LoadScene("MenuBermain");
    }

    // ==========================================
    // BERSIHKAN EVENT
    // ==========================================

    private void OnDestroy()
    {
        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            if (pilihan != null &&
                i < pilihan.Length &&
                pilihan[i] != null &&
                aksiPilihan[i] != null)
            {
                pilihan[i].onClick.RemoveListener(
                    aksiPilihan[i]
                );
            }
        }

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
