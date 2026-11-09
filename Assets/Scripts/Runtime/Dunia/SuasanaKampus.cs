using UnityEngine;

// Effects live in their own groups and never change the player's routes or camera.
public sealed class SuasanaKampus : MonoBehaviour
{
    public GameObject suasanaGA, suasanaGK2;
    public GameObject suasanaJasmine, suasanaGH;
    public GameObject suasanaTamanUNKLAB;
    public GameObject interiorPioneer;
    public AudioSource angin, burung;
    public AudioSource jangkrik, kodok;
    [Range(0, 1)] public float volumeAngin = .22f;
    [Range(0, 1)] public float volumeBurung = .16f;
    ArthurJalurKampus arthur;
    bool anginDijeda,burungDijeda,jangkrikDijeda,kodokDijeda;
    void Start()
    {
        arthur = FindFirstObjectByType<ArthurJalurKampus>();
        if (angin != null) { angin.volume = 0; angin.Play(); }
        if (burung != null) { burung.volume = 0; burung.Play(); }
        if (jangkrik != null) { jangkrik.volume = 0; jangkrik.Play(); }
        if (kodok != null) { kodok.volume = 0; kodok.Play(); }
    }
    void Update()
    {
        string route = arthur != null && arthur.JalurAktif != null ? arthur.JalurAktif.name : "";
        bool taman = route.StartsWith("UNK_");
        bool tamanLuar=taman&&!route.EndsWith("_Dalam");
        if(suasanaTamanUNKLAB!=null&&suasanaTamanUNKLAB.activeSelf!=tamanLuar)suasanaTamanUNKLAB.SetActive(tamanLuar);
        bool ga = route.StartsWith("GA_"), gk2 = route.StartsWith("GK2_");
        bool jasmine = arthur != null && arthur.JalurAktif == null && arthur.fisikDepanJasmine != null && arthur.fisikDepanJasmine.activeInHierarchy;
        bool gh = route == "GH_LantaiSatu";
        if (suasanaJasmine != null && suasanaJasmine.activeSelf != jasmine) suasanaJasmine.SetActive(jasmine);
        if (suasanaGH != null && suasanaGH.activeSelf != gh) suasanaGH.SetActive(gh);
        if (suasanaGA != null && suasanaGA.activeSelf != ga) suasanaGA.SetActive(ga);
        if (suasanaGK2 != null && suasanaGK2.activeSelf != gk2) suasanaGK2.SetActive(gk2);
        bool chapel = route.StartsWith("Pioneer_");
        if (interiorPioneer != null && interiorPioneer.activeSelf != chapel) interiorPioneer.SetActive(chapel);
        bool indoor = route.Contains("Lobby") || route.Contains("Lorong") || route.Contains("LantaiDua") || route.Contains("LantaiTiga");
        float level = ga || gk2 || jasmine || gh || taman ? (indoor ? .22f : 1f) : 0f;
        float blend = 1 - Mathf.Exp(-2.5f * Time.unscaledDeltaTime);
        Campur(angin,volumeAngin*level,blend,ref anginDijeda);
        bool malam=SiklusWaktuKampus.Instance!=null&&SiklusWaktuKampus.Instance.waktu==WaktuKampus.Malam;
        Campur(burung,malam?0:volumeBurung*level*(indoor?.5f:1f),blend,ref burungDijeda);
        Campur(jangkrik,gh||malam&&tamanLuar?.15f:0,blend,ref jangkrikDijeda);
        Campur(kodok,gh?.13f:0,blend,ref kodokDijeda);
    }
    static void Campur(AudioSource sumber,float target,float blend,ref bool dijeda)
    {
        if(sumber==null)return;
        if(target>0&&dijeda){sumber.UnPause();dijeda=false;}
        sumber.volume=Mathf.Lerp(sumber.volume,target,blend);
        if(target==0&&sumber.volume<.001f&&!dijeda){sumber.volume=0;sumber.Pause();dijeda=true;}
    }
}
