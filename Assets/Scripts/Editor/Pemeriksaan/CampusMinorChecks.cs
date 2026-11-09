using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class CampusMinorChecks
{
    const string Dir="work/minor-tangga-taptiptup/", Key="Campus.MinorTangga";
    static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    static double until;
    static ArthurJalurKampus route;
    static KarakterController arthur;
    static PijakanTanggaArthur fix;
    static int frames, checkedFrames;
    static float deepest;
    static readonly List<string> errors=new List<string>();
    static CampusMinorChecks(){EditorApplication.update+=Tick;}
    static void Log(string s)=>File.AppendAllText(Dir+"pengujian.txt",s+"\n");
    static void Check(bool ok,string s){if(!ok)throw new Exception(s);Log("PASS: "+s);}
    static void Tick()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        try
        {
            if(File.Exists(Dir+"uji.request"))
            {
                File.Delete(Dir+"uji.request");
                if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Perlu Edit Mode tersimpan.");
                SessionState.SetBool(Key+"mute",EditorUtility.audioMasterMute);SessionState.SetBool(Key+"background",Application.runInBackground);
                EditorUtility.audioMasterMute=true;SessionState.SetBool(Key,true);SessionState.SetBool(Key+"done",false);
                File.WriteAllText(Dir+"pengujian.txt","Uji pijakan tangga dan TapTIpTup\n");EditorApplication.isPlaying=true;return;
            }
            if(!SessionState.GetBool(Key,false))return;
            if(!EditorApplication.isPlaying)
            {
                if(EditorApplication.isPlayingOrWillChangePlaymode)return;
                Camera.onPreCull-=Observe;RenderPipelineManager.beginCameraRendering-=ObserveSRP;Application.logMessageReceived-=Error;
                EditorUtility.audioMasterMute=SessionState.GetBool(Key+"mute",false);Application.runInBackground=SessionState.GetBool(Key+"background",false);
                JedaSaatTidakAktif.PengujianLatarBelakang=false;SessionState.SetBool(Key,false);steps.Clear();Log("SELESAI: Play mati, pengaturan sementara dipulihkan.");return;
            }
            JedaSaatTidakAktif.PengujianLatarBelakang=true;Application.runInBackground=true;EditorApplication.isPaused=false;
            var pause=UnityEngine.Object.FindFirstObjectByType<JedaSaatTidakAktif>();if(pause!=null)pause.AturJeda(false);Time.timeScale=1;
            if(SessionState.GetBool(Key+"done",false)||Time.timeSinceLevelLoad<1)return;
            if(steps.Count==0){steps.Push(Run());until=0;}
            if(EditorApplication.timeSinceStartup<until)return;
            var e=steps.Peek();if(!e.MoveNext()){steps.Pop();if(steps.Count==0){SessionState.SetBool(Key+"done",true);EditorApplication.isPlaying=false;}return;}
            if(e.Current is IEnumerator child)steps.Push(child);else if(e.Current is float t)until=EditorApplication.timeSinceStartup+t;
        }
        catch(Exception e){Log("GAGAL: "+e);SessionState.SetBool(Key+"done",true);if(EditorApplication.isPlaying)EditorApplication.isPlaying=false;steps.Clear();}
    }
    static void Error(string s,string stack,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(s);}
    static void ObserveSRP(ScriptableRenderContext context,Camera c)=>Observe(c);
    static void Observe(Camera c)
    {
        if(route==null||c!=route.kamera||!fix.enabled)return;frames++;
        if(route.JalurAktif==null)return;
        foreach(var z in fix.zona)
        {
            if(z.jalur!=route.JalurAktif||route.JarakDiJalur<z.awal||route.JarakDiJalur>z.akhir)continue;
            checkedFrames++;
            foreach(var foot in new[]{fix.kakiKiri,fix.kakiKanan})
            {
                var b=foot.sprite.bounds;
                for(int i=0;i<=8;i++)
                {
                    var p=foot.transform.TransformPoint(new Vector3(Mathf.Lerp(b.min.x,b.max.x,i/8f),b.min.y,b.center.z));
                    foreach(var t in z.tapak)if(t.Tinggi(p,out float y))deepest=Mathf.Max(deepest,y-p.y);
                }
            }
        }
    }
    static void Place(JalurKampus r,float d)
    {
        arthur.BerhentiBergerak();r.Sampel(d,out var p,out _,out _);
        var g=new GameObject("TujuanUjiSementara");g.transform.position=p;g.AddComponent<TujuanJalurKampus>().jalur=r;route.PindahKe(g.transform);UnityEngine.Object.Destroy(g);
    }
    static IEnumerator Walk(float target)
    {
        float end=Time.realtimeSinceStartup+18;arthur.kecepatanJalan=6;
        while(Mathf.Abs(route.JarakDiJalur-target)>.08f)
        {
            if(Time.realtimeSinceStartup>end)throw new Exception("Gerak terhambat "+route.JalurAktif.name+" "+route.JarakDiJalur+" -> "+target);
            route.JalurAktif.Sampel(route.JarakDiJalur,out _,out var d,out _);
            float input=Mathf.Sign(target-route.JarakDiJalur)*(Vector3.Dot(d,route.kamera.transform.right)>=0?1:-1);
            if(input>0)arthur.JalanKeKanan();else arthur.JalanKeKiri();yield return null;
        }
        arthur.BerhentiBergerak();yield return .2f;
    }
    static IEnumerator Wait(Func<bool> ready,string note)
    {
        float end=Time.realtimeSinceStartup+8;
        while(!ready()){if(Time.realtimeSinceStartup>end)throw new Exception("Batas tunggu: "+note);yield return null;}
    }
    static IEnumerator Run()
    {
        route=UnityEngine.Object.FindFirstObjectByType<ArthurJalurKampus>();arthur=route.GetComponent<KarakterController>();fix=arthur.GetComponent<PijakanTanggaArthur>();
        UnityEngine.Object.FindFirstObjectByType<LazyBarArthur>().tanpaBatasWaktu=true;
        frames=checkedFrames=0;deepest=0;errors.Clear();Application.logMessageReceived+=Error;
        Camera.onPreCull+=Observe;RenderPipelineManager.beginCameraRendering+=ObserveSRP;
        int initial=Time.frameCount;yield return .3f;Check(Time.frameCount>initial+2,"Frame game benar-benar berjalan");
        var back=fix.zona.First().jalur;
        Place(back,back.JarakTerdekat(new Vector3(14.55f,.95533f,-104.4685f)));fix.enabled=false;yield return .5f;
        ScreenCapture.CaptureScreenshot(Dir+"tangga-sebelum.png");yield return .2f;
        fix.enabled=true;yield return .3f;ScreenCapture.CaptureScreenshot(Dir+"tangga-sesudah.png");yield return .2f;
        foreach(var z in fix.zona)
        {
            Place(z.jalur,z.awal-.25f);yield return .25f;
            yield return Walk(z.akhir+.25f);yield return Walk(z.awal-.25f);
            Check(true,"Tangga dua arah tanpa macet: "+z.jalur.name+" @ "+z.awal.ToString("F2"));
        }
        Check(checkedFrames>50&&deepest<.005f,"Sepatu tidak tenggelam pada tapak asli selama "+checkedFrames+" frame tangga (penetrasi="+deepest+")");
        var g=UnityEngine.Object.FindFirstObjectByType<RhythmArthur>(FindObjectsInactive.Include);route.PindahKe(GameObject.Find("RuteKampus_CharacterLab/TitikKembaliJasmine_DariKantin").transform);yield return .3f;
        var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();hp.BukaHPDefault();
        yield return Wait(()=>hp.daftarIkon.Any(i=>i!=null&&i.name=="IkonGame"&&i.activeInHierarchy),"animasi menu HP");
        yield return .25f;hp.BukaAplikasi("IkonGame");yield return Wait(()=>g.Status==RhythmArthur.Fase.Siap,"game di HP");yield return .4f;
        Check(g.Status==RhythmArthur.Fase.Siap,"Game terbuka melalui ikon HP");
        var titles=g.panel.GetComponentsInChildren<TMPro.TMP_Text>(true).Where(t=>t.name=="JudulGame"||t==g.judulMenu).ToArray();
        foreach(var t in titles){t.ForceMeshUpdate(true);Check(t.text=="TapTIpTup"&&t.font.name=="AmaticSC-Regular SDF"&&!t.isTextOverflowing,"Judul dan font lama pas: "+t.name);}
        ScreenCapture.CaptureScreenshot(Dir+"TapTIpTup-menu.png");yield return .2f;
        g.PilihLevel(2);Check(g.judulMenu.text=="TapTIpTup","Judul tetap sama saat ganti level");g.PilihLevel(1);g.Tutup();
        yield return Wait(()=>!hp.AplikasiSedangBuka,"tutup aplikasi");yield return .3f;hp.TutupHP();yield return .4f;
        Check(!g.dialog.minigameAktif&&!arthur.kunciPergerakan,"Keluar memulihkan kendali Arthur");
        Check(errors.Count==0,"Tidak ada error runtime baru");Log("LULUS: pijakan visual tangga, gerak dua arah, judul dan font TapTIpTup, serta keluar dari HP.");
    }
}
