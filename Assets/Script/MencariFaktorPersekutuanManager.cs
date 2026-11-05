using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Game khusus "Mencari Faktor Persekutuan": 6 angka, 4 slot drag-and-drop.
// JANGAN pasang komponen ini pada scene QuizKPKFPB atau UjianKPKFPB.
public class MencariFaktorPersekutuanManager : MonoBehaviour
{
    [Header("Panel Soal")]
    [SerializeField] private TMP_Text txtInstruksi;
    [SerializeField] private TMP_Text txtProgressSoal;

    [Header("Panel Deret Faktor")]
    [SerializeField] private TMP_Text txtDeret1;
    [SerializeField] private TMP_Text txtDeret2;

    [Header("Area Jawaban")]
    [SerializeField] private TMP_Text txtPetunjuk;
    [SerializeField] private RectTransform[] slot = new RectTransform[4];

    [Header("Enam Kotak Pilihan (01-06)")]
    [SerializeField] private Button[] pilihan = new Button[6];
    [SerializeField] private Sprite ikonSalah;

    [Header("Panel Hasil")]
    [SerializeField] private GameObject panelHasil;
    [SerializeField] private TMP_Text txtNilaiSkor;
    [SerializeField] private GameObject[] bintangEmas = new GameObject[3];

    [Header("Tombol Panel Hasil")]
    [SerializeField] private Button btnUlangi;
    [SerializeField] private Button btnMenuBermain;

    [Header("Pengaturan")]
    [SerializeField, Min(0f)] private float jedaAntarSoal = 0.8f;

    private const int JUMLAH_SOAL = 10;
    private const int JUMLAH_PILIHAN = 6;
    private const int JUMLAH_SLOT = 4;
    private const string KUNCI_BINTANG = "MathQuest_MencariFaktorPersekutuan_Bintang";

    // Setiap pasangan memiliki TEPAT 4 faktor persekutuan positif.
    private readonly Vector2Int[] bankSoal =
    {
        new Vector2Int(12,18), new Vector2Int(16,24), new Vector2Int(20,30),
        new Vector2Int(28,42), new Vector2Int(30,45), new Vector2Int(42,63),
        new Vector2Int(44,66), new Vector2Int(52,78), new Vector2Int(54,81),
        new Vector2Int(66,99)
    };

    // 6 opsi per soal: 4 faktor persekutuan + 2 pengecoh.
    private readonly int[,] bankPilihan =
    {
        { 1, 2, 3, 4, 6, 9 },
        { 1, 2, 3, 4, 6, 8 },
        { 1, 2, 3, 4, 5, 10 },
        { 1, 2, 3, 4, 7, 14 },
        { 1, 2, 3, 5, 9, 15 },
        { 1, 2, 3, 7, 9, 21 },
        { 1, 2, 3, 4, 11, 22 },
        { 1, 2, 3, 4, 13, 26 },
        { 1, 2, 3, 6, 9, 27 },
        { 1, 2, 3, 9, 11, 33 }
    };

    private readonly TMP_Text[] teksPilihan = new TMP_Text[JUMLAH_PILIHAN];
    private readonly Image[] gambarPilihan = new Image[JUMLAH_PILIHAN];
    private readonly Image[] ikonStatus = new Image[JUMLAH_PILIHAN];
    private readonly CanvasGroup[] grupPilihan = new CanvasGroup[JUMLAH_PILIHAN];
    private readonly int[] angkaPilihan = new int[JUMLAH_PILIHAN];
    private readonly bool[] sudahDitempatkan = new bool[JUMLAH_PILIHAN];
    private readonly GameObject[] isiSlot = new GameObject[JUMLAH_SLOT];
    private readonly List<EventTrigger.Entry>[] eventBawaan = new List<EventTrigger.Entry>[JUMLAH_PILIHAN];
    private readonly EventTrigger[] pemicu = new EventTrigger[JUMLAH_PILIHAN];

    private Canvas canvasUtama;
    private RectTransform canvasRect;
    private GameObject pratinjauSeret;
    private int indeksSeret = -1;
    private int nomorSoal, jumlahBenar, soalTanpaKesalahan;
    private bool adaKesalahan, sedangTransisi, permainanSelesai;
    private bool siap;

    private void Start()
    {
        // Lindungi scene Quiz/Ujian dari script permainan yang salah terpasang.
        string namaScene = SceneManager.GetActiveScene().name;
        if (namaScene == "QuizKPKFPB" || namaScene == "UjianKPKFPB")
        {
            Debug.LogWarning("MencariFaktorPersekutuanManager terpasang di scene " + namaScene +
                ". Komponen ini dinonaktifkan. Hapus komponen dari GameObject pada scene tersebut.", this);
            enabled = false;
            return;
        }

        if (!PeriksaInspector()) { enabled = false; return; }
        canvasUtama = pilihan[0].GetComponentInParent<Canvas>();
        if (canvasUtama == null)
        {
            Debug.LogError("MencariFaktorPersekutuan: Canvas untuk Pilihan_01 tidak ditemukan", this);
            enabled = false; return;
        }
        canvasUtama = canvasUtama.rootCanvas;
        canvasRect = canvasUtama.GetComponent<RectTransform>();
        if (canvasRect == null || !SiapkanPilihan()) { enabled = false; return; }
        HubungkanTombol();
        siap = true;
        MulaiPermainan();
    }

    // Pesan error per-field agar jelas objek apa yang harus di-drag ke Inspector.
    private bool PeriksaInspector()
    {
        bool ok = true;
        if (txtInstruksi == null) { Debug.LogError("MencariFaktorPersekutuan: Txt Instruksi kosong", this); ok = false; }
        if (txtProgressSoal == null) { Debug.LogError("MencariFaktorPersekutuan: Txt Progress Soal kosong", this); ok = false; }
        if (txtDeret1 == null) { Debug.LogError("MencariFaktorPersekutuan: Txt Deret 1 kosong", this); ok = false; }
        if (txtDeret2 == null) { Debug.LogError("MencariFaktorPersekutuan: Txt Deret 2 kosong", this); ok = false; }
        if (txtPetunjuk == null) { Debug.LogError("MencariFaktorPersekutuan: Txt Petunjuk kosong", this); ok = false; }
        if (panelHasil == null) { Debug.LogError("MencariFaktorPersekutuan: Panel Hasil kosong", this); ok = false; }
        if (txtNilaiSkor == null) { Debug.LogError("MencariFaktorPersekutuan: Txt Nilai Skor kosong", this); ok = false; }
        if (pilihan == null || pilihan.Length != JUMLAH_PILIHAN)
        { Debug.LogError("MencariFaktorPersekutuan: Pilihan harus 6 elemen", this); ok = false; }
        else for (int i = 0; i < JUMLAH_PILIHAN; i++) if (pilihan[i] == null)
        { Debug.LogError("MencariFaktorPersekutuan: Pilihan Element " + i + " kosong", this); ok = false; }
        if (slot == null || slot.Length != JUMLAH_SLOT)
        { Debug.LogError("MencariFaktorPersekutuan: Slot harus 4 elemen", this); ok = false; }
        else for (int i = 0; i < JUMLAH_SLOT; i++) if (slot[i] == null)
        { Debug.LogError("MencariFaktorPersekutuan: Slot Element " + i + " kosong", this); ok = false; }
        if (bintangEmas == null || bintangEmas.Length != 3)
        { Debug.LogError("MencariFaktorPersekutuan: Bintang Emas harus 3 elemen", this); ok = false; }
        else for (int i = 0; i < 3; i++) if (bintangEmas[i] == null)
        { Debug.LogError("MencariFaktorPersekutuan: Bintang Emas Element " + i + " kosong", this); ok = false; }
        if (btnUlangi == null) Debug.LogWarning("MencariFaktorPersekutuan: Btn Ulangi belum disetel; akan dicari otomatis", this);
        if (btnMenuBermain == null) Debug.LogWarning("MencariFaktorPersekutuan: Btn Menu Bermain belum disetel; akan dicari otomatis", this);
        return ok;
    }

    private bool SiapkanPilihan()
    {
        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            Button btn = pilihan[i];
            gambarPilihan[i] = btn.GetComponent<Image>();
            Transform label = btn.transform.Find("TXT_Angka");
            if (label == null)
            {
                foreach (TMP_Text tmp in btn.GetComponentsInChildren<TMP_Text>(true))
                    if (tmp.name == "TXT_Angka") { label = tmp.transform; break; }
            }
            teksPilihan[i] = label == null ? null : label.GetComponent<TMP_Text>();
            if (gambarPilihan[i] == null || teksPilihan[i] == null)
            {
                Debug.LogError(btn.name + " harus punya Image dan child TXT_Angka (TextMeshProUGUI)", btn);
                return false;
            }
            teksPilihan[i].raycastTarget = false;
            grupPilihan[i] = btn.GetComponent<CanvasGroup>();
            if (grupPilihan[i] == null) grupPilihan[i] = btn.gameObject.AddComponent<CanvasGroup>();
            Transform status = btn.transform.Find("Icon_Status");
            if (status != null) ikonStatus[i] = status.GetComponent<Image>();
            if (ikonStatus[i] != null)
            { ikonStatus[i].raycastTarget = false; ikonStatus[i].gameObject.SetActive(false); }
            btn.transition = Selectable.Transition.None;
            EventTrigger trigger = btn.GetComponent<EventTrigger>();
            if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();
            pemicu[i] = trigger;
            if (trigger.triggers == null) trigger.triggers = new List<EventTrigger.Entry>();
            // Pertahankan EventTrigger bawaan pengguna, lalu tambahkan milik script.
            eventBawaan[i] = new List<EventTrigger.Entry>(trigger.triggers);
            int indeks = i;
            TambahEvent(trigger, EventTriggerType.BeginDrag, e => MulaiSeret(indeks, (PointerEventData)e));
            TambahEvent(trigger, EventTriggerType.Drag, e => GerakkanSeret(indeks, (PointerEventData)e));
            TambahEvent(trigger, EventTriggerType.EndDrag, e => AkhiriSeret(indeks, (PointerEventData)e));
        }
        return true;
    }
    private static void TambahEvent(EventTrigger trigger, EventTriggerType tipe, UnityEngine.Events.UnityAction<BaseEventData> aksi)
    {
        EventTrigger.Entry e = new EventTrigger.Entry();
        e.eventID = tipe; e.callback.AddListener(aksi); trigger.triggers.Add(e);
    }
    private void HubungkanTombol()
    {
        if (btnUlangi == null) btnUlangi = CariTombol("BTN_Ulangi");
        if (btnMenuBermain == null) btnMenuBermain = CariTombol("BTN_MenuBermain");
        if (btnMenuBermain == null) btnMenuBermain = CariTombol("BTN_KembaliMenuBermain");
        if (btnUlangi != null) btnUlangi.onClick.AddListener(UlangiPermainan);
        if (btnMenuBermain != null) btnMenuBermain.onClick.AddListener(KembaliMenuBermain);
    }
    private Button CariTombol(string nama)
    {
        if (panelHasil == null) return null;
        foreach (Button b in panelHasil.GetComponentsInChildren<Button>(true))
            if (b.name == nama) return b;
        return null;
    }

    public void MulaiPermainan()
    {
        if (!siap) return;
        StopAllCoroutines();
        Time.timeScale = 1f;
        BersihkanSeret();
        nomorSoal = 0; jumlahBenar = 0; soalTanpaKesalahan = 0;
        adaKesalahan = false; sedangTransisi = false; permainanSelesai = false;
        foreach (GameObject b in bintangEmas) b.SetActive(false);
        panelHasil.SetActive(false);
        TampilkanSoal();
    }
    public void UlangiPermainan() { MulaiPermainan(); }

    private static string BuatDeretFaktor(int n)
    {
        List<int> faktor = new List<int>();
        for (int i = 1; i <= n; i++) if (n % i == 0) faktor.Add(i);
        return "Faktor " + n + ": " + string.Join(", ", faktor);
    }
    private void TampilkanSoal()
    {
        BersihkanSeret();
        jumlahBenar = 0; adaKesalahan = false; sedangTransisi = false;
        Vector2Int p = bankSoal[nomorSoal];
        txtInstruksi.text = "Cari faktor persekutuan dari " + p.x + " dan " + p.y;
        txtProgressSoal.text = "Soal " + (nomorSoal + 1) + "/" + JUMLAH_SOAL;
        txtDeret1.text = BuatDeretFaktor(p.x);
        txtDeret2.text = BuatDeretFaktor(p.y);
        txtPetunjuk.text = "Seret semua faktor persekutuan ke kotak kosong";
        for (int i = 0; i < JUMLAH_SLOT; i++)
        {
            if (isiSlot[i] != null) Destroy(isiSlot[i]);
            isiSlot[i] = null;
        }
        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            angkaPilihan[i] = bankPilihan[nomorSoal, i];
            sudahDitempatkan[i] = false;
            pilihan[i].gameObject.SetActive(true);
            pilihan[i].interactable = true;
            teksPilihan[i].text = angkaPilihan[i].ToString();
            teksPilihan[i].gameObject.SetActive(true);
            grupPilihan[i].alpha = 1f;
            grupPilihan[i].interactable = true;
            grupPilihan[i].blocksRaycasts = true;
            if (ikonStatus[i] != null) ikonStatus[i].gameObject.SetActive(false);
        }
    }

    private void MulaiSeret(int i, PointerEventData data)
    {
        if (data == null || sedangTransisi || permainanSelesai || indeksSeret >= 0 || sudahDitempatkan[i]) return;
        indeksSeret = i;
        pratinjauSeret = BuatVisualAngka(i, canvasRect,
            ((RectTransform)pilihan[i].transform).rect.size, "Pratinjau_Seret");
        if (pratinjauSeret == null) { indeksSeret = -1; return; }
        grupPilihan[i].alpha = 0f;
        grupPilihan[i].blocksRaycasts = false;
        PindahkanVisual(data);
    }
    private void GerakkanSeret(int i, PointerEventData data)
    {
        if (i == indeksSeret && data != null && pratinjauSeret != null) PindahkanVisual(data);
    }
    private void PindahkanVisual(PointerEventData data)
    {
        Vector3 world;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect,
            data.position, KameraUI(data), out world)) pratinjauSeret.transform.position = world;
    }
    private Camera KameraUI(PointerEventData data)
    {
        return canvasUtama.renderMode == RenderMode.ScreenSpaceOverlay ? null : data.pressEventCamera;
    }

    private void AkhiriSeret(int i, PointerEventData data)
    {
        if (i != indeksSeret) return;
        int tujuan = -1;
        if (data != null)
            for (int s = 0; s < JUMLAH_SLOT; s++)
                if (isiSlot[s] == null && RectTransformUtility.RectangleContainsScreenPoint(
                    slot[s], data.position, KameraUI(data))) { tujuan = s; break; }
        BersihkanSeret();
        if (tujuan < 0 || sedangTransisi || permainanSelesai) return;
        int angka = angkaPilihan[i];
        Vector2Int p = bankSoal[nomorSoal];
        bool benar = angka > 0 && p.x % angka == 0 && p.y % angka == 0;
        if (!benar)
        {
            adaKesalahan = true;
            if (ikonStatus[i] != null && ikonSalah != null)
            {
                ikonStatus[i].sprite = ikonSalah;
                ikonStatus[i].color = Color.white;
                ikonStatus[i].gameObject.SetActive(true);
                StartCoroutine(HilangkanIkonSalah(i));
            }
            return;
        }
        float ukuran = Mathf.Min(slot[tujuan].rect.width, slot[tujuan].rect.height) * 0.9f;
        isiSlot[tujuan] = BuatVisualAngka(i, slot[tujuan], new Vector2(ukuran, ukuran), "IMG_AngkaMasuk");
        sudahDitempatkan[i] = true;
        pilihan[i].interactable = false;
        grupPilihan[i].alpha = 0f;
        grupPilihan[i].interactable = false;
        grupPilihan[i].blocksRaycasts = false;
        jumlahBenar++;
        if (jumlahBenar == JUMLAH_SLOT)
        {
            sedangTransisi = true;
            if (!adaKesalahan) soalTanpaKesalahan++;
            StartCoroutine(LanjutSoalBerikutnya());
        }
    }
    private IEnumerator HilangkanIkonSalah(int i)
    {
        yield return new WaitForSeconds(0.5f);
        if (ikonStatus[i] != null) ikonStatus[i].gameObject.SetActive(false);
    }

    private GameObject BuatVisualAngka(int i, RectTransform induk, Vector2 ukuran, string nama)
    {
        if (induk == null) return null;
        GameObject visual = new GameObject(nama, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform r = visual.GetComponent<RectTransform>();
        r.SetParent(induk, false);
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta = ukuran;
        r.localScale = Vector3.one;
        Image g = visual.GetComponent<Image>();
        g.sprite = gambarPilihan[i].sprite;
        g.color = gambarPilihan[i].color;
        g.preserveAspect = true;
        g.raycastTarget = false;
        GameObject angkaObj = new GameObject("TXT_AngkaMasuk", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform rt = angkaObj.GetComponent<RectTransform>();
        rt.SetParent(r, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        TextMeshProUGUI txt = angkaObj.GetComponent<TextMeshProUGUI>();
        TMP_Text asal = teksPilihan[i];
        txt.text = angkaPilihan[i].ToString();
        txt.font = asal.font;
        txt.fontStyle = asal.fontStyle;
        txt.color = asal.color;
        txt.fontSize = asal.fontSize;
        txt.alignment = TextAlignmentOptions.Center;
        txt.enableAutoSizing = false;
        txt.raycastTarget = false;
        r.SetAsLastSibling();
        return visual;
    }
    private void BersihkanSeret()
    {
        if (indeksSeret >= 0 && indeksSeret < JUMLAH_PILIHAN && grupPilihan[indeksSeret] != null)
        {
            if (!sudahDitempatkan[indeksSeret])
            {
                grupPilihan[indeksSeret].alpha = 1f;
                grupPilihan[indeksSeret].blocksRaycasts = true;
            }
        }
        if (pratinjauSeret != null) Destroy(pratinjauSeret);
        pratinjauSeret = null;
        indeksSeret = -1;
    }
    private IEnumerator LanjutSoalBerikutnya()
    {
        yield return new WaitForSeconds(jedaAntarSoal);
        nomorSoal++;
        if (nomorSoal == JUMLAH_SOAL) TampilkanHasil();
        else TampilkanSoal();
    }
    private void TampilkanHasil()
    {
        BersihkanSeret();
        permainanSelesai = sedangTransisi = true;
        int skor = soalTanpaKesalahan * 10;
        int bintang = skor >= 90 ? 3 : skor >= 70 ? 2 : 1;
        txtNilaiSkor.text = skor.ToString();
        for (int i = 0; i < 3; i++) bintangEmas[i].SetActive(i < bintang);
        if (bintang > PlayerPrefs.GetInt(KUNCI_BINTANG, 0))
        { PlayerPrefs.SetInt(KUNCI_BINTANG, bintang); PlayerPrefs.Save(); }
        panelHasil.SetActive(true);
    }
    public void KembaliMenuBermain()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded("MenuBermain")) SceneManager.LoadScene("MenuBermain");
        else Debug.LogError("Scene MenuBermain belum ada di Build Profiles > Scene List", this);
    }
    private void OnDestroy()
    {
        if (btnUlangi != null) btnUlangi.onClick.RemoveListener(UlangiPermainan);
        if (btnMenuBermain != null) btnMenuBermain.onClick.RemoveListener(KembaliMenuBermain);
        for (int i = 0; i < JUMLAH_PILIHAN; i++)
        {
            if (pemicu[i] != null && eventBawaan[i] != null) pemicu[i].triggers = eventBawaan[i];
        }
    }
}
