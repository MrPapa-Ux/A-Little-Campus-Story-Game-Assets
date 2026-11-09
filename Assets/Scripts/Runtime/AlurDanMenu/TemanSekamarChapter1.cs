using UnityEngine;

// Hanya dipasang pada Atan di adegan cerita. Dialog Feth memakai skenario biasa.
[RequireComponent(typeof(NpcYangBisaDiatur),typeof(AdeganNpcTaman))]
public sealed class TemanSekamarChapter1 : MonoBehaviour
{
    public CeritaKamar201Arthur cerita;
    [TextArea(2,4)] public string petunjuk="Hard diskmu belum ketemu? Coba periksa di bawah lemari di belakangku.";
    public BarisDialog[] setelahHardiskDitemukan;
    public bool SudahMemberiSelamat {get;private set;}
    AreaInteraksi area;AdeganNpcTaman penutup;SistemPilihan pilihan;bool dialogDiperbarui;
    void Start(){
        area=GetComponent<AreaInteraksi>();penutup=GetComponent<AdeganNpcTaman>();
        pilihan=FindFirstObjectByType<SistemPilihan>();
        if(pilihan!=null)pilihan.PercakapanSelesai+=Selesai;
        penutup.menghilangSetelahDialog=false;
        SegarkanDialog();
    }
    void Update(){if(!dialogDiperbarui&&cerita!=null&&cerita.Status==CeritaKamar201Arthur.Tahap.Selesai&&
        !(DialogManager.Instance!=null&&DialogManager.Instance.PercakapanAktif))SegarkanDialog();}
    void SegarkanDialog(){
        bool ditemukan=cerita!=null&&cerita.Status==CeritaKamar201Arthur.Tahap.Selesai;
        area.skenarioDialog=ditemukan?setelahHardiskDitemukan:new[]{new BarisDialog{
            siapaYangBicara=Pembicara.Kanan_NPC,namaKarakter="Atan",teksDialog=petunjuk,teksTombolLanjut="Selesai"}};
        dialogDiperbarui=ditemukan;penutup.menghilangSetelahDialog=ditemukan;
    }
    void Selesai(AreaInteraksi pemilik,bool tuntas){if(pemilik==area&&tuntas&&dialogDiperbarui)SudahMemberiSelamat=true;}
    void OnDestroy(){if(pilihan!=null)pilihan.PercakapanSelesai-=Selesai;}
}
