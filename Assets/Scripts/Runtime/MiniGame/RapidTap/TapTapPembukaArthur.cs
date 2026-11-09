using UnityEngine;
using TMPro;
using UnityEngine.Events;

// Percobaan pembuka Chapter 1: hasil naratif kalah lalu menang.
// Tidak menyentuh latihan Raka/rhythm maupun quest mode eksplorasi.
public sealed class TapTapPembukaArthur : MonoBehaviour
{
    public enum Tahap { Mengetik, SiapPertama, RondePertama, KalahPertama, RondeKedua, Menang, Selesai }
    public PratinjauArthurMengetik pembukaan;
    public GameObject ponsel;
    public UnityEngine.UI.Button bukaPonsel, tombolKetuk, tombolMulai, tombolKembali;
    public TMP_Text judul, status, waktu, hitungan, godaan, labelKetuk, labelMulai;
    public UnityEngine.UI.Image isiBar;
    public AudioSource sumberEfek;
    public AudioClip bunyiKetuk;
    [Min(1)] public float durasiPertama=5, durasiKedua=6;
    [Min(1)] public int targetKetukan=15;
    public UnityEvent saatUrutanSelesai=new UnityEvent();
    public Tahap Status {get;private set;}
    public int Ketukan {get;private set;}
    public int JumlahHasilKalah {get;private set;}
    public int JumlahHasilMenang {get;private set;}
    float mulai,denyut;int frameTerakhir=-1;Vector3 ukuranTombol;
    public float SisaDetik => Mathf.Max(0,(Status==Tahap.RondePertama?durasiPertama:durasiKedua)-(Time.unscaledTime-mulai));
    void Awake(){
        ukuranTombol=tombolKetuk.transform.localScale;
        bukaPonsel.onClick.AddListener(Buka);tombolMulai.onClick.AddListener(Lanjut);
        tombolKetuk.onClick.AddListener(Ketuk);tombolKembali.onClick.AddListener(Kembali);
        ponsel.SetActive(false);Status=Tahap.Mengetik;bukaPonsel.gameObject.SetActive(true);
    }
    public void Buka(){
        if(Status!=Tahap.Mengetik)return;
        Status=Tahap.SiapPertama;bukaPonsel.gameObject.SetActive(false);pembukaan.enabled=false;ponsel.SetActive(true);
        judul.text="TAP-TAP";status.text="Sebentar saja... satu permainan dulu.";
        waktu.text="";hitungan.text="";godaan.text="Ketuk tombol berulang.\nSentuhan ditahan tidak dihitung.";
        labelKetuk.text="KETUK DI SINI";labelMulai.text="MULAI";isiBar.fillAmount=0;
        tombolKetuk.interactable=false;tombolMulai.gameObject.SetActive(true);tombolKembali.gameObject.SetActive(true);
    }
    public void Lanjut(){
        if(Status==Tahap.Menang){SelesaikanUrutan();return;}
        if(Status!=Tahap.SiapPertama&&Status!=Tahap.KalahPertama)return;
        bool kedua=Status==Tahap.KalahPertama;Status=kedua?Tahap.RondeKedua:Tahap.RondePertama;
        mulai=Time.unscaledTime;Ketukan=0;frameTerakhir=-1;isiBar.fillAmount=0;
        judul.text="TAP-TAP";status.text=kedua?"Sekali lagi. Aku bisa.":"Ayo, sedikit lagi...";
        godaan.text=kedua?"Mulai dari satu ketukan.":"Pikiranku malah ke mana-mana...";
        hitungan.text="0 KETUKAN";labelKetuk.text="KETUK!";
        tombolKetuk.interactable=true;tombolMulai.gameObject.SetActive(false);tombolKembali.gameObject.SetActive(false);
    }
    public void Ketuk(){
        if((Status!=Tahap.RondePertama&&Status!=Tahap.RondeKedua)||frameTerakhir==Time.frameCount||SisaDetik<=0)return;
        frameTerakhir=Time.frameCount;Ketukan++;denyut=1;
        hitungan.text=Ketukan+" KETUKAN";
        if(sumberEfek!=null&&bunyiKetuk!=null)sumberEfek.PlayOneShot(bunyiKetuk,.4f);
        if(Status==Tahap.RondeKedua&&Ketukan>=targetKetukan)Hasil(true);
    }
    void Update(){
        denyut=Mathf.MoveTowards(denyut,0,Time.unscaledDeltaTime*5);tombolKetuk.transform.localScale=ukuranTombol*(1+denyut*.035f);
        if(Status!=Tahap.RondePertama&&Status!=Tahap.RondeKedua)return;
        float durasi=Status==Tahap.RondePertama?durasiPertama:durasiKedua;
        float elapsed=Mathf.Clamp01((Time.unscaledTime-mulai)/durasi);
        waktu.text=SisaDetik.ToString("0.0")+" DETIK";
        isiBar.fillAmount=Status==Tahap.RondePertama?Mathf.Min(.88f,Ketukan/(float)targetKetukan):Mathf.Clamp01(Mathf.Max(Ketukan/(float)targetKetukan,elapsed));
        if(SisaDetik<=0)Hasil(Status==Tahap.RondeKedua);
    }
    void Hasil(bool menang){
        Status=menang?Tahap.Menang:Tahap.KalahPertama;
        if(menang)JumlahHasilMenang++;else JumlahHasilKalah++;
        judul.text=menang?"BERHASIL!":"BELUM KALI INI";
        status.text=menang?"Akhirnya. Cukup dulu mainnya.":"Ah... coba sekali lagi.";
        godaan.text=menang?"Satu langkah kecil.":"Tarik napas dulu.";
        waktu.text="";labelKetuk.text=menang?"SELESAI":"COBA LAGI";labelMulai.text=menang?"LANJUTKAN":"COBA LAGI";
        isiBar.fillAmount=menang?1:0;tombolKetuk.interactable=false;tombolMulai.gameObject.SetActive(true);tombolKembali.gameObject.SetActive(false);
    }
    public void Kembali(){
        if(Status!=Tahap.SiapPertama)return;
        ponsel.SetActive(false);Status=Tahap.Mengetik;bukaPonsel.gameObject.SetActive(true);pembukaan.enabled=true;
    }
    [ContextMenu("Ulangi contoh Tap-Tap")]
    public void Ulangi(){
        Status=Tahap.Mengetik;Ketukan=JumlahHasilKalah=JumlahHasilMenang=0;denyut=0;
        ponsel.SetActive(false);bukaPonsel.gameObject.SetActive(true);pembukaan.enabled=true;
        if(sumberEfek!=null)sumberEfek.Stop();
    }
    void SelesaikanUrutan(){
        if(Status!=Tahap.Menang)return;
        Status=Tahap.Selesai;ponsel.SetActive(false);pembukaan.enabled=true;saatUrutanSelesai.Invoke();
    }
    void OnDisable(){if(sumberEfek!=null)sumberEfek.Stop();}
}
