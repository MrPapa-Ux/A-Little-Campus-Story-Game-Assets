#if ALCS_MOD || UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Included in the MOD build only. Saves use its separate Android application identifier.
public sealed class MenuModeUjiArthur : MonoBehaviour
{
    public static MenuModeUjiArthur Instance {get;private set;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Mulai(){if(ModeUjiArthur.Aktif&&Instance==null)new GameObject("MOD Uji Arthur").AddComponent<MenuModeUjiArthur>();}
    readonly List<(string,Action)> aksi=new List<(string,Action)>();
    readonly Queue<string> log=new Queue<string>();
    GameObject panel,isi;
    TMP_Text statistik,pesan,halaman;
    TMP_FontAsset font;Sprite bingkai;
    ArthurJalurKampus arthur;KarakterController pemain;SistemPilihan pilihan;
    int tab,page,errors,frames;float elapsed,fps,slowest,refresh,baseSpeed;bool open,lockedBefore,warping,statsOn=true,noclip;
    string message="MOD terpisah • progres versi biasa tetap terpisah";
    void Awake(){
        if(!ModeUjiArthur.Aktif||Instance!=null){Destroy(gameObject);return;}
        Instance=this;DontDestroyOnLoad(gameObject);
        var cfg=Resources.Load<PengaturanModeUjiArthur>("PengaturanModeUjiArthur");
        if(cfg!=null){font=cfg.font;bingkai=cfg.bingkai;}
        if(font==null)font=Resources.Load<TMP_FontAsset>("Fonts & Materials/Caveat-VariableFont_wght SDF");
        SceneManager.sceneLoaded+=Adegan;Application.logMessageReceived+=Catat;
        BuatUI();Referensi();
    }
    void OnDestroy(){SceneManager.sceneLoaded-=Adegan;Application.logMessageReceived-=Catat;if(Instance==this)Instance=null;}
    void Adegan(Scene scene,LoadSceneMode mode){if(mode!=LoadSceneMode.Single)return;arthur=null;pemain=null;baseSpeed=0;noclip=false;open=false;ModeUjiArthur.MenuTerbuka=false;panel.SetActive(false);Referensi();}
    void Referensi(){
        arthur=FindFirstObjectByType<ArthurJalurKampus>();pemain=arthur!=null?arthur.GetComponent<KarakterController>():null;
        pilihan=FindFirstObjectByType<SistemPilihan>();if(pemain!=null&&baseSpeed==0)baseSpeed=pemain.kecepatanJalan;
    }
    void Catat(string text,string trace,LogType type){if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;errors++;if(log.Count==30)log.Dequeue();log.Enqueue(type+": "+text+"\n"+trace);}
    void Update(){
        frames++;elapsed+=Time.unscaledDeltaTime;slowest=Mathf.Max(slowest,Time.unscaledDeltaTime*1000);
        if(elapsed>=.5f){fps=frames/elapsed;frames=0;elapsed=0;}
        if(Time.unscaledTime<refresh)return;refresh=Time.unscaledTime+.5f;
        if(statistik!=null){statistik.transform.parent.gameObject.SetActive(statsOn);statistik.text=$"MOD  {fps:0} FPS · {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()/1048576} MB · Error {errors}";}
        if(open&&pesan!=null)pesan.text=SceneManager.GetActiveScene().name+" · "+message+" · "+(arthur!=null?$"{KawasanKampus.Instance?.KawasanAktif} / {arthur.JalurAktif?.name}":"");
    }
    RectTransform Kotak(string name,Transform parent,Vector2 pos,Vector2 size){var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=pos;return r;}
    TMP_Text Teks(Transform parent,string text,Vector2 pos,Vector2 size,int fs){var r=Kotak(text,parent,pos,size);var t=r.gameObject.AddComponent<TextMeshProUGUI>();if(font!=null)t.font=font;t.text=text;t.fontSize=fs;t.color=new Color32(40,35,37,255);t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
    void Bingkai(Transform parent,Vector2 size){if(bingkai==null)return;var r=Kotak("Bingkai sketsa",parent,Vector2.zero,size);var im=r.gameObject.AddComponent<UnityEngine.UI.Image>();im.sprite=bingkai;im.type=UnityEngine.UI.Image.Type.Sliced;im.raycastTarget=false;}
    void Tombol(Transform parent,string text,Vector2 pos,Vector2 size,Action action){
        var r=Kotak(text,parent,pos,size);var im=r.gameObject.AddComponent<UnityEngine.UI.Image>();im.color=new Color32(247,221,169,255);Bingkai(r,size);
        var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.onClick.AddListener(()=>{try{action();}catch(Exception e){message=e.Message;Debug.LogException(e);}});Teks(r,text,Vector2.zero,size-new Vector2(16,6),25);
    }
    void BuatUI(){
        var c=gameObject.AddComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=32000;
        var scale=gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();scale.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1280,720);scale.matchWidthOrHeight=.5f;
        gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        var launch=Kotak("Buka MOD",transform,Vector2.zero,new Vector2(108,48));launch.anchorMin=launch.anchorMax=new Vector2(0,1);launch.anchoredPosition=new Vector2(70,-38);
        var im=launch.gameObject.AddComponent<UnityEngine.UI.Image>();im.color=new Color32(247,221,169,240);var b=launch.gameObject.AddComponent<UnityEngine.UI.Button>();b.onClick.AddListener(Toggle);Teks(launch,"MOD",Vector2.zero,new Vector2(100,45),28);
        var statsFrame=Kotak("Statistik",transform,Vector2.zero,new Vector2(740,40));statsFrame.anchorMin=statsFrame.anchorMax=new Vector2(.5f,0);statsFrame.anchoredPosition=new Vector2(0,32);
        var statsBackground=statsFrame.gameObject.AddComponent<UnityEngine.UI.Image>();statsBackground.color=new Color32(40,35,37,220);statsBackground.raycastTarget=false;
        statistik=Teks(statsFrame,"MOD",Vector2.zero,new Vector2(720,32),22);statistik.color=Color.white;
        var p=Kotak("Panel MOD",transform,Vector2.zero,new Vector2(1170,620));panel=p.gameObject;im=panel.AddComponent<UnityEngine.UI.Image>();im.color=new Color32(255,248,230,255);Bingkai(p,new Vector2(1170,620));
        Teks(p,"Catatan Pengujian Arthur",new Vector2(-280,240),new Vector2(550,50),36);
        Tombol(p,"Kembali bermain",new Vector2(425,240),new Vector2(235,46),Toggle);
        string[] labels={"Teleport","Adegan","Bantuan","Minigame"};for(int i=0;i<4;i++){int n=i;Tombol(p,labels[i],new Vector2(-420+i*280,199),new Vector2(257,46),()=>{tab=n;page=0;Segarkan();});}
        isi=Kotak("Pilihan MOD",p,new Vector2(0,-5),new Vector2(1110,330)).gameObject;
        pesan=Teks(p,"",new Vector2(0,-218),new Vector2(1090,32),20);pesan.maxVisibleLines=1;pesan.overflowMode=TextOverflowModes.Ellipsis;
        Tombol(p,"←",new Vector2(-125,-182),new Vector2(70,42),()=>{page=Mathf.Max(0,page-1);Segarkan();});halaman=Teks(p,"",new Vector2(0,-182),new Vector2(160,42),24);
        Tombol(p,"→",new Vector2(125,-182),new Vector2(70,42),()=>{page++;Segarkan();});panel.SetActive(false);
    }
    public void Toggle(){
        if(warping)return;Referensi();open=!open;
        ModeUjiArthur.MenuTerbuka=open;
        if(pemain!=null){pemain.BerhentiBergerak();if(open){lockedBefore=pemain.kunciPergerakan;pemain.kunciPergerakan=true;}else pemain.kunciPergerakan=lockedBefore;}
        panel.SetActive(open);if(open)Segarkan();
    }
    void Jalankan(Action action){if(open)Toggle();action();}
    void Tambah(string label,Action action)=>aksi.Add((label,action));
    void Segarkan(){
        foreach(Transform t in isi.transform)Destroy(t.gameObject);aksi.Clear();Referensi();
        if(tab==0){
            foreach(var r in FindObjectsByType<JalurKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.titik!=null&&r.titik.Length>1).OrderBy(r=>r.name)){var route=r;Tambah(route.name,()=>Jalankan(()=>StartCoroutine(Teleport(route,route.Panjang*.5f))));}
        }else if(tab==1){
            foreach(var name in new[]{"Chapter1_Kamar201","Explorer","MainMenu","Pratinjau Pedang Harapan","Pratinjau Langit Edel"}){var scene=name;Tambah("Buka "+scene,()=>Jalankan(()=>StartCoroutine(GantiScene(scene))));}
            Tambah("Lewati tutorial kamar",()=>Jalankan(()=>{var c=FindFirstObjectByType<CeritaKamar201Arthur>();if(c!=null)c.MulaiPencarian();}));
            Tambah("Adegan makan di Dining",()=>Jalankan(()=>FindFirstObjectByType<PerjalananDiningChapter1>()?.MulaiAdegan()));
            Tambah("Mulai perjalanan Chapel",()=>Jalankan(()=>FindFirstObjectByType<PerjalananChapelChapter1>()?.MulaiPerjalanan()));
            Tambah("Mulai ibadah Rabu",()=>Jalankan(()=>FindFirstObjectByType<PerjalananChapelChapter1>()?.MulaiIbadah()));
            Tambah("Tugas pulang dari ibadah",()=>Jalankan(()=>FindFirstObjectByType<PerjalananChapelChapter1>()?.SiapkanTugasPulang()));
        }else if(tab==2){
            Tambah("Timer: "+(ModeUjiArthur.BekukanTimer?"BEKU":"normal"),()=>{ModeUjiArthur.BekukanTimer=!ModeUjiArthur.BekukanTimer;Segarkan();});
            Tambah("Batas jalan: "+(ModeUjiArthur.AbaikanBatas?"bebas":"normal"),()=>{ModeUjiArthur.AbaikanBatas=!ModeUjiArthur.AbaikanBatas;Segarkan();});
            Tambah("Lewati Quizo: "+ModeUjiArthur.LewatiKuis,()=>{ModeUjiArthur.LewatiKuis=!ModeUjiArthur.LewatiKuis;Segarkan();});
            Tambah("Kecepatan ×1 / ×2 / ×4",()=>{if(pemain!=null){float mul=pemain.kecepatanJalan/baseSpeed;mul=mul<1.5f?2:mul<3?4:1;pemain.kecepatanJalan=baseSpeed*mul;message="Kecepatan ×"+mul;}});
            Tambah("Tembus penghalang: "+noclip,()=>{noclip=!noclip;if(arthur!=null&&arthur.colliderJalur!=null)arthur.colliderJalur.detectCollisions=!noclip;Segarkan();});
            Tambah("Pulihkan bar",()=>Jalankan(()=>pemain?.GetComponent<LazyBarArthur>()?.PulihkanBar()));
            Tambah("Lepaskan kontrol yang tersangkut",()=>Jalankan(()=>{FindFirstObjectByType<SistemHP>()?.TutupHP();pilihan?.TutupPercakapanTerarah();if(pemain!=null){pemain.BerhentiBergerak();pemain.kunciPergerakan=false;}message="Kontrol dipulihkan. Tutup adegan aktif sebelum berpindah.";}));
            Tambah("Berikan hadiah side quest (uji)",()=>{var book=FindFirstObjectByType<BukuArthur>();if(book!=null)foreach(var q in FindObjectsByType<TantanganSampingArthur>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(q=>!string.IsNullOrEmpty(q.hadiah)))book.Tambah(JenisBukuArthur.Tas,new CatatanArthur{id=q.IdHadiah,judul=q.hadiah,ikon=q.ikonHadiah,isi="Item pengujian MOD."},false);message="Hadiah ditambahkan ke tas MOD.";});
            foreach(WaktuKampus time in Enum.GetValues(typeof(WaktuKampus))){var t=time;Tambah("Waktu: "+t,()=>Jalankan(()=>FindFirstObjectByType<SiklusWaktuKampus>()?.AturWaktu(t)));}
            Tambah("Mulai hujan",()=>Jalankan(()=>FindFirstObjectByType<SiklusWaktuKampus>()?.PratinjauHujan(true)));
            Tambah("Akhiri hujan",()=>Jalankan(()=>FindFirstObjectByType<SiklusWaktuKampus>()?.PratinjauHujan(false)));
            Tambah("Tampilkan FPS: "+statsOn,()=>{statsOn=!statsOn;Segarkan();});
            Tambah("Simpan laporan bug",SimpanLaporan);
        }else{
            foreach(var q in FindObjectsByType<TantanganSampingArthur>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(q=>!q.pratinjauSaja&&!q.itemSaja).OrderBy(q=>q.jenis)){var challenge=q;Tambah(q.jenis+" / "+q.namaNpc,()=>Jalankan(()=>challenge.Mulai()));}
        }
        int count=Mathf.Max(1,(aksi.Count+7)/8);page=Mathf.Clamp(page,0,count-1);halaman.text=$"{page+1} / {count}";
        for(int i=0;i<8&&page*8+i<aksi.Count;i++){var a=aksi[page*8+i];Tombol(isi.transform,a.Item1,new Vector2(i%2==0?-275:275,116-(i/2)*78),new Vector2(526,65),a.Item2);}
    }
    IEnumerator GantiScene(string scene){if(KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat)yield break;if(!Application.CanStreamedLevelBeLoaded(scene)){message="Adegan belum ada di build.";yield break;}LoadingManager.sceneTujuan=scene;SutradaraTransisi.sceneTujuanUtama=null;yield return SceneManager.LoadSceneAsync(scene);}
    public IEnumerator Teleport(JalurKampus route,float distance){
        if(warping||route==null||pemain==null||KawasanKampus.Instance==null||KawasanKampus.Instance.SedangMemuat)yield break;
        if(DialogManager.Instance!=null&&DialogManager.Instance.minigameAktif){message="Tutup minigame dahulu.";yield break;}
        warping=true;FindFirstObjectByType<SistemHP>()?.TutupHP();pilihan?.TutupPercakapanTerarah();
        float end=Time.unscaledTime+3;while(DialogManager.Instance!=null&&DialogManager.Instance.PercakapanAktif&&Time.unscaledTime<end)yield return null;
        if(DialogManager.Instance!=null&&DialogManager.Instance.PercakapanAktif){warping=false;yield break;}
        var room=FindFirstObjectByType<CeritaKamar201Arthur>();if(room!=null&&room.Status==CeritaKamar201Arthur.Tahap.Tutorial)room.MulaiPencarian();
        pemain.BerhentiBergerak();bool old=pemain.kunciPergerakan;pemain.kunciPergerakan=true;
        var target=new GameObject("Tujuan MOD sementara");route.Sampel(Mathf.Clamp(distance,0,route.Panjang),out var pos,out _,out _);target.transform.position=pos;target.AddComponent<TujuanJalurKampus>().jalur=route;
        yield return KawasanKampus.Instance.Pindahkan(target.transform,()=>{
            if(pilihan!=null){if(pilihan.grupFisikDepan!=null)pilihan.grupFisikDepan.SetActive(false);if(pilihan.grupFisikBelakang!=null)pilihan.grupFisikBelakang.SetActive(false);if(pilihan.grupFisikRuangan!=null)pilihan.grupFisikRuangan.SetActive(false);}
            arthur.PindahKe(target.transform);if(arthur.colliderJalur!=null)arthur.colliderJalur.detectCollisions=!noclip;
        });
        Destroy(target);if(pemain!=null){pemain.BerhentiBergerak();pemain.kunciPergerakan=old;}warping=false;message=arthur.JalurAktif==route?"Teleport: "+route.name:"Perpindahan gagal; periksa laporan bug.";
    }
    public void SimpanLaporan(){
        Referensi();var record=new Laporan{waktu=DateTime.Now.ToString("s"),scene=SceneManager.GetActiveScene().name,jalur=arthur?.JalurAktif?.name,posisi=arthur!=null?arthur.transform.position:Vector3.zero,fps=fps,frameTerberatMs=slowest,memoriMB=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()/1048576,error=errors,log=log.ToArray()};
        var path=Path.Combine(Application.persistentDataPath,"Laporan-Bug-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".json");File.WriteAllText(path,JsonUtility.ToJson(record,true));message="Laporan tersimpan: "+Path.GetFileName(path);
    }
    [Serializable]sealed class Laporan{public string waktu,scene,jalur;public Vector3 posisi;public float fps,frameTerberatMs;public long memoriMB;public int error;public string[] log;}
}
#endif
