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

public static class CampusCheckpoint22Checks
{
    public const string Key="Campus.Revision22.Check";
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
            if(File.Exists("work/revision22/run.request")&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isPlaying&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&Compiled())
            {
                File.Delete("work/revision22/run.request");CampusCheckpoint22.Apply();
                SessionState.SetBool(Key+".BGConfigured",!PlayerSettings.runInBackground);
                SessionState.SetBool(Key,true);File.WriteAllText("work/revision22/runtime-checks.txt","RUNNING\n");EditorApplication.isPlaying=true;return;
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
    static void Check(bool ok,string s){if(!ok)throw new Exception(s);results.Add("OK: "+s);File.WriteAllText("work/revision22/progress.txt",string.Join("\n",results));}
    static void Finish(Exception error)
    {
        Application.logMessageReceived-=Log;
        if(started){Application.targetFrameRate=fps;QualitySettings.vSyncCount=vsync;}
        if(error==null&&errors.Count>0)error=new Exception(string.Join("\n",errors));
        try{RestoreView();}catch(Exception e){if(error==null)error=e;}
        File.WriteAllText("work/revision22/runtime-checks.txt",(error==null?"PASS":"FAIL\n"+error)+"\n"+string.Join("\n",results));
        Application.runInBackground=false;EditorApplication.isPaused=false;EditorApplication.isPlaying=false;SessionState.SetBool(Key,false);started=false;
    }
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
    static IEnumerator Capture(string name){ScreenCapture.CaptureScreenshot("work/revision22/"+name+".png");int f=Time.frameCount;while(Time.frameCount<f+3)yield return null;}
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
        var size=Activator.CreateInstance(type,Flags,null,new object[]{Enum.ToObject(enumType,1),width,height,"Revisi22 "+width+"x"+height},null);
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
        if(DialogManager.Instance==null){var load=UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("CharacterLab");while(!load.isDone)yield return null;yield return Wait(.7f);}
        var d=DialogManager.Instance;var p=d.arthurScript;p.GetComponent<LazyBarArthur>().enabled=false;
        foreach(var module in EventSystem.current.GetComponents<BaseInputModule>())module.enabled=false;
        yield return Wait(.7f);InitView();var camera=p.GetComponentInChildren<Camera>();
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1200,720),new Vector2Int(1024,768),new Vector2Int(1600,650),new Vector2Int(720,960)})
        {
            View(size.x,size.y);yield return Wait(.6f);Canvas.ForceUpdateCanvases();
            Check(camera.rect==new Rect(0,0,1,1)&&Mathf.Abs(camera.aspect-size.x/(float)size.y)<.001f,"Kamera memenuhi layar tanpa bilah "+size);
            foreach(var root in new[]{"UI_Gameplay","CanvasSistemHP","CanvasPengaturan"})
            {
                var canvas=Resources.FindObjectsOfTypeAll<Canvas>().First(c=>c.name==root&&c.gameObject.scene==d.gameObject.scene);
                var frame=(RectTransform)canvas.transform.Find("AreaTampilan16x9");var corners=new Vector3[4];frame.GetWorldCorners(corners);
                Check(Vector2.Distance(corners[0],Vector2.zero)<2&&Vector2.Distance(corners[2],new Vector2(Screen.width,Screen.height))<2,"UI mengikuti seluruh layar "+root);
            }
            yield return Capture("world-"+size.x);
        }
        View(1280,720);yield return Wait(.5f);
        var tap=p.GetComponent<KetukKepalaArthur>();var dance=p.GetComponent<NPCJogetGoofy>();
        Vector2 head=camera.WorldToScreenPoint(tap.kepala.bounds.center);
        tap.Ketuk(Vector2.zero);tap.Ketuk(Vector2.zero);Check(!tap.SedangJoget,"Ketukan di luar kepala diabaikan");
        tap.Ketuk(head);yield return Wait(.08f);Check(!tap.SedangJoget,"Satu ketukan tidak memulai joget");
        var hand=dance.tanganKiri.localRotation;float ground=dance.kakiKiri.GetComponent<SpriteRenderer>().bounds.min.y;Vector3 position=p.transform.position;
        tap.Ketuk(head);float start=Time.time;
        Check(tap.SedangJoget&&dance.enabled&&p.kunciPergerakan&&!p.animator.enabled,"Ketuk ganda memulai aksi NPC pada Arthur");
        yield return Wait(.25f);Check(Quaternion.Angle(hand,dance.tanganKiri.localRotation)>3,"Tangan Arthur benar-benar bergerak joget");
        Check(Mathf.Abs(ground-dance.kakiKiri.GetComponent<SpriteRenderer>().bounds.min.y)<.01f,"Telapak tetap menapak saat joget");
        yield return Capture("arthur-joget");tap.Ketuk(head);
        while(tap.SedangJoget&&Time.time-start<3)yield return null;
        Check(!tap.SedangJoget&&Time.time-start>=1.95f&&Time.time-start<2.2f,"Joget selesai sekitar dua detik, ketukan tambahan tidak memperpanjang");
        Check(p.animator.enabled&&!p.kunciPergerakan&&!dance.enabled&&Vector3.Distance(position,p.transform.position)<.05f,"Pose dan gerakan pulih tanpa teleport");
        p.JalanKeKanan();yield return Wait(.3f);Check(p.transform.position.x>position.x+.1f,"Arthur bisa berjalan lagi setelah joget");p.BerhentiBergerak();
        head=camera.WorldToScreenPoint(tap.kepala.bounds.center);tap.Ketuk(head);yield return Wait(.05f);tap.Ketuk(head);Check(tap.SedangJoget,"Joget dapat dipakai kembali");
        d.BukaDialog();yield return Wait(.1f);Check(!tap.SedangJoget&&p.kunciPergerakan,"Dialog membatalkan joget dan mempertahankan kunci gerak");
        tap.Ketuk(head);tap.Ketuk(head);Check(!tap.SedangJoget,"Ketuk kepala saat dialog diabaikan");d.TutupDialog();
        var loadMenu=UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainMenu");while(!loadMenu.isDone)yield return null;yield return Wait(.8f);
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1200,980),new Vector2Int(1600,650)})
        {
            View(size.x,size.y);yield return Wait(.8f);Canvas.ForceUpdateCanvases();
            var frame=UnityEngine.Object.FindFirstObjectByType<FramePlayer>();var rect=frame.targetImage.rectTransform;var corners=new Vector3[4];rect.GetWorldCorners(corners);
            Check(corners[0].x<=1&&corners[0].y<=1&&corners[2].x>=Screen.width-1&&corners[2].y>=Screen.height-1,"Latar menu menutupi layar penuh "+size);
            Check(Mathf.Abs(rect.rect.width/rect.rect.height-frame.targetImage.sprite.rect.width/frame.targetImage.sprite.rect.height)<.002f,"Proporsi gambar menu terjaga");
            var credit=GameObject.Find("KreditPembuat").GetComponent<TMP_Text>();yield return Capture("inspect-menu");Fits(credit.rectTransform,"Kredit menu");
            Check(UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Count(t=>t.text.Contains("Chipp"))==1,"Satu label kredit menu aktif");
            foreach(var button in UnityEngine.Object.FindFirstObjectByType<TitleMenuManager>().GetComponentsInChildren<Button>())Fits((RectTransform)button.transform,"Tombol menu "+button.name);
            yield return Capture("menu-"+size.x+"x"+size.y);
        }
        Check(errors.Count==0,"Tidak ada error runtime selama revisi 22");
    }
}
