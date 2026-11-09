// PANDUAN PROYEK - Paket Chapel / ChapelPioneer
// Fungsi: Membangun prefab/material/mesh chapel dan membuka scene contoh melalui API Unity.
// Panduan: Assets/ChapelPioneer/BACA DULU.md

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class PioneerChapelImporter
{
    const string Root="Assets/ChapelPioneer";
    [Serializable]public class ColliderData{public string name;public float[] center,size;}
    [Serializable]public class Data{public int revision;public string[] materials,static_mesh_colliders;public ColliderData[] colliders;}
    static Vector3 V(float[] p)=>new Vector3(p[0],p[1],p[2]);

    [MenuItem("Tools/A Little Campus Story/Chapel/1 - Siapkan Prefab")]
    public static void BuildPrefab()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Hentikan Play Mode dulu.");
        Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");Directory.CreateDirectory(Root+"/Meshes");
        AssetDatabase.Refresh();
        var data=JsonUtility.FromJson<Data>(File.ReadAllText(Root+"/PioneerChapel_Data.json"));
        Shader shader=Shader.Find(GraphicsSettings.currentRenderPipeline!=null?"Universal Render Pipeline/Lit":"Standard");
        if(shader==null)throw new InvalidOperationException("Shader proyek belum ditemukan.");
        var palette=new Dictionary<string,Material>();
        foreach(string name in data.materials)
        {
            string path=Root+"/Materials/"+name+".mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(shader);AssetDatabase.CreateAsset(mat,path);}else mat.shader=shader;
            var texturePath=Root+"/Textures/"+name+".png";
            var ti=(TextureImporter)AssetImporter.GetAtPath(texturePath);
            if(ti!=null)
            {
                ti.textureType=TextureImporterType.Default;ti.wrapMode=TextureWrapMode.Repeat;
                ti.filterMode=FilterMode.Bilinear;ti.mipmapEnabled=true;ti.maxTextureSize=512;
                ti.isReadable=false;ti.textureCompression=TextureImporterCompression.Compressed;
                var mobile=ti.GetPlatformTextureSettings("Android");mobile.overridden=true;
                mobile.maxTextureSize=512;mobile.format=TextureImporterFormat.ASTC_6x6;ti.SetPlatformTextureSettings(mobile);ti.SaveAndReimport();
            }
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            bool glass=name=="Kaca"||name=="KacaTerang";
            Color tint=Color.white;if(glass)tint.a=name=="Kaca"?.28f:.17f;
            mat.name=name;mat.mainTexture=tex;mat.color=tint;mat.enableInstancing=true;
            if(mat.HasProperty("_BaseMap"))mat.SetTexture("_BaseMap",tex);
            if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",tint);
            if(mat.HasProperty("_Smoothness"))mat.SetFloat("_Smoothness",0);
            if(mat.HasProperty("_Glossiness"))mat.SetFloat("_Glossiness",0);
            if(mat.HasProperty("_Metallic"))mat.SetFloat("_Metallic",0);
            if(mat.HasProperty("_Cull"))mat.SetFloat("_Cull",0);
            if(mat.HasProperty("_SpecularHighlights"))mat.SetFloat("_SpecularHighlights",0);
            if(mat.HasProperty("_EnvironmentReflections"))mat.SetFloat("_EnvironmentReflections",0);
            if(mat.HasProperty("_Surface"))mat.SetFloat("_Surface",glass?1:0);
            if(mat.HasProperty("_Blend"))mat.SetFloat("_Blend",0);
            if(mat.HasProperty("_SrcBlend"))mat.SetFloat("_SrcBlend",(float)(glass?BlendMode.SrcAlpha:BlendMode.One));
            if(mat.HasProperty("_DstBlend"))mat.SetFloat("_DstBlend",(float)(glass?BlendMode.OneMinusSrcAlpha:BlendMode.Zero));
            if(mat.HasProperty("_SrcBlendAlpha"))mat.SetFloat("_SrcBlendAlpha",(float)BlendMode.One);
            if(mat.HasProperty("_DstBlendAlpha"))mat.SetFloat("_DstBlendAlpha",(float)(glass?BlendMode.OneMinusSrcAlpha:BlendMode.Zero));
            if(mat.HasProperty("_ZWrite"))mat.SetFloat("_ZWrite",glass?0:1);
            mat.DisableKeyword("_ALPHATEST_ON");mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            if(glass)mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");else mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetOverrideTag("RenderType",glass?"Transparent":"Opaque");mat.renderQueue=glass?(int)RenderQueue.Transparent:-1;
            mat.SetShaderPassEnabled("ShadowCaster",!glass);
            EditorUtility.SetDirty(mat);palette[name]=mat;
        }
        var temp=EditorSceneManager.NewPreviewScene();
        try
        {
            var instance=new GameObject("Pioneer Chapel - Revisi "+data.revision);SceneManager.MoveGameObjectToScene(instance,temp);
            using(var reader=new BinaryReader(File.OpenRead(Root+"/PioneerChapel_Mesh.bytes")))
            {
                if(System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4))!="PCP1")throw new InvalidDataException("Sumber mesh chapel tidak sesuai.");
                int count=reader.ReadInt32();
                for(int chunk=0;chunk<count;chunk++)
                {
                    string name=System.Text.Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                    int vc=reader.ReadInt32(),fc=reader.ReadInt32();var vertices=new Vector3[vc];var uvs=new Vector2[vc];
                    for(int i=0;i<vc;i++)vertices[i]=new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
                    for(int i=0;i<vc;i++)uvs[i]=new Vector2(reader.ReadSingle(),reader.ReadSingle());
                    var indices=new int[fc*3];for(int i=0;i<indices.Length;i++)indices[i]=reader.ReadInt32();
                    var grouped=new SortedDictionary<int,List<int>>();
                    for(int i=0;i<fc;i++){int mat=reader.ReadInt32();if(!grouped.ContainsKey(mat))grouped[mat]=new List<int>();grouped[mat].Add(indices[i*3]);grouped[mat].Add(indices[i*3+1]);grouped[mat].Add(indices[i*3+2]);}
                    string meshPath=Root+"/Meshes/"+name+".asset";
                    var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);bool fresh=mesh==null;if(fresh)mesh=new Mesh();else mesh.Clear();
                    mesh.name=name;mesh.indexFormat=vc>65535?IndexFormat.UInt32:IndexFormat.UInt16;mesh.vertices=vertices;mesh.uv=uvs;mesh.subMeshCount=grouped.Count;
                    int sub=0;foreach(var list in grouped.Values)mesh.SetTriangles(list,sub++);
                    mesh.RecalculateNormals();mesh.RecalculateBounds();
                    if(fresh)AssetDatabase.CreateAsset(mesh,meshPath);else EditorUtility.SetDirty(mesh);
                    var piece=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));piece.transform.SetParent(instance.transform,false);
                    piece.GetComponent<MeshFilter>().sharedMesh=mesh;
                    var r=piece.GetComponent<MeshRenderer>();r.sharedMaterials=grouped.Keys.Select(i=>palette[data.materials[i]]).ToArray();
                    r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;piece.isStatic=true;
                    if(data.static_mesh_colliders!=null && data.static_mesh_colliders.Any(prefix=>name.StartsWith(prefix)))
                    {
                        var collision=piece.AddComponent<MeshCollider>();collision.sharedMesh=mesh;collision.convex=false;
                    }
                }
            }
            var colliders=new GameObject("Colliders_Sederhana");colliders.transform.SetParent(instance.transform,false);
            foreach(var item in data.colliders)
            {
                var go=new GameObject(item.name);go.transform.SetParent(colliders.transform,false);
                var box=go.AddComponent<BoxCollider>();box.center=V(item.center);box.size=V(item.size);
            }
            var view=instance.AddComponent<PioneerChapelView>();
            Func<string,GameObject> find=prefix=>instance.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name.StartsWith(prefix))?.gameObject;
            view.atap=find("05_");view.atapSayap=find("17_");view.dindingBelakang=find("08_");view.balkon=find("14_");view.alatMusik=find("12_");
            string prefabPath=Root+"/Prefabs/PioneerChapel_Interior.prefab";
            PrefabUtility.SaveAsPrefabAsset(instance,prefabPath);
            view.bukaAtap=true;view.bukaBelakang=true;view.sembunyikanBalkon=true;view.Terapkan();
            PrefabUtility.SaveAsPrefabAsset(instance,Root+"/Prefabs/PioneerChapel_Cutaway.prefab");
            AssetDatabase.SaveAssets();
            Selection.activeObject=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            EditorGUIUtility.PingObject(Selection.activeObject);
            Debug.Log("Prefab chapel siap. Model lengkap dan Cutaway ada di Assets/ChapelPioneer/Prefabs.");
        }
        finally{EditorSceneManager.ClosePreviewScene(temp);}
    }

    // Used by the project opener to build the latest model and show it immediately.
    public static void BuildAndOpenPreview()
    {
        BuildPrefab();
        OpenPreview();
    }

    [MenuItem("Tools/A Little Campus Story/Chapel/3 - Lihat Belakang")]
    public static void ShowRear() => ShowAngle(new Vector3(0,3,-8),new Vector3(0,9,-35.8f));
    [MenuItem("Tools/A Little Campus Story/Chapel/4 - Lihat Mimbar")]
    public static void ShowFront() => ShowAngle(new Vector3(0,3,-5),new Vector3(0,7.5f,35));
    [MenuItem("Tools/A Little Campus Story/Chapel/5 - Lihat Foyer Belakang")]
    public static void ShowFoyer() => ShowAngle(new Vector3(-12,2.1f,-33.4f),new Vector3(10,2.3f,-31.9f));
    [MenuItem("Tools/A Little Campus Story/Chapel/6 - Lihat Cabang Keluar")]
    public static void ShowExitBranch() => ShowAngle(new Vector3(14,1.8f,-25.7f),new Vector3(20.6f,1.8f,-26.4f));
    [MenuItem("Tools/A Little Campus Story/Chapel/7 - Lihat Kaca Hitam")]
    public static void ShowDarkRoom() => ShowAngle(new Vector3(7,2,-19),new Vector3(5.35f,2.1f,-25.3f));

    static void ShowAngle(Vector3 eye,Vector3 target)
    {
        ChapelCursorRecovery.Recover();
        var view=SceneView.lastActiveSceneView;
        if(view==null)view=EditorWindow.GetWindow<SceneView>();
        view.Show();view.Focus();
        view.maximized=false;
        var marker=new GameObject("Temporary chapel view"){hideFlags=HideFlags.HideAndDontSave};
        marker.transform.position=eye;marker.transform.LookAt(target);
        view.AlignViewToObject(marker.transform);
        UnityEngine.Object.DestroyImmediate(marker);view.Repaint();
    }

    [MenuItem("Tools/A Little Campus Story/Chapel/2 - Buka Scene Contoh")]
    public static void OpenPreview()
    {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/PioneerChapel_Interior.prefab");
        if(prefab==null){BuildPrefab();prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/PioneerChapel_Interior.prefab");}
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SceneManager.SetActiveScene(scene);
        var chapel=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.72f,.68f,.57f);
        RenderSettings.skybox=null;RenderSettings.fog=false;
        var sun=new GameObject("Cahaya lembut chapel").AddComponent<Light>();sun.type=LightType.Directional;
        sun.intensity=.85f;sun.color=new Color(1,.90f,.74f);sun.transform.rotation=Quaternion.Euler(48,-28,0);sun.shadows=LightShadows.Soft;
        var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.tag="MainCamera";
        // Open from the nave facing the pulpit so the main entry view shows the finished stage.
        camera.transform.position=new Vector3(0,3.0f,-5.0f);camera.transform.LookAt(new Vector3(0,7.5f,35.0f));
        camera.fieldOfView=76;camera.nearClipPlane=.08f;camera.farClipPlane=150;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.72f,.81f,.84f);
        Directory.CreateDirectory(Root+"/Scenes");EditorSceneManager.SaveScene(scene,Root+"/Scenes/PioneerChapel_Preview.unity");
        Selection.activeGameObject=null;if(SceneView.lastActiveSceneView!=null)SceneView.lastActiveSceneView.AlignViewToObject(camera.transform);
    }
}
