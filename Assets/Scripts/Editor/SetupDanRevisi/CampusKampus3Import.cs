using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
// Mengimpor satu world yang LevelName-nya "Jasmine,Kantin,GK2".
// Pemasangan world terbaru pada CharacterLab dan salinannya, Explorer.
public sealed class CampusKampus3AssetImport : AssetPostprocessor
{
    internal const string Folder = "Assets/WorldExplorer/JasmineKantinGK2/";
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Folder, StringComparison.Ordinal)) return;
        var importer = (TextureImporter)assetImporter;
        importer.maxTextureSize = 4096;
        importer.mipmapEnabled = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.filterMode = FilterMode.Bilinear;
        importer.anisoLevel = 2;
        importer.isReadable = false;
    }
    void OnPreprocessModel()
    {
        if (!assetPath.StartsWith(Folder, StringComparison.Ordinal)) return;
        var importer = (ModelImporter)assetImporter;
        importer.globalScale = 1f;
        importer.importAnimation = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.isReadable = false;
    }
}
[InitializeOnLoad]
public static class CampusKampus3Import
{
    const string Work = "work/kampus3/";
    const string Folder = CampusKampus3AssetImport.Folder;
    static readonly string[] ScenePaths = { "Assets/Scenes/CharacterLab.unity", "Assets/Scenes/Explorer.unity" };
    static double idleSejak;
    static CampusKampus3Import() { EditorApplication.update += Tick; }
    static void Tick()
    {
        if (AssetDatabase.IsAssetImportWorkerProcess()) return;
        if (!File.Exists(Work + "revisi-siap.txt")) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            idleSejak = 0;
            return;
        }
        if (idleSejak == 0) { idleSejak = EditorApplication.timeSinceStartup; return; }
        if (EditorApplication.timeSinceStartup - idleSejak < 2) return;
        File.Delete(Work + "revisi-siap.txt");
        try { Impor(); }
        catch (Exception e) { File.WriteAllText(Work + "status-impor.txt", "BELUM SELESAI\n" + e); }
    }
    [MenuItem("Tools/A Little Campus Story/Explorer/Impor Jasmine Kantin GK2 ke CharacterLab")]
    public static void Impor()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Matikan Play dahulu.");
        var before = EditorSceneManager.GetSceneManagerSetup();
        foreach (var item in before)
            if (item.isLoaded && SceneManager.GetSceneByPath(item.path).isDirty)
                throw new Exception("Simpan perubahan scene pengguna dahulu: " + item.path);
        string modelPath = Folder + "Jasmine_Kantin_GK2.obj";
        string texturePath = Folder + "Jasmine_Kantin_GK2-RGBA.png";
        AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceSynchronousImport);
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (model == null || texture == null) throw new Exception("Model atau tekstur belum berhasil diimpor.");
        if (texture.width > 4096 || texture.height > 4096) throw new Exception("Tekstur Unity belum dibatasi ke 4096.");
        var reference = AssetDatabase.LoadAssetAtPath<Material>("Assets/All Asset/AllMap/AsramaJasmineDanSekitarnyaAs/White_ConcreteMat.mat");
        if (reference == null) throw new Exception("Material acuan model lama tidak ditemukan.");
        string materialPath = Folder + "KertasKampus.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(reference) { name = "KertasKampus" };
            AssetDatabase.CreateAsset(material, materialPath);
        }
        material.SetTexture("_BaseMap", texture);
        material.SetTexture("_MainTex", texture);
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        var laporan = new System.Text.StringBuilder("TERPASANG\nWorld: Jasmine,Kantin,GK2\n");
        try
        {
            foreach (string scenePath in ScenePaths)
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var roots = scene.GetRootGameObjects();
                var original = roots.SingleOrDefault(x => x.name == "AsramaJasmineDanSekitarnya");
                if (original == null) throw new Exception("World acuan lama tidak ditemukan pada " + scenePath);
                var instance = roots.FirstOrDefault(x => x.name == "World_JasmineKantinGK2");
                if (instance == null) instance = (GameObject)PrefabUtility.InstantiatePrefab(model, scene);
                instance.name = "World_JasmineKantinGK2";
                // Offset model lama=(-51,62,39); importer OBJ Unity membalik X.
                instance.transform.position = original.transform.TransformPoint(new Vector3(51f, 62f, 39f));
                instance.transform.rotation = original.transform.rotation;
                instance.transform.localScale = original.transform.localScale;
                var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
                if (renderers.Length == 0) throw new Exception("Mesh ekspor kosong.");
                foreach (var renderer in renderers)
                {
                    var filter = renderer.GetComponent<MeshFilter>();
                    int slots = filter != null && filter.sharedMesh != null ? filter.sharedMesh.subMeshCount : renderer.sharedMaterials.Length;
                    renderer.sharedMaterials = Enumerable.Repeat(material, Math.Max(1, slots)).ToArray();
                    renderer.receiveShadows = true;
                }
                if (scenePath.EndsWith("CharacterLab.unity", StringComparison.Ordinal))
                    PrefabUtility.SaveAsPrefabAssetAndConnect(instance, Folder + "JasmineKantinGK2.prefab", InteractionMode.AutomatedAction);
                original.SetActive(false);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Scene gagal disimpan: " + scenePath);
                laporan.AppendLine(scenePath + " — root " + instance.transform.position.ToString("F5") + ", renderer " + renderers.Length);
                var camera = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Camera>(true)).FirstOrDefault(x => x.CompareTag("MainCamera"));
                if (camera != null) Foto(camera, scene.name);
            }
            AssetDatabase.SaveAssets();
            laporan.AppendLine("Tekstur Unity: " + texture.width + "x" + texture.height);
            laporan.AppendLine("World lama dinonaktifkan dan tetap tersedia. Cadangan scene disimpan sebelum perubahan.");
            File.WriteAllText(Work + "status-impor.txt", laporan.ToString());
            EditorSceneManager.OpenScene(ScenePaths[0], OpenSceneMode.Single);
        }
        catch
        {
            EditorSceneManager.RestoreSceneManagerSetup(before);
            throw;
        }
    }
    static void Foto(Camera camera, string namaScene)
    {
        var oldTarget = camera.targetTexture;
        var oldActive = RenderTexture.active;
        float oldAspect = camera.aspect;
        var target = RenderTexture.GetTemporary(1280, 720, 24);
        var pixels = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.aspect = 16f / 9f;
            camera.Render();
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            pixels.Apply();
            File.WriteAllBytes(Work + "preview-" + namaScene + ".png", pixels.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = oldTarget; camera.aspect = oldAspect;
            RenderTexture.active = oldActive;
            UnityEngine.Object.DestroyImmediate(pixels);
            RenderTexture.ReleaseTemporary(target);
        }
    }
}
