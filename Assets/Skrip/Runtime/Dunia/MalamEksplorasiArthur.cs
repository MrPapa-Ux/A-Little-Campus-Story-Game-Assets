using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// NPC harian beristirahat saat malam. Adegan malam Chapter 1 tetap mengatur NPC ceritanya sendiri.
[DefaultExecutionOrder(600)]
public sealed class MalamEksplorasiArthur : MonoBehaviour
{
    public static MalamEksplorasiArthur Instance { get; private set; }
    public static bool NpcTidur => Instance != null && Instance.waktu != null && Instance.waktu.waktu == WaktuKampus.Malam;
    public SiklusWaktuKampus waktu;
    public Transform tujuanKamar;
    public float batasMalam = 180f;
    // CharacterLab boleh menampilkan siklus suasana untuk penataan, tetapi tidak memulangkan pemain.
    public bool PulangOtomatis => gameObject.scene.name == "Explorer" || gameObject.scene.name == "Penataan Kampus - Jalur NPC dan Bubble";
    public bool KamarDikunci { get; private set; }
    public bool SedangTidur { get; private set; }
    public static bool JamIstirahat(float jam) => jam >= 21 || jam < 6;
    public static bool JamTidurPaksa(float jam) => jam >= 23 || jam < 6;
    public static bool BolehPindah(Transform tujuan)
    {
        if(Instance==null || !Instance.KamarDikunci || tujuan==null)return true;
        var r=tujuan.GetComponent<TujuanJalurKampus>();
        return r!=null && r.jalur!=null && r.jalur.name=="GH_Kamar201";
    }
    [Min(.1f)] public float durasiPudarNpc = 1.5f;
    public float DetikMalam { get; private set; }
    public bool SedangPulang { get; private set; }
    readonly Dictionary<GameObject, bool> disembunyikan = new Dictionary<GameObject, bool>();
    readonly Dictionary<SpriteRenderer, Color> warnaNpc = new Dictionary<SpriteRenderer, Color>();
    readonly Dictionary<Behaviour, bool> interaksiNpc = new Dictionary<Behaviour, bool>();
    readonly HashSet<GameObject> sedangPudar = new HashSet<GameObject>();
    int versiPudar;
    bool malamLama;
    float cekBerikut;
    SistemHP hp;
    SistemPilihan pilihan;
    ArthurJalurKampus arthur;
    void Awake() { Instance = this; }
    void Start() { hp=FindFirstObjectByType<SistemHP>(); pilihan=FindFirstObjectByType<SistemPilihan>(); arthur=FindFirstObjectByType<ArthurJalurKampus>(); }
    void Sembunyikan(GameObject g)
    {
        if(g==null || g.GetComponentInParent<ArthurJalurKampus>()!=null)return;
        if(disembunyikan.ContainsKey(g))
        {
            // Grup kawasan dapat mengaktifkan NPC lagi setelah proses pudar selesai.
            if(g.activeSelf && !sedangPudar.Contains(g))g.SetActive(false);
            return;
        }
        disembunyikan.Add(g,g.activeSelf);
        if(!g.activeInHierarchy){g.SetActive(false);return;}
        foreach(var s in g.GetComponentsInChildren<SpriteRenderer>(true))if(!warnaNpc.ContainsKey(s))warnaNpc.Add(s,s.color);
        foreach(var b in g.GetComponents<Behaviour>())
            if(b is NpcYangBisaDiatur || b is AreaInteraksi || b is NPCBubbleTrigger)
            {if(!interaksiNpc.ContainsKey(b))interaksiNpc.Add(b,b.enabled);b.enabled=false;}
        sedangPudar.Add(g);
        StartCoroutine(PudarNpc(g));
    }
    IEnumerator PudarNpc(GameObject g)
    {
        int versi=versiPudar;
        var gambar=g.GetComponentsInChildren<SpriteRenderer>(true);
        float t=0;
        while(g!=null && versi==versiPudar && NpcTidur && t<durasiPudarNpc)
        {
            float alpha=1-Mathf.SmoothStep(0,1,t/durasiPudarNpc);
            foreach(var s in gambar)if(s!=null && warnaNpc.TryGetValue(s,out var c)){c.a*=alpha;s.color=c;}
            t+=Time.deltaTime;yield return null;
        }
        if(versi==versiPudar)
        {
            if(g!=null && NpcTidur)g.SetActive(false);
            sedangPudar.Remove(g);
        }
    }
    void PulihkanNpc()
    {
        versiPudar++;sedangPudar.Clear();
        foreach(var p in warnaNpc)if(p.Key!=null)p.Key.color=p.Value;
        foreach(var p in interaksiNpc)if(p.Key!=null)p.Key.enabled=p.Value;
        foreach(var p in disembunyikan)if(p.Key!=null)p.Key.SetActive(p.Value);
        warnaNpc.Clear();interaksiNpc.Clear();disembunyikan.Clear();
    }
    void Update()
    {
        var cerita=PerjalananChapelChapter1.Instance;
        if(cerita!=null&&cerita.isActiveAndEnabled&&cerita.MalamAktif){KamarDikunci=false;return;}
        bool malam=NpcTidur;
        if(malam && Time.unscaledTime>=cekBerikut)
        {
            cekBerikut=Time.unscaledTime+.5f;
            foreach(var n in FindObjectsByType<NpcKampusHidup>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Sembunyikan(n.gameObject);
            foreach(var n in FindObjectsByType<NpcYangBisaDiatur>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Sembunyikan(n.gameObject);
            foreach(var n in FindObjectsByType<PelariSekilasNpc>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Sembunyikan(n.gameObject);
            foreach(var n in FindObjectsByType<NPCBubbleTrigger>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            { var a=n.GetComponent<AreaInteraksi>(); if(a!=null && a.tipeInteraksi==TipeInteraksi.NPC_Karakter)Sembunyikan(n.gameObject); }
        }
        if(!malam && malamLama) { PulihkanNpc(); DetikMalam=0; }
        malamLama=malam;
        if(!PulangOtomatis || waktu==null || SedangPulang || SedangTidur)return;
        if(!JamIstirahat(waktu.Jam)){KamarDikunci=false;return;}
        var d=DialogManager.Instance; var k=KawasanKampus.Instance;
        KamarDikunci=true;
        bool sibuk=(d!=null&&(d.PercakapanAktif||d.minigameAktif))||k==null||!k.Siap||k.SedangMemuat||(pilihan!=null&&pilihan.efekTransisi!=null&&pilihan.efekTransisi.SedangBerjalan);
        if(sibuk)return;
        if(hp!=null && hp.AplikasiSedangBuka){hp.TutupAplikasi();return;}
        if(hp!=null && hp.hpSedangBuka){hp.TutupHP();return;}
        DetikMalam+=Time.deltaTime;
        if(tujuanKamar==null || arthur==null || pilihan==null || pilihan.efekTransisi==null)return;
        KamarDikunci=true;
        if(arthur.JalurAktif==null || arthur.JalurAktif.name!="GH_Kamar201")StartCoroutine(Pulang());
        else if(JamTidurPaksa(waktu.Jam))MintaTidur();
    }
    IEnumerator Pulang()
    {
        SedangPulang=true;
        var p=arthur.GetComponent<KarakterController>(); bool kunci=p.kunciPergerakan;
        var transisi=pilihan.efekTransisi; float tutup=transisi.durasiTutup;
        try{
            p.BerhentiBergerak();p.kunciPergerakan=true;transisi.durasiTutup=2f;
            transisi.MulaiTransisiTeleportRutin(()=>PindahKamar());
            while(transisi.SedangBerjalan)yield return null;
        }finally{transisi.durasiTutup=tutup;p.kunciPergerakan=kunci;SedangPulang=false;}
    }
    IEnumerator PindahKamar()
    {
        bool tiba=false;
        yield return KawasanKampus.Instance.Pindahkan(tujuanKamar,()=>{arthur.PindahKe(tujuanKamar);tiba=true;});
        if(!tiba)KamarDikunci=false;
    }
    public void MintaTidur()
    {
        if(!KamarDikunci || SedangPulang || SedangTidur || arthur==null || arthur.JalurAktif==null || arthur.JalurAktif.name!="GH_Kamar201")return;
        StartCoroutine(Tidur());
    }
    IEnumerator Tidur()
    {
        SedangTidur=true;
        var p=arthur.GetComponent<KarakterController>();var d=DialogManager.Instance;
        var transisi=pilihan.efekTransisi;
        try{
            if(d!=null && d.PercakapanAktif){pilihan.TutupPercakapanTerarah();while(d.PercakapanAktif)yield return null;}
            p.BerhentiBergerak();p.kunciPergerakan=true;
            transisi.MulaiTransisiTeleportRutin(()=>BangunPagi());
            while(transisi.SedangBerjalan)yield return null;
        }finally{p.kunciPergerakan=false;SedangTidur=false;}
    }
    IEnumerator BangunPagi()
    {
        yield return new WaitForSecondsRealtime(1);
        waktu.AturJam(SiklusWaktuKampus.JamMulaiPermainan);
        KamarDikunci=false;DetikMalam=0;PulihkanNpc();
    }
    void OnDestroy(){if(Instance==this)Instance=null;}
}
