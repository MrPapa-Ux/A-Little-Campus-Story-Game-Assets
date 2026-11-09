// PANDUAN PROYEK - Runtime / Ponsel/Musik
// Fungsi: Menampilkan notifikasi judul dan ikon berputar berdasarkan pemutar musik yang aktif.
// Panduan: Assets/Scripts/Runtime/Ponsel/Musik/README.md

using UnityEngine;
using TMPro;

// Notifikasi saja: audio dimiliki MusikArthur agar tidak ada lagu diputar ganda.
[RequireComponent(typeof(CanvasGroup))]
public class PemutarMusikUI : MonoBehaviour
{
    public RectTransform ikonMusik;
    public TMP_Text judul;
    public float kecepatanPutar=-150f,durasiTampil=5f,kecepatanFade=1.5f;
    public MusikArthur pemutar;
    public SistemHP hp;
    CanvasGroup grup;
    float sisa;
    AudioClip clipTerakhir;
    bool memutarTerakhir;
    bool ditutupiAplikasi;
    void OnEnable()
    {
        grup=GetComponent<CanvasGroup>();grup.alpha=0;grup.blocksRaycasts=false;grup.interactable=false;
        var lama=GetComponent<AudioSource>();if(lama!=null){lama.playOnAwake=false;lama.Stop();}
        if(judul==null)judul=GetComponentInChildren<TMP_Text>(true);
        Sambungkan();
    }
    void Start(){Sambungkan();}
    void Sambungkan()
    {
        if(pemutar==null)pemutar=MusikArthur.Instance;
        if(pemutar==null)return;
        pemutar.Berubah-=Segarkan;pemutar.Berubah+=Segarkan;Segarkan();
    }
    void OnDisable(){if(pemutar!=null)pemutar.Berubah-=Segarkan;}
    void Segarkan()
    {
        bool main=pemutar!=null&&pemutar.Memutar;
        var clip=pemutar==null?null:pemutar.sumber.clip;
        if(main && (!memutarTerakhir || clip!=clipTerakhir)){sisa=durasiTampil;grup.alpha=1;}
        if(judul!=null)judul.text=clip==null?"":"Now Playing: "+(pemutar.Lagu?.judul??clip.name)+(main?"":" (jeda)");
        memutarTerakhir=main;clipTerakhir=clip;
    }
    void Update()
    {
        if(pemutar==null){Sambungkan();return;}
        if(pemutar.Memutar!=memutarTerakhir)Segarkan();
        if(hp!=null&&hp.AplikasiSedangBuka){ditutupiAplikasi=true;grup.alpha=0;return;}
        if(ditutupiAplikasi){ditutupiAplikasi=false;if(sisa>0&&clipTerakhir!=null)grup.alpha=1;}
        if(grup.alpha<=0)return;
        if(pemutar.Memutar&&ikonMusik!=null)ikonMusik.Rotate(0,0,kecepatanPutar*Time.unscaledDeltaTime);
        sisa-=Time.unscaledDeltaTime;
        if(sisa<=0)grup.alpha=Mathf.MoveTowards(grup.alpha,0,Time.unscaledDeltaTime*kecepatanFade);
    }
}
