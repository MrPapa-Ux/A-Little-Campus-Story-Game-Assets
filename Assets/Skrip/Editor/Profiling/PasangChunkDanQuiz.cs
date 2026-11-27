using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public static class PasangChunkDanQuiz {
 public static void Run(){try{
  Directory.CreateDirectory("../work/chunk-20261126");
  var main=EditorSceneManager.OpenScene("Assets/Scenes/Penataan Kampus - Jalur NPC dan Bubble.unity");
  var original=main.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MrQuizoChapter1>(true)).Single(q=>q.namaPenjaga=="Mr. Quizzo");
  foreach(var name in new[]{"Penataan Kampus - Jalur NPC dan Bubble","Explorer"}){
   var scene=name==main.name?main:EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity",OpenSceneMode.Additive);SceneManager.SetActiveScene(scene);
   var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
   string backup="../work/chunk-20261126/"+name+"-sebelum.unity";if(!File.Exists(backup))EditorSceneManager.SaveScene(scene,backup,true);
   var q=all.Select(t=>t.GetComponent<MrQuizoChapter1>()).FirstOrDefault(x=>x!=null&&x.namaPenjaga=="Mr. Quizzo");
   if(q==null){var root=UnityEngine.Object.Instantiate(original.transform.parent.gameObject);root.name="NPC Quizzo - Akses Jasmine ke GK2";SceneManager.MoveGameObjectToScene(root,scene);q=root.GetComponentInChildren<MrQuizoChapter1>(true);q.kuisSebelumnya=null;}
   Transform Cari(string n)=>all.Single(t=>t.name==n);
   var route=Cari("GK2_JalurUtama_BelokOtomatis").GetComponent<JalurKampus>();
   var points=q.posisiJaga.ToList();var entry=q.transform.parent.Find("Posisi Quizzo - Depan Jasmine ke GK2");
   if(entry==null){entry=new GameObject("Posisi Quizzo - Depan Jasmine ke GK2").transform;entry.SetParent(q.transform.parent,false);}
   entry.position=Cari("TitikKembaliJasmine_DariGK2").position+new Vector3(2.2f,-ArthurJalurKampus.JarakKaki,0);
   var p=entry.GetComponent<TujuanJalurKampus>()??entry.gameObject.AddComponent<TujuanJalurKampus>();p.jalur=null;
   var inside=q.transform.parent.Find("Posisi kuis jalur bawah GK2");
   q.posisiJaga=new[]{entry};q.batasJalan=Array.Empty<TujuanJalurKampus>();q.tujuanGA=null;q.bolehTanpaKuisSebelumnya=true;
   q.pintuKeluar=all.Select(t=>t.GetComponent<AreaInteraksi>()).Where(a=>a!=null&&Vector3.Distance(a.transform.position,entry.position)<10&&a.daftarOpsi!=null&&a.daftarOpsi.Any(o=>o.titikTujuan!=null&&o.titikTujuan.GetComponent<TujuanJalurKampus>()?.jalur==route)).ToArray();
   if(inside!=null)UnityEngine.Object.DestroyImmediate(inside.gameObject);
   q.transform.position=entry.position;q.GetComponent<NpcYangBisaDiatur>().jalurInteraksi=null;q.GetComponent<NpcYangBisaDiatur>().jarakInteraksi=3.2f;
   q.transform.parent.name="NPC Mr. Quizzo - Depan Jalan Asrama Jasmine";q.gameObject.name="Mr. Quizzo - Depan Jasmine";
   q.sapaan="Mau ke GK2, Arthur? Sebelum lanjut, ada kuis singkat. Salah boleh coba lagi, tenang.";
   if(q.pintuKeluar.Length==0)throw new Exception("Gerbang masuk GK2 tidak ditemukan");
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  }
  File.WriteAllText("../work/chunk-20261126/quiz-setup.txt","LULUS: Quizzo hanya di depan jalan Jasmine pada scene cerita dan Explorer. Titik dan penghalang di dalam GK2 dihapus. NPC tidak ikut culling chunk.");
  AssetDatabase.SaveAssets();if(Environment.GetEnvironmentVariable("ALCS_CHAIN")!="1")EditorApplication.Exit(0);
 }catch(Exception e){File.WriteAllText("../work/chunk-20261126/setup-error.txt",e.ToString());Debug.LogException(e);EditorApplication.Exit(1);}}
}
