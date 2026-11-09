// PANDUAN PROYEK - Runtime / AlurDanMenu
// Fungsi: Mengurus tombol mulai, popup, pengaturan, eksplorasi, penghargaan, dan status tamat pada menu.
// Panduan: Assets/Scripts/Runtime/AlurDanMenu/README.md

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using TMPro;

public class TitleMenuManager : MonoBehaviour
{
    [Header("Panel Antarmuka (GDD 4.11)")]
    public GameObject warningPopup; // Textbox Peringatan (Mulai)
    public GameObject pengaturanPanel; // Overlay Gerigi
    public GameObject eksplorasiPanel; // UI Box Pilihan MC
    public GameObject penghargaanPanel; // UI Box 4 Lencana

    [Header("Tombol Terkunci (GDD 4.7.3.1)")]
    public Button eksplorasiButton;
    public Button penghargaanButton;
    
    // CanvasGroup buat ngatur Opacity 50% atau 100%
    public CanvasGroup eksplorasiCanvasGroup; 
    public CanvasGroup penghargaanCanvasGroup;

    [Header("Mode Explorer")]
    public string namaSceneExplorer = "Explorer";

    void Start()
    {
        var scaler=GetComponent<UnityEngine.UI.CanvasScaler>();
        if(scaler!=null)scaler.screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
        // 1. Matikan semua panel popup di awal biar layar bersih
        if(warningPopup != null) warningPopup.SetActive(false);
        if(pengaturanPanel != null) pengaturanPanel.SetActive(false);
        if(eksplorasiPanel != null) eksplorasiPanel.SetActive(false);
        if(penghargaanPanel != null) penghargaanPanel.SetActive(false);

        // 2. CEK STATUS TAMAT (Logika Eksplorasi & Penghargaan)
        int gameStatus = PlayerPrefs.GetInt("GameFinished", 0);
        
        if (gameStatus == 1)
        {
            // JIKA TAMAT: Tombol Aktif & Solid 100%
            if(penghargaanButton != null) penghargaanButton.interactable = true;
            if(penghargaanCanvasGroup != null) penghargaanCanvasGroup.alpha = 1.0f;
        }
        else
        {
            // JIKA BELUM TAMAT: Tombol Mati & Transparan 50%
            if(penghargaanButton != null) penghargaanButton.interactable = false;
            if(penghargaanCanvasGroup != null) penghargaanCanvasGroup.alpha = 0.5f; 
        }

        // Eksplorasi mengikuti status tamat demo, terpisah dari progres sesi cerita.
        bool eksplorasiTerbuka = gameStatus == 1;
        if (eksplorasiButton != null) eksplorasiButton.interactable = eksplorasiTerbuka;
        if (eksplorasiCanvasGroup != null)
        {
            eksplorasiCanvasGroup.alpha = eksplorasiTerbuka ? 1f : .5f;
            eksplorasiCanvasGroup.interactable = eksplorasiTerbuka;
            eksplorasiCanvasGroup.blocksRaycasts = eksplorasiTerbuka;
        }

        // 3. Sinkronisasi Audio Global dari PlayerPrefs
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);

        // The brightness filter must cover the same screen as the background.
        var filter=transform.Find("PanelHitam") as RectTransform;
        if(filter!=null){filter.anchorMin=Vector2.zero;filter.anchorMax=Vector2.one;filter.offsetMin=filter.offsetMax=Vector2.zero;}
        // Fit the entire original video to the screen; no crop or separate title.
        foreach(var player in FindObjectsByType<FramePlayer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            var image=player.targetImage;
            if(image==null||image.sprite==null)continue;
            var fit=image.GetComponent<UnityEngine.UI.AspectRatioFitter>();if(fit!=null)fit.enabled=false;
            var rect=image.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            rect.localScale=Vector3.one;image.preserveAspect=false;
            image.raycastTarget=false;
        }
        AturKreditJudul();
    }

    private void AturKreditJudul()
    {
        GameObject penutup = GameObject.Find("PenutupCredit");
        if (penutup == null) return;

        TMP_Text kredit = null;
        foreach (TMP_Text teks in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (teks != null && teks.text.IndexOf("Chipp", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                kredit = teks;
                break;
            }
        }
        if (kredit == null) return;

        RectTransform bidang = penutup.GetComponent<RectTransform>();
        Image kertas = penutup.GetComponent<Image>();
        if (bidang == null || kertas == null) return;

        // Label kertas tetap menutup kredit putih yang tertanam pada frame logo.
        // Lebihkan bidang hingga melewati tepi layar supaya watermark di pojok tidak bocor.
        bidang.anchorMin = bidang.anchorMax = bidang.pivot = new Vector2(1f, 0f);
        bidang.sizeDelta = new Vector2(290f, 86f);
        bidang.anchoredPosition = Vector2.zero;
        bidang.localRotation = Quaternion.identity;
        bidang.SetAsLastSibling();
        kertas.color = Color.white;
        kertas.raycastTarget = true;

        Outline garis = penutup.GetComponent<Outline>();
        if (garis == null) garis = penutup.AddComponent<Outline>();
        garis.effectColor = new Color32(55, 38, 27, 245);
        garis.effectDistance = new Vector2(2.5f, -2.5f);
        garis.useGraphicAlpha = true;

        // Label tampil seperti tombol menu, tetapi tidak diberi fungsi klik.
        Button tombolKredit = penutup.GetComponent<Button>();
        if (tombolKredit == null) tombolKredit = penutup.AddComponent<Button>();
        tombolKredit.targetGraphic = kertas;
        tombolKredit.transition = Selectable.Transition.ColorTint;
        tombolKredit.interactable = true;
        ColorBlock warnaTombol = ColorBlock.defaultColorBlock;
        warnaTombol.normalColor = new Color32(218, 218, 218, 255);
        warnaTombol.highlightedColor = new Color32(242, 242, 242, 255);
        warnaTombol.pressedColor = new Color32(165, 165, 165, 255);
        warnaTombol.selectedColor = new Color32(232, 232, 232, 255);
        warnaTombol.disabledColor = new Color32(175, 175, 175, 255);
        warnaTombol.fadeDuration = .08f;
        tombolKredit.colors = warnaTombol;

        kredit.transform.SetParent(bidang, false);
        kredit.gameObject.name = "KreditPembuat";
        RectTransform label = kredit.rectTransform;
        label.anchorMin = Vector2.zero;
        label.anchorMax = Vector2.one;
        label.pivot = new Vector2(.5f, .5f);
        label.offsetMin = new Vector2(12f, 5f);
        label.offsetMax = new Vector2(-12f, -5f);
        label.anchoredPosition = Vector2.zero;
        label.localRotation = Quaternion.identity;

        TMP_FontAsset fontUtama = Resources.Load<TMP_FontAsset>("Fonts & Materials/AmaticSC-Regular SDF");
        if (fontUtama != null)
        {
            kredit.font = fontUtama;
            kredit.fontSharedMaterial = fontUtama.material;
        }
        kredit.text = "By: Chippzzz";
        kredit.fontSize = 43f;
        kredit.fontStyle = FontStyles.Normal;
        kredit.color = new Color32(52, 38, 29, 255);
        kredit.alignment = TextAlignmentOptions.Center;
        kredit.enableAutoSizing = false;
        kredit.enableWordWrapping = false;
        kredit.raycastTarget = false;
        kredit.margin = Vector4.zero;

        WigglyContainer goyang = penutup.GetComponent<WigglyContainer>();
        if (goyang != null) {goyang.enabled=false;goyang.AturPosisiDasar();}
        kredit.ForceMeshUpdate();
    }

    // ==========================================
    // FUNGSI-FUNGSI TOMBOL TITLE SCREEN
    // ==========================================
    
    // --- TOMBOL MULAI & POP UP ---
    public void KlikTombolMulai() { 
        if(warningPopup != null) warningPopup.SetActive(true); 
    }
    
    public void BatalMain() { 
        if(warningPopup != null) warningPopup.SetActive(false); 
    }
    
    // 👇👇👇 INI BAGIAN YANG SUDAH KAKAK REVISI TOTAL 👇👇👇
    public void KonfirmasiMain() { 
        LoadingManager.sceneTujuan = "CharacterLab";
        SutradaraTransisi.sceneTujuanUtama = null;
        Debug.Log("Melempar pemain ke ruang transisi Arthur...");
        // Langsung panggil scene transisi sesuai GDD Fase 0!
        SceneManager.LoadScene("Transisi_Arthur"); 
    }
    // 👆👆👆 ========================================== 👆👆👆

    // --- TOMBOL EKSPLORASI ---
    public void KlikTombolEksplorasi() { 
        if (PlayerPrefs.GetInt("GameFinished", 0) != 1) return;
        LoadingManager.sceneTujuan = namaSceneExplorer;
        SutradaraTransisi.sceneTujuanUtama = null;
        SceneManager.LoadScene("LoadingScene");
    }

    // --- TOMBOL PENGATURAN ---
    public void KlikTombolPengaturan() { 
        if(pengaturanPanel != null) pengaturanPanel.SetActive(true); 
    }

    // --- TOMBOL PENGHARGAAN & LINK NOVELLA ---
    public void KlikTombolPenghargaan() { 
        if(penghargaanPanel != null) penghargaanPanel.SetActive(true); 
    }

    // Fungsi Sakti Buka Google Drive (Sesuai GDD Bab 4.4 - Auto Quit)
    public void BukaNovella(string linkGoogleDrive)
    {
        Debug.Log("Membuka Novella di Browser...");
        Application.OpenURL(linkGoogleDrive); // Buka Browser ke GDrive
        Application.Quit(); // Langsung matikan game biar RAM Android lega!
    }

    // --- TOMBOL KELUAR ---
    public void KlikTombolKeluar() { 
        Debug.Log("Sistem Ditutup!"); 
        Application.Quit(); 
    }
}
