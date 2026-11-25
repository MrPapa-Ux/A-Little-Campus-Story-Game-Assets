using UnityEngine;

// One small flock is reused during a successful visit; no birds are spawned per frame.
public sealed class KawananBurungKampus : MonoBehaviour
{
    public GameObject prefabBurung;
    [Range(0,1)] public float peluang=.2f;
    public float jedaTerbang=45f;
    public string Zona { get; private set; }="";
    public bool KunjunganBerhasil { get; private set; }
    public int JumlahUndian { get; private set; }
    public GameObject Kawanan => kawanan;
    readonly System.Random random=new System.Random();
    ArthurJalurKampus arthur;GameObject kawanan;Vector3 awal,arah;
    float mulai,berikutnya;bool terbang;
    public static string ZonaUntuk(JalurKampus jalur)
    {
        if(jalur==null||jalur.diDalamRuangan)return "";
        string n=jalur.name;
        if(n=="GA_Taman_MenujuPioneer")return "PioneerTengah";
        if(n.StartsWith("UNK_")&&!n.Contains("Interior")&&!n.EndsWith("_Dalam"))return "TamanUNKLAB";
        return "";
    }
    void Update()
    {
        if(arthur==null)arthur=FindFirstObjectByType<ArthurJalurKampus>();
        if(arthur==null||KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat)return;
        string zona=ZonaUntuk(arthur.JalurAktif);
        if(zona!=Zona){Bersihkan();Zona=zona;KunjunganBerhasil=false;
            if(zona!=""){JumlahUndian++;KunjunganBerhasil=PertemuanHewanKampus.Pilih((float)random.NextDouble(),1,peluang)!=PertemuanHewanKampus.Hasil.TidakAda;berikutnya=Time.time+1;}}
        if(!KunjunganBerhasil||prefabBurung==null||arthur.kamera==null)return;
        if(DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif)){
            if(kawanan!=null)kawanan.SetActive(false);terbang=false;berikutnya=Time.time+2;return;}
        if(!terbang&&Time.time>=berikutnya)MulaiTerbang();
        if(terbang){float t=Time.time-mulai;kawanan.transform.position=awal+arah*(t*5f);
            if(t>=12){kawanan.SetActive(false);terbang=false;berikutnya=Time.time+Mathf.Max(10,jedaTerbang);}}
    }
    void MulaiTerbang()
    {
        if(kawanan==null){kawanan=Instantiate(prefabBurung,transform);kawanan.name="Burung_Kunjungan_"+Zona;
            foreach(var r in kawanan.GetComponentsInChildren<Renderer>(true)){r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;}
            foreach(var a in kawanan.GetComponentsInChildren<Animator>(true))a.cullingMode=AnimatorCullingMode.CullUpdateTransforms;
        }
        var kamera=arthur.kamera.transform;arah=Vector3.ProjectOnPlane(kamera.right,Vector3.up).normalized;
        var depan=Vector3.ProjectOnPlane(kamera.forward,Vector3.up).normalized;
        awal=arthur.transform.position+depan*18+Vector3.up*7-arah*30;
        kawanan.transform.SetPositionAndRotation(awal,Quaternion.LookRotation(arah));
        kawanan.transform.localScale=Vector3.one*5;kawanan.SetActive(true);mulai=Time.time;terbang=true;
    }
    void Bersihkan(){if(kawanan!=null)Destroy(kawanan);kawanan=null;terbang=false;}
    void OnDisable(){Bersihkan();Zona="";}
#if UNITY_EDITOR
    public void Pratinjau(){if(arthur==null)arthur=FindFirstObjectByType<ArthurJalurKampus>();Zona=ZonaUntuk(arthur.JalurAktif);if(Zona=="")return;KunjunganBerhasil=true;MulaiTerbang();}
#endif
}
