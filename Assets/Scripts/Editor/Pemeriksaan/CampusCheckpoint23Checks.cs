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

public static class CampusCheckpoint23Checks
{
    public const string Key="Campus.Revision23.Check";
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
            if(File.Exists("work/revision23/run.request")&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isPlaying&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&Compiled())
            {
                File.Delete("work/revision23/run.request");
                SessionState.SetBool(Key+".BGConfigured",!PlayerSettings.runInBackground);
                SessionState.SetBool(Key,true);File.WriteAllText("work/revision23/runtime-checks.txt","RUNNING\n");EditorApplication.isPlaying=true;return;
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
    static void Check(bool ok,string s){if(!ok)throw new Exception(s);results.Add("OK: "+s);File.WriteAllText("work/revision23/progress.txt",string.Join("\n",results));}
    static void Finish(Exception error)
    {
        Application.logMessageReceived-=Log;
        if(started){Application.targetFrameRate=fps;QualitySettings.vSyncCount=vsync;}
        if(error==null&&errors.Count>0)error=new Exception(string.Join("\n",errors));
        try{RestoreView();}catch(Exception e){if(error==null)error=e;}
        File.WriteAllText("work/revision23/runtime-checks.txt",(error==null?"PASS":"FAIL\n"+error)+"\n"+string.Join("\n",results));
        Application.runInBackground=false;EditorApplication.isPaused=false;EditorApplication.isPlaying=false;SessionState.SetBool(Key,false);started=false;
    }
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
    static IEnumerator Capture(string name){ScreenCapture.CaptureScreenshot("work/revision23/"+name+".png");int f=Time.frameCount;while(Time.frameCount<f+3)yield return null;}
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
        var size=Activator.CreateInstance(type,Flags,null,new object[]{Enum.ToObject(enumType,1),width,height,"Revisi23 "+width+"x"+height},null);
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
        var tap=p.GetComponent<KetukKepalaArthur>();var face=p.GetComponentInChildren<EkspresiWajah2D>();var cam=p.GetComponentInChildren<Camera>();
        var bimo=GameObject.Find("NPC_Bimo_Joget").GetComponentInChildren<EkspresiWajah2D>();
        Check(bimo.EkspresiAktif==EmosiKarakter.Senang,"Ekspresi referensi Bimo adalah Senang dengan mata tertutup");
        foreach(bool custom in new[]{false,true})
        {
            face.gunakanEkspresi=custom;face.ekspresi=EmosiKarakter.Marah;yield return Wait(.1f);
            Vector2 head=cam.WorldToScreenPoint(tap.kepala.bounds.center);tap.Ketuk(head);yield return Wait(.06f);tap.Ketuk(head);yield return Wait(.15f);
            Check(tap.SedangJoget&&face.EkspresiAktif==bimo.EkspresiAktif,"Arthur memakai ekspresi mata tertutup Bimo selama joget");
            var eyes=face.transform.Find("Ekspresi_Senang/MataEkspresi");
            Check(eyes!=null&&eyes.GetComponentsInChildren<MeshRenderer>().Length==4,"Dua mask mata dan dua garis mata tertutup dirender");
            yield return Capture("joget-mata-tertutup");
            float deadline=Time.time+3;while(tap.SedangJoget&&Time.time<deadline){CheckEye(face);yield return null;}
            yield return Wait(.1f);
            Check(!tap.SedangJoget&&face.gunakanEkspresi==custom&&face.ekspresi==EmosiKarakter.Marah&&face.EkspresiAktif==(custom?EmosiKarakter.Marah:EmosiKarakter.Netral),"Ekspresi sebelumnya pulih tepat setelah joget");
        }
        Vector2 point=cam.WorldToScreenPoint(tap.kepala.bounds.center);tap.Ketuk(point);yield return Wait(.06f);tap.Ketuk(point);yield return Wait(.1f);
        face.AturEkspresiDialog(EmosiKarakter.Sedih);d.BukaDialog();yield return Wait(.1f);
        Check(!tap.SedangJoget&&face.EkspresiAktif==EmosiKarakter.Sedih,"Pembatalan joget tidak menimpa ekspresi dialog baru");
        d.TutupDialog();face.KembalikanEkspresi();yield return Wait(.1f);
        Check(errors.Count==0,"Tidak ada error runtime");
    }
    static void CheckEye(EkspresiWajah2D face){if(face.EkspresiAktif!=EmosiKarakter.Senang)throw new Exception("Ekspresi joget berubah sebelum selesai.");}
}
