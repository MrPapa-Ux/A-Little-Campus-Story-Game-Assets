using UnityEngine;
using UnityEngine.SceneManagement;

public enum WaktuKampus { Pagi, Sore, Malam }

// Waktu suasana terpisah dari timer tantangan/cerita. Tidak mengubah timer Arthur.
[DefaultExecutionOrder(250)]
public sealed class SiklusWaktuKampus : MonoBehaviour
{
    public static SiklusWaktuKampus Instance { get; private set; }
    public WaktuKampus waktu = WaktuKampus.Pagi;
    public bool berjalanOtomatis = true;
    [Min(60)] public float detikSatuHari=720;
    public const float JamMulaiPermainan = 7f;
    public float Jam {get;private set;}=JamMulaiPermainan;
    float jamTujuan=-1;
    [Min(30)] public float durasiPagi = 480, durasiSore = 300, durasiMalam = 420;
    [Min(.1f)] public float durasiPeralihan = 6;
    public Light matahari;
    [Range(0,1)] public float peluangLangitHijau=.01f;
    public bool LangitHijau {get;private set;}
    public bool Hujan {get;private set;}
    [Header("Langit hujan")]
    [Min(.1f)] public float durasiAwanReda = 5, jedaMendungReda = 4, durasiCerahKembali = 6;
    [SerializeField, Range(0,4)] int tingkatPeluangHujan;
    public float PeluangHujanBerikutnya => PeluangHujanUntuk(tingkatPeluangHujan);
    public float KepadatanAwan {get;private set;}
    public float KadarMendung {get;private set;}
    bool hujanVisualTerakhir;
    float detikReda, awanSaatReda, mendungSaatReda;
    HujanKampusArthur tiraiHujan;
    PerlengkapanHujanArthur perlengkapanHujan;
    float bulanAwal;
    bool arahBulanSiap;
    public bool DiLuarRuangan {get;private set;}=true;
    ArthurJalurKampus arthur;SistemPilihan pilihan;string kawasanLangit;
    public float DetikFase { get; private set; }
    WaktuKampus diterapkan;
    Material langitAsli, langit;
    Color warnaAwal, ambientAwal, equatorAwal, groundAwal, langitAwal, tanahLangitAwal;
    Quaternion sudutAwal;
    float intensitasAwal, awalPeralihan, exposureAwal;
    bool siap;
    void Awake(){Instance=this;}
    void Start()
    {
        arthur=FindFirstObjectByType<ArthurJalurKampus>();pilihan=FindFirstObjectByType<SistemPilihan>();
        if(matahari==null)matahari=RenderSettings.sun;
        if(matahari==null)foreach(var lampu in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if(lampu.type==LightType.Directional){matahari=lampu;break;}
        if(matahari!=null)RenderSettings.sun=matahari;
        langitAsli=RenderSettings.skybox;
        if(langitAsli!=null){langit=new Material(langitAsli);RenderSettings.skybox=langit;}
        // Scene yang pernah disimpan pada malam hari tetap memulai sesi baru pukul 07.00.
        waktu=WaktuKampus.Pagi;Jam=JamMulaiPermainan;jamTujuan=-1;DetikFase=0;berjalanOtomatis=true;
        AmbilAwal();diterapkan=waktu;awalPeralihan=Time.unscaledTime-durasiPeralihan;siap=true;Terapkan(1);
    }
    void OnEnable(){SceneManager.sceneLoaded+=SceneLoaded;}
    void OnDisable(){SceneManager.sceneLoaded-=SceneLoaded;}
    void SceneLoaded(Scene s,LoadSceneMode m){if(siap)Terapkan(Mathf.Clamp01((Time.unscaledTime-awalPeralihan)/durasiPeralihan));}
    void AmbilAwal()
    {
        warnaAwal=matahari?matahari.color:Color.white;intensitasAwal=matahari?matahari.intensity:1;
        sudutAwal=matahari?matahari.transform.rotation:Quaternion.identity;
        ambientAwal=RenderSettings.ambientSkyColor;equatorAwal=RenderSettings.ambientEquatorColor;groundAwal=RenderSettings.ambientGroundColor;
        langitAwal=langit!=null&&langit.HasProperty("_SkyTint")?langit.GetColor("_SkyTint"):new Color(.5f,.62f,.76f);
        tanahLangitAwal=langit!=null&&langit.HasProperty("_GroundColor")?langit.GetColor("_GroundColor"):new Color(.4f,.4f,.4f);
        exposureAwal=langit!=null&&langit.HasProperty("_Exposure")?langit.GetFloat("_Exposure"):1;
        bulanAwal = langit != null && langit.HasProperty("_Bulan") ? langit.GetFloat("_Bulan") : 0;
    }
    void Update()
    {
        if(!siap)return;
        PeriksaKawasan();
        if(waktu!=diterapkan)
        {
            jamTujuan=waktu==WaktuKampus.Pagi?JamMulaiPermainan:waktu==WaktuKampus.Sore?16:20;
            AmbilAwal();
            diterapkan=waktu;
            awalPeralihan=Time.unscaledTime;
            DetikFase=0;
            UndiCuaca(Random.value);
            arahBulanSiap = false;
        }
        var dialog=DialogManager.Instance;
        bool jeda=(dialog!=null&&(dialog.PercakapanAktif||dialog.minigameAktif))||(KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat);
        if(!jeda&&jamTujuan>=0){
            float selisih=Mathf.Repeat(jamTujuan-Jam,24);float langkah=Mathf.Min(.1f,Time.deltaTime)*24/Mathf.Max(18,durasiPeralihan);
            Jam=Mathf.Repeat(Jam+Mathf.Min(selisih,langkah),24);
            if(selisih<=langkah+.0001f){Jam=jamTujuan;jamTujuan=-1;}
        }
        else if(berjalanOtomatis&&!jeda){
            DetikFase+=Time.deltaTime;Jam=JamSetelah(Jam,Time.deltaTime,detikSatuHari);
            var fase=FaseJam(Jam);if(fase!=waktu){waktu=diterapkan=fase;DetikFase=0;UndiCuaca(Random.value);}
        }
        PerbaruiLangitHujan(Time.deltaTime);
        Terapkan(Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.unscaledTime-awalPeralihan)/Mathf.Max(.1f,durasiPeralihan))));
        bool tertutup = (KawasanKampus.Instance != null && KawasanKampus.Instance.SedangMemuat) ||
            (pilihan != null && pilihan.efekTransisi != null && pilihan.efekTransisi.SedangBerjalan);
        if (Hujan && perlengkapanHujan == null && arthur != null)
        {
            perlengkapanHujan = gameObject.AddComponent<PerlengkapanHujanArthur>();
            var pemain = arthur.GetComponent<KarakterController>();
            perlengkapanHujan.Siapkan(pemain != null ? pemain.wadahVisual : null);
        }
        if (perlengkapanHujan != null)
            perlengkapanHujan.Atur(Hujan, DiLuarRuangan, tertutup, jeda);
        bool tampilHujan = Hujan && DiLuarRuangan && !jeda && !tertutup;
        if (tampilHujan && tiraiHujan == null) tiraiHujan = HujanKampusArthur.Buat(transform);
        if (tiraiHujan != null && tiraiHujan.gameObject.activeSelf != tampilHujan)
            tiraiHujan.gameObject.SetActive(tampilHujan);
    }
    void Terapkan(float t)
    {
        bool sore=waktu==WaktuKampus.Sore,malam=waktu==WaktuKampus.Malam;
        float terang=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.14f,.32f,Mathf.Sin((Jam-6)*Mathf.PI/12)));
        float senja=terang*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.08f,.60f,Mathf.Sin((Jam-6)*Mathf.PI/12))));
        Color cahaya=Color.Lerp(new Color(.56f,.67f,1),Color.Lerp(new Color(1,.97f,.91f),new Color(1,.72f,.47f),senja),terang);
        Color sky=Color.Lerp(new Color(.10f,.15f,.29f),Color.Lerp(new Color(.32f,.53f,.79f),new Color(.72f,.38f,.30f),senja),terang);
        Color horizon=Color.Lerp(new Color(.12f,.18f,.30f),Color.Lerp(new Color(.62f,.78f,.94f),new Color(.85f,.60f,.45f),senja),terang);
        float mendungAktif = DiLuarRuangan ? KadarMendung : 0f;
        if (mendungAktif > 0)
        {
            Color mendung = malam ? new Color(.045f,.06f,.09f) : new Color(.23f,.27f,.31f);
            sky = Color.Lerp(sky, mendung, mendungAktif);
            horizon = Color.Lerp(horizon, mendung * 1.15f, mendungAktif * .94f);
        }
        if(LangitHijau&&DiLuarRuangan){sky=Color.Lerp(sky,new Color(.32f,.53f,.38f),(malam?.25f:.65f)*(1-mendungAktif));horizon=Color.Lerp(horizon,new Color(.48f,.66f,.48f),(malam?.25f:.8f)*(1-mendungAktif));}
        Color ambient=Color.Lerp(new Color(.18f,.23f,.36f),Color.Lerp(new Color(.53f,.62f,.72f),new Color(.52f,.40f,.34f),senja),terang);
        ambient=Color.Lerp(ambient,malam?new Color(.18f,.21f,.28f):new Color(.43f,.47f,.52f),mendungAktif);
        cahaya=Color.Lerp(cahaya,new Color(.69f,.75f,.83f),mendungAktif);
        // Interiors retain a readable warm fill instead of receiving full outdoor sunlight.
        if(!DiLuarRuangan){ambient=new Color(.57f,.55f,.51f);cahaya=new Color(1,.92f,.81f);}
        if(matahari){matahari.color=Color.Lerp(warnaAwal,cahaya,t);matahari.intensity=Mathf.Lerp(intensitasAwal,DiLuarRuangan?Mathf.Lerp(.18f,1.02f,terang)*Mathf.Lerp(1,.40f,mendungAktif):.28f,t);matahari.transform.rotation=Quaternion.Slerp(matahari.transform.rotation,Quaternion.Euler((Jam<6||Jam>=18?Jam*15+90:Jam*15-90),150,0),Application.isPlaying?1-Mathf.Exp(-Time.unscaledDeltaTime*3):1);}
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=Color.Lerp(ambientAwal,ambient,t);
        RenderSettings.ambientEquatorColor=Color.Lerp(equatorAwal,ambient*.82f,t);
        RenderSettings.ambientGroundColor=Color.Lerp(groundAwal,ambient*.58f,t);
        // The mobile vertex shader uses the same live gradient, without stale baked probes.
        Shader.SetGlobalColor("_ArthurAmbientSky",RenderSettings.ambientSkyColor);
        Shader.SetGlobalColor("_ArthurAmbientEquator",RenderSettings.ambientEquatorColor);
        Shader.SetGlobalColor("_ArthurAmbientGround",RenderSettings.ambientGroundColor);
        Shader.SetGlobalFloat("_ArthurAmbientActif",1);
        if(langit!=null&&langit.HasProperty("_Matahari"))
        {
            var surya=Quaternion.Euler(Jam*15-90,150,0)*Vector3.back;
            langit.SetFloat("_Matahari",DiLuarRuangan?(1-KepadatanAwan)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.03f,.06f,surya.y)):0);
            langit.SetVector("_ArahMatahari",surya);
            langit.SetColor("_WarnaMatahari",Color.Lerp(new Color(1,.93f,.66f),new Color(1,.64f,.30f),senja));
        }
        if(langit!=null&&langit.HasProperty("_SkyTint")){if(RenderSettings.skybox!=langit)RenderSettings.skybox=langit;langit.SetColor("_SkyTint",Color.Lerp(langitAwal,sky,t));if(langit.HasProperty("_Exposure"))langit.SetFloat("_Exposure",Mathf.Lerp(exposureAwal,Mathf.Lerp(.45f,1f,terang),t));if(langit.HasProperty("_GroundColor"))langit.SetColor("_GroundColor",Color.Lerp(tanahLangitAwal,Color.Lerp(new Color(.12f,.16f,.24f),Color.Lerp(new Color(.40f,.55f,.75f),new Color(.45f,.32f,.35f),senja),terang),t));if(langit.HasProperty("_HorizonColor"))langit.SetColor("_HorizonColor",horizon);if(langit.HasProperty("_Mountains"))langit.SetFloat("_Mountains",DiLuarRuangan?1:0);if(langit.HasProperty("_MountainTint"))langit.SetColor("_MountainTint",Color.Lerp(sky,horizon,.25f)*.78f);}
        if(RenderSettings.fog)RenderSettings.fogColor=Color.Lerp(RenderSettings.ambientSkyColor,sky,.5f);
        if(langit!=null&&langit.HasProperty("_Awan"))langit.SetFloat("_Awan",DiLuarRuangan?KepadatanAwan:0f);
        if (langit != null && langit.HasProperty("_Bulan"))
        {
            if (!arahBulanSiap && malam && DiLuarRuangan && arthur != null && arthur.kamera != null)
            {
                Vector3 arah = arthur.kamera.ViewportPointToRay(new Vector3(.64f, .91f, 0)).direction;
                arah.y = Mathf.Max(.15f, arah.y);
                langit.SetVector("_ArahBulan", arah.normalized);
                arahBulanSiap = true;
            }
            langit.SetVector("_ArahBulan",Quaternion.Euler(Jam*15+90,150,0)*Vector3.back);
            langit.SetFloat("_Bulan", DiLuarRuangan ? (1-terang)*Mathf.Lerp(1f,.08f,KepadatanAwan) : 0f);
        }
    }
    public static float JamSetelah(float jam,float detik,float durasiHari=720)=>Mathf.Repeat(jam+detik*24/Mathf.Max(60,durasiHari),24);
    public static WaktuKampus FaseJam(float jam)=>jam>=6&&jam<16?WaktuKampus.Pagi:jam>=16&&jam<18?WaktuKampus.Sore:WaktuKampus.Malam;
    public void AturJam(float jam){AmbilAwal();Jam=Mathf.Repeat(jam,24);waktu=diterapkan=FaseJam(Jam);jamTujuan=-1;DetikFase=0;awalPeralihan=Time.unscaledTime;}
    public void AturWaktu(WaktuKampus pilihan){waktu=pilihan;DetikFase=0;jamTujuan=pilihan==WaktuKampus.Pagi?JamMulaiPermainan:pilihan==WaktuKampus.Sore?16:20;}
    public static bool PilihLangitHijau(float undian,float peluang=.01f)=>undian<Mathf.Clamp01(peluang);
    public static float PeluangHujanUntuk(int tingkat) => tingkat <= 0 ? .01f : tingkat == 1 ? .10f : tingkat == 2 ? .50f : tingkat == 3 ? .70f : 1f;
    public static bool PilihHujan(float undian, float peluang = .01f) => undian >= 0f && undian <= 1f && (peluang >= 1f || undian < Mathf.Clamp01(peluang));
    void UndiCuaca(float undian)
    {
        Hujan=PilihHujan(undian,PeluangHujanBerikutnya);
        if(Hujan)tingkatPeluangHujan=(tingkatPeluangHujan+1)%5;
    }
    void PerbaruiLangitHujan(float delta)
    {
        if (Hujan)
        {
            KepadatanAwan = Mathf.MoveTowards(KepadatanAwan, 1f, delta / Mathf.Max(.1f,durasiPeralihan));
            KadarMendung = Mathf.MoveTowards(KadarMendung, 1f, delta / Mathf.Max(.1f,durasiPeralihan));
        }
        else
        {
            if (hujanVisualTerakhir)
            {
                detikReda=0; awanSaatReda=KepadatanAwan; mendungSaatReda=KadarMendung;
            }
            detikReda+=delta;
            // Awan menipis dahulu, langit tetap mendung sesaat, kemudian kembali cerah.
            KepadatanAwan=Mathf.Lerp(awanSaatReda,0,Mathf.SmoothStep(0,1,detikReda/Mathf.Max(.1f,durasiAwanReda)));
            KadarMendung=Mathf.Lerp(mendungSaatReda,0,Mathf.SmoothStep(0,1,(detikReda-durasiAwanReda-jedaMendungReda)/Mathf.Max(.1f,durasiCerahKembali)));
        }
        hujanVisualTerakhir=Hujan;
    }
#if UNITY_EDITOR || UNITY_ANDROID
    public void PratinjauHujan(bool aktif) => Hujan = aktif;
#endif
    void PeriksaKawasan()
    {
        var k=KawasanKampus.Instance;if(k==null||!k.Siap||k.SedangMemuat||arthur==null)return;
        bool luar=arthur.JalurAktif!=null?!arthur.JalurAktif.diDalamRuangan:pilihan==null||pilihan.grupFisikRuangan==null||!pilihan.grupFisikRuangan.activeInHierarchy;
        if(DiLuarRuangan!=luar)
        {
            // Mulai dari cahaya yang sedang terlihat, bukan langsung meloncat
            // antara isi lobby dan jalur taman saat Arthur melewati batas jalur.
            AmbilAwal();
            awalPeralihan=Time.unscaledTime;
        }
        DiLuarRuangan=luar;
        if(!luar){LangitHijau=false;kawasanLangit=null;return;}
        string zona=PertemuanHewanKampus.ZonaUntuk(arthur.JalurAktif);string key=k.KawasanAktif+"/"+zona;
        if(key==kawasanLangit)return;kawasanLangit=key;LangitHijau=PilihLangitHijau(Random.value,peluangLangitHijau);
    }
    public void PratinjauHijau(bool aktif){LangitHijau=aktif&&DiLuarRuangan;}
    [ContextMenu("Coba Pagi")] void Pagi(){AturWaktu(WaktuKampus.Pagi);}
    [ContextMenu("Coba Sore")] void Sore(){AturWaktu(WaktuKampus.Sore);}
    [ContextMenu("Coba Malam")] void Malam(){AturWaktu(WaktuKampus.Malam);}
    void OnDestroy(){if(Instance==this){Instance=null;Shader.SetGlobalFloat("_ArthurAmbientActif",0);}if(langit!=null){if(RenderSettings.skybox==langit)RenderSettings.skybox=langitAsli;Destroy(langit);}}
}
