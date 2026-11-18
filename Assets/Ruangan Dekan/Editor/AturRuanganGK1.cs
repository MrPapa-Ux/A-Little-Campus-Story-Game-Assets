using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AturRuanganGK1 : EditorWindow
{
    int pilihan;
    Vector2 scroll;
    static readonly string[] Rooms={"GK1_RuanganDekanFilkom","GK1_RuanganDosen"};
    [MenuItem("Arthur/Ruangan GK1/BUKA PENGATUR RUANGAN")]
    public static void Open(){GetWindow<AturRuanganGK1>("Atur Ruangan GK1").Show();}
    public static void BukaRuangan(bool dosen){var w=GetWindow<AturRuanganGK1>("Atur Ruangan GK1");w.pilihan=dosen?1:0;w.Show();var r=Find<JalurKampus>(Rooms[w.pilihan]);if(r!=null)Lihat(r);}
    static T Find<T>(string name) where T:Component => SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).FirstOrDefault(t=>t.name==name);
    void OnGUI()
    {
        scroll=EditorGUILayout.BeginScrollView(scroll);
        GUILayout.Label("ATUR KAMERA DAN BUBBLE",EditorStyles.boldLabel);
        pilihan=GUILayout.Toolbar(pilihan,new[]{"Dekan FILKOM","Ruangan Dosen"});
        var route=Find<JalurKampus>(Rooms[pilihan]);
        if(route==null){EditorGUILayout.HelpBox("Buka Explorer, CharacterLab, atau Chapter1_Kamar201.",MessageType.Info);EditorGUILayout.EndScrollView();return;}
        using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
        {
            if(GUILayout.Button("1. LIHAT RUANGAN",GUILayout.Height(36)))Lihat(route);
            EditorGUILayout.HelpBox("Atur tampilan di tab Scene: tahan klik kanan + W/A/S/D untuk berpindah, gerakkan mouse untuk melihat. Setelah cocok, klik tombol 2. Tab Game saat Play akan memakai sudut ini.",MessageType.Info);
            if(GUILayout.Button("2. PAKAI SUDUT PANDANG INI",GUILayout.Height(36)))Ambil(route);
            EditorGUI.BeginChangeCheck();
            float zoom=EditorGUILayout.Slider("Lebar pandangan",route.sudutPandangan,45,85);
            float size=EditorGUILayout.Slider("Ukuran Arthur",route.ukuranVisualArthur,.4f,1f);
            if(EditorGUI.EndChangeCheck()){Undo.RecordObject(route,"Atur tampilan ruangan");route.sudutPandangan=zoom;route.ukuranVisualArthur=size;Dirty(route);}
            var markers=route.GetComponentsInChildren<InteraksiJalurKampus>(true);
            foreach(var m in markers)
            {
                GUILayout.Space(8);GUILayout.Label("BUBBLE PINTU KELUAR",EditorStyles.boldLabel);
                if(GUILayout.Button("Pilih titik bubble, lalu geser dengan W"))Select(m.transform);
                EditorGUI.BeginChangeCheck();float h=EditorGUILayout.Slider("Besar bubble",m.tinggiIkonDiLayar,.07f,.18f);float gap=EditorGUILayout.Slider("Jarak di atas kepala",m.jarakDariKepala,.05f,.5f);
                if(EditorGUI.EndChangeCheck()){Undo.RecordObject(m,"Atur bubble ruangan");m.tinggiIkonDiLayar=h;m.jarakDariKepala=gap;Dirty(m);}
            }
            if(GUILayout.Button("3. SIMPAN RUANGAN",GUILayout.Height(32)))EditorSceneManager.SaveScene(route.gameObject.scene);
            if(GUILayout.Button("Pilih kelompok NPC ruangan")){var n=route.GetComponentInChildren<KelompokNpcManual>(true);if(n!=null)Select(n.transform);}
        }
        EditorGUILayout.HelpBox("Matikan Play sebelum mengatur supaya perubahan tersimpan. Ukuran Arthur hanya berlaku di ruangan yang dipilih.",MessageType.Info);
        EditorGUILayout.EndScrollView();
    }
    static void Dirty(Component c){EditorUtility.SetDirty(c);EditorSceneManager.MarkSceneDirty(c.gameObject.scene);}
    static void Select(Transform t){Selection.activeGameObject=t.gameObject;EditorGUIUtility.PingObject(t);Tools.current=Tool.Move;Tools.hidden=false;}
    static void Lihat(JalurKampus r)
    {
        var v=SceneView.lastActiveSceneView;if(v==null||r.posisiKameraUjung==null||r.bidikanKameraUjung==null)return;
        var delta=r.bidikanKameraUjung.position-r.posisiKameraUjung.position;
        v.in2DMode=false;v.orthographic=false;v.cameraSettings.fieldOfView=r.sudutPandangan;
        v.LookAtDirect(r.bidikanKameraUjung.position,Quaternion.LookRotation(delta),delta.magnitude*Mathf.Sin(r.sudutPandangan*Mathf.Deg2Rad*.5f));
        v.Focus();
    }
    static void Ambil(JalurKampus r)
    {
        var v=SceneView.lastActiveSceneView;if(v==null||v.camera==null||r.posisiKameraUjung==null||r.bidikanKameraUjung==null)return;
        Undo.RecordObjects(new Object[]{r,r.posisiKameraUjung,r.bidikanKameraUjung},"Pakai sudut Scene untuk ruangan");
        float distance=Mathf.Max(2,Vector3.Distance(r.posisiKameraUjung.position,r.bidikanKameraUjung.position));
        r.posisiKameraUjung.position=v.camera.transform.position;
        r.bidikanKameraUjung.position=v.camera.transform.position+v.camera.transform.forward*distance;
        r.kameraTetap=true;r.sudutPandangan=v.camera.fieldOfView;Dirty(r);Dirty(r.posisiKameraUjung);Dirty(r.bidikanKameraUjung);
    }
}
