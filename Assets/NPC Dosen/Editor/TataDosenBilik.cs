using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class TataDosenBilik
{
 const string Work="work/dosen-bilik-20261110/";
 static T[] All<T>(Scene s) where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
 static void Head(string name){string p="Assets/NPC Dosen/FILKOM/Kepala/"+name+".png";AssetDatabase.ImportAsset(p);var i=(TextureImporter)AssetImporter.GetAtPath(p);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.GetSourceTextureWidthAndHeight(out int w,out int h);i.spritePixelsPerUnit=h/.97f;i.alphaIsTransparency=true;i.mipmapEnabled=false;i.maxTextureSize=512;i.SaveAndReimport();}
 public static void Run(){
 Directory.CreateDirectory(Work+"cadangan");Head("Jimmy H Moedjahedy");Head("Marchel Timothy Tombeng");var original=SceneManager.GetActiveScene();var log=new System.Collections.Generic.List<string>();
 foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Chapter1Preview/Chapter1_Kamar201.unity"}){
 var s=SceneManager.GetSceneByPath(path);bool opened=s.isLoaded;if(!opened)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
 try{
 if(s.isDirty)throw new Exception("Scene belum disimpan: "+path);
 string bak=Work+"cadangan/"+Path.GetFileName(path);if(!File.Exists(bak))EditorSceneManager.SaveScene(s,bak,true);
 var dean=All<JalurKampus>(s).Single(r=>r.name=="GK1_RuanganDekanFilkom");var office=All<JalurKampus>(s).Single(r=>r.name=="GK1_RuanganDosen");
 var ns=All<NpcYangBisaDiatur>(s);var stenly=ns.Single(n=>n.namaNpc.Contains("Stenly"));Vector3 stenlyPos=stenly.transform.position;
 var semmy=ns.Single(n=>n.namaNpc.Contains("Semmy"));var parent=stenly.transform.parent;
 Put(semmy,dean,parent,new Vector3(-2.05f,.025f,-3.65f));
 string[] keys={"Jimmy H Moedjahedy","Marchel Timothy Tombeng"};string[] labels={"Jimmy H. Moedjahedy, S.Kom., M.M., M.Kom.","Ir. Marchel Timothy Tombeng, S.Kom., MS"};
 for(int j=0;j<2;j++){
 var n=All<NpcYangBisaDiatur>(s).FirstOrDefault(x=>x.name==keys[j]);
 if(n==null){var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NPC Dosen/FILKOM/Prefab/Semmy W Taju.prefab"),s);n=go.GetComponent<NpcYangBisaDiatur>();go.name=keys[j];}
 n.namaNpc=j==0?"Jimmy H. Moedjahedy":"Marchel Timothy Tombeng";n.dialog="Selamat datang. Saya "+labels[j]+(j==0?", Kaprodi Sistem Informasi.":".");n.Terapkan();
 var life=n.GetComponent<NpcKampusHidup>();var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/NPC Dosen/FILKOM/Kepala/"+keys[j]+".png");life.kepala.GetComponent<SpriteRenderer>().sprite=sprite;n.GetComponent<AreaInteraksi>().potretDialog=sprite;
 Put(n,dean,parent,new Vector3(-2.05f,.025f,j==0?-1.05f:1.55f));
 string prefab="Assets/NPC Dosen/FILKOM/Prefab/"+keys[j]+".prefab";if(!File.Exists(prefab))PrefabUtility.SaveAsPrefabAsset(n.gameObject,prefab);
 }
 string[] others={"Argha","Julio","Joe Y.","Green Ferry","Green A."};
 for(int j=0;j<others.Length;j++){var n=ns.Single(x=>x.namaNpc.Contains(others[j]));Put(n,office,n.transform.parent,new Vector3(-3.55f,.025f,-3.73f+j*1.7f));}
 foreach(var r in new[]{dean,office}){
 var k=r.GetComponent<KameraPilihanPemilik>();if(k==null)k=r.gameObject.AddComponent<KameraPilihanPemilik>();
 k.gunakan=true;k.ikutiArthur=true;k.kehalusan=4;k.sudut=65;
 r.Sampel(r.Panjang*.5f,out k.acuan,out _,out _);
 k.posisi=new Vector3(r.transform.position.x+2.8f,r.transform.position.y+2.4f,k.acuan.z);
 k.bidikan=new Vector3(r.transform.position.x-2.05f,r.transform.position.y+1.1f,k.acuan.z);EditorUtility.SetDirty(k);
 }
 if(stenly.transform.position!=stenlyPos)throw new Exception("Stenly berpindah");
 EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);log.Add(path+": 3 bilik dekan, 5 bilik dosen, Stenly tetap; kamera mengikuti.");
 }finally{if(!opened)EditorSceneManager.CloseScene(s,true);}
 }
 SceneManager.SetActiveScene(original);AssetDatabase.SaveAssets();File.WriteAllLines(Work+"hasil.txt",log);
 }
 static void Put(NpcYangBisaDiatur n,JalurKampus r,Transform parent,Vector3 local){n.transform.SetParent(parent,true);n.transform.position=r.transform.TransformPoint(local);var ukuran=UnityEngine.Object.FindObjectsByType<NpcYangBisaDiatur>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(x=>x.gameObject.scene==n.gameObject.scene&&x.namaNpc.Contains("Stenly")).transform.lossyScale;var skalaInduk=parent.lossyScale;n.transform.localScale=new Vector3(ukuran.x/skalaInduk.x,ukuran.y/skalaInduk.y,ukuran.z/skalaInduk.z);n.jalurInteraksi=r;Vector3 target=n.transform.position+Vector3.up*ArthurJalurKampus.JarakKaki;r.Sampel(r.JarakTerdekat(target),out var p,out _,out _);n.geserTitikInteraksi=p-Vector3.up*ArthurJalurKampus.JarakKaki-n.transform.position;n.jarakInteraksi=1.05f;PrefabUtility.RecordPrefabInstancePropertyModifications(n.transform);PrefabUtility.RecordPrefabInstancePropertyModifications(n);EditorUtility.SetDirty(n);}
}
