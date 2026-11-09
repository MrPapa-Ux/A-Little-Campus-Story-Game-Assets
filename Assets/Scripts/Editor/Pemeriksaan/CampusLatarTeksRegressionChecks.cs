// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Latar Teks Regression; alat pengujian Editor, bukan komponen gameplay.
// Panduan: Assets/Scripts/Editor/Pemeriksaan/README.md

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class CampusLatarTeksRegressionChecks
{
    const string Key="Campus.LatarTeks.Regression";
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    static bool started;
    static string Work(string name)=>Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",name);
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    public static void Begin()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Mulai dari Edit Mode.");
        EditorSceneManager.SaveOpenScenes();EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        File.WriteAllText(Work("latar-teks-regression-checks.txt"),"RUNNING\n");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||EditorApplication.isCompiling||EditorApplication.isUpdating||!EditorApplication.isPlaying)return;
        if(!started){started=true;results.Clear();errors.Clear();Application.logMessageReceived+=Log;stack.Clear();stack.Push(Run());}
        try{while(stack.Count>0){var next=stack.Peek();if(!next.MoveNext()){stack.Pop();continue;}if(next.Current is IEnumerator child){stack.Push(child);continue;}return;}Finish(null);}catch(Exception e){Finish(e);}
    }
    static void Log(string s,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(s);}
    static void Finish(Exception e)
    {
        Application.logMessageReceived-=Log;if(e==null&&errors.Count>0)e=new Exception(string.Join("\n",errors));
        File.WriteAllText(Work("latar-teks-regression-checks.txt"),(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));started=false;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;
    }
    static void Check(bool ok,string note){if(!ok)throw new Exception(note);results.Add("OK: "+note);}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}

    static IEnumerator Run()
    {
        Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();yield return Wait(.3f);
        var h=UnityEngine.Object.FindFirstObjectByType<SistemHP>();var app=h.tas;var buku=h.GetComponent<BukuArthur>();
        h.BukaHPDefault();yield return Wait(1f);h.BukaAplikasi("IkonTas");yield return Wait(1.3f);
        var fit=app.GetComponentsInChildren<LatarTeksPasHP>();Check(fit.Length==2,"Dua latar otomatis aktif");
        int rebuildEvents=0;
        Action<UnityEngine.Object> observe=changed=>{if(CanvasUpdateRegistry.IsRebuildingGraphics()&&fit.Any(f=>f.teks==changed))rebuildEvents++;};
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(observe);
        try
        {
            for(int i=0;i<40;i++)
            {
                var before=fit.Select(f=>((RectTransform)f.transform).sizeDelta).ToArray();
                buku.barang[0].judul=i%2==0?"Air minum untuk kelas":"Struk";buku.Segarkan();
                var hint=fit.First(f=>f.name=="LatarPetunjuk").teks;hint.text=i%2==0?"GESER RITSLETING\nUNTUK PILIH BARANG":"TARIK RITSLETING\nUNTUK PINDAH BARANG";
                Canvas.ForceUpdateCanvases();
                Check(fit.Select((f,n)=>((RectTransform)f.transform).sizeDelta==before[n]).All(x=>x),"Rebuild grafis tidak mengubah ukuran latar secara langsung: "+i);
                yield return null;yield return null;
                foreach(var f in fit)
                {
                    var p=f.teks.GetPreferredValues(f.teks.text,1000,float.PositiveInfinity);
                    var expected=new Vector2(Mathf.Clamp(p.x+f.ruangTepi.x,f.ukuranMinimum.x,f.ukuranMaksimum.x),Mathf.Clamp(p.y+f.ruangTepi.y,f.ukuranMinimum.y,f.ukuranMaksimum.y));
                    Check(Vector2.Distance(expected,((RectTransform)f.transform).sizeDelta)<.1f,"Ukuran benar setelah LateUpdate: "+i+"/"+f.name);
                }
            }
            Check(rebuildEvents>=40,"Pengujian benar-benar memicu callback TMP saat graphic rebuild: "+rebuildEvents);
            var stable=fit.Select(f=>((RectTransform)f.transform).sizeDelta).ToArray();yield return Wait(1f);
            Check(fit.Select((f,n)=>((RectTransform)f.transform).sizeDelta==stable[n]).All(x=>x),"Ukuran tetap stabil ketika hanya wiggly yang berjalan");
            for(int i=0;i<3;i++){h.TutupAplikasi();yield return Wait(.9f);h.BukaAplikasi("IkonTas");yield return Wait(1.1f);app.GeserHalaman(i+1);Canvas.ForceUpdateCanvases();yield return null;yield return null;Check(app.Pilihan==i+1,"Buka ulang aplikasi dan pergantian barang tetap bekerja: "+i);}
            Check(errors.Count==0,"Tidak ada error/exception baru selama stress test");
        }
        finally{TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(observe);}
        h.TutupAplikasi();yield return Wait(.9f);h.TutupHP();yield return Wait(.4f);
    }
}
