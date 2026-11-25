using UnityEditor;
using UnityEngine;

// Restore normal Editor input once after importing this fix. Never capture the mouse.
[InitializeOnLoad]
public static class ChapelCursorRecovery
{
    static ChapelCursorRecovery()
    {
        if (!SessionState.GetBool("Chapel.CursorRecovery10",false))
            EditorApplication.delayCall += Recover;
    }

    [MenuItem("Tools/A Little Campus Story/Chapel/0 - Pulihkan Kursor")]
    public static void Recover()
    {
        Cursor.lockState=CursorLockMode.None;
        Cursor.visible=true;
        Cursor.SetCursor(null,Vector2.zero,CursorMode.Auto);
        SessionState.SetBool("Chapel.CursorRecovery10",true);
        SceneView.RepaintAll();
        Debug.Log("Chapel cursor recovery: visible="+Cursor.visible+", lock="+Cursor.lockState+". This controls Unity only, not the Windows pointer settings.");
    }
}
