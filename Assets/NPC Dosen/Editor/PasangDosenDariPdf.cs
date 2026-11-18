using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Source order and faculty assignments follow Dr.pdf, not current web staff lists.
[InitializeOnLoad]
public static class PasangDosenDariPdf
{
    const string Work = "work/npc-dr-preview/pemasangan/";
    static readonly string[] Names = {"Elvis R Sumanti","Fanny Soewignyo","Deske W Mandagi","Ika Prayanthi","Moody Wahongan","Stenly R Pungus","Semmy W Taju","Argha O Silitonga","Julio Kolapitawondal","Joe Y Y Mambu","Green Ferry Mandias","Green A Sandag"};
    static readonly string[] Labels = {"Elvis R. Sumanti","Fanny Soewignyo","Deske W. Mandagi","Ika Prayanthi","Moody Wahongan","Stenly R. Pungus","Semmy W. Taju","Argha O. Silitonga","Julio Kolapitawondal","Joe Y. Y. Mambu","Green Ferry Mandias","Green A. Sandag"};
    static readonly string[] Roles = {"Dekan FEB","Dosen FEB","Kaprodi Pascasarjana (MM)","Kaprodi Akuntansi","Kaprodi Manajemen","Dekan FILKOM","Kaprodi Informatika","Dosen FILKOM","Dosen FILKOM","Dosen FILKOM","Dosen FILKOM","Dosen FILKOM"};
    static PasangDosenDariPdf() { EditorApplication.update += Tick; }
    static T[] All<T>(Scene s) where T : Component => s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
    static void Tick()
    {
        if (!File.Exists(Work + "pasang-gk1-lantai4.request") || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(Work + "pasang-gk1-lantai4.request");
        try { Run(); } catch (Exception e) { File.WriteAllText(Work + "hasil.txt", e.ToString()); Debug.LogException(e); }
    }
    static string Faculty(int i) => i < 5 ? "FEKON" : "FILKOM";
    static string HeadPath(int i) => "Assets/NPC Dosen/" + Faculty(i) + "/Kepala/" + Names[i] + ".png";
    static string PrefabPath(int i) => "Assets/NPC Dosen/" + Faculty(i) + "/Prefab/" + Names[i] + ".prefab";
    static void Configure(GameObject go, int i)
    {
        go.name = Labels[i] + " - " + Faculty(i);
        var n = go.GetComponent<NpcYangBisaDiatur>();
        n.namaNpc = Labels[i]; n.dialogSederhana = true;
        // Editable fictional introduction; no invented quote or academic advice.
        n.dialog = "Selamat datang. Saya " + Labels[i] + ", " + Roles[i] + ".";
        n.jarakInteraksi = 2.1f; n.Terapkan();
        var life = go.GetComponent<NpcKampusHidup>();
        var head = life.kepala.GetComponent<SpriteRenderer>();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HeadPath(i));
        if (sprite == null) throw new Exception("Sprite belum terbaca: " + HeadPath(i));
        head.sprite = sprite; head.color = Color.white;
        life.kepala.localRotation = Quaternion.identity;
        // The new head already contains its own hair and ears.
        foreach (var t in life.kepala.GetComponentsInChildren<Transform>(true))
            if (t != life.kepala && (t.name.StartsWith("Rambut") || t.name == "Telinga")) t.gameObject.SetActive(false);
        var face = head.GetComponent<EkspresiWajah2D>();
        face.animasiKedip = false; face.gunakanEkspresi = false;
        face.posisiMulut = new Vector2(0, -.29f);
        face.ukuranMaskMulut = new Vector2(.19f, .075f);
        // Mouth locations measured on each already-approved head crop, normalized to head height.
        float[] mouthY = {-.29f,-.30f,-.30f,-.29f,-.30f,-.30f,-.29f,-.30f,-.30f,-.30f,-.29f,-.30f};
        face.posisiMulut = new Vector2(0, mouthY[i]);
        var area = go.GetComponent<AreaInteraksi>(); area.potretDialog = sprite;
        var bubble = go.GetComponent<NPCBubbleTrigger>(); bubble.kepalaNpc = head;
        bubble.arahkanEkorKeKepala = true; bubble.jarakEkorDariMulut = .16f;
        life.kegiatan = KegiatanNpcKampus.Berdiri; life.fase = i * .71f;
        life.idleArthur = AssetDatabase.LoadAssetAtPath<PolaIdleArthurNpc>("Assets/NpcKampus/Template/PolaIdleArthurNpc.asset");
        life.kakiKiri = go.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "KakiKiri");
        life.kakiKanan = go.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "KakiKanan");
        foreach (var c in go.GetComponentsInChildren<Collider>(true)) c.enabled = false;
        foreach (var c in go.GetComponentsInChildren<Collider2D>(true)) c.enabled = false;
    }
    static Transform Group(Scene s, string name, string region, Transform parent)
    {
        var existing = All<Transform>(s).FirstOrDefault(t => t.name == name);
        if (existing != null) return existing;
        var go = new GameObject(name); SceneManager.MoveGameObjectToScene(go, s);
        if (parent != null) go.transform.SetParent(parent, false);
        go.AddComponent<KelompokNpcManual>().kawasan = region;
        return go.transform;
    }
    static Vector3 Along(JalurKampus route, float fraction)
    {
        var p = route.titik.Where(t => t != null).ToArray();
        if (p.Length < 2) throw new Exception("Jalur GA belum mempunyai titik.");
        var distances = new float[p.Length]; for (int j = 1; j < p.Length; j++) distances[j] = distances[j-1] + Vector3.Distance(p[j-1].position,p[j].position);
        float d = distances.Last() * fraction;
        for (int j = 1; j < p.Length; j++) if (d <= distances[j]) return Vector3.Lerp(p[j-1].position,p[j].position,Mathf.InverseLerp(distances[j-1],distances[j],d));
        return p.Last().position;
    }
    [MenuItem("Arthur/NPC Dosen/Pasang semua wajah dari PDF")]
    public static void Run()
    {
        Directory.CreateDirectory(Work + "cadangan");
        var original = SceneManager.GetActiveScene(); var log = new List<string>();
        for (int i = 0; i < Names.Length; i++)
        {
            AssetDatabase.ImportAsset(HeadPath(i), ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(HeadPath(i));
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = (importer.GetSourceTextureHeight() - 12) / .97f;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spritePivot = new Vector2(.5f,.5f); settings.spriteAlignment = (int)SpriteAlignment.Center; importer.SetTextureSettings(settings);
            importer.alphaIsTransparency = true; importer.mipmapEnabled = false; importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.Compressed; importer.SaveAndReimport();
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath(i)));
            if (File.Exists(PrefabPath(i))) continue;
            var go = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AlatNpcArthur.Prefab));
            try { Configure(go,i); PrefabUtility.SaveAsPrefabAsset(go,PrefabPath(i)); } finally { UnityEngine.Object.DestroyImmediate(go); }
        }
        foreach (var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Chapter1Preview/Chapter1_Kamar201.unity"})
        {
            var s = SceneManager.GetSceneByPath(path); bool opened = s.IsValid() && s.isLoaded;
            if (!opened) s = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var routes = All<JalurKampus>(s);
                var dosen = routes.FirstOrDefault(r => r.name == "GK1_RuanganDosen");
                var dean = routes.FirstOrDefault(r => r.name == "GK1_RuanganDekanFilkom");
                var ga = routes.FirstOrDefault(r => r.name == "GK1_Lantai4_Kanan");
                if (dosen == null || dean == null || ga == null) throw new Exception("Ruangan atau jalur GK1 lantai 4 kanan belum ada pada " + path);
                if (!File.Exists(Work+"cadangan/"+Path.GetFileName(path))) EditorSceneManager.SaveScene(s,Work+"cadangan/"+Path.GetFileName(path),true);
                var fk = Group(s,"NPC DOSEN - FILKOM (6 dosen)","GK1",dosen.transform);
                var dk = Group(s,"NPC DEKAN - FILKOM (Stenly)","GK1",dean.transform);
                var fe = Group(s,"NPC DOSEN - FEKON FEB (5 dosen)","GK1",null);
                int count = 0;
                for (int i = 0; i < Names.Length; i++)
                {
                    var parent = i < 5 ? fe : i == 5 ? dk : fk;
                    if (parent.Cast<Transform>().Any(t => t.name == Labels[i]+" - "+Faculty(i))) continue;
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath(i)),s);
                    go.transform.SetParent(parent,false);
                    var n = go.GetComponent<NpcYangBisaDiatur>();
                    if (i < 5) { n.jalurInteraksi = ga; var p = Along(ga,.15f+i*.16f); p.y -= ArthurJalurKampus.JarakKaki; var tangent = ga.titik.Last().position-ga.titik.First().position; tangent.y=0; var side=Vector3.Cross(Vector3.up,tangent.normalized)*.65f; go.transform.position = p + side; n.geserTitikInteraksi = -side; }
                    else { n.jalurInteraksi = i == 5 ? dean : dosen; go.transform.localPosition = i == 5 ? new Vector3(.95f,.025f,2.1f) : new Vector3(-2.55f,.025f,-3.6f+(i-6)*1.55f); n.geserTitikInteraksi = Vector3.right*.7f; }
                    if (go.transform is RectTransform rect) rect.anchoredPosition3D = rect.localPosition;
                    go.transform.rotation = Quaternion.Euler(0,i<5 ? -90 : 0,0);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform); PrefabUtility.RecordPrefabInstancePropertyModifications(n);
                    count++;
                }
                EditorSceneManager.MarkSceneDirty(s); EditorSceneManager.SaveScene(s);
                log.Add(path+": "+count+" NPC baru; FILKOM=7, FEKON/FEB=5. Semua nama, kepala, potret, idle, suara, dan jalur interaksi tersambung.");
            } finally { if (!opened) EditorSceneManager.CloseScene(s,true); }
        }
        if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
        AssetDatabase.SaveAssets(); SceneView.RepaintAll(); File.WriteAllLines(Work+"hasil.txt",log);
    }
    // TextureImporter.GetSourceTextureWidthAndHeight is available in this project's Unity version.
    static int GetSourceTextureHeight(this TextureImporter importer) { importer.GetSourceTextureWidthAndHeight(out var width,out var height); return height; }
}
