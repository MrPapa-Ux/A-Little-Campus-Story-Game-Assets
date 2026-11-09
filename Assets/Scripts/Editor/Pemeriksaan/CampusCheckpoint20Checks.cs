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

public static class CampusCheckpoint20Checks
{
    public const string Key="Campus.Revision20.Check";
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
            if(File.Exists("work/layout20/run.request")&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&Compiled())
            {
                File.Delete("work/layout20/run.request");CampusCheckpoint20.Apply();CampusCheckpoint20.Apply();
                SessionState.SetBool(Key+".BGConfigured",!PlayerSettings.runInBackground);
                SessionState.SetBool(Key,true);File.WriteAllText("work/layout20/runtime-checks.txt","RUNNING\n");EditorApplication.isPlaying=true;return;
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
    static void Check(bool ok,string s){if(!ok)throw new Exception(s);results.Add("OK: "+s);File.WriteAllText("work/layout20/progress.txt",string.Join("\n",results));}
    static void Finish(Exception error)
    {
        Application.logMessageReceived-=Log;
        if(started){Application.targetFrameRate=fps;QualitySettings.vSyncCount=vsync;}
        if(error==null&&errors.Count>0)error=new Exception(string.Join("\n",errors));
        try{RestoreView();}catch(Exception e){if(error==null)error=e;}
        File.WriteAllText("work/layout20/runtime-checks.txt",(error==null?"PASS":"FAIL\n"+error)+"\n"+string.Join("\n",results));
        Application.runInBackground=false;EditorApplication.isPaused=false;EditorApplication.isPlaying=false;SessionState.SetBool(Key,false);started=false;
    }
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
    static IEnumerator Capture(string name){ScreenCapture.CaptureScreenshot("work/layout20/"+name+".png");int f=Time.frameCount;while(Time.frameCount<f+3)yield return null;}
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
        var size=Activator.CreateInstance(type,Flags,null,new object[]{Enum.ToObject(enumType,1),width,height,"Revisi20 "+width+"x"+height},null);
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
        yield return null;
        var focus=UnityEngine.Object.FindFirstObjectByType<JedaSaatTidakAktif>();focus.AturJeda(false);
        var d=DialogManager.Instance;var p=d.arthurScript;var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();
        var lazy=p.GetComponent<LazyBarArthur>();lazy.enabled=false;var system=d.GetComponent<SistemPilihan>();
        foreach(var module in EventSystem.current.GetComponents<BaseInputModule>())module.enabled=false;
        yield return Wait(.7f);InitView();
        Check(SessionState.GetBool(Key+".BGConfigured",false),"Run In Background tetap nonaktif di luar uji");
        var lyra=GameObject.Find("NPC_Lyra");
        p.transform.position=new Vector3(lyra.transform.position.x-.8f,p.transform.position.y,p.transform.position.z);Physics2D.SyncTransforms();yield return Wait(.5f);
        Check(d.bisaInteraksi&&d.idInteraksiAktif==lyra.GetComponent<AreaInteraksi>().idTempat,"Trigger fisik Lyra aktif");
        var cam=p.GetComponentInChildren<Camera>();float fov=cam.fieldOfView;Vector3 landmark=p.transform.position+new Vector3(3,2,0);Vector3 baseline=Vector3.zero;
        int n=0;
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(960,540),new Vector2Int(1024,768),new Vector2Int(436,629),new Vector2Int(1600,650)})
        {
            View(size.x,size.y);yield return Wait(.8f);Canvas.ForceUpdateCanvases();
            Check(Screen.width==size.x&&Screen.height==size.y,"Resolusi aktual "+size);
            var area=BingkaiKameraTetap.Area(size.x,size.y);
            Check(Mathf.Abs(cam.aspect-BingkaiKameraTetap.Rasio)<.001f&&Vector4.Distance(new Vector4(cam.rect.x,cam.rect.y,cam.rect.width,cam.rect.height),new Vector4(area.x,area.y,area.width,area.height))<.001f,"Bingkai kamera 16:9 "+size);
            var v=cam.WorldToViewportPoint(landmark);if(n++==0)baseline=v;
            Check(Vector3.Distance(v,baseline)<.01f&&cam.fieldOfView==fov,"Komposisi dunia dan FOV tetap "+size);
            foreach(var root in new[]{"UI_Gameplay","CanvasDialog","CanvasSistemHP","CanvasPengaturan","CanvasTransisiAplikasi"})
            {
                var canvas=Resources.FindObjectsOfTypeAll<Canvas>().First(c=>c.name==root&&c.gameObject.scene==d.gameObject.scene);
                if(!canvas.gameObject.activeInHierarchy)continue;
                var frame=(RectTransform)canvas.transform.Find("AreaTampilan16x9");var corners=new Vector3[4];frame.GetWorldCorners(corners);
                Check(Vector2.Distance(corners[0],cam.pixelRect.min)<2&&Vector2.Distance(corners[2],cam.pixelRect.max)<2,"Area UI sejajar kamera: "+root+" "+size);
                Check(frame.childCount>0&&frame.Find("AreaTampilan16x9")==null,"Pembungkus tidak berulang: "+root);
            }
            system.AksiTombolInteraksi();yield return Wait(.8f);
            Check(d.PercakapanAktif&&d.canvasDialog.activeInHierarchy&&system.teksCeritaUtama.text.Length>0,"Dialog Lyra tampil "+size);
            Check(d.hudSaatPercakapan.Length==2&&d.hudSaatPercakapan.All(g=>g.alpha==0&&!g.blocksRaycasts&&!g.interactable),"Bar dan ikon HP tersembunyi selama dialog");
            var dialogFrame=(RectTransform)d.canvasDialog.transform.Find("AreaTampilan16x9");var dc=new Vector3[4];dialogFrame.GetWorldCorners(dc);
            Check(Vector2.Distance(dc[0],cam.pixelRect.min)<2&&Vector2.Distance(dc[2],cam.pixelRect.max)<2,"Area dialog aktif sejajar kamera");
            yield return Capture("inspect-dialog");
            Glyphs(system.teksCeritaUtama);
            var bounds=system.teksCeritaUtama.textBounds;var lo=RectTransformUtility.WorldToScreenPoint(null,system.teksCeritaUtama.transform.TransformPoint(bounds.min));var hi=RectTransformUtility.WorldToScreenPoint(null,system.teksCeritaUtama.transform.TransformPoint(bounds.max));
            Check(lo.x>=cam.pixelRect.xMin&&lo.y>=cam.pixelRect.yMin&&hi.x<=cam.pixelRect.xMax&&hi.y<=cam.pixelRect.yMax,"Glyph dialog utuh di bingkai "+lo+" "+hi);
            var label=RectTransformUtility.WorldToScreenPoint(null,system.teksCeritaUtama.transform.position);
            Check(label.y>cam.pixelRect.center.y,"Teks dialog berada di bagian atas bingkai");
            yield return Capture("dialog-"+size.x+"x"+size.y);
            d.canvasDialog.SetActive(false);yield return Wait(.1f);
            Check(d.PercakapanAktif&&d.hudSaatPercakapan.All(g=>g.alpha==0),"HUD tetap tersembunyi saat selingan emote");
            d.canvasDialog.SetActive(true);d.TutupDialog();yield return Wait(.3f);
            Check(!d.PercakapanAktif&&d.hudSaatPercakapan.All(g=>g.alpha==1&&g.blocksRaycasts&&g.interactable),"HUD kembali setelah dialog selesai");
            foreach(var name in new[]{"Btn_Kiri","Btn_Kanan","Btn_Interact","Btn_HandPhone"})Fits((RectTransform)Resources.FindObjectsOfTypeAll<Button>().First(b=>b.name==name&&b.gameObject.scene==d.gameObject.scene).transform,name);
            yield return Capture("world-"+size.x+"x"+size.y);
        }
        var custom=d.hudSaatPercakapan[0];custom.alpha=.7f;custom.interactable=false;d.BukaDialog();d.BukaDialog();d.TutupDialog();
        Check(Mathf.Abs(custom.alpha-.7f)<.001f&&!custom.interactable,"Status HUD sebelum dialog dipulihkan tepat meski buka berulang");custom.alpha=1;custom.interactable=true;
        View(1280,720);yield return Wait(.6f);
        var nara=GameObject.Find("NPC_Nara_Ekspresi");p.transform.position=new Vector3(nara.transform.position.x-.8f,p.transform.position.y,p.transform.position.z);Physics2D.SyncTransforms();yield return Wait(.5f);
        system.AksiTombolInteraksi();yield return Wait(.8f);Check(d.PercakapanAktif&&d.canvasDialog.activeInHierarchy,"Dialog Nara tampil dengan HUD tersembunyi");yield return Capture("nara");d.TutupDialog();yield return Wait(.4f);
        hp.BukaHPDefault();yield return Wait(1);hp.BukaAplikasi("IkonMusic");yield return Wait(1.5f);
        Fits((RectTransform)hp.ponselBersama.transform,"Panel musik HP");yield return Capture("music");
        hp.TutupAplikasi();yield return Wait(1.5f);hp.TutupHP();yield return Wait(.7f);
        Check(errors.Count==0,"Tidak ada error runtime selama pengujian");
    }
}
