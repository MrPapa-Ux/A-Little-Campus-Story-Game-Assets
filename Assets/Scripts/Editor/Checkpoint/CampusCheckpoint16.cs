// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menerapkan susunan/revisi yang dibuat pada checkpoint 16.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class CampusCheckpoint16
{
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit Mode diperlukan.");
        var app=UnityEngine.Object.FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include).tas;
        var name=app.labelBaris[0];name.rectTransform.sizeDelta=new Vector2(430,90);
        Paper(name,"LatarNamaBarang",new Vector2(230,86),-.7f);
        var hint=app.transform.Find("BingkaiHP/Kertas/BagianDalamTas/LabelKompartemen").GetComponent<TMP_Text>();hint.rectTransform.sizeDelta=new Vector2(570,125);
        Paper(hint,"LatarPetunjuk",new Vector2(380,140),.5f);
        EditorSceneManager.MarkSceneDirty(app.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
    }
    static void Paper(TMP_Text text,string name,Vector2 size,float angle)
    {
        var parent=text.transform.parent;var rect=parent.Find(name) as RectTransform;
        if(rect==null){rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);rect.gameObject.AddComponent<Image>();}
        rect.anchorMin=text.rectTransform.anchorMin;rect.anchorMax=text.rectTransform.anchorMax;rect.pivot=text.rectTransform.pivot;rect.anchoredPosition=text.rectTransform.anchoredPosition;rect.sizeDelta=size;rect.localRotation=Quaternion.Euler(0,0,angle);
        int textIndex=text.transform.GetSiblingIndex();rect.SetSiblingIndex(rect.GetSiblingIndex()<textIndex?textIndex-1:textIndex);
        var image=rect.GetComponent<Image>();image.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");image.type=Image.Type.Sliced;image.color=new Color32(246,232,202,255);image.raycastTarget=false;
        var outline=rect.GetComponent<Outline>();if(outline==null)outline=rect.gameObject.AddComponent<Outline>();outline.effectColor=new Color32(43,31,23,255);outline.effectDistance=new Vector2(2,-2);
        var wig=rect.GetComponent<WigglyBox>();if(wig==null)wig=rect.gameObject.AddComponent<WigglyBox>();wig.kecepatanAnimasi=7;wig.kekuatanGoyangPosisi=1.3f;wig.kekuatanGoyangRotasi=.65f;
        var fit=rect.GetComponent<LatarTeksPasHP>();if(fit==null)fit=rect.gameObject.AddComponent<LatarTeksPasHP>();fit.teks=text;fit.ruangTepi=new Vector2(40,18);fit.ukuranMinimum=new Vector2(name=="LatarPetunjuk"?240:130,name=="LatarPetunjuk"?120:76);fit.ukuranMaksimum=new Vector2(520,153);
        text.color=new Color32(35,27,20,255);text.raycastTarget=false;
        EditorUtility.SetDirty(text);EditorUtility.SetDirty(wig);EditorUtility.SetDirty(image);
    }
}
