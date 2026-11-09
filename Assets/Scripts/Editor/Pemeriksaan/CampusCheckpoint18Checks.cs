using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class CampusCheckpoint18Checks
{
    const string Key="Campus.Revision18.Check";
    static IEnumerator run;static bool started,ticking;static int fps,vsync;
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    [MenuItem("Tools/A Little Campus Story/Revisi 18/Uji tap dan audio")]
    public static void Begin()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode diperlukan.");
        CampusCheckpoint18.Apply();SessionState.SetBool(Key,true);SessionState.SetBool(Key+".BG",Application.runInBackground);
        Application.runInBackground=true;File.WriteAllText("work/audio18/runtime-checks.txt","RUNNING\n");EditorApplication.isPlaying=true;
    }
    static bool Compiled()
    {
        var latest=Directory.GetFiles("Assets/Scripts","*.cs",SearchOption.AllDirectories).Max(File.GetLastWriteTimeUtc);
        return File.GetLastWriteTimeUtc("Library/ScriptAssemblies/Assembly-CSharp-Editor.dll")>=latest;
    }
    static void Tick()
    {
        if(ticking)return;ticking=true;
        try
        {
            if(File.Exists("work/audio18/run.request")&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&Compiled())
            {File.Move("work/audio18/run.request","work/audio18/run.started");Begin();return;}
            if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            EditorApplication.QueuePlayerLoopUpdate();
            if(!started){started=true;results.Clear();errors.Clear();fps=Application.targetFrameRate;vsync=QualitySettings.vSyncCount;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;Application.logMessageReceived+=Log;run=Run();}
            if(!run.MoveNext())Finish(null);
        }
        catch(Exception e){Finish(e);}finally{ticking=false;}
    }
    static void Log(string s,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(s);}
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);results.Add("OK: "+message);}
    static void Finish(Exception e)
    {
        Application.logMessageReceived-=Log;Application.targetFrameRate=fps;QualitySettings.vSyncCount=vsync;
        if(e==null&&errors.Count>0)e=new Exception(string.Join("\n",errors));
        File.WriteAllText("work/audio18/runtime-checks.txt",(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));
        started=false;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;Application.runInBackground=SessionState.GetBool(Key+".BG",false);
    }
    static float peak;
    static IEnumerator Observe(AudioSource src,float seconds)
    {
        peak=0;float until=Time.realtimeSinceStartup+seconds;var sample=new float[256];
        while(Time.realtimeSinceStartup<until){if(src!=null){src.GetOutputData(sample,0);foreach(float x in sample)peak=Mathf.Max(peak,Mathf.Abs(x));}yield return null;}
    }
    static IEnumerator Capture(string name)
    {
        Canvas.ForceUpdateCanvases();ScreenCapture.CaptureScreenshot("work/audio18/"+name+".png");
        int frame=Time.frameCount;while(Time.frameCount<frame+3)yield return null;
    }
    static void Pointer(RhythmArthur g,int i)
    {
        Canvas.ForceUpdateCanvases();var pos=RectTransformUtility.WorldToScreenPoint(null,g.papan.rectTransform.TransformPoint(g.papan.PosisiNot(i)));
        var e=new PointerEventData(EventSystem.current){position=pos,button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
        Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject)==g.papan.gameObject,"Target tap tidak tertutup UI: "+i);
        ExecuteEvents.Execute(g.papan.gameObject,e,ExecuteEvents.pointerDownHandler);
    }
    static IEnumerator Run()
    {
        yield return null;
        var g=UnityEngine.Object.FindFirstObjectByType<RhythmArthur>(FindObjectsInactive.Include);
        var p=g.dialog.arthurScript;var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();DialogManager.Instance=g.dialog;
        foreach(var m in EventSystem.current.GetComponents<BaseInputModule>())m.enabled=false;
        p.GetComponent<LazyBarArthur>().enabled=false;
        var music=MusikArthur.Instance;music.sumber.Stop();music.tema.Stop();
        var wait=Observe(null,.7f);while(wait.MoveNext())yield return null;
        int frame=Time.frameCount;float end=Time.realtimeSinceStartup+.2f;while(Time.realtimeSinceStartup<end)yield return null;Check(Time.frameCount>frame+2,"Player loop benar-benar berjalan");
        var step=p.transform.Find("Langkah Arthur").GetComponent<AudioSource>();
        p.kunciPergerakan=false;p.BerhentiBergerak();int steps=p.JumlahLangkah;p.JalanKeKanan();
        var obs=Observe(step,1.6f);while(obs.MoveNext())yield return null;
        Check(p.JumlahLangkah==steps&&peak<.0001f,"Berjalan sepenuhnya tanpa suara langkah");
        p.BerhentiJalan();end=Time.realtimeSinceStartup+.35f;while(Time.realtimeSinceStartup<end)yield return null;
        p.JalanKeKiri();end=Time.realtimeSinceStartup+.04f;while(Time.realtimeSinceStartup<end)yield return null;p.LepasKiri();
        end=Time.realtimeSinceStartup+.04f;while(Time.realtimeSinceStartup<end)yield return null;p.JalanKeKiri();
        Check(p.SedangBerlari,"Ketuk ganda mengaktifkan lari");obs=Observe(step,1.4f);while(obs.MoveNext())yield return null;
        Check(p.JumlahLangkah>steps&&peak>.01f,"Potongan lari konkret mengeluarkan audio: peak="+peak);p.BerhentiBergerak();
        p.transform.position=new Vector3(0,p.transform.position.y,p.transform.position.z);Physics2D.SyncTransforms();
        int common=BunyiTombolUI.JumlahUmum,tap=BunyiTombolUI.JumlahKetukHP;hp.TekanTombolUIKanan();hp.TekanTombolUIKiri();hp.TekanTombolUIInteract();
        Check(common==BunyiTombolUI.JumlahUmum&&tap==BunyiTombolUI.JumlahKetukHP,"Kontrol dunia tetap tanpa bunyi UI");
        var ui=UnityEngine.Object.FindFirstObjectByType<BunyiTombolUI>();var sources=ui.GetComponents<AudioSource>();
        hp.BukaHPDefault();obs=Observe(sources.Last(),1.1f);while(obs.MoveNext())yield return null;
        Check(hp.hpSedangBuka&&peak>.01f,"Bunyi buka HP benar-benar keluar: peak="+peak);
        hp.TekanTombolUIKanan();obs=Observe(sources.Last(),.18f);while(obs.MoveNext())yield return null;Check(peak>.01f,"Bunyi navigasi HP benar-benar keluar: peak="+peak);
        hp.TekanTombolUIKiri();hp.TekanTombolUIInteract();end=Time.realtimeSinceStartup+1.8f;while(Time.realtimeSinceStartup<end)yield return null;
        if(hp.AplikasiSedangBuka){hp.TutupAplikasi();end=Time.realtimeSinceStartup+1.8f;while(Time.realtimeSinceStartup<end)yield return null;}
        Check(music.playlist.lagu[0].judul=="Jam Pulang - Campus Groove"&&music.playlist.lagu.All(l=>l==null||l.audio==null||!l.audio.name.Contains("Heartbeat")),"Album aktif memakai komposisi baru dan tidak merujuk rekaman Persona");
        Check(music.playlist.lagu.Count(l=>l?.audio!=null)==2&&music.playlist.lagu.All(l=>l?.judul!="Campus Loop"),"Penerapan revisi berulang tidak menduplikasi lagu album");
        music.Pilih(0);music.PutarJeda();obs=Observe(music.sumber,2f);while(obs.MoveNext())yield return null;Check(music.Memutar&&peak>.01f,"Lagu album baru benar-benar diputar: peak="+peak);music.PutarJeda();
        hp.BukaAplikasi("IkonGame");end=Time.realtimeSinceStartup+2f;while(Time.realtimeSinceStartup<end)yield return null;
        Check(g.Status==RhythmArthur.Fase.Siap&&!g.isiMenu.text.Contains("pemindai"),"Instruksi memakai tap lingkaran tanpa garis");
        var capture=Capture("tap-menu");while(capture.MoveNext())yield return null;g.jedaSaatKehilanganFokus=false;
        float prevLife=float.MaxValue,prevGap=float.MaxValue;int prevCount=0;
        for(int level=1;level<=3;level++)
        {
            g.PilihLevel(level);Check(g.DurasiNot<prevLife&&g.JedaNot<prevGap&&g.JumlahNot>prevCount,"Kesulitan meningkat secara monoton pada level "+level);
            prevLife=g.DurasiNot;prevGap=g.JedaNot;prevCount=g.JumlahNot;g.MulaiRonde();
            Check(g.Ronde.durasiNot==g.DurasiNot,"Semua waktu yang terlihat dapat diketuk pada level "+level);
            // A human-style response delay, rather than a bot that only hits an exact beat.
            bool shot=false,paused=false;double limit=EditorApplication.timeSinceStartup+35;
            while(g.Status==RhythmArthur.Fase.Bermain&&EditorApplication.timeSinceStartup<limit)
            {
                if(level==1&&!paused&&g.WaktuLagu>2)
                {
                    paused=true;g.Jeda();double frozen=g.WaktuLagu;end=Time.realtimeSinceStartup+.3f;
                    while(Time.realtimeSinceStartup<end)yield return null;
                    Check(g.Status==RhythmArthur.Fase.Jeda&&Math.Abs(g.WaktuLagu-frozen)<.001,"Jeda membekukan waktu target");g.Lanjutkan();
                }
                for(int i=0;i<g.Ronde.not.Length;i++)if(g.Ronde.hasil[i]==0&&g.WaktuLagu>=g.Ronde.not[i].waktu+.55+(i%3)*.04&&g.papan.Terlihat(i))Pointer(g,i);
                if(!shot&&g.WaktuLagu>3.95){shot=true;capture=Capture("tap-level-"+level);while(capture.MoveNext())yield return null;}
                yield return null;
            }
            Check(g.Status==RhythmArthur.Fase.Menang&&g.Ronde.Miss==0,"Level "+level+" selesai dengan reaksi manusia 550–630 ms, tanpa presisi beat");
        }
        // Boundary and difficulty checks use the actual model with a deliberately slow response.
        int[] misses=new int[3];for(int level=1;level<=3;level++)
        {
            var notes=RhythmArthur.BuatChartLevel(level,112);float life=level==1?2.4f:level==2?1.45f:.85f;
            var model=new ArthurRhythmRound(notes,.09f,.22f,life);
            for(int i=0;i<notes.Length;i++){model.Lewatkan(notes[i].waktu+1);if(model.hasil[i]==0)model.TekanNot(i,notes[i].waktu+1);}
            model.Lewatkan(100);misses[level-1]=model.Miss;
        }
        Check(misses[0]==0&&misses[1]==0&&misses[2]>0,"Reaksi lambat 1 detik diterima Mudah/Normal dan terlambat pada Sulit");
        capture=Capture("tap-result");while(capture.MoveNext())yield return null;g.Tutup();end=Time.realtimeSinceStartup+1.8f;while(Time.realtimeSinceStartup<end)yield return null;hp.TutupHP();
        Check(errors.Count==0,"Tidak ada error runtime selama pengujian tap, HP, musik, dan lari");
    }
}
