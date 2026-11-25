using UnityEngine;

// Dosen menyusuri jalur kampus yang sama dengan Arthur, tanpa NavMesh/collider penghalang.
[RequireComponent(typeof(NpcYangBisaDiatur), typeof(NpcKampusHidup))]
public sealed class DosenBerjalanKampus : MonoBehaviour
{
    public JalurKampus jalur;
    [Min(0)] public float jarakAwal, jarakAkhir=10;
    [Min(.1f)] public float kecepatan=.85f;
    [Min(1)] public float jarakBerhenti=3.2f;
    public Vector3 geserDariJalur;
    public bool mulaiDariAkhir;
    public bool berlari;
    [Tooltip("Pengaturan lama. Pejalan sekarang selalu memakai jalan Arthur saat bergerak dan idle saat berhenti.")]
    public bool pakaiIdleArthur;
    public bool SedangBerjalan {get;private set;}
    public float PosisiJalur {get;private set;}
    NpcKampusHidup gerak;
    NpcYangBisaDiatur interaksi;
    ArthurJalurKampus arthur;
    int arah=1;
    float istirahat;

    void Start()
    {
        gerak=GetComponent<NpcKampusHidup>();interaksi=GetComponent<NpcYangBisaDiatur>();
        pakaiIdleArthur=false;berlari=false;
        if(gerak.idleArthur==null)gerak.idleArthur=Resources.Load<PolaIdleArthurNpc>("PolaIdlePejalanArthur");
        if(gerak.jalanArthur==null)gerak.jalanArthur=Resources.Load<PolaIdleArthurNpc>("PolaJalanPejalanArthur");
        gerak.langkahMengikutiGerak=true;gerak.waktuJalan=0;
        arthur=FindFirstObjectByType<ArthurJalurKampus>();
        if(jalur==null){enabled=false;return;}
        jarakAwal=Mathf.Clamp(jarakAwal,0,jalur.Panjang);
        jarakAkhir=Mathf.Clamp(jarakAkhir,jarakAwal,jalur.Panjang);
        PosisiJalur=mulaiDariAkhir?jarakAkhir:jarakAwal;arah=mulaiDariAkhir?-1:1;
        Tempatkan();
    }
    void Update()
    {
        if(jalur==null||gerak==null||arthur==null)return;
        var dialog=DialogManager.Instance;
        bool dekat=arthur.JalurAktif==jalur&&
            (arthur.transform.position-Vector3.up*ArthurJalurKampus.JarakKaki-transform.position-interaksi.geserTitikInteraksi).sqrMagnitude<jarakBerhenti*jarakBerhenti;
        bool jeda=JedaSaatTidakAktif.Dijeda||MalamEksplorasiArthur.NpcTidur||
            (KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat)||
            (dialog!=null&&(dialog.PercakapanAktif||dialog.minigameAktif));
        SedangBerjalan=!jeda&&!dekat&&Time.time>=istirahat;
        gerak.kegiatan=SedangBerjalan?KegiatanNpcKampus.Berjalan:KegiatanNpcKampus.Berdiri;
        if(!SedangBerjalan)return;
        float sebelum=PosisiJalur;
        PosisiJalur=Mathf.MoveTowards(PosisiJalur,arah>0?jarakAkhir:jarakAwal,kecepatan*Time.deltaTime);
        gerak.waktuJalan+=Mathf.Abs(PosisiJalur-sebelum)/2.5f;
        Tempatkan();
        if(Mathf.Abs(PosisiJalur-(arah>0?jarakAkhir:jarakAwal))<.001f){arah=-arah;istirahat=Time.time+1.2f;}
    }
    void Tempatkan()
    {
        jalur.Sampel(PosisiJalur,out var p,out var maju,out _);
        transform.position=p-Vector3.up*ArthurJalurKampus.JarakKaki+geserDariJalur;
        if(gerak.visual!=null&&arthur!=null&&arthur.kamera!=null){
            var skala=gerak.visual.localScale;
            float sisi=Vector3.Dot(maju*arah,arthur.kamera.transform.right);
            if(Mathf.Abs(sisi)>.05f)skala.x=Mathf.Abs(skala.x)*Mathf.Sign(sisi);
            gerak.visual.localScale=skala;
        }
    }
}
