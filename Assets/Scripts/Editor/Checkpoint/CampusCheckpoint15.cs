// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menerapkan susunan/revisi yang dibuat pada checkpoint 15.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class CampusCheckpoint15
{
    static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/AmaticSC-Regular SDF.asset");
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit Mode diperlukan.");
        var h=UnityEngine.Object.FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include);
        foreach(var app in new[]{h.tas,h.pesan,h.tugas})
        {
            Style(app.jumlah,52);app.jumlah.rectTransform.anchoredPosition=new Vector2(-419,-329);app.jumlah.rectTransform.sizeDelta=new Vector2(590,72);
            Style(app.labelAksi,48);app.labelAksi.rectTransform.sizeDelta=new Vector2(690,84);
            var sheet=app.transform.Find("BingkaiHP/Kertas");
            foreach(string name in new[]{"Kiri","Kanan"}){var arrow=sheet.Find(name).GetComponentInChildren<TMP_Text>(true);Style(arrow,56);arrow.rectTransform.sizeDelta=new Vector2(110,90);}
            EditorUtility.SetDirty(app);app.Segarkan();
        }
        var sms=h.pesan.transform.Find("BingkaiHP/Kertas");var inbox=sms.Find("Subjudul").GetComponent<TMP_Text>();Style(inbox,56);inbox.rectTransform.sizeDelta=new Vector2(590,82);Style(h.pesan.statusKontak,42);h.pesan.statusKontak.rectTransform.sizeDelta=new Vector2(560,75);
        var bag=h.tas;
        foreach(var button in bag.tombolBaris.Skip(1))button.gameObject.SetActive(false);
        bag.tombolBaris=bag.tombolBaris.Take(1).ToArray();bag.labelBaris=bag.labelBaris.Take(1).ToArray();bag.latarBaris=bag.latarBaris.Take(1).ToArray();bag.ikonBaris=bag.ikonBaris.Take(1).ToArray();
        var row=(RectTransform)bag.tombolBaris[0].transform;row.anchoredPosition=new Vector2(-419,45);row.sizeDelta=new Vector2(530,340);
        bag.warnaPilihan=Color.clear;bag.latarBaris[0].color=Color.clear;
        var outline=row.GetComponent<Outline>();if(outline!=null)UnityEngine.Object.DestroyImmediate(outline);
        var seam=row.Find("JahitanSlot");if(seam!=null)seam.gameObject.SetActive(false);
        var icon=bag.ikonBaris[0].rectTransform;icon.anchoredPosition=new Vector2(0,52);icon.sizeDelta=new Vector2(230,230);
        var label=bag.labelBaris[0];label.rectTransform.anchoredPosition=new Vector2(0,-120);label.rectTransform.sizeDelta=new Vector2(500,90);Style(label,52);label.color=new Color32(255,241,215,255);
        var hint=bag.transform.Find("BingkaiHP/Kertas/BagianDalamTas/LabelKompartemen").GetComponent<TMP_Text>();Style(hint,48);hint.text="TARIK RITSLETING\nUNTUK PINDAH BARANG";hint.rectTransform.sizeDelta=new Vector2(630,125);hint.rectTransform.anchoredPosition=new Vector2(0,-230);
        bag.Segarkan();EditorUtility.SetDirty(bag);
        EditorSceneManager.MarkSceneDirty(h.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
    }
    static void Style(TMP_Text t,float size){t.font=Font;t.fontStyle=FontStyles.Normal;t.fontSize=size;t.enableAutoSizing=false;t.overflowMode=TextOverflowModes.Ellipsis;EditorUtility.SetDirty(t);}
}
