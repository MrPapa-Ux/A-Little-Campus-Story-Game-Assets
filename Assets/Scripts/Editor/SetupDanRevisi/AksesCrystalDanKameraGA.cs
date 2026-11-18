using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
public static class AksesCrystalDanKameraGA
{
 const string Work="work/ga-lantai1-dan-akses-crystal-20261112/";
 static Transform Point(Transform parent,string name,Vector3 position){var t=new GameObject(name).transform;t.SetParent(parent,false);t.position=position;return t;}
 public static void Run(){
  if(EditorApplication.isPlaying)throw new Exception("Hentikan Play.");Directory.CreateDirectory(Work+"cadangan");var original=SceneManager.GetActiveScene();
  foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Chapter1Preview/Chapter1_Kamar201.unity"}){
   var scene=SceneManager.GetSceneByPath(path);bool loaded=scene.isLoaded;if(!loaded)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
   if(!File.Exists(Work+"cadangan/"+Path.GetFileName(path)))EditorSceneManager.SaveScene(scene,Work+"cadangan/"+Path.GetFileName(path),true);
   var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();var banned=all.FirstOrDefault(t=>t.name=="RUTE CRYSTAL LANTAI 2");
   if(banned!=null){foreach(var area in all.Where(t=>t!=null&&!t.IsChildOf(banned)).Select(t=>t.GetComponent<AreaInteraksi>()).Where(a=>a!=null))if(area.daftarOpsi!=null)area.daftarOpsi=area.daftarOpsi.Where(o=>o.titikTujuan==null||!o.titikTujuan.IsChildOf(banned)).ToArray();foreach(var manager in all.Select(t=>t.GetComponent<KawasanKampus>()).Where(k=>k!=null))if(manager.titikMulaiPengujian!=null&&manager.titikMulaiPengujian.IsChildOf(banned))manager.titikMulaiPengujian=null;Object.DestroyImmediate(banned.gameObject);}
   var routes=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<JalurKampus>(true)).ToArray();var lower=routes.Single(r=>r.name=="GA_LorongTanggaLobby");var upper=routes.Single(r=>r.name=="GA_LorongTengah_LantaiDua");var wide=routes.Single(r=>r.name=="GA_JalanDepan");string wideBefore=EditorJsonUtility.ToJson(wide);var finalPosition=lower.posisiKameraUjung.position;var finalTarget=lower.bidikanKameraUjung.position;
   var previous=lower.transform.Find("Kamera samping lantai 1 - acuan sinematik lantai 2");if(previous!=null)Object.DestroyImmediate(previous.gameObject);var group=new GameObject("Kamera samping lantai 1 - acuan sinematik lantai 2").transform;group.SetParent(lower.transform,false);var delta=lower.titik[0].position-upper.titik[0].position;
   lower.bingkaiLokal=(upper.bingkaiLokal??Array.Empty<JalurKampus.BingkaiLokal>()).Select((b,i)=>new JalurKampus.BingkaiLokal{pusat=Point(group,"Pusat "+i,b.pusat.position+delta),posisiKamera=Point(group,"Posisi samping "+i,b.posisiKamera.position+delta),bidikan=Point(group,"Bidikan samping "+i,b.bidikan.position+delta),radius=b.radius,radiusPenuh=b.radiusPenuh}).ToArray();
   foreach(var frame in lower.bingkaiLokal){frame.posisiKamera.position=frame.pusat.position+new Vector3(2,.34467f,2.5f);frame.bidikan.position=frame.pusat.position+new Vector3(0,-.66533f,0);}
   if(lower.bingkaiLokal.Length==0)throw new Exception("Kamera acuan lantai dua belum tersedia.");
   if(wideBefore!=EditorJsonUtility.ToJson(wide)||lower.posisiKameraUjung.position!=finalPosition||lower.bidikanKameraUjung.position!=finalTarget)throw new Exception("Bingkai lebar depan berubah.");EditorUtility.SetDirty(lower);
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);if(!loaded)EditorSceneManager.CloseScene(scene,true);
  }SceneManager.SetActiveScene(original);AssetDatabase.SaveAssets();
 }
}
