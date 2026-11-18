using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class RuanganDekanImporter
{
    const string Root = "Assets/Ruangan Dekan";
    [Serializable] sealed class Info { public string[] materials; public int triangles, station_count; }
    static string ReadName(BinaryReader r) => System.Text.Encoding.UTF8.GetString(r.ReadBytes(r.ReadInt32()));
    [MenuItem("Arthur/Ruangan Dekan/1 - Siapkan Model dan Prefab")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Hentikan Play sebelum menyiapkan aset.");
        Directory.CreateDirectory(Root + "/Materials"); Directory.CreateDirectory(Root + "/Meshes"); Directory.CreateDirectory(Root + "/Prefabs");
        AssetDatabase.Refresh();
        var data = JsonUtility.FromJson<Info>(File.ReadAllText(Root + "/RuanganDekan_Data.json"));
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null) throw new InvalidOperationException("Shader proyek tidak ditemukan.");
        var materials = new Material[data.materials.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            string name = data.materials[i], texturePath = Root + "/Textures/" + name + ".png";
            var ti = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (ti != null)
            {
                ti.maxTextureSize = 512; ti.mipmapEnabled = true; ti.wrapMode = TextureWrapMode.Repeat;
                ti.isReadable = false; ti.textureCompression = TextureImporterCompression.Compressed;
                ti.SaveAndReimport();
            }
            string path = Root + "/Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, path); }
            mat.shader = shader; mat.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            Color tint = Color.white;
            if (name == "DindingPutih") tint = new Color(0.96f,0.99f,1.0f);
            if (name == "LantaiAbu") tint = new Color(0.58f,0.66f,0.7f);
            if (name == "PanelPutih") tint = new Color(0.95f,0.98f,1.0f);
            if (name == "RangkaHitam") tint = new Color(0.52f,0.52f,0.52f);
            if (name == "KacaPartisi") tint = new Color(0.65f,0.82f,0.77f);
            if (name == "KacaMeja") tint = new Color(0.75f,0.85f,0.83f);
            if (name == "KayuMeja") tint = new Color(0.8f,0.76f,0.63f);
            if (name == "KayuPintu") tint = new Color(0.9f,0.73f,0.48f);
            if (name == "SofaCokelat") tint = new Color(0.64f,0.52f,0.36f);
            if (name == "KursiBiru") tint = new Color(0.37f,0.66f,1.0f);
            if (name == "Logam") tint = new Color(0.95f,0.98f,1.0f);
            if (name == "RakKrem") tint = new Color(1.0f,1.0f,0.93f);
            if (name == "KainTirai") tint = new Color(1.0f,0.98f,0.84f);
            if (name == "LayarMonitor") tint = new Color(1.0f,1.0f,1.0f);
            if (name == "BerkasHijau") tint = new Color(0.86f,1.0f,0.88f);
            if (name == "Kertas") tint = new Color(1.0f,1.0f,1.0f);
            if (name == "BungaKuning") tint = new Color(1.0f,1.0f,0.86f);
            if (name == "BungaMerah") tint = new Color(1.0f,1.0f,1.0f);
            if (name == "Daun") tint = new Color(1.0f,1.0f,1.0f);
            if (name == "Lampu") tint = new Color(1.0f,1.0f,0.82f);
            if (name == "KacaPartisi" || name == "KacaMeja")
            {
                tint.a = name == "KacaPartisi" ? .34f : .30f;
                mat.SetFloat("_Surface",1); mat.SetFloat("_Blend",0);
                mat.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha); mat.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite",0); mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.SetOverrideTag("RenderType","Transparent"); mat.renderQueue=3000;
                mat.SetShaderPassEnabled("ShadowCaster",false);
            }
            mat.color = tint;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mat.mainTexture);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0);
            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0);
            mat.enableInstancing = true; EditorUtility.SetDirty(mat); materials[i] = mat;
        }
        var preview = EditorSceneManager.NewPreviewScene();
        try
        {
            var root = new GameObject("Ruangan Dekan");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, preview);
            int triangles = 0;
            using (var reader = new BinaryReader(File.OpenRead(Root + "/RuanganDekan_Mesh.bytes")))
            {
                int count = reader.ReadInt32();
                for (int k = 0; k < count; k++)
                {
                    string name = ReadName(reader); int vc = reader.ReadInt32(), fc = reader.ReadInt32();
                    var vertices = new Vector3[vc]; var uv = new Vector2[vc]; var faces = new int[fc * 3]; var faceMaterials = new int[fc];
                    for (int v = 0; v < vc; v++) vertices[v] = new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
                    for (int v = 0; v < vc; v++) uv[v] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    for (int f = 0; f < faces.Length; f++) faces[f] = reader.ReadInt32();
                    for (int f = 0; f < fc; f++) faceMaterials[f] = reader.ReadInt32();
                    int[] used = faceMaterials.Distinct().ToArray();
                    string meshPath = Root + "/Meshes/" + name + ".asset";
                    var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                    if (mesh == null) { mesh = new Mesh(); AssetDatabase.CreateAsset(mesh, meshPath); }
                    mesh.Clear(); mesh.name = name; mesh.vertices = vertices; mesh.uv = uv; mesh.subMeshCount = used.Length;
                    for (int j = 0; j < used.Length; j++)
                    {
                        var indices = new List<int>();
                        for (int f = 0; f < fc; f++) if (faceMaterials[f] == used[j]) { indices.Add(faces[f*3]);indices.Add(faces[f*3+1]);indices.Add(faces[f*3+2]); }
                        mesh.SetTriangles(indices,j);
                    }
                    mesh.RecalculateNormals(); mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
                    var piece = new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer)); piece.transform.SetParent(root.transform,false);
                    piece.GetComponent<MeshFilter>().sharedMesh = mesh; piece.GetComponent<MeshRenderer>().sharedMaterials = used.Select(i=>materials[i]).ToArray();
                    GameObjectUtility.SetStaticEditorFlags(piece,StaticEditorFlags.BatchingStatic);
                    // Keep the door and window openings physically open. Mesh colliders follow actual geometry.
                    if (name.StartsWith("01_") || name.StartsWith("02_") || name.StartsWith("03_") || name.StartsWith("04_") || name.StartsWith("05_") || name.StartsWith("Partisi_") || name.StartsWith("Meja_") || name.StartsWith("Sofa_") || name.StartsWith("Lemari_"))
                        piece.AddComponent<MeshCollider>().sharedMesh = mesh;
                    triangles += fc;
                }
            }
            if (triangles != data.triangles) throw new InvalidOperationException("Jumlah geometri tidak cocok.");
            PrefabUtility.SaveAsPrefabAsset(root,Root+"/Prefabs/Ruangan Dekan.prefab");
            foreach(var t in root.GetComponentsInChildren<Transform>(true))
                if(t.name.StartsWith("02_")||t.name.StartsWith("04_")||t.name.StartsWith("08_")||t.name.StartsWith("10_")||t.name.StartsWith("AC_"))t.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root,Root+"/Prefabs/Ruangan Dekan - Potongan.prefab");
            AssetDatabase.SaveAssets();
            Debug.Log("RUANGAN_DEKAN_OK: "+triangles+" triangles; "+data.station_count+" bilik perkiraan; prefab lengkap dan potongan tersimpan.");
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }
    }
    public static void BuildPreview()
    {
        Build();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/Ruangan Dekan.prefab");
        PrefabUtility.InstantiatePrefab(prefab,scene);
        RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.82f,.84f,.84f); RenderSettings.fog = false;
        var light = new GameObject("Cahaya Pratinjau").AddComponent<Light>(); light.type=LightType.Directional;light.intensity=.65f;light.transform.rotation=Quaternion.Euler(55,-30,0);light.shadows=LightShadows.None;
        var camera = new GameObject("Kamera Ruangan Dekan").AddComponent<Camera>();camera.transform.position=new Vector3(.8f,2.1f,-5.15f);camera.transform.LookAt(new Vector3(-.1f,1.1f,1f));camera.fieldOfView=76;camera.nearClipPlane=.05f;camera.farClipPlane=60;
        Directory.CreateDirectory(Root+"/Scenes"); EditorSceneManager.SaveScene(scene,Root+"/Scenes/Ruangan Dekan.unity");
        EditorSceneManager.CloseScene(scene,true);
    }
    [MenuItem("Arthur/Ruangan Dekan/2 - Buka Pratinjau")]
    public static void Open()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/Ruangan Dekan.prefab")==null)BuildPreview();
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        EditorSceneManager.OpenScene(Root+"/Scenes/Ruangan Dekan.unity");
    }
}
