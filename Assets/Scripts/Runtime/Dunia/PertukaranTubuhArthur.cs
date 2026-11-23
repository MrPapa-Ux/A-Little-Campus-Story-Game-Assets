using System.Collections;
using UnityEngine;

// Keep the existing player, camera, input and Animator. Only its appearance is borrowed.
[DefaultExecutionOrder(550)]
public sealed class PertukaranTubuhArthur : MonoBehaviour
{
    [System.Serializable] public sealed class Bagian
    {
        public SpriteRenderer pemain;
        public Sprite perempuan;
        public Color warnaPerempuan = Color.white;
        public bool sembunyikan;
        [HideInInspector] public Sprite spriteArthur;
        [System.NonSerialized] public Sprite awal;
        [System.NonSerialized] public Color warnaAwal;
        [System.NonSerialized] public bool aktifAwal;
        [System.NonSerialized] public Vector3 posisiAwal, skalaAwal;
        [System.NonSerialized] public Quaternion rotasiAwal;
        [System.NonSerialized] public int urutanAwal;
        [System.NonSerialized] public bool balikXAwal, balikYAwal;
        [System.NonSerialized] public SpriteRenderer sandaran;
        [System.NonSerialized] public AksesoriDadaSejajar aksesori;
        [System.NonSerialized] public float ukuranAksesoriAwal;
        [System.NonSerialized] public Vector2 leherAwal;
    }
    public string namaPerempuan = "Naya";
    public Sprite potretPerempuan;
    public Bagian[] bagian;
    public GameObject npcPerempuan, tubuhArthur;
    public JalurKampus jalurKelas;
    public EkspresiWajah2D[] wajahArthur;
    public Vector3 posisiNaya=new Vector3(42.2f,.7f,-53.8f);
    public Vector3 posisiTubuhArthur=new Vector3(39.2f,.7f,-53.8f);
    public bool DiTubuhPerempuan { get; private set; }
    public bool SedangBertukar { get; private set; }
    ArthurJalurKampus arthur;
    KarakterController pemain;
    bool siap;
    bool[] wajahAktif;
    GameObject[] lapisanWajah;
    bool[] lapisanAktif;
    Transform kepalaNaya;SpriteRenderer rambutNaya;
    // RectTransform roots cloned from the NPC template retain anchored X/Y when
    // their ordinary Transform parent is deserialized differently on Android.
    void PosisikanTubuh()
    {
        if(npcPerempuan!=null)npcPerempuan.transform.position=posisiNaya;
        if(tubuhArthur!=null)tubuhArthur.transform.position=posisiTubuhArthur;
    }
    void LateUpdate()
    {
        PosisikanTubuh();
        if(!DiTubuhPerempuan)return;
        foreach(var b in bagian??System.Array.Empty<Bagian>())if(b.pemain!=null&&b.sandaran!=null)
        {
            if(b.aksesori!=null&&b.aksesori.enabled)continue;
            var t=b.pemain.transform;var acuan=b.sandaran.transform;
            bool rambut=b.pemain.name.Contains("Rambut");
            var offset=rambut&&kepalaNaya!=null&&rambutNaya!=null?kepalaNaya.InverseTransformPoint(rambutNaya.transform.position):rambut?new Vector3(-.02f,.01f,0):new Vector3(0,.10f,-.002f);
            t.position=acuan.TransformPoint(offset);
            t.rotation=acuan.rotation*(rambut&&kepalaNaya!=null&&rambutNaya!=null?Quaternion.Inverse(kepalaNaya.rotation)*rambutNaya.transform.rotation:Quaternion.Euler(0,0,rambut?2f:0f));
            var s=acuan.lossyScale;var induk=t.parent!=null?t.parent.lossyScale:Vector3.one;
            float ukuran=rambut?1f:.045f;
            t.localScale=new Vector3(s.x/induk.x*ukuran,s.y/induk.y*ukuran,s.z/induk.z*ukuran);
            if(rambut){b.pemain.flipX=b.sandaran.flipX;b.pemain.flipY=b.sandaran.flipY;b.pemain.enabled=true;b.pemain.gameObject.SetActive(true);}
            b.pemain.sortingOrder=b.sandaran.sortingOrder+(rambut?2:1);
        }
        foreach(var b in bagian??System.Array.Empty<Bagian>())
            if(b.pemain!=null&&b.pemain.transform.IsChildOf(transform)&&b.perempuan!=null&&!b.sembunyikan)
                b.pemain.sprite=b.perempuan;
    }

    void Awake()
    {
        arthur=GetComponent<ArthurJalurKampus>();pemain=GetComponent<KarakterController>();
        if(npcPerempuan!=null){kepalaNaya=npcPerempuan.GetComponent<NpcKampusHidup>()?.kepala;foreach(var r in npcPerempuan.GetComponentsInChildren<SpriteRenderer>(true))if(r.name.Contains("Rambut")){rambutNaya=r;break;}}
        PosisikanTubuh();
        if(npcPerempuan!=null)
            foreach(var gambar in npcPerempuan.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if(gambar.name.Contains("Lanyard"))gambar.transform.localScale*=.64f;
            }
        foreach(var tubuh in new[]{npcPerempuan,tubuhArthur})if(tubuh!=null){var npc=tubuh.GetComponent<NpcYangBisaDiatur>();if(npc!=null)npc.geserTitikInteraksi=Vector3.zero;}
        foreach(var b in bagian ?? System.Array.Empty<Bagian>()) if(b.pemain!=null)
        {
            b.awal=b.spriteArthur!=null?b.spriteArthur:b.pemain.sprite;b.warnaAwal=b.pemain.color;b.aktifAwal=b.pemain.enabled;
            b.pemain.sprite=b.awal;
            b.posisiAwal=b.pemain.transform.localPosition;b.skalaAwal=b.pemain.transform.localScale;
            b.rotasiAwal=b.pemain.transform.localRotation;b.urutanAwal=b.pemain.sortingOrder;
            b.balikXAwal=b.pemain.flipX;b.balikYAwal=b.pemain.flipY;
            b.aksesori=b.pemain.GetComponent<AksesoriDadaSejajar>();
            if(b.aksesori!=null){b.ukuranAksesoriAwal=b.aksesori.ukuranTerhadapBadan;b.leherAwal=b.aksesori.titikLeherSprite;}
            if(b.pemain.name.Contains("Rambut")||b.pemain.name.Contains("Lanyard"))
            {
                bool rambut=b.pemain.name.Contains("Rambut");
                foreach(var calon in bagian)if(calon.pemain!=null&&calon.pemain.transform.parent==b.pemain.transform.parent&&
                    (rambut?calon.pemain.name.StartsWith("Kepala"):calon.pemain.name=="Badan")){b.sandaran=calon.pemain;break;}
            }
        }
        wajahAktif=new bool[wajahArthur==null?0:wajahArthur.Length];
        for(int i=0;i<wajahAktif.Length;i++)wajahAktif[i]=wajahArthur[i]!=null&&wajahArthur[i].enabled;
        if(tubuhArthur!=null)tubuhArthur.SetActive(false);
        siap=true;
    }
    public bool BolehBertukar(bool kembali)
    {
        if(!siap||SedangBertukar||arthur==null||arthur.JalurAktif!=jalurKelas||kembali!=DiTubuhPerempuan)return false;
        var target=kembali?tubuhArthur:npcPerempuan;
        if(target==null||!target.activeInHierarchy)return false;
        var kaki=arthur.transform.position-Vector3.up*ArthurJalurKampus.JarakKaki;
        return (kaki-target.transform.position).sqrMagnitude<3.5f*3.5f;
    }
    public void MintaBertukar(bool kembali)
    {
        if(BolehBertukar(kembali))StartCoroutine(Bertukar(kembali));
    }
    IEnumerator Bertukar(bool kembali)
    {
        SedangBertukar=true;
        // Closing the dialog owns the input lock. Do not exchange while its portrait is animating.
        var pilihan=FindFirstObjectByType<SistemPilihan>();
        if(pilihan!=null)pilihan.TutupPercakapanTerarah();
        float batas=Time.unscaledTime+3;
        while(DialogManager.Instance!=null&&DialogManager.Instance.PercakapanAktif&&Time.unscaledTime<batas)yield return null;
        if(DialogManager.Instance!=null&&DialogManager.Instance.PercakapanAktif){SedangBertukar=false;yield break;}
        pemain.BerhentiBergerak();
        var transisi=pilihan!=null?pilihan.efekTransisi:null;
        System.Action ganti=()=>Terapkan(!kembali);
        if(transisi!=null)
        {
            transisi.MulaiTransisiTeleport(ganti);
            yield return null;
            while(transisi.SedangBerjalan)yield return null;
        }
        else ganti();
        pemain.BerhentiBergerak();SedangBertukar=false;
    }
    void SimpanLapisanWajah()
    {
        var daftar=new System.Collections.Generic.List<GameObject>();
        foreach(var w in wajahArthur ?? System.Array.Empty<EkspresiWajah2D>())if(w!=null)
            foreach(Transform t in w.GetComponentsInChildren<Transform>(true))
                if(t!=w.transform&&(t.name.Contains("Runtime")||t.name.StartsWith("Ekspresi")||t.name.StartsWith("Emosi")))
                { if(!daftar.Contains(t.gameObject))daftar.Add(t.gameObject); }
        lapisanWajah=daftar.ToArray();lapisanAktif=new bool[lapisanWajah.Length];
        for(int i=0;i<lapisanWajah.Length;i++){lapisanAktif[i]=lapisanWajah[i].activeSelf;lapisanWajah[i].SetActive(false);}
    }
    public void Terapkan(bool perempuan)
    {
        if(!siap||perempuan==DiTubuhPerempuan)return;
        if(perempuan)SimpanLapisanWajah();
        DiTubuhPerempuan=perempuan;
        foreach(var b in bagian ?? System.Array.Empty<Bagian>())if(b.pemain!=null)
        {
            b.pemain.sprite=perempuan&&b.perempuan!=null?b.perempuan:b.awal;
            b.pemain.color=perempuan?b.warnaPerempuan:b.warnaAwal;
            b.pemain.enabled=perempuan?!b.sembunyikan&&b.aktifAwal:b.aktifAwal;
            if(b.aksesori!=null){b.aksesori.ukuranTerhadapBadan=perempuan?.44f:b.ukuranAksesoriAwal;b.aksesori.titikLeherSprite=perempuan?new Vector2(.025f,.48f):b.leherAwal;}
            if(!perempuan&&b.sandaran!=null){b.pemain.transform.localPosition=b.posisiAwal;b.pemain.transform.localScale=b.skalaAwal;b.pemain.transform.localRotation=b.rotasiAwal;b.pemain.sortingOrder=b.urutanAwal;b.pemain.flipX=b.balikXAwal;b.pemain.flipY=b.balikYAwal;}
        }
        for(int i=0;i<wajahAktif.Length;i++)if(wajahArthur[i]!=null)wajahArthur[i].enabled=!perempuan&&wajahAktif[i];
        if(!perempuan&&lapisanWajah!=null)
            for(int i=0;i<lapisanWajah.Length;i++)if(lapisanWajah[i]!=null)lapisanWajah[i].SetActive(lapisanAktif[i]);
        if(npcPerempuan!=null)npcPerempuan.SetActive(!perempuan);
        if(tubuhArthur!=null)tubuhArthur.SetActive(perempuan);
        var buku=GetComponent<BukuArthur>();
        if(buku==null)buku=FindFirstObjectByType<BukuArthur>();
        if(buku!=null)buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{
            id="jasmine_pinjam_tubuh_naya",judul="Kembali ke tubuh Arthur",dikendalikanQuest=true,
            selesai=!perempuan,isi=perempuan?"Tubuh Arthur menunggu di kelas GK2. Kembali ke kelas itu dan ajak bicara Arthur untuk bertukar kembali.":"Arthur sudah kembali ke tubuhnya."});
    }
}
