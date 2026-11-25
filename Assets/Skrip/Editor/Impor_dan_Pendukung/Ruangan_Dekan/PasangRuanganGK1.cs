using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-shot, additive setup: preserves the two door markers positioned by the owner.
[InitializeOnLoad]
public static class PasangRuanganGK1
{
 const string Work="work/ruangan-dekan-20261109/";
 static PasangRuanganGK1(){EditorApplication.update+=Tick;}
 static T[] All<T>(Scene s) where T:Component => s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
 static void Tick(){if(!File.Exists(Work+"pasang.request")||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;File.Delete(Work+"pasang.request");try{Run();}catch(Exception e){File.WriteAllText(Work+"pasang-hasil.txt",e.ToString());Debug.LogException(e);}}
 static Transform Point(Transform parent,string name,Vector3 local){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=local;return t;}
 static Transform Destination(Transform parent,string name,Vector3 world,JalurKampus route){var t=Point(parent,name,Vector3.zero);t.position=world;t.gameObject.AddComponent<TujuanJalurKampus>().jalur=route;return t;}
 static void Option(InteraksiJalurKampus marker,string label,Transform destination){var a=marker.GetComponent<AreaInteraksi>();a.idTempat=marker.name;a.tipeInteraksi=TipeInteraksi.Tempat_Teleportasi;a.daftarOpsi=new[]{new OpsiInteraksi{teksOpsi=label,titikTujuan=destination,nyalakanGrupDepan=false,nyalakanGrupBelakang=false,nyalakanGrupRuangan=false}};a.skenarioDialog=Array.Empty<BarisDialog>();a.tampilkanTombolKeluar=true;EditorUtility.SetDirty(a);}
 static InteraksiJalurKampus Copy(InteraksiJalurKampus template,Transform parent,string name,Vector3 position,JalurKampus route){var go=UnityEngine.Object.Instantiate(template.gameObject,parent);go.name=name;go.transform.position=position;var m=go.GetComponent<InteraksiJalurKampus>();m.jalur=route;m.jalurTambahan=Array.Empty<JalurKampus>();m.ikutiKepalaArthur=false;m.radius=1.15f;m.ikon.localScale=m.ukuranIkon;m.ikon.position=position+Vector3.up*.9f;foreach(var c in go.GetComponents<Collider2D>())UnityEngine.Object.DestroyImmediate(c);return m;}
 static void Room(Scene scene,InteraksiJalurKampus door,string roomName,string path,Vector3 offset,bool dean){
  string key=dean?"GK1_RuanganDekanFilkom":"GK1_RuanganDosen";
  if(All<JalurKampus>(scene).Any(r=>r.name==key))throw new Exception(key+" sudah ada; tidak ditimpa agar penataanmu aman.");
  var lobby=door.jalur;if(lobby==null)throw new Exception("Jalur pintu belum diisi.");
  door.name=dean?"GK1_Masuk_RuanganDekanFilkom":"GK1_Masuk_RuanganDosen";
  var group=new GameObject(key);SceneManager.MoveGameObjectToScene(group,scene);group.transform.position=offset;
  var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path),scene);model.transform.SetParent(group.transform,false);model.transform.localPosition=Vector3.zero;model.name=roomName;
  var route=group.AddComponent<JalurKampus>();route.diDalamRuangan=true;route.kananMengikutiUrutanTitik=true;route.arahKamera=new[]{0f,0f};
  float x=dean ? .25f : -1.85f;float first=dean?-3.1f:-4.2f;float last=dean?2.8f:3.4f;
  route.titik=new[]{Point(group.transform,"Titik_01_Keluar",new Vector3(x,ArthurJalurKampus.JarakKaki,first)),Point(group.transform,"Titik_02_Dalam",new Vector3(x,ArthurJalurKampus.JarakKaki,last))};
  route.radiusColliderKhusus=.22f;route.sudutPandangan=76;route.bidangDekatKamera=.05f;route.kehalusanKamera=5;
  route.posisiKameraUjung=Point(group.transform,"Kamera - geser untuk atur sudut",new Vector3(x,2.7f,dean?-5.45f:-5.95f));
  route.bidikanKameraUjung=Point(group.transform,"Bidikan kamera - geser untuk atur arah",new Vector3(x,1.2f,.4f));route.kameraTetap=true;
  JalurKampusInspector.Rebuild(route);
  var masuk=Destination(group.transform,"Titik Masuk - "+roomName,offset+new Vector3(x,ArthurJalurKampus.JarakKaki,dean?-1.2f:-2.5f),route);
  var balik=Destination(door.transform,"Titik Kembali - Lobby GK1",door.transform.position,lobby);
  Option(door,"Masuk ke "+roomName,masuk);
  var keluar=Copy(door,group.transform,"GK1_Keluar_"+(dean?"RuanganDekanFilkom":"RuanganDosen"),route.titik[0].position,route);Option(keluar,"Kembali ke lobby GK1",balik);
  var text=keluar.ikon.GetComponentInChildren<TMPro.TMP_Text>(true);if(text!=null)text.text="↓";
  // Local lights stay editable and cast no additional real-time shadows.
  var light=Point(group.transform,"Lampu ruangan - boleh digeser",new Vector3(0,3.05f,0)).gameObject.AddComponent<Light>();light.type=LightType.Point;light.range=12;light.intensity=1.1f;light.color=new Color(1,.95f,.86f);light.shadows=LightShadows.None;
  EditorUtility.SetDirty(door);EditorSceneManager.MarkSceneDirty(scene);
 }
 [MenuItem("Arthur/Ruangan GK1/Pasang dua pintu yang sudah ditandai")]
 public static void Run(){
  var original=SceneManager.GetActiveScene();var source=SceneManager.GetSceneByPath("Assets/Scenes/Explorer.unity");
  if(!source.IsValid()||!source.isLoaded)throw new Exception("Buka Explorer yang berisi dua titik pintu buatanmu terlebih dahulu.");
  var doors=All<InteraksiJalurKampus>(source).Where(m=>m.name=="GK1_LobbyTengah_Bubble"||m.name=="GK1_LobbyTengah_Bubble (1)").OrderBy(m=>m.name).ToArray();
  if(doors.Length!=2)throw new Exception("Harus ada tepat dua titik GK1_LobbyTengah_Bubble yang ditandai. Tidak ada titik yang diganti.");
  var templates=new[]{doors[0],doors[1]};var positions=doors.Select(d=>d.transform.position).ToArray();
  var log=new List<string>();Directory.CreateDirectory(Work+"cadangan-pintu");
  foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Scenes/Penataan Kampus - Jalur NPC dan Bubble.unity","Assets/Scenes/Chapter1_Arthur.unity"}){
   if(!File.Exists(path))continue;var s=SceneManager.GetSceneByPath(path);bool already=s.IsValid()&&s.isLoaded;if(!already)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
   try{
    var lobby=All<JalurKampus>(s).FirstOrDefault(r=>r.name=="GK1_LobbyTengah");if(lobby==null){log.Add(path+": jalur GK1 tidak ada, dilewati.");continue;}
    if(All<JalurKampus>(s).Any(r=>r.name=="GK1_RuanganDekanFilkom")){log.Add(path+": ruangan sudah ada, tidak ditimpa.");continue;}
    var backup=Work+"cadangan-pintu/"+Path.GetFileName(path);if(!File.Exists(backup))EditorSceneManager.SaveScene(s,backup,true);
    var current=path==source.path?templates:Enumerable.Range(0,2).Select(i=>Copy(templates[i],lobby.transform.parent,"Pintu Sementara "+i,positions[i],lobby)).ToArray();
    Room(s,current[0],"Ruangan Dekan Filkom","Assets/Aset/Dunia/Ruang_Dekan/Prefabs/Ruangan Dekan.prefab",new Vector3(2300,0,-50),true);
    Room(s,current[1],"Ruangan Dosen","Assets/Aset/Dunia/Ruang_Dosen/Prefabs/Ruangan Dosen.prefab",new Vector3(2340,0,-50),false);
    EditorSceneManager.SaveScene(s);log.Add(path+": dua pintu masuk + dua pintu kembali tersambung; model dan titik bisa digeser.");
   }finally{if(!already)EditorSceneManager.CloseScene(s,true);}
  }
  if(original.IsValid()&&original.isLoaded)SceneManager.SetActiveScene(original);
  Selection.activeGameObject=templates[0].gameObject;SceneView.RepaintAll();AssetDatabase.SaveAssets();File.WriteAllLines(Work+"pasang-hasil.txt",log);
 }
}

