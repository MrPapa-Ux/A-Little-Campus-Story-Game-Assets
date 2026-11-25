using System;
using TMPro;
using UnityEngine;

// Area marker, not a surveyed GPS projection of independently exported Minecraft worlds.
public sealed class PosisiArthurDiPeta : MonoBehaviour
{
    public PetaKampusHP peta;
    public ArthurJalurKampus arthur;
    public RectTransform penanda;
    public TMP_Text labelPosisi;
    public RectTransform tulisanYou;
    [Tooltip("Posisi area yang bulatan petanya sedang disembunyikan; tetap dipakai oleh penanda You.")]
    public TitikPetaKampus[] areaTanpaLingkaran=Array.Empty<TitikPetaKampus>();
    public string AreaSaatIni {get;private set;}="";
    public Vector2 PosisiNormal {get;private set;}
    Vector2 letakTulisan=new Vector2(0,85);
    float berikutnya;
    void OnEnable(){berikutnya=0;Perbarui();}
    void Update(){if(tulisanYou!=null)tulisanYou.anchoredPosition=letakTulisan+Vector2.up*(Mathf.Sin(Time.unscaledTime*2.6f)*3);if(Time.unscaledTime<berikutnya)return;berikutnya=Time.unscaledTime+.15f;Perbarui();}
    public static string NamaArea(string n)
    {
        if(string.IsNullOrEmpty(n))return "Jasmine";
        if(n=="GA_Taman_MenujuPioneer")return "Jalan menuju Chapel";
        if(n.StartsWith("Pioneer_",StringComparison.Ordinal))return "Chapel";
        if(n.StartsWith("GA_",StringComparison.Ordinal))return "GA";
        if(n.StartsWith("GH_",StringComparison.Ordinal))return "Guest House";
        if(n.StartsWith("GK2_",StringComparison.Ordinal))return "GK 2";
        if(n.StartsWith("GK1_",StringComparison.Ordinal))return "GK 1";
        if(n.StartsWith("GK3_",StringComparison.Ordinal))return "GK 3";
        if(n.StartsWith("Kantin_",StringComparison.Ordinal))return "Kantin";
        if(n.StartsWith("UNK_Dining",StringComparison.Ordinal))return "Kafetaria";
        if(n.StartsWith("UNK_Crystal",StringComparison.Ordinal))return "Crystal";
        if(n.StartsWith("UNK_StudyGarden",StringComparison.Ordinal))return "Study Garden";
        if(n=="UNK_TamanKanan"||n=="UNK_TanggaTaman")return "Crystal";
        if(n=="UNK_Plaza"||n=="UNK_TanggaPlaza"||n=="UNK_Selasar")return "Edelweiss";
        return n.StartsWith("UNK_",StringComparison.Ordinal)?"Taman UNKLAB":"";
    }
    bool Posisi(string nama,out Vector2 p){foreach(var l in peta.lokasi)if(l.nama==nama){p=l.posisi;return true;}foreach(var l in areaTanpaLingkaran)if(l!=null&&l.nama==nama){p=l.posisi;return true;}p=default;return false;}
    public void Perbarui()
    {
        if(peta==null||peta.isiPeta==null||penanda==null||arthur==null)return;
        string area=NamaArea(arthur.JalurAktif!=null?arthur.JalurAktif.name:"");Vector2 pos;bool ada;
        if(area=="Jalan menuju Chapel"){
            ada=Posisi("GA",out var a)&Posisi("Chapel",out var b);
            pos=Vector2.Lerp(a,b,arthur.JarakDiJalur/Mathf.Max(.01f,arthur.JalurAktif.Panjang));
        }else if(area=="Taman UNKLAB"){
            ada=Posisi("GA",out var a)&Posisi("Kafetaria",out var b);pos=Vector2.Lerp(a,b,.5f);
        }else ada=Posisi(area,out pos);
        AreaSaatIni=area;PosisiNormal=pos;penanda.gameObject.SetActive(ada);
        peta.TandaiAreaArthur(area);
        // Panah berhenti di atas tepi lingkaran. Di Kafetaria gunakan sisi kanan
        // supaya nama tempat yang tercetak pada gambar tetap terbaca.
        letakTulisan=area=="Kafetaria"?new Vector2(100,55):new Vector2(0,85);
        if(labelPosisi!=null)labelPosisi.text="";
        if(!ada)return;
        penanda.anchoredPosition=new Vector2((pos.x-.5f)*peta.isiPeta.rect.width,(.5f-pos.y)*peta.isiPeta.rect.height);
        penanda.localScale=Vector3.one/Mathf.Max(1,peta.Zoom);
    }
}
