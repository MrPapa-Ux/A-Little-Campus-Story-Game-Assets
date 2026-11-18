using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
[InitializeOnLoad]
public static class RapikanTampilanRuanganGK1
{
 const string Work="work/ruangan-dekan-20261109/tampilan/";
 static RapikanTampilanRuanganGK1(){EditorApplication.update+=Tick;}
 static T[] All<T>(Scene s) where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
 static void Tick(){if(!File.Exists(Work+"rapikan.request")||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;File.Delete(Work+"rapikan.request");try{Run();}catch(Exception e){File.WriteAllText(Work+"hasil.txt",e.ToString());Debug.LogException(e);}}
 public static void Run()
 {
  var active=SceneManager.GetActiveScene();var log=new List<string>();Directory.CreateDirectory(Work+"cadangan");
  foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Chapter1Preview/Chapter1_Kamar201.unity"})
  {
   var s=SceneManager.GetSceneByPath(path);bool opened=s.IsValid()&&s.isLoaded;if(!opened)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
   try{
    if(!File.Exists(Work+"cadangan/"+Path.GetFileName(path)))EditorSceneManager.SaveScene(s,Work+"cadangan/"+Path.GetFileName(path),true);
    foreach(var p in All<ArthurJalurKampus>(s))if(p.GetComponent<UkuranArthurPadaJalur>()==null)p.gameObject.AddComponent<UkuranArthurPadaJalur>();
    foreach(var r in All<JalurKampus>(s).Where(r=>r.name=="GK1_RuanganDekanFilkom"||r.name=="GK1_RuanganDosen"))
    {
     r.ukuranVisualArthur=.65f;r.tinggiColliderKhusus=2f;r.radiusColliderKhusus=.22f;
     // Keep the owner's camera position; the new editor allows a direct Scene-view capture.
     foreach(var m in r.GetComponentsInChildren<InteraksiJalurKampus>(true)){
      m.ikutiKepalaArthur=false;m.tinggiIkonDiLayar=.11f;m.jarakDariKepala=.14f;m.radius=1.1f;m.waktuHalusDiLayar=0;
      if(m.ikon!=null)m.ikon.position=m.transform.position+Vector3.up*.3f;EditorUtility.SetDirty(m);
     }
     foreach(var n in r.GetComponentsInChildren<NpcYangBisaDiatur>(true)){
      if(!n.name.Contains(" - FILKOM"))continue;
      n.transform.localScale=Vector3.one*.65f;PrefabUtility.RecordPrefabInstancePropertyModifications(n.transform);
     }
     EditorUtility.SetDirty(r);
    }
    EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);log.Add(path+": Arthur ruangan 65%, collider 2 m, bubble keluar 11% layar; pengaturan kamera pemilik dipertahankan.");
   }finally{if(!opened)EditorSceneManager.CloseScene(s,true);}
  }
  if(active.IsValid()&&active.isLoaded)SceneManager.SetActiveScene(active);AssetDatabase.SaveAssets();File.WriteAllLines(Work+"hasil.txt",log);AturRuanganGK1.Open();
 }
}
