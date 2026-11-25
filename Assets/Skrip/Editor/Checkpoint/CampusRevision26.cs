using UnityEditor;
using UnityEditor.SceneManagement;
public static class CampusRevision26
{
    public static void ReloadRestoredScene()
    {
        if(EditorApplication.isPlaying||EditorApplication.isPlayingOrWillChangePlaymode)throw new System.InvalidOperationException("Tunggu Edit Mode.");
        var active=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(active.isDirty)throw new System.InvalidOperationException("Scene memiliki perubahan yang belum disimpan; muat ulang pemulihan setelah perubahan pengguna disimpan.");
        if(!string.IsNullOrEmpty(active.path))EditorSceneManager.OpenScene(active.path,OpenSceneMode.Single);
    }
}
