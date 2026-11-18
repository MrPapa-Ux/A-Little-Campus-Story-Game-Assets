using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;

// Revisi dua tangga dan salinan lantai atas; ukuran acuan dalam meter.
public static class CrystalLantaiDua
{
 const string Root="Assets/Asrama Crystal Edel Referensi", Work="work/crystal-lantai2-20261112/",UpperName="Lantai 2 - salinan denah lantai 1";
 static Material tile,red,cream,white;static readonly Vector3 Origin=new Vector3(3000,0,0);
 struct Rect {public float a,b,c,d;public Rect(float x0,float x1,float z0,float z1){a=x0;b=x1;c=z0;d=z1;}}
 static readonly Rect[] Holes={new Rect(-9.16f,-5.2f,-5.92f,-1.99f),new Rect(6.06f,10.02f,-5.92f,-1.99f)};
 sealed class Geo {
  public List<Vector3> v=new List<Vector3>(),n=new List<Vector3>();public List<Vector2> uv=new List<Vector2>();public List<Material> mats=new List<Material>();public List<List<int>> indices=new List<List<int>>();
  public void Box(Vector3 p,Vector3 size,Material mat,Quaternion rotation=default){if(rotation==default)rotation=Quaternion.identity;int slot=mats.IndexOf(mat);if(slot<0){slot=mats.Count;mats.Add(mat);indices.Add(new List<int>());}
   var corners=new[]{new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1)};
   foreach(var face in new[]{new[]{1,0,3,2},new[]{4,5,6,7},new[]{0,4,7,3},new[]{5,1,2,6},new[]{3,7,6,2},new[]{0,1,5,4}}){int start=v.Count;var points=face.Select(i=>p+rotation*Vector3.Scale(corners[i],size)*.5f).ToArray();var normal=Vector3.Cross(points[1]-points[0],points[2]-points[0]).normalized;foreach(var q in points){v.Add(q);n.Add(normal);uv.Add(Mathf.Abs(normal.y)>.5f?new Vector2(q.x,q.z):Mathf.Abs(normal.x)>.5f?new Vector2(q.z,q.y):new Vector2(q.x,q.y));}indices[slot].AddRange(new[]{start,start+1,start+2,start,start+2,start+3});}
  }
  public void Rod(Vector3 a,Vector3 b,float width){Box((a+b)*.5f,new Vector3(width,width,(b-a).magnitude),cream,Quaternion.LookRotation(b-a));}
 }
 static Mesh Save(string name,Geo geo){string path=Root+"/Meshes/"+name+".asset";var m=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(m==null){m=new Mesh{name=name};AssetDatabase.CreateAsset(m,path);}m.Clear();m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetVertices(geo.v);m.SetNormals(geo.n);m.SetUVs(0,geo.uv);m.subMeshCount=geo.indices.Count;for(int i=0;i<geo.indices.Count;i++)m.SetTriangles(geo.indices[i],i);m.RecalculateBounds();m.UploadMeshData(false);EditorUtility.SetDirty(m);return m;}
 static GameObject Part(Transform parent,string name,Geo geo,bool collider){var t=parent.Find(name);if(t==null){t=new GameObject(name).transform;t.SetParent(parent,false);}var mf=t.GetComponent<MeshFilter>();if(mf==null)mf=t.gameObject.AddComponent<MeshFilter>();mf.sharedMesh=Save(name,geo);var r=t.GetComponent<MeshRenderer>();if(r==null)r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterials=geo.mats.ToArray();if(collider){var c=t.GetComponent<MeshCollider>();if(c==null)c=t.gameObject.AddComponent<MeshCollider>();c.sharedMesh=mf.sharedMesh;}return t.gameObject;}
 static IEnumerable<Rect> Minus(Rect r,Rect h){if(h.a>=r.b||h.b<=r.a||h.c>=r.d||h.d<=r.c){yield return r;yield break;}float a=Mathf.Max(r.a,h.a),b=Mathf.Min(r.b,h.b),c=Mathf.Max(r.c,h.c),d=Mathf.Min(r.d,h.d);if(r.a<a)yield return new Rect(r.a,a,r.c,r.d);if(b<r.b)yield return new Rect(b,r.b,r.c,r.d);if(r.c<c)yield return new Rect(a,b,r.c,c);if(d<r.d)yield return new Rect(a,b,d,r.d);}
 static Geo Slab(float top,bool holes,Material material){var geo=new Geo();var areas=new[]{new Rect(-60.76f,-5.2f,-6.4f,6.4f),new Rect(5.2f,60.76f,-6.4f,6.4f),new Rect(-5.2f,6,-9.8f,1.6f),new Rect(-9.16f,-5.2f,-9.8f,-6.4f),new Rect(6,10.02f,-9.8f,-6.4f),new Rect(-5.2f,1.1f,1.6f,11.15f)}.ToList();if(holes)foreach(var hole in Holes)areas=areas.SelectMany(r=>Minus(r,hole)).ToList();foreach(var r in areas)geo.Box(new Vector3((r.a+r.b)*.5f,top-.07f,(r.c+r.d)*.5f),new Vector3(r.b-r.a,.14f,r.d-r.c),material);return geo;}
 static void Stairs(Transform root,int index){
  var floor=root.GetComponentsInChildren<MeshFilter>().Single(f=>f.name=="Jalan lantai biasa di depan tangga").sharedMesh.bounds;
  // Dinding bagian dalam di sisi lobi sudah berada pada tepi lantai kanan/kiri.
  float min=floor.min.x+(index==0?.06f:0)+.03f,max=floor.max.x-.06f-.03f,mid=(min+max)*.5f;
  float leftMin=min,leftMax=mid-.09f,rightMin=mid+.09f,rightMax=max,left=(leftMin+leftMax)*.5f,right=(rightMin+rightMax)*.5f;
  var g=new Geo();var rails=new Geo();float turn=-4.7f;
  for(int i=0;i<9;i++){float y=(i+1)*.2f,z=-2-(i+.5f)*.3f;g.Box(new Vector3(right,y*.5f,z),new Vector3(rightMax-rightMin,y,.3f),tile);g.Box(new Vector3(right,y+.006f,z+.115f),new Vector3(rightMax-rightMin,.018f,.07f),red);float yy=1.8f+(i+1)*.2f,zz=turn+(i+.5f)*.3f;g.Box(new Vector3(left,yy-.08f,zz),new Vector3(leftMax-leftMin,.16f,.3f),tile);g.Box(new Vector3(left,yy+.006f,zz-.115f),new Vector3(leftMax-leftMin,.018f,.07f),red);}
  g.Box(new Vector3(mid,1.72f,-5.3f),new Vector3(max-min,.16f,1.2f),tile);g.Box(new Vector3(left,3.52f,-1.65f),new Vector3(leftMax-leftMin,.16f,.7f),tile);
  var a=new Vector3(left,1.63f,turn);var b=new Vector3(left,3.43f,-2);g.Box((a+b)*.5f,new Vector3(leftMax-leftMin,.16f,(b-a).magnitude),tile,Quaternion.LookRotation(b-a));
  foreach(bool upper in new[]{false,true})foreach(bool outer in new[]{false,true}){
   float x=upper?(outer?leftMin+.02f:leftMax-.02f):(outer?rightMax-.02f:rightMin+.02f);
   for(int i=0;i<=9;i++){float y=upper?1.8f+i*.2f:i*.2f,z=upper?turn+i*.3f:-2-i*.3f;rails.Rod(new Vector3(x,y,z),new Vector3(x,y+.92f,z),.035f);}
   rails.Rod(new Vector3(x,upper?2.72f:.92f,upper?turn:-2),new Vector3(x,upper?4.52f:2.72f,upper?-2:turn),.04f);
  }
  for(int i=0;i<=10;i++){float x=Mathf.Lerp(min+.02f,max-.02f,i/10f);rails.Rod(new Vector3(x,1.8f,-5.86f),new Vector3(x,2.72f,-5.86f),.035f);}rails.Rod(new Vector3(min+.02f,2.72f,-5.86f),new Vector3(max-.02f,2.72f,-5.86f),.04f);
  foreach(var name in new[]{"N1 menjauhi lorong - bordes - N2 kembali ke lorong","Pagar dan pegangan tangga krem"}){var old=root.Find(name);if(old!=null)Object.DestroyImmediate(old.gameObject);}
  Part(root,"Tangga rapat 3cm - pijakan dan bordes "+index,g,true);Part(root,"Tangga rapat 3cm - pagar dan pegangan "+index,rails,false);
  var marker=root.Find("Arah N1 dari lorong ke depan bangunan");if(marker!=null)marker.localPosition=new Vector3(right,0,-2);
 }
 static void Upper(Transform building){
  var old=building.Find(UpperName);if(old!=null)Object.DestroyImmediate(old.gameObject);
  var children=building.Cast<Transform>().ToArray();var upper=new GameObject(UpperName).transform;upper.SetParent(building,false);upper.localPosition=Vector3.up*3.6f;
  foreach(var source in children){if(source.name.StartsWith("Plafon")||source.name.StartsWith("Lantai dasar")||source.name.StartsWith("Sambungan plafon"))continue;var copy=Object.Instantiate(source.gameObject,upper);copy.name=source.name;if(PrefabUtility.IsOutermostPrefabInstanceRoot(copy))PrefabUtility.UnpackPrefabInstance(copy,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);}
  foreach(var t in upper.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Tangga rapat 3cm")||t.name.StartsWith("Tempat cukur")||t.name.StartsWith("Gudang kecil")||t.name.StartsWith("Arah N1")||t.name.StartsWith("Pohon Natal")||t.name.StartsWith("Televisi LED")||t.name.StartsWith("Papan nama di belakang")).ToArray())if(t!=null)Object.DestroyImmediate(t.gameObject);
  int serial=0;foreach(var filter in upper.GetComponentsInChildren<MeshFilter>(true).ToArray()){
   var m=filter.sharedMesh;if(m==null)continue;bool floor=m.bounds.min.y<-.03f,high=m.bounds.max.y>3.6f;if(!floor&&!high)continue;var geo=new Geo();var vertices=m.vertices;var normals=m.normals;var uv=m.uv;var materials=filter.GetComponent<Renderer>().sharedMaterials;for(int v=0;v<vertices.Length;v++){var p=vertices[v];p.y=Mathf.Min(p.y,3.3f);geo.v.Add(p);geo.n.Add(normals[v]);geo.uv.Add(uv.Length>v?uv[v]:Vector2.zero);}for(int sub=0;sub<m.subMeshCount;sub++){geo.mats.Add(materials[sub]);var keep=new List<int>();var ids=m.GetTriangles(sub);for(int i=0;i<ids.Length;i+=3)if(!floor||vertices[ids[i]].y>.001f||vertices[ids[i+1]].y>.001f||vertices[ids[i+2]].y>.001f)keep.AddRange(new[]{ids[i],ids[i+1],ids[i+2]});geo.indices.Add(keep);}filter.sharedMesh=Save("Lantai2 dasar tanpa bidang tumpuk "+serial++,geo);var c=filter.GetComponent<MeshCollider>();if(c!=null)c.sharedMesh=filter.sharedMesh;
  }
  Part(upper,"Lantai 2 - pelat dengan dua lubang tangga",Slab(0,true,tile),true);
  Part(upper,"Lantai 2 - plafon penutup",Slab(3.3f,false,white),true);
  var ceilings=building.Find("Plafon - nonaktifkan untuk melihat denah");if(ceilings!=null)ceilings.gameObject.SetActive(false);
  // Penutup lantai bawah juga mempunyai lubang tangga yang sama, tidak memotong jalur naik.
  Part(building,"Sambungan plafon lantai 1 dengan dua lubang tangga",Slab(3.44f,true,white),true);
 }
 public static void Build(){
  if(EditorApplication.isPlaying)throw new Exception("Hentikan Play.");Directory.CreateDirectory(Work);string path=Root+"/Prefabs/Asrama Crystal - Lantai 1.prefab";if(!File.Exists(Work+"Crystal-sebelum-lantai2.prefab"))File.Copy(path,Work+"Crystal-sebelum-lantai2.prefab");
  tile=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Ubin 60cm.mat");red=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Tepi anak tangga merah tua.mat");cream=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Krem pagar dan kusen.mat");white=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/Putih dinding.mat");
  var prefab=PrefabUtility.LoadPrefabContents(path);try{var stairs=prefab.transform.Cast<Transform>().Where(t=>t.name.StartsWith("Tangga ")).ToArray();if(stairs.Length!=2)throw new Exception("Harus dua tangga.");for(int i=0;i<stairs.Length;i++)Stairs(stairs[i],i);Upper(prefab.transform);HapusColliderKosong(prefab);PrefabUtility.SaveAsPrefabAsset(prefab,path);}finally{PrefabUtility.UnloadPrefabContents(prefab);}AssetDatabase.SaveAssets();
  RefreshScene();
 }
 static int HapusColliderKosong(GameObject root){int count=0;foreach(var collider in root.GetComponentsInChildren<MeshCollider>(true)){var m=collider.sharedMesh;if(m==null||!m.isReadable)continue;var points=m.vertices;var ids=m.triangles;bool valid=false;for(int i=0;i+2<ids.Length;i+=3)if(Vector3.Cross(points[ids[i+1]]-points[ids[i]],points[ids[i+2]]-points[ids[i]]).sqrMagnitude>1e-10f){valid=true;break;}if(!valid){Object.DestroyImmediate(collider);count++;}}return count;}
 public static int RapikanColliderKosong(){var path=Root+"/Prefabs/Asrama Crystal - Lantai 1.prefab";var root=PrefabUtility.LoadPrefabContents(path);try{int count=HapusColliderKosong(root);PrefabUtility.SaveAsPrefabAsset(root,path);return count;}finally{PrefabUtility.UnloadPrefabContents(root);}}
 static void RefreshScene(){var original=SceneManager.GetActiveScene();var s=SceneManager.GetSceneByPath(IntegrasiCrystalDanKelas.CrystalScene);bool loaded=s.isLoaded;if(!loaded)s=EditorSceneManager.OpenScene(IntegrasiCrystalDanKelas.CrystalScene,OpenSceneMode.Additive);var shader=Shader.Find("Arthur/Crystal/Potongan Samping");foreach(var renderer in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>{if(m==null||m.shader==shader)return m;var p=Root+"/Materials/Potongan/"+m.name+".mat";var copy=AssetDatabase.LoadAssetAtPath<Material>(p);if(copy==null){copy=new Material(m);copy.shader=shader;copy.SetFloat("_CrystalGround",m.name.Contains("Ubin")?1:0);AssetDatabase.CreateAsset(copy,p);}return copy;}).ToArray();PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);EditorUtility.SetDirty(renderer);}EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);if(!loaded)EditorSceneManager.CloseScene(s,true);SceneManager.SetActiveScene(original);AssetDatabase.SaveAssets();}
 static JalurKampus Route(Transform root,string name,Vector3[] feet){var t=new GameObject(name).transform;t.SetParent(root,false);var r=t.gameObject.AddComponent<JalurKampus>();r.diDalamRuangan=true;r.ukuranVisualArthur=.72f;r.kananMengikutiUrutanTitik=false;r.radiusColliderKhusus=.18f;r.tinggiColliderKhusus=2.2f;r.titik=feet.Select((p,i)=>{var point=new GameObject("Titik "+i).transform;point.SetParent(t,false);point.position=Origin+p+Vector3.up*ArthurJalurKampus.JarakKaki;return point;}).ToArray();var c=t.gameObject.AddComponent<KameraPilihanPemilik>();c.gunakan=true;c.ikutiArthur=true;c.menyampingJalur=true;c.balikSisi=true;c.jarakSamping=5.4f;c.tinggiSamping=2.25f;c.tinggiFokusSamping=.7f;c.sudut=55;c.kehalusan=7;JalurKampusInspector.Rebuild(r);return r;}
 static Transform Destination(Transform root,JalurKampus r,Vector3 point,string name){var t=new GameObject(name).transform;t.SetParent(root,false);t.position=Origin+point+Vector3.up*ArthurJalurKampus.JarakKaki;t.gameObject.AddComponent<TujuanJalurKampus>().jalur=r;return t;}
 static void Choice(Transform root,InteraksiJalurKampus source,JalurKampus route,Vector3 point,string text,Transform destination){
  var marker=root.GetComponentsInChildren<InteraksiJalurKampus>(true).FirstOrDefault(m=>m.jalur==route&&Vector3.Distance(m.transform.position,Origin+point+Vector3.up*ArthurJalurKampus.JarakKaki)<.02f);
  if(marker==null){var g=Object.Instantiate(source.gameObject,root);g.name=route.name+"_Sambungan_"+text;foreach(var d in g.GetComponentsInChildren<TujuanJalurKampus>(true))Object.DestroyImmediate(d.gameObject);marker=g.GetComponent<InteraksiJalurKampus>();marker.jalur=route;marker.jalurTambahan=Array.Empty<JalurKampus>();marker.radius=.85f;marker.ikutiKepalaArthur=false;marker.tinggiKepalaDiTitik=-.5f;marker.transform.position=Origin+point+Vector3.up*ArthurJalurKampus.JarakKaki;marker.GetComponent<AreaInteraksi>().daftarOpsi=Array.Empty<OpsiInteraksi>();}
  var area=marker.GetComponent<AreaInteraksi>();area.idTempat=marker.name;area.tipeInteraksi=TipeInteraksi.Tempat_Teleportasi;area.skenarioDialog=Array.Empty<BarisDialog>();area.daftarOpsi=area.daftarOpsi.Concat(new[]{new OpsiInteraksi{teksOpsi=text,titikTujuan=destination,pesanBelumTersedia=""}}).ToArray();area.tampilkanTombolKeluar=true;
 }
 static void Join(Transform root,InteraksiJalurKampus source,JalurKampus a,Vector3 pa,JalurKampus b,Vector3 pb,string toA,string toB){Choice(root,source,a,pa,toB,Destination(root,b,pb,"Kedatangan "+b.name+" dari "+a.name));Choice(root,source,b,pb,toA,Destination(root,a,pa,"Kedatangan "+a.name+" dari "+b.name));}
 public static void Routes(){
  if(EditorApplication.isPlaying)throw new Exception("Hentikan Play.");var original=SceneManager.GetActiveScene();
  foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Chapter1Preview/Chapter1_Kamar201.unity"}){
   var s=SceneManager.GetSceneByPath(path);bool loaded=s.isLoaded;if(!loaded)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);SceneManager.SetActiveScene(s);
   var all=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();var parent=all.Single(t=>t.name=="INTERIOR CRYSTAL DAN KELAS - jalur aktif");if(parent.Find("RUTE CRYSTAL LANTAI 2")!=null)throw new Exception("Jalur lantai2 sudah ada.");EditorSceneManager.SaveScene(s,Work+Path.GetFileName(path),true);
   var root=new GameObject("RUTE CRYSTAL LANTAI 2").transform;root.SetParent(parent,false);var source=parent.GetComponentsInChildren<InteraksiJalurKampus>().First(m=>m.name=="UNK_CrystalInterior_Keluar");
   var left=Route(root,"UNK_CrystalInterior_Lantai2_LorongKiri",new[]{new Vector3(-54,3.6f,-.6f),new Vector3(0,3.6f,-.6f)});var right=Route(root,"UNK_CrystalInterior_Lantai2_LorongKanan",new[]{new Vector3(0,3.6f,-.6f),new Vector3(10.5f,3.6f,-.6f),new Vector3(11.3f,3.6f,0),new Vector3(54,3.6f,0)});var branch=Route(root,"UNK_CrystalInterior_Lantai2_Cabang",new[]{new Vector3(0,3.6f,.5f),new Vector3(0,3.6f,10.1f)});var lobby=Route(root,"UNK_CrystalInterior_Lantai2_Lobi",new[]{new Vector3(0,3.6f,-.6f),new Vector3(0,3.6f,-3.5f),new Vector3(4.19f,3.6f,-3.5f),new Vector3(4.19f,3.6f,-8.8f)});
   var junction=new Vector3(0,3.6f,-.6f);Join(root,source,left,junction,right,junction,"Ke lorong kiri lantai 2","Ke lorong kanan lantai 2");Join(root,source,left,junction,branch,new Vector3(0,3.6f,.5f),"Ke simpang lantai 2","Ke cabang lantai 2");Join(root,source,left,junction,lobby,junction,"Ke simpang lantai 2","Ke area duduk lantai 2");
   foreach(int side in new[]{0,1}){
    float min=side==0?-9.07f:6.09f,max=side==0?-5.29f:9.93f,middle=(min+max)*.5f,l=(min+middle-.09f)*.5f,r=(middle+.09f+max)*.5f;
    var points=new List<Vector3>{new Vector3(r,0,-.6f),new Vector3(r,0,-1.8f)};for(int i=0;i<9;i++)points.Add(new Vector3(r,(i+1)*.2f,-2-(i+.5f)*.3f));points.Add(new Vector3(r,1.8f,-5.2f));points.Add(new Vector3(l,1.8f,-5.2f));for(int i=0;i<9;i++)points.Add(new Vector3(l,1.8f+(i+1)*.2f,-4.7f+(i+.5f)*.3f));points.Add(new Vector3(l,3.6f,-1.5f));points.Add(new Vector3(l,3.6f,-.6f));
    var stairs=Route(root,"UNK_CrystalInterior_Tangga"+(side==0?"Kiri":"Kanan"),points.ToArray());var ground=parent.GetComponentsInChildren<JalurKampus>().Single(j=>j.name==(side==0?"UNK_CrystalInterior_LorongKiri":"UNK_CrystalInterior_LorongKanan"));Join(root,source,ground,points[0],stairs,points[0],"Kembali ke lorong lantai 1","Naik tangga "+(side==0?"kiri":"kanan"));Join(root,source,stairs,points.Last(),side==0?left:right,points.Last(),"Turun melalui tangga "+(side==0?"kiri":"kanan"),"Ke lorong lantai 2");
   }
   EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);if(!loaded)EditorSceneManager.CloseScene(s,true);
  }SceneManager.SetActiveScene(original);AssetDatabase.SaveAssets();
 }
}
