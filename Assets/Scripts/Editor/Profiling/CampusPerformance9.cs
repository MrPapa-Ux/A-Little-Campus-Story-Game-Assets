// PANDUAN PROYEK - Editor / Profiling
// Fungsi: Mengukur indikator performa di Editor; hasil bukan benchmark Android fisik.
// Panduan: Assets/Scripts/Editor/Profiling/README.md

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.Profiling;

public static class CampusPerformance9
{
    const string Key="Campus.Performance9";
    static IEnumerator routine;
    static string Root=>Directory.GetParent(Application.dataPath).FullName;
    [Serializable] class Sample { public string mode; public int frames; public double mainThreadMs,gcBytes,drawCalls,batches,triangles,gcUsedBytes; }
    [Serializable] class Report { public string environment="Unity Editor; bukan hasil benchmark Android"; public Sample[] samples; }
    [InitializeOnLoadMethod] static void Register(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    public static void Begin(string label){SessionState.SetString(Key,label);EditorApplication.isPlaying=true;}
    static void Tick()
    {
        if(!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating||SessionState.GetString(Key,"")=="")return;
        if(routine==null)routine=Measure();
        try{if(!routine.MoveNext()){routine=null;SessionState.SetString(Key,"");EditorApplication.isPlaying=false;}}
        catch(Exception e){File.WriteAllText(Path.Combine(Root,"work/performance9-error.txt"),e.ToString());SessionState.SetString(Key,"");routine=null;EditorApplication.isPlaying=false;}
    }
    static IEnumerator Measure()
    {
        Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();yield return null;yield return null;
        var player=UnityEngine.Object.FindFirstObjectByType<KarakterController>(); player.GetComponent<LazyBarArthur>().enabled=false;
        player.GetComponent<Rigidbody2D>().gravityScale=0f;
        var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();
        using(var cpu=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1))
        using(var gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1))
        using(var used=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Used Memory",1))
        {
            var list=new List<Sample>();
            for(int mode=0;mode<2;mode++)
            {
                if(mode==1){hp.BukaHPDefault();float until=Time.realtimeSinceStartup+1f;while(Time.realtimeSinceStartup<until)yield return null;hp.BukaAplikasi("IkonMap");}
                for(int warm=0;warm<150;warm++)yield return null;
                var s=new Sample{mode=mode==0?"world":"map",frames=240};
                for(int i=0;i<s.frames;i++){yield return null;s.mainThreadMs+=cpu.LastValue/1000000.0;s.gcBytes+=gc.LastValue;s.gcUsedBytes+=used.LastValue;s.drawCalls+=UnityStats.drawCalls;s.batches+=UnityStats.batches;s.triangles+=UnityStats.triangles;}
                s.mainThreadMs/=s.frames;s.gcBytes/=s.frames;s.gcUsedBytes/=s.frames;s.drawCalls/=s.frames;s.batches/=s.frames;s.triangles/=s.frames;list.Add(s);
            }
            File.WriteAllText(Path.Combine(Root,"work/performance9-"+SessionState.GetString(Key,"")+".json"),JsonUtility.ToJson(new Report{samples=list.ToArray()},true));
        }
    }
}
