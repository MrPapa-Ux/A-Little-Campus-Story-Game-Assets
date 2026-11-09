using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class AlatWorldArthur
{
    const string Main="Assets/Scenes/CharacterLab.unity",GH="Assets/WorldStreaming/Kawasan_JasmineGK2.unity";
    static PeganganWorldKampus[] cache;static bool memindah,menggantiPilihan,pilihanDijadwalkan;static double cek;
    static AlatWorldArthur(){EditorApplication.update+=Update;Selection.selectionChanged+=PilihSatuKesatuan;EditorApplication.hierarchyChanged+=()=>cache=null;EditorApplication.playModeStateChanged+=_=>cache=null;Undo.undoRedoPerformed+=()=>cache=null;}
    static void PilihSatuKesatuan(){if(menggantiPilihan||pilihanDijadwalkan||EditorApplication.isPlayingOrWillChangePlaymode)return;pilihanDijadwalkan=true;EditorApplication.delayCall+=RapikanPilihan;}
    static void RapikanPilihan(){pilihanDijadwalkan=false;if(menggantiPilihan||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)return;var p=UnityEngine.Object.FindObjectsByType<PeganganWorldKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(x=>x.gameObject.scene.path==Main);if(p==null)return;bool gh=Selection.gameObjects.Any(g=>g==p.gameObject||MilikGh(g.transform));if(!gh||(Selection.gameObjects.Length==1&&Selection.activeGameObject==p.gameObject))return;menggantiPilihan=true;try{Selection.objects=new UnityEngine.Object[]{p.gameObject};Tools.pivotMode=PivotMode.Pivot;}finally{menggantiPilihan=false;}}
    static bool MilikGh(Transform t){for(;t!=null;t=t.parent)if(t.name=="Asrama_GuestHouse"||t.name=="World_GuestHouse"||t.name=="Parkir_GuestHouse")return true;return false;}
    static Transform[] AkarGh(Scene s){var all=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true));var group=all.FirstOrDefault(t=>t.name=="Asrama_GuestHouse");return group!=null?new[]{group}:all.Where(t=>t.name=="World_GuestHouse"||t.name=="Parkir_GuestHouse").ToArray();}
    static Scene BukaGh(){var s=SceneManager.GetSceneByPath(GH);if(!s.isLoaded)s=EditorSceneManager.OpenScene(GH,OpenSceneMode.Additive);return s;}
    [MenuItem("Arthur/World/Pindahkan Guest House")]
    public static void Pilih(){if(EditorApplication.isPlayingOrWillChangePlaymode){Debug.Log("Hentikan Play sebelum memindahkan GH.");return;}var main=SceneManager.GetSceneByPath(Main);if(!main.isLoaded){Debug.Log("Buka CharacterLab terlebih dahulu.");return;}BukaGh();var p=main.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PeganganWorldKampus>(true)).FirstOrDefault();
        if(p==null){var go=new GameObject("PINDAHKAN WORLD GH");SceneManager.MoveGameObjectToScene(go,main);go.tag="EditorOnly";go.transform.position=new Vector3(294,2,-18);p=go.AddComponent<PeganganWorldKampus>();p.posisiTersimpan=go.transform.position;Undo.RegisterCreatedObjectUndo(go,"Pegangan world GH");EditorSceneManager.MarkSceneDirty(main);}
        SiapkanRotasi(p);Selection.objects=new UnityEngine.Object[]{p.gameObject};Tools.current=Tool.Move;Tools.pivotMode=PivotMode.Pivot;EditorGUIUtility.PingObject(p);SceneVisibilityManager.instance.ExitIsolation();var v=SceneView.lastActiveSceneView;if(v!=null){v.drawGizmos=true;v.LookAt(p.transform.position,Quaternion.Euler(90,0,0),80,true,true);}DaftarTempatArthur.OpenWorld();SceneView.RepaintAll();}
    [MenuItem("Arthur/World/Putar Guest House")]
    public static void PilihPutar(){Pilih();Tools.current=Tool.Rotate;}
    static void SiapkanRotasi(PeganganWorldKampus p){if(p.rotasiSiap)return;p.rotasiTersimpan=Quaternion.Euler(0,p.transform.eulerAngles.y,0);p.rotasiSiap=true;EditorUtility.SetDirty(p);}
    static void Update(){if(memindah||EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.timeSinceStartup<cek)return;cek=EditorApplication.timeSinceStartup+.04;if(cache==null)cache=UnityEngine.Object.FindObjectsByType<PeganganWorldKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(p=>p.gameObject.scene.path==Main).ToArray();foreach(var p in cache)if(p!=null){SiapkanRotasi(p);if((p.transform.position-p.posisiTersimpan).sqrMagnitude>.000001f||Quaternion.Angle(p.transform.rotation,p.rotasiTersimpan)>.001f)Sinkron(p);}}
    public static void GeserKe(PeganganWorldKampus p,Vector3 posisi){Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Pindahkan world GH dan jalur");Undo.RecordObject(p.transform,"Pegangan GH");p.transform.position=posisi;Sinkron(p);Undo.FlushUndoRecordObjects();Undo.CollapseUndoOperations(group);}
    public static void PutarKe(PeganganWorldKampus p,float derajat){SiapkanRotasi(p);Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Putar seluruh GH dan jalur");Undo.RecordObject(p.transform,"Putar pegangan GH");p.transform.rotation=Quaternion.Euler(0,derajat,0);Sinkron(p);Undo.FlushUndoRecordObjects();Undo.CollapseUndoOperations(group);}
    static void Tambah(HashSet<Transform> set,Transform t){if(t!=null)set.Add(t);}
    public static void Sinkron(PeganganWorldKampus p){if(memindah||EditorApplication.isPlayingOrWillChangePlaymode)return;SiapkanRotasi(p);var posisiLama=p.posisiTersimpan;var posisiBaru=p.transform.position;var rotasiBaru=Quaternion.Euler(0,p.transform.eulerAngles.y,0);var putaran=rotasiBaru*Quaternion.Inverse(p.rotasiTersimpan);var delta=posisiBaru-posisiLama;if(delta.sqrMagnitude<.000001f&&Quaternion.Angle(putaran,Quaternion.identity)<.001f){p.transform.rotation=rotasiBaru;return;}memindah=true;try{
        var gh=BukaGh();var main=SceneManager.GetSceneByPath(Main);var roots=main.GetRootGameObjects();var paths=roots.SelectMany(g=>g.GetComponentsInChildren<JalurKampus>(true)).Where(r=>r.name.StartsWith("GH_",StringComparison.Ordinal)).ToArray();var set=new HashSet<Transform>();
        foreach(var t in AkarGh(gh))Tambah(set,t);
        foreach(var r in paths){Tambah(set,r.transform);foreach(var t in r.titik??Array.Empty<Transform>())Tambah(set,t);if(r.pijakanKhususJalur!=null)Tambah(set,r.pijakanKhususJalur.transform);foreach(var t in new[]{r.posisiKameraAwal,r.bidikanKameraAwal,r.posisiKameraUjung,r.bidikanKameraUjung})Tambah(set,t);foreach(var b in r.bingkaiLokal??Array.Empty<JalurKampus.BingkaiLokal>())if(b!=null){Tambah(set,b.pusat);Tambah(set,b.posisiKamera);Tambah(set,b.bidikan);}}
        foreach(var m in roots.SelectMany(g=>g.GetComponentsInChildren<InteraksiJalurKampus>(true)))if(m.jalur!=null&&paths.Contains(m.jalur))Tambah(set,m.transform);
        foreach(var d in roots.SelectMany(g=>g.GetComponentsInChildren<TujuanJalurKampus>(true)))if(d.jalur!=null&&paths.Contains(d.jalur))Tambah(set,d.transform);
        foreach(var n in roots.SelectMany(g=>g.GetComponentsInChildren<KelompokNpcManual>(true)))if(n.kawasan=="GH")Tambah(set,n.transform);
        var ambiance=roots.SelectMany(g=>g.GetComponentsInChildren<SuasanaKampus>(true)).FirstOrDefault();if(ambiance!=null&&ambiance.suasanaGH!=null)Tambah(set,ambiance.suasanaGH.transform);
        var top=set.Where(t=>!set.Any(other=>other!=t&&t.IsChildOf(other))).ToArray();var tracks=paths.Select(r=>r.lintasanKamera).Where(t=>t!=null).Distinct().ToArray();Undo.RecordObjects(top.Cast<UnityEngine.Object>().Concat(paths).Concat(tracks).Concat(new UnityEngine.Object[]{p,p.transform}).Distinct().ToArray(),"Atur seluruh world GH dan jalur");
        Func<Vector3,Vector3> titik=v=>posisiBaru+putaran*(v-posisiLama);
        foreach(var t in top){t.SetPositionAndRotation(titik(t.position),putaran*t.rotation);if(t is RectTransform rect)rect.anchoredPosition3D=rect.localPosition;PrefabUtility.RecordPrefabInstancePropertyModifications(t);EditorUtility.SetDirty(t);}
        float yaw=Mathf.DeltaAngle(0,putaran.eulerAngles.y);
        foreach(var r in paths){if(r.arahKamera!=null)for(int i=0;i<r.arahKamera.Length;i++)r.arahKamera[i]+=yaw;r.pusatAcuanBatas=titik(r.batasDalamAcuanWorld?r.pusatAcuanBatas:Vector3.zero);r.rotasiAcuanBatas=putaran*(r.batasDalamAcuanWorld?r.rotasiAcuanBatas:Quaternion.identity);r.batasDalamAcuanWorld=true;EditorUtility.SetDirty(r);}
        foreach(var track in tracks){track.pusatAcuan=titik(track.parameterDalamAcuanWorld?track.pusatAcuan:Vector3.zero);track.rotasiAcuan=putaran*(track.parameterDalamAcuanWorld?track.rotasiAcuan:Quaternion.identity);track.parameterDalamAcuanWorld=true;for(int i=0;i<track.bingkai.Length;i++){var b=track.bingkai[i];b.posisi=titik(b.posisi);b.bidikan=titik(b.bidikan);track.bingkai[i]=b;}EditorUtility.SetDirty(track);}
        p.transform.rotation=rotasiBaru;p.posisiTersimpan=posisiBaru;p.rotasiTersimpan=rotasiBaru;EditorUtility.SetDirty(p);EditorSceneManager.MarkSceneDirty(main);EditorSceneManager.MarkSceneDirty(gh);SceneView.RepaintAll();
    }finally{memindah=false;}}
    public static void Simpan(){if(EditorApplication.isPlayingOrWillChangePlaymode)return;var main=SceneManager.GetSceneByPath(Main);var gh=SceneManager.GetSceneByPath(GH);if(main.isLoaded)EditorSceneManager.SaveScene(main);if(gh.isLoaded)EditorSceneManager.SaveScene(gh);}
    [DrawGizmo(GizmoType.Selected|GizmoType.Active)]
    static void GambarWorld(PeganganWorldKampus p,GizmoType tipe){var s=SceneManager.GetSceneByPath(GH);if(!s.isLoaded)return;var rs=AkarGh(s).SelectMany(t=>t.GetComponentsInChildren<Renderer>(true)).ToArray();if(rs.Length==0)return;var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);Gizmos.color=Color.cyan;Gizmos.DrawWireCube(b.center,b.size);Handles.Label(p.transform.position+Vector3.up*3,"SELURUH GH — W: geser | E: putar lingkaran hijau");}
    public static void Panel(){EditorGUILayout.HelpBox("Pilih satu PEGANGAN lewat tombol di bawah, bukan potongan bangunannya. W: geser seluruh GH. E: putar dengan lingkaran HIJAU (sumbu Y). Kotak biru muda menunjukkan seluruh model GH yang ikut. Jalur, parkiran, bubble dan kamera ikut bersama. Ctrl+Z: batalkan. Setelah cocok, simpan kedua scene.",MessageType.Info);using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode)){if(GUILayout.Button("GESER seluruh GH (W)"))Pilih();if(GUILayout.Button("PUTAR seluruh GH (E)"))PilihPutar();var p=Selection.activeGameObject!=null?Selection.activeGameObject.GetComponent<PeganganWorldKampus>():null;if(p!=null){EditorGUI.BeginChangeCheck();float yaw=EditorGUILayout.FloatField("Putaran mendatar (derajat)",p.transform.eulerAngles.y);if(EditorGUI.EndChangeCheck())PutarKe(p,yaw);}if(GUILayout.Button("Simpan GH dan jalurnya"))Simpan();}EditorGUILayout.LabelField("Objek di Hierarchy: PINDAHKAN WORLD GH",EditorStyles.wordWrappedLabel);}
}




