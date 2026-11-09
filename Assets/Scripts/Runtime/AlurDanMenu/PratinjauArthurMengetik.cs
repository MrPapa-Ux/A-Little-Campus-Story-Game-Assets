using UnityEngine;
using TMPro;

// Contoh pembukaan saja. Tidak mengaktifkan quest atau mengganti mode eksplorasi.
public sealed class PratinjauArthurMengetik : MonoBehaviour
{
    public Transform kepala, badan, tanganKiri, tanganKanan;
    public Light cahayaLaptop;
    public AudioSource suaraKetikan;
    public TMP_Text teksPikiran;
    public JariMengetikLaptop[] tanganKhusus;
    [TextArea] public string kalimat = "Aku tertekan, aku buat tugas yang tidak menyenangkan ini untuk siapa?";
    public bool tampilkanTeks = true;
    [Min(0)] public float jedaTeks = 3f;
    [Range(5,60)] public float hurufPerDetik = 21f;
    [Range(.2f,1f)] public float kedipKursor = .48f;
    [Min(0)] public float tahanTeksSelesai = 1.5f;
    [Min(.1f)] public float durasiTeksMenghilang = 1f;
    [Min(0)] public float jedaSetelahKoma = 1.5f;
    [Min(0)] public float jedaSetelahTandaTanya = 1.5f;
    float JedaTanda(char huruf)=>huruf==','?jedaSetelahKoma:huruf=='?'?jedaSetelahTandaTanya:0;
    public float DurasiPengetikan {
        get {float durasi=kalimat.Length/Mathf.Max(1,hurufPerDetik);foreach(char huruf in kalimat)durasi+=JedaTanda(huruf);return durasi;}
    }
    public float DurasiTeks => jedaTeks + DurasiPengetikan + tahanTeksSelesai + durasiTeksMenghilang;
    public int HitungHurufTerlihat(float waktu){
        float batas=0,langkah=1/Mathf.Max(1,hurufPerDetik);
        for(int i=0;i<kalimat.Length;i++){
            batas+=langkah;if(waktu<batas)return i;
            batas+=JedaTanda(kalimat[i]);if(waktu<batas)return i+1;
        }
        return kalimat.Length;
    }
    Vector3 posisiKepala, posisiBadan, posisiKiri, posisiKanan;
    Quaternion rotasiKiri,rotasiKanan;
    float waktuBerjalan, ketukanBerikut;
    public int JumlahKetukan { get; private set; }
    public float WaktuAdegan => waktuBerjalan;
    public int HurufTerlihat { get; private set; }
#if UNITY_EDITOR
    void OnValidate(){UnityEditor.EditorApplication.delayCall+=PratinjauTeksEditor;}
    void PratinjauTeksEditor(){
        if(this==null||Application.isPlaying||teksPikiran==null)return;
        teksPikiran.text=kalimat+"│";teksPikiran.alpha=1;teksPikiran.gameObject.SetActive(tampilkanTeks);
    }
#endif
    void Awake(){
        posisiKepala=kepala.localPosition;posisiBadan=badan.localPosition;
        posisiKiri=tanganKiri.localPosition;posisiKanan=tanganKanan.localPosition;
        rotasiKiri=tanganKiri.localRotation;rotasiKanan=tanganKanan.localRotation;
        Ulangi();
    }
    [ContextMenu("Ulangi contoh mengetik")]
    public void Ulangi(){
        waktuBerjalan=0;ketukanBerikut=0;JumlahKetukan=0;HurufTerlihat=0;
        if(teksPikiran!=null){teksPikiran.text="";teksPikiran.alpha=1;teksPikiran.gameObject.SetActive(tampilkanTeks);}
    }
    void Update(){
        // Loading atau satu frame yang tersendat tidak boleh melewatkan awal kalimat.
        waktuBerjalan+=Mathf.Min(Time.unscaledDeltaTime,.05f);
        float t=WaktuAdegan;AturFrame(t);
        if(t>=ketukanBerikut){
            float jeda=(Mathf.FloorToInt(t*1.2f)%7==6)?.6f:Random.Range(.085f,.18f);
            ketukanBerikut=t+jeda;
            if(suaraKetikan!=null&&suaraKetikan.clip!=null){suaraKetikan.pitch=Random.Range(.92f,1.12f);suaraKetikan.PlayOneShot(suaraKetikan.clip,.58f);JumlahKetukan++;}
        }
    }
    // Bisa dipakai untuk merekam cuplikan pada waktu yang terukur.
    public void AturFrame(float t){
        float napas=Mathf.Sin(t*1.65f)*.008f;
        bool mengetik=Mathf.FloorToInt(t*1.2f)%7!=6;
        float kiri=mengetik?Mathf.Sin(t*34f)*.012f:0;
        float kanan=mengetik?Mathf.Sin(t*34f+Mathf.PI)*.012f:0;
        kepala.localPosition=posisiKepala+Vector3.up*napas;
        badan.localPosition=posisiBadan+Vector3.up*napas*.6f;
        tanganKiri.localPosition=posisiKiri+Vector3.up*kiri;
        tanganKanan.localPosition=posisiKanan+Vector3.up*kanan;
        tanganKiri.localRotation=rotasiKiri*Quaternion.Euler(0,0,kiri*160f);
        tanganKanan.localRotation=rotasiKanan*Quaternion.Euler(0,0,kanan*160f);
        if(tanganKhusus!=null)foreach(var tangan in tanganKhusus)if(tangan!=null)tangan.AturFrame(t);
        if(cahayaLaptop!=null)cahayaLaptop.intensity=1.15f+Mathf.Sin(t*2.2f)*.025f;
        if(teksPikiran!=null){
            teksPikiran.gameObject.SetActive(tampilkanTeks);
            HurufTerlihat=HitungHurufTerlihat(t-jedaTeks);
            string kursor=(Mathf.FloorToInt(t/kedipKursor)%2==0)?"│":"<alpha=#00>│</alpha>";
            teksPikiran.text=t<jedaTeks?"":kalimat.Substring(0,HurufTerlihat)+kursor;
            float selesai=jedaTeks+DurasiPengetikan+tahanTeksSelesai;
            teksPikiran.alpha=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((t-selesai)/Mathf.Max(.1f,durasiTeksMenghilang)));
            if(t>=DurasiTeks)teksPikiran.gameObject.SetActive(false);
        }
    }
    void OnDisable(){if(suaraKetikan!=null)suaraKetikan.Stop();}
}
