#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
[InitializeOnLoad]
public static class RuanganDekanPendingBuild
{
    const string Request="work/ruangan-dekan-20261109/build.request";
    static RuanganDekanPendingBuild(){EditorApplication.update+=Tick;}
    static void Tick()
    {
        if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
        File.Delete(Request);
        try{RuanganDekanImporter.BuildPreview();File.WriteAllText("work/ruangan-dekan-20261109/unity-result.txt","OK: prefab lengkap, prefab potongan dan scene pratinjau berhasil disimpan.");}
        catch(System.Exception ex){File.WriteAllText("work/ruangan-dekan-20261109/unity-result.txt",ex.ToString());Debug.LogException(ex);}
    }
}
#endif
