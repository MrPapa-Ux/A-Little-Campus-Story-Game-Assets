using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
public static class BuildMod20261127 {
 public static void Run(){var target=NamedBuildTarget.Android;string id=PlayerSettings.GetApplicationIdentifier(target),nama=PlayerSettings.productName;bool bundle=EditorUserBuildSettings.buildAppBundle;int code=1;
  string status=Path.GetFullPath("../work/mod-release-20261127/build-status.txt");Directory.CreateDirectory(Path.GetDirectoryName(status));
  try{
   if(id!="com.UnityTechnologies.com.unity.template.urpblank"||PlayerSettings.GetScriptingDefineSymbols(target).Contains("ALCS_MOD"))throw new Exception("Identitas dasar bukan versi utama.");
   PlayerSettings.SetApplicationIdentifier(target,"com.chipzz.alittlecampusstory.moduji");PlayerSettings.productName="ALCS MOD Uji";EditorUserBuildSettings.buildAppBundle=false;
   Environment.SetEnvironmentVariable("PATH",@"C:\Windows\System32;C:\Windows;"+Environment.GetEnvironmentVariable("PATH"));Environment.SetEnvironmentVariable("JAVA_HOME",UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath);
   var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToList();
   foreach(var s in new[]{"Assets/Chapter1Preview/MinigameRahasia/Pratinjau Pedang Harapan.unity","Assets/Chapter1Preview/MinigameRahasia/Pratinjau Langit Edel.unity"})if(!scenes.Contains(s))scenes.Add(s);
   string output=Path.GetFullPath("Builds/Android/A Little Campus Story - MOD 20261127.apk");File.WriteAllText(status,"BUILDING\n"+output);
   var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes.ToArray(),locationPathName=output,target=BuildTarget.Android,options=BuildOptions.None,extraScriptingDefines=new[]{"ALCS_MOD"}});
   File.WriteAllText(status,r.summary.result+"\nErrors="+r.summary.totalErrors+"\nWarnings="+r.summary.totalWarnings+"\nPackage=com.chipzz.alittlecampusstory.moduji\n"+output+"\n"+string.Join("\n",r.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content)));
   code=r.summary.result==UnityEditor.Build.Reporting.BuildResult.Succeeded?0:1;
  }catch(Exception e){File.WriteAllText(status,"FAILED\n"+e);Debug.LogException(e);}
  finally{PlayerSettings.SetApplicationIdentifier(target,id);PlayerSettings.productName=nama;EditorUserBuildSettings.buildAppBundle=bundle;AssetDatabase.SaveAssets();File.WriteAllText(Path.Combine(Path.GetDirectoryName(status),"identitas-dipulihkan.txt"),PlayerSettings.GetApplicationIdentifier(target)+"\n"+PlayerSettings.productName+"\nDefines="+PlayerSettings.GetScriptingDefineSymbols(target));EditorApplication.Exit(code);}
 }
}
