using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Pratinjau hanya untuk editor: tidak menyimpan semua kawasan sebagai aktif di build.
public sealed class PanelWorldKampus : EditorWindow
{
    static readonly string[] Label = { "Jasmine, Guest House, GK2 dan Kantin", "GA", "Taman UNKLAB, Crystal, Edel dan Dining", "GK1", "Pioneer Chapel", "Kamar 201" };
    static readonly string[] Id = { "JasmineGK2", "GA", "TamanUNKLAB", "GK1", "Pioneer", "GHRoom" };
    static readonly string[] File = { "JasmineGK2", "GA", "TamanUNKLAB", "GK1", "Pioneer", "GH_Kamar201" };
    static GameObject prefab;
    static string pesan;
    static Vector2 scroll;

    [MenuItem("Arthur/World/Tampilkan atau sembunyikan world")]
    [MenuItem("Arthur/BUKA ALAT WORLD DAN PREFAB", false, 0)]
    public static void Buka() { var w=GetWindow<PanelWorldKampus>("World dan Prefab");w.minSize=new Vector2(440,680);w.Show(); }
    void OnGUI() { Panel(); }
    public static void Panel()
    {
        EditorGUILayout.HelpBox("Bangunan berada dalam scene kawasan terpisah. Klik TAMPILKAN agar bangunan terlihat bersama NPC. Tidak perlu Play. Perubahan posisi NPC disimpan dengan Ctrl+S.", MessageType.Info);
        using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
        {
            if (GUILayout.Button("Tampilkan world sesuai NPC / jalur terpilih", GUILayout.Height(30))) TampilkanPilihan();
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(300));
            for (int i = 0; i < File.Length; i++)
            {
                int index = i;
                var s = SceneManager.GetSceneByPath(Path(i));
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField(Label[i], EditorStyles.boldLabel);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(s.isLoaded ? "Fokus world" : "TAMPILKAN")) Tampilkan(index, true);
                        using (new EditorGUI.DisabledScope(!s.isLoaded))
                            if (GUILayout.Button("Tutup pratinjau")) Tutup(index);
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Prefab pilihanmu", EditorStyles.boldLabel);
            prefab = (GameObject)EditorGUILayout.ObjectField("Tarik prefab ke sini", prefab, typeof(GameObject), false);
            using (new EditorGUI.DisabledScope(prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab)))
                if (GUILayout.Button("Tambahkan prefab ke Scene")) TambahkanPrefab();
            var g = Selection.activeGameObject;
            using (new EditorGUI.DisabledScope(g == null || !g.scene.IsValid()))
            {
                if (GUILayout.Button("Aktifkan objek terpilih beserta induknya")) AktifkanPilihan();
                if (GUILayout.Button("Nonaktifkan objek terpilih"))
                {
                    Undo.RecordObject(g, "Nonaktifkan objek"); g.SetActive(false);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(g);
                    EditorSceneManager.MarkSceneDirty(g.scene);
                }
            }
        }
        if (!string.IsNullOrEmpty(pesan)) EditorGUILayout.HelpBox(pesan, MessageType.None);
        EditorGUILayout.HelpBox("Tampilkan/Tutup pratinjau hanya memuat atau menutup scene di editor. Saat Play, game tetap memuat kawasan sesuai lokasi Arthur. Aktivasi NPC saat malam tetap mengikuti aturan permainan.", MessageType.None);
    }
    static string Path(int i) => "Assets/WorldStreaming/Kawasan_" + File[i] + ".unity";
    public static Scene Tampilkan(int i, bool fokus)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return default;
        var aktif = SceneManager.GetActiveScene();
        var s = SceneManager.GetSceneByPath(Path(i));
        if (!s.isLoaded) s = EditorSceneManager.OpenScene(Path(i), OpenSceneMode.Additive);
        if (aktif.IsValid() && aktif.isLoaded) SceneManager.SetActiveScene(aktif);
        foreach (var root in s.GetRootGameObjects()) SceneVisibilityManager.instance.Show(root, true);
        SceneVisibilityManager.instance.ExitIsolation();
        if (fokus && SceneView.lastActiveSceneView != null)
        {
            var rs = s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Renderer>(true)).Where(r => r.enabled && r.gameObject.activeInHierarchy).ToArray();
            if (rs.Length > 0)
            {
                var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
                SceneView.lastActiveSceneView.Frame(b, false);
            }
        }
        pesan = Label[i] + " sudah ditampilkan. Pilih NPC di Hierarchy, tekan F untuk fokus dan W untuk menggeser.";
        SceneView.RepaintAll(); return s;
    }
    public static void TampilkanPilihan()
    {
        var t = Selection.activeTransform;
        if (t == null) { pesan = "Pilih NPC atau titik jalurnya terlebih dahulu, atau gunakan tombol kawasan di bawah."; return; }
        var kelompok = t.GetComponentInParent<KelompokNpcManual>(true);
        var jalur = t.GetComponentInParent<JalurKampus>(true);
        var interaksi = t.GetComponentInParent<InteraksiJalurKampus>(true);
        var tujuan = t.GetComponentInParent<TujuanJalurKampus>(true);
        if (jalur == null && interaksi != null) jalur = interaksi.jalur;
        if (jalur == null && tujuan != null) jalur = tujuan.jalur;
        string id = kelompok != null ? kelompok.kawasan : jalur != null ? KawasanKampus.IdUntuk(jalur) : null;
        if (id == "GH") id = "JasmineGK2";
        int index = Array.IndexOf(Id, id);
        if (index < 0) { pesan = "Kawasan objek ini belum diketahui. Pilih langsung tombol GA, Taman, atau kawasan lainnya."; return; }
        Tampilkan(index, false);
        SceneView.lastActiveSceneView?.Frame(new Bounds(t.position, Vector3.one * 12), false);
    }
    static void Tutup(int i)
    {
        var s = SceneManager.GetSceneByPath(Path(i));
        if (!s.isLoaded || (s.isDirty && !EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new[] { s }))) return;
        if (SceneManager.sceneCount <= 1) { pesan = "Buka scene utama (CharacterLab atau Explorer) sebelum menutup kawasan."; return; }
        EditorSceneManager.CloseScene(s, true);
    }
    static void TambahkanPrefab()
    {
        var s = SceneManager.GetActiveScene();
        var posisi = Selection.activeTransform != null ? Selection.activeTransform.position : SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.pivot : Vector3.zero;
        var g = (GameObject)PrefabUtility.InstantiatePrefab(prefab, s);
        Undo.RegisterCreatedObjectUndo(g, "Tambahkan prefab");
        g.transform.position = posisi; g.SetActive(true); Selection.activeGameObject = g;
        Tools.current = Tool.Move; EditorSceneManager.MarkSceneDirty(s);
        pesan = "Prefab ditambahkan pada scene aktif. Geser dengan W lalu simpan Ctrl+S. Untuk NPC gunakan menu Arthur > NPC agar dialognya ikut disiapkan.";
    }
    public static void AktifkanPilihan()
    {
        var g = Selection.activeGameObject; if (g == null || !g.scene.IsValid()) return;
        for (var t = g.transform; t != null; t = t.parent)
        {
            if (t.gameObject.activeSelf) continue;
            Undo.RecordObject(t.gameObject, "Aktifkan objek dan induk"); t.gameObject.SetActive(true);
            PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);
        }
        SceneVisibilityManager.instance.Show(g, true);
        EditorSceneManager.MarkSceneDirty(g.scene); SceneView.RepaintAll();
    }
}

// Akses satu klik selalu terlihat di Scene; tidak perlu menelusuri submenu.
[InitializeOnLoad]
public static class TombolWorldDiScene
{
    static TombolWorldDiScene()
    {
        SceneView.duringSceneGui -= Gambar;
        SceneView.duringSceneGui += Gambar;
    }
    static void Gambar(SceneView view)
    {
        Handles.BeginGUI();
        var kotak = new Rect(Mathf.Max(12, view.position.width - 350), 10, 245, 63);
        GUILayout.BeginArea(kotak, GUIContent.none, EditorStyles.helpBox);
        var warna = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, .65f, .22f);
        if (GUILayout.Button("BUKA ALAT WORLD / PREFAB", GUILayout.Height(30)))
            PanelWorldKampus.Buka();
        GUI.backgroundColor = warna;
        GUILayout.Label("Pilih kawasan, lalu klik TAMPILKAN.", EditorStyles.miniLabel);
        GUILayout.EndArea();
        Handles.EndGUI();
    }
}
