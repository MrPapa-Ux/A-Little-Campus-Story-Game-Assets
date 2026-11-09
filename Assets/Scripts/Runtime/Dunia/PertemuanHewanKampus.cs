using System;
using UnityEngine;

public sealed class PertemuanHewanKampus : MonoBehaviour
{
    public enum Hasil { TidakAda, Anjing, Kucing, Keduanya }
    [Range(0,1)] public float peluang=.2f;
    public Sprite[] bagianAnjing,bagianKucing;
    public Sprite[] bagianAnjingNormal,bagianKucingNormal;
    public Material bahan;
    public Material bahanBayangan;
    [Min(4f)] public float jarakMunculMinimal=7f;
    public Hasil HasilKunjungan { get; private set; }
    public string Zona { get; private set; }="";
    public int JumlahUndian { get; private set; }
    public bool VarianHumanoid { get; private set; }
    readonly System.Random random=new System.Random();
    ArthurJalurKampus arthur;HewanKampus anjing,kucing;
    float cobaBerikutnya;
    public static Hasil Pilih(float undian,int jenis,float peluang=.2f)=>undian<peluang?(Hasil)Mathf.Clamp(jenis,1,3):Hasil.TidakAda;
    public static bool PilihHumanoid(float undian,float peluang=.2f)=>undian<Mathf.Min(.01f,peluang);
    public static string ZonaUntuk(JalurKampus r)
    {
        if(r==null)return "";
        var n=r.name;
        if(n.StartsWith("UNK_Crystal",StringComparison.Ordinal)||n=="UNK_Bundaran_JalurKananCrystal")return "Crystal";
        if(n.StartsWith("UNK_Dining",StringComparison.Ordinal)||n.StartsWith("UNK_StudyGarden",StringComparison.Ordinal))return "Dining";
        if(n=="UNK_TamanKanan"||n=="UNK_TanggaTaman")return "Crystal";
        if(n=="UNK_Plaza"||n=="UNK_TanggaPlaza"||n=="UNK_Selasar")return "Edel";
        if(n.StartsWith("UNK_",StringComparison.Ordinal))return "TamanUNKLAB";
        return r.name=="GA_Taman_MenujuPioneer"?"MenujuPioneer":"";
    }
    public static bool BolehMuncul(JalurKampus r)
    {
        if(r==null)return false;var n=r.name;
        return n=="GA_Taman_MenujuPioneer"||n=="UNK_Tugu"||n=="UNK_Plaza"||n=="UNK_Selasar"||n=="UNK_TamanKanan"||n=="UNK_Dining_Depan"||n=="UNK_Dining_Depan_Lanjutan"||n=="UNK_Crystal_Depan"||n=="UNK_Crystal_DepanKanan"||n=="UNK_Crystal_JalurKiri"||n=="UNK_Bundaran_JalurKananCrystal"||n.StartsWith("UNK_Dining_Jalur",StringComparison.Ordinal);
    }
    void Start(){arthur=FindFirstObjectByType<ArthurJalurKampus>();}
    void Update()
    {
        if(arthur==null||KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat)return;
        var route=arthur.JalurAktif;string zone=ZonaUntuk(route);
        if(zone!=Zona){Hapus();Zona=zone;HasilKunjungan=Hasil.TidakAda;VarianHumanoid=false;if(zone!=""){JumlahUndian++;float roll=(float)random.NextDouble();HasilKunjungan=Pilih(roll,random.Next(1,4),peluang);VarianHumanoid=PilihHumanoid(roll,peluang);}}
        bool outdoors=BolehMuncul(route);
        // A successful encounter must not be stranded on a distant, invisible old road.
        // Reposition the same result only when every pet is outside the view and far away.
        if(outdoors&&(anjing!=null||kucing!=null)&&BolehPindah(anjing,route)&&BolehPindah(kucing,route))Hapus();
        if(outdoors&&HasilKunjungan!=Hasil.TidakAda&&anjing==null&&kucing==null&&Time.time>=cobaBerikutnya){Buat(route);cobaBerikutnya=Time.time+.5f;}
        if(anjing!=null)anjing.gameObject.SetActive(outdoors&&(anjing.transform.position-arthur.transform.position).sqrMagnitude<1600);
        if(kucing!=null)kucing.gameObject.SetActive(outdoors&&(kucing.transform.position-arthur.transform.position).sqrMagnitude<1600);
    }
    bool BolehPindah(HewanKampus pet,JalurKampus route)
    {
        if(pet==null)return true;
        if(pet.Jalur==route||(pet.transform.position-arthur.transform.position).sqrMagnitude<18*18)return false;
        var c=arthur.kamera;if(c==null)return false;var p=c.WorldToViewportPoint(pet.transform.position+Vector3.up*.4f);
        return p.z<=0||p.x<-.15f||p.x>1.15f||p.y<-.15f||p.y>1.15f;
    }
    void Buat(JalurKampus route)
    {
        float dogSide=-1.5f,catSide=1.7f;
        // Stay on the outside of the flower divider on the two Dining approaches.
        if(route.name=="UNK_Dining_JalurAtas"){dogSide=-1.5f;catSide=-1.7f;}
        if(route.name=="UNK_Dining_JalurBawah"){dogSide=1.5f;catSide=1.7f;}
        var dog=VarianHumanoid?bagianAnjing:bagianAnjingNormal;var cat=VarianHumanoid?bagianKucing:bagianKucingNormal;
        if((HasilKunjungan==Hasil.Anjing||HasilKunjungan==Hasil.Keduanya)&&dog!=null&&dog.Length==7)anjing=Spawn(false,dog,route,dogSide,9f);
        if((HasilKunjungan==Hasil.Kucing||HasilKunjungan==Hasil.Keduanya)&&cat!=null&&cat.Length==7)kucing=Spawn(true,cat,route,catSide,14f);
    }
    HewanKampus Spawn(bool cat,Sprite[] parts,JalurKampus route,float lateral,float offset)
    {
        float d=arthur.JarakDiJalur;float sign=d<route.Panjang*.5f?1:-1;
        for(int i=0;i<8;i++){
            float direction=i<4?sign:-sign;float distance=Mathf.Clamp(d+direction*(offset+(i%4)*3),.6f,Mathf.Max(.6f,route.Panjang-.6f));
            route.Sampel(distance,out var p,out var tangent,out _);if(Mathf.Abs(tangent.y)>.15f)continue;
            var world=p-Vector3.up*ArthurJalurKampus.JarakKaki+Vector3.Cross(Vector3.up,tangent).normalized*lateral;
            var delta=world-arthur.transform.position;delta.y=0;if(delta.sqrMagnitude<jarakMunculMinimal*jarakMunculMinimal)continue;
            if(cat&&anjing!=null){var between=world-anjing.transform.position;between.y=0;if(between.sqrMagnitude<25)continue;}
            var go=new GameObject(cat?"KucingKampus":"AnjingKampus");go.transform.SetParent(transform);var pet=go.AddComponent<HewanKampus>();
            pet.Siapkan(cat,parts,bahan,route,distance,lateral,arthur,VarianHumanoid,bahanBayangan);return pet;
        }
        return null;
    }
    void Hapus(){if(anjing!=null)Destroy(anjing.gameObject);if(kucing!=null)Destroy(kucing.gameObject);anjing=null;kucing=null;}
    void OnDisable(){Hapus();Zona="";}
#if UNITY_EDITOR
    // Explicit test hook; production visits always use the single probability draw above.
    public void Pratinjau(Hasil hasil,bool humanoid=false){if(arthur==null)arthur=FindFirstObjectByType<ArthurJalurKampus>();Hapus();Zona=ZonaUntuk(arthur.JalurAktif);HasilKunjungan=hasil;VarianHumanoid=humanoid;if(BolehMuncul(arthur.JalurAktif))Buat(arthur.JalurAktif);}
#endif
}
