using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MencariFPBManager : MonoBehaviour
{
    [Header("Panel_Soal")]
    [SerializeField] private TMP_Text txtInstruksi;
    [SerializeField] private TMP_Text txtPetunjuk;
    [SerializeField] private TMP_Text txtProgressSoal;

    [Header("Label di kiri baris (tetap gunakan objek lama)")]
    [SerializeField] private TMP_Text txtLabelKelipatanA;
    [SerializeField] private TMP_Text txtLabelKelipatanB;

    [Header("Dua Area_Pilihan: biru lalu oranye")]
    [SerializeField] private Transform areaPilihanA;
    [SerializeField] private Transform areaPilihanB;

    [Header("Kotak Hasil Hijau")]
    [SerializeField] private TMP_Text txtLabelHasil;
    [SerializeField] private TMP_Text txtHasilFPB;

    [Header("Panel_Hasil")]
    [SerializeField] private GameObject panelHasil;
    [SerializeField] private TMP_Text txtNilaiSkor;
    [SerializeField] private GameObject[] bintangEmas = new GameObject[3];
    [SerializeField] private Button btnUlangi;
    [SerializeField] private Button btnMenuBermain;

    [Header("Neon (tanpa ikon centang/silang)")]
    [SerializeField] private Color neonBarisA = new Color(0.12f, 1f, 1f, 1f);
    [SerializeField] private Color neonBarisB = new Color(1f, 0.94f, 0.12f, 1f);
    [SerializeField] private Color neonBenar = new Color(0.15f, 1f, 0.45f, 1f);
    [SerializeField] private Color neonSalah = new Color(1f, 0.15f, 0.25f, 1f);
    [SerializeField, Range(1f, 20f)] private float neonLuar = 9f;
    [SerializeField, Range(1f, 12f)] private float neonDalam = 4f;

    [Header("Waktu")]
    [SerializeField, Min(0.1f)] private float jedaSalah = 0.7f;
    [SerializeField, Min(0.1f)] private float jedaAntarSoal = 0.9f;

    private const int JUMLAH_KOTAK = 6;
    private const int JUMLAH_SOAL = 10;
    private const string KEY_BINTANG = "MathQuest_MencariFPB_Bintang";
    private const string PETUNJUK = "Ketuk faktor terbesar yang sama pada kedua baris.";

    // Semua bilangan dalam bank ini mempunyai tepat enam faktor positif.
    // Urutan pasangan/soal diacak untuk setiap sesi; tidak ada soal berulang.
    private static readonly Vector2Int[] BANK_SOAL =
    {
        new Vector2Int(12, 18), // FPB 6
        new Vector2Int(12, 20), // 4
        new Vector2Int(12, 32), // 4
        new Vector2Int(12, 45), // 3
        new Vector2Int(18, 20), // 2
        new Vector2Int(18, 45), // 9
        new Vector2Int(18, 63), // 9
        new Vector2Int(18, 50), // 2
        new Vector2Int(20, 28), // 4
        new Vector2Int(20, 50), // 10
        new Vector2Int(20, 75), // 5
        new Vector2Int(28, 32), // 4
        new Vector2Int(28, 44), // 4
        new Vector2Int(28, 98), // 14
        new Vector2Int(32, 44), // 4
        new Vector2Int(32, 68), // 4
        new Vector2Int(44, 99), // 11
        new Vector2Int(44, 52), // 4
        new Vector2Int(45, 63), // 9
        new Vector2Int(45, 75), // 15
        new Vector2Int(50, 75), // 25
        new Vector2Int(52, 68), // 4
        new Vector2Int(52, 76), // 4
        new Vector2Int(63, 99), // 9
        new Vector2Int(68, 92), // 4
        new Vector2Int(75, 99), // 3
        new Vector2Int(76, 92), // 4
        new Vector2Int(63, 98)  // 7
    };

    private readonly List<Vector2Int> urutanSoal = new List<Vector2Int>();
    private readonly Button[,] tombol = new Button[2, JUMLAH_KOTAK];
    private readonly TMP_Text[,] teksAngka = new TMP_Text[2, JUMLAH_KOTAK];
    private readonly Outline[,] outlineLuar = new Outline[2, JUMLAH_KOTAK];
    private readonly Outline[,] outlineDalam = new Outline[2, JUMLAH_KOTAK];
    private readonly UnityAction[,] aksiKlik = new UnityAction[2, JUMLAH_KOTAK];
    private readonly int[,] angkaKotak = new int[2, JUMLAH_KOTAK];
    private readonly int[] pilihanIndex = { -1, -1 };

    private int nomorSoal;
    private int skor;
    private int jawabanFPB;
    private bool pernahSalah;
    private bool sedangMemeriksa;
    private bool selesai;
    private bool siap;

    private void Start()
    {
        if (!ValidasiInspector() || !SiapkanKotak())
        {
            Debug.LogError("MencariFPBManager: referensi atau struktur kotak belum lengkap.", this);
            enabled = false;
            return;
        }

        if (btnUlangi == null) btnUlangi = CariTombolHasil("BTN_Ulangi", "Ulangi");
        if (btnMenuBermain == null) btnMenuBermain = CariTombolHasil("BTN_MenuBermain", "BTN_KembaliMenuBermain", "MenuBermain");

        if (btnUlangi != null) btnUlangi.onClick.AddListener(UlangiPermainan);
        else Debug.LogWarning("MencariFPB: tombol Ulangi belum dihubungkan.", this);

        if (btnMenuBermain != null) btnMenuBermain.onClick.AddListener(KembaliMenuBermain);
        else Debug.LogWarning("MencariFPB: tombol MenuBermain belum dihubungkan.", this);

        siap = true;
        MulaiPermainan();
    }

    private bool ValidasiInspector()
    {
        if (txtInstruksi == null || txtPetunjuk == null || txtProgressSoal == null ||
            txtLabelKelipatanA == null || txtLabelKelipatanB == null ||
            areaPilihanA == null || areaPilihanB == null ||
            txtHasilFPB == null || panelHasil == null || txtNilaiSkor == null)
        {
            Debug.LogError("MencariFPB: ada field wajib yang kosong di Inspector.", this);
            return false;
        }
        if (areaPilihanA == areaPilihanB)
        {
            Debug.LogError("MencariFPB: kedua Area_Pilihan harus berbeda.", this);
            return false;
        }
        if (bintangEmas == null || bintangEmas.Length != 3)
        {
            Debug.LogError("MencariFPB: Bintang Emas harus Size 3.", this);
            return false;
        }
        for (int i = 0; i < 3; i++)
        {
            if (bintangEmas[i] == null)
            {
                Debug.LogError("MencariFPB: isi ketiga objek bintang emas.", this);
                return false;
            }
        }
        return true;
    }

    private Transform CariAnak(Transform induk, string nama)
    {
        foreach (Transform anak in induk.GetComponentsInChildren<Transform>(true))
            if (anak != induk && anak.name == nama) return anak;
        return null;
    }

    private bool SiapkanKotak()
    {
        Transform[] area = { areaPilihanA, areaPilihanB };
        for (int baris = 0; baris < 2; baris++)
        {
            for (int i = 0; i < JUMLAH_KOTAK; i++)
            {
                string nama = "Pilihan_" + (i + 1).ToString("00");
                Transform objek = area[baris].Find(nama);
                if (objek == null)
                {
                    Debug.LogError("Tidak ditemukan " + nama + " di " + area[baris].name, this);
                    return false;
                }

                Button b = objek.GetComponent<Button>();
                Image gambar = objek.GetComponent<Image>();
                Transform textTransform = CariAnak(objek, "TXT_Angka");
                TMP_Text t = textTransform == null ? null : textTransform.GetComponent<TMP_Text>();
                if (b == null || gambar == null || t == null)
                {
                    Debug.LogError(nama + " harus memiliki Button, Image, dan child TXT_Angka.", objek);
                    return false;
                }

                tombol[baris, i] = b;
                teksAngka[baris, i] = t;

                // Jangan ubah warna sprite asli saat diklik.
                b.transition = Selectable.Transition.None;
                t.gameObject.SetActive(true);
                t.raycastTarget = false;
                PusatkanTeks(t);

                // Matikan ikon lama dari scene yang diduplikasi.
                Transform status = CariAnak(objek, "Icon_Status");
                if (status != null) status.gameObject.SetActive(false);

                // Dua lapisan Outline, agar neon langsung terlihat pada klik pertama.
                outlineLuar[baris, i] = objek.gameObject.AddComponent<Outline>();
                outlineLuar[baris, i].useGraphicAlpha = false;
                outlineLuar[baris, i].enabled = false;

                outlineDalam[baris, i] = objek.gameObject.AddComponent<Outline>();
                outlineDalam[baris, i].useGraphicAlpha = false;
                outlineDalam[baris, i].enabled = false;

                int barisLokal = baris;
                int indeksLokal = i;
                aksiKlik[baris, i] = () => KetukAngka(barisLokal, indeksLokal);
                b.onClick.AddListener(aksiKlik[baris, i]);
            }
        }
        PusatkanTeks(txtHasilFPB);
        return true;
    }

    // Menyesuaikan teks secara otomatis ke ukuran kotak yang sudah ada.
    private void PusatkanTeks(TMP_Text teks)
    {
        if (teks == null) return;
        RectTransform rect = teks.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        teks.alignment = TextAlignmentOptions.Center;
        teks.margin = Vector4.zero;
        teks.raycastTarget = false;
        teks.enableAutoSizing = true;
        teks.fontSizeMin = 16f;
        teks.fontSizeMax = Mathf.Max(16f, teks.fontSize);
    }

    private Button CariTombolHasil(params string[] namaNama)
    {
        foreach (Button b in panelHasil.GetComponentsInChildren<Button>(true))
            foreach (string nama in namaNama)
                if (b.name == nama) return b;
        return null;
    }

    private void SetNeon(int baris, int indeks, Color warna, bool aktif)
    {
        Outline luar = outlineLuar[baris, indeks];
        Outline dalam = outlineDalam[baris, indeks];
        if (luar == null || dalam == null) return;

        luar.effectColor = new Color(warna.r, warna.g, warna.b, 0.65f);
        luar.effectDistance = new Vector2(neonLuar, neonLuar);
        luar.enabled = aktif;

        dalam.effectColor = warna;
        dalam.effectDistance = new Vector2(neonDalam, neonDalam);
        dalam.enabled = aktif;
    }

    private List<int> CariFaktor(int bilangan)
    {
        List<int> hasil = new List<int>();
        for (int i = 1; i <= bilangan; i++)
            if (bilangan % i == 0) hasil.Add(i);
        return hasil;
    }

    private int HitungFPB(int a, int b)
    {
        while (b != 0)
        {
            int sisa = a % b;
            a = b;
            b = sisa;
        }
        return a;
    }

    public void MulaiPermainan()
    {
        if (!siap) return;

        StopAllCoroutines();
        Time.timeScale = 1f;
        urutanSoal.Clear();
        urutanSoal.AddRange(BANK_SOAL);

        // Fisher-Yates: ambil 10 pasangan berbeda secara acak.
        for (int i = urutanSoal.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2Int sementara = urutanSoal[i];
            urutanSoal[i] = urutanSoal[j];
            urutanSoal[j] = sementara;
        }

        nomorSoal = 0;
        skor = 0;
        selesai = false;
        sedangMemeriksa = false;
        panelHasil.SetActive(false);
        foreach (GameObject bintang in bintangEmas) bintang.SetActive(false);
        TampilkanSoal();
    }

    private void TampilkanSoal()
    {
        Vector2Int soal = urutanSoal[nomorSoal];
        // Acak juga sisi A/B tanpa mengubah pasangan matematikanya.
        if (Random.Range(0, 2) == 1)
            soal = new Vector2Int(soal.y, soal.x);

        List<int> faktorA = CariFaktor(soal.x);
        List<int> faktorB = CariFaktor(soal.y);

        if (faktorA.Count != JUMLAH_KOTAK || faktorB.Count != JUMLAH_KOTAK)
        {
            Debug.LogError("Bank soal salah: " + soal.x + " atau " + soal.y +
                           " tidak memiliki tepat 6 faktor.", this);
            enabled = false;
            return;
        }

        jawabanFPB = HitungFPB(soal.x, soal.y);
        pilihanIndex[0] = -1;
        pilihanIndex[1] = -1;
        pernahSalah = false;
        sedangMemeriksa = false;

        txtInstruksi.richText = true;
        txtInstruksi.text = "Cari FPB dari <color=#258FFC>" + soal.x +
            "</color> dan <color=#258FFC>" + soal.y + "</color>!";
        txtPetunjuk.text = PETUNJUK;
        txtProgressSoal.text = "Soal " + (nomorSoal + 1) + "/" + JUMLAH_SOAL;

        // Nama GameObject boleh tetap TXT_LabelKelipatanA/B,
        // tetapi isinya kini menunjukkan bilangan yang dicari faktornya.
        txtLabelKelipatanA.text = soal.x + " =";
        txtLabelKelipatanB.text = soal.y + " =";

        if (txtLabelHasil != null) txtLabelHasil.text = "Hasil =";
        txtHasilFPB.text = "?";

        for (int baris = 0; baris < 2; baris++)
        {
            List<int> daftar = baris == 0 ? faktorA : faktorB;
            for (int i = 0; i < JUMLAH_KOTAK; i++)
            {
                angkaKotak[baris, i] = daftar[i];
                teksAngka[baris, i].text = daftar[i].ToString();
                tombol[baris, i].interactable = true;
                SetNeon(baris, i, Color.white, false);
                Transform status = CariAnak(tombol[baris, i].transform, "Icon_Status");
                if (status != null) status.gameObject.SetActive(false);
            }
        }
    }

    private void KetukAngka(int baris, int indeks)
    {
        if (selesai || sedangMemeriksa) return;

        int lama = pilihanIndex[baris];
        if (lama >= 0) SetNeon(baris, lama, Color.white, false);

        // Klik kotak yang sama untuk batal memilih.
        if (lama == indeks)
        {
            pilihanIndex[baris] = -1;
            return;
        }

        pilihanIndex[baris] = indeks;
        // Langsung neon sejak klik PERTAMA, tidak menunggu pilihan kedua.
        SetNeon(baris, indeks, baris == 0 ? neonBarisA : neonBarisB, true);

        if (pilihanIndex[0] >= 0 && pilihanIndex[1] >= 0)
            StartCoroutine(PeriksaJawaban());
    }

    private IEnumerator PeriksaJawaban()
    {
        sedangMemeriksa = true;
        int a = pilihanIndex[0];
        int b = pilihanIndex[1];
        int nilaiA = angkaKotak[0, a];
        int nilaiB = angkaKotak[1, b];

        bool benar = nilaiA == nilaiB && nilaiA == jawabanFPB;
        if (benar)
        {
            SetNeon(0, a, neonBenar, true);
            SetNeon(1, b, neonBenar, true);
            txtHasilFPB.text = jawabanFPB.ToString();
            txtPetunjuk.text = "Benar! FPB-nya adalah " + jawabanFPB + ".";

            if (!pernahSalah) skor += 10;

            for (int baris = 0; baris < 2; baris++)
                for (int i = 0; i < JUMLAH_KOTAK; i++)
                    tombol[baris, i].interactable = false;

            yield return new WaitForSeconds(jedaAntarSoal);
            nomorSoal++;
            if (nomorSoal >= JUMLAH_SOAL) TampilkanHasil();
            else TampilkanSoal();
        }
        else
        {
            pernahSalah = true;
            SetNeon(0, a, neonSalah, true);
            SetNeon(1, b, neonSalah, true);
            txtPetunjuk.text = "Belum tepat. Cari faktor yang sama dan paling besar.";
            yield return new WaitForSeconds(jedaSalah);

            SetNeon(0, a, Color.white, false);
            SetNeon(1, b, Color.white, false);
            pilihanIndex[0] = -1;
            pilihanIndex[1] = -1;
            txtPetunjuk.text = PETUNJUK;
            sedangMemeriksa = false;
        }
    }

    private void TampilkanHasil()
    {
        selesai = true;
        sedangMemeriksa = true;
        txtNilaiSkor.text = skor.ToString(); // Label "Skor:" sudah di panel.

        int jumlahBintang = skor >= 90 ? 3 : skor >= 70 ? 2 : 1;
        for (int i = 0; i < 3; i++) bintangEmas[i].SetActive(i < jumlahBintang);

        int terbaik = PlayerPrefs.GetInt(KEY_BINTANG, 0);
        if (jumlahBintang > terbaik)
        {
            PlayerPrefs.SetInt(KEY_BINTANG, jumlahBintang);
            PlayerPrefs.Save();
        }

        panelHasil.SetActive(true);
    }

    public void UlangiPermainan()
    {
        MulaiPermainan();
    }

    public void KembaliMenuBermain()
    {
        Time.timeScale = 1f;
        if (!Application.CanStreamedLevelBeLoaded("MenuBermain"))
        {
            Debug.LogError("Tambahkan scene MenuBermain ke Build Profiles > Scene List.", this);
            return;
        }
        SceneManager.LoadScene("MenuBermain");
    }

    private void OnDestroy()
    {
        for (int baris = 0; baris < 2; baris++)
            for (int i = 0; i < JUMLAH_KOTAK; i++)
                if (tombol[baris, i] != null && aksiKlik[baris, i] != null)
                    tombol[baris, i].onClick.RemoveListener(aksiKlik[baris, i]);

        if (btnUlangi != null) btnUlangi.onClick.RemoveListener(UlangiPermainan);
        if (btnMenuBermain != null) btnMenuBermain.onClick.RemoveListener(KembaliMenuBermain);
    }
}
