#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
[InitializeOnLoad]
public static class RuangBilikPendingBuild
{
    const string Request="work/ruang-bilik-20261109/build.request";
    static RuangBilikPendingBuild(){EditorApplication.update+=Tick;}
    static void Tick()
    {
        if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
        File.Delete(Request);
        try{RuangBilikReferensiImporter.BuildPreview();File.WriteAllText("work/ruang-bilik-20261109/unity-result.txt","OK: prefab lengkap, prefab potongan dan scene pratinjau berhasil disimpan.");}
        catch(System.Exception ex){File.WriteAllText("work/ruang-bilik-20261109/unity-result.txt",ex.ToString());Debug.LogException(ex);}
    }
}
#endif
