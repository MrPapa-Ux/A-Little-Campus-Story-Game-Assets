using UnityEngine;
using UnityEngine.Events;

// Khusus adegan cerita Dining. Tidak dipasang pada CharacterLab atau NPC eksplorasi.
public sealed class AdeganMakanDiningArthur : MonoBehaviour
{
    [Header("Pilih sudut kamera lalu Play")]
    [Range(1,3)] public int sudutKamera=1;
    public Camera kamera;
    public float fovKamera3=48;
    public Transform[] titikKamera;
    public Transform sasaranKamera,visualArthur,kepala,badan,tanganMakan,tanganDiam,sendok;
    [Min(5)] public float durasiAdegan=12;
    public bool ulangiUntukPratinjau=true;
    public UnityEvent saatMakanSelesai=new UnityEvent();
    [Header("Punchline sesudah Tap-Tap Mekar")]
    public EkspresiWajah2D wajahArthur;
    [Range(0,1)] public float sedihRingan=.28f;
    public CanvasGroup penutupFade;
    public float tahanPunchline=2f,durasiFade=.65f;
    public UnityEvent saatPunchlineSelesai=new UnityEvent();
    public bool PunchlineAktif {get;private set;}
    public float WaktuMulaiFade {get;private set;}=-1;
    Coroutine urutanPunchline;

    [Header("Malam hanya untuk satu adegan cerita")]
    public bool pratinjauMalam;
    public Light lampuUtama;
    public Light[] lampuDining;
    public Transform[] figuran=System.Array.Empty<Transform>();
    public bool MalamSudahDipakai {get;private set;}
    public float WaktuAdegan {get;private set;}
    public bool Selesai {get;private set;}
    Vector3 awalKepala,awalBadan,awalMakan,awalDiam;
    Quaternion rotasiMakan;
    Color ambientAwal,warnaUtama;float intensitasAwal;
    Material skyboxAwal;Color latarAwal;CameraClearFlags clearAwal;Vector3[] posisiFiguran;
    UnityEngine.Rendering.AmbientMode modeAwal;
    bool siap,malamAktif;int kameraTerakhir;
    void Awake(){Siapkan();PilihKamera(sudutKamera);if(wajahArthur!=null)wajahArthur.AturSedihCerita(true,sedihRingan);if(penutupFade!=null){penutupFade.alpha=0;penutupFade.blocksRaycasts=false;}if(pratinjauMalam)TampilkanMalamSekali();}
    void Siapkan(){if(siap)return;siap=true;awalKepala=kepala.localPosition;awalBadan=badan.localPosition;awalMakan=tanganMakan.localPosition;awalDiam=tanganDiam.localPosition;rotasiMakan=tanganMakan.localRotation;ambientAwal=RenderSettings.ambientLight;modeAwal=RenderSettings.ambientMode;skyboxAwal=RenderSettings.skybox;if(kamera!=null){latarAwal=kamera.backgroundColor;clearAwal=kamera.clearFlags;}posisiFiguran=new Vector3[figuran.Length];for(int i=0;i<figuran.Length;i++)if(figuran[i]!=null)posisiFiguran[i]=figuran[i].position;if(lampuUtama!=null){warnaUtama=lampuUtama.color;intensitasAwal=lampuUtama.intensity;}}
    void Update(){
        if(kameraTerakhir!=sudutKamera)PilihKamera(sudutKamera);
        if(Selesai)return;
        WaktuAdegan+=Mathf.Min(Time.deltaTime,.05f);AturFrame(WaktuAdegan);
        if(WaktuAdegan>=durasiAdegan&&!ulangiUntukPratinjau){Selesai=true;saatMakanSelesai.Invoke();}
    }
    public void AturFrame(float waktu){
        Siapkan();
        for(int i=0;i<figuran.Length;i++)if(figuran[i]!=null)figuran[i].position=posisiFiguran[i]+Vector3.up*(Mathf.Sin(waktu*1.4f+i)*.008f);
        // Arthur duduk santai dan termenung; makanan tetap di meja.
        // Tidak ada gerakan menyendok atau mengunyah.
        float napas=Mathf.Sin(waktu*1.25f);
        kepala.localPosition=awalKepala+Vector3.up*napas*.008f;
        badan.localPosition=awalBadan+Vector3.up*napas*.005f;
        tanganMakan.localPosition=awalMakan+Vector3.up*napas*.004f;
        tanganDiam.localPosition=awalDiam+Vector3.up*napas*.004f;
    }

    public void PilihKamera(int nomor){
        sudutKamera=Mathf.Clamp(nomor,1,3);kameraTerakhir=sudutKamera;
        if(kamera==null||titikKamera==null||titikKamera.Length<sudutKamera)return;
        var t=titikKamera[sudutKamera-1];kamera.transform.SetPositionAndRotation(t.position,t.rotation);
        // Kamera statis selama adegan; tidak mengitari atau menembus meja/dinding.
        kamera.fieldOfView=sudutKamera==3?fovKamera3:43;
        if(visualArthur!=null)visualArthur.rotation=Quaternion.Euler(0,kamera.transform.eulerAngles.y,0);
        foreach(var npc in figuran)if(npc!=null)npc.rotation=Quaternion.Euler(0,kamera.transform.eulerAngles.y,0);
    }
    [ContextMenu("Kamera 1 - Depan dekat")] void Kamera1()=>PilihKamera(1);
    [ContextMenu("Kamera 2 - Serong meja")] void Kamera2()=>PilihKamera(2);
    [ContextMenu("Kamera 3 - Lebih luas")] void Kamera3()=>PilihKamera(3);
    // Panggil dari event kemenangan Tap-Tap Mekar, bukan dari timer adegan duduk.
    [ContextMenu("Pratinjau punchline 2 detik (saat Play)")]
    public void TampilkanPunchlineSetelahMekar()
    {
        if(!Application.isPlaying||PunchlineAktif)return;
        urutanPunchline=StartCoroutine(Punchline());
    }
    System.Collections.IEnumerator Punchline()
    {
        PunchlineAktif=true;WaktuMulaiFade=-1;
        if(wajahArthur!=null)wajahArthur.AturSedihCerita(true,1);
        if(penutupFade!=null){penutupFade.alpha=0;penutupFade.blocksRaycasts=true;}
        yield return new WaitForSecondsRealtime(tahanPunchline);
        WaktuMulaiFade=Time.unscaledTime;
        if(penutupFade!=null){float t=0;while(t<durasiFade){t+=Time.unscaledDeltaTime;penutupFade.alpha=Mathf.SmoothStep(0,1,t/Mathf.Max(.01f,durasiFade));yield return null;}penutupFade.alpha=1;}
        urutanPunchline=null;saatPunchlineSelesai.Invoke();
    }
    [ContextMenu("Kembali ke pose sebelum Mekar (saat Play)")]
    public void KembaliKePoseTenang()
    {
        if(urutanPunchline!=null)StopCoroutine(urutanPunchline);urutanPunchline=null;PunchlineAktif=false;WaktuMulaiFade=-1;
        if(wajahArthur!=null)wajahArthur.AturSedihCerita(true,sedihRingan);
        if(penutupFade!=null){penutupFade.alpha=0;penutupFade.blocksRaycasts=false;}
    }
    public void TampilkanMalamSekali(){
        if(MalamSudahDipakai)return;Siapkan();MalamSudahDipakai=true;malamAktif=true;
        RenderSettings.skybox=null;if(kamera!=null){kamera.clearFlags=CameraClearFlags.SolidColor;kamera.backgroundColor=new Color(.025f,.04f,.08f);}
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.12f,.16f,.25f);
        if(lampuUtama!=null){lampuUtama.intensity=.12f;lampuUtama.color=new Color(.38f,.5f,.8f);}
        foreach(var l in lampuDining)if(l!=null)l.enabled=true;
    }
    void OnDisable(){if(!malamAktif)return;RenderSettings.skybox=skyboxAwal;if(kamera!=null){kamera.backgroundColor=latarAwal;kamera.clearFlags=clearAwal;}RenderSettings.ambientMode=modeAwal;RenderSettings.ambientLight=ambientAwal;if(lampuUtama!=null){lampuUtama.color=warnaUtama;lampuUtama.intensity=intensitasAwal;}foreach(var l in lampuDining)if(l!=null)l.enabled=false;malamAktif=false;}
}
