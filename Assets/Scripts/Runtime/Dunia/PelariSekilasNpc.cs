using UnityEngine;

public enum GayaPelintasNpc { JalanArthur, LariArthur, JalanUnik }

// Jalur rumput pendek dan tetap; tidak memakai collider, interaksi, atau pencarian jalan.
public sealed class PelariSekilasNpc : MonoBehaviour
{
    public Transform pelari;
    [Tooltip("Opsional: tampil hanya di jalur ini. Kantin, kamar, dan Chapel tidak memakai pelintas.")]
    public JalurKampus jalurKhusus;
    [Tooltip("Geser dua penanda ini di Scene untuk mengatur lintasan pelari.")]
    public Transform titikAwal,titikAkhir;
    public Vector3 awal,akhir;
    public Animator animatorArthur;
    public bool samakanKecepatanArthur=true;
    [Range(0,1)] public float peluangLari=.2f;
    [Tooltip("Bagian dari seluruh kemunculan. Termasuk kelompok berjalan.")]
    [Range(0,1)] public float peluangJalanUnik=.05f;
    public float durasiJalan=20f,durasiLari=15f,jedaAwal=3.5f;
    [Range(.2f,2)] public float tempoAnimasiArthur=.85f;
    public GayaPelintasNpc GayaAktif {get;private set;}
    public bool SedangBerlari {get;private set;}
    public int JumlahLintasan {get;private set;}
    SpriteRenderer[] gambar;Color[] warna;Renderer[] lainnya;
    float mulai,berikut,durasiAktif;ArthurJalurKampus arthur;
    NpcKampusHidup gerakUnik;GayaPelintasNpc? gayaPratinjau;
    Vector3 Awal=>titikAwal!=null?titikAwal.position:awal;
    Vector3 Akhir=>titikAkhir!=null?titikAkhir.position:akhir;
    Vector3 awalLintasan,akhirLintasan;
    [ContextMenu("Pratinjau pelari saat Play")]
    public void Pratinjau(){if(Application.isPlaying&&!SedangBerlari)berikut=Time.time;}
    public void PratinjauGaya(GayaPelintasNpc gaya){if(Application.isPlaying&&!SedangBerlari){gayaPratinjau=gaya;berikut=Time.time;}}
    void Awake(){if(pelari==null){enabled=false;return;}gerakUnik=pelari.GetComponent<NpcKampusHidup>();gambar=pelari.GetComponentsInChildren<SpriteRenderer>(true);warna=new Color[gambar.Length];for(int i=0;i<gambar.Length;i++)warna[i]=gambar[i].color;lainnya=pelari.GetComponentsInChildren<MeshRenderer>(true);}
    void OnEnable(){berikut=Time.time+jedaAwal;SedangBerlari=false;if(pelari!=null)pelari.gameObject.SetActive(false);}
    void OnDisable(){SedangBerlari=false;if(pelari!=null)pelari.gameObject.SetActive(false);}
    void Update()
    {
        if(pelari==null)return;
        if(arthur==null)arthur=FindFirstObjectByType<ArthurJalurKampus>();
        // Pengelola malam memudarkan sprite yang masih terlihat; jangan mematikannya mendadak.
        if(MalamEksplorasiArthur.NpcTidur)return;
        if(jalurKhusus!=null && (arthur==null||arthur.JalurAktif!=jalurKhusus))
        { if(SedangBerlari){SedangBerlari=false;pelari.gameObject.SetActive(false);}return; }
        bool jeda=DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif);
        if(SedangBerlari)
        {
            if(jeda){mulai+=Time.deltaTime;if(animatorArthur!=null)animatorArthur.speed=0;return;}
            if(animatorArthur!=null)animatorArthur.speed=tempoAnimasiArthur;
            float durasi=durasiAktif,waktu=Time.time-mulai;
            float t=waktu/durasi;pelari.position=Vector3.Lerp(awalLintasan,akhirLintasan,Mathf.Clamp01(t));
            // SmoothStep's first two arguments are output values, not time thresholds.
            // Keep full opacity until the final 1.25 seconds of the pass.
            float alpha=Mathf.Min(Mathf.Clamp01(waktu/.25f),1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(durasi-1.25f,durasi,waktu)));
            for(int i=0;i<gambar.Length;i++){var c=warna[i];c.a*=alpha;gambar[i].color=c;}
            foreach(var r in lainnya)r.enabled=alpha>.3f;
            if(t>=1){SedangBerlari=false;pelari.gameObject.SetActive(false);berikut=Time.time+Random.Range(24,38);}
            return;
        }
        if(Time.time<berikut||jeda)return;berikut=Time.time+2;
        if(arthur==null)arthur=FindFirstObjectByType<ArthurJalurKampus>();
        if(arthur==null||(arthur.transform.position-(Awal+Akhir)*.5f).sqrMagnitude>28*28)return;
        // Jangan memunculkan figuran tepat di tubuh Arthur.
        if((arthur.transform.position-Awal).sqrMagnitude<3*3)return;
        // Kamera dekat hanya melihat sebagian lintasan; cukup satu bagian tengah terlihat.
        // Tetap tolak lintasan yang seluruhnya di luar layar atau di belakang kamera.
        if(arthur.kamera==null)return;
        bool lintasanTerlihat=false;
        foreach(float t in new[]{.25f,.5f,.65f})
        {
            Vector3 p=arthur.kamera.WorldToViewportPoint(Vector3.Lerp(Awal,Akhir,t)+Vector3.up*.9f);
            if(p.z>=2&&p.x>=.06f&&p.x<=.94f&&p.y>=.06f&&p.y<=.9f){lintasanTerlihat=true;break;}
        }
        if(!lintasanTerlihat)return;
        // One roll: 20% run, 75% Arthur walk, 5% unique walk = 80% walking.
        float roll=Random.value;
        GayaAktif=gayaPratinjau??(roll<peluangLari?GayaPelintasNpc.LariArthur:roll<Mathf.Min(1,peluangLari+peluangJalanUnik)?GayaPelintasNpc.JalanUnik:GayaPelintasNpc.JalanArthur);
        gayaPratinjau=null;bool unik=GayaAktif==GayaPelintasNpc.JalanUnik;
        if(!unik&&(animatorArthur==null||animatorArthur.runtimeAnimatorController==null))return;
        if(animatorArthur!=null)animatorArthur.enabled=false;
        foreach(var s in gambar)s.enabled=true;
        if(gerakUnik!=null){gerakUnik.enabled=unik;if(gerakUnik.visual!=null){var scale=gerakUnik.visual.localScale;scale.x=Mathf.Abs(scale.x)*(Vector3.Dot(Akhir-Awal,arthur.kamera.transform.right)>=0?1:-1);gerakUnik.visual.localScale=scale;}}
        awalLintasan=Awal;akhirLintasan=Akhir;durasiAktif=Mathf.Max(2,GayaAktif==GayaPelintasNpc.LariArthur?durasiLari:durasiJalan);
        if(samakanKecepatanArthur && arthur!=null)
        {
            var kontrol=arthur.GetComponent<KarakterController>();
            if(kontrol!=null)
            {
                float kecepatan=kontrol.kecepatanJalan*(GayaAktif==GayaPelintasNpc.LariArthur?1f:kontrol.pengaliKecepatanJalan);
                durasiAktif=Vector3.Distance(awalLintasan,akhirLintasan)/Mathf.Max(.1f,kecepatan);
                tempoAnimasiArthur=kontrol.animator!=null?kontrol.animator.speed:1f;
            }
        }
        pelari.position=awalLintasan;pelari.gameObject.SetActive(true);
        if(animatorArthur!=null&&!unik){animatorArthur.enabled=true;animatorArthur.Rebind();animatorArthur.SetBool("isJalan",true);animatorArthur.SetBool("isLari",GayaAktif==GayaPelintasNpc.LariArthur);animatorArthur.speed=tempoAnimasiArthur;animatorArthur.Update(0);}
        mulai=Time.time;SedangBerlari=true;JumlahLintasan++;
    }
    void LateUpdate(){if(SedangBerlari&&gerakUnik!=null&&gerakUnik.visual!=null&&arthur!=null&&arthur.kamera!=null)gerakUnik.visual.rotation=Quaternion.Euler(0,arthur.kamera.transform.eulerAngles.y,0);}
    void OnDrawGizmosSelected(){Gizmos.color=Color.green;Gizmos.DrawLine(Awal,Akhir);Gizmos.DrawWireSphere(Awal,.25f);Gizmos.DrawWireSphere(Akhir,.25f);}
}
