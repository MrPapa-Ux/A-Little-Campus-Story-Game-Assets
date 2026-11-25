using System.Collections;
using UnityEngine;

// Isi, ukuran, dan posisi bubble disimpan langsung di objek UI; tidak ditimpa saat Play.
public sealed class BubbleWorshipPembukaArthur : MonoBehaviour
{
    public AdeganRhythmPembukaArthur adegan;
    public CanvasGroup[] bubble;
    [Min(0)] public float tahanHasilMenang=1.5f;
    [Min(.1f)] public float lamaBubble=2f;
    [Min(0)] public float jedaAntarBubble=.5f;
    public bool MomenTerekam1Aktif {get;private set;}
    public int JumlahBubbleTampil {get;private set;}
    public bool Selesai {get;private set;}
    public UnityEngine.Events.UnityEvent setelahWorship = new UnityEngine.Events.UnityEvent();
    [Header("Monolog sesudah WORSHIP — posisi dan tulisan dapat diatur pada bubble")]
    public CanvasGroup monologPagi;
    [Min(1)] public float lamaMonologPagi=3f;
    Coroutine urutan;
    LewatiCutsceneArthur lewati;
    bool dilewati;
    Vector3[] skalaDasar;
    Vector3 SkalaDasar(CanvasGroup b){
        if(skalaDasar==null||skalaDasar.Length!=bubble.Length){
            skalaDasar=new Vector3[bubble.Length];
            for(int i=0;i<bubble.Length;i++)skalaDasar[i]=bubble[i]!=null?bubble[i].transform.localScale:Vector3.one;
        }
        int index=System.Array.IndexOf(bubble,b);return index>=0?skalaDasar[index]:Vector3.one;
    }
    void Awake(){AturUlang();}
    public void AturUlang(){
        if(urutan!=null)StopCoroutine(urutan);
        lewati?.Selesai();dilewati=false;
        urutan=null;MomenTerekam1Aktif=false;JumlahBubbleTampil=0;Selesai=false;
        foreach(var b in bubble)if(b!=null){var skala=SkalaDasar(b);b.alpha=0;b.transform.localScale=skala;}
        if(monologPagi!=null)monologPagi.alpha=0;
    }
    [ContextMenu("Coba dua bubble WORSHIP")]
    public void Mulai(){
        AturUlang();MomenTerekam1Aktif=true;
        lewati=GetComponent<LewatiCutsceneArthur>()??gameObject.AddComponent<LewatiCutsceneArthur>();
        lewati.Mulai(()=>dilewati=true,adegan.pembukaan.teksPikiran!=null?adegan.pembukaan.teksPikiran.font:null,true);
        urutan=StartCoroutine(Urutan());
    }
    IEnumerator Urutan(){
        float tahan=0;while(!dilewati&&tahan<tahanHasilMenang){tahan+=Time.unscaledDeltaTime;yield return null;}
        adegan.rhythm.TutupDariHP();adegan.layarHP.gameObject.SetActive(false);adegan.hpPortrait.gameObject.SetActive(false);
        foreach(var b in bubble){
            dilewati=false;
            if(b==null)continue;
            JumlahBubbleTampil++;
            var skala=SkalaDasar(b);float mulai=Time.unscaledTime,akhir=mulai+lamaBubble;
            while(!dilewati&&Time.unscaledTime<akhir){
                float waktu=Time.unscaledTime-mulai;
                float masuk=Mathf.Clamp01(waktu/.12f),keluar=Mathf.Clamp01((akhir-Time.unscaledTime)/.15f);
                b.alpha=Mathf.SmoothStep(0,1,Mathf.Min(masuk,keluar));b.transform.localScale=skala*Mathf.Lerp(.9f,1,b.alpha);yield return null;
            }
            b.alpha=0;b.transform.localScale=skala;
        }
        if(monologPagi!=null){
            dilewati=false;
            float mulai=Time.unscaledTime;
            while(!dilewati&&Time.unscaledTime-mulai<lamaMonologPagi){
                float t=Time.unscaledTime-mulai;
                monologPagi.alpha=Mathf.SmoothStep(0,1,Mathf.Min(Mathf.Clamp01(t/.2f),Mathf.Clamp01((lamaMonologPagi-t)/.25f)));
                yield return null;
            }
            monologPagi.alpha=0;
        }
        foreach(var b in bubble)if(b!=null){b.alpha=0;b.transform.localScale=SkalaDasar(b);}
        if(monologPagi!=null)monologPagi.alpha=0;
        lewati?.Selesai();Selesai=true;urutan=null;setelahWorship.Invoke();
    }
    IEnumerator Animasi(CanvasGroup b,bool muncul,float durasi){
        float t=0;var skala=SkalaDasar(b);
        while(t<durasi){t+=Time.unscaledDeltaTime;float x=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/durasi));b.alpha=muncul?x:1-x;b.transform.localScale=skala*Mathf.Lerp(muncul?.9f:1,muncul?1:.9f,x);yield return null;}
        b.alpha=muncul?1:0;b.transform.localScale=skala;
    }
    void OnDisable(){lewati?.Selesai();if(urutan!=null){StopCoroutine(urutan);urutan=null;}}
}
