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
    }
    public void MainkanRondePenutup(){MulaiRonde(true);}
    void MulaiRonde(bool kedua){
        rhythm.NotCepatPembuka = permainanDikendalikanPemain && !kedua;
        rhythm.MulaiAdeganOtomatis();Status=kedua?Fase.RondeKedua:Fase.RondePertama;
        rhythm.papan.raycastTarget=permainanDikendalikanPemain;
        layarHP.blocksRaycasts=permainanDikendalikanPemain;layarHP.interactable=permainanDikendalikanPemain;
        rhythm.labelUtama.gameObject.SetActive(false);
        foreach(var b in rhythm.menu.GetComponentsInChildren<UnityEngine.UI.Button>(true))b.gameObject.SetActive(false);
    }
    void Update(){
        float now=Time.unscaledTime;
        float durasiPembukaan=pembukaan.tampilkanTeks?Mathf.Max(lamaMengetik,pembukaan.DurasiTeks):lamaMengetik;
        if(Status==Fase.Mengetik&&pembukaan.WaktuAdegan>=durasiPembukaan){
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
        if(bubbleIstirahat!=null){
            float muncul=0;while(muncul<.25f){muncul+=Time.unscaledDeltaTime;bubbleIstirahat.alpha=Mathf.SmoothStep(0,1,muncul/.25f);yield return null;}
            yield return new WaitForSecondsRealtime(lamaBubbleIstirahat);
            float hilang=0;while(hilang<.25f){hilang+=Time.unscaledDeltaTime;bubbleIstirahat.alpha=1-Mathf.SmoothStep(0,1,hilang/.25f);yield return null;}bubbleIstirahat.alpha=0;
        }
        hpPortrait.gameObject.SetActive(true);kelompokPortrait.alpha=1;kelompokPortrait.blocksRaycasts=false;kelompokPortrait.interactable=false;
        foreach(var i in ikonHP)i.SetActive(false);sorotGame.SetActive(false);
        var gelap=layarHitam.color;gelap.a=1;layarHitam.color=gelap;BunyiTombolUI.MainkanBukaHP();
        float t=0;
        while(t<durasiMuncul){t+=Time.unscaledDeltaTime;float x=Mathf.Clamp01(t/Mathf.Max(.01f,durasiMuncul));float ease=1+2.70158f*Mathf.Pow(x-1,3)+1.70158f*Mathf.Pow(x-1,2);hpPortrait.localScale=skalaHP*ease;yield return null;}
        hpPortrait.localScale=skalaHP;yield return new WaitForSecondsRealtime(jedaLayarHitam);
        t=0;while(t<durasiLayarNyala){t+=Time.unscaledDeltaTime;gelap.a=1-Mathf.Clamp01(t/Mathf.Max(.01f,durasiLayarNyala));layarHitam.color=gelap;yield return null;}
        gelap.a=0;layarHitam.color=gelap;foreach(var i in ikonHP)i.SetActive(true);
        float awalSorotan=Time.unscaledTime;
        for(int i=0;i<ikonHP.Length;i++){
            foreach(var ikon in ikonHP){var sorot=ikon.transform.Find("TelahTerpilih");if(sorot!=null)sorot.gameObject.SetActive(false);}
            var aktif=ikonHP[i].transform.Find("TelahTerpilih");if(aktif!=null)aktif.gameObject.SetActive(true);IndeksSorotan=i;
            BunyiTombolUI.MainkanKetukHP();
            float akhir=awalSorotan+(i+1)*jedaSorotAplikasi;while(Time.unscaledTime<akhir)yield return null;
            if(aktif!=null&&aktif.gameObject==sorotGame)break;
        }
        foreach(var w in hpPortrait.GetComponentsInChildren<WigglyContainer>(true))w.enabled=false;
        yield return putarHP.KeLandscape(bidangLandscape);
        layarHP.gameObject.SetActive(true);layarHP.alpha=1;layarHP.transform.localScale=Vector3.one;
        efekBukaGame.AturBidang(bidangLandscape);efekBukaGame.Buka(()=>MulaiRonde(false));
        while(efekBukaGame.SedangBerjalan)yield return null;urutanBuka=null;
    }
    void SembunyikanTombolHasil(){foreach(var b in rhythm.menu.GetComponentsInChildren<UnityEngine.UI.Button>(true))b.gameObject.SetActive(false);}
}
