using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Konfigurasi khusus Android; tidak menyunting scene, jalur, atau profil PC.
public static class AndroidSmoothRevision
{
    const string Folder = "work/android-smooth-20261110";
    [MenuItem("Arthur/Performa/Terapkan Android ringan")]
    public static void Terapkan()
    {
        if (EditorApplication.isPlaying) throw new Exception("Hentikan Play terlebih dahulu.");
        Directory.CreateDirectory(Folder + "/sebelum");
        var rp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
        var so = new SerializedObject(rp);
        Set(so,"m_MainLightShadowsSupported",false);
        Set(so,"m_AdditionalLightShadowsSupported",false);
        Set(so,"m_SoftShadowsSupported",false);
        Set(so,"m_ReflectionProbeBlending",false);
        Set(so,"m_ReflectionProbeBoxProjection",false);
        so.FindProperty("m_AdditionalLightsRenderingMode").intValue=(int)LightRenderingMode.PerVertex;
        so.ApplyModifiedPropertiesWithoutUndo();
        rp.renderScale = 1f;
        rp.msaaSampleCount = 1;
        rp.supportsHDR = false;
        EditorUtility.SetDirty(rp);
        PlayerSettings.Android.optimizedFramePacing = true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[]{GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3});
        var log = new System.Collections.Generic.List<string>();
        // Hanya tekstur 3D lingkungan. Sprite karakter, UI, font dan peta tidak disentuh.
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/WorldExplorer","Assets/ChapelPioneer","Assets/All Asset/AllMap"}))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.textureType != TextureImporterType.Default) continue;
            string backup = Folder + "/sebelum/" + path + ".meta";
            Directory.CreateDirectory(Path.GetDirectoryName(backup));
            if (!File.Exists(backup)) File.Copy(path + ".meta",backup);
            var settings = importer.GetPlatformTextureSettings("Android");
            settings.name = "Android";
            settings.overridden = true;
            settings.maxTextureSize = 4096;
            // ETC2 bekerja pada seluruh perangkat GLES3, tanpa fallback ASTC ke RGBA.
            settings.format = TextureImporterFormat.ETC2_RGBA8;
            settings.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(settings);
            importer.SaveAndReimport();
            log.Add(path);
        }
        AssetDatabase.SaveAssets();
        File.WriteAllLines(Folder + "/tekstur-android.txt",log);
        File.WriteAllText(Folder + "/setup.txt","OK: shadows off, HDR off, MSAA off, Vulkan first, vertex lights, Android world ETC2 max4096. Textures=" + log.Count);
    }
    static void Set(SerializedObject so,string name,bool value)
    {
        var p=so.FindProperty(name);if(p==null)throw new Exception("Property hilang: "+name);p.boolValue=value;
    }
    public static void Periksa()
    {
        var hasil = new System.Collections.Generic.List<string>();
        Action<bool,string> cek=(ok,nama)=>{hasil.Add((ok?"PASS ":"FAIL ")+nama);if(!ok)throw new Exception(nama);};
        float min=PerformaAndroidArthur.SkalaUntuk(2400,1080,PerformaAndroidArthur.SisiPendekMinimum);
        float max=PerformaAndroidArthur.HitungSkala(2400,1080);
        cek(Mathf.Approximately(min,720f/1080)&&Mathf.Approximately(max,1f),"Rentang dunia 720-1080p pada layar 1080p");
        cek(Mathf.Approximately(max,PerformaAndroidArthur.HitungSkala(1080,2400)),"Rotasi layar");
        int stabil=0;float skala=max;
        for(int i=0;i<20;i++)skala=PerformaAndroidArthur.SesuaikanSkala(skala,min,max,125,ref stabil);
        cek(Mathf.Approximately(skala,min),"Frame sangat lambat menurunkan kualitas sampai minimum");
        for(int i=0;i<5;i++)skala=PerformaAndroidArthur.SesuaikanSkala(skala,min,max,16.67f,ref stabil);
        cek(Mathf.Approximately(skala,min),"Kualitas tidak meloncat setelah beberapa frame stabil");
        for(int i=0;i<120;i++)skala=PerformaAndroidArthur.SesuaikanSkala(skala,min,max,16.67f,ref stabil);
        cek(Mathf.Approximately(skala,max),"Kualitas dapat pulih tanpa melewati batas");
        cek(PerformaAndroidArthur.HitungSkala(0,0)>0,"Dimensi layar belum siap tidak membagi nol");
        var rp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
        cek(!rp.supportsMainLightShadows&&!rp.supportsAdditionalLightShadows&&!rp.supportsHDR&&rp.msaaSampleCount==1,"Profil Android ringan tersimpan tanpa MSAA");
        cek(!PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android)&&PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).First()==GraphicsDeviceType.Vulkan,"Vulkan menjadi pilihan pertama Android");
        cek(rp.additionalLightsRenderingMode==LightRenderingMode.PerVertex,"Lampu tambahan per vertex");
        cek(PlayerSettings.Android.optimizedFramePacing,"Android frame pacing aktif");
        File.WriteAllLines(Folder+"/checks.txt",hasil);
    }
    public static void Build()
    {
        try
        {
            Periksa();
            Environment.SetEnvironmentVariable("PATH",@"C:\Windows\System32;C:\Windows;"+Environment.GetEnvironmentVariable("PATH"));
            Environment.SetEnvironmentVariable("JAVA_HOME",UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath);
            Directory.CreateDirectory("Builds/Android");
            string namaApk = Environment.GetEnvironmentVariable("ARTHUR_ANDROID_BUILD_NAME");
            string output=Path.GetFullPath(Path.Combine("Builds/Android",
                string.IsNullOrWhiteSpace(namaApk) ? "A Little Campus Story - Android Smooth.apk" : Path.GetFileName(namaApk)));
            EditorUserBuildSettings.buildAppBundle=false;
            File.WriteAllText(Folder+"/build.txt","BUILDING");
            var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=output,target=BuildTarget.Android,options=BuildOptions.None });
            File.WriteAllText(Folder+"/build.txt",r.summary.result+"\nBytes="+r.summary.totalSize+"\nErrors="+r.summary.totalErrors+"\n"+output+"\n"+string.Join("\n",r.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content)));
        }
        catch(Exception e){File.WriteAllText(Folder+"/build.txt",e.ToString());throw;}
    }
}
