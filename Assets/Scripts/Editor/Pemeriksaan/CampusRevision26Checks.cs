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

public static class CampusRevision26Checks
{
    public const string Key="Campus.Revision26.Check";
    static IEnumerator running;static bool busy,started;static int fps,vsync,oldSize=-1;
    static EditorWindow view;static PropertyInfo selected;static object group;
    static readonly List<int> customSizes=new List<int>();
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.delayCall+=Tick;}
    static void Tick()
    {
        if(busy)return;busy=true;
        try
        {
            if(File.Exists("work/restore26/run.request")&&EditorApplication.isPlaying&&!SessionState.GetBool(Key,false)&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating)
            {EditorApplication.isPlaying=false;return;}
            if(File.Exists("work/restore26/run.request")&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isPlaying&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&Compiled())
            {
                File.Delete("work/restore26/run.request");CampusRevision26.ReloadRestoredScene();
                SessionState.SetBool(Key+".BGConfigured",!PlayerSettings.runInBackground);
                SessionState.SetBool(Key,true);File.WriteAllText("work/restore26/runtime-checks.txt","RUNNING\n");EditorApplication.isPlaying=true;return;
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
    static bool Compiled()=>File.GetLastWriteTimeUtc("Library/ScriptAssemblies/Assembly-CSharp-Editor.dll")>=Directory.GetFiles("Assets/Scripts/Editor","*.cs",SearchOption.AllDirectories).Max(File.GetLastWriteTimeUtc)&&File.GetLastWriteTimeUtc("Library/ScriptAssemblies/Assembly-CSharp.dll")>=Directory.GetFiles("Assets/Scripts/Runtime","*.cs",SearchOption.AllDirectories).Max(File.GetLastWriteTimeUtc);
    static void Log(string s,string trace,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(s);}
    static void Check(bool ok,string s){if(!ok)throw new Exception(s);results.Add("OK: "+s);File.WriteAllText("work/restore26/progress.txt",string.Join("\n",results));}
    static void Finish(Exception error)
    {
        Application.logMessageReceived-=Log;
        if(started){Application.targetFrameRate=fps;QualitySettings.vSyncCount=vsync;}
        if(error==null&&errors.Count>0)error=new Exception(string.Join("\n",errors));
        try{RestoreView();}catch(Exception e){if(error==null)error=e;}
        File.WriteAllText("work/restore26/runtime-checks.txt",(error==null?"PASS":"FAIL\n"+error)+"\n"+string.Join("\n",results));
        Application.runInBackground=false;EditorApplication.isPaused=false;EditorApplication.isPlaying=false;SessionState.SetBool(Key,false);started=false;
    }
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
    static IEnumerator Capture(string name){ScreenCapture.CaptureScreenshot("work/restore26/"+name+".png");int f=Time.frameCount;while(Time.frameCount<f+3)yield return null;}
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
        var size=Activator.CreateInstance(type,Flags,null,new object[]{Enum.ToObject(enumType,1),width,height,"Revisi26 "+width+"x"+height},null);
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
    static IEnumerator Run()
    {
        yield return null;UnityEngine.Object.FindFirstObjectByType<JedaSaatTidakAktif>().AturJeda(false);
        if(DialogManager.Instance==null){var op=UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("CharacterLab");while(!op.isDone)yield return null;yield return Wait(.6f);}
        var d=DialogManager.Instance;var p=d.arthurScript;p.GetComponent<LazyBarArthur>().enabled=false;
        foreach(var module in EventSystem.current.GetComponents<BaseInputModule>())module.enabled=false;
        yield return Wait(.7f);InitView();View(1280,720);yield return Wait(.5f);
        Check(UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include,FindObjectsSortMode.None).All(m=>m==null||!new[]{"BayanganKarakter","KontrolSentuhAdaptif","TataMenuAdaptif","LatarMenuLapisan"}.Contains(m.GetType().Name)),"Tambahan visual checkpoint 25 sudah dibatalkan");
        var source=p.transform.Find("Langkah Arthur").GetComponent<AudioSource>();Check(Mathf.Abs(source.volume-.4f)<.001f,"Volume lari 0,4: sepertiga lebih kecil daripada revisi yang ditolak");
        int before=p.JumlahLangkah;p.JalanKeKanan();yield return Wait(.5f);p.BerhentiBergerak();Check(p.JumlahLangkah==before,"Berjalan tetap senyap");
        p.JalanKeKiri();yield return Wait(.04f);p.LepasKiri();yield return Wait(.04f);p.JalanKeKiri();
        var data=new float[256];float peak=0,end=Time.realtimeSinceStartup+2.2f;before=p.JumlahLangkah;
        while(Time.realtimeSinceStartup<end){source.GetOutputData(data,0);foreach(float value in data)peak=Mathf.Max(peak,Mathf.Abs(value));yield return null;}
        p.BerhentiBergerak();yield return Wait(.5f);Check(p.JumlahLangkah>before&&peak>.001f,"Audio lari tetap aktif mengikuti animasi; peak "+peak);
        var camera=p.GetComponentInChildren<Camera>();var bob=camera.GetComponent<KameraGoyang>();if(bob!=null)bob.enabled=false;
        var rb=p.GetComponent<Rigidbody2D>();rb.bodyType=RigidbodyType2D.Kinematic;p.animator.enabled=false;
        Vector3 origin=camera.transform.position;Vector3 a=origin+camera.transform.forward*10,b=a+camera.transform.up;
        float pixelHeight=0,actorHeight=0;
        foreach(var size in new[]{new Vector2Int(1390,800),new Vector2Int(764,800),new Vector2Int(640,420),new Vector2Int(365,650),new Vector2Int(1280,720)})
        {
            View(size.x,size.y);yield return Wait(.7f);
            Check(Screen.width==size.x&&Screen.height==size.y,"Resolusi benar-benar berubah "+size);
            float projected=Mathf.Abs(camera.WorldToScreenPoint(b).y-camera.WorldToScreenPoint(a).y);
            var renderers=p.wadahVisual.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.enabled).ToArray();float lo=renderers.Min(r=>camera.WorldToScreenPoint(r.bounds.min).y),hi=renderers.Max(r=>camera.WorldToScreenPoint(r.bounds.max).y);
            if(pixelHeight==0){pixelHeight=projected;actorHeight=hi-lo;}
            Check(Mathf.Abs(projected-pixelHeight)<.15f&&Mathf.Abs((hi-lo)-actorHeight)<.2f,"Ukuran dunia dan Arthur dalam pixel tetap "+size+" landmark="+projected+" Arthur="+(hi-lo));
            Check(Vector3.Distance(camera.transform.position,origin)<.001f&&camera.rect==new Rect(0,0,1,1),"Posisi kamera tetap dan layar penuh");
            yield return Capture("world-"+size.x+"x"+size.y);
        }
        var load=UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainMenu");while(!load.isDone)yield return null;yield return Wait(.6f);
        var menu=UnityEngine.Object.FindFirstObjectByType<TitleMenuManager>();var frame=UnityEngine.Object.FindFirstObjectByType<FramePlayer>();
        Check(frame.frames.Count==300,"Video asli 300 frame dipulihkan");
        Check(menu.transform.Find("LatarKampusPenuh")==null&&menu.transform.Find("JudulMenuUtuh")==null,"Tidak ada pemecahan judul atau crop latar dari checkpoint 25");
        frame.enabled=false;frame.targetImage.sprite=frame.frames.First(f=>f.name=="ezgif-frame-034");
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1170,810),new Vector2Int(365,650)})
        {
            View(size.x,size.y);yield return Wait(.6f);Canvas.ForceUpdateCanvases();var corners=new Vector3[4];frame.targetImage.rectTransform.GetWorldCorners(corners);
            Check(Vector2.Distance(corners[0],Vector2.zero)<2&&Vector2.Distance(corners[2],new Vector2(Screen.width,Screen.height))<2,"Seluruh video asli pas layar tanpa crop "+size);
            Check(frame.targetImage.material==null||frame.targetImage.material.shader.name!="Campus/JudulTransparan","Materi video asli dipakai");
            foreach(var button in menu.GetComponentsInChildren<Button>())Fits((RectTransform)button.transform,"Menu "+button.name);
            yield return Capture("menu-"+size.x+"x"+size.y);
        }
        Check(errors.Count==0,"Tidak ada error runtime selama pengujian");
    }
}
