// PANDUAN PROYEK - Runtime / ParameterDanGlitch
// Fungsi: Mengatur parameter lazy bar Arthur dan hubungan kondisi pemain dengan efek glitch.
// Panduan: Assets/Scripts/Runtime/ParameterDanGlitch/README.md

using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;
using UnityEngine.SceneManagement; 

public class LazyBarArthur : MonoBehaviour
{
    [Header("Referensi UI Utama")]
    public Image lazyBarFill;
    public Image lazyIkonFill; 
    public RectTransform lazyBarUtama; 

    [Header("Referensi Efek Glitch")]
    public GameObject panelGlitch;
    public TextMeshProUGUI teksGlitch;
    public EfekGlitchLayar efekGlitchLayar;
    public string namaSceneLoading = "LoadingScene";

    [Header("Deteksi UI Otomatis")]
    [Tooltip("Tarik objek UI Panel Dialog / Canvas Dialog milikmu ke sini!")]
    public GameObject panelDialogUtama; 
    
    // Referensi untuk deteksi HP (Otomatis dicari oleh sistem)
    private SistemHP referensiHP; 
    private PerjalananChapelChapter1 ceritaChapel;
    private KarakterController pengendaliArthur;

    [Header("Pengaturan Teks Glitch")]
    public float durasiSpam = 1.2f;
    public int jumlahKataPerFrame = 5;
    public float jedaSebelumPindahScene = 0.3f;
    public string[] kataGlitch = { "MALAS ", "ERROR_404 ", "BANGUN ", "SISTEM_GAGAL ", "011001 ", "!!! ", "REBOOTING... ", "FATAL_ERR ", "NULL " };

    [Header("Pengaturan Waktu")]
    [Tooltip("Khusus Explorer: bar selalu penuh dan tidak memicu gagal karena waktu habis.")]
    public bool tanpaBatasWaktu = false;
    [Tooltip("Khusus cerita Arthur: baca langkah nyata pada jalur kampus maupun gerak bebas Jasmine. HP selalu menjeda bar.")]
    public bool aturanCeritaArthur;
    public bool JedaKhususCerita {get;set;}
    public float batasWaktuMalas = 15f;
    private float sisaWaktu;
    [Tooltip("Opsional untuk adegan cerita: pemulihan momen terekam tanpa memuat ulang seluruh pembuka.")]
    public UnityEngine.Events.UnityEvent ketikaGlitchSelesai = new UnityEngine.Events.UnityEvent();
    [Min(0)] public float jedaSebelumBerkurang;
    float waktuDiam;
    public float RasioTersisa => Mathf.Clamp01(sisaWaktu/Mathf.Max(.01f,batasWaktuMalas));
    public void PulihkanBar(){
        StopAllCoroutines();waktuHabis=false;sisaWaktu=batasWaktuMalas;waktuDiam=0;
        if(efekGlitchLayar!=null)efekGlitchLayar.Gelapkan();
        if(panelGlitch!=null)panelGlitch.SetActive(false);
        if(lazyBarFill!=null)lazyBarFill.fillAmount=1;
        if(lazyIkonFill!=null)lazyIkonFill.fillAmount=1;
        ResetEfekVisual();
    }
    bool HPMenjedaParameter {
        get {
            if(referensiHP==null)referensiHP=FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include);
            return referensiHP!=null&&referensiHP.MenjedaParameter;
        }
    }
    bool MalamCeritaAktif=>ceritaChapel!=null&&ceritaChapel.isActiveAndEnabled&&ceritaChapel.MalamAktif;
    public void GagalKarenaBatasWaktu(){if(!waktuHabis&&!tanpaBatasWaktu&&!JedaKhususCerita&&!MalamCeritaAktif&&!HPMenjedaParameter)PicuEfekGlitch();}
    // Timer quest malam terpisah dari bar yang sengaja disembunyikan saat ibadah/pulang.
    public void GagalKarenaBatasWaktuQuest(){if(!waktuHabis&&!JedaKhususCerita&&!HPMenjedaParameter)PicuEfekGlitch();}

    [Header("Status Pergerakan & Sistem")]
    public bool sedangBergerak = false;
    public bool waktuHabis = false; 
    public bool sedangDialog = false; 

    [Header("Pengaturan Efek Juicy")]
    public float kecepatanNapasNormal = 2f;
    public float kecepatanNapasPanik = 15f;
    public float intensitasNapas = 0.03f; 
    public float intensitasGoyang = 3f;  
    [Range(0.05f, 0.8f)] public float ambangPanik = 0.3f;

    [Header("Guncangan Kritis")]
    [Min(1f)] public float pengaliGoyangKritis = 4f;
    [Min(0f)] public float jarakGuncang = 18f;
    [Range(1f, 20f)] public float frekuensiGuncang = 12f;
    [Min(1f)] public float pengaliNapasKritis = 2f;

    [Header("Pengaturan Anomali Warna")]
    public Color warnaNormal = new Color(1f, 0.5f, 0f); 
    public Color warnaAnomali = Color.red; 

    private Vector3 skalaAwal;
    private Quaternion rotasiAwal;
    private Vector2 posisiAwal;
    private float faseEfek;
    private float faseGuncang;
    private float panikHalus;

    void Start()
    {
        sisaWaktu = batasWaktuMalas;
        waktuHabis = false;
        
        if (lazyBarUtama != null) 
        {
            skalaAwal = lazyBarUtama.localScale;
            rotasiAwal = lazyBarUtama.localRotation;
            posisiAwal = lazyBarUtama.anchoredPosition;
            foreach (UnityEngine.UI.Graphic graphic in lazyBarUtama.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
                graphic.raycastTarget = false;
        }
        if (lazyBarFill != null) lazyBarFill.raycastTarget = false;
        if (lazyIkonFill != null) lazyIkonFill.raycastTarget = false;

        if (lazyBarFill != null) lazyBarFill.fillAmount = 1f;
        ResetEfekVisual();

        if (panelGlitch != null) panelGlitch.SetActive(false);

        // Cari skrip HP secara otomatis tanpa bikin warning kuning!
        referensiHP = FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include);
        ceritaChapel = FindFirstObjectByType<PerjalananChapelChapter1>();
        pengendaliArthur = GetComponent<KarakterController>();
    }

    void LateUpdate()
    {
        if (tanpaBatasWaktu)
        {
            sisaWaktu = batasWaktuMalas;
            waktuHabis = false;
            if (lazyBarFill != null) lazyBarFill.fillAmount = 1f;
            if (lazyIkonFill != null) lazyIkonFill.fillAmount = 1f;
            ResetEfekVisual();
            return;
        }

        // 1. DETEKSI DIALOG & HP OTOMATIS
        sedangDialog = (panelDialogUtama != null && panelDialogUtama.activeInHierarchy) ||
            (DialogManager.Instance != null && DialogManager.Instance.PercakapanAktif);
        if (referensiHP == null)
            referensiHP = FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include);

        bool sedangBukaHP = false;
        if (referensiHP != null)
        {
            sedangBukaHP = referensiHP.MenjedaParameter;
        }

        // 2. KUNCI BAR! (Berhenti kalau waktu habis, ada dialog, ATAU HP lagi dibuka!)
        if (waktuHabis) return;
        if(JedaKhususCerita||MalamCeritaAktif){TenangkanEfekVisual();return;}
        if (sedangDialog || sedangBukaHP || (DialogManager.Instance!=null&&DialogManager.Instance.minigameAktif) || (KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat))
        {
            TenangkanEfekVisual();
            return;
        }

        // Cerita memakai perpindahan nyata, bukan sentuhan layar/ikon aplikasi.
        if(aturanCeritaArthur)sedangBergerak=pengendaliArthur!=null&&pengendaliArthur.KecepatanAktual>.05f;
        else sedangBergerak=Input.GetAxisRaw("Horizontal")!=0||Input.GetAxisRaw("Vertical")!=0||Input.GetMouseButton(0);

        // 4. Logika Kurang/Tambah Bar
        if (!sedangBergerak)
        {
            waktuDiam+=Time.deltaTime;
            if(waktuDiam>=jedaSebelumBerkurang)sisaWaktu -= Time.deltaTime;
            if (sisaWaktu <= 0)
            {
                sisaWaktu = 0;
                if (lazyBarFill != null) lazyBarFill.fillAmount = 0f;
                PicuEfekGlitch();
                return;
            }
        }
        else
        {
            waktuDiam=0;
            sisaWaktu += Time.deltaTime * 10f; 
            if (sisaWaktu >= batasWaktuMalas)
            {
                sisaWaktu = batasWaktuMalas;
            }
        }

        // 5. Update UI Bar
        if (lazyBarFill != null)
        {
            lazyBarFill.fillAmount = sisaWaktu / Mathf.Max(0.01f, batasWaktuMalas);
        }

        TerapkanEfekVisual();
    }

    private void TerapkanEfekVisual()
    {
        if (lazyBarUtama == null) return;

        float rasioWaktu = Mathf.Clamp01(sisaWaktu / Mathf.Max(0.01f, batasWaktuMalas));
        float targetPanik = Mathf.Clamp01((ambangPanik - rasioWaktu) / Mathf.Max(0.01f, ambangPanik));
        panikHalus = Mathf.Lerp(panikHalus, targetPanik, 1f - Mathf.Exp(-8f * Time.deltaTime));
        if (targetPanik == 0f && panikHalus < 0.001f)
        {
            TenangkanEfekVisual();
            return;
        }
        float rasioKepanikan = panikHalus;
        float kekuatanKritis = Mathf.Pow(rasioKepanikan, 1.35f);

        float kecepatanAktif = Mathf.Lerp(kecepatanNapasNormal, kecepatanNapasPanik, rasioKepanikan);

        faseEfek = Mathf.Repeat(faseEfek + Time.deltaTime * kecepatanAktif, Mathf.PI * 4f);
        float denyutNapas = Mathf.Sin(faseEfek) * intensitasNapas * rasioKepanikan * Mathf.Lerp(1f, pengaliNapasKritis, kekuatanKritis);
        lazyBarUtama.localScale = Vector3.Scale(skalaAwal, new Vector3(1f + denyutNapas, 1f + denyutNapas, 1f));

        // Ritme berbasis waktu dengan harmonik bulat: berulang konsisten,
        // tanpa posisi random, penumpukan offset, atau loncatan fase saat kecepatan berubah.
        float frekuensiAktif = frekuensiGuncang * Mathf.Lerp(0.6f, 1f, kekuatanKritis);
        faseGuncang = Mathf.Repeat(faseGuncang + Time.deltaTime * frekuensiAktif * Mathf.PI * 2f, Mathf.PI * 2f);
        float ayunan = Mathf.Sin(faseGuncang) * 0.7f + Mathf.Sin(faseGuncang * 3f) * 0.3f;
        float hentakanY = Mathf.Cos(faseGuncang * 2f) * 0.65f + Mathf.Cos(faseGuncang * 4f) * 0.35f;
        lazyBarUtama.anchoredPosition = posisiAwal + new Vector2(ayunan, hentakanY * 0.55f) * jarakGuncang * kekuatanKritis;
        float goyangan = ayunan * intensitasGoyang * pengaliGoyangKritis * kekuatanKritis;
        lazyBarUtama.localRotation = rotasiAwal * Quaternion.Euler(0, 0, goyangan);

        if (lazyIkonFill != null)
        {
            float kedipAnomali = (Mathf.Sin(faseEfek) + 1f) * 0.5f;
            lazyIkonFill.color = Color.Lerp(warnaNormal, warnaAnomali, kedipAnomali * rasioKepanikan);
        }
    }

    private void ResetEfekVisual()
    {
        panikHalus = 0f;
        faseEfek = 0f;
        faseGuncang = 0f;
        if (lazyBarUtama != null)
        {
            lazyBarUtama.localScale = skalaAwal;
            lazyBarUtama.localRotation = rotasiAwal;
            lazyBarUtama.anchoredPosition = posisiAwal;
        }
        if (lazyIkonFill != null) lazyIkonFill.color = warnaNormal;
    }

    // Jeda menenangkan visual tanpa memindahkan bar secara mendadak atau
    // mengulang fase getar setiap kali aplikasi/dialog berganti.
    private void TenangkanEfekVisual()
    {
        float t = 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime);
        panikHalus = Mathf.Lerp(panikHalus, 0f, t);
        if (lazyBarUtama != null)
        {
            lazyBarUtama.localScale = Vector3.Lerp(lazyBarUtama.localScale, skalaAwal, t);
            lazyBarUtama.localRotation = Quaternion.Slerp(lazyBarUtama.localRotation, rotasiAwal, t);
            lazyBarUtama.anchoredPosition = Vector2.Lerp(lazyBarUtama.anchoredPosition, posisiAwal, t);
        }
        if (lazyIkonFill != null) lazyIkonFill.color = Color.Lerp(lazyIkonFill.color, warnaNormal, t);
    }

    private void PicuEfekGlitch()
    {
        waktuHabis = true; 
        if(MusikArthur.Instance!=null)MusikArthur.Instance.HentikanSemua();
        var pemain=GetComponent<KarakterController>();
        if(pemain!=null){pemain.BerhentiBergerak();pemain.kunciPergerakan=true;}

        if (lazyBarUtama != null)
        {
            lazyBarUtama.localScale = skalaAwal;
            lazyBarUtama.localRotation = rotasiAwal;
            lazyBarUtama.anchoredPosition = posisiAwal;
        }
        if (lazyIkonFill != null)
        {
            lazyIkonFill.color = warnaAnomali; 
        }

        if (panelGlitch != null) panelGlitch.SetActive(true);
        StartCoroutine(BanjirTeksGlitch());
    }

    private IEnumerator BanjirTeksGlitch()
    {
        if(panelGlitch!=null)
        {
            if(efekGlitchLayar==null)efekGlitchLayar=panelGlitch.GetComponent<EfekGlitchLayar>();
            if(efekGlitchLayar==null)efekGlitchLayar=panelGlitch.AddComponent<EfekGlitchLayar>();
            efekGlitchLayar.Mulai(teksGlitch,kataGlitch);
        }
        Image gambarPanel=panelGlitch!=null?panelGlitch.GetComponent<Image>():null;
        if(gambarPanel!=null)
        {
            // Sprite bawaan Background memiliki transparansi sendiri; gunakan bidang solid.
            gambarPanel.sprite=null;gambarPanel.type=Image.Type.Simple;gambarPanel.enabled=true;
        }
        float timer=0,durasi=Mathf.Max(.1f,durasiSpam);
        while(timer<durasi)
        {
            timer+=Time.unscaledDeltaTime;
            float rasio=Mathf.Clamp01(timer/durasi);
            if(efekGlitchLayar!=null)efekGlitchLayar.Tampilkan(rasio);
            if(gambarPanel!=null)gambarPanel.color=new Color(0,0,0,Mathf.Lerp(.18f,.96f,rasio));
            yield return null;
        }
        if(efekGlitchLayar!=null)efekGlitchLayar.Gelapkan();
        if(gambarPanel!=null)gambarPanel.color=Color.black;
        yield return new WaitForSecondsRealtime(Mathf.Max(0,jedaSebelumPindahScene));
        if(ketikaGlitchSelesai!=null&&ketikaGlitchSelesai.GetPersistentEventCount()>0)ketikaGlitchSelesai.Invoke();
        else SceneManager.LoadScene(namaSceneLoading);
    }
}
