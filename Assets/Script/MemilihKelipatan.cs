
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MemilihKelipatanV2 : MonoBehaviour
{
    [Header("Soal")]
    [SerializeField] private TMP_Text txtInstruksi;
    [SerializeField] private TMP_Text txtProgressSoal;
    [SerializeField] private Button[] pilihan = new Button[8];

    [Header("Ikon Jawaban")]
    [SerializeField] private Sprite ikonBenar;
    [SerializeField] private Sprite ikonSalah;

    [Header("Warna Kotak Saat Diklik")]
    [SerializeField] private Sprite kotakBenar;
    [SerializeField] private Sprite kotakSalah;

    [Header("Hasil Permainan")]
    [SerializeField] private GameObject panelHasil;
    [SerializeField] private TMP_Text txtNilaiSkor;
    [SerializeField] private GameObject[] bintangEmas =
        new GameObject[3];

    [Header("Tombol")]
    [SerializeField] private Button btnMenuBermain;
    [SerializeField] private Button btnUlangi;

    [Header("Pengaturan")]
    [SerializeField] private int jumlahSoal = 10;
    [SerializeField] private int angkaMaksimum = 60;
    [SerializeField] private float jedaSoal = 0.8f;

    private TMP_Text[] teksAngka;
    private Image[] ikonStatus;
    private Sprite[] spriteAsli;
    private Color[] warnaAsli;
    private bool[] sudahDipilih;

    private int soalKe;
    private int target;
    private int benarTersisa;
    private int totalSalah;

    private bool sedangTransisi;

    private List<int> urutanTarget = new List<int>();

    // ======================================
    // INISIALISASI
    // ======================================

    private void Start()
    {
        if (!SiapkanKomponen())
        {
            enabled = false;
            return;
        }

        for (int i = 0; i < pilihan.Length; i++)
        {
            int index = i;

            pilihan[i].onClick.AddListener(
                () => PilihAngka(index)
            );
        }

        btnUlangi.onClick.AddListener(MulaiPermainan);

        btnMenuBermain.onClick.AddListener(
            KembaliMenuBermain
        );

        MulaiPermainan();
    }

    // ======================================
    // MENCARI CHILD TERMASUK NONAKTIF
    // ======================================

    private Transform CariAnak(
        Transform induk,
        string nama
    )
    {
        Transform[] semuaAnak =
            induk.GetComponentsInChildren<Transform>(true);

        foreach (Transform anak in semuaAnak)
        {
            if (anak != induk && anak.name.Trim() == nama)
                return anak;
        }

        return null;
    }

    // ======================================
    // VALIDASI INSPECTOR
    // ======================================

    private bool SiapkanKomponen()
    {
        if (txtInstruksi == null ||
            txtProgressSoal == null ||
            ikonBenar == null ||
            ikonSalah == null ||
            kotakBenar == null ||
            kotakSalah == null ||
            panelHasil == null ||
            txtNilaiSkor == null ||
            btnMenuBermain == null ||
            btnUlangi == null)
        {
            Debug.LogError(
                "GameManager: Referensi Inspector belum lengkap."
            );

            return false;
        }

        if (pilihan == null || pilihan.Length != 8 ||
            bintangEmas == null || bintangEmas.Length != 3)
        {
            Debug.LogError(
                "Harus ada 8 pilihan dan 3 bintang."
            );

            return false;
        }

        if (jumlahSoal < 1)
        {
            Debug.LogError(
                "Jumlah soal harus minimal 1."
            );

            return false;
        }

        int batasTarget = Mathf.Min(
            12,
            angkaMaksimum / 5
        );

        int jumlahTargetTersedia = batasTarget - 1;

        if (jumlahTargetTersedia < jumlahSoal)
        {
            Debug.LogError(
                "Target kelipatan unik tidak cukup untuk " +
                jumlahSoal + " soal. " +
                "Atur Angka Maksimum menjadi 60 " +
                "untuk 10 soal tanpa pengulangan."
            );

            return false;
        }

        foreach (GameObject bintang in bintangEmas)
        {
            if (bintang == null)
            {
                Debug.LogError(
                    "Referensi bintang belum lengkap."
                );

                return false;
            }
        }

        teksAngka = new TMP_Text[8];
        ikonStatus = new Image[8];
        spriteAsli = new Sprite[8];
        warnaAsli = new Color[8];
        sudahDipilih = new bool[8];

        for (int i = 0; i < 8; i++)
        {
            if (pilihan[i] == null ||
                pilihan[i].image == null)
            {
                Debug.LogError(
                    "Button pilihan ke-" + (i + 1) +
                    " belum lengkap."
                );

                return false;
            }

            Transform angka = CariAnak(
                pilihan[i].transform,
                "TXT_Angka"
            );

            Transform status = CariAnak(
                pilihan[i].transform,
                "Icon_Status"
            );

            if (angka == null || status == null)
            {
                Debug.LogError(
                    "TXT_Angka atau Icon_Status tidak ada pada " +
                    pilihan[i].name
                );

                return false;
            }

            teksAngka[i] =
                angka.GetComponent<TMP_Text>();

            ikonStatus[i] =
                status.GetComponent<Image>();

            if (teksAngka[i] == null ||
                ikonStatus[i] == null)
            {
                Debug.LogError(
                    "Komponen TMP atau Image tidak ditemukan pada " +
                    pilihan[i].name
                );

                return false;
            }

            spriteAsli[i] = pilihan[i].image.sprite;
            warnaAsli[i] = pilihan[i].image.color;

            pilihan[i].transition =
                Selectable.Transition.None;

            teksAngka[i].raycastTarget = false;
            ikonStatus[i].raycastTarget = false;

            ikonStatus[i].gameObject.SetActive(false);
        }

        return true;
    }

    // ======================================
    // MENYIAPKAN 10 TARGET UNIK
    // ======================================

    private void SiapkanUrutanTarget()
    {
        urutanTarget.Clear();

        // Dengan angkaMaksimum = 60,
        // target tersedia dari 2 sampai 12.
        int batasTarget = Mathf.Min(
            12,
            angkaMaksimum / 5
        );

        for (int angka = 2; angka <= batasTarget; angka++)
        {
            urutanTarget.Add(angka);
        }

        // Acak sekali saat permainan dimulai.
        for (int i = urutanTarget.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            int sementara = urutanTarget[i];

            urutanTarget[i] = urutanTarget[j];
            urutanTarget[j] = sementara;
        }
    }

    // ======================================
    // MULAI / ULANGI PERMAINAN
    // ======================================

    public void MulaiPermainan()
    {
        StopAllCoroutines();

        soalKe = 1;
        totalSalah = 0;
        sedangTransisi = false;

        panelHasil.SetActive(false);

        foreach (GameObject bintang in bintangEmas)
        {
            bintang.SetActive(false);
        }

        SiapkanUrutanTarget();

        BuatSoal();
    }

    // ======================================
    // MEMBUAT SOAL TANPA TARGET BERULANG
    // ======================================

    private void BuatSoal()
    {
        sedangTransisi = false;

        // Mengambil target dari urutan unik.
        target = urutanTarget[soalKe - 1];

        txtInstruksi.text =
            "Pilih semua kelipatan dari " +
            "<color=#218DFF>" +
            target +
            "</color>";

        txtProgressSoal.text =
            "Soal " + soalKe + "/" + jumlahSoal;

        List<int> angkaPilihan = new List<int>();

        // Lima kelipatan yang benar.
        angkaPilihan.Add(target * 1);
        angkaPilihan.Add(target * 2);
        angkaPilihan.Add(target * 3);
        angkaPilihan.Add(target * 4);
        angkaPilihan.Add(target * 5);

        // Tiga angka pengecoh.
        angkaPilihan.Add(target * 2 - 1);
        angkaPilihan.Add(target * 4 - 1);
        angkaPilihan.Add(target * 5 - 1);

        // Menampilkan angka kecil ke besar.
        angkaPilihan.Sort();

        benarTersisa = 5;

        for (int i = 0; i < 8; i++)
        {
            sudahDipilih[i] = false;

            // Kembalikan warna asli kotak.
            pilihan[i].image.sprite = spriteAsli[i];
            pilihan[i].image.color = warnaAsli[i];

            pilihan[i].interactable = true;

            teksAngka[i].text =
                angkaPilihan[i].ToString();

            // Bersihkan ikon feedback.
            ikonStatus[i].sprite = null;
            ikonStatus[i].gameObject.SetActive(false);
        }
    }

    // ======================================
    // SISTEM KLIK + AUDIO + BENAR / SALAH
    // ======================================

    private void PilihAngka(int index)
    {
        // Tidak boleh memproses klik berulang.
        if (sedangTransisi || sudahDipilih[index])
            return;

        // ==================================
        // BARU: SOUND KLIK OTOMATIS
        // ==================================

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        // ==================================
        // AKHIR PENAMBAHAN AUDIO
        // ==================================

        sudahDipilih[index] = true;

        int angka = int.Parse(
            teksAngka[index].text
        );

        bool benar = angka % target == 0;

        // Kotak tidak boleh dipilih dua kali.
        pilihan[index].interactable = false;

        // Ganti gambar kotak berdasarkan jawaban.
        pilihan[index].image.sprite =
            benar ? kotakBenar : kotakSalah;

        pilihan[index].image.color = Color.white;

        // Tampilkan hanya satu ikon feedback.
        ikonStatus[index].gameObject.SetActive(false);

        ikonStatus[index].sprite =
            benar ? ikonBenar : ikonSalah;

        ikonStatus[index].color = Color.white;

        ikonStatus[index].gameObject.SetActive(true);

        // Jawaban benar.
        if (benar)
        {
            benarTersisa--;

            if (benarTersisa == 0)
            {
                sedangTransisi = true;

                StartCoroutine(
                    LanjutOtomatis()
                );
            }
        }
        // Jawaban salah.
        else
        {
            totalSalah++;
        }
    }

    // ======================================
    // PERPINDAHAN SOAL OTOMATIS
    // ======================================

    private IEnumerator LanjutOtomatis()
    {
        yield return new WaitForSeconds(jedaSoal);

        if (soalKe >= jumlahSoal)
        {
            TampilkanHasil();
        }
        else
        {
            soalKe++;
            BuatSoal();
        }
    }

    // ======================================
    // HASIL AKHIR
    // ======================================

    private void TampilkanHasil()
    {
        sedangTransisi = true;

        int skor = Mathf.Clamp(
            100 - totalSalah * 5,
            0,
            100
        );

        txtNilaiSkor.text = skor.ToString();

        // Sistem penilaian bintang.
        int jumlahBintang =
            skor >= 90 ? 3 :
            skor >= 70 ? 2 : 1;

        for (int i = 0; i < 3; i++)
        {
            bintangEmas[i].SetActive(
                i < jumlahBintang
            );
        }

        // Menyimpan hasil bintang terbaik.
        string kunci =
            "MathQuest_MemilihKelipatan_Bintang";

        int terbaik = PlayerPrefs.GetInt(
            kunci,
            0
        );

        if (jumlahBintang > terbaik)
        {
            PlayerPrefs.SetInt(
                kunci,
                jumlahBintang
            );

            PlayerPrefs.Save();
        }

        panelHasil.SetActive(true);
    }

    // ======================================
    // KEMBALI KE MENU BERMAIN
    // ======================================

    public void KembaliMenuBermain()
    {
        SceneManager.LoadScene("MenuBermain");
    }
}
