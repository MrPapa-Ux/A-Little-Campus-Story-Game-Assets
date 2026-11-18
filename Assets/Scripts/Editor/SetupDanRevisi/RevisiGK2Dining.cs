using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class RevisiGK2Dining
{
 const string W="work/gk2-dining-20261110/";
 static T[] All<T>(Scene s) where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
 static Transform Point(Transform parent,string name,Vector3 p){var t=new GameObject(name).transform;t.SetParent(parent,false);t.position=p;return t;}
 static GameObject Root(Scene s,string name){var found=s.GetRootGameObjects().FirstOrDefault(g=>g.name==name);if(found!=null)return found;var go=new GameObject(name);SceneManager.MoveGameObjectToScene(go,s);return go;}
 static void Figure(Scene s,Transform parent,string name,Vector3 pos,int i){
 var variants=UnityEngine.Object.FindObjectsByType<NpcKampusHidup>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(n=>n.name.StartsWith("Mahasiswa_Taman_")).OrderBy(n=>n.name).ToArray();
 var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(AlatNpcArthur.Prefab);var go=variants.Length>0?UnityEngine.Object.Instantiate(variants[i%variants.Length].gameObject):(GameObject)PrefabUtility.InstantiatePrefab(prefab,s);if(go.scene!=s)SceneManager.MoveGameObjectToScene(go,s);if(PrefabUtility.IsPartOfPrefabInstance(go))PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);go.name=name;go.transform.SetParent(parent,false);go.transform.position=pos;if(go.transform is RectTransform rect)rect.anchoredPosition3D=rect.localPosition;
 var n=go.GetComponent<NpcYangBisaDiatur>();if(n!=null)n.enabled=false;var a=go.GetComponent<AreaInteraksi>();if(a!=null)a.enabled=false;var b=go.GetComponent<NPCBubbleTrigger>();if(b!=null){b.enabled=false;if(b.bubbleChatTransform!=null)b.bubbleChatTransform.gameObject.SetActive(false);}
 foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;foreach(var c in go.GetComponentsInChildren<Collider2D>(true))c.enabled=false;
 var life=go.GetComponent<NpcKampusHidup>();life.fase=i*.73f;life.kegiatan=i%3==0?KegiatanNpcKampus.Belajar:KegiatanNpcKampus.Mengobrol;life.jarakAnimasi=32;
 var body=life.badan.GetComponent<SpriteRenderer>();if(body!=null)body.color=new[]{new Color(.72f,.87f,1),new Color(1,.8f,.65f),new Color(.82f,1,.8f),new Color(.95f,.8f,1)}[i%4];
 }

 public static void RagamFiguran(){
 var original=SceneManager.GetActiveScene();
 foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Chapter1Preview/Chapter1_Kamar201.unity"}){
 var s=SceneManager.GetSceneByPath(path);bool loaded=s.isLoaded;if(!loaded)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
 try{var existing=All<NpcKampusHidup>(s).Where(n=>n.name.StartsWith("Mahasiswa kelas 1 - ")||n.name.StartsWith("Mahasiswa Dining - ")).ToArray();
 foreach(var n in existing){var parent=n.transform.parent;var pos=n.transform.position;var name=n.name;int index=int.Parse(name.Substring(name.LastIndexOf(' ')+1))-1;UnityEngine.Object.DestroyImmediate(n.gameObject);Figure(s,parent,name,pos,index);}
 var walkers=All<PelariSekilasNpc>(s).Where(p=>p.name.StartsWith("Dining berjalan bolak-balik - ")).OrderBy(p=>p.name).ToArray();for(int i=0;i<walkers.Length;i++){float x=new[]{1150.4f,1151.2f,1156f}[i];var a=walkers[i].titikAwal.position;var b=walkers[i].titikAkhir.position;a.x=b.x=x;walkers[i].titikAwal.position=a;walkers[i].titikAkhir.position=b;}
 EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);
 }finally{if(!loaded)EditorSceneManager.CloseScene(s,true);}}
 SceneManager.SetActiveScene(original);}

 [MenuItem("Arthur/Revisi GK2 dan Dining/Lengkapi ruangan 2 dan 3")]
 public static void RuanganDuaTiga(){
 const string folder="work/gk2-kelas23-20261110/";
 Directory.CreateDirectory(folder+"cadangan");var original=SceneManager.GetActiveScene();var log=new List<string>();
 foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Chapter1Preview/Chapter1_Kamar201.unity"}){
 var scene=SceneManager.GetSceneByPath(path);bool loaded=scene.isLoaded;if(!loaded)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
 try{
 var backup=folder+"cadangan/"+Path.GetFileName(path);if(!File.Exists(backup))EditorSceneManager.SaveScene(scene,backup,true);
 var drafts=Root(scene,"RANCANGAN GK2 - masuk tiap bubble - belum disambungkan");drafts.tag="EditorOnly";
 for(int room=2;room<=3;room++){
 float floor=room==2?.7f:-2.3f;float y=floor+ArthurJalurKampus.JarakKaki;
 string key="GK2_Kelas"+room+"_RancanganLorong";var existing=drafts.transform.Find(key);JalurKampus route;
 if(existing==null){var go=new GameObject(key);go.transform.SetParent(drafts.transform,false);route=go.AddComponent<JalurKampus>();route.diDalamRuangan=true;route.kananMengikutiUrutanTitik=true;
 Vector3[] pts=room==2?new[]{new Vector3(36.32f,y,-53.8f),new Vector3(40,y,-53.8f),new Vector3(44.3f,y,-53.8f)}:new[]{new Vector3(-.8f,y,-54.8f),new Vector3(-.8f,y,-46),new Vector3(-.8f,y,-37.2f)};
 route.titik=pts.Select((p,i)=>Point(go.transform,(i+1).ToString("00")+" - Ruangan "+room+" - geser untuk rapikan",p)).ToArray();route.arahKamera=new[]{room==2?180f:90f,room==2?180f:90f,room==2?180f:90f};
 var t=Point(go.transform,"Titik Masuk Ruangan "+room+" - rancangan di dalam",pts[0]);t.gameObject.AddComponent<TujuanJalurKampus>().jalur=route;
 }else route=existing.GetComponent<JalurKampus>();
 var crowd=Root(scene,"NPC_Kelas"+room+"_GK2");if(crowd.GetComponent<KelompokNpcManual>()==null)crowd.AddComponent<KelompokNpcManual>().kawasan="JasmineGK2";
 int count=room==2?6:8;if(crowd.transform.childCount==0)for(int i=0;i<count;i++){
 var pos=room==2?new Vector3(37.8f+(i%3)*2.7f,floor+.025f,-50.5f+(i/3)*2.5f):new Vector3(-3.5f-(i%2)*3.7f,floor+.025f,-52.4f+(i/2)*4.5f);
 Figure(scene,crowd.transform,"Mahasiswa ruangan "+room+" - "+(i+1),pos,i+room*3);
 }
 if(route.titik.Length!=3||crowd.transform.childCount!=count)throw new Exception("Jumlah titik/NPC salah untuk "+key);
 if(route.titik.Any(t=>Mathf.Abs(t.position.y-y)>.001f))throw new Exception("Tinggi jalur salah: "+key);
 log.Add(path+" | ruangan="+room+" | lantai="+floor+" | titik=3 | NPC="+count+" | EditorOnly, belum disambungkan");
 }
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
 }finally{if(!loaded)EditorSceneManager.CloseScene(scene,true);}
 }
 SceneManager.SetActiveScene(original);AssetDatabase.SaveAssets();File.WriteAllLines(folder+"hasil.txt",log);
 }
 [MenuItem("Arthur/Revisi GK2 dan Dining/Terapkan rancangan")]
 public static void Run(){
 Directory.CreateDirectory(W+"cadangan");var original=SceneManager.GetActiveScene();var source=SceneManager.GetSceneByPath("Assets/Scenes/Explorer.unity");
 if(!source.isLoaded)throw new Exception("Buka Explorer yang memuat penanda pemilik.");
 var marks=All<InteraksiJalurKampus>(source).Where(m=>m.name.StartsWith("GK2_TerasAtas_Bubble")||m.name.StartsWith("GK2_JalurBelakang_Bubble")).OrderBy(m=>m.name).ToArray();
 var pelintasSource=All<PelariSekilasNpc>(source).First(p=>p.name=="Pelintas_UNK_Dining_Dalam");
 var positions=marks.Select(m=>m.transform.position).ToArray();var names=marks.Select(m=>m.name).ToArray();var log=new List<string>();
 foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Chapter1Preview/Chapter1_Kamar201.unity"}){
 var s=SceneManager.GetSceneByPath(path);bool loaded=s.isLoaded;if(!loaded)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
 try{
 // Save a copy including the owner's unsaved marker edits, never discard them.
 var backup=W+"cadangan/"+Path.GetFileName(path);if(!File.Exists(backup))EditorSceneManager.SaveScene(s,backup,true);
 var ns=All<NpcYangBisaDiatur>(s);var semmy=ns.Single(n=>n.namaNpc.Contains("Semmy"));var jimmy=ns.Single(n=>n.namaNpc.Contains("Jimmy"));
 string flag=W+Path.GetFileNameWithoutExtension(path)+"-swap.done";
 if(!File.Exists(flag)){var p=semmy.transform.position;semmy.transform.position=jimmy.transform.position;jimmy.transform.position=p;foreach(var n in new[]{semmy,jimmy}){var r=n.jalurInteraksi;r.Sampel(r.JarakTerdekat(n.transform.position),out var q,out _,out _);n.geserTitikInteraksi=q-Vector3.up*ArthurJalurKampus.JarakKaki-n.transform.position;PrefabUtility.RecordPrefabInstancePropertyModifications(n.transform);PrefabUtility.RecordPrefabInstancePropertyModifications(n);}}
 var drafts=Root(s,"RANCANGAN GK2 - masuk tiap bubble - belum disambungkan");drafts.tag="EditorOnly";
 for(int i=0;i<marks.Length;i++){
 string key="GK2_DrafMasuk_"+(i+1).ToString("00")+" - "+names[i];if(drafts.transform.Find(key)!=null)continue;
 var go=new GameObject(key);go.transform.SetParent(drafts.transform,false);var r=go.AddComponent<JalurKampus>();r.diDalamRuangan=true;r.kananMengikutiUrutanTitik=true;r.arahKamera=new[]{180f,180f};r.titik=new[]{Point(go.transform,"01 - dekat pintu",positions[i]),Point(go.transform,"02 - masuk ruangan - geser saya",positions[i]+(names[i].Contains("TerasAtas")?Vector3.forward:Vector3.right)*2f)};
 }
 if(drafts.transform.Find("GK2_Kelas1_RancanganLorong") ==null){var go=new GameObject("GK2_Kelas1_RancanganLorong");go.transform.SetParent(drafts.transform,false);var r=go.AddComponent<JalurKampus>();r.diDalamRuangan=true;r.kananMengikutiUrutanTitik=true;r.arahKamera=new[]{180f,180f,180f};r.titik=new[]{Point(go.transform,"01 - sisi pintu kiri",new Vector3(16.28f,.7f+ArthurJalurKampus.JarakKaki,-53.8f)),Point(go.transform,"02 - tengah - atur sesuai meja",new Vector3(24,.7f+ArthurJalurKampus.JarakKaki,-53.8f)),Point(go.transform,"03 - sisi pintu satunya",new Vector3(32.19f,.7f+ArthurJalurKampus.JarakKaki,-53.8f))};}
 var classroom=Root(s,"NPC_Kelas1_GK2");if(classroom.GetComponent<KelompokNpcManual>()==null)classroom.AddComponent<KelompokNpcManual>().kawasan="JasmineGK2";
 if(classroom.transform.childCount==0)for(int i=0;i<8;i++)Figure(s,classroom.transform,"Mahasiswa kelas 1 - "+(i+1),new Vector3(18.3f+(i%4)*3.7f,.725f,-50.5f+(i/4)*2.5f),i);
 var dining=Root(s,"Keramaian_Interior_Dining");var gate=dining.GetComponent<KeramaianDining>();if(gate==null)gate=dining.AddComponent<KeramaianDining>();gate.jalur=All<JalurKampus>(s).Single(r=>r.name=="UNK_Dining_Dalam");
 if(gate.isi==null){gate.isi=new GameObject("Figuran - hanya saat di dalam Dining");gate.isi.transform.SetParent(dining.transform,false);for(int i=0;i<12;i++)Figure(s,gate.isi.transform,"Mahasiswa Dining - "+(i+1),new Vector3(i%2==0?1149.8f:1157.5f,8.04f,-.5f+(i/2)*8.5f),i);
 var template=All<PelariSekilasNpc>(s).FirstOrDefault(p=>p.name=="Pelintas_UNK_Dining_Dalam")??pelintasSource;for(int i=0;i<3;i++){var go=UnityEngine.Object.Instantiate(template.gameObject,gate.isi.transform);go.name="Dining berjalan bolak-balik - "+(i+1);var p=go.GetComponent<PelariSekilasNpc>();p.jalurKhusus=gate.jalur;p.jedaAwal=1+i*4;p.jedaMinimum=2;p.jedaMaksimum=5;p.bolakBalik=true;p.peluangLari=0;p.peluangJalanUnik=0;float x=new[]{1150.4f,1151.2f,1156f}[i];p.titikAwal.position=new Vector3(x,8.04f,-3+i*6);p.titikAkhir.position=new Vector3(x,8.04f,18+i*9);}}
 EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);File.WriteAllText(flag,"Semmy dan Jimmy ditukar, jangan tukar ulang otomatis.");log.Add(path+": swap, 5 draf masuk, 1 draf lorong kelas 1, 8 NPC kelas, 12 figuran + 3 pelintas Dining.");
 }finally{if(!loaded)EditorSceneManager.CloseScene(s,true);}
 }
 SceneManager.SetActiveScene(original);AssetDatabase.SaveAssets();File.WriteAllLines(W+"hasil.txt",log);
 }
}
