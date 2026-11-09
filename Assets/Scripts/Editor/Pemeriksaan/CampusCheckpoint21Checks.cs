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

public static class CampusCheckpoint21Checks
{
    public const string Key="Campus.Revision21.Check";
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
            if(File.Exists("work/transition21/run.request")&&!EditorApplication.isPlayingOrWillChangePlaymode&&!EditorApplication.isCompiling&&!EditorApplication.isUpdating&&Compiled())
            {
                File.Delete("work/transition21/run.request");
                SessionState.SetBool(Key+".BGConfigured",!PlayerSettings.runInBackground);
                SessionState.SetBool(Key,true);File.WriteAllText("work/transition21/runtime-checks.txt","RUNNING\n");EditorApplication.isPlaying=true;return;
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
    static void Check(bool ok,string s){if(!ok)throw new Exception(s);results.Add("OK: "+s);File.WriteAllText("work/transition21/progress.txt",string.Join("\n",results));}
    static void Finish(Exception error)
    {
        Application.logMessageReceived-=Log;
        if(started){Application.targetFrameRate=fps;QualitySettings.vSyncCount=vsync;}
        if(error==null&&errors.Count>0)error=new Exception(string.Join("\n",errors));
        try{RestoreView();}catch(Exception e){if(error==null)error=e;}
        File.WriteAllText("work/transition21/runtime-checks.txt",(error==null?"PASS":"FAIL\n"+error)+"\n"+string.Join("\n",results));
        Application.runInBackground=false;EditorApplication.isPaused=false;EditorApplication.isPlaying=false;SessionState.SetBool(Key,false);started=false;
    }
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
    static IEnumerator Capture(string name){ScreenCapture.CaptureScreenshot("work/transition21/"+name+".png");int f=Time.frameCount;while(Time.frameCount<f+3)yield return null;}
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
        var size=Activator.CreateInstance(type,Flags,null,new object[]{Enum.ToObject(enumType,1),width,height,"Revisi21 "+width+"x"+height},null);
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
        var d=DialogManager.Instance;d.arthurScript.GetComponent<LazyBarArthur>().enabled=false;
        foreach(var module in EventSystem.current.GetComponents<BaseInputModule>())module.enabled=false;
        yield return Wait(.5f);InitView();
        var transition=d.GetComponent<SistemPilihan>().efekTransisi;
        Check(transition!=null,"Referensi transisi asli SistemPilihan tersedia");
        Check(SessionState.GetBool(Key+".BGConfigured",false),"Run In Background proyek tetap mati");
        foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1024,768),new Vector2Int(436,629)})
        {
            View(size.x,size.y);yield return Wait(.7f);
            int calls=0;float atBlack=-1;bool closeShot=false,openShot=false;float last=0;bool opened=false;
            transition.MulaiTransisiTeleport(()=>{calls++;atBlack=transition.blinds.First(b=>b!=null).GetComponent<CanvasGroup>().alpha;ScreenCapture.CaptureScreenshot("work/transition21/black-"+size.x+".png");});
            transition.MulaiTransisiTeleport(()=>calls+=100);
            var image=transition.blinds.First(b=>b!=null);var group=image.GetComponent<CanvasGroup>();
            Check(transition.blinds.Count(b=>b!=null&&b.enabled)==1,"Satu bidang utuh, tanpa bilah terpisah "+size);
            Check(image.sprite==null&&image.type==Image.Type.Simple&&group.blocksRaycasts,"Bidang hitam polos memblokir input saat transisi");
            float start=Time.realtimeSinceStartup;float deadline=start+4;
            while(transition.SedangBerjalan&&Time.realtimeSinceStartup<deadline)
            {
                float alpha=group.alpha;
                if(calls==0)CheckSample(alpha>=last-.001f,"Fade menutup tidak melompat");
                else {if(alpha<last-.001f)opened=true;if(opened)CheckSample(alpha<=last+.001f,"Fade membuka tidak melompat");}
                if(alpha>.35f&&alpha<.75f)
                {
                    if(calls==0&&!closeShot){ScreenCapture.CaptureScreenshot("work/transition21/close-"+size.x+".png");closeShot=true;}
                    if(calls==1&&!openShot){ScreenCapture.CaptureScreenshot("work/transition21/open-"+size.x+".png");openShot=true;}
                }
                last=alpha;yield return null;
            }
            Check(!transition.SedangBerjalan&&calls==1&&atBlack==1,"Perpindahan tepat sekali saat sepenuhnya gelap "+size);
            Check(closeShot&&openShot&&opened,"Kedua fase fade dirender "+size);
            Check(group.alpha==0&&!group.blocksRaycasts&&!transition.gameObject.activeSelf,"Transisi selesai bersih, input dilepas");
            Check(Time.realtimeSinceStartup-start<2,"Transisi singkat, tanpa menunggu berlebihan");
            yield return Wait(.2f);yield return Capture("after-"+size.x);
            var corners=new Vector3[4];image.rectTransform.GetWorldCorners(corners);
            Check(Vector2.Distance(corners[0],Vector2.zero)<2&&Vector2.Distance(corners[2],new Vector2(Screen.width,Screen.height))<2,"Bidang transisi menutup seluruh layar "+size);
        }
        Check(errors.Count==0,"Tidak ada error runtime");
    }
    static void CheckSample(bool ok,string message){if(!ok)throw new Exception(message);}
}
