using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
[InitializeOnLoad] public static class RuanganGK1Probe {
 static RuanganGK1Probe(){EditorApplication.update+=Tick;}
 static void Tick(){if(!File.Exists("work/ruangan-dekan-20261109/probe.request")||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
 File.Delete("work/ruangan-dekan-20261109/probe.request");
 var rows=new System.Collections.Generic.List<string>();
 for(int i=0;i<SceneManager.sceneCount;i++){var s=SceneManager.GetSceneAt(i);rows.Add("SCENE "+s.path+" dirty="+s.isDirty);foreach(var t in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Where(t=>t.name.Contains("GK1_LobbyTengah"))) {rows.Add(t.name+" pos="+t.position+" id="+t.GetInstanceID());var m=t.GetComponent<InteraksiJalurKampus>();if(m!=null)rows.Add(" route="+m.jalur?.name+" opts="+string.Join(";",m.GetComponent<AreaInteraksi>().daftarOpsi.Select(o=>o.teksOpsi)));}}
 File.WriteAllLines("work/ruangan-dekan-20261109/probe.txt",rows); }
}
