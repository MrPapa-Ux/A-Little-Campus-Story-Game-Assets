using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class CampusCheckpoint20
{
    [MenuItem("Tools/A Little Campus Story/Revisi 20/Panel dialog atas dan HUD")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Gunakan Edit Mode.");
        var dialog=UnityEngine.Object.FindFirstObjectByType<DialogManager>();
        if(dialog==null)throw new InvalidOperationException("Buka CharacterLab.");
        var panel=(RectTransform)dialog.GetComponent<SistemPilihan>().teksCeritaUtama.transform.parent;
        // The existing sliced artwork has transparent padding above/below its visible box.
        panel.anchorMin=panel.anchorMax=new Vector2(.5f,1);panel.pivot=new Vector2(.5f,.5f);panel.anchoredPosition=new Vector2(0,-200);
        var ui=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(c=>c.name=="UI_Gameplay");
        dialog.hudSaatPercakapan=new[]{Group(Child(ui.transform,"LazyBar_Utama")),Group(Child(ui.transform,"Btn_HandPhone"))};
        var camera=dialog.arthurScript.GetComponentInChildren<Camera>();
        if(camera.GetComponent<BingkaiKameraTetap>()==null)camera.gameObject.AddComponent<BingkaiKameraTetap>();
        foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(!new[]{"UI_Gameplay","CanvasDialog","CanvasSistemHP","CanvasPengaturan","CanvasTransisiAplikasi"}.Contains(canvas.name))continue;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            Wrap(canvas,scaler.referenceResolution);EditorUtility.SetDirty(scaler);
        }
        EditorUtility.SetDirty(dialog);EditorSceneManager.MarkSceneDirty(dialog.gameObject.scene);EditorSceneManager.SaveScene(dialog.gameObject.scene);AssetDatabase.SaveAssets();
        Directory.CreateDirectory("work/layout20");File.WriteAllText("work/layout20/applied.txt",DateTime.UtcNow.ToString("o"));
    }
    static CanvasGroup Group(Transform target){var group=target.GetComponent<CanvasGroup>();if(group==null)group=target.gameObject.AddComponent<CanvasGroup>();return group;}
    static Transform Child(Transform parent,string name)=>parent.GetComponentsInChildren<Transform>(true).First(t=>t.name==name);
    static void Wrap(Canvas canvas,Vector2 size)
    {
        var existing=canvas.transform.Find("AreaTampilan16x9");if(existing!=null)return;
        var children=canvas.transform.Cast<Transform>().ToArray();
        var area=new GameObject("AreaTampilan16x9",typeof(RectTransform)).GetComponent<RectTransform>();area.SetParent(canvas.transform,false);
        area.anchorMin=area.anchorMax=area.pivot=new Vector2(.5f,.5f);area.anchoredPosition=Vector2.zero;area.sizeDelta=size;
        foreach(var child in children)
        {
            var rect=child as RectTransform;if(rect==null)continue;
            var min=rect.anchorMin;var max=rect.anchorMax;var pivot=rect.pivot;var pos=rect.anchoredPosition3D;var delta=rect.sizeDelta;var scale=rect.localScale;var rot=rect.localRotation;
            rect.SetParent(area,false);rect.anchorMin=min;rect.anchorMax=max;rect.pivot=pivot;rect.sizeDelta=delta;rect.anchoredPosition3D=pos;rect.localScale=scale;rect.localRotation=rot;
        }
    }
}
