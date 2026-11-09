// PANDUAN PROYEK - Runtime / MiniGame/RapidTap
// Fungsi: Menyimpan percobaan rapid tap dan pintu masuk kompatibilitas menuju rhythm; jangan dianggap satu-satunya pengendali game terbaru.
// Panduan: Assets/Scripts/Runtime/MiniGame/RapidTap/README.md

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Percobaan lokal: tidak mengubah quest, inventaris, atau tujuan scene cerita.
public class RapidTapArthur : MonoBehaviour
{
    [Header("Pintu masuk NPC ke rhythm revisi")]
    public RhythmArthur rhythm;
    public enum Fase { Tutup, Siap, Bermain, Menang, Gagal, Glitch }
    [Header("Aturan percobaan")]
    [Min(1)] public int targetKetukan=20;
    [Min(.5f)] public float durasiDetik=6;
    public bool gunakanGlitchSaatGagal=true;
    [Header("Hasil (opsional untuk integrasi cerita nanti)")]
    public UnityEvent saatMenang=new UnityEvent(),saatGagal=new UnityEvent();
    [Header("Referensi UI")]
    public GameObject panel;
    public TextMeshProUGUI judul,status,waktu,hitungan,godaan,labelKetuk,labelMulai;
    public UnityEngine.UI.Image isiBar;
    public UnityEngine.UI.Button tombolKetuk,tombolMulai,tombolKeluar;
    public DialogManager dialog;
    public LazyBarArthur lazyBar;
    public Fase Status {get;private set;}
    public int Ketukan {get;private set;}
    public float SisaDetik=>Status==Fase.Bermain?Mathf.Max(0,akhir-Time.unscaledTime):durasiRonde;
    float akhir,bolehInput,denyut;
    int targetRonde,tickTerakhir=-1,frameKetuk=-1;
    float durasiRonde;
    bool kunciSebelumnya,kunciDimiliki;
    Vector3 posisiAman;
    Coroutine gagal;
    GameObject glitchAktif;
    static readonly string[] Godaan={"NANTI AJA...","MASIH ADA WAKTU...","SEBENTAR LAGI...","MULAI DULU!"};

    void Awake()
    {
        tombolKetuk.onClick.AddListener(Ketuk);tombolMulai.onClick.AddListener(MulaiRonde);tombolKeluar.onClick.AddListener(Tutup);
        Status=Fase.Tutup;panel.SetActive(false);
    }
    public void Buka()
    {
        if(rhythm!=null){rhythm.Buka();return;}
        if(Status!=Fase.Tutup||dialog==null||dialog.minigameAktif||lazyBar!=null&&lazyBar.waktuHabis)return;
        var hp=FindFirstObjectByType<SistemHP>();if(hp!=null&&hp.hpSedangBuka)return;
        kunciDimiliki=true;dialog.minigameAktif=true;
        if(dialog.arthurScript!=null){kunciSebelumnya=dialog.arthurScript.kunciPergerakan;posisiAman=dialog.arthurScript.transform.position;dialog.arthurScript.BerhentiBergerak();dialog.arthurScript.kunciPergerakan=true;}
        panel.SetActive(true);Status=Fase.Siap;Ketukan=0;isiBar.fillAmount=0;
        judul.text="AYO, MULAI DULU!";status.text="Raka: Satu langkah kecil dulu, Arthur.";
        waktu.text=durasiDetik.ToString("0.#")+" DETIK";hitungan.text=targetKetukan+" KETUKAN";
        godaan.text="Ketuk tombol oranye berulang.\nTahan tombol tidak dihitung.";
        labelKetuk.text="KETUK DI SINI";labelMulai.text="MULAI";
        tombolKetuk.interactable=false;tombolMulai.gameObject.SetActive(true);tombolKeluar.interactable=true;
        bolehInput=Time.unscaledTime+.2f;
    }
    public void MulaiRonde()
    {
        if(Status!=Fase.Siap&&Status!=Fase.Menang&&Status!=Fase.Gagal)return;
        targetRonde=Mathf.Max(1,targetKetukan);durasiRonde=Mathf.Max(.5f,durasiDetik);akhir=Time.unscaledTime+durasiRonde;
        Ketukan=0;frameKetuk=-1;tickTerakhir=-1;isiBar.fillAmount=0;Status=Fase.Bermain;
        judul.text="LAWAN RASA MENUNDA";status.text="Ketuk berulang sampai indikator penuh.";godaan.text=Godaan[0];
        labelKetuk.text="MULAI DULU!";tombolKetuk.interactable=true;tombolMulai.gameObject.SetActive(false);
        hitungan.SetText("{0} / {1}",Ketukan,targetRonde);bolehInput=Time.unscaledTime+.12f;
    }
    void Update()
    {
        if(Status==Fase.Tutup)return;
        denyut=Mathf.MoveTowards(denyut,0,Time.unscaledDeltaTime*4);
        tombolKetuk.transform.localScale=Vector3.one*(1+denyut*.055f);
        if(Input.GetKeyDown(KeyCode.Escape)){Tutup();return;}
        if(Status!=Fase.Bermain)return;
        if(Time.unscaledTime>=akhir){Selesai(false);return;}
        int tick=Mathf.CeilToInt(SisaDetik*10);
        if(tick!=tickTerakhir){tickTerakhir=tick;waktu.text=(tick/10f).ToString("0.0")+" DETIK";}
        if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Return))Ketuk();
    }
    public void Ketuk()
    {
        if(Status!=Fase.Bermain||Time.unscaledTime<bolehInput||frameKetuk==Time.frameCount)return;
        if(Time.unscaledTime>=akhir){Selesai(false);return;}
        frameKetuk=Time.frameCount;Ketukan++;denyut=1;
        isiBar.fillAmount=Ketukan/(float)targetRonde;hitungan.SetText("{0} / {1}",Ketukan,targetRonde);
        godaan.text=Godaan[Mathf.Min(3,Ketukan*4/targetRonde)];
        if(Ketukan>=targetRonde)Selesai(true);
    }
    void Selesai(bool menang)
    {
        Status=menang?Fase.Menang:Fase.Gagal;tombolKetuk.interactable=false;
        judul.text=menang?"CLARITY!":"BELUM KALI INI";
        status.text=menang?"Raka: Tuh, kamu sudah mulai. Lanjutkan!":"Raka: Tarik napas. Kita coba lagi.";
        waktu.text=menang?"BERHASIL":"WAKTU HABIS";godaan.text=menang?"Tidak harus sempurna. Mulai dulu.":"Ini latihan. Kamu boleh mencoba lagi.";
        labelKetuk.text=menang?"SATU LANGKAH MAJU":"COBA LAGI, YUK";labelMulai.text="COBA LAGI";
        tombolMulai.gameObject.SetActive(true);
        if(menang)saatMenang.Invoke();else
        {
            if(dialog.arthurScript!=null){dialog.arthurScript.BerhentiBergerak();dialog.arthurScript.transform.position=posisiAman;}
            saatGagal.Invoke();
            if(gunakanGlitchSaatGagal&&lazyBar!=null&&lazyBar.panelGlitch!=null)gagal=StartCoroutine(GlitchLatihan());
        }
    }
    IEnumerator GlitchLatihan()
    {
        Status=Fase.Glitch;tombolMulai.gameObject.SetActive(false);tombolKeluar.interactable=false;
        glitchAktif=lazyBar.panelGlitch;glitchAktif.SetActive(true);
        var fx=lazyBar.efekGlitchLayar;
        if(fx!=null)fx.Mulai(lazyBar.teksGlitch,lazyBar.kataGlitch);
        var image=glitchAktif.GetComponent<UnityEngine.UI.Image>();
        if(image!=null){image.sprite=null;image.type=UnityEngine.UI.Image.Type.Simple;image.color=new Color(0,0,0,.85f);}
        float mulai=Time.unscaledTime;
        while(Time.unscaledTime-mulai<.65f){if(fx!=null)fx.Tampilkan((Time.unscaledTime-mulai)/.65f);yield return null;}
        BersihkanGlitch();Status=Fase.Gagal;tombolMulai.gameObject.SetActive(true);tombolKeluar.interactable=true;gagal=null;
    }
    void BersihkanGlitch()
    {
        if(glitchAktif==null)return;
        var fx=glitchAktif.GetComponent<EfekGlitchLayar>();if(fx!=null)fx.Gelapkan();glitchAktif.SetActive(false);glitchAktif=null;
    }
    public void Tutup()
    {
        if(rhythm!=null)rhythm.Tutup();
        if(gagal!=null){StopCoroutine(gagal);gagal=null;}BersihkanGlitch();
        Status=Fase.Tutup;if(panel!=null)panel.SetActive(false);
        if(kunciDimiliki&&dialog!=null){dialog.minigameAktif=false;if(dialog.arthurScript!=null)dialog.arthurScript.kunciPergerakan=kunciSebelumnya;}
        kunciDimiliki=false;
    }
    void OnDisable(){Tutup();}
}
