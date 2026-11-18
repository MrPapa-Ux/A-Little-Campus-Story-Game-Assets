using UnityEngine;

// Optional per-route override. Existing camera choreography remains intact when disabled.
public sealed class KameraPilihanPemilik : MonoBehaviour
{
    public bool gunakan;
    public bool ikutiArthur;
    [Tooltip("Melihat lintasan dari samping; arah kamera mengikuti belokan jalur.")]
    public bool menyampingJalur;
    public bool balikSisi;
    [Min(1)] public float jarakSamping=5.4f;
    public float tinggiSamping=2.25f, tinggiFokusSamping=.7f;
    public Vector3 posisi, bidikan, acuan;
    [Range(20,90)] public float sudut=60;
    [Min(.1f)] public float kehalusan=5;
    bool siap;
    Vector3 posisiHalus; Quaternion rotasiHalus; float sudutHalus;
    static Camera latarKhusus;
    static CameraClearFlags latarSemula;
    static Color warnaSemula;
    public static void PulihkanLatar(){if(latarKhusus!=null){latarKhusus.clearFlags=latarSemula;latarKhusus.backgroundColor=warnaSemula;latarKhusus=null;}}
    public void Terapkan(Camera camera, Vector3 titik, bool langsung)
    {
        if(!gunakan){siap=false;return;}
        Vector3 delta=ikutiArthur?titik-acuan:Vector3.zero;
        Vector3 p=posisi+delta, b=bidikan+delta;
        if(menyampingJalur)
        {
            if(latarKhusus!=camera){PulihkanLatar();latarKhusus=camera;latarSemula=camera.clearFlags;warnaSemula=camera.backgroundColor;}
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.42f,.44f,.43f);
            var route=GetComponent<JalurKampus>();
            if(route!=null)
            {
                route.Sampel(route.JarakTerdekat(titik),out _,out var arah,out _);
                arah.y=0;arah.Normalize();
                var kaki=titik-Vector3.up*ArthurJalurKampus.JarakKaki;
                p=kaki+Vector3.Cross(Vector3.up,arah)*jarakSamping*(balikSisi?-1:1)+Vector3.up*tinggiSamping;
                b=kaki+Vector3.up*tinggiFokusSamping;
            }
        }
        if((b-p).sqrMagnitude<.001f)return;
        float t=langsung||!siap?1:1-Mathf.Exp(-kehalusan*Time.deltaTime);
        posisiHalus=Vector3.Lerp(posisiHalus,p,t);rotasiHalus=Quaternion.Slerp(rotasiHalus,Quaternion.LookRotation(b-p),t);sudutHalus=Mathf.Lerp(sudutHalus,sudut,t);
        camera.transform.SetPositionAndRotation(posisiHalus,rotasiHalus);
        camera.fieldOfView=sudutHalus;siap=true;
        if(menyampingJalur)
        {
            // Potongan interior hanya pada geometri di atas lantai; ubin di depan
            // kamera tetap utuh dan collider pintu/tembok tidak berubah.
            var normal=camera.transform.forward;normal.y=0;normal.Normalize();
            var potong=titik-Vector3.up*ArthurJalurKampus.JarakKaki-normal*.65f;
            Shader.SetGlobalVector("_CrystalCutPlane",new Vector4(normal.x,normal.y,normal.z,-Vector3.Dot(normal,potong)));
            Shader.SetGlobalFloat("_CrystalCutActive",1);
            Shader.SetGlobalFloat("_CrystalFloorY",titik.y-ArthurJalurKampus.JarakKaki);
            camera.nearClipPlane=.05f;
        }
    }
    void OnDisable(){siap=false;if(menyampingJalur){Shader.SetGlobalFloat("_CrystalCutActive",0);PulihkanLatar();}}
}
