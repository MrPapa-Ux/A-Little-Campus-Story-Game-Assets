using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CeritaKamar201Arthur : MonoBehaviour
{
    [Serializable] public class Halaman {public string judul;[TextArea(3,8)] public string isi;public Vector2 titikPetunjuk;public float sudutPanah;public Sprite gambar;}
    public enum Tahap {Memuat,Tutorial,Mencari,WaktuHabis,Selesai}
    public Halaman[] tutorial;
    public GameObject panel;
    public TMP_Text judul,isi,nomor,teksLanjut,timer;
    public Button kembali,lanjut;
    public RectTransform penandaTutorial;
    public UnityEngine.UI.Image gambarPetunjuk;
    public KarakterController pemain;
    public ArthurJalurKampus arthur;
    public Transform titikAwal;
    public BukuArthur buku;
    public AreaInteraksi areaHardisk;
    public GameObject titikHardisk,visualHardisk;
    public InteraksiJalurKampus pintuKeluar;
    public Sprite ikonHardisk;
    public LazyBarArthur procrastination;
    public NotifikasiMomenTerekam notifikasiMomen;
    [Min(1)] public float durasiPencarian=240;
    public Tahap Status {get;private set;}=Tahap.Memuat;
    public float SisaWaktu {get;private set;}
    public int HalamanAktif {get;private set;}
    public bool MomenTerekam1Aktif {get;private set;}
    const string IdItem="chapter1_hardisk_arthur",IdQuest="chapter1_cari_hardisk";
    SistemPilihan pilihan;
    SistemHP hp;
    IEnumerator Start(){
        pemain.kunciPergerakan=true;panel.SetActive(false);timer.gameObject.SetActive(false);BlokirInteraksi(true);
        if(procrastination!=null)procrastination.tanpaBatasWaktu=true;
        titikHardisk.SetActive(false);visualHardisk.SetActive(false);
        if(pintuKeluar!=null)pintuKeluar.enabled=false;
        kembali.onClick.AddListener(Sebelumnya);lanjut.onClick.AddListener(Berikutnya);
        pilihan=FindFirstObjectByType<SistemPilihan>();if(pilihan!=null)pilihan.PercakapanSelesai+=SetelahDialog;
        hp=FindFirstObjectByType<SistemHP>();
        while(KawasanKampus.Instance!=null&&!KawasanKampus.Instance.Siap){
            if(!string.IsNullOrEmpty(KawasanKampus.Instance.KesalahanTerakhir)){judul.text="Kamar belum berhasil dimuat";isi.text="Hentikan dan coba Play kembali.";panel.SetActive(true);yield break;}
            yield return null;
        }
        while(KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat)yield return null;
        // Transisi kawasan memiliki kunci kendalinya sendiri; tutorial tetap mengunci pemain.
        yield return new WaitForSeconds(.3f);
        MomenTerekam1Aktif=true;Status=Tahap.Tutorial;HalamanAktif=0;TampilkanTutorial();
    }
    void Update(){
        if(Status==Tahap.Tutorial||Status==Tahap.WaktuHabis)pemain.kunciPergerakan=true;
        if(Status!=Tahap.Mencari)return;
        if(procrastination!=null&&procrastination.waktuHabis)return;
        if(DialogManager.Instance!=null&&DialogManager.Instance.PercakapanAktif)return;
        if(hp!=null&&hp.MenjedaParameter)return;
        SisaWaktu=Mathf.Max(0,SisaWaktu-Time.deltaTime);SegarkanTimer();
        if(SisaWaktu<=0){
            if(procrastination!=null){procrastination.GagalKarenaBatasWaktu();return;}
            TampilkanGagal();
        }
    }
    public void TampilkanGagal(){
        if(Status==Tahap.Selesai){var chapel=FindFirstObjectByType<PerjalananChapelChapter1>();if(chapel!=null&&chapel.PulihkanSetelahGlitch())return;GetComponent<PerjalananDandiChapter1>()?.GagalPerjalanan();return;}
            Status=Tahap.WaktuHabis;pemain.kunciPergerakan=true;titikHardisk.SetActive(false);visualHardisk.SetActive(false);
            panel.SetActive(true);BlokirInteraksi(true);judul.text="Coba Lagi";isi.text="Pencarian belum selesai.";
            nomor.text="";kembali.gameObject.SetActive(false);teksLanjut.text="Coba Lagi";
    }
    public void Sebelumnya(){if(Status!=Tahap.Tutorial)return;HalamanAktif=Mathf.Max(0,HalamanAktif-1);TampilkanTutorial();}
    public void Berikutnya(){
        if(Status==Tahap.Tutorial){if(HalamanAktif<tutorial.Length-1){HalamanAktif++;TampilkanTutorial();}else MulaiPencarian();}
        else if(Status==Tahap.WaktuHabis){arthur.PindahKe(titikAwal);MulaiPencarian();}
        else if(Status==Tahap.Selesai){panel.SetActive(false);BlokirInteraksi(false);pemain.BerhentiBergerak();pemain.kunciPergerakan=false;}
    }
    void TampilkanTutorial(){
        panel.SetActive(true);BlokirInteraksi(true);pemain.kunciPergerakan=true;
        judul.text=tutorial[HalamanAktif].judul;isi.text=tutorial[HalamanAktif].isi;nomor.text=(HalamanAktif+1)+" / "+tutorial.Length;
        kembali.gameObject.SetActive(HalamanAktif>0);teksLanjut.text=HalamanAktif==tutorial.Length-1?"Mulai Bermain":"Lanjut";
        if(penandaTutorial!=null){var halaman=tutorial[HalamanAktif];penandaTutorial.anchorMin=penandaTutorial.anchorMax=halaman.titikPetunjuk;penandaTutorial.anchoredPosition=Vector2.zero;penandaTutorial.localRotation=Quaternion.Euler(0,0,halaman.sudutPanah);}
        if(gambarPetunjuk!=null&&tutorial[HalamanAktif].gambar!=null)gambarPetunjuk.sprite=tutorial[HalamanAktif].gambar;
    }
    public void MulaiPencarian(){
        if(Status!=Tahap.Tutorial&&Status!=Tahap.WaktuHabis)return;
        panel.SetActive(false);BlokirInteraksi(false);Status=Tahap.Mencari;SisaWaktu=durasiPencarian;pemain.BerhentiBergerak();pemain.kunciPergerakan=false;
        titikHardisk.SetActive(true);visualHardisk.SetActive(true);timer.gameObject.SetActive(true);SegarkanTimer();
        if(procrastination!=null){procrastination.JedaKhususCerita=false;procrastination.PulihkanBar();procrastination.tanpaBatasWaktu=false;}
        if(notifikasiMomen!=null)notifikasiMomen.Tampilkan(1);
        buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{id=IdQuest,judul="Cari Hardisk di kamar",dikendalikanQuest=true,isi="Cari Hardisk Arthur di kamar 201. Periksa barang di dekat lemari. Waktu pencarian empat menit."});
    }
    void SetelahDialog(AreaInteraksi area,bool tuntas){if(tuntas&&area==areaHardisk)AmbilHardisk();}
    public bool AmbilHardisk(){
        if(Status!=Tahap.Mencari||Vector3.Distance(arthur.transform.position,areaHardisk.transform.position)>2.3f)return false;
        Status=Tahap.Selesai;titikHardisk.SetActive(false);visualHardisk.SetActive(false);timer.gameObject.SetActive(false);
        buku.Tambah(JenisBukuArthur.Tas,new CatatanArthur{id=IdItem,judul="Hardisk Arthur",ikon=ikonHardisk,jumlah=1,isi="Hard disk milik Arthur. Ia sering lupa menaruhnya."});
        buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{id=IdQuest,judul="Cari Hardisk di kamar",dikendalikanQuest=true,selesai=true,isi="Hardisk Arthur ditemukan dan sudah masuk ke Tas."});
        panel.SetActive(true);BlokirInteraksi(true);judul.text="Hardisk Arthur ditemukan";isi.text="Hard disk telah dimasukkan ke Tas. Buka Tas melalui HP untuk melihat barang.";nomor.text="Tugas selesai";teksLanjut.text="Lanjut";kembali.gameObject.SetActive(false);pemain.kunciPergerakan=true;
        if(pintuKeluar!=null)pintuKeluar.enabled=true;return true;
    }
    void SegarkanTimer(){int detik=Mathf.CeilToInt(SisaWaktu);timer.text="Cari Hardisk di kamar  "+(detik/60).ToString("00")+":"+(detik%60).ToString("00");}
    void BlokirInteraksi(bool blokir){if(DialogManager.Instance!=null)DialogManager.Instance.minigameAktif=blokir;}
    void OnDestroy(){if(pilihan!=null)pilihan.PercakapanSelesai-=SetelahDialog;}
}
