// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Glitch; alat pengujian Editor, bukan komponen gameplay.
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

public static class CampusGlitchChecks
{
    const string Key="Campus.Glitch.Checks";
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    static bool started;
    static string Work(string name)=>Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",name);
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    public static void Begin()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Mulai dari Edit Mode.");
        EditorSceneManager.SaveOpenScenes();EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        File.WriteAllText(Work("glitch-checks.txt"),"RUNNING\n");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
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
        File.WriteAllText(Work("glitch-checks.txt"),(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));started=false;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;
    }
    static void Check(bool ok,string note){if(!ok)throw new Exception(note);results.Add("OK: "+note);}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}

    static IEnumerator Run()
    {
        Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();yield return Wait(.4f);
        var b=UnityEngine.Object.FindFirstObjectByType<LazyBarArthur>();b.enabled=false;
        Check(b.efekGlitchLayar!=null,"Efek terpasang pada panel glitch lama");
        Check(b.efekGlitchLayar.fontError!=null,"Font error menggunakan aset yang sudah tersedia");
        Check(b.durasiSpam==3,"Durasi glitch scene tetap tiga detik");
        typeof(LazyBarArthur).GetMethod("PicuEfekGlitch",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(b,null);
        yield return Wait(.2f);Canvas.ForceUpdateCanvases();
        Check(b.waktuHabis&&b.panelGlitch.activeInHierarchy,"Pemicu asli mengaktifkan glitch dan status waktu habis");
        Check(b.teksGlitch.text.Length>2500,"Banjir teks langsung memenuhi layar, bukan menunggu append per frame");
        var errorsUI=b.panelGlitch.GetComponentsInChildren<TextMeshProUGUI>(true).Where(t=>t.name.StartsWith("ErrorTumpuk_")).ToArray();
        Check(errorsUI.Length==6,"Pool enam jendela error dibuat sekali");
        Check(errorsUI.All(t=>!t.raycastTarget&&!t.enableAutoSizing),"Label error tidak menghalangi input dan tidak memakai AutoSize");
        int objects=b.panelGlitch.GetComponentsInChildren<Transform>(true).Length;
        ScreenCapture.CaptureScreenshot(Work("glitch-awal.png"));yield return Wait(1f);Canvas.ForceUpdateCanvases();
        Check(errorsUI.Count(t=>t.gameObject.activeInHierarchy)>=2,"Error bertambah dan menimpa teks seiring intensitas");
        var graphic=b.panelGlitch.GetComponentInChildren<KotakGlitchUI>();var mesh=graphic.canvasRenderer.GetMesh();
        Check(mesh!=null&&mesh.vertexCount>250,"Kotak rusak dan scanline benar-benar memiliki mesh tampil");
        Check(graphic.Jendela.Take(2).All(r=>r.width>0&&r.height>0),"Kotak error mempunyai ukuran nyata");
        Check(b.panelGlitch.GetComponent<Image>().sprite==null,"Penutup gelap memakai bidang solid, bukan sprite transparan");
        ScreenCapture.CaptureScreenshot(Work("glitch-tengah.png"));yield return Wait(1.2f);Canvas.ForceUpdateCanvases();
        Check(b.panelGlitch.GetComponentsInChildren<Transform>(true).Length==objects,"Jumlah objek tetap saat efek berjalan");
        Check(errorsUI.Count(t=>t.gameObject.activeInHierarchy)>=4,"Menjelang akhir layar dipenuhi jendela error");
        Check(b.efekGlitchLayar.JumlahPembaruan<35,"Pembaruan efek dibatasi, bukan setiap frame render");
        Check(b.teksGlitch.text.Length<8192,"Teks dibatasi oleh buffer, tidak membesar tanpa batas");
        ScreenCapture.CaptureScreenshot(Work("glitch-akhir.png"));yield return Wait(.65f);
        float end=Time.realtimeSinceStartup+3;while(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="LoadingScene"&&Time.realtimeSinceStartup<end)yield return null;
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="LoadingScene","Alur asli tetap berpindah ke LoadingScene");
        Check(errors.Count==0,"Tidak ada error Unity baru selama glitch dan perpindahan scene");
    }
}
