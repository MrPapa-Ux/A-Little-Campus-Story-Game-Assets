using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Batch entry: no dialogs, no graphics window, and no change to the normal app identity.
public static class BuildApkTerpisah20261121
{
    const string Work="work/mod-jasmine-20261120/";
    [Serializable] sealed class Identitas { public string id,name;public bool bundle; }
    public static void Run(){
        var original=JsonUtility.FromJson<Identitas>(File.ReadAllText(Work+"identitas-asli.json"));
        int code=1;
        try{
            Pulihkan(original);
            Environment.SetEnvironmentVariable("PATH",@"C:\Windows\System32;C:\Windows;"+Environment.GetEnvironmentVariable("PATH"));
            Environment.SetEnvironmentVariable("JAVA_HOME",UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath);
            Directory.CreateDirectory("Builds/Android");
            bool mod=Buat(true,original);Pulihkan(original);
            bool main=Buat(false,original);code=mod&&main?0:1;
        }catch(Exception e){File.WriteAllText(Work+"batch-error.txt",e.ToString());Debug.LogException(e);}
        finally{Pulihkan(original);AssetDatabase.SaveAssets();File.WriteAllText(Work+"batch-exit.txt",code.ToString());EditorApplication.Exit(code);}
    }
    public static void ModOnly(){
        var original=JsonUtility.FromJson<Identitas>(File.ReadAllText(Work+"identitas-asli.json"));int code=1;
        try{Pulihkan(original);Environment.SetEnvironmentVariable("PATH",@"C:\Windows\System32;C:\Windows;"+Environment.GetEnvironmentVariable("PATH"));Environment.SetEnvironmentVariable("JAVA_HOME",UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath);code=Buat(true,original)?0:1;}
        catch(Exception e){File.WriteAllText(Work+"batch-mod-error.txt",e.ToString());Debug.LogException(e);}
        finally{Pulihkan(original);AssetDatabase.SaveAssets();File.WriteAllText(Work+"batch-mod-exit.txt",code.ToString());EditorApplication.Exit(code);}
    }
    static void Pulihkan(Identitas original){PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,original.id);PlayerSettings.productName=original.name;EditorUserBuildSettings.buildAppBundle=original.bundle;}
    static bool Buat(bool mod,Identitas original){
        var key=mod?"mod":"utama";var output=Path.GetFullPath("Builds/Android/A Little Campus Story - "+(mod?"MOD Uji":"Utama")+" 20261121.apk");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,mod?"com.chipzz.alittlecampusstory.moduji":original.id);PlayerSettings.productName=mod?"ALCS MOD Uji":original.name;EditorUserBuildSettings.buildAppBundle=false;
        var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToList();
        if(mod)foreach(var path in new[]{"Assets/Chapter1Preview/MinigameRahasia/Pratinjau Pedang Harapan.unity","Assets/Chapter1Preview/MinigameRahasia/Pratinjau Langit Edel.unity"})if(!scenes.Contains(path))scenes.Add(path);
        File.WriteAllText(Work+"build-"+key+"-status.txt","BUILDING (batch)");
        try{
            var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes.ToArray(),locationPathName=output,target=BuildTarget.Android,options=BuildOptions.None,extraScriptingDefines=mod?new[]{"ALCS_MOD"}:Array.Empty<string>()});
            File.WriteAllText(Work+"build-"+key+"-status.txt",r.summary.result+"\nBytes="+r.summary.totalSize+"\nErrors="+r.summary.totalErrors+"\nPackage="+PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)+"\n"+output+"\n"+string.Join("\n",r.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content)));
            return r.summary.result==BuildResult.Succeeded;
        }catch(Exception e){File.WriteAllText(Work+"build-"+key+"-status.txt","FAIL "+e);Debug.LogException(e);return false;}
    }
}
