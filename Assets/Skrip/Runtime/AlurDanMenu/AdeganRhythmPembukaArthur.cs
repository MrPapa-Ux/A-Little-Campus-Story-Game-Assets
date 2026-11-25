using UnityEngine;

// Pembukaan HP otomatis, lalu permainan dapat dikendalikan pemain.
[DefaultExecutionOrder(-100)]
public sealed class AdeganRhythmPembukaArthur : MonoBehaviour
{
    public enum Fase { Mengetik, MembukaHP, RondePertama, HasilPertama, RondeKedua, Selesai }
    public PratinjauArthurMengetik pembukaan;
    public RhythmArthur rhythm;
    public CanvasGroup layarHP;
    public RectTransform hpPortrait, bidangLandscape;
    public CanvasGroup kelompokPortrait;
    public UnityEngine.UI.Image layarHitam;
    public GameObject[] ikonHP;
    public GameObject sorotGame;
    public TransformasiLayarHP putarHP;
    public TransisiAplikasiTV efekBukaGame;
    public BubbleWorshipPembukaArthur worship;
    public bool permainanDikendalikanPemain = true;
    public CanvasGroup bubbleIstirahat;
    [Min(.5f)] public float lamaBubbleIstirahat=2.5f;
    [Min(0)] public float lamaMengetik=8;
    [Min(.1f)] public float lamaMembukaHP=.5f;
    [Min(.5f)] public float jedaHasilPertama=2;
    public float durasiMuncul=.3f, jedaLayarHitam=.1f, durasiLayarNyala=.2f, jedaIkon=.8f;
    [Min(.1f)] public float jedaSorotAplikasi=.5f;
    public int IndeksSorotan {get;private set;}=-1;
    public Fase Status {get;private set;}
    public int RondeKalah {get;private set;}
    public int RondeMenang {get;private set;}
    float awal,waktuFase;
    Coroutine urutanBuka;Vector3 posisiHP,skalaHP;Quaternion rotasiHP;bool posisiDisimpan;
    Rect amanLandscape;
    int lebarLandscape, tinggiLandscape;
    LewatiCutsceneArthur lewatiPembuka;
    bool pembukaanDilewati;
    bool lanjutMengetik;
    System.Collections.IEnumerator TungguPembukaan(float durasi){
        float t=0;while(!pembukaanDilewati&&t<durasi){t+=Time.unscaledDeltaTime;yield return null;}
    }
    void Start(){
        var canvas=rhythm.papan.GetComponentInParent<Canvas>();
        if(canvas!=null&&canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>()==null)canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        if(UnityEngine.EventSystems.EventSystem.current==null){var input=new GameObject("Input UI Pembuka",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.EventSystems.StandaloneInputModule));}
        Ulangi();
    }
    [ContextMenu("Ulangi adegan rhythm otomatis")]
    public void Ulangi(){
        if(bubbleIstirahat!=null)bubbleIstirahat.alpha=0;
        IndeksSorotan=-1;
        if(ikonHP!=null)foreach(var ikon in ikonHP){if(ikon==null)continue;var sorot=ikon.transform.Find("TelahTerpilih");if(sorot!=null)sorot.gameObject.SetActive(false);}
        if(worship!=null)worship.AturUlang();
        if(urutanBuka!=null){StopCoroutine(urutanBuka);urutanBuka=null;}
        if(efekBukaGame!=null)efekBukaGame.Batalkan();
        if(!posisiDisimpan){posisiHP=hpPortrait.localPosition;skalaHP=hpPortrait.localScale;rotasiHP=hpPortrait.localRotation;posisiDisimpan=true;}
        hpPortrait.localPosition=posisiHP;hpPortrait.localScale=skalaHP;hpPortrait.localRotation=rotasiHP;hpPortrait.gameObject.SetActive(false);
        rhythm.TutupDariHP();rhythm.tombolJeda.gameObject.SetActive(false);rhythm.tombolKeluar.gameObject.SetActive(false);
        layarHP.alpha=0;layarHP.gameObject.SetActive(false);layarHP.blocksRaycasts=false;layarHP.interactable=false;
        pembukaan.enabled=true;pembukaan.Ulangi();awal=Time.unscaledTime;Status=Fase.Mengetik;RondeKalah=RondeMenang=0;
        pembukaanDilewati=lanjutMengetik=false;lewatiPembuka?.Selesai();
        // Penutup memiliki Skip monolog sendiri dan tidak boleh melewati tantangan diam.
        if(permainanDikendalikanPemain){
            lewatiPembuka=GetComponent<LewatiCutsceneArthur>()??gameObject.AddComponent<LewatiCutsceneArthur>();
            lewatiPembuka.Mulai(()=>{
                if(Status==Fase.Mengetik){if(pembukaan.HurufTerlihat<pembukaan.kalimat.Length)pembukaan.TampilkanKalimatLengkap();else lanjutMengetik=true;}
                else if(Status==Fase.MembukaHP)pembukaanDilewati=true;
            },pembukaan.teksPikiran!=null?pembukaan.teksPikiran.font:null,true);
        }
    }
    public void MainkanRondePenutup(){MulaiRonde(true);}
    void MulaiRonde(bool kedua){
        lewatiPembuka?.Selesai();
        rhythm.NotCepatPembuka = permainanDikendalikanPemain && !kedua;
        rhythm.MulaiAdeganOtomatis();Status=kedua?Fase.RondeKedua:Fase.RondePertama;
        rhythm.papan.raycastTarget=permainanDikendalikanPemain;
        layarHP.blocksRaycasts=permainanDikendalikanPemain;layarHP.interactable=permainanDikendalikanPemain;
        rhythm.labelUtama.gameObject.SetActive(false);
        foreach(var b in rhythm.menu.GetComponentsInChildren<UnityEngine.UI.Button>(true))b.gameObject.SetActive(false);
    }
    void Update(){
        float now=Time.unscaledTime;
        if (Status == Fase.RondePertama || Status == Fase.HasilPertama || Status == Fase.RondeKedua || Status == Fase.Selesai)
            SesuaikanLandscape();
        float durasiPembukaan=pembukaan.tampilkanTeks?Mathf.Max(lamaMengetik,pembukaan.DurasiTeks):lamaMengetik;
        if(Status==Fase.Mengetik&&(lanjutMengetik||pembukaan.WaktuAdegan>=durasiPembukaan)){
            pembukaan.enabled=false;waktuFase=now;Status=Fase.MembukaHP;urutanBuka=StartCoroutine(BukaHPBertahap());
        }
        if(Status==Fase.MembukaHP)return;
        if(Status==Fase.HasilPertama){if(now-waktuFase>=jedaHasilPertama)MulaiRonde(true);return;}
        if(Status!=Fase.RondePertama&&Status!=Fase.RondeKedua)return;
        if(rhythm.Status==RhythmArthur.Fase.Jeda){
            if(!JedaSaatTidakAktif.Dijeda&&(Application.isFocused
#if UNITY_EDITOR
                ||JedaSaatTidakAktif.PengujianLatarBelakang
#endif
                ))rhythm.Lanjutkan();return;
        }
        if(!permainanDikendalikanPemain&&rhythm.Status==RhythmArthur.Fase.Bermain&&rhythm.Ronde!=null){
            double t=rhythm.WaktuLagu;
            for(int i=0;i<rhythm.Ronde.not.Length;i++){
                // Arthur sering meleset pada ronde pertama; ronde kedua mengenai seluruh lingkaran.
                if(Status==Fase.RondePertama&&i%4!=0)continue;
                if(rhythm.Ronde.hasil[i]==0&&t>=rhythm.Ronde.not[i].waktu+.18&&t<rhythm.Ronde.not[i].waktu+rhythm.DurasiNot)rhythm.PukulNot(i);
            }
        }
        if((Status==Fase.RondePertama||Status==Fase.RondeKedua)&&rhythm.Status==RhythmArthur.Fase.Gagal){
            RondeKalah++;Status=Fase.HasilPertama;waktuFase=now;SembunyikanTombolHasil();
        }else if(Status==Fase.RondeKedua&&rhythm.Status==RhythmArthur.Fase.Menang){
            RondeMenang++;Status=Fase.Selesai;SembunyikanTombolHasil();
            if(worship!=null)worship.Mulai();
        }
    }
    System.Collections.IEnumerator BukaHPBertahap(){
        pembukaanDilewati=false;
        if(bubbleIstirahat!=null){
            float muncul=0;while(!pembukaanDilewati&&muncul<.25f){muncul+=Time.unscaledDeltaTime;bubbleIstirahat.alpha=Mathf.SmoothStep(0,1,muncul/.25f);yield return null;}
            yield return TungguPembukaan(lamaBubbleIstirahat);
            float hilang=0;while(!pembukaanDilewati&&hilang<.25f){hilang+=Time.unscaledDeltaTime;bubbleIstirahat.alpha=1-Mathf.SmoothStep(0,1,hilang/.25f);yield return null;}bubbleIstirahat.alpha=0;
        }
        pembukaanDilewati=false;
        hpPortrait.gameObject.SetActive(true);kelompokPortrait.alpha=1;kelompokPortrait.blocksRaycasts=false;kelompokPortrait.interactable=false;
        Canvas.ForceUpdateCanvases();
        Vector3 ukuranPortrait = SesuaikanPortrait();
        foreach(var i in ikonHP)i.SetActive(false);sorotGame.SetActive(false);
        var gelap=layarHitam.color;gelap.a=1;layarHitam.color=gelap;BunyiTombolUI.MainkanBukaHP();
        float t=0;
        while(!pembukaanDilewati&&t<durasiMuncul){t+=Time.unscaledDeltaTime;float x=Mathf.Clamp01(t/Mathf.Max(.01f,durasiMuncul));hpPortrait.localScale=ukuranPortrait*Mathf.SmoothStep(0,1,x);yield return null;}
        hpPortrait.localScale=ukuranPortrait;yield return TungguPembukaan(jedaLayarHitam);
        t=0;while(!pembukaanDilewati&&t<durasiLayarNyala){t+=Time.unscaledDeltaTime;gelap.a=1-Mathf.Clamp01(t/Mathf.Max(.01f,durasiLayarNyala));layarHitam.color=gelap;yield return null;}
        gelap.a=0;layarHitam.color=gelap;foreach(var i in ikonHP)i.SetActive(true);
        pembukaanDilewati=false;
        for(int i=0;i<ikonHP.Length;i++){
            foreach(var ikon in ikonHP){var sorot=ikon.transform.Find("TelahTerpilih");if(sorot!=null)sorot.gameObject.SetActive(false);}
            var aktif=ikonHP[i].transform.Find("TelahTerpilih");if(aktif!=null)aktif.gameObject.SetActive(true);IndeksSorotan=i;
            BunyiTombolUI.MainkanKetukHP();
            float akhir=Time.unscaledTime+jedaSorotAplikasi;while(!pembukaanDilewati&&Time.unscaledTime<akhir)yield return null;
            pembukaanDilewati=false;
            if(aktif!=null&&aktif.gameObject==sorotGame)break;
        }
        foreach(var w in hpPortrait.GetComponentsInChildren<WigglyContainer>(true))w.enabled=false;
        SesuaikanLandscape(true);
        yield return putarHP.KeLandscape(bidangLandscape, pertahankanProporsi: true);
        layarHP.gameObject.SetActive(true);layarHP.alpha=1;
        efekBukaGame.AturBidang(bidangLandscape);efekBukaGame.Buka(()=>MulaiRonde(false));
        while(efekBukaGame.SedangBerjalan)yield return null;urutanBuka=null;
    }
    Vector3 SesuaikanPortrait()
    {
        if (!(hpPortrait.parent is RectTransform parent) || putarHP.badan == null) return skalaHP;
        var badan = putarHP.badan.rectTransform;
        var sudut = new Vector3[4];
        badan.GetWorldCorners(sudut);
        Vector2 min = parent.InverseTransformPoint(sudut[0]);
        Vector2 max = parent.InverseTransformPoint(sudut[2]);
        Vector2 ukuran = max - min;
        if (ukuran.x <= 0 || ukuran.y <= 0) return skalaHP;
        Rect aman = PonselAplikasiBersama.KotakAman(parent);
        // Ukuran badan HP mengacu pada tampilan portrait saat bermain di Kamar 201.
        float skala = Mathf.Min(1, aman.width * .44f / ukuran.x, aman.height * .66f / ukuran.y);
        Vector2 offset = (min + max) * .5f - (Vector2)hpPortrait.localPosition;
        Vector2 pusat = aman.center - offset * skala;
        hpPortrait.localPosition = new Vector3(pusat.x, pusat.y, posisiHP.z);
        return skalaHP * skala;
    }

    void SesuaikanLandscape(bool paksa = false)
    {
        if (!paksa && lebarLandscape == Screen.width && tinggiLandscape == Screen.height && amanLandscape == Screen.safeArea) return;
        lebarLandscape = Screen.width;
        tinggiLandscape = Screen.height;
        amanLandscape = Screen.safeArea;
        PonselAplikasiBersama.PasTengah(bidangLandscape, new Vector2(.9f, .9f));
    }
    void SembunyikanTombolHasil(){foreach(var b in rhythm.menu.GetComponentsInChildren<UnityEngine.UI.Button>(true))b.gameObject.SetActive(false);}
    void OnDisable(){lewatiPembuka?.Selesai();}
}
