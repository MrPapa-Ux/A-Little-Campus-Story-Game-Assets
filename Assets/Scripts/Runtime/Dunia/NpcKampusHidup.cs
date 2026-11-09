using UnityEngine;

public enum KegiatanNpcKampus { Berdiri, Mengobrol, Belajar, Duduk, Penjual, Joget, TepukTangan, Berlari }

// Figuran tidak mempunyai collider penahan jalan atau AI pencarian lintasan.
public sealed class NpcKampusHidup : MonoBehaviour
{
    public KegiatanNpcKampus kegiatan;
    public Transform visual, kepala, badan, tanganKiri, tanganKanan, kakiKiri, kakiKanan;
    public float fase;
    public float jarakAnimasi=42;
    [Tooltip("Gerak idle Arthur yang mengikuti pose awal NPC, tanpa mengubah rambut atau lokasi NPC.")]
    public PolaIdleArthurNpc idleArthur;
    Renderer[] gambar;Transform[] bagian;Vector3[] posisi;Quaternion[] rotasi;
    ArthurJalurKampus arthur;float berikut;bool terlihat=true;
    void Awake()
    {
        gambar=GetComponentsInChildren<Renderer>(true);bagian=new[]{kepala,badan,tanganKiri,tanganKanan,kakiKiri,kakiKanan};posisi=new Vector3[6];rotasi=new Quaternion[6];
        for(int i=0;i<6;i++)if(bagian[i]!=null){posisi[i]=bagian[i].localPosition;rotasi[i]=bagian[i].localRotation;}
    }
    void LateUpdate()
    {
        if(arthur==null)arthur=FindFirstObjectByType<ArthurJalurKampus>();
        if(arthur==null||arthur.kamera==null)return;
        bool dekat=(arthur.transform.position-transform.position).sqrMagnitude<jarakAnimasi*jarakAnimasi;
        if(terlihat!=dekat){terlihat=dekat;foreach(var renderer in gambar)if(renderer!=null)renderer.enabled=dekat;}
        if(!dekat)return;
        if(visual!=null)visual.rotation=Quaternion.Euler(0,arthur.kamera.transform.eulerAngles.y,0);
        if(Time.time<berikut)return;berikut=Time.time+.08f;
        float t=Time.time+fase,s=Mathf.Sin(t*1.6f),r=Mathf.Sin(t*.7f);
        bool joget=kegiatan==KegiatanNpcKampus.Joget,tepuk=kegiatan==KegiatanNpcKampus.TepukTangan,lari=kegiatan==KegiatanNpcKampus.Berlari;
        if(joget||tepuk||lari)
        {
            float ayun=Mathf.Sin(t*(lari?12:5.5f));
            for(int i=0;i<bagian.Length;i++)if(bagian[i]!=null)
            {
                Vector3 p=posisi[i];float a=0;
                if(i<2){p+=new Vector3(joget?ayun*.10f:0,Mathf.Abs(ayun)*(lari?.07f:.025f),0);a=joget?ayun*6:ayun*1.5f;}
                else if(i<4){float tanda=i==2?1:-1;
                    if(tepuk){p.x=Mathf.Lerp(posisi[i].x,(posisi[2].x+posisi[3].x)*.5f, .5f+.4f*Mathf.Cos(t*7));p.y+=.18f;a=tanda*20;}
                    else {p.y+=joget?.15f:0;a=ayun*tanda*(lari?22:30);}}
                else {float langkah=ayun*(i==4?1:-1);p.x+=langkah*(lari?.23f:.035f);p.y+=Mathf.Max(0,langkah)*(lari?.19f:.025f);}
                bagian[i].localPosition=p;bagian[i].localRotation=rotasi[i]*Quaternion.Euler(0,0,a);
            }
            return;
        }
        bool bicara=kegiatan==KegiatanNpcKampus.Mengobrol,belajar=kegiatan==KegiatanNpcKampus.Belajar;
        if(idleArthur!=null){
            for(int i=0;i<bagian.Length;i++)if(bagian[i]!=null){bagian[i].localPosition=posisi[i]+idleArthur.Gerak(i,t);bagian[i].localRotation=rotasi[i];}
            return;
        }
        for(int i=0;i<4;i++)if(bagian[i]!=null){float kecil=i==0?.015f:.008f;bagian[i].localPosition=posisi[i]+Vector3.up*s*kecil;
            float sudut=i==0?(belajar?2:bisik(bicara))*r:i>=2?(bicara?7:kegiatan==KegiatanNpcKampus.Penjual?5:belajar?1:2)*Mathf.Sin(t*(bicara?2:1)+(i==2?0:2)):s;
            bagian[i].localRotation=rotasi[i]*Quaternion.Euler(0,0,sudut);}
    }
    static float bisik(bool bicara)=>bicara?4:1.5f;
}
