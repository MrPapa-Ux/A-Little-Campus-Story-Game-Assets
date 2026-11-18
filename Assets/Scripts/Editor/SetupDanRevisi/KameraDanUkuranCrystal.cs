using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
public static class KameraDanUkuranCrystal
{
 public static void Run(){
  if(EditorApplication.isPlaying)throw new Exception("Hentikan Play dahulu.");
  const string work="work/kamera-crystal-20261112/";Directory.CreateDirectory(work+"cadangan");var original=SceneManager.GetActiveScene();
  foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Chapter1Preview/Chapter1_Kamar201.unity"}){
   var scene=SceneManager.GetSceneByPath(path);bool opened=scene.isLoaded;if(!opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
   if(!File.Exists(work+"cadangan/"+Path.GetFileName(path)))EditorSceneManager.SaveScene(scene,work+"cadangan/"+Path.GetFileName(path),true);
   var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
   foreach(var route in all.Where(t=>t!=null).Select(t=>t.GetComponent<JalurKampus>()).Where(r=>r!=null&&r.name.StartsWith("UNK_CrystalInterior_")).ToArray()){
    route.ukuranVisualArthur=.72f;route.kananMengikutiUrutanTitik=false;var camera=route.GetComponent<KameraPilihanPemilik>();camera.gunakan=true;camera.ikutiArthur=true;camera.menyampingJalur=true;camera.balikSisi=true;camera.jarakSamping=5.4f;camera.tinggiSamping=2.25f;camera.tinggiFokusSamping=.7f;camera.sudut=55;camera.kehalusan=7;
    if(route.name=="UNK_CrystalInterior_LorongKanan"){
     foreach(var t in route.titik)Object.DestroyImmediate(t.gameObject);
     route.titik=new[]{new Vector3(0,0,-.6f),new Vector3(10.5f,0,-.6f),new Vector3(11.3f,0,0),new Vector3(54,0,0)}.Select((p,i)=>{var t=new GameObject("Titik "+i).transform;t.SetParent(route.transform,false);t.position=new Vector3(3000,ArthurJalurKampus.JarakKaki,0)+p;return t;}).ToArray();JalurKampusInspector.Rebuild(route);
    }
    // Cadangan Editor dan arah sprite tetap sesuai bidang kamera menyamping.
    route.Sampel(0,out var point,out var direction,out _);var feet=point-Vector3.up*ArthurJalurKampus.JarakKaki;camera.acuan=point;camera.posisi=feet-Vector3.Cross(Vector3.up,direction.normalized)*5.4f+Vector3.up*2.25f;camera.bidikan=feet+Vector3.up*.7f;EditorUtility.SetDirty(camera);EditorUtility.SetDirty(route);
   }
   var group=all.First(t=>t!=null&&t.name=="NPC Crystal - geser sesuai kebutuhan");foreach(var npc in group.GetComponentsInChildren<NpcYangBisaDiatur>(true)){npc.transform.localScale=Vector3.one*.72f;PrefabUtility.RecordPrefabInstancePropertyModifications(npc.transform);EditorUtility.SetDirty(npc.transform);}
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);if(!opened)EditorSceneManager.CloseScene(scene,true);
  }
  SceneManager.SetActiveScene(original);Finishing();AssetDatabase.SaveAssets();
 }
 static void Finishing(){
  const string root="Assets/Asrama Crystal Edel Referensi";
  var vertices=new System.Collections.Generic.List<Vector3>();var indices=new System.Collections.Generic.List<int>();
  void Box(Vector3 center,Vector3 size){int start=vertices.Count;foreach(var corner in new[]{new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1)})vertices.Add(center+Vector3.Scale(corner,size)*.5f);foreach(int i in new[]{0,2,1,0,3,2,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,3,7,6,3,6,2,0,1,5,0,5,4})indices.Add(start+i);}
  foreach(float side in new[]{-1f,1f}){Box(new Vector3(0,0,side*.032f),new Vector3(.055f,.18f,.018f));Box(new Vector3(0,0,side*.068f),new Vector3(.036f,.034f,.07f));Box(new Vector3(-.057f,0,side*.106f),new Vector3(.15f,.032f,.035f));}
  string meshPath=root+"/Meshes/Gagang tuas persegi dua sisi.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(mesh==null){mesh=new Mesh{name="Gagang tuas persegi dua sisi"};AssetDatabase.CreateAsset(mesh,meshPath);}mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
  string materialPath=root+"/Materials/Logam gagang tuas.mat";var metal=AssetDatabase.LoadAssetAtPath<Material>(materialPath);if(metal==null){metal=new Material(Shader.Find("Universal Render Pipeline/Lit"));metal.SetColor("_BaseColor",new Color(.58f,.60f,.62f));metal.SetFloat("_Metallic",.65f);metal.SetFloat("_Smoothness",.4f);AssetDatabase.CreateAsset(metal,materialPath);}
  string prefabPath=root+"/Prefabs/Asrama Crystal - Lantai 1.prefab";var prefab=PrefabUtility.LoadPrefabContents(prefabPath);
  try{
   foreach(var hinge in prefab.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Engsel")).ToArray()){
    var door=hinge.Find("Daun pintu");var collider=door.GetComponent<BoxCollider>();var handle=hinge.Find("Gagang tuas persegi - dua sisi");if(handle==null){handle=new GameObject("Gagang tuas persegi - dua sisi").transform;handle.SetParent(hinge,false);handle.gameObject.AddComponent<MeshFilter>();handle.gameObject.AddComponent<MeshRenderer>();}
    handle.localPosition=new Vector3(collider.size.x-.14f,1.02f,0);handle.GetComponent<MeshFilter>().sharedMesh=mesh;handle.GetComponent<MeshRenderer>().sharedMaterial=metal;
   }
   foreach(var stairs in prefab.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Tangga kiri N1 N2"||t.name=="Tangga kanan N1 N2"))foreach(var filter in stairs.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name=="N1 menjauhi lorong - bordes - N2 kembali ke lorong"||f.name=="Pagar dan pegangan tangga krem")){
    if(filter.sharedMesh.name.StartsWith("Rapat dinding"))continue;
    var replacement=Object.Instantiate(filter.sharedMesh);replacement.name="Rapat dinding "+stairs.name+" "+filter.name;float middle=replacement.bounds.center.x;var points=replacement.vertices;for(int i=0;i<points.Length;i++)if(points[i].x<middle)points[i].x-=.4f;replacement.vertices=points;replacement.RecalculateBounds();string path=root+"/Meshes/"+replacement.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing==null){AssetDatabase.CreateAsset(replacement,path);existing=replacement;}else Object.DestroyImmediate(replacement);filter.sharedMesh=existing;var collision=filter.GetComponent<MeshCollider>();if(collision!=null)collision.sharedMesh=existing;
   }
   var chairs=prefab.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Deretan kursi biru sisi kanan lantai 1");foreach(Transform chair in chairs){var p=chair.localPosition;p.z=-.93f;chair.localPosition=p;}
   var floor=prefab.transform.Find("Lantai dasar menyatu - tahap lantai 1");if(floor==null){floor=new GameObject("Lantai dasar menyatu - tahap lantai 1").transform;floor.SetParent(prefab.transform,false);var ground=new Mesh{name="Lantai dasar Crystal"};ground.vertices=new[]{new Vector3(-70,-.012f,-20),new Vector3(-70,-.012f,20),new Vector3(70,-.012f,20),new Vector3(70,-.012f,-20)};ground.uv=new[]{new Vector2(-70,-20),new Vector2(-70,20),new Vector2(70,20),new Vector2(70,-20)};ground.triangles=new[]{0,1,2,0,2,3};ground.RecalculateNormals();ground.RecalculateBounds();AssetDatabase.CreateAsset(ground,root+"/Meshes/Lantai dasar Crystal.asset");floor.gameObject.AddComponent<MeshFilter>().sharedMesh=ground;floor.gameObject.AddComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(root+"/Materials/Ubin 60cm.mat");var collider=floor.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,-.055f,0);collider.size=new Vector3(140,.08f,40);}
   PrefabUtility.SaveAsPrefabAsset(prefab,prefabPath);
  }finally{PrefabUtility.UnloadPrefabContents(prefab);}
  var original=SceneManager.GetActiveScene();var scene=SceneManager.GetSceneByPath(IntegrasiCrystalDanKelas.CrystalScene);bool opened=scene.isLoaded;if(!opened)scene=EditorSceneManager.OpenScene(IntegrasiCrystalDanKelas.CrystalScene,OpenSceneMode.Additive);
  var shader=Shader.Find("Arthur/Crystal/Potongan Samping");if(shader==null)throw new Exception("Shader potongan Crystal belum tersedia.");
  if(!AssetDatabase.IsValidFolder(root+"/Materials/Potongan"))AssetDatabase.CreateFolder(root+"/Materials","Potongan");
  var cache=new System.Collections.Generic.Dictionary<Material,Material>();foreach(var renderer in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){
   renderer.sharedMaterials=renderer.sharedMaterials.Select(source=>{if(source==null)return source;if(source.shader==shader){source.SetFloat("_CrystalGround",source.name.Contains("Ubin")?1:0);EditorUtility.SetDirty(source);return source;}if(cache.TryGetValue(source,out var found))return found;string path=root+"/Materials/Potongan/"+source.name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat==null){mat=new Material(source);mat.shader=shader;AssetDatabase.CreateAsset(mat,path);}else{mat.CopyPropertiesFromMaterial(source);mat.shader=shader;}mat.SetFloat("_CrystalGround",source.name.Contains("Ubin")?1:0);cache[source]=mat;EditorUtility.SetDirty(mat);return mat;}).ToArray();PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);EditorUtility.SetDirty(renderer);
  }
  EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);if(!opened)EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(original);
 }
}
