using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

// Memasang prefab yang sudah disetujui; tidak membangun ulang bentuk aset referensi.
public static class IntegrasiCrystalDanKelas
{
 const string Work="work/integrasi-crystal-kelas-20261112/";
 const string CrystalPrefab="Assets/Aset/Dunia/Asrama_Crystal_dan_Edel/Prefabs/Asrama Crystal - Lantai 1.prefab";
 const string ClassPrefab="Assets/Aset/Dunia/Ruang_Kelas/Prefabs/Ruangan Kelas Referensi.prefab";
 public const string CrystalScene="Assets/WorldStreaming/Kawasan_CrystalInterior.unity";
 public const string ClassScene="Assets/WorldStreaming/Kawasan_KelasDandi.unity";
 static readonly Vector3 CrystalOrigin=new Vector3(3000,0,0),ClassOrigin=new Vector3(3300,0,0);
 static T[] All<T>(Scene s)where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
 static Transform Group(string name,Transform parent=null){var t=new GameObject(name).transform;if(parent!=null)t.SetParent(parent,false);return t;}
 static Transform Destination(Transform root,string name,JalurKampus route,Vector3 feet){var t=Group(name,root);t.position=feet+Vector3.up*ArthurJalurKampus.JarakKaki;t.gameObject.AddComponent<TujuanJalurKampus>().jalur=route;return t;}
 static void Option(InteraksiJalurKampus m,string text,Transform dest){var area=m.GetComponent<AreaInteraksi>();area.idTempat=m.name;area.tipeInteraksi=TipeInteraksi.Tempat_Teleportasi;area.skenarioDialog=Array.Empty<BarisDialog>();area.daftarOpsi=new[]{new OpsiInteraksi{teksOpsi=text,titikTujuan=dest,pesanBelumTersedia=""}};area.tampilkanTombolKeluar=true;EditorUtility.SetDirty(area);}
 static InteraksiJalurKampus Bubble(InteraksiJalurKampus source,Transform root,string name,JalurKampus route,Vector3 feet,string text,Transform dest){
  var g=Object.Instantiate(source.gameObject,root);g.name=name;
  foreach(var t in g.GetComponentsInChildren<TujuanJalurKampus>(true))Object.DestroyImmediate(t.gameObject);
  var m=g.GetComponent<InteraksiJalurKampus>();m.jalur=route;m.jalurTambahan=Array.Empty<JalurKampus>();m.radius=1.15f;m.ikutiKepalaArthur=false;m.tinggiIkonDiLayar=.10f;g.transform.position=feet+Vector3.up*ArthurJalurKampus.JarakKaki;
  if(m.ikon!=null)m.ikon.position=g.transform.position+Vector3.up*.5f;Option(m,text,dest);return m;
 }
 static JalurKampus Route(Transform parent,string name,Vector3 origin,Vector3[] points,Vector3 cameraOffset){
  var t=Group(name,parent);var r=t.gameObject.AddComponent<JalurKampus>();r.diDalamRuangan=true;r.kananMengikutiUrutanTitik=true;r.radiusColliderKhusus=.22f;r.tinggiColliderKhusus=2.2f;r.bidangDekatKamera=.04f;
  r.titik=points.Select((p,i)=>{var a=Group("Titik "+i,t);a.position=origin+p+Vector3.up*ArthurJalurKampus.JarakKaki;return a;}).ToArray();r.arahKamera=new float[points.Length];
  var c=t.gameObject.AddComponent<KameraPilihanPemilik>();c.gunakan=true;c.ikutiArthur=true;c.kehalusan=6;c.sudut=65;c.acuan=r.titik[0].position;c.posisi=origin+points[0]+cameraOffset;c.bidikan=origin+points[0]+Vector3.up*1.05f;
  float yaw=Quaternion.LookRotation(c.bidikan-c.posisi).eulerAngles.y;for(int i=0;i<r.arahKamera.Length;i++)r.arahKamera[i]=yaw;
  JalurKampusInspector.Rebuild(r);return r;
 }
 static void Connect(InteraksiJalurKampus source,Transform root,JalurKampus a,Vector3 pa,JalurKampus b,Vector3 pb,string labelA,string labelB){
  var da=Destination(root,a.name+"_Kedatangan_"+b.name,a,pa);var db=Destination(root,b.name+"_Kedatangan_"+a.name,b,pb);
  void Choice(JalurKampus route,Vector3 feet,string text,Transform target,string name){var marker=root.GetComponentsInChildren<InteraksiJalurKampus>(true).FirstOrDefault(m=>m.jalur==route&&Vector3.Distance(m.transform.position,feet+Vector3.up*ArthurJalurKampus.JarakKaki)<.02f);if(marker==null)Bubble(source,root,name,route,feet,text,target);else{var area=marker.GetComponent<AreaInteraksi>();area.daftarOpsi=area.daftarOpsi.Concat(new[]{new OpsiInteraksi{teksOpsi=text,titikTujuan=target,pesanBelumTersedia=""}}).ToArray();}}
  Choice(a,pa,labelB,db,a.name+"_Simpang");Choice(b,pb,labelA,da,b.name+"_Simpang");
 }
 static void Npc(Transform root,string name,Vector3 feet,JalurKampus route,string dialog,int index){
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Aset/Karakter/NPC_Kampus/Template/NPC_Mahasiswa.prefab");var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);g.name=name;g.transform.position=feet+Vector3.up*.03f;
  var n=g.GetComponent<NpcYangBisaDiatur>();n.namaNpc=name;n.dialog=dialog;n.jalurInteraksi=route;n.jarakInteraksi=1.6f;n.Terapkan();var life=g.GetComponent<NpcKampusHidup>();if(life!=null){life.fase=index*1.7f;life.kegiatan=KegiatanNpcKampus.Mengobrol;}
  if(g.transform is RectTransform rect)rect.anchoredPosition3D=rect.localPosition;foreach(var component in g.GetComponents<Component>())PrefabUtility.RecordPrefabInstancePropertyModifications(component);
 }
 static Scene Interior(string path,string prefab,Vector3 origin){
  var s=SceneManager.GetSceneByPath(path);if(!s.isLoaded)s=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);else foreach(var root in s.GetRootGameObjects())Object.DestroyImmediate(root);
  SceneManager.SetActiveScene(s);var g=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefab),s);g.transform.position=origin;EditorSceneManager.SaveScene(s,path);return s;
 }
 static void CloseCrystalDoors(){var root=PrefabUtility.LoadPrefabContents(CrystalPrefab);try{foreach(var hinge in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Engsel")))hinge.localRotation=Quaternion.identity;PrefabUtility.SaveAsPrefabAsset(root,CrystalPrefab);}finally{PrefabUtility.UnloadPrefabContents(root);}}
 static void Foliage(Scene scene){
  var root=Group("Rimbunan hijau di luar kaca kelas");SceneManager.MoveGameObjectToScene(root.gameObject,scene);root.position=ClassOrigin;
  var vertices=new List<Vector3>();var triangles=new List<int>();var random=new System.Random(29);
  foreach(float side in new[]{-1f,1f})for(int i=0;i<18;i++){
   var center=new Vector3(side*(9.7f+(float)random.NextDouble()),1.45f+(float)random.NextDouble()*1.8f,-5.2f+i*.62f);
   int start=vertices.Count;const int rings=6,slices=10;for(int y=0;y<=rings;y++)for(int x=0;x<=slices;x++){float latitude=Mathf.PI*y/rings,longitude=Mathf.PI*2*x/slices;vertices.Add(center+Vector3.Scale(new Vector3(Mathf.Sin(latitude)*Mathf.Cos(longitude),Mathf.Cos(latitude),Mathf.Sin(latitude)*Mathf.Sin(longitude)),new Vector3(1.05f,.85f,1.15f)));}
   for(int y=0;y<rings;y++)for(int x=0;x<slices;x++){int a=start+y*(slices+1)+x,b=a+slices+1;triangles.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
  }
  var mesh=new Mesh{name="Kanopi hijau luar kelas"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();string mp="Assets/Aset/Dunia/Ruang_Kelas/Meshes/Rimbunan luar kelas.asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(mp);if(old!=null){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;EditorUtility.SetDirty(mesh);}else AssetDatabase.CreateAsset(mesh,mp);
  string materialPath="Assets/Aset/Dunia/Ruang_Kelas/Materials/Rimbunan hijau.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.SetColor("_BaseColor",new Color(.12f,.38f,.09f));material.SetFloat("_Smoothness",.05f);AssetDatabase.CreateAsset(material,materialPath);}root.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;root.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
 }
 [MenuItem("Arthur/Integrasi/Pasang Crystal dan kelas Dandi")]
 public static void Run(){
  if(EditorApplication.isPlaying)throw new Exception("Hentikan Play sebelum pemasangan.");Directory.CreateDirectory(Work+"cadangan");var original=SceneManager.GetActiveScene();var explorer=SceneManager.GetSceneByPath("Assets/Scenes/Explorer.unity");if(!explorer.isLoaded)throw new Exception("Buka Explorer.");
  // Semua edit pengguna yang sudah ada dicadangkan sebelum menyentuh sambungan.
  var paths=new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Scenes/Penataan Kampus - Jalur NPC dan Bubble.unity"};
  foreach(var path in paths){var scene=SceneManager.GetSceneByPath(path);if(scene.isLoaded)EditorSceneManager.SaveScene(scene);if(!File.Exists(Work+"cadangan/"+Path.GetFileName(path)))File.Copy(path,Work+"cadangan/"+Path.GetFileName(path));}
  CloseCrystalDoors();var crystal=Interior(CrystalScene,CrystalPrefab,CrystalOrigin);var classroom=Interior(ClassScene,ClassPrefab,ClassOrigin);Foliage(classroom);EditorSceneManager.SaveScene(classroom);SceneManager.SetActiveScene(explorer);
  foreach(var path in paths){
   var s=SceneManager.GetSceneByPath(path);bool wasOpen=s.isLoaded;if(!wasOpen)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);SceneManager.SetActiveScene(s);
   var previous=s.GetRootGameObjects().FirstOrDefault(g=>g.name=="INTERIOR CRYSTAL DAN KELAS - jalur aktif");if(previous!=null&&previous.transform.Find("NPC kelas Dandi")!=null){if(!wasOpen)EditorSceneManager.CloseScene(s,true);continue;}if(previous!=null)Object.DestroyImmediate(previous);
   var root=Group("INTERIOR CRYSTAL DAN KELAS - jalur aktif");
   var source=All<InteraksiJalurKampus>(s).Single(m=>m.name=="UNK_Crystal_KembaliBundaran");
   var deleted=All<InteraksiJalurKampus>(s).FirstOrDefault(m=>m.name=="UNK_Crystal_Depan_Bubble (1)");if(deleted!=null)Object.DestroyImmediate(deleted.gameObject);
   var lobby=Route(root,"UNK_CrystalInterior_Lobi",CrystalOrigin,new[]{new Vector3(4.19f,0,-9.1f),new Vector3(4.19f,0,-3.5f),new Vector3(0,0,-3.5f),Vector3.zero},new Vector3(-4.5f,2.05f,0));
   var left=Route(root,"UNK_CrystalInterior_LorongKiri",CrystalOrigin,new[]{new Vector3(-54,0,-.6f),new Vector3(0,0,-.6f)},new Vector3(4.5f,2,0));
   var right=Route(root,"UNK_CrystalInterior_LorongKanan",CrystalOrigin,new[]{new Vector3(0,0,-.6f),new Vector3(54,0,-.6f)},new Vector3(-4.5f,2,0));
   var branch=Route(root,"UNK_CrystalInterior_CabangT",CrystalOrigin,new[]{new Vector3(0,0,.5f),new Vector3(0,0,10.1f)},new Vector3(0,2,-4.3f));
   var arrival=Destination(root,"TitikMasuk_AsramaCrystal",lobby,CrystalOrigin+new Vector3(4.19f,0,-9.1f));var outside=Destination(root,"TitikKeluar_AsramaCrystal",source.jalur,source.transform.position-Vector3.up*ArthurJalurKampus.JarakKaki);Option(source,"Masuk ke dalam Asrama Crystal",arrival);
   Bubble(source,root,"UNK_CrystalInterior_Keluar",lobby,CrystalOrigin+new Vector3(4.19f,0,-9.1f),"Keluar dari Asrama Crystal",outside);
   Connect(source,root,lobby,CrystalOrigin,left,CrystalOrigin,"Ke lobi Crystal","Ke lorong kiri");Connect(source,root,lobby,CrystalOrigin,right,CrystalOrigin,"Ke lobi Crystal","Ke lorong kanan");Connect(source,root,lobby,CrystalOrigin,branch,CrystalOrigin+new Vector3(0,0,.5f),"Ke lobi Crystal","Ke lorong cabang");
   // Di sisi lorong tersedia sambungan lintas sayap tanpa harus kembali ke pintu masuk.
   Connect(source,root,left,CrystalOrigin,right,CrystalOrigin,"Ke lorong kiri","Ke lorong kanan");
   var crowd=Group("NPC Crystal - geser sesuai kebutuhan",root);crowd.gameObject.AddComponent<KelompokNpcManual>().kawasan="CrystalInterior";
   Npc(crowd,"Penghuni Crystal",CrystalOrigin+new Vector3(3.25f,0,-3.9f),lobby,"Selamat datang di Asrama Crystal. Kamar-kamarnya sedang ditutup.",0);
   Npc(crowd,"Mahasiswa lorong kiri",CrystalOrigin+new Vector3(-13,0,.65f),left,"Aku sedang menunggu temanku di lorong.",1);
   Npc(crowd,"Mahasiswa lorong kanan",CrystalOrigin+new Vector3(18,0,.65f),right,"Biasanya kami berkumpul di lobi setelah kelas.",2);
   Npc(crowd,"Penghuni dekat cabang",CrystalOrigin+new Vector3(-.55f,0,7.5f),branch,"Lorong ini menuju kamar penghuni. Pintunya sedang ditutup.",3);
   var door=All<InteraksiJalurKampus>(s).FirstOrDefault(m=>m.name=="GK1_Lantai3_Kiri_Bubble");
   if(door==null){var reference=All<InteraksiJalurKampus>(explorer).Single(m=>m.name=="GK1_Lantai3_Kiri_Bubble");var clone=Object.Instantiate(reference.gameObject,root);clone.name=reference.name;door=clone.GetComponent<InteraksiJalurKampus>();door.jalur=All<JalurKampus>(s).Single(r=>r.name==reference.jalur.name);door.jalurTambahan=Array.Empty<JalurKampus>();}
   var front=Route(root,"GK1_KelasDandi_Depan",ClassOrigin,new[]{new Vector3(7.6f,0,4.5f),new Vector3(0,0,4.5f)},new Vector3(0,2.4f,-4));
   var aisle=Route(root,"GK1_KelasDandi_Tengah",ClassOrigin,new[]{new Vector3(0,0,-2.7f),new Vector3(0,0,4.5f)},new Vector3(4.8f,2.4f,0));
   var enter=Destination(root,"TitikMasuk_KelasDandi",front,ClassOrigin+new Vector3(7.6f,0,4.5f));var back=Destination(root,"TitikKembali_GK1Lantai3",door.jalur,door.transform.position-Vector3.up*ArthurJalurKampus.JarakKaki);Option(door,"Masuk ke kelas",enter);
   Bubble(door,root,"GK1_KelasDandi_Keluar",front,ClassOrigin+new Vector3(7.6f,0,4.5f),"Keluar ke GK1 lantai 3",back);Connect(door,root,front,ClassOrigin+new Vector3(0,0,4.5f),aisle,ClassOrigin+new Vector3(0,0,4.5f),"Ke pintu kelas","Ke lorong tengah kelas");
   var dandi=All<NpcYangBisaDiatur>(s).FirstOrDefault(n=>n.GetComponent<KepergianDandi>()!=null);var classNpcs=Group("NPC kelas Dandi",root);classNpcs.gameObject.AddComponent<KelompokNpcManual>().kawasan="KelasDandi";
   if(dandi!=null){dandi.transform.SetParent(classNpcs,true);dandi.transform.position=ClassOrigin+new Vector3(-.85f,.03f,1.7f);dandi.name="Dandi — Dalam Kelas GK1 Lantai 3 — Geser Saya";dandi.jalurInteraksi=aisle;var leaving=dandi.GetComponent<KepergianDandi>();if(leaving.tujuanBerjalan!=null)leaving.tujuanBerjalan.position=ClassOrigin+new Vector3(-.85f,.03f,3.8f);}
   foreach(var quest in All<PerjalananDandiChapter1>(s))quest.depanKelas=Destination(root,"TitikPulih_DekatDandi",aisle,ClassOrigin+new Vector3(0,0,.8f));
   foreach(var manager in All<KawasanKampus>(s)){var entries=(manager.kawasan??Array.Empty<KawasanKampus.Kawasan>()).ToList();entries.RemoveAll(k=>k.id=="CrystalInterior"||k.id=="KelasDandi");entries.Add(new KawasanKampus.Kawasan{id="CrystalInterior",scene=CrystalScene});entries.Add(new KawasanKampus.Kawasan{id="KelasDandi",scene=ClassScene});manager.kawasan=entries.ToArray();EditorUtility.SetDirty(manager);}
   EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);if(!wasOpen)EditorSceneManager.CloseScene(s,true);
  }
  var build=EditorBuildSettings.scenes.ToList();foreach(var path in new[]{CrystalScene,ClassScene})if(!build.Any(x=>x.path==path))build.Add(new EditorBuildSettingsScene(path,true));EditorBuildSettings.scenes=build.ToArray();
  EditorSceneManager.SaveScene(crystal);EditorSceneManager.CloseScene(crystal,true);EditorSceneManager.CloseScene(classroom,true);SceneManager.SetActiveScene(original);AssetDatabase.SaveAssets();File.WriteAllText(Work+"pemasangan.txt","Crystal:4 jalur,4 NPC,semua daun pintu tertutup. Kelas:prefab100 kursi,2 jalur,Dandi dipindah beserta tujuan pergi/titik pulih quest,rimbunan di luar kaca. Explorer/CharacterLab/Penataan Kampus - Jalur NPC dan Bubble diselaraskan. Dua interior dimuat terpisah;lantai2 tidak dibuat.");
  RapikanLintasanDanPosisi();
 }
 public static void RapikanLintasanDanPosisi(){
  var original=SceneManager.GetActiveScene();foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Scenes/Penataan Kampus - Jalur NPC dan Bubble.unity"}){
   var s=SceneManager.GetSceneByPath(path);bool loaded=s.isLoaded;if(!loaded)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
   var root=s.GetRootGameObjects().Single(g=>g.name=="INTERIOR CRYSTAL DAN KELAS - jalur aktif");
   foreach(var t in root.GetComponentsInChildren<Transform>(true))if(Mathf.Abs(t.position.z)<.001f&&Mathf.Abs(t.position.x-CrystalOrigin.x)<60){var p=t.position;p.z=-.6f;t.position=p;}
   foreach(var route in root.GetComponentsInChildren<JalurKampus>(true)){if(route.name.Contains("LorongKiri")||route.name.Contains("LorongKanan")){var c=route.GetComponent<KameraPilihanPemilik>();if(Mathf.Abs(c.acuan.z)<.001f){c.acuan+=Vector3.back*.6f;c.posisi+=Vector3.back*.6f;c.bidikan+=Vector3.back*.6f;}}JalurKampusInspector.Rebuild(route);}
   foreach(var n in root.GetComponentsInChildren<NpcYangBisaDiatur>(true)){
    if(n.GetComponent<KepergianDandi>()!=null){n.transform.position=ClassOrigin+new Vector3(-.85f,.03f,1.7f);n.jalurInteraksi=root.GetComponentsInChildren<JalurKampus>().Single(r=>r.name=="GK1_KelasDandi_Tengah");}
    if(n.transform is RectTransform rect)rect.anchoredPosition3D=rect.localPosition;
    foreach(var component in n.GetComponents<Component>()){PrefabUtility.RecordPrefabInstancePropertyModifications(component);EditorUtility.SetDirty(component);}
   }
   EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);if(!loaded)EditorSceneManager.CloseScene(s,true);
  }SceneManager.SetActiveScene(original);
 }
}
