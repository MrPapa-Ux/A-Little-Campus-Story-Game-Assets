using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Dipasang hanya pada CharacterLab dan Explorer, bukan scene Chapter 1.
[DefaultExecutionOrder(600)]
public sealed class MalamEksplorasiArthur : MonoBehaviour
{
    public static MalamEksplorasiArthur Instance { get; private set; }
    public static bool NpcTidur => Instance != null && Instance.waktu != null && Instance.waktu.waktu == WaktuKampus.Malam;
    public SiklusWaktuKampus waktu;
    public Transform tujuanKamar;
    public float batasMalam = 180f;
    // CharacterLab boleh menampilkan siklus suasana untuk penataan, tetapi tidak memulangkan pemain.
    public bool PulangOtomatis => gameObject.scene.name == "Explorer";
    [Min(.1f)] public float durasiPudarNpc = 1.5f;
    public float DetikMalam { get; private set; }
    public bool SedangPulang { get; private set; }
    readonly Dictionary<GameObject, bool> disembunyikan = new Dictionary<GameObject, bool>();
    readonly Dictionary<SpriteRenderer, Color> warnaNpc = new Dictionary<SpriteRenderer, Color>();
    readonly Dictionary<Behaviour, bool> interaksiNpc = new Dictionary<Behaviour, bool>();
    bool malamLama;
    float cekBerikut;
    SistemHP hp;
    SistemPilihan pilihan;
    ArthurJalurKampus arthur;
    void Awake() { Instance = this; }
    void Start() { hp=FindFirstObjectByType<SistemHP>(); pilihan=FindFirstObjectByType<SistemPilihan>(); arthur=FindFirstObjectByType<ArthurJalurKampus>(); }
    void Sembunyikan(GameObject g)
    {
        if(g==null || disembunyikan.ContainsKey(g))return;
        disembunyikan.Add(g,g.activeSelf);
        if(!g.activeInHierarchy){g.SetActive(false);return;}
        foreach(var s in g.GetComponentsInChildren<SpriteRenderer>(true))if(!warnaNpc.ContainsKey(s))warnaNpc.Add(s,s.color);
        foreach(var b in g.GetComponents<Behaviour>())
            if(b is NpcYangBisaDiatur || b is AreaInteraksi || b is NPCBubbleTrigger)
            {if(!interaksiNpc.ContainsKey(b))interaksiNpc.Add(b,b.enabled);b.enabled=false;}
        StartCoroutine(PudarNpc(g));
    }
    IEnumerator PudarNpc(GameObject g)
    {
        var gambar=g.GetComponentsInChildren<SpriteRenderer>(true);
        float t=0;
        while(g!=null && NpcTidur && t<durasiPudarNpc)
        {
            float alpha=1-Mathf.SmoothStep(0,1,t/durasiPudarNpc);
            foreach(var s in gambar)if(s!=null && warnaNpc.TryGetValue(s,out var c)){c.a*=alpha;s.color=c;}
            t+=Time.deltaTime;yield return null;
        }
        if(g!=null && NpcTidur)g.SetActive(false);
    }
    void PulihkanNpc()
    {
        foreach(var p in warnaNpc)if(p.Key!=null)p.Key.color=p.Value;
        foreach(var p in interaksiNpc)if(p.Key!=null)p.Key.enabled=p.Value;
        foreach(var p in disembunyikan)if(p.Key!=null)p.Key.SetActive(p.Value);
        warnaNpc.Clear();interaksiNpc.Clear();disembunyikan.Clear();
    }
    void Update()
    {
        bool malam=NpcTidur;
        if(malam && Time.unscaledTime>=cekBerikut)
        {
            cekBerikut=Time.unscaledTime+.5f;
            foreach(var n in FindObjectsByType<NpcKampusHidup>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Sembunyikan(n.gameObject);
            foreach(var n in FindObjectsByType<NpcYangBisaDiatur>(FindObjectsInactive.Include,FindObjectsSortMode.None)) Sembunyikan(n.gameObject);
            foreach(var n in FindObjectsByType<NPCBubbleTrigger>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            { var a=n.GetComponent<AreaInteraksi>(); if(a!=null && a.tipeInteraksi==TipeInteraksi.NPC_Karakter)Sembunyikan(n.gameObject); }
        }
        if(!malam && malamLama) { PulihkanNpc(); DetikMalam=0; }
        malamLama=malam;
        if(!malam || SedangPulang || !PulangOtomatis)return;
        var d=DialogManager.Instance; var k=KawasanKampus.Instance;
        bool sibuk=(d!=null&&(d.PercakapanAktif||d.minigameAktif))||(hp!=null&&(hp.hpSedangBuka||hp.AplikasiSedangBuka))||k==null||!k.Siap||k.SedangMemuat||(pilihan!=null&&pilihan.efekTransisi!=null&&pilihan.efekTransisi.SedangBerjalan);
        if(sibuk)return;
        DetikMalam+=Time.deltaTime;
        if(DetikMalam>=batasMalam && tujuanKamar!=null && pilihan!=null && pilihan.efekTransisi!=null)StartCoroutine(Pulang());
    }
    IEnumerator Pulang()
    {
        SedangPulang=true;
        var p=arthur.GetComponent<KarakterController>(); bool kunci=p.kunciPergerakan;
        p.BerhentiBergerak();p.kunciPergerakan=true;
        var transisi=pilihan.efekTransisi; float tutup=transisi.durasiTutup;
        transisi.durasiTutup=2f;
        transisi.MulaiTransisiTeleportRutin(()=>PindahPagi());
        while(transisi.SedangBerjalan)yield return null;
        transisi.durasiTutup=tutup;p.kunciPergerakan=kunci;SedangPulang=false;
        if(waktu.waktu==WaktuKampus.Malam)DetikMalam=batasMalam-10; // gagal muat: coba lagi tanpa mengunci pemain.
    }
    IEnumerator PindahPagi()
    {
        bool tiba=false;
        yield return KawasanKampus.Instance.Pindahkan(tujuanKamar,()=>{arthur.PindahKe(tujuanKamar);tiba=true;});
        if(tiba){waktu.AturWaktu(WaktuKampus.Pagi);DetikMalam=0;}
    }
    void OnDestroy(){if(Instance==this)Instance=null;}
}
