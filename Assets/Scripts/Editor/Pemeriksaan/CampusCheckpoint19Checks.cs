using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class CampusCheckpoint19Checks
{
    public const string Key="Campus.Revision19.Check";
    static IEnumerator running;static bool busy,started;static int fps,vsync,oldSize=-1;
    static EditorWindow view;static PropertyInfo selected;static object group;
    static readonly List<int> customSizes=new List<int>();
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    static void Tick()
    {
        if(busy)return;busy=true;
        try
        {
            if(File.Exists("work/bug19/run.request")&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&Compiled())
            {
                File.Delete("work/bug19/run.request");CampusCheckpoint19.Apply();CampusCheckpoint19.Apply();
                SessionState.SetBool(Key+".BGConfigured",!PlayerSettings.runInBackground);
                SessionState.SetBool(Key,true);File.WriteAllText("work/bug19/runtime-checks.txt","RUNNING\n");EditorApplication.isPlaying=true;return;
            }
            if(!SessionState.GetBool(Key,false)||!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            EditorApplication.QueuePlayerLoopUpdate();
            if(!started)
            {
                started=true;results.Clear();errors.Clear();Application.logMessageReceived+=Log;
                fps=Application.targetFrameRate;vsync=QualitySettings.vSyncCount;Application.targetFrameRate=60;QualitySettings.vSyncCount=0;
                JedaSaatTidakAktif.PengujianLatarBelakang=true;Application.runInBackground=true;
                stack.Clear();running=Run();stack.Push(running);
            }
            while(stack.Count>0)
            {
                var item=stack.Peek();if(!item.MoveNext()){stack.Pop();continue;}
                if(item.Current is IEnumerator child){stack.Push(child);continue;}return;
            }
            Finish(null);
        }
        catch(Exception e){Finish(e);}finally{busy=false;}
    }
    static bool Compiled()=>File.GetLastWriteTimeUtc("Library/ScriptAssemblies/Assembly-CSharp-Editor.dll")>=Directory.GetFiles("Assets/Scripts","*.cs",SearchOption.AllDirectories).Max(File.GetLastWriteTimeUtc);
    static void Log(string s,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(s);}
    static void Check(bool ok,string s){if(!ok)throw new Exception(s);results.Add("OK: "+s);File.WriteAllText("work/bug19/progress.txt",string.Join("\n",results));}
    static void Finish(Exception error)
    {
        Application.logMessageReceived-=Log;
        if(started){Application.targetFrameRate=fps;QualitySettings.vSyncCount=vsync;}
        if(error==null&&errors.Count>0)error=new Exception(string.Join("\n",errors));
        try{RestoreView();}catch(Exception e){if(error==null)error=e;}
        File.WriteAllText("work/bug19/runtime-checks.txt",(error==null?"PASS":"FAIL\n"+error)+"\n"+string.Join("\n",results));
        Application.runInBackground=false;EditorApplication.isPaused=false;EditorApplication.isPlaying=false;SessionState.SetBool(Key,false);started=false;
    }
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
    static IEnumerator Capture(string name){ScreenCapture.CaptureScreenshot("work/bug19/"+name+".png");int f=Time.frameCount;while(Time.frameCount<f+3)yield return null;}
    static void InitView()
    {
        var asm=typeof(Editor).Assembly;var type=asm.GetType("UnityEditor.GameView");
        view=Resources.FindObjectsOfTypeAll<EditorWindow>().First(w=>w.GetType()==type);
        selected=type.GetProperty("selectedSizeIndex",Flags);oldSize=(int)selected.GetValue(view);
        var sizesType=asm.GetType("UnityEditor.GameViewSizes");var instance=sizesType.BaseType.GetProperty("instance",Flags).GetValue(null);
        group=sizesType.GetMethod("GetGroup",Flags).Invoke(instance,new object[]{0});
    }
    static void View(int width,int height)
    {
        var asm=typeof(Editor).Assembly;var type=asm.GetType("UnityEditor.GameViewSize");var enumType=asm.GetType("UnityEditor.GameViewSizeType");
        var size=Activator.CreateInstance(type,Flags,null,new object[]{Enum.ToObject(enumType,1),width,height,"Revisi19 "+width+"x"+height},null);
        int index=(int)group.GetType().GetMethod("GetTotalCount",Flags).Invoke(group,null);
        group.GetType().GetMethod("AddCustomSize",Flags).Invoke(group,new[]{size});customSizes.Add(index);
        selected.SetValue(view,index);view.Repaint();
    }
    static void RestoreView()
    {
        if(view==null||oldSize<0)return;selected.SetValue(view,oldSize);view.Repaint();
        int builtins=(int)group.GetType().GetMethod("GetBuiltinCount",Flags).Invoke(group,null);
        for(int i=customSizes.Count-1;i>=0;i--)group.GetType().GetMethod("RemoveCustomSize",Flags).Invoke(group,new object[]{customSizes[i]-builtins});
        customSizes.Clear();oldSize=-1;
    }
    static void Fits(RectTransform rect,string name)
    {
        var c=new Vector3[4];rect.GetWorldCorners(c);var canvas=rect.GetComponentInParent<Canvas>().rootCanvas;
        var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        Check(rect.rect.width>0&&rect.rect.height>0&&c.All(v=>{var p=RectTransformUtility.WorldToScreenPoint(camera,v);return p.x>=-2&&p.x<=Screen.width+2&&p.y>=-2&&p.y<=Screen.height+2;}),name+" utuh pada "+Screen.width+"x"+Screen.height);
    }
    static void Glyphs(TMP_Text label)
    {
        var info=label.textInfo;var fx=label.GetComponent<WigglyText>();float limit=fx!=null?fx.kekuatanGoyang*1.6f+1:1;
        foreach(var c in info.characterInfo.Take(info.characterCount).Where(c=>c.isVisible))
        {
            var vertices=info.meshInfo[c.materialReferenceIndex].vertices;var expected=new[]{c.bottomLeft,c.topLeft,c.topRight,c.bottomRight};
            for(int j=0;j<4;j++)if(Vector3.Distance(vertices[c.vertexIndex+j],expected[j])>limit)throw new Exception("Vertex teks keluar dari glyph setelah resize: "+label.name);
        }
    }
    static IEnumerator Voice(SuaraDialogKertas voice,string name)
    {
        int before=voice.JumlahBunyi;float peak=0,end=Time.realtimeSinceStartup+.7f,next=0;var data=new float[256];
        while(Time.realtimeSinceStartup<end)
        {
            if(Time.realtimeSinceStartup>next){voice.Huruf('a');next=Time.realtimeSinceStartup+.08f;}
            voice.sumber.GetOutputData(data,0);foreach(float x in data)peak=Mathf.Max(peak,Mathf.Abs(x));yield return null;
        }
        voice.Hentikan();Check(voice.JumlahBunyi>before+3&&peak>.005f,"Suara teks "+name+" keluar per huruf: "+peak);
    }
    static IEnumerator Run()
    {
        yield return null;
        var focus=UnityEngine.Object.FindFirstObjectByType<JedaSaatTidakAktif>();focus.AturJeda(false);
        var d=UnityEngine.Object.FindFirstObjectByType<DialogManager>();var p=d.arthurScript;var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();
        var lazy=p.GetComponent<LazyBarArthur>();lazy.enabled=false;var system=d.GetComponent<SistemPilihan>();var music=MusikArthur.Instance;
        foreach(var module in EventSystem.current.GetComponents<BaseInputModule>())module.enabled=false;
        yield return Wait(.7f);Check(DialogManager.Instance==d,"Interaksi menggunakan DialogManager scene yang sebenarnya");
        Check(SessionState.GetBool(Key+".BGConfigured",false),"Run In Background proyek dimatikan di luar pengujian");
        InitView();
        var lyra=GameObject.Find("NPC_Lyra");var area=lyra.GetComponent<AreaInteraksi>();
        p.transform.position=new Vector3(lyra.transform.position.x-.8f,p.transform.position.y,p.transform.position.z);Physics2D.SyncTransforms();yield return Wait(.4f);
        Check(d.bisaInteraksi&&d.idInteraksiAktif==area.idTempat,"Trigger fisik Lyra mengirim skenario dialog");
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1024,768),new Vector2Int(1600,720)})
        {
            View(size.x,size.y);yield return Wait(.5f);Canvas.ForceUpdateCanvases();
            Check(Screen.width==size.x&&Screen.height==size.y,"Game View benar-benar berubah ukuran");
            system.AksiTombolInteraksi();yield return Wait(.7f);
            Check(d.canvasDialog.activeInHierarchy&&d.canvasDialog.GetComponent<CanvasGroup>().alpha>.99f&&system.teksCeritaUtama.text.Length>0,"Canvas dialog Lyra dan teks tampil");
            Fits((RectTransform)d.canvasDialog.transform.Find("PanelKotakDialog"),"Panel dialog");
            Glyphs(system.teksCeritaUtama);yield return Capture("lyra-"+size.x+"x"+size.y);
            d.TutupDialog();yield return Wait(.4f);
            foreach(var button in new[]{"Btn_Kiri","Btn_Kanan","Btn_Interact","Btn_HandPhone"})
                Fits((RectTransform)Resources.FindObjectsOfTypeAll<UnityEngine.UI.Button>().First(b=>b.gameObject.scene==d.gameObject.scene&&b.name==button).transform,button);
        }
        yield return Voice(lyra.GetComponent<SuaraDialogKertas>(),"Lyra");yield return Voice(p.GetComponent<SuaraDialogKertas>(),"Arthur");
        Check(lyra.GetComponent<SuaraDialogKertas>().profil.potongan.All(c=>c.name.StartsWith("DialogCewek"))&&p.GetComponent<SuaraDialogKertas>().profil.potongan.All(c=>c.name.StartsWith("DialogCowok")),"Register suara perempuan/laki-laki terpisah");
        hp.BukaHPDefault();yield return Wait(1);hp.BukaAplikasi("IkonMusic");yield return Wait(1.5f);music.Pilih(0);music.PutarJeda();
        foreach(var size in new[]{new Vector2Int(1024,768),new Vector2Int(1600,720),new Vector2Int(1280,720)})
        {
            View(size.x,size.y);yield return Wait(.8f);hp.ponselBersama.Sesuaikan();yield return Wait(.2f);
            Fits((RectTransform)hp.ponselBersama.transform,"Bingkai musik HP");
            foreach(var text in hp.musik.GetComponentsInChildren<TMP_Text>())Glyphs(text);
            Check(music.Memutar,"Musik terus normal saat ukuran layar berubah");yield return Capture("music-"+size.x+"x"+size.y);
        }
        hp.TutupAplikasi();yield return Wait(1.5f);hp.TutupHP();yield return Wait(.7f);
        p.kunciPergerakan=false;p.BerhentiBergerak();int steps=p.JumlahLangkah;p.JalanKeKanan();yield return Wait(1.6f);
        Check(p.JumlahLangkah==steps,"Berjalan tetap senyap");p.BerhentiBergerak();
        p.JalanKeKiri();yield return Wait(.04f);p.LepasKiri();yield return Wait(.04f);p.JalanKeKiri();
        Check(p.SedangBerlari,"Lari aktif melalui ketuk ganda");
        var runClip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Referensi/Rayman_Reference/ArthurWalk.anim");var contacts=AnimationUtility.GetAnimationEvents(runClip);
        Check(contacts.Length==8,"Delapan kontak pijakan pada animasi lari 4,24 detik");
        float until=Time.realtimeSinceStartup+3.2f;int last=p.JumlahLangkah;float peakRun=0;var samples=new float[256];var source=p.transform.Find("Langkah Arthur").GetComponent<AudioSource>();
        while(Time.realtimeSinceStartup<until)
        {
            if(p.JumlahLangkah>last)
            {
                float t=Mathf.Repeat(p.animator.GetCurrentAnimatorStateInfo(0).normalizedTime,1)*runClip.length;
                Check(contacts.Any(e=>Mathf.Min(Mathf.Abs(e.time-t),runClip.length-Mathf.Abs(e.time-t))<.14f),"Bunyi lari mengikuti frame kaki menapak");last=p.JumlahLangkah;
            }
            source.GetOutputData(samples,0);foreach(float x in samples)peakRun=Mathf.Max(peakRun,Mathf.Abs(x));yield return null;
        }
        Check(p.JumlahLangkah-steps>=4&&p.JumlahLangkah-steps<=8&&peakRun>.01f,"Irama lari sesuai animasi dan audio keluar");p.BerhentiBergerak();
        float songTime=music.sumber.time,fill=lazy.lazyBarFill.fillAmount,volume=AudioListener.volume;
        JedaSaatTidakAktif.PengujianLatarBelakang=false;focus.SendMessage("OnApplicationFocus",false);
        songTime=music.sumber.time;
        Check(Time.timeScale==0&&AudioListener.pause,"Kehilangan fokus membekukan simulasi dan audio");yield return Wait(.4f);
        Check(Mathf.Abs(music.sumber.time-songTime)<.03f&&Mathf.Abs(lazy.lazyBarFill.fillAmount-fill)<.001f,"Musik dan bar tidak maju saat tidak fokus (delta="+(music.sumber.time-songTime)+")");
        focus.SendMessage("OnApplicationFocus",true);JedaSaatTidakAktif.PengujianLatarBelakang=true;yield return Wait(.2f);
        Check(Time.timeScale==1&&!AudioListener.pause&&Mathf.Approximately(AudioListener.volume,volume)&&music.sumber.time>songTime+.05f,"Kembali fokus memulihkan waktu/audio tanpa mengubah volume");
        JedaEditorSaatTidakAktif.Terapkan(false);int frame=Time.frameCount;songTime=music.sumber.time;yield return Wait(.3f);
        Check(EditorApplication.isPaused&&Time.frameCount<=frame+1&&Mathf.Abs(music.sumber.time-songTime)<.03f,"Jeda Editor menghentikan player loop serta audio saat pindah tab");
        JedaEditorSaatTidakAktif.Terapkan(true);yield return Wait(.1f);Check(!EditorApplication.isPaused&&Time.timeScale==1&&!AudioListener.pause,"Player loop Editor pulih setelah fokus kembali");
        lazy.durasiSpam=10;lazy.jedaSebelumPindahScene=10;
        typeof(LazyBarArthur).GetField("sisaWaktu",Flags).SetValue(lazy,.01f);lazy.enabled=true;yield return Wait(.7f);
        Check(lazy.waktuHabis&&!music.Memutar&&!music.tema.isPlaying,"Bar habis menghentikan lagu dan tema sebelum transisi");
        music.TahanMinigame(false);yield return Wait(.3f);Check(!music.Memutar,"Lagu tidak menyala kembali setelah kegagalan");
        Check(errors.Count==0,"Tidak ada error runtime pada dialog, resize, suara, lari, fokus dan kegagalan");
    }
}
