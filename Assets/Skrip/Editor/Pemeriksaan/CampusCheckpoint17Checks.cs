using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Tests execute on the live CharacterLab scene in Play Mode; no gameplay state is saved.
public static class CampusCheckpoint17Checks
{
    const string Key="Campus.Checkpoint17.Verify";
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    static bool started;static int oldFrameRate,oldVSync;
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    [MenuItem("Tools/A Little Campus Story/Checkpoint 17/Uji runtime")]
    public static void Begin()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Mulai dari Edit Mode.");
        CampusCheckpoint17.Apply();SessionState.SetBool(Key,true);
        SessionState.SetBool(Key+".Background",Application.runInBackground);Application.runInBackground=true;
        File.WriteAllText("work/audio17/runtime-checks.txt","RUNNING\n");EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        // A one-shot task request lets the user's open Editor run its own tests without UI input.
        if(File.Exists("work/audio17/run-checkpoint.request")&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating)
        {
            File.Move("work/audio17/run-checkpoint.request","work/audio17/run-checkpoint.started");
            try { Begin(); } catch(Exception e) { File.WriteAllText("work/audio17/runtime-checks.txt","FAIL\n"+e); }
            return;
        }
        if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        EditorApplication.QueuePlayerLoopUpdate();
        if(!started){started=true;results.Clear();errors.Clear();Application.logMessageReceived+=Log;stack.Clear();stack.Push(Run());}
        try{while(stack.Count>0){var n=stack.Peek();if(!n.MoveNext()){stack.Pop();continue;}if(n.Current is IEnumerator child){stack.Push(child);continue;}return;}Finish(null);}catch(Exception e){Finish(e);}
    }
    static void Log(string s,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(s);}
    static void Finish(Exception e)
    {
        Application.logMessageReceived-=Log;Application.targetFrameRate=oldFrameRate;QualitySettings.vSyncCount=oldVSync;
        if(e==null&&errors.Count>0)e=new Exception(string.Join("\n",errors));
        File.WriteAllText("work/audio17/runtime-checks.txt",(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));
        SessionState.SetBool(Key,false);started=false;EditorApplication.isPlaying=false;
        Application.runInBackground=SessionState.GetBool(Key+".Background",false);
    }
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);results.Add("OK: "+message);}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
    static float measuredStepPeak;
    static IEnumerator Listen(AudioSource source,float seconds)
    {
        measuredStepPeak=0;var samples=new float[256];float end=Time.realtimeSinceStartup+seconds;
        while(Time.realtimeSinceStartup<end)
        {
            source.GetOutputData(samples,0);
            foreach(float sample in samples)measuredStepPeak=Mathf.Max(measuredStepPeak,Mathf.Abs(sample));
            yield return null;
        }
    }
    static IEnumerator LevelLanjutan(RhythmArthur g,int level,int expected)
    {
        var button=g.menu.transform.Find("PilihLevel"+level).GetComponent<Button>();
        Canvas.ForceUpdateCanvases();var click=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position),button=PointerEventData.InputButton.Left};
        var menuHits=new List<RaycastResult>();EventSystem.current.RaycastAll(click,menuHits);
        Check(menuHits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(menuHits[0].gameObject)==button.gameObject,"Pilihan level dapat disentuh: "+level);
        ExecuteEvents.Execute(button.gameObject,click,ExecuteEvents.pointerClickHandler);
        Check(g.LevelPilihan==level,"Pilihan level mengubah tingkat permainan: "+level);
        g.MulaiRonde();Check(g.Ronde.not.Length==expected,"Jumlah not level "+level+": "+expected);
        double limit=EditorApplication.timeSinceStartup+35;
        while(g.Status==RhythmArthur.Fase.Bermain&&EditorApplication.timeSinceStartup<limit)
        {
            double t=g.WaktuLagu;
            for(int i=0;i<g.Ronde.not.Length;i++)if(g.Ronde.hasil[i]==0&&t>=g.Ronde.not[i].waktu-.02&&t<g.Ronde.not[i].waktu+.15)
            {
                var pos=RectTransformUtility.WorldToScreenPoint(null,g.papan.rectTransform.TransformPoint(g.papan.PosisiNot(i)));
                var e=new PointerEventData(EventSystem.current){position=pos,button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
                Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject)==g.papan.gameObject,"Not level "+level+" dapat disentuh: "+i);
                ExecuteEvents.Execute(g.papan.gameObject,e,ExecuteEvents.pointerDownHandler);
            }
            yield return null;
        }
        Check(g.Status==RhythmArthur.Fase.Menang&&g.Ronde.Terkena==expected,"Level "+level+" selesai lewat sentuhan not");
    }
    static IEnumerator Run()
    {
        oldFrameRate=Application.targetFrameRate;oldVSync=QualitySettings.vSyncCount;
        Application.targetFrameRate=120;QualitySettings.vSyncCount=0;
        yield return null;
        var p=UnityEngine.Object.FindFirstObjectByType<KarakterController>();
        var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();
        var g=UnityEngine.Object.FindFirstObjectByType<RhythmArthur>(FindObjectsInactive.Include);
        Check(p!=null&&hp!=null&&g!=null,"Arthur, HP, dan rhythm tersedia di scene aktif");
        // Bind the test to this scene, rather than a singleton from an interrupted Editor reload.
        Check(g.dialog!=null&&g.dialog.arthurScript!=null,"Referensi dialog berasal dari scene pengujian");
        DialogManager.Instance=g.dialog;
        foreach(var module in EventSystem.current.GetComponents<BaseInputModule>())module.enabled=false;
        var lazy=p.GetComponent<LazyBarArthur>();if(lazy!=null)lazy.enabled=false;
        if(MusikArthur.Instance!=null){MusikArthur.Instance.sumber.Stop();MusikArthur.Instance.tema.Stop();}
        yield return Wait(.7f);
        int frame=Time.frameCount;yield return Wait(.15f);Check(Time.frameCount>frame+2,"Player loop berjalan selama pengujian");
        var step=p.transform.Find("Langkah Arthur").GetComponent<AudioSource>();
        foreach(string name in new[]{"Jalan1","Jalan2","Jalan3","Jalan4","Lari1","Lari2","Lari3","Lari4","KetukHP","BukaHP","Dialog1","Dialog2","Dialog3","CampusLoop","RhythmCampus"})
        {
            var clip=Resources.Load<AudioClip>("ArthurSfx/"+name);Check(clip!=null&&clip.loadState==AudioDataLoadState.Loaded,"Klip dimuat: "+name);
        }
        Check(step.volume>=.6f,"Volume langkah tidak lagi teredam ganda");
        p.kunciPergerakan=false;p.BerhentiBergerak();int count=p.JumlahLangkah;yield return Wait(.25f);
        Check(p.JumlahLangkah==count,"Diam tidak memutar langkah");
        float startX=p.transform.position.x;p.JalanKeKanan();yield return Listen(step,1.6f);
        results.Add("STATE walk x="+p.transform.position.x+" start="+startX+" steps="+p.JumlahLangkah+" contacts="+p.GetComponent<Rigidbody2D>().GetContacts(new ContactPoint2D[12]));
        Check(p.transform.position.x>startX+1&&p.JumlahLangkah>=count+2,"Jalan menghasilkan langkah pada render rate tinggi");
        Check(measuredStepPeak>.01f,"Pemutar langkah jalan benar-benar mengeluarkan sampel audio: peak="+measuredStepPeak);
        p.BerhentiJalan();yield return Wait(.35f);p.JalanKeKiri();yield return Wait(.04f);p.LepasKiri();yield return Wait(.04f);p.JalanKeKiri();
        Check(p.SedangBerlari,"Ketuk ganda mengaktifkan lari");count=p.JumlahLangkah;yield return Listen(step,1.2f);Check(p.JumlahLangkah>=count+2,"Lari menghasilkan langkah dengan irama lebih cepat");
        Check(measuredStepPeak>.01f,"Pemutar langkah lari benar-benar mengeluarkan sampel audio: peak="+measuredStepPeak);
        p.BerhentiBergerak();p.kunciPergerakan=true;count=p.JumlahLangkah;yield return Wait(.3f);Check(p.JumlahLangkah==count,"Penguncian gerak menghentikan langkah");p.kunciPergerakan=false;
        p.transform.position=new Vector3(0,p.transform.position.y,p.transform.position.z);Physics2D.SyncTransforms();yield return Wait(.15f);
        int ui=BunyiTombolUI.JumlahUmum,tap=BunyiTombolUI.JumlahKetukHP;
        foreach(var b in UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(b=>b.name=="Btn_Kiri"||b.name=="Btn_Kanan"||b.name=="Btn_Interact"))b.onClick.Invoke();
        Check(ui==BunyiTombolUI.JumlahUmum&&tap==BunyiTombolUI.JumlahKetukHP,"Kontrol dunia tidak memutar bunyi umum atau bunyi HP");
        if(g.dialog.canvasDialog!=null&&g.dialog.canvasDialog.activeSelf)g.dialog.TutupDialog();
        DialogManager.Instance=g.dialog;
        int opened=BunyiTombolUI.JumlahBukaHP;hp.BukaHPDefault();yield return Wait(1.1f);
        Check(hp.hpSedangBuka&&BunyiTombolUI.JumlahBukaHP==opened+1,"Membuka HP memutar tepat satu bunyi pendek");
        hp.TekanTombolUIKanan();hp.TekanTombolUIKiri();Check(BunyiTombolUI.JumlahKetukHP==tap+2,"Kanan dan kiri berbunyi di dalam HP");
        Check(!g.panel.GetComponentsInChildren<RhythmJalurInput>(true).Any(l=>l.gameObject.activeSelf),"Empat tombol jalur lama dinonaktifkan");
        hp.BukaAplikasi("IkonGame");yield return Wait(2.0f);
        Check(g.Status==RhythmArthur.Fase.Siap&&g.papan.raycastTarget,"Game HP membuka papan not bulat yang menerima sentuhan");
        Check(g.LevelPilihan==1&&g.menu.GetComponentsInChildren<Button>(true).Count(b=>b.name.StartsWith("PilihLevel"))==3,"Tepat tiga level tersedia; Mudah menjadi pilihan awal");
        Check(g.judulMenu.text=="RHYTHM"&&!g.isiMenu.text.Contains("MULAI DULU"),"Menu menggunakan teks formal dan instruksi garis pemindai");
        ScreenCapture.CaptureScreenshot("work/audio17/rhythm-menu.png");yield return Wait(.15f);
        g.jedaSaatKehilanganFokus=false;g.MulaiRonde();yield return Wait(.6f);
        Check(g.Ronde.not.Length==16&&g.TempoRonde==90&&g.Ronde.jendelaGood>=.29f&&g.papan.RadiusNot>50,"Level Mudah memakai 16 not, 90 BPM, target besar, dan waktu ketuk longgar");
        Check(g.audioBeat.isPlaying&&Math.Abs(g.audioBeat.time-g.WaktuLagu)<.18,"Audio adaptasi dan not mengikuti clock DSP yang sama");
        g.Jeda();double paused=g.WaktuLagu;float line=g.papan.Garis;yield return Wait(.25f);
        Check(Math.Abs(g.WaktuLagu-paused)<.001&&Mathf.Abs(g.papan.Garis-line)<.01&&!g.audioBeat.isPlaying,"Jeda membekukan lagu dan garis pemindai bersama");g.Lanjutkan();
        bool screenshot=false,up=false,down=false;float last=g.papan.Garis;double timeout=EditorApplication.timeSinceStartup+35;
        while(g.Status==RhythmArthur.Fase.Bermain&&EditorApplication.timeSinceStartup<timeout)
        {
            float current=g.papan.Garis;up|=current>last+.01f;down|=current<last-.01f;last=current;
            double t=g.WaktuLagu;
            for(int i=0;i<g.Ronde.not.Length;i++)if(g.Ronde.hasil[i]==0&&t>=g.Ronde.not[i].waktu-.02&&t<g.Ronde.not[i].waktu+.15)
            {
                var local=g.papan.PosisiNot(i);
                var position=RectTransformUtility.WorldToScreenPoint(null,g.papan.rectTransform.TransformPoint(local));
                var e=new PointerEventData(EventSystem.current){position=position,button=PointerEventData.InputButton.Left};
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
                Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject)==g.papan.gameObject,"Not menerima sentuhan tanpa tertutup UI: "+i);
                ExecuteEvents.Execute(g.papan.gameObject,e,ExecuteEvents.pointerDownHandler);
            }
            if(!screenshot&&g.Ronde.Terkena>=6){screenshot=true;ScreenCapture.CaptureScreenshot("work/audio17/rhythm-play.png");}
            yield return null;
        }
        Check(up&&down,"Garis pemindai bergerak naik dan turun");
        results.Add("STATE result="+g.Status+" hit="+g.Ronde.Terkena+" miss="+g.Ronde.Miss);
        Check(g.Status==RhythmArthur.Fase.Menang&&g.Ronde.Terkena==g.Ronde.not.Length,"Satu ronde selesai lewat sentuhan koordinat not, bukan tombol jalur");
        yield return LevelLanjutan(g,2,27);
        yield return LevelLanjutan(g,3,42);
        Check(g.judulMenu.text=="SELESAI"&&g.isiMenu.text.Contains("MAX COMBO"),"Hasil menampilkan skor, akurasi, penilaian, dan combo");
        ScreenCapture.CaptureScreenshot("work/audio17/rhythm-result.png");yield return Wait(.15f);
        g.Tutup();yield return Wait(1.7f);hp.TutupHP();yield return Wait(.8f);
        Check(!hp.hpSedangBuka&&!p.kunciPergerakan,"Keluar dari game dan HP mengembalikan kontrol Arthur");
        Check(errors.Count==0,"Tidak ada error runtime selama langkah, HP, dan ronde rhythm");
    }
}
