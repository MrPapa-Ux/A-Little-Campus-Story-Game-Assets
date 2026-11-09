using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Panel tambahan pada daftar tempat yang sudah ada; tidak memindahkan NPC lama.
public static class AlatNpcArthur
{
    public const string Prefab="Assets/NpcKampus/Template/NPC_Mahasiswa.prefab";
    static readonly string[] kawasan={"JasmineGK2","GA","GH","TamanUNKLAB","GK1","Pioneer"};
    static int pilihan;static string cari="";static Vector2 scroll;static bool klikLantai;
    static AlatNpcArthur(){SceneView.duringSceneGui+=SceneGui;EditorApplication.playModeStateChanged+=_=>klikLantai=false;}
    [MenuItem("Arthur/NPC/Tambah atau cari NPC")]
    static void Buka(){DaftarTempatArthur.OpenNpc();}
    public static GameObject Tambah(Vector3 kaki,string id)
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Hentikan Play untuk menambah NPC.");
        var scene=SceneManager.GetSceneByPath("Assets/Scenes/CharacterLab.unity");if(!scene.IsValid()||!scene.isLoaded)throw new InvalidOperationException("Buka CharacterLab terlebih dahulu.");
        var template=AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);if(template==null)throw new InvalidOperationException("Template NPC belum tersedia.");
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Tambah NPC Arthur");
        var parent=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<KelompokNpcManual>(true)).FirstOrDefault(g=>g.kawasan==id);
        if(parent==null){var folder=new GameObject("NPC_Manual_"+id);SceneManager.MoveGameObjectToScene(folder,scene);Undo.RegisterCreatedObjectUndo(folder,"Kelompok NPC");parent=Undo.AddComponent<KelompokNpcManual>(folder);parent.kawasan=id;}
        var go=(GameObject)PrefabUtility.InstantiatePrefab(template,scene);Undo.RegisterCreatedObjectUndo(go,"Tambah NPC");Undo.SetTransformParent(go.transform,parent.transform,"Kawasan NPC");go.name=GameObjectUtility.GetUniqueNameForSibling(parent.transform,"NPC_Mahasiswa");AturPosisi(go,kaki);EditorSceneManager.MarkSceneDirty(scene);Undo.CollapseUndoOperations(group);Pilih(go);return go;
    }
    public static GameObject Duplikat(GameObject source)
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||source==null)return null;
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Duplikat NPC Arthur");
        var clone=UnityEngine.Object.Instantiate(source,source.transform.parent);clone.name=GameObjectUtility.GetUniqueNameForSibling(source.transform.parent,source.name);Undo.RegisterCreatedObjectUndo(clone,"Duplikat NPC");AturPosisi(clone,source.transform.position+Vector3.right*1.8f);EditorSceneManager.MarkSceneDirty(clone.scene);Undo.CollapseUndoOperations(group);Pilih(clone);return clone;
    }
    static void AturPosisi(GameObject go,Vector3 p){go.transform.position=p;if(go.transform is RectTransform r)r.anchoredPosition3D=r.localPosition;PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);EditorUtility.SetDirty(go.transform);}
    static void Pilih(GameObject g){Selection.activeGameObject=g;EditorGUIUtility.PingObject(g);Tools.current=Tool.Move;Tools.hidden=false;Tools.pivotRotation=PivotRotation.Global;Tools.pivotMode=PivotMode.Pivot;var view=SceneView.lastActiveSceneView;if(view!=null){view.drawGizmos=true;view.Frame(new Bounds(g.transform.position+Vector3.up,Vector3.one*3),false);}}
    [MenuItem("Arthur/NPC/Geser seluruh NPC terpilih")]
    static void GeserTerpilih(){var g=AkarNpcTerpilih();if(g!=null)Pilih(g);else Buka();}
    static GameObject AkarNpcTerpilih(){var g=Selection.activeGameObject;if(g==null)return null;var n=g.GetComponentInParent<NpcKampusHidup>(true);if(n!=null)return n.gameObject;var a=g.GetComponentInParent<AreaInteraksi>(true);return a!=null&&a.tipeInteraksi==TipeInteraksi.NPC_Karakter?a.gameObject:null;}
    static GameObject[] Semua()=>UnityEngine.Object.FindObjectsByType<AreaInteraksi>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(a=>a.tipeInteraksi==TipeInteraksi.NPC_Karakter&&a.gameObject.scene.IsValid()).Select(a=>a.gameObject).Concat(UnityEngine.Object.FindObjectsByType<NpcKampusHidup>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(n=>n.gameObject)).Distinct().OrderBy(g=>g.name).ToArray();
    public static void Panel()
    {
        EditorGUILayout.HelpBox("Tambah satu NPC, klik lantainya, lalu geser dengan W. Ctrl+D atau Ctrl+C/Ctrl+V menggandakan NPC. Pilih NPC pada daftar untuk menemukan lokasi dan membaca dialognya. Simpan scene dengan Ctrl+S saat Play berhenti.",MessageType.Info);
        var dipilih=AkarNpcTerpilih();
        if(dipilih!=null){EditorGUILayout.LabelField("NPC terpilih",dipilih.name);if(GUILayout.Button("Geser SELURUH NPC ini (panah W)"))Pilih(dipilih);}
        pilihan=EditorGUILayout.Popup("Kawasan NPC baru",pilihan,kawasan);
        using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode)){
            if(GUILayout.Button(klikLantai?"Klik lantai di Scene — Esc membatalkan":"+ NPC: klik lantai di Scene")){klikLantai=!klikLantai;SceneView.RepaintAll();}
            if(GUILayout.Button("+ NPC di posisi objek terpilih")){var p=Selection.activeTransform!=null?Selection.activeTransform.position:SceneView.lastActiveSceneView!=null?SceneView.lastActiveSceneView.pivot:Vector3.zero;Tambah(p,kawasan[pilihan]);}
        }
        cari=EditorGUILayout.TextField("Cari NPC / dialog",cari);var list=Semua();EditorGUILayout.LabelField("NPC ditemukan",list.Length.ToString());scroll=EditorGUILayout.BeginScrollView(scroll);
        foreach(var g in list){var area=g.GetComponent<AreaInteraksi>();var simple=g.GetComponent<NpcYangBisaDiatur>();var text=area!=null&&area.skenarioDialog!=null?string.Join(" ",area.skenarioDialog.Select(b=>b.teksDialog)):"";var region=g.GetComponentInParent<KelompokNpcManual>(true);string label=g.name+" | "+(region!=null?region.kawasan:g.transform.position.x>1000?"Taman / kampus":"Jasmine / kantin");if(!string.IsNullOrEmpty(cari)&&!(label+" "+text).ToLowerInvariant().Contains(cari.ToLowerInvariant()))continue;
            using(new EditorGUILayout.VerticalScope("box")){if(GUILayout.Button(label+"\n"+g.transform.position.ToString("F2")))Pilih(g);if(!string.IsNullOrEmpty(text))EditorGUILayout.LabelField(text,EditorStyles.wordWrappedLabel);using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode)){if(GUILayout.Button("Duplikat NPC ini"))Duplikat(g);if(Selection.activeGameObject==g&&simple!=null){using(var data=new SerializedObject(simple)){data.Update();EditorGUILayout.PropertyField(data.FindProperty("namaNpc"));EditorGUILayout.PropertyField(data.FindProperty("dialogSederhana"));if(simple.dialogSederhana)EditorGUILayout.PropertyField(data.FindProperty("dialog"));data.ApplyModifiedProperties();}if(!simple.dialogSederhana&&area!=null){using(var data=new SerializedObject(area)){data.Update();EditorGUILayout.PropertyField(data.FindProperty("skenarioDialog"),new GUIContent("Baris dialog (bisa diubah)"),true);data.ApplyModifiedProperties();}}}}
            }
        }EditorGUILayout.EndScrollView();
    }
    static bool Lantai(Vector2 mouse,out Vector3 p)
    {
        var ray=HandleUtility.GUIPointToWorldRay(mouse);if(Physics.Raycast(ray,out var hit,3000,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)&&hit.normal.y>.5f){p=hit.point+Vector3.up*.025f;return true;}
        var picked=HandleUtility.PickGameObject(mouse,false);var mesh=picked!=null?picked.GetComponent<MeshFilter>():null;
        if(mesh!=null&&mesh.sharedMesh!=null){MeshCollider c=null;try{c=picked.AddComponent<MeshCollider>();c.sharedMesh=mesh.sharedMesh;if(c.Raycast(ray,out hit,3000)&&hit.normal.y>.5f){p=hit.point+Vector3.up*.025f;return true;}}finally{if(c!=null)UnityEngine.Object.DestroyImmediate(c);}}
        p=default;return false;
    }
    static void SceneGui(SceneView view)
    {
        if(!klikLantai||EditorApplication.isPlayingOrWillChangePlaymode)return;var e=Event.current;HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));if(e.type==EventType.KeyDown&&e.keyCode==KeyCode.Escape){klikLantai=false;e.Use();view.Repaint();return;}
        bool valid=Lantai(e.mousePosition,out var p);if(valid){Handles.color=Color.cyan;Handles.DrawWireDisc(p,Vector3.up,.45f);Handles.Label(p+Vector3.up*.2f,"Tambah NPC di sini");}
        Handles.BeginGUI();GUILayout.BeginArea(new Rect(12,12,330,58),EditorStyles.helpBox);GUILayout.Label("Klik lantai untuk satu NPC. Esc: batal.");GUILayout.Label("Kawasan: "+kawasan[pilihan]);GUILayout.EndArea();Handles.EndGUI();
        if(valid&&e.type==EventType.MouseDown&&e.button==0&&!e.alt){klikLantai=false;Tambah(p,kawasan[pilihan]);e.Use();}if(e.type==EventType.MouseMove)view.Repaint();
    }
}
