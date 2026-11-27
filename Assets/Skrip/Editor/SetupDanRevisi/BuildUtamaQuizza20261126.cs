using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

// Entry point batch: tidak menampilkan dialog atau mengganti identitas aplikasi utama.
public static class BuildUtamaQuizza20261126
{
    public static void Run()
    {
        string status=Path.GetFullPath("../work/quizo-quizza-20261126/build-status.txt");
        int exit=1;
        try {
            if(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android)!="com.UnityTechnologies.com.unity.template.urpblank" ||
               PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android).Contains("ALCS_MOD"))
                throw new Exception("Build ini harus memakai identitas aplikasi utama tanpa MOD.");
            string nama=Environment.GetEnvironmentVariable("ALCS_BUILD_OUTPUT");
            string output=Path.GetFullPath("Builds/Android/"+(string.IsNullOrWhiteSpace(nama)?"A Little Campus Story - Utama Quizza 20261126.apk":Path.GetFileName(nama)));
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(status,"BUILDING (batch tanpa jendela)\n"+output);
            Environment.SetEnvironmentVariable("PATH",@"C:\Windows\System32;C:\Windows;"+Environment.GetEnvironmentVariable("PATH"));
            Environment.SetEnvironmentVariable("JAVA_HOME",UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath);
            EditorUserBuildSettings.buildAppBundle=false;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName=output,target=BuildTarget.Android,options=BuildOptions.None,
                extraScriptingDefines=Array.Empty<string>()
            });
            File.WriteAllText(status,report.summary.result+"\nBytes="+report.summary.totalSize+"\nErrors="+report.summary.totalErrors+
                "\nWarnings="+report.summary.totalWarnings+"\n"+output+"\n"+
                string.Join("\n",report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content)));
            if(report.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded)exit=0;
        } catch(Exception e){File.WriteAllText(status,"FAILED\n"+e);Debug.LogException(e);}
        finally {AssetDatabase.SaveAssets();EditorApplication.Exit(exit);}
    }
}
