// PANDUAN PROYEK - Editor / SetupDanRevisi
// Fungsi: Menerapkan revisi bentuk bukaan ritsleting tas.
// Panduan: Assets/Scripts/Editor/SetupDanRevisi/README.md

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class CampusRitsletingOpening
{
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit Mode diperlukan.");
        var app=UnityEngine.Object.FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include).tas;var zip=app.ritsleting;
        var image=zip.GetComponent<Image>();image.color=Color.clear;image.raycastTarget=true;
        var outline=zip.GetComponent<Outline>();if(outline!=null)outline.enabled=false;
        foreach(Transform child in zip.transform)if(child.name.StartsWith("Gigi_"))child.gameObject.SetActive(false);
        var visual=zip.transform.Find("BukaanVisual") as RectTransform;
        if(visual==null){visual=new GameObject("BukaanVisual",typeof(RectTransform)).GetComponent<RectTransform>();visual.SetParent(zip.transform,false);visual.gameObject.AddComponent<BukaanRitsletingUI>();}
        visual.anchorMin=Vector2.zero;visual.anchorMax=Vector2.one;visual.pivot=new Vector2(.5f,.5f);visual.sizeDelta=Vector2.zero;visual.anchoredPosition=Vector2.zero;visual.SetAsFirstSibling();
        zip.bukaan=visual.GetComponent<BukaanRitsletingUI>();zip.bukaan.raycastTarget=false;zip.bukaan.margin=zip.margin;zip.tarikan.SetAsLastSibling();zip.Segarkan();
        EditorUtility.SetDirty(zip);EditorSceneManager.MarkSceneDirty(app.gameObject.scene);EditorSceneManager.SaveOpenScenes();
    }
}
