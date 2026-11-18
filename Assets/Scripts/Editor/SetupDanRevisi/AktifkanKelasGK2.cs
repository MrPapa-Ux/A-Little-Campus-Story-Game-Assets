using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class AktifkanKelasGK2
{
 const string W="work/gk2-aktif-remote-20261110/";
 static T[] All<T>(Scene s) where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
 static Transform Dest(Transform parent,string name,Vector3 pos,JalurKampus route){var t=parent.Find(name);if(t==null){t=new GameObject(name).transform;t.SetParent(parent,false);}t.position=pos;var d=t.GetComponent<TujuanJalurKampus>();if(d==null)d=t.gameObject.AddComponent<TujuanJalurKampus>();d.jalur=route;return t;}
 static void Option(InteraksiJalurKampus marker,string label,Transform dest){var a=marker.GetComponent<AreaInteraksi>();a.idTempat=marker.name;a.tipeInteraksi=TipeInteraksi.Tempat_Teleportasi;a.skenarioDialog=Array.Empty<BarisDialog>();a.daftarOpsi=new[]{new OpsiInteraksi{teksOpsi=label,titikTujuan=dest,pesanBelumTersedia="",nyalakanGrupDepan=false,nyalakanGrupBelakang=false,nyalakanGrupRuangan=false}};a.tampilkanTombolKeluar=true;EditorUtility.SetDirty(a);}
 [MenuItem("Arthur/GK2/Aktifkan kelas 1 sampai 3")]
 public static void Run(){
 Directory.CreateDirectory(W+"cadangan");var original=SceneManager.GetActiveScene();var source=SceneManager.GetSceneByPath("Assets/Scenes/Explorer.unity");
 if(!source.isLoaded||EditorApplication.isPlaying)throw new Exception("Buka Explorer, hentikan Play.");
 string[] doors={"GK2_TerasAtas_Bubble","GK2_TerasAtas_Bubble (1)","GK2_TerasAtas_Bubble (2)","GK2_JalurBelakang_Bubble","GK2_JalurBelakang_Bubble (1)"};int[] rooms={1,1,2,3,3};
 var originals=doors.Select(n=>All<InteraksiJalurKampus>(source).Single(m=>m.name==n)).ToArray();
 var report=new List<string>();
 foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Chapter1Preview/Chapter1_Kamar201.unity"}){
 var s=SceneManager.GetSceneByPath(path);bool opened=s.isLoaded;if(!opened)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
 try{
 string backup=W+"cadangan/"+Path.GetFileName(path);if(!File.Exists(backup))EditorSceneManager.SaveScene(s,backup,true);
 var root=s.GetRootGameObjects().FirstOrDefault(g=>g.name=="RUTE KELAS GK2 - aktif");if(root==null){root=new GameObject("RUTE KELAS GK2 - aktif");SceneManager.MoveGameObjectToScene(root,s);}root.tag="Untagged";
 var routes=new JalurKampus[3];
 for(int room=1;room<=3;room++){
 var r=All<JalurKampus>(s).FirstOrDefault(x=>x.name=="GK2_Kelas"+room)??All<JalurKampus>(s).Single(x=>x.name=="GK2_Kelas"+room+"_RancanganLorong");r.transform.SetParent(root.transform,true);r.gameObject.tag="Untagged";r.name="GK2_Kelas"+room;r.diDalamRuangan=true;r.kananMengikutiUrutanTitik=false;r.radiusColliderKhusus=.22f;r.tinggiColliderKhusus=2.2f;r.bidangDekatKamera=.05f;
 var cam=r.GetComponent<KameraPilihanPemilik>();if(cam==null)cam=r.gameObject.AddComponent<KameraPilihanPemilik>();cam.gunakan=true;cam.ikutiArthur=true;cam.kehalusan=5;cam.sudut=65;r.Sampel(r.Panjang*.5f,out cam.acuan,out _,out _);float floor=room==3?-2.3f:.7f;
 cam.posisi=room==3?new Vector3(-6,floor+2.45f,cam.acuan.z):new Vector3(cam.acuan.x,floor+2.45f,-48.6f);
 cam.bidikan=room==3?new Vector3(-.8f,floor+1.2f,cam.acuan.z):new Vector3(cam.acuan.x,floor+1.2f,-53.8f);
 JalurKampusInspector.Rebuild(r);routes[room-1]=r;EditorUtility.SetDirty(r);EditorUtility.SetDirty(cam);
 }
 for(int i=0;i<doors.Length;i++){
 var outside=All<InteraksiJalurKampus>(s).FirstOrDefault(m=>m.name==doors[i]);
 if(outside==null){var go=UnityEngine.Object.Instantiate(originals[i].gameObject);SceneManager.MoveGameObjectToScene(go,s);go.name=doors[i];var match=All<JalurKampus>(s).Single(r=>r.name==originals[i].jalur.name);go.transform.SetParent(match.transform.parent,true);outside=go.GetComponent<InteraksiJalurKampus>();outside.jalur=match;outside.jalurTambahan=Array.Empty<JalurKampus>();}
 var route=routes[rooms[i]-1];route.Sampel(route.JarakTerdekat(outside.transform.position),out var insidePos,out _,out _);
 var arrival=Dest(route.transform,"TitikMasuk_Kelas"+rooms[i]+"_Pintu"+(i+1),insidePos,route);
 var back=Dest(outside.transform,"TitikKembali_Teras_Pintu"+(i+1),outside.transform.position,outside.jalur);
 Option(outside,"Masuk ke kelas "+rooms[i],arrival);
 string exitName="GK2_Kelas"+rooms[i]+"_Keluar_Pintu"+(i+1);var exit=All<InteraksiJalurKampus>(s).FirstOrDefault(m=>m.name==exitName);
 if(exit==null){var go=UnityEngine.Object.Instantiate(outside.gameObject,route.transform);go.name=exitName;exit=go.GetComponent<InteraksiJalurKampus>();foreach(var child in go.GetComponentsInChildren<TujuanJalurKampus>(true))UnityEngine.Object.DestroyImmediate(child.gameObject);}
 exit.transform.position=insidePos;exit.jalur=route;exit.jalurTambahan=Array.Empty<JalurKampus>();exit.radius=1.1f;exit.ikutiKepalaArthur=false;exit.tinggiIkonDiLayar=.10f;
 if(exit.ikon!=null){exit.ikon.position=insidePos+Vector3.up*.6f;var text=exit.ikon.GetComponentInChildren<TMPro.TMP_Text>(true);if(text!=null)text.text="↓";}
 Option(exit,"Keluar ke teras GK2",back);
 if(outside.GetComponent<AreaInteraksi>().daftarOpsi[0].titikTujuan.GetComponent<TujuanJalurKampus>().jalur!=route||exit.GetComponent<AreaInteraksi>().daftarOpsi[0].titikTujuan!=back)throw new Exception("Sambungan gagal "+outside.name);
 report.Add(path+" | "+outside.name+" <-> "+route.name+" | kedatangan="+insidePos);
 }
 EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);
 }finally{if(!opened)EditorSceneManager.CloseScene(s,true);}}
 SceneManager.SetActiveScene(original);AssetDatabase.SaveAssets();File.WriteAllLines(W+"pemasangan.txt",report);
 }
}
