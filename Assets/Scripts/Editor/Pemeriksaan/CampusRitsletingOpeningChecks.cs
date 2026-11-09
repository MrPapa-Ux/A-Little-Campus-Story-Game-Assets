// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Ritsleting Opening; alat pengujian Editor, bukan komponen gameplay.
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

public static class CampusRitsletingOpeningChecks
{
    const string Key="Campus.Zipper.Opening";
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    static bool started;
    static string Work(string name)=>Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",name);
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    public static void Begin()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Mulai dari Edit Mode.");
        EditorSceneManager.SaveOpenScenes();EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        File.WriteAllText(Work("zipper-opening-checks.txt"),"RUNNING\n");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
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
        File.WriteAllText(Work("zipper-opening-checks.txt"),(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));started=false;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;
    }
    static void Check(bool ok,string note){if(!ok)throw new Exception(note);results.Add("OK: "+note);}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}

    static IEnumerator Run()
    {
        Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();yield return Wait(.3f);
        var h=UnityEngine.Object.FindFirstObjectByType<SistemHP>();var app=h.tas;var zip=app.ritsleting;
        h.BukaHPDefault();yield return Wait(1f);h.BukaAplikasi("IkonTas");yield return Wait(1.3f);
        var item=app.ikonBaris[0].rectTransform;var sheet=app.transform.Find("BingkaiHP/Kertas");var nav=(RectTransform)sheet.Find("IkonNavigasi");
        Check(app.JumlahHalaman==8,"Delapan barang contoh, satu barang per halaman");
        Check(zip.bukaan!=null&&!zip.bukaan.raycastTarget,"Visual bukaan tidak menghalangi sentuhan");
        Check(zip.transform.Cast<Transform>().Where(t=>t.name.StartsWith("Gigi_")).All(t=>!t.gameObject.activeSelf),"Gigi lama tidak menumpuk di visual baru");
        for(int i=0;i<=8;i++)
        {
            float t=i/8f;var r=(RectTransform)zip.transform;
            var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(new Vector3(Mathf.Lerp(r.rect.xMin+zip.margin,r.rect.xMax-zip.margin,t),0,0)))};
            zip.OnDrag(e);yield return null;Canvas.ForceUpdateCanvases();
            Check(Mathf.Abs(zip.bukaan.kemajuan-t)<.001f,"Visual mengikuti tarikan kontinu: "+t);
            Check(Mathf.Abs(app.Halaman-t*7)<=.501f,"Barang mengikuti tarikan: "+t);
            var mesh=zip.bukaan.canvasRenderer.GetMesh();Check(mesh!=null&&mesh.vertexCount>0,"Mesh bukaan tampil: "+t);
            var verts=mesh.vertices;float low=verts.Min(v=>zip.bukaan.transform.TransformPoint(v).y);
            var c=new Vector3[4];item.GetWorldCorners(c);Check(low>c.Max(v=>v.y),"Bukaan berada di atas gambar barang: "+t);
            zip.tarikan.GetWorldCorners(c);float handleLow=c.Min(v=>v.y);item.GetWorldCorners(c);Check(handleLow>c.Max(v=>v.y),"Pegangan tidak menyentuh gambar barang: "+t);
            nav.GetWorldCorners(c);Check(low>c.Max(v=>v.y),"Bukaan jauh dari ikon navigasi: "+t);
            Check(verts.Max(v=>v.y)<=26&&verts.Min(v=>v.y)>=-26,"Bukaan tetap dalam tinggi jalur ritsleting: "+t);
            if(i==0||i==4||i==8){ScreenCapture.CaptureScreenshot(Work("zipper-opening-"+i+".png"));yield return Wait(.3f);}
            zip.OnEndDrag(e);Check(Mathf.Abs(zip.bukaan.kemajuan-app.Halaman/7f)<.001f,"Tarikan berhenti tepat di barang terpilih: "+t);
        }
        app.GeserHalaman(99);Check(app.Halaman==7,"Batas kanan tidak melampaui barang terakhir");app.GeserHalaman(-99);Check(app.Halaman==0,"Batas kiri tidak melampaui barang pertama");
        var saved=app.buku.barang.ToArray();app.buku.barang.Clear();app.buku.Segarkan();zip.Segarkan();Check(app.JumlahHalaman==1&&zip.bukaan.kemajuan==0,"Tas kosong tetap tertutup");
        app.buku.barang.Add(saved[0]);app.buku.Segarkan();zip.Segarkan();Check(app.JumlahHalaman==1&&zip.bukaan.kemajuan==0,"Satu barang tidak membuka halaman tambahan");
        app.buku.barang.AddRange(saved.Skip(1));app.buku.Segarkan();
        Check(errors.Count==0,"Tidak ada error baru saat drag dan ganti isi tas");
        h.TutupAplikasi();yield return Wait(.9f);h.TutupHP();yield return Wait(.4f);
    }
}

