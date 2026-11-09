using System.Linq;
using UnityEngine;
using UnityEditor;

public static class DaftarJalurArthur
{
    static string cari="GK1";static Vector2 scroll;
    public static void Panel(){EditorGUILayout.HelpBox("Cari nama gedung atau lantai. Pilih jalurnya untuk mengatur di Inspector, atau lihat titik pertama untuk mulai menggeser bulatan hijau. Jalur bertanda Rancangan belum dihubungkan ke teleportasi.",MessageType.Info);cari=EditorGUILayout.TextField("Cari jalur / lantai",cari);scroll=EditorGUILayout.BeginScrollView(scroll);
        foreach(var r in UnityEngine.Object.FindObjectsByType<JalurKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.gameObject.scene.IsValid()).OrderBy(r=>r.name)){if(!string.IsNullOrEmpty(cari)&&!r.name.ToLowerInvariant().Contains(cari.ToLowerInvariant()))continue;using(new EditorGUILayout.VerticalScope("box")){if(GUILayout.Button(r.name)){Selection.activeGameObject=r.gameObject;EditorGUIUtility.PingObject(r);}EditorGUILayout.LabelField("Titik",(r.titik?.Length??0).ToString());if(r.titik!=null&&r.titik.Length>0&&r.titik[0]!=null&&GUILayout.Button("Lihat titik pertama")){Selection.activeTransform=r.titik[0];SceneView.lastActiveSceneView?.Frame(new Bounds(r.titik[0].position,Vector3.one*8),false);}}}EditorGUILayout.EndScrollView();}
}
