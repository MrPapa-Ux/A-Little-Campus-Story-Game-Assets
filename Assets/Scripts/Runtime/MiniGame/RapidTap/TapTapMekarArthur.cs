using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Tantangan cerita Dining; terpisah dari permainan rhythm HP pembuka dan latihan Raka.
public sealed class TapTapMekarArthur : MonoBehaviour
{
    public enum Fase { Tertutup, Petunjuk, Bermain, Gagal, Menang, Punchline, Refleksi, Selesai }
    [Header("Isi kata dapat direvisi pemilik; daftar awal adalah usulan, bukan kutipan GDD")]
    public string[] kataBuruk={"Gagal","Tertinggal","Tidak mampu","Beban","Percuma","Takut","Ragu","Sendiri","Terlambat","Tidak cukup","Dibandingkan","Dihakimi","Mengecewakan","Tak berguna","Lemah","Tidak layak","Terabaikan","Kalah","Buntu","Menyerah"};
    [Tooltip("Nilai penyesuaian permainan, tidak ditetapkan angkanya dalam GDD.")]
    [Min(2)]public float batasPercobaanAwal=8;
    public AdeganMakanDiningArthur adegan;
    public GameObject panel,lembar,areaPermainan,areaRefleksi;
    public TMP_Text judul,petunjuk,kemajuan,waktu,labelLanjut,labelKembali,teksRefleksi;
    public TMP_Text[] labelKata;
    public UnityEngine.UI.Button ketuk,lanjut,kembali,lanjutRefleksi;
    public BungaMekarArthur bunga;
    public BulatanKataMekar papan;
    public EfekFearMekar fear;
    public AudioSource suara;public AudioClip ketukan;
    public NotifikasiMomenTerekam notifikasiMomen;
    public UnityEvent<int> saatMomenTerekam=new UnityEvent<int>();
    public UnityEvent saatSelesai=new UnityEvent();
    public Fase Status{get;private set;}
    public int Percobaan{get;private set;}
    public int KataTersisih{get;private set;}
    public int JumlahGagal{get;private set;}
    public bool Momen3Aktif{get;private set;}
    public bool Momen4Aktif{get;private set;}
    public float SisaWaktu=>Mathf.Max(0,batasPercobaanAwal-berjalan);
    int halaman,indeksRefleksi,frameKetuk=-1;float berjalan,denyut;bool siap;
    readonly float[] pudar=new float[20];
    readonly string[] refleksi={"Aku harus tenang.","Orang sabar disayang Tuhan.","Aku harus banyak berdoa."};
    void Awake(){Siapkan();panel.SetActive(false);areaRefleksi.SetActive(false);Status=Fase.Tertutup;}
    void Start(){TandaiMasukDining();}
    void Siapkan(){if(siap)return;siap=true;ketuk.onClick.AddListener(Ketuk);lanjut.onClick.AddListener(Lanjut);kembali.onClick.AddListener(Sebelumnya);lanjutRefleksi.onClick.AddListener(LanjutRefleksi);if(adegan!=null){adegan.saatMakanSelesai.AddListener(BukaPetunjuk);adegan.saatPunchlineSelesai.AddListener(MulaiRefleksi);}}
    public void TandaiMasukDining(){if(Momen3Aktif)return;Momen3Aktif=true;Momen(3);}
    void Momen(int n){if(notifikasiMomen!=null)notifikasiMomen.Tampilkan(n);saatMomenTerekam.Invoke(n);}
    public void BukaPetunjuk(){Siapkan();if(Status!=Fase.Tertutup)return;TandaiMasukDining();halaman=0;Status=Fase.Petunjuk;panel.SetActive(true);lembar.SetActive(true);areaRefleksi.SetActive(false);TulisPetunjuk();}
    void TulisPetunjuk(){petunjuk.rectTransform.anchoredPosition=new Vector2(0,-330);petunjuk.rectTransform.sizeDelta=new Vector2(1300,130);petunjuk.fontSize=36;judul.text="TAP-TAP MEKAR";areaPermainan.SetActive(false);papan.gameObject.SetActive(false);kemajuan.text="";waktu.text="";petunjuk.text=halaman==0?"Ketuk bulatan yang muncul di sekitar Arthur. Setiap bulatan berisi satu kata.":"Ketuk bulatan sebelum cincin waktunya habis untuk menyisihkan kata tersebut.\nTekan MULAI untuk bermain.";labelLanjut.text=halaman==0?"LANJUT":"MULAI";lanjut.gameObject.SetActive(true);kembali.gameObject.SetActive(halaman==1);labelKembali.text="SEBELUMNYA";fear.Atur(.15f);}
    public void Sebelumnya(){if(Status==Fase.Petunjuk&&halaman==1){halaman=0;TulisPetunjuk();}}
    public void Lanjut(){
        if(Status==Fase.Petunjuk){if(halaman==0){halaman=1;TulisPetunjuk();}else MulaiPercobaan();}
        else if(Status==Fase.Gagal)MulaiPercobaan();
        else if(Status==Fase.Menang)StartCoroutine(AkhirMekar());
    }
    public void MulaiPercobaan(){
        if(Status!=Fase.Petunjuk&&Status!=Fase.Gagal)return;
        Percobaan++;KataTersisih=0;frameKetuk=-1;berjalan=0;Status=Fase.Bermain;denyut=0;
        areaPermainan.SetActive(true);lanjut.gameObject.SetActive(false);kembali.gameObject.SetActive(false);ketuk.interactable=true;
        petunjuk.rectTransform.anchoredPosition=new Vector2(0,290);petunjuk.rectTransform.sizeDelta=new Vector2(1300,80);petunjuk.fontSize=32;
        judul.text="TAP-TAP MEKAR";petunjuk.text="";
        areaPermainan.SetActive(false);ketuk.gameObject.SetActive(false);papan.gameObject.SetActive(true);papan.Mulai();fear.MulaiOvt(.68f);TulisKemajuan();
    }
    void Update(){
        float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);
        if(Status==Fase.Bermain && (Application.isFocused
#if UNITY_EDITOR
            ||JedaSaatTidakAktif.PengujianLatarBelakang
#endif
            )){
            berjalan+=dt;TulisKemajuan();if(Percobaan<=3&&berjalan>=batasPercobaanAwal)Hasil(false);
        }
        if(Status==Fase.Bermain){papan.Perbarui(dt);fear.Atur(Percobaan<=3?.68f+.12f*(1-SisaWaktu/batasPercobaanAwal):.48f+.20f*(1-KataTersisih/20f));}
    }
    void TulisKemajuan(){kemajuan.text=KataTersisih+" / 20 kata";waktu.text=Percobaan<=3?SisaWaktu.ToString("0.0")+" detik":"Selesaikan dengan tenang.";}
    public void Ketuk(){ }
    public void KetukKata(int i){
        if(Status!=Fase.Bermain||frameKetuk==Time.frameCount||!papan.Terlihat(i))return;frameKetuk=Time.frameCount;
        papan.Sisihkan(i);KataTersisih++;TulisKemajuan();
        if(suara!=null&&ketukan!=null)suara.PlayOneShot(ketukan,.25f);
        if(KataTersisih==20)Hasil(Percobaan>=4);
    }
    void Hasil(bool berhasil){
        if(Status!=Fase.Bermain)return;papan.Hentikan();papan.gameObject.SetActive(false);petunjuk.rectTransform.anchoredPosition=new Vector2(0,-330);ketuk.interactable=false;Status=berhasil?Fase.Menang:Fase.Gagal;
        if(berhasil){judul.text="BERHASIL";petunjuk.text="Kata-kata telah tersisih.";labelLanjut.text="LANJUT";fear.Atur(0);Momen4Aktif=true;}
        else {JumlahGagal++;kemajuan.text="Kata-kata kembali muncul.";judul.text="BELUM BERHASIL";petunjuk.text="Kata-kata itu kembali memenuhi pikiran Arthur.";labelLanjut.text="COBA LAGI";fear.Atur(.85f);}
        waktu.text="";lanjut.gameObject.SetActive(true);
    }
    IEnumerator AkhirMekar(){Status=Fase.Punchline;fear.Atur(0);panel.SetActive(false);adegan.TampilkanPunchlineSetelahMekar();yield break;}
    void MulaiRefleksi(){if(Status!=Fase.Punchline)return;Status=Fase.Refleksi;panel.SetActive(true);lembar.SetActive(false);areaRefleksi.SetActive(true);indeksRefleksi=0;teksRefleksi.text=refleksi[0];}
    public void LanjutRefleksi(){if(Status!=Fase.Refleksi)return;if(++indeksRefleksi<refleksi.Length){teksRefleksi.text=refleksi[indeksRefleksi];return;}Status=Fase.Selesai;panel.SetActive(false);fear.Atur(0);adegan.KembaliKePoseTenang();saatSelesai.Invoke();}
    [ContextMenu("Ulangi pratinjau Mekar (saat Play)")]
    public void Ulangi(){if(!Application.isPlaying)return;StopAllCoroutines();Percobaan=JumlahGagal=KataTersisih=0;Momen3Aktif=Momen4Aktif=false;Status=Fase.Tertutup;fear.Atur(0);panel.SetActive(false);adegan.KembaliKePoseTenang();BukaPetunjuk();}
    void OnDestroy(){if(adegan!=null){adegan.saatMakanSelesai.RemoveListener(BukaPetunjuk);adegan.saatPunchlineSelesai.RemoveListener(MulaiRefleksi);}}
}
