// PANDUAN PROYEK - Runtime / Pengaturan
// Fungsi: Mengurus konfirmasi keluar dari gameplay ke menu dan penutupan pengaturan HP.
// Panduan: Assets/Scripts/Runtime/Pengaturan/README.md

using UnityEngine;
using UnityEngine.SceneManagement;

public class PengaturanGameplay : MonoBehaviour
{
    public GameObject panelKonfirmasi;
    public SistemHP sistemHP;
    public string sceneMenu = "MainMenu";
    private bool sedangKeluar;

    void OnEnable()
    {
        sedangKeluar = false;
        if (panelKonfirmasi != null) panelKonfirmasi.SetActive(false);
    }

    public void MintaKeluar()
    {
        if (!sedangKeluar && panelKonfirmasi != null) panelKonfirmasi.SetActive(true);
    }

    public void BatalKeluar()
    {
        if (!sedangKeluar && panelKonfirmasi != null) panelKonfirmasi.SetActive(false);
    }

    public void YaKeluar()
    {
        if (sedangKeluar || panelKonfirmasi == null || !panelKonfirmasi.activeInHierarchy) return;
        sedangKeluar = true;
        if (sistemHP != null && sistemHP.transisiAplikasi != null)
            sistemHP.transisiAplikasi.Tutup(KembaliKeMenu, true);
        else KembaliKeMenu();
    }

    private void KembaliKeMenu()
    {
        // Progres percakapan, posisi, dan parameter saat ini berada di scene.
        // Memuat MainMenu dengan Single membuang sesi itu, tanpa menghapus preferensi.
        LoadingManager.sceneTujuan = "CharacterLab";
        SutradaraTransisi.sceneTujuanUtama = null;
        Time.timeScale = 1f;
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneMenu, LoadSceneMode.Single);
    }

    public void TutupPengaturan()
    {
        if (sedangKeluar || (sistemHP != null && sistemHP.transisiAplikasi != null && sistemHP.transisiAplikasi.SedangBerjalan)) return;
        BatalKeluar();
        if (sistemHP != null) sistemHP.TutupAplikasi();
        else gameObject.SetActive(false);
    }
}
