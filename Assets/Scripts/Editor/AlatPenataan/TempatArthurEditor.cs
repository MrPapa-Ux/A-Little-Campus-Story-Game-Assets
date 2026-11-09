using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;

// Alat editor saja: memudahkan pemilik proyek menyunting tempat tanpa mengubah skrip.
public sealed class DaftarTempatArthur : EditorWindow
{
 string cari=""; Vector2 scroll;int tab;
 [MenuItem("Arthur/Daftar tempat dan bubble")]
 public static void Open(){GetWindow<DaftarTempatArthur>("Tempat Arthur");}
 public static void OpenNpc(){var w=GetWindow<DaftarTempatArthur>("Tempat Arthur");w.tab=1;w.Repaint();}
 public static void OpenWorld(){var w=GetWindow<DaftarTempatArthur>("Tempat Arthur");w.tab=3;w.Repaint();}
 [MenuItem("Arthur/Buka buku manual")]
 static void Manual(){Application.OpenURL(Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Panduan Pemilik/Buku Manual Arthur.pdf"));}
 void OnGUI(){
  tab=GUILayout.Toolbar(tab,new[]{"Tempat / bubble","NPC","Jalur","World"});
  if(tab==1){AlatNpcArthur.Panel();return;}
  if(tab==2){DaftarJalurArthur.Panel();return;}
  if(tab==3){PanelWorldKampus.Panel();EditorGUILayout.Space();AlatWorldArthur.Panel();return;}
  EditorGUILayout.HelpBox("Cari nama tempat, pilih objeknya, lalu atur di Inspector. Hentikan Play sebelum menyimpan perubahan.",MessageType.Info);
  TambahTempatArthur.Panel();
  EditorGUILayout.Space();cari=EditorGUILayout.TextField("Cari tempat",cari);scroll=EditorGUILayout.BeginScrollView(scroll);
  foreach(var a in UnityEngine.Object.FindObjectsByType<AreaInteraksi>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(a=>a.gameObject.scene.IsValid()).OrderBy(a=>a.name)){
   if(!string.IsNullOrEmpty(cari)&&!(a.name+" "+a.idTempat).ToLowerInvariant().Contains(cari.ToLowerInvariant()))continue;
   if(GUILayout.Button(a.name+"\n"+a.idTempat,GUILayout.Height(42))){Selection.activeGameObject=a.gameObject;SceneView.lastActiveSceneView?.FrameSelected();}
  }EditorGUILayout.EndScrollView();
 }
}
[CustomEditor(typeof(InteraksiJalurKampus))]
public sealed class InteraksiJalurKampusInspector : Editor
{
 bool pengaturanLanjutan;
 public override void OnInspectorGUI(){
  var m=(InteraksiJalurKampus)target;var area=m.GetComponent<AreaInteraksi>();
  EditorGUILayout.LabelField("TEMPAT BUBBLE INTERAKSI",EditorStyles.boldLabel);
  EditorGUILayout.HelpBox("Fungsi: "+BubbleArthurScene.Ringkasan(m)+"\nJalur: "+(m.jalur!=null?m.jalur.name:"Belum dipilih")+"\n\nKlik penanda oranye BUBBLE di Scene, lalu seret panahnya. Lingkaran di lantai adalah jangkauan interaksi. Ctrl+Z membatalkan. Matikan Play dan simpan dengan Ctrl+S.",MessageType.Info);
  using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode)){
   BubbleArthurScene.IkutiJalur=EditorGUILayout.ToggleLeft(new GUIContent("Tempel ke jalur hijau saat digeser","Arthur harus dapat mencapai titik bubble. Matikan hanya jika kamu sengaja ingin posisi bebas."),BubbleArthurScene.IkutiJalur);
   if(GUILayout.Button("Geser tempat bubble di Scene"))BubbleArthurScene.Pilih(m);
   if(GUILayout.Button("Pusatkan tempat bubble ke jalur hijau"))BubbleArthurScene.Geser(m,m.transform.position,true);
  }
  EditorGUILayout.HelpBox("Yang digeser adalah tempat Arthur berdiri untuk berinteraksi. Gambar bubble muncul di atas titik itu saat Play. Titik kedatangan yang berimpit ikut bergeser; garis hijau tetap.",MessageType.None);
  if(area!=null&&area.daftarOpsi!=null)foreach(var o in area.daftarOpsi){
   if(o==null)continue;EditorGUILayout.LabelField("Pilihan",o.teksOpsi,EditorStyles.wordWrappedLabel);
   if(o.titikTujuan!=null){var d=o.titikTujuan.GetComponent<TujuanJalurKampus>();EditorGUILayout.LabelField("Menuju",d!=null&&d.jalur!=null?d.jalur.name:o.titikTujuan.name,EditorStyles.wordWrappedLabel);if(GUILayout.Button("Lihat tujuan: "+o.teksOpsi)){Selection.activeTransform=o.titikTujuan;SceneView.lastActiveSceneView?.FrameSelected();}}
   else if(o.cabangJalur!=null)EditorGUILayout.LabelField("Aksi","Membuka cabang jalan tanpa teleportasi");
  }
  if(area!=null){
   EditorGUILayout.Space();EditorGUILayout.LabelField("Atur pilihan bubble",EditorStyles.boldLabel);
   using(var data=new SerializedObject(area)){
    data.Update();var options=data.FindProperty("daftarOpsi");
    for(int i=0;i<options.arraySize;i++){var row=options.GetArrayElementAtIndex(i);EditorGUILayout.PropertyField(row.FindPropertyRelative("teksOpsi"),new GUIContent("Nama pilihan"));EditorGUILayout.PropertyField(row.FindPropertyRelative("titikTujuan"),new GUIContent("Titik tujuan"));if(row.FindPropertyRelative("titikTujuan").objectReferenceValue==null&&row.FindPropertyRelative("cabangJalur").objectReferenceValue==null){EditorGUILayout.PropertyField(row.FindPropertyRelative("pesanBelumTersedia"),new GUIContent("Pesan sementara"));EditorGUILayout.HelpBox("Isi Titik tujuan untuk menyambungkan perpindahan. Kamu bisa menyeret titik tujuan dari Hierarchy ke kolom ini.",MessageType.Info);}}
    data.ApplyModifiedProperties();
   }
   var text=m.ikon!=null?m.ikon.GetComponentInChildren<TMPro.TMP_Text>(true):null;
   if(text!=null){EditorGUI.BeginChangeCheck();var arrow=EditorGUILayout.TextField("Teks / panah bubble",text.text);if(EditorGUI.EndChangeCheck()){Undo.RecordObject(text,"Ubah panah bubble");text.text=arrow;EditorUtility.SetDirty(text);}}
  }
  if(m.jalur!=null){m.jalur.Sampel(m.jalur.JarakTerdekat(m.transform.position),out var p,out _,out _);if(Vector3.Distance(p,m.transform.position)>m.radius)EditorGUILayout.HelpBox("Bubble terlalu jauh dari jalur Arthur. Klik Pusatkan tempat bubble ke jalur hijau agar bisa dijangkau.",MessageType.Warning);}
  pengaturanLanjutan=EditorGUILayout.Foldout(pengaturanLanjutan,"Pengaturan lanjutan (jalur, jangkauan, tampilan)",true);
  if(pengaturanLanjutan)DrawDefaultInspector();
 }
 void OnSceneGUI(){var m=(InteraksiJalurKampus)target;if(m.ikon!=null){Handles.color=new Color(1,.62f,.12f);Handles.DrawDottedLine(m.transform.position-Vector3.up*ArthurJalurKampus.JarakKaki,m.ikon.position,4);}}
}
[CustomEditor(typeof(JalurKampus))]
public sealed class JalurKampusInspector : Editor
{
 public override void OnInspectorGUI(){TambahTempatArthur.Panel();EditorGUILayout.Space();DrawDefaultInspector();EditorGUILayout.HelpBox("Di Scene, klik bulatan hijau lalu seret panah pada bulatan itu. Pijakan otomatis diperbarui setelah dilepas. Titik sambungan yang berimpit ikut bergerak. Untuk perubahan lewat Inspector, gunakan tombol perbarui pijakan. Matikan Play sebelum mengedit.",MessageType.Info);
  using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))if(GUILayout.Button("Perbarui pijakan dari titik jalur"))Rebuild((JalurKampus)target);
 }
 public static void Rebuild(JalurKampus r,bool gabungUndo=false){
  if(r.titik==null||r.titik.Length<2||r.titik.Any(t=>t==null)){Debug.LogWarning("Lengkapi minimal dua titik jalur.",r);return;}
  if(!gabungUndo)Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Geser lintasan Arthur");Undo.RecordObject(r,"Pijakan jalur");
  if(r.pijakanKhususJalur!=null)Undo.DestroyObjectImmediate(r.pijakanKhususJalur);
  var go=new GameObject("PijakanKhususLantai");Undo.RegisterCreatedObjectUndo(go,"Pijakan jalur");go.transform.SetParent(r.transform,true);go.transform.position=Vector3.zero;
  for(int i=1;i<r.titik.Length;i++){var d=r.titik[i].position-r.titik[i-1].position;if(d.magnitude<.001f)continue;var step=new GameObject("Pijakan_"+i);Undo.RegisterCreatedObjectUndo(step,"Pijakan jalur");step.transform.SetParent(go.transform,true);step.transform.position=(r.titik[i].position+r.titik[i-1].position)*.5f-Vector3.up*(ArthurJalurKampus.JarakKaki+.15f);step.transform.rotation=Quaternion.LookRotation(d);var box=step.AddComponent<BoxCollider>();box.size=new Vector3(1.15f,.3f,d.magnitude+.06f);}
  r.pijakanKhususJalur=go;go.SetActive(false);EditorUtility.SetDirty(r);
  foreach(var b in UnityEngine.Object.FindObjectsByType<PengaturCabangKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(b.jalurLobby==r||b.jalanDepan==r||b.jalanBelakang==r){
   Undo.RecordObject(b.transform,"Ikuti titik simpang");foreach(var wall in new[]{b.dindingDepan,b.dindingBelakang})if(wall!=null){Undo.RecordObject(wall,"Pembatas cabang");Undo.RecordObject(wall.transform,"Posisi pembatas cabang");}b.SesuaikanDenganJalur();
  }
  Undo.CollapseUndoOperations(group);
 }
}

// Scene-wide picking works even when the building or Hand tool is selected.
[InitializeOnLoad]
public static class TitikLintasanArthurScene
{
 static readonly System.Collections.Generic.HashSet<JalurKampus> pending=new System.Collections.Generic.HashSet<JalurKampus>();
 static Transform dipilih;
 static int groupGeser=-1;
 static Tool toolSemula;
 static bool menyembunyikanTool;
 static TitikLintasanArthurScene(){SceneView.duringSceneGui+=Draw;Selection.selectionChanged+=SelectionChanged;AssemblyReloadEvents.beforeAssemblyReload+=RestoreTool;EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.ExitingEditMode)SelesaikanPerubahan();RestoreTool();};}
 static void RestoreTool(){if(menyembunyikanTool&&Tools.current==Tool.None)Tools.current=toolSemula;menyembunyikanTool=false;dipilih=null;}
 static void SelectionChanged(){if(dipilih!=null&&Selection.activeTransform!=dipilih)RestoreTool();}
 static void Draw(SceneView view){
  if(EditorApplication.isPlayingOrWillChangePlaymode||!view.drawGizmos)return;
  var routes=UnityEngine.Object.FindObjectsByType<JalurKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.gameObject.scene.IsValid()&&r.gameObject.activeInHierarchy&&r.titik!=null).ToArray();
  var seen=new System.Collections.Generic.HashSet<Transform>();var oldColor=Handles.color;Handles.color=new Color(.3f,1,.2f);
  foreach(var r in routes)foreach(var t in r.titik){if(t==null||!seen.Add(t))continue;var foot=t.position-Vector3.up*ArthurJalurKampus.JarakKaki;
   if(Handles.Button(foot,Quaternion.identity,.13f,.32f,Handles.SphereHandleCap)){
    PilihTitik(t);view.Repaint();
   }
  }
  if(dipilih==null&&Selection.activeTransform!=null&&routes.Any(r=>r.titik.Contains(Selection.activeTransform)))PilihTitik(Selection.activeTransform);
  if(dipilih!=null){var foot=dipilih.position-Vector3.up*ArthurJalurKampus.JarakKaki;Handles.Label(foot+Vector3.up*.4f,dipilih.parent.name+" / "+dipilih.name);EditorGUI.BeginChangeCheck();var next=Handles.PositionHandle(foot,Quaternion.identity);if(EditorGUI.EndChangeCheck())MovePoint(dipilih,next+Vector3.up*ArthurJalurKampus.JarakKaki,routes);TambahTempatArthur.PanelScene(dipilih);}
  Handles.color=oldColor;
  if(GUIUtility.hotControl==0)SelesaikanPerubahan();
 }
 public static void PilihTitik(Transform point){Selection.activeTransform=point;if(!menyembunyikanTool){toolSemula=Tools.current;menyembunyikanTool=true;}dipilih=point;Tools.current=Tool.None;SceneView.RepaintAll();}
 public static void SelesaikanPerubahan(){if(pending.Count==0)return;foreach(var r in pending)if(r!=null)JalurKampusInspector.Rebuild(r,true);pending.Clear();if(groupGeser>=0)Undo.CollapseUndoOperations(groupGeser);groupGeser=-1;}
 public static void MovePoint(Transform point,Vector3 next,JalurKampus[] routes){
  if(pending.Count==0)groupGeser=Undo.GetCurrentGroup();
  var old=point.position;var moved=new System.Collections.Generic.HashSet<Transform>();
  foreach(var r in routes)if(r!=null&&r.titik!=null)foreach(var t in r.titik)if(t!=null&&(t==point||(t.position-old).sqrMagnitude<=.000001f))moved.Add(t);
  var affected=routes.Where(r=>r!=null&&r.titik!=null&&r.titik.Any(t=>t!=null&&moved.Contains(t))).ToArray();
  var anchors=new System.Collections.Generic.List<Anchor>();
  foreach(var m in UnityEngine.Object.FindObjectsByType<InteraksiJalurKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(affected.Contains(m.jalur)&&!moved.Contains(m.transform))anchors.Add(Capture(m.transform,m.jalur));
  foreach(var d in UnityEngine.Object.FindObjectsByType<TujuanJalurKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(affected.Contains(d.jalur)&&!moved.Contains(d.transform))anchors.Add(Capture(d.transform,d.jalur));
  foreach(var r in affected){pending.Add(r);UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(r.gameObject.scene);}
  foreach(var t in moved){Undo.RecordObject(t,"Geser lintasan Arthur");t.position=next;PrefabUtility.RecordPrefabInstancePropertyModifications(t);EditorUtility.SetDirty(t);}
  foreach(var a in anchors){Undo.RecordObject(a.target,"Ikuti lintasan Arthur");a.target.position=Vector3.Lerp(a.route.titik[a.segment].position,a.route.titik[a.segment+1].position,a.blend)+a.offset;PrefabUtility.RecordPrefabInstancePropertyModifications(a.target);EditorUtility.SetDirty(a.target);}
  SceneView.RepaintAll();
 }
 struct Anchor{public Transform target;public JalurKampus route;public int segment;public float blend;public Vector3 offset;}
 static Anchor Capture(Transform target,JalurKampus route){
  float distance=route.JarakTerdekat(target.position);int segment=0;for(;segment<route.titik.Length-2;segment++){float length=Vector3.Distance(route.titik[segment].position,route.titik[segment+1].position);if(distance<=length)break;distance-=length;}
  float size=Vector3.Distance(route.titik[segment].position,route.titik[segment+1].position);float blend=size>.001f?Mathf.Clamp01(distance/size):0;return new Anchor{target=target,route=route,segment=segment,blend=blend,offset=target.position-Vector3.Lerp(route.titik[segment].position,route.titik[segment+1].position,blend)};
 }
}
