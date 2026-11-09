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
    [Min(30)] public float durasiPagi = 480, durasiSore = 300, durasiMalam = 420;
    [Min(.1f)] public float durasiPeralihan = 6;
    public Light matahari;
    [Range(0,1)] public float peluangLangitHijau=.01f;
    public bool LangitHijau {get;private set;}
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
        langitAsli=RenderSettings.skybox;
        if(langitAsli!=null){langit=new Material(langitAsli);RenderSettings.skybox=langit;}
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
    }
    void Update()
    {
        if(!siap)return;
        PeriksaKawasan();
        if(waktu!=diterapkan){AmbilAwal();diterapkan=waktu;awalPeralihan=Time.unscaledTime;DetikFase=0;}
        var dialog=DialogManager.Instance;
        bool jeda=(dialog!=null&&(dialog.PercakapanAktif||dialog.minigameAktif))||(KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat);
        if(berjalanOtomatis&&!jeda && !(waktu==WaktuKampus.Malam && MalamEksplorasiArthur.Instance!=null && MalamEksplorasiArthur.Instance.PulangOtomatis)){DetikFase+=Time.deltaTime;float lama=waktu==WaktuKampus.Pagi?durasiPagi:waktu==WaktuKampus.Sore?durasiSore:durasiMalam;if(DetikFase>=lama)AturWaktu((WaktuKampus)(((int)waktu+1)%3));}
        Terapkan(Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.unscaledTime-awalPeralihan)/Mathf.Max(.1f,durasiPeralihan))));
    }
    void Terapkan(float t)
    {
        bool sore=waktu==WaktuKampus.Sore,malam=waktu==WaktuKampus.Malam;
        Color cahaya=malam?new Color(.56f,.67f,1):sore?new Color(1,.72f,.47f):new Color(1,.97f,.91f);
        Color sky=malam?new Color(.10f,.15f,.29f):sore?new Color(.72f,.38f,.30f):new Color(.32f,.53f,.79f);
        Color horizon=malam?new Color(.12f,.18f,.30f):sore?new Color(.85f,.60f,.45f):new Color(.62f,.78f,.94f);
        if(LangitHijau&&DiLuarRuangan){sky=Color.Lerp(sky,new Color(.32f,.53f,.38f),malam?.25f:.65f);horizon=Color.Lerp(horizon,new Color(.48f,.66f,.48f),malam?.25f:.8f);}
        Color ambient=malam?new Color(.28f,.34f,.49f):sore?new Color(.65f,.48f,.39f):new Color(.72f,.78f,.84f);
        if(matahari){matahari.color=Color.Lerp(warnaAwal,cahaya,t);matahari.intensity=Mathf.Lerp(intensitasAwal,malam?.22f:sore?.85f:1.1f,t);matahari.transform.rotation=Quaternion.Slerp(sudutAwal,Quaternion.Euler(malam?38:sore?18:50,malam?130:sore?-65:-35,0),t);}
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=Color.Lerp(ambientAwal,ambient,t);
        RenderSettings.ambientEquatorColor=Color.Lerp(equatorAwal,ambient*.82f,t);
        RenderSettings.ambientGroundColor=Color.Lerp(groundAwal,ambient*.58f,t);
        if(langit!=null&&langit.HasProperty("_SkyTint")){if(RenderSettings.skybox!=langit)RenderSettings.skybox=langit;langit.SetColor("_SkyTint",Color.Lerp(langitAwal,sky,t));if(langit.HasProperty("_Exposure"))langit.SetFloat("_Exposure",Mathf.Lerp(exposureAwal,malam?.45f:sore?.85f:1f,t));if(langit.HasProperty("_GroundColor"))langit.SetColor("_GroundColor",Color.Lerp(tanahLangitAwal,malam?new Color(.12f,.16f,.24f):sore?new Color(.45f,.32f,.35f):new Color(.40f,.55f,.75f),t));if(langit.HasProperty("_HorizonColor"))langit.SetColor("_HorizonColor",horizon);if(langit.HasProperty("_Mountains"))langit.SetFloat("_Mountains",DiLuarRuangan?1:0);if(langit.HasProperty("_MountainTint"))langit.SetColor("_MountainTint",Color.Lerp(sky,horizon,.25f)*.78f);}
        if(RenderSettings.fog)RenderSettings.fogColor=Color.Lerp(RenderSettings.ambientSkyColor,sky,.5f);
    }
    public void AturWaktu(WaktuKampus pilihan){waktu=pilihan;DetikFase=0;}
    public static bool PilihLangitHijau(float undian,float peluang=.01f)=>undian<Mathf.Clamp01(peluang);
    void PeriksaKawasan()
    {
        var k=KawasanKampus.Instance;if(k==null||!k.Siap||k.SedangMemuat||arthur==null)return;
        bool luar=arthur.JalurAktif!=null?!arthur.JalurAktif.diDalamRuangan:pilihan==null||pilihan.grupFisikRuangan==null||!pilihan.grupFisikRuangan.activeInHierarchy;
        DiLuarRuangan=luar;
        if(!luar){LangitHijau=false;kawasanLangit=null;return;}
        string zona=PertemuanHewanKampus.ZonaUntuk(arthur.JalurAktif);string key=k.KawasanAktif+"/"+zona;
        if(key==kawasanLangit)return;kawasanLangit=key;LangitHijau=PilihLangitHijau(Random.value,peluangLangitHijau);
    }
    public void PratinjauHijau(bool aktif){LangitHijau=aktif&&DiLuarRuangan;}
    [ContextMenu("Coba Pagi")] void Pagi(){AturWaktu(WaktuKampus.Pagi);}
    [ContextMenu("Coba Sore")] void Sore(){AturWaktu(WaktuKampus.Sore);}
    [ContextMenu("Coba Malam")] void Malam(){AturWaktu(WaktuKampus.Malam);}
    void OnDestroy(){if(Instance==this)Instance=null;if(langit!=null){if(RenderSettings.skybox==langit)RenderSettings.skybox=langitAsli;Destroy(langit);}}
}
