using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Menu sederhana: pilih lampu/bubble, geser dengan W, simpan bila sedang Play.
[InitializeOnLoad]
public static class AturMalamGAMenu
{
 const string Key="Arthur.GAMalam.Penataan";
 [Serializable] class Titik { public string id; public Vector3 posisi,skala; public Quaternion rotasi; public float intensitas,jarak,sudut; public Color warna; public bool lampu; }
 [Serializable] class Simpan { public Titik[] titik; }
 static bool penanda=true;
 static AturMalamGAMenu(){EditorApplication.playModeStateChanged+=SaatModeBerubah;SceneView.duringSceneGui+=Gambar;}
 static MalamGAChapter1 Config()=>UnityEngine.Object.FindFirstObjectByType<MalamGAChapter1>(FindObjectsInactive.Include);
 static void Pilih(UnityEngine.Object o){if(o==null)return;Selection.activeObject=o;EditorGUIUtility.PingObject(o);Tools.current=Tool.Move;SceneView.lastActiveSceneView?.FrameSelected();}
 [MenuItem("Arthur/Atur Malam GA/01 Pilih bubble ujung lobby")]
 static void Bubble(){var c=Config();if(c!=null&&c.titikBubbleLobbyMalam!=null)Pilih(c.titikBubbleLobbyMalam.gameObject);}
 [MenuItem("Arthur/Atur Malam GA/02 Pilih semua lampu")]
 static void Lampu(){var c=Config();if(c!=null&&c.grupCahaya!=null){Selection.objects=c.grupCahaya.GetComponentsInChildren<Light>(true).Select(l=>(UnityEngine.Object)l.gameObject).ToArray();SceneView.lastActiveSceneView?.FrameSelected();}}
 [MenuItem("Arthur/Atur Malam GA/03 Tampilkan atau sembunyikan penanda")]
 static void Toggle(){penanda=!penanda;SceneView.RepaintAll();}
 [MenuItem("Arthur/Atur Malam GA/04 Simpan penataan dari Play")]
 static void SimpanPlay(){
  var c=Config();if(c==null)return;
  if(!EditorApplication.isPlaying){EditorSceneManager.MarkSceneDirty(c.gameObject.scene);EditorSceneManager.SaveScene(c.gameObject.scene);return;}
  var ts=c.grupCahaya.GetComponentsInChildren<Light>(true).Select(l=>l.transform).Concat(new[]{c.titikBubbleLobbyMalam}).Where(t=>t!=null);
  var data=new Simpan{titik=ts.Select(t=>{var l=t.GetComponent<Light>();return new Titik{id=GlobalObjectId.GetGlobalObjectIdSlow(t).ToString(),posisi=t.position,rotasi=t.rotation,skala=t.localScale,lampu=l!=null,intensitas=l!=null?l.intensity:0,jarak=l!=null?l.range:0,sudut=l!=null?l.spotAngle:0,warna=l!=null?l.color:Color.white};}).ToArray()};
  SessionState.SetString(Key,JsonUtility.ToJson(data));EditorApplication.isPlaying=false;
 }
 static void SaatModeBerubah(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredEditMode)return;string json=SessionState.GetString(Key,"");if(string.IsNullOrEmpty(json))return;SessionState.EraseString(Key);var data=JsonUtility.FromJson<Simpan>(json);foreach(var p in data.titik){if(!GlobalObjectId.TryParse(p.id,out var id))continue;var t=GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as Transform;if(t==null)continue;Undo.RecordObject(t,"Atur malam GA");t.SetPositionAndRotation(p.posisi,p.rotasi);t.localScale=p.skala;var l=t.GetComponent<Light>();if(p.lampu&&l!=null){Undo.RecordObject(l,"Atur lampu GA");l.intensity=p.intensitas;l.range=p.jarak;l.spotAngle=p.sudut;l.color=p.warna;EditorUtility.SetDirty(l);}EditorSceneManager.MarkSceneDirty(t.gameObject.scene);}var c=Config();if(c!=null)EditorSceneManager.SaveScene(c.gameObject.scene);Debug.Log("Penataan lampu dan bubble malam GA tersimpan. Mode Play dihentikan.");}
 static void Gambar(SceneView view){if(!penanda)return;var c=Config();if(c==null)return;var lights=c.grupCahaya!=null?c.grupCahaya.GetComponentsInChildren<Light>(true):Array.Empty<Light>();foreach(var l in lights)Tombol(l.transform,l.name,Color.yellow);if(c.titikBubbleLobbyMalam!=null)Tombol(c.titikBubbleLobbyMalam,"BUBBLE MALAM — GESER TITIK INI",Color.cyan);}
 static void Tombol(Transform t,string nama,Color warna){Handles.color=warna;float size=HandleUtility.GetHandleSize(t.position)*.10f;if(Handles.Button(t.position,Quaternion.identity,size,size,Handles.SphereHandleCap))Pilih(t.gameObject);Handles.Label(t.position+Vector3.up*size*1.8f,nama);}
}
