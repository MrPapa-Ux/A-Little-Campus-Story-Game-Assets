using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class KameraSemuaJalurArthur : EditorWindow
{
    JalurKampus jalur; string cari=""; Vector2 scroll; bool ikut;
    public static void Buka(){GetWindow<KameraSemuaJalurArthur>("Kamera Semua Jalur").Show();}
    public static void Pintu(bool dosen)
    {
        PanelWorldKampus.Tampilkan(3,false);
        var m=Object.FindObjectsByType<InteraksiJalurKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(x=>x.name==(dosen?"GK1_Masuk_RuanganDosen":"GK1_Masuk_RuanganDekanFilkom"));
        if(m==null){Debug.LogWarning("Titik pintu tidak ditemukan di scene utama yang terbuka.");return;}
        Selection.activeGameObject=m.gameObject;Tools.current=Tool.Move;
        var v=SceneView.lastActiveSceneView;
        if(v!=null){v.in2DMode=false;v.orthographic=false;v.LookAtDirect(m.transform.position+Vector3.up*.5f,Quaternion.Euler(8,90,0),5);v.Repaint();}
    }
    void OnGUI()
    {
        EditorGUILayout.HelpBox("Berlaku untuk SEMUA jalur. Pilih jalur, atur sudut di Scene, lalu pakai sudut tersebut. Perubahan hanya berlaku pada jalur pilihanmu.",MessageType.Info);
        using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
        {
            if(GUILayout.Button("Pakai jalur objek yang sedang dipilih",GUILayout.Height(28)))
            {
                var t=Selection.activeTransform;
                if(t!=null){jalur=t.GetComponentInParent<JalurKampus>();var m=t.GetComponentInParent<InteraksiJalurKampus>();if(jalur==null&&m!=null)jalur=m.jalur;}
            }
            jalur=(JalurKampus)EditorGUILayout.ObjectField("Jalur pilihan",jalur,typeof(JalurKampus),true);
            cari=EditorGUILayout.TextField("Cari nama jalur",cari);
            scroll=EditorGUILayout.BeginScrollView(scroll,GUILayout.Height(140));
            foreach(var r in Object.FindObjectsByType<JalurKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.name.ToLowerInvariant().Contains(cari.ToLowerInvariant())).OrderBy(r=>r.name))
                if(GUILayout.Button(r.name))jalur=r;
            EditorGUILayout.EndScrollView();
            if(jalur==null)return;
            GUILayout.Label(jalur.name,EditorStyles.boldLabel);
            if(GUILayout.Button("1. LIHAT JALUR DAN BANGUNANNYA",GUILayout.Height(32)))
            {
                Selection.activeGameObject=jalur.gameObject;PanelWorldKampus.TampilkanPilihan();
                var v=SceneView.lastActiveSceneView;var k=jalur.GetComponent<KameraPilihanPemilik>();
                if(k!=null&&k.gunakan)Lihat(v,k.posisi,k.bidikan,k.sudut);
                else if(jalur.posisiKameraUjung!=null&&jalur.bidikanKameraUjung!=null)Lihat(v,jalur.posisiKameraUjung.position,jalur.bidikanKameraUjung.position,jalur.sudutPandangan);
            }
            EditorGUILayout.HelpBox("Di Scene: tahan klik kanan, gerakkan mouse; W/A/S/D untuk bergeser. Roda mouse untuk mendekat. Tidak perlu Play.",MessageType.None);
            ikut=EditorGUILayout.Toggle("Kamera ikut Arthur berjalan",ikut);
            if(GUILayout.Button("2. PAKAI SUDUT SCENE INI",GUILayout.Height(32)))
            {
                var v=SceneView.lastActiveSceneView;if(v==null||v.camera==null)return;
                var k=jalur.GetComponent<KameraPilihanPemilik>();if(k==null)k=Undo.AddComponent<KameraPilihanPemilik>(jalur.gameObject);
                Undo.RecordObject(k,"Atur kamera jalur");k.gunakan=true;k.ikutiArthur=ikut;k.posisi=v.camera.transform.position;
                jalur.Sampel(jalur.Panjang*.5f,out k.acuan,out _,out _);
                k.bidikan=k.posisi+v.camera.transform.forward*Mathf.Max(2,Vector3.Distance(k.posisi,k.acuan));k.sudut=v.camera.fieldOfView;
                EditorUtility.SetDirty(k);EditorSceneManager.MarkSceneDirty(jalur.gameObject.scene);
            }
            var custom=jalur.GetComponent<KameraPilihanPemilik>();
            if(custom!=null&&custom.gunakan&&GUILayout.Button("Kembalikan kamera asli jalur ini")){Undo.RecordObject(custom,"Kembalikan kamera asli");custom.gunakan=false;EditorUtility.SetDirty(custom);EditorSceneManager.MarkSceneDirty(jalur.gameObject.scene);}
            if(GUILayout.Button("3. SIMPAN",GUILayout.Height(30)))EditorSceneManager.SaveScene(jalur.gameObject.scene);
        }
    }
    static void Lihat(SceneView v,Vector3 p,Vector3 b,float fov)
    {if(v==null||(b-p).sqrMagnitude<.001f)return;v.in2DMode=false;v.orthographic=false;v.cameraSettings.fieldOfView=fov;v.LookAtDirect(b,Quaternion.LookRotation(b-p),(b-p).magnitude*Mathf.Sin(fov*Mathf.Deg2Rad*.5f));}
}

