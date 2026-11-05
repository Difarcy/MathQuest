using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Dipakai di scene QuizKPKFPB. UjianKPKFPBManager mewarisi script ini.
// UI (font, ukuran, posisi, anchor) tetap diatur melalui Inspector Unity.
public class QuizKPKFPBManager : MonoBehaviour
{
    [Header("Panel_Soal")]
    [SerializeField] private TMP_Text txtInstruksi;
    [SerializeField] private TMP_Text txtProgressSoal;

    [Header("Tombol Jawaban A-D")]
    [SerializeField] private Button btnA, btnB, btnC, btnD;

    [Header("Panel_Hasil")]
    [SerializeField] private GameObject panelHasil;
    [SerializeField] private TMP_Text txtNilaiSkor;
    [SerializeField] private GameObject[] bintangEmas = new GameObject[3];
    [SerializeField] private Button btnUlangi, btnMenuPilihan;

    [Header("Efek Feedback")]
    [SerializeField, Min(0f)] private float durasiNeonAwal = 0.12f;
    [SerializeField, Min(0f)] private float durasiFeedback = 0.85f;

    [Header("Keterbacaan Jawaban")]
    [SerializeField] private bool aturWarnaTeksJawaban = true;
    [SerializeField] private Color warnaTeksJawaban = new Color(0.12f, 0.16f, 0.23f, 1f);

    [Header("Ujian (berlaku pada kelas turunan)")]
    [SerializeField] private bool periksaUnlockDiBuild = true;

    protected virtual bool ModeUjian { get { return false; } }
    private const int JumlahSoal = 10;
    private const string KunciUnlock = "MathQuest_Ujian_Unlocked";
    private readonly Button[] tombol = new Button[4];
    private readonly TMP_Text[] teksJawaban = new TMP_Text[4];
    private readonly Outline[,] neon = new Outline[4, 2];
    private readonly UnityAction[] aksi = new UnityAction[4];
    private readonly List<Soal> sesi = new List<Soal>();
    private int nomorSoal, skor;
    private bool siap, menilai;

    private struct Soal
    {
        public string tanya;
        public string[] pilihan;
        public int benar;
        public Soal(string t, string[] p, int b) { tanya = t; pilihan = p; benar = b; }
    }

    protected virtual void Start()
    {
        if (ModeUjian && periksaUnlockDiBuild && !Application.isEditor &&
            PlayerPrefs.GetInt(KunciUnlock, 0) != 1)
        {
            KembaliMenuPilihan();
            return;
        }

        tombol[0] = btnA; tombol[1] = btnB; tombol[2] = btnC; tombol[3] = btnD;
        if (!PeriksaInspector()) { enabled = false; return; }

        for (int i = 0; i < 4; i++)
        {
            teksJawaban[i] = CariTeksJawaban(tombol[i]);
            if (teksJawaban[i] == null)
            {
                Debug.LogError("[" + name + "] " + tombol[i].name +
                    " tidak memiliki TMP jawaban. Buat child TXT_Jawaban, gunakan TextMeshProUGUI.", tombol[i]);
                enabled = false;
                return;
            }
            teksJawaban[i].gameObject.SetActive(true);
            teksJawaban[i].enabled = true;
            teksJawaban[i].raycastTarget = false;
            if (aturWarnaTeksJawaban) teksJawaban[i].color = warnaTeksJawaban;
            // Tidak mengubah font size, auto sizing, posisi, RectTransform ataupun font asset.
            tombol[i].transition = Selectable.Transition.None;
            Outline[] outlines = tombol[i].GetComponents<Outline>();
            neon[i, 0] = outlines.Length > 0 ? outlines[0] : tombol[i].gameObject.AddComponent<Outline>();
            neon[i, 1] = outlines.Length > 1 ? outlines[1] : tombol[i].gameObject.AddComponent<Outline>();
            neon[i, 0].effectDistance = new Vector2(7f, 7f);
            neon[i, 1].effectDistance = new Vector2(3f, 3f);
            neon[i, 0].useGraphicAlpha = false;
            neon[i, 1].useGraphicAlpha = false;
            MatikanNeon(i);
            int index = i;
            aksi[i] = () => Jawab(index);
            tombol[i].onClick.AddListener(aksi[i]);
        }

        if (btnUlangi != null) btnUlangi.onClick.AddListener(Mulai);
        if (btnMenuPilihan != null) btnMenuPilihan.onClick.AddListener(KembaliMenuPilihan);
        siap = true;
        Mulai();
    }

    private bool PeriksaInspector()
    {
        bool ok = true;
        if (txtInstruksi == null) { Debug.LogError(name + ": Txt Instruksi kosong", this); ok = false; }
        if (txtProgressSoal == null) { Debug.LogError(name + ": Txt Progress Soal kosong", this); ok = false; }
        if (panelHasil == null) { Debug.LogError(name + ": Panel Hasil kosong", this); ok = false; }
        if (txtNilaiSkor == null) { Debug.LogError(name + ": Txt Nilai Skor kosong", this); ok = false; }
        if (bintangEmas == null || bintangEmas.Length != 3)
        { Debug.LogError(name + ": Bintang Emas harus tepat 3 elemen", this); ok = false; }
        else for (int i = 0; i < 3; i++) if (bintangEmas[i] == null)
        { Debug.LogError(name + ": Bintang Emas Elemen " + i + " kosong", this); ok = false; }
        for (int i = 0; i < 4; i++) if (tombol[i] == null)
        { Debug.LogError(name + ": Btn " + "ABCD"[i] + " kosong", this); ok = false; }
        // Dua tombol panel hasil bersifat opsional, misalnya bila dipasang melalui UI lain.
        if (btnUlangi == null) Debug.LogWarning(name + ": Btn Ulangi belum dihubungkan", this);
        if (btnMenuPilihan == null) Debug.LogWarning(name + ": Btn Menu Pilihan belum dihubungkan", this);
        return ok;
    }

    private TMP_Text CariTeksJawaban(Button b)
    {
        TMP_Text[] semua = b.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in semua)
            if (t.name == "TXT_Jawaban") return t;
        // Cadangan: hindari teks dekoratif A, B, C, D.
        foreach (TMP_Text t in semua)
            if (t.name == "TXT_Opsi" || t.name == "TXT_Pilihan") return t;
        foreach (TMP_Text t in semua)
            if (t.text.Trim() != "A" && t.text.Trim() != "B" && t.text.Trim() != "C" && t.text.Trim() != "D") return t;
        return null;
    }

    public void Mulai()
    {
        if (!siap) return;
        StopAllCoroutines();
        Time.timeScale = 1f;
        skor = 0; nomorSoal = 0; menilai = false;
        panelHasil.SetActive(false);
        foreach (GameObject b in bintangEmas) b.SetActive(false);
        BangunSesi();
        TampilkanSoal();
    }

    private static void Acak<T>(IList<T> a)
    {
        for (int i = a.Count - 1; i > 0; i--)
        { int j = UnityEngine.Random.Range(0, i + 1); T temp = a[i]; a[i] = a[j]; a[j] = temp; }
    }
    private static T Ambil<T>(IList<T> a) { return a[UnityEngine.Random.Range(0, a.Count)]; }
    private static int FPB(int a, int b) { while (b != 0) { int r = a % b; a = b; b = r; } return a; }
    private static int KPK(int a, int b) { return a / FPB(a, b) * b; }
    private static List<int> Faktor(int n)
    {
        List<int> hasil = new List<int>();
        for (int i = 1; i <= n; i++) if (n % i == 0) hasil.Add(i);
        return hasil;
    }

    // 30 pola soal (0-29), dengan parameter berbeda untuk Quiz/Ujian.
    // Setiap sesi mengambil 10 pola unik: keenam kelompok materi + dua cerita + dua variasi.
    private void BangunSesi()
    {
        List<int> indeks = new List<int>();
        for (int kategori = 0; kategori < 6; kategori++)
            indeks.Add(UnityEngine.Random.Range(kategori * 4, kategori * 4 + 4));
        int cerita1 = UnityEngine.Random.Range(24, 30);
        int cerita2;
        do { cerita2 = UnityEngine.Random.Range(24, 30); } while (cerita2 == cerita1);
        indeks.Add(cerita1); indeks.Add(cerita2);
        List<int> sisa = new List<int>();
        for (int i = 0; i < 30; i++) if (!indeks.Contains(i)) sisa.Add(i);
        Acak(sisa);
        indeks.Add(sisa[0]); indeks.Add(sisa[1]); Acak(indeks);
        sesi.Clear();
        HashSet<string> dipakai = new HashSet<string>();
        foreach (int id in indeks)
        {
            Soal s = BuatSoal(id);
            for (int ulang = 0; ulang < 80 && dipakai.Contains(s.tanya); ulang++) s = BuatSoal(id);
            sesi.Add(s); dipakai.Add(s.tanya);
        }
    }

    // Empat opsi numerik berbeda, tepat satu jawaban benar, semua nilai positif.
    private static Soal Numerik(string pertanyaan, int benar, int langkah = 1, string satuan = "")
    {
        langkah = Mathf.Max(1, langkah);
        List<int> angka = new List<int> { benar };
        int[] geser = { -1, 1, -2, 2, -3, 3, 4, -4, 5, 6, 7, 8 };
        foreach (int offset in geser)
        {
            int kandidat = benar + langkah * offset;
            if (kandidat > 0 && !angka.Contains(kandidat)) angka.Add(kandidat);
            if (angka.Count == 4) break;
        }
        Acak(angka);
        string[] ops = new string[4];
        for (int i = 0; i < 4; i++) ops[i] = angka[i] + satuan;
        return new Soal(pertanyaan, ops, angka.IndexOf(benar));
    }

    private static Soal TanyaFaktor(int n)
    {
        List<int> semua = Faktor(n);
        List<int> bukan = new List<int>();
        for (int i = 2; i < n; i++) if (n % i != 0) bukan.Add(i);
        Acak(bukan);
        int faktorBenar = semua[UnityEngine.Random.Range(1, semua.Count - 1)];
        return Teks("Manakah yang merupakan faktor dari " + n + "?",
            faktorBenar.ToString(), bukan[0].ToString(), bukan[1].ToString(), bukan[2].ToString());
    }

    private static Soal Teks(string pertanyaan, string benar, string salah1, string salah2, string salah3)
    {
        List<string> pilihan = new List<string> { benar, salah1, salah2, salah3 };
        Acak(pilihan);
        return new Soal(pertanyaan, pilihan.ToArray(), pilihan.IndexOf(benar));
    }

    private static readonly Vector2Int[] KpkQuiz = {
        new Vector2Int(2, 3), new Vector2Int(3, 4), new Vector2Int(4, 6),
        new Vector2Int(4, 8), new Vector2Int(5, 6), new Vector2Int(6, 8),
        new Vector2Int(6, 9), new Vector2Int(8, 12)
    };
    private static readonly Vector2Int[] KpkUjian = {
        new Vector2Int(6, 8), new Vector2Int(8, 12), new Vector2Int(9, 12),
        new Vector2Int(10, 15), new Vector2Int(12, 18), new Vector2Int(12, 15),
        new Vector2Int(14, 21), new Vector2Int(15, 20)
    };
    private static readonly Vector2Int[] FpbQuiz = {
        new Vector2Int(8, 12), new Vector2Int(12, 18), new Vector2Int(12, 20),
        new Vector2Int(16, 24), new Vector2Int(18, 24), new Vector2Int(20, 30),
        new Vector2Int(24, 36), new Vector2Int(15, 25)
    };
    private static readonly Vector2Int[] FpbUjian = {
        new Vector2Int(24, 36), new Vector2Int(30, 45), new Vector2Int(36, 48),
        new Vector2Int(40, 60), new Vector2Int(42, 56), new Vector2Int(48, 72),
        new Vector2Int(54, 72), new Vector2Int(60, 90)
    };

    private Soal BuatSoal(int id)
    {
        bool u = ModeUjian;
        int n, k, a, b, benar;
        Vector2Int p;
        List<int> f;
        switch (id)
        {
            // Faktor: dasar pada Quiz, operasi gabungan sederhana pada Ujian.
            case 0:
                n = Ambil(u ? new[] { 24, 30, 36, 40, 48 } : new[] { 12, 16, 18, 20, 24 });
                return TanyaFaktor(n);
            case 1:
                n = Ambil(u ? new[] { 24, 30, 36, 40, 48 } : new[] { 8, 12, 16, 18, 20 });
                return Numerik("Berapa banyak faktor positif dari " + n + "?", Faktor(n).Count, 1);
            case 2:
                n = Ambil(u ? new[] { 30, 36, 40, 48 } : new[] { 12, 16, 18, 24 });
                f = Faktor(n);
                return Numerik("Faktor terbesar dari " + n + " selain " + n + " adalah ...", f[f.Count - 2], u ? 2 : 1);
            case 3:
                n = Ambil(u ? new[] { 30, 36, 40, 48 } : new[] { 12, 18, 20, 24 });
                f = Faktor(n); k = 0;
                foreach (int v in f) if (v % 2 == 0) k++;
                return Numerik("Berapa faktor genap dari " + n + "?", k, 1);
            // Faktor persekutuan
            case 4:
                p = Ambil(u ? FpbUjian : FpbQuiz);
                return Numerik("Berapa banyak faktor persekutuan dari " + p.x + " dan " + p.y + "?", Faktor(FPB(p.x, p.y)).Count);
            case 5:
                p = Ambil(u ? FpbUjian : FpbQuiz);
                return Numerik("Faktor persekutuan terbesar dari " + p.x + " dan " + p.y + " adalah ...", FPB(p.x, p.y), u ? 2 : 1);
            case 6:
                p = Ambil(u ? FpbUjian : FpbQuiz); f = Faktor(FPB(p.x, p.y));
                return Numerik("Faktor persekutuan terbesar kedua dari " + p.x + " dan " + p.y + " adalah ...", f[f.Count - 2]);
            case 7:
                p = Ambil(u ? FpbUjian : FpbQuiz); f = Faktor(FPB(p.x, p.y)); k = 0;
                foreach (int v in f) if (v % 2 == 0) k++;
                return Numerik("Berapa faktor persekutuan genap dari " + p.x + " dan " + p.y + "?", k);
            // Kelipatan
            case 8:
                n = UnityEngine.Random.Range(u ? 6 : 2, u ? 13 : 9);
                k = UnityEngine.Random.Range(u ? 5 : 2, u ? 10 : 6);
                return Numerik("Kelipatan ke-" + k + " dari " + n + " adalah ...", n * k, n);
            case 9:
                n = UnityEngine.Random.Range(u ? 6 : 2, u ? 13 : 9); k = UnityEngine.Random.Range(2, 6);
                return Numerik("Setelah " + (n * k) + ", kelipatan " + n + " berikutnya adalah ...", n * (k + 1), n);
            case 10:
                n = UnityEngine.Random.Range(u ? 6 : 2, u ? 13 : 9); k = u ? 4 : 2;
                return Numerik("Selisih kelipatan ke-" + (k + 2) + " dan ke-" + k + " dari " + n + " adalah ...", 2 * n, n);
            case 11:
                n = UnityEngine.Random.Range(u ? 6 : 2, u ? 13 : 9);
                return Numerik("Kelipatan " + n + " yang ke-" + (u ? 7 : 3) + " adalah ...", n * (u ? 7 : 3), n);
            // Kelipatan persekutuan
            case 12:
                p = Ambil(u ? KpkUjian : KpkQuiz);
                return Numerik("Kelipatan persekutuan positif pertama dari " + p.x + " dan " + p.y + " adalah ...", KPK(p.x, p.y), Mathf.Min(p.x, p.y));
            case 13:
                p = Ambil(u ? KpkUjian : KpkQuiz); benar = KPK(p.x, p.y);
                return Numerik("Kelipatan persekutuan positif kedua dari " + p.x + " dan " + p.y + " adalah ...", 2 * benar, benar);
            case 14:
                p = Ambil(u ? KpkUjian : KpkQuiz); benar = KPK(p.x, p.y);
                return Numerik("Setelah " + (u ? 2 : 1) * benar + ", kelipatan persekutuan " + p.x + " dan " + p.y + " berikutnya adalah ...", (u ? 3 : 2) * benar, benar);
            case 15:
                p = Ambil(u ? KpkUjian : KpkQuiz); benar = KPK(p.x, p.y);
                return Numerik("Kelipatan persekutuan positif ke-" + (u ? 4 : 3) + " dari " + p.x + " dan " + p.y + " adalah ...", (u ? 4 : 3) * benar, benar);
            // KPK
            case 16:
                p = Ambil(u ? KpkUjian : KpkQuiz);
                return Numerik("Tentukan KPK dari " + p.x + " dan " + p.y + ".", KPK(p.x, p.y), Mathf.Min(p.x, p.y));
            case 17:
                if (u) { a = 4; b = 6; n = 9; }
                else { a = 2; b = 3; n = 4; }
                return Numerik("Tentukan KPK dari " + a + ", " + b + ", dan " + n + ".", KPK(KPK(a, b), n), 2);
            case 18:
                p = Ambil(u ? KpkUjian : KpkQuiz);
                return Numerik("KPK dari " + p.x + " dan " + p.y + " adalah ...", KPK(p.x, p.y), p.x);
            case 19:
                p = Ambil(u ? KpkUjian : KpkQuiz);
                return Numerik("Bilangan terkecil yang habis dibagi " + p.x + " dan " + p.y + " adalah ...", KPK(p.x, p.y), p.x);
            // FPB
            case 20:
                p = Ambil(u ? FpbUjian : FpbQuiz);
                return Numerik("Tentukan FPB dari " + p.x + " dan " + p.y + ".", FPB(p.x, p.y), 2);
            case 21:
                if (u) { a = 24; b = 36; n = 60; }
                else { a = 12; b = 18; n = 24; }
                return Numerik("Tentukan FPB dari " + a + ", " + b + ", dan " + n + ".", FPB(FPB(a, b), n), 2);
            case 22:
                p = Ambil(u ? FpbUjian : FpbQuiz);
                return Numerik("Pembagi terbesar yang membagi " + p.x + " dan " + p.y + " tanpa sisa adalah ...", FPB(p.x, p.y), 2);
            case 23:
                n = u ? UnityEngine.Random.Range(6, 13) : UnityEngine.Random.Range(2, 7);
                return Numerik("FPB dari " + (2 * n) + " dan " + (3 * n) + " adalah ...", n, 1);
            // Cerita: ujian angka lebih tinggi dan lebih banyak langkah.
            case 24:
                p = Ambil(u ? KpkUjian : KpkQuiz);
                return Numerik(u
                    ? "Ani berlatih tiap " + p.x + " hari dan Budi tiap " + p.y + " hari. Mereka bersama hari ini. Berapa hari lagi sampai pertemuan bersama KEDUA setelah hari ini?"
                    : "Ani berlatih tiap " + p.x + " hari, Budi tiap " + p.y + " hari. Jika hari ini bersama, berapa hari lagi mereka bersama?",
                    KPK(p.x, p.y) * (u ? 2 : 1), p.x, " hari");
            case 25:
                p = Ambil(u ? KpkUjian : KpkQuiz);
                return Numerik("Dua lonceng berbunyi tiap " + p.x + " dan " + p.y + " menit. Keduanya baru berbunyi bersama. Kapan berbunyi bersama lagi?", KPK(p.x, p.y), p.x, " menit");
            case 26:
                p = Ambil(u ? KpkUjian : KpkQuiz);
                return Numerik("Lampu A menyala tiap " + p.x + " detik dan B tiap " + p.y + " detik. Jika sekarang bersama, kapan bersama lagi?", KPK(p.x, p.y), p.x, " detik");
            case 27:
                p = Ambil(u ? FpbUjian : FpbQuiz);
                return u
                    ? Numerik("Ada " + p.x + " pensil dan " + p.y + " penghapus. Semua dibuat menjadi paket identik sebanyak mungkin tanpa sisa. Berapa pensil dalam SETIAP paket?", p.x / FPB(p.x, p.y), 1, " pensil")
                    : Numerik("Ada " + p.x + " pensil dan " + p.y + " penghapus. Dibuat paket identik sebanyak mungkin tanpa sisa. Ada berapa paket?", FPB(p.x, p.y), 2, " paket");
            case 28:
                p = Ambil(u ? FpbUjian : FpbQuiz);
                return u
                    ? Numerik("Pita " + p.x + " cm dan " + p.y + " cm dipotong sama panjang, sepanjang mungkin, tanpa sisa. Berapa JUMLAH seluruh potongan?", (p.x + p.y) / FPB(p.x, p.y), 1, " potong")
                    : Numerik("Pita panjang " + p.x + " cm dan " + p.y + " cm dipotong sama panjang, sepanjang mungkin tanpa sisa. Panjang tiap potongan?", FPB(p.x, p.y), 2, " cm");
            case 29:
                p = Ambil(u ? FpbUjian : FpbQuiz);
                return u
                    ? Numerik("Ada " + p.x + " apel dan " + p.y + " jeruk. Dibagi ke kantong sebanyak mungkin dengan isi tiap jenis sama tanpa sisa. Berapa JERUK per kantong?", p.y / FPB(p.x, p.y), 1, " jeruk")
                    : Numerik("Ada " + p.x + " apel dan " + p.y + " jeruk. Dibagi ke kantong sebanyak mungkin dengan isi sama tanpa sisa. Berapa kantong?", FPB(p.x, p.y), 2, " kantong");
            default: throw new ArgumentOutOfRangeException("id");
        }
    }

    private void TampilkanSoal()
    {
        menilai = false;
        Soal q = sesi[nomorSoal];
        txtInstruksi.text = q.tanya;
        txtProgressSoal.text = "Soal " + (nomorSoal + 1) + "/" + JumlahSoal;
        for (int i = 0; i < 4; i++)
        {
            teksJawaban[i].text = q.pilihan[i];
            teksJawaban[i].gameObject.SetActive(true);
            teksJawaban[i].enabled = true;
            if (aturWarnaTeksJawaban) teksJawaban[i].color = warnaTeksJawaban;
            tombol[i].interactable = true;
            MatikanNeon(i);
        }
    }
    private void MatikanNeon(int i)
    {
        if (neon[i, 0] != null) neon[i, 0].enabled = false;
        if (neon[i, 1] != null) neon[i, 1].enabled = false;
    }
    private void NyalakanNeon(int i, Color warna)
    {
        for (int j = 0; j < 2; j++)
        {
            neon[i, j].effectColor = new Color(warna.r, warna.g, warna.b, j == 0 ? 0.5f : 1f);
            neon[i, j].enabled = true;
        }
    }
    private void Jawab(int i)
    {
        if (!siap || menilai || nomorSoal >= sesi.Count) return;
        menilai = true;
        foreach (Button b in tombol) b.interactable = false;
        NyalakanNeon(i, new Color(0.2f, 0.95f, 1f));
        StartCoroutine(Feedback(i));
    }
    private IEnumerator Feedback(int dipilih)
    {
        yield return new WaitForSeconds(durasiNeonAwal);
        bool benar = dipilih == sesi[nomorSoal].benar;
        Color hijau = new Color(0.2f, 1f, 0.35f);
        NyalakanNeon(dipilih, benar ? hijau : new Color(1f, 0.2f, 0.25f));
        if (benar) skor += 10;
        else NyalakanNeon(sesi[nomorSoal].benar, hijau);
        yield return new WaitForSeconds(durasiFeedback);
        nomorSoal++;
        if (nomorSoal >= JumlahSoal) Selesaikan();
        else TampilkanSoal();
    }
    private void Selesaikan()
    {
        string prefix = ModeUjian ? "MathQuest_UjianKPKFPB_" : "MathQuest_QuizKPKFPB_";
        txtNilaiSkor.text = skor.ToString();
        int bintang = skor >= 90 ? 3 : skor >= 70 ? 2 : 1;
        for (int i = 0; i < 3; i++) bintangEmas[i].SetActive(i < bintang);
        PlayerPrefs.SetInt(prefix + "Selesai", 1);
        PlayerPrefs.SetInt(prefix + "SkorTertinggi", Mathf.Max(skor, PlayerPrefs.GetInt(prefix + "SkorTertinggi", 0)));
        PlayerPrefs.SetInt(prefix + "Bintang", Mathf.Max(bintang, PlayerPrefs.GetInt(prefix + "Bintang", 0)));
        if (!ModeUjian) PlayerPrefs.SetInt(KunciUnlock, 1); // selesai sekali, tanpa nilai minimum
        PlayerPrefs.Save();
        panelHasil.SetActive(true);
    }
    public void KembaliMenuPilihan()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded("MenuPilihan")) SceneManager.LoadScene("MenuPilihan");
        else Debug.LogError("Scene MenuPilihan belum didaftarkan dalam Build Profiles > Scene List", this);
    }
    protected virtual void OnDestroy()
    {
        for (int i = 0; i < 4; i++)
            if (tombol[i] != null && aksi[i] != null) tombol[i].onClick.RemoveListener(aksi[i]);
        if (btnUlangi != null) btnUlangi.onClick.RemoveListener(Mulai);
        if (btnMenuPilihan != null) btnMenuPilihan.onClick.RemoveListener(KembaliMenuPilihan);
    }
}
