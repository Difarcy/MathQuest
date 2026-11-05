using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MencariKPKManager : MonoBehaviour
{
    [Header("Panel_Soal")]
    [SerializeField] private TMP_Text txtInstruksi;
    [SerializeField] private TMP_Text txtPetunjuk;
    [SerializeField] private TMP_Text txtProgressSoal;

    [Header("Label Kelipatan")]
    [SerializeField] private TMP_Text txtLabelKelipatanA;
    [SerializeField] private TMP_Text txtLabelKelipatanB;

    [Header("Dua Area_Pilihan (biru dan oranye)")]
    [SerializeField] private Transform areaKelipatanA;
    [SerializeField] private Transform areaKelipatanB;

    [Header("Area_HasilKPK")]
    [SerializeField] private TMP_Text txtLabelHasil;
    [SerializeField] private TMP_Text txtHasilKPK;

    [Header("Panel_Hasil")]
    [SerializeField] private GameObject panelHasil;
    [SerializeField] private TMP_Text txtNilaiSkor;
    [SerializeField] private GameObject[] bintangEmas = new GameObject[3];
    [SerializeField] private Button btnUlangi;
    [SerializeField] private Button btnMenuBermain;

    [Header("Efek Neon (tanpa ikon centang/silang)")]
    [SerializeField, Range(2f, 20f)] private float neonLuar = 9f;
    [SerializeField, Range(1f, 10f)] private float neonDalam = 3f;
    [SerializeField, Min(0.1f)] private float jedaJawabanSalah = 0.7f;
    [SerializeField, Min(0.1f)] private float jedaAntarSoal = 1f;

    private const int JumlahSoal = 10;
    private const int JumlahKotak = 6;
    private const string KunciBintang = "MathQuest_MencariKPK_Bintang";
    private const string PetunjukAwal =
        "Ketuk bilangan terkecil yang sama pada kedua baris.";

    // Seluruh soal memiliki KPK pada enam kelipatan pertama kedua baris.
    private static readonly Vector2Int[] BankSoal =
    {
        new Vector2Int(2, 3),   new Vector2Int(3, 4),
        new Vector2Int(4, 6),   new Vector2Int(4, 8),
        new Vector2Int(4, 5),   new Vector2Int(6, 8),
        new Vector2Int(6, 9),   new Vector2Int(8, 12),
        new Vector2Int(9, 12),  new Vector2Int(10, 12),
        new Vector2Int(12, 15), new Vector2Int(5, 6),
        new Vector2Int(6, 10),  new Vector2Int(8, 10),
        new Vector2Int(9, 15),  new Vector2Int(10, 15),
        new Vector2Int(12, 18), new Vector2Int(15, 20),
        new Vector2Int(14, 21), new Vector2Int(18, 24),
        new Vector2Int(5, 10),  new Vector2Int(7, 14),
        new Vector2Int(8, 16),  new Vector2Int(11, 22),
        new Vector2Int(5, 15)
    };

    // Neon biru/cyan saat baris atas dipilih, neon kuning saat bawah.
    private static readonly Color NeonBiru = new Color(0.10f, 0.95f, 1f, 1f);
    private static readonly Color NeonKuning = new Color(1f, 0.92f, 0.10f, 1f);
    private static readonly Color NeonBenar = new Color(0.12f, 1f, 0.43f, 1f);
    private static readonly Color NeonSalah = new Color(1f, 0.16f, 0.25f, 1f);

    private readonly Button[,] tombol = new Button[2, JumlahKotak];
    private readonly TMP_Text[,] teksAngka = new TMP_Text[2, JumlahKotak];
    private readonly Image[,] gambar = new Image[2, JumlahKotak];
    private readonly Outline[,] garisLuar = new Outline[2, JumlahKotak];
    private readonly Outline[,] garisDalam = new Outline[2, JumlahKotak];
    private readonly UnityAction[,] pendengar = new UnityAction[2, JumlahKotak];
    private readonly List<Vector2Int> urutanSoal = new List<Vector2Int>();
    private readonly int[] bilangan = new int[2];
    private readonly int[] pilihanAktif = { -1, -1 };

    private int nomorSoal;
    private int kpk;
    private int skor;
    private bool pernahSalah;
    private bool sedangMemeriksa;
    private bool sudahSelesai;
    private bool siap;

    private void Start()
    {
        if (!PeriksaInspector() || !SiapkanPilihan())
        {
            enabled = false;
            return;
        }

        if (btnUlangi != null)
            btnUlangi.onClick.AddListener(UlangiPermainan);

        if (btnMenuBermain != null)
            btnMenuBermain.onClick.AddListener(KembaliMenuBermain);

        siap = true;
        MulaiPermainan();
    }

    private bool PeriksaInspector()
    {
        if (txtInstruksi == null || txtPetunjuk == null ||
            txtProgressSoal == null || txtLabelKelipatanA == null ||
            txtLabelKelipatanB == null || areaKelipatanA == null ||
            areaKelipatanB == null || txtHasilKPK == null ||
            panelHasil == null || txtNilaiSkor == null)
        {
            Debug.LogError("MencariKPKManager: referensi Inspector belum lengkap.", this);
            return false;
        }

        if (areaKelipatanA == areaKelipatanB)
        {
            Debug.LogError("Area Kelipatan A dan B harus berbeda.", this);
            return false;
        }

        return true;
    }

    private bool SiapkanPilihan()
    {
        Transform[] area = { areaKelipatanA, areaKelipatanB };

        for (int baris = 0; baris < 2; baris++)
        {
            for (int i = 0; i < JumlahKotak; i++)
            {
                string nama = "Pilihan_" + (i + 1).ToString("00");
                Transform objek = area[baris].Find(nama);
                if (objek == null)
                {
                    Debug.LogError("Tidak ditemukan " + nama + " pada " + area[baris].name, this);
                    return false;
                }

                Button b = objek.GetComponent<Button>();
                Image img = objek.GetComponent<Image>();
                Transform label = CariAnak(objek, "TXT_Angka");
                TMP_Text teks = label == null ? null : label.GetComponent<TMP_Text>();

                if (b == null || img == null || teks == null)
                {
                    Debug.LogError(nama + " perlu Button, Image, dan TXT_Angka.", objek);
                    return false;
                }

                tombol[baris, i] = b;
                gambar[baris, i] = img;
                teksAngka[baris, i] = teks;

                // Warna asli dan bentuk Sprite tidak diubah ketika dipilih.
                b.transition = Selectable.Transition.None;
                img.raycastTarget = true;
                teks.gameObject.SetActive(true);
                RapikanAngka(teks);

                // Ikon_Status lama tidak digunakan sama sekali.
                Transform ikon = CariAnak(objek, "Icon_Status");
                if (ikon != null)
                    ikon.gameObject.SetActive(false);

                // Efek Outline neon dipasang otomatis ke Image Button.
                Outline luar = objek.gameObject.AddComponent<Outline>();
                luar.useGraphicAlpha = false;
                luar.effectDistance = new Vector2(neonLuar, neonLuar);
                luar.enabled = false;

                Outline dalam = objek.gameObject.AddComponent<Outline>();
                dalam.useGraphicAlpha = false;
                dalam.effectDistance = new Vector2(neonDalam, neonDalam);
                dalam.enabled = false;

                garisLuar[baris, i] = luar;
                garisDalam[baris, i] = dalam;

                int barisLokal = baris;
                int indeksLokal = i;
                pendengar[baris, i] = () => KetukAngka(barisLokal, indeksLokal);
                b.onClick.AddListener(pendengar[baris, i]);
            }
        }

        RapikanAngka(txtHasilKPK);
        return true;
    }

    private Transform CariAnak(Transform induk, string nama)
    {
        foreach (Transform anak in induk.GetComponentsInChildren<Transform>(true))
        {
            if (anak != induk && anak.name == nama)
                return anak;
        }
        return null;
    }

    private void RapikanAngka(TMP_Text teks)
    {
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

    private void AturNeon(int baris, int indeks, bool hidup, Color warna)
    {
        Outline luar = garisLuar[baris, indeks];
        Outline dalam = garisDalam[baris, indeks];
        if (luar == null || dalam == null)
            return;

        if (hidup)
        {
            luar.effectColor = new Color(warna.r, warna.g, warna.b, 0.45f);
            dalam.effectColor = new Color(warna.r, warna.g, warna.b, 1f);
            luar.effectDistance = new Vector2(neonLuar, neonLuar);
            dalam.effectDistance = new Vector2(neonDalam, neonDalam);
        }
        luar.enabled = hidup;
        dalam.enabled = hidup;
    }

    private void MatikanSemuaNeon()
    {
        for (int baris = 0; baris < 2; baris++)
            for (int i = 0; i < JumlahKotak; i++)
                AturNeon(baris, i, false, Color.white);
    }

    public void MulaiPermainan()
    {
        if (!siap) return;

        StopAllCoroutines();
        Time.timeScale = 1f;
        urutanSoal.Clear();
        urutanSoal.AddRange(BankSoal);

        for (int i = urutanSoal.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2Int sementara = urutanSoal[i];
            urutanSoal[i] = urutanSoal[j];
            urutanSoal[j] = sementara;
        }

        nomorSoal = 0;
        skor = 0;
        sedangMemeriksa = false;
        sudahSelesai = false;

        panelHasil.SetActive(false);
        if (bintangEmas != null)
        {
            foreach (GameObject bintang in bintangEmas)
                if (bintang != null) bintang.SetActive(false);
        }

        TampilkanSoal();
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

    private void TampilkanSoal()
    {
        Vector2Int soal = urutanSoal[nomorSoal];
        bilangan[0] = soal.x;
        bilangan[1] = soal.y;
        kpk = soal.x / HitungFPB(soal.x, soal.y) * soal.y;

        pilihanAktif[0] = -1;
        pilihanAktif[1] = -1;
        pernahSalah = false;
        sedangMemeriksa = false;

        txtInstruksi.richText = true;
        txtInstruksi.text = "Cari KPK dari <color=#258FFC>" + soal.x +
            "</color> dan <color=#258FFC>" + soal.y + "</color>!";
        txtPetunjuk.text = PetunjukAwal;
        txtProgressSoal.text = "Soal " + (nomorSoal + 1) + "/" + JumlahSoal;
        txtLabelKelipatanA.text = soal.x + " =";
        txtLabelKelipatanB.text = soal.y + " =";

        if (txtLabelHasil != null)
            txtLabelHasil.text = "Hasil KPK";
        txtHasilKPK.text = "?";

        MatikanSemuaNeon();
        for (int baris = 0; baris < 2; baris++)
        {
            for (int i = 0; i < JumlahKotak; i++)
            {
                teksAngka[baris, i].text = (bilangan[baris] * (i + 1)).ToString();
                tombol[baris, i].interactable = true;
                Transform ikon = CariAnak(tombol[baris, i].transform, "Icon_Status");
                if (ikon != null) ikon.gameObject.SetActive(false);
            }
        }
    }

    private void KetukAngka(int baris, int indeks)
    {
        if (sudahSelesai || sedangMemeriksa)
            return;

        // Saat klik PERTAMA langsung muncul neon.
        int sebelumnya = pilihanAktif[baris];
        if (sebelumnya == indeks)
        {
            AturNeon(baris, indeks, false, Color.white);
            pilihanAktif[baris] = -1;
            return;
        }

        if (sebelumnya >= 0)
            AturNeon(baris, sebelumnya, false, Color.white);

        pilihanAktif[baris] = indeks;
        AturNeon(baris, indeks, true, baris == 0 ? NeonBiru : NeonKuning);

        // Tidak ada ikon/centang, baik di klik pertama maupun kedua.
        if (pilihanAktif[0] >= 0 && pilihanAktif[1] >= 0)
            StartCoroutine(PeriksaJawaban());
    }

    private IEnumerator PeriksaJawaban()
    {
        sedangMemeriksa = true;

        int indeksA = pilihanAktif[0];
        int indeksB = pilihanAktif[1];
        int nilaiA = bilangan[0] * (indeksA + 1);
        int nilaiB = bilangan[1] * (indeksB + 1);
        bool benar = nilaiA == nilaiB && nilaiA == kpk;

        if (benar)
        {
            AturNeon(0, indeksA, true, NeonBenar);
            AturNeon(1, indeksB, true, NeonBenar);

            txtHasilKPK.text = kpk.ToString();
            txtPetunjuk.text = "Benar! KPK-nya adalah " + kpk + ".";

            if (!pernahSalah)
                skor += 10;

            for (int baris = 0; baris < 2; baris++)
                for (int i = 0; i < JumlahKotak; i++)
                    tombol[baris, i].interactable = false;

            yield return new WaitForSeconds(jedaAntarSoal);
            nomorSoal++;
            if (nomorSoal >= JumlahSoal)
                TampilkanHasil();
            else
                TampilkanSoal();
        }
        else
        {
            pernahSalah = true;
            AturNeon(0, indeksA, true, NeonSalah);
            AturNeon(1, indeksB, true, NeonSalah);
            txtPetunjuk.text =
                "Belum tepat. Cari bilangan yang sama dan paling kecil.";

            yield return new WaitForSeconds(jedaJawabanSalah);
            AturNeon(0, indeksA, false, Color.white);
            AturNeon(1, indeksB, false, Color.white);
            pilihanAktif[0] = -1;
            pilihanAktif[1] = -1;
            sedangMemeriksa = false;
            txtPetunjuk.text = PetunjukAwal;
        }
    }

    private void TampilkanHasil()
    {
        sudahSelesai = true;
        sedangMemeriksa = true;
        txtNilaiSkor.text = skor.ToString();

        int jumlahBintang = skor >= 90 ? 3 : skor >= 70 ? 2 : 1;
        if (bintangEmas != null)
        {
            for (int i = 0; i < bintangEmas.Length; i++)
                if (bintangEmas[i] != null)
                    bintangEmas[i].SetActive(i < jumlahBintang);
        }

        int rekor = PlayerPrefs.GetInt(KunciBintang, 0);
        if (jumlahBintang > rekor)
        {
            PlayerPrefs.SetInt(KunciBintang, jumlahBintang);
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
            Debug.LogError(
                "Scene MenuBermain belum tercantum di Build Profiles > Scene List.",
                this);
            return;
        }
        SceneManager.LoadScene("MenuBermain");
    }

    private void OnDestroy()
    {
        for (int baris = 0; baris < 2; baris++)
        {
            for (int i = 0; i < JumlahKotak; i++)
            {
                if (tombol[baris, i] != null && pendengar[baris, i] != null)
                    tombol[baris, i].onClick.RemoveListener(pendengar[baris, i]);
            }
        }
        if (btnUlangi != null)
            btnUlangi.onClick.RemoveListener(UlangiPermainan);
        if (btnMenuBermain != null)
            btnMenuBermain.onClick.RemoveListener(KembaliMenuBermain);
    }
}
