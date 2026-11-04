
using UnityEngine;
using UnityEngine.SceneManagement;

public class KonfirmasiKeluarGame : MonoBehaviour
{
    [SerializeField]
    private GameObject panelKonfirmasiKeluar;

    private float waktuSebelumnya = 1f;
    private bool sedangKonfirmasi = false;

    private void Awake()
    {
        if (panelKonfirmasiKeluar != null)
            panelKonfirmasiKeluar.SetActive(false);
    }

    // Dipanggil tombol kembali merah.
    public void TampilkanKonfirmasi()
    {
        if (panelKonfirmasiKeluar == null ||
            sedangKonfirmasi)
            return;

        waktuSebelumnya = Time.timeScale;
        sedangKonfirmasi = true;

        panelKonfirmasiKeluar.SetActive(true);
        Time.timeScale = 0f;
    }

    // Tombol TIDAK.
    public void PilihTidak()
    {
        if (!sedangKonfirmasi)
            return;

        panelKonfirmasiKeluar.SetActive(false);
        Time.timeScale = waktuSebelumnya;
        sedangKonfirmasi = false;
    }

    // Tombol YA.
    public void PilihYa()
    {
        Time.timeScale = waktuSebelumnya;
        sedangKonfirmasi = false;

        SceneManager.LoadScene("MenuBermain");
    }

    private void OnDisable()
    {
        if (sedangKonfirmasi)
        {
            Time.timeScale = waktuSebelumnya;
            sedangKonfirmasi = false;
        }
    }
}
