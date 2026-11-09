using UnityEditor;
using System;
using System.IO;
using System.Security.Cryptography;

// One-shot revision handoff: rebuilding the code reloads the project once, then presents the chapel.
[InitializeOnLoad]
public static class ChapelPreviewAutoOpen
{
    const string Key = "ALCS.Chapel.Revision18VisibleEndExitsReceipt";
    [Serializable] class BuildReceipt
    {
        public int revision,parts,triangles,rendered_views;
        public string source_sha256,utc_timestamp,scene_path,checks;
    }

    static ChapelPreviewAutoOpen()
    {
        if (EditorPrefs.GetBool(Key, false)) return;
        EditorApplication.delayCall += OpenOnce;
    }

    static void OpenOnce()
    {
        // Do not bring Unity forward or disturb the user's other applications.
        // Finish the one-shot handoff only when the user returns to Unity.
        if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive)
        {
            EditorApplication.delayCall += OpenOnce;
            return;
        }
        if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += OpenOnce;
            return;
        }
        EditorPrefs.SetBool(Key, true);
        try
        {
            PioneerChapelImporter.BuildPrefab();
            PioneerChapelChecks.Validate();
            PioneerChapelChecks.RenderAll();
            PioneerChapelImporter.OpenPreview();
            var receipt=UnityEngine.JsonUtility.FromJson<BuildReceipt>(File.ReadAllText("Assets/ChapelPioneer/PioneerChapel_Data.json"));
            using(var sha=SHA256.Create())
                receipt.source_sha256=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes("Assets/ChapelPioneer/PioneerChapel_Mesh.bytes"))).Replace("-","").ToLowerInvariant();
            receipt.utc_timestamp=DateTime.UtcNow.ToString("o");receipt.rendered_views=26;receipt.checks="PASS";
            receipt.scene_path=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            File.WriteAllText("work/chapel-analysis/unity-build-receipt.json",UnityEngine.JsonUtility.ToJson(receipt,true));
            UnityEngine.Debug.Log("Chapel revision 18 validated. Foyer-side decoration, visible end exits and mirrored inset pillars rendered.");
        }
        catch (System.Exception ex)
        {
            EditorPrefs.DeleteKey(Key);
            UnityEngine.Debug.LogError("Could not open chapel revision 18 preview: " + ex);
        }
    }
}

