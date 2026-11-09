// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menerapkan susunan/revisi yang dibuat pada checkpoint 8.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CampusCheckpoint8
{
    static readonly Color Paper = new Color32(255,246,221,255);
    static readonly Color Ink = new Color32(43,47,38,255);
    static readonly Color Orange = new Color32(226,112,27,255);
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Pasang di Edit Mode.");
        EditorSceneManager.SaveOpenScenes();
        const string prefab = "Assets/UI/Prefabs/FullScreenSettings.prefab";
        GameObject contents = PrefabUtility.LoadPrefabContents(prefab);
        try { PengaturanMengambang(contents); PrefabUtility.SaveAsPrefabAsset(contents,prefab); }
        finally { PrefabUtility.UnloadPrefabContents(contents); }
        foreach (string path in new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/CharacterLab.unity" })
        {
            Scene scene = EditorSceneManager.OpenScene(path);
            foreach (PengaturanManager settings in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PengaturanManager>(true))) PengaturanMengambang(settings.gameObject);
            var hp = UnityEngine.Object.FindFirstObjectByType<SistemHP>();
            if (hp != null)
            {
                Peta(hp); Placeholder(hp); PoseHP(hp);
                var settingsBorder = hp.panelPengaturan.transform.Find("BingkaiMengambang").GetComponent<UnityEngine.UI.Image>();
                WarnaBingkai(hp.panelPengaturan,hp,settingsBorder);
                var bimo = GameObject.Find("NPC_Bimo_Joget");
                var animator = bimo.GetComponentInChildren<Animator>(true);
                AnimationClip idle = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Referensi/Rayman_Reference/NPCIIdle.anim");
                idle.SampleAnimation(animator.gameObject,0f); animator.enabled = false;
                foreach (EkspresiWajah2D face in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<EkspresiWajah2D>(true)))
                {
                    Transform hair = face.transform.parent.Find("Rambut 1");
                    Transform ear = face.transform.parent.Find("Telinga");
                    if (hair != null) hair.GetComponent<SpriteRenderer>().sortingOrder = face.kepala.sortingOrder + 10;
                    if (ear != null) ear.GetComponent<SpriteRenderer>().sortingOrder = face.kepala.sortingOrder + 12;
                }
                foreach (MobilKampus c in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MobilKampus>(true)))
                { c.peluangKlakson = .03f; c.arahJalur = Vector3.left; c.jalur = "JalanKampusDepan"; EditorUtility.SetDirty(c); }
                EditorUtility.SetDirty(hp);
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work/checkpoint8-apply.txt"),"COMPLETE: aplikasi mengambang, wiggly, pose HP, joget menapak, mata asli dan lalu lintas 3%.\n");
    }
    static void Peta(SistemHP hp)
    {
        var root = hp.petaKampus.transform;
        root.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,.28f);
        root.Find("BingkaiHP").localScale = Vector3.one * .56f;
        var sheet = root.Find("BingkaiHP/Kertas");
        var close = (RectTransform)(root.Find("BingkaiHP/KembaliKeHP") ?? sheet.Find("KembaliKeHP")); close.SetParent(sheet,false); TutupRapi(close,86f);
        foreach (string name in new[] { "JudulApp", "LokasiTerpilih", "Interaksi/Tulisan", "Ikhtisar/Tulisan" }) Wiggle(sheet.Find(name).GetComponent<TMP_Text>(),1.5f);
        foreach (var location in hp.petaKampus.lokasi) location.lingkaran.rectTransform.sizeDelta = new Vector2(68,68);
        WarnaBingkai(root.gameObject,hp,root.Find("BingkaiHP").GetComponent<UnityEngine.UI.Image>());
    }
    static void PengaturanMengambang(GameObject panel)
    {
        panel.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,.28f);
        var box = (RectTransform)panel.transform.Find("Box_Pengaturan"); box.localScale = Vector3.one * .78f;
        var image = box.GetComponent<UnityEngine.UI.Image>(); image.color = Paper; image.raycastTarget = true;
        var outline = box.GetComponent<UnityEngine.UI.Outline>(); if (outline != null) outline.enabled = false;
        var border = Border(panel.transform,"BingkaiMengambang"); border.localScale = Vector3.one * .78f;
        border.SetAsFirstSibling();
        TutupRapi((RectTransform)box.Find("Btn_TutupX"),68f);
        foreach (string name in new[] { "SettingsHeading", "Judul_Volume", "Judul_Cahaya" }) Wiggle(box.Find(name).GetComponent<TMP_Text>(),1.4f);
    }
    static void Placeholder(SistemHP hp)
    {
        Transform root = hp.panelSegeraHadir.transform; root.GetComponent<UnityEngine.UI.Image>().color = new Color(0,0,0,.28f);
        Transform sheet = root.Find("KertasAplikasi");
        if (sheet == null)
        {
            var card = Rect(root,"KertasAplikasi"); Stretch(card); Image(card,Paper,true); sheet = card;
            foreach (string name in new[] { "AppTitle", "ComingSoonMessage", "Btn_KembaliPonsel" })
            {
                var child = (RectTransform)root.Find(name); Vector2 pos = child.anchoredPosition; child.SetParent(card,false); child.anchoredPosition = pos;
            }
        }
        sheet.localScale = Vector3.one * .78f;
        var border = Border(root,"BingkaiMengambang"); border.localScale = Vector3.one * .78f; border.SetAsFirstSibling();
        WarnaBingkai(root.gameObject,hp,border.GetComponent<UnityEngine.UI.Image>());
    }
    static void PoseHP(SistemHP hp)
    {
        var player = UnityEngine.Object.FindFirstObjectByType<KarakterController>(); var v = player.wadahVisual;
        var pose = player.GetComponent<PoseMemegangHP>(); if (pose == null) pose = player.gameObject.AddComponent<PoseMemegangHP>();
        pose.wadahVisual = v; pose.kepala = v.Find("KepalaArthur"); pose.rambut = v.Find("Rambut 1"); pose.telinga = v.Find("Telinga"); pose.tanganKiri = v.Find("TanganKiri"); pose.tanganKanan = v.Find("TanganKanan");
        pose.poseBerdiri = player.animator.runtimeAnimatorController.animationClips.First(c=>c.name.Contains("Idle"));
        var phone = v.Find("HPDalamGenggaman");
        if (phone == null) { phone = new GameObject("HPDalamGenggaman",typeof(SpriteRenderer)).transform; phone.SetParent(v,false); }
        var sprite = phone.GetComponent<SpriteRenderer>(); sprite.sprite = hp.badanHP.sprite; sprite.color = hp.warnaDefaultHP;
        sprite.sortingLayerID = pose.kepala.GetComponent<SpriteRenderer>().sortingLayerID; sprite.sortingOrder = 5;
        phone.localScale = Vector3.one * (.55f / sprite.sprite.bounds.size.y); phone.localPosition = pose.posisiHP; phone.gameObject.SetActive(false);
        pose.hpKecil = sprite; hp.poseKarakter = pose; EditorUtility.SetDirty(pose);
    }
    static void WarnaBingkai(GameObject root,SistemHP hp,UnityEngine.UI.Image frame)
    {
        var sync = root.GetComponent<BingkaiAplikasiHP>(); if (sync == null) sync = root.AddComponent<BingkaiAplikasiHP>(); sync.sistemHP = hp; sync.bingkai = frame; EditorUtility.SetDirty(sync);
    }
    static void TutupRapi(RectTransform r,float size)
    {
        r.anchorMin = r.anchorMax = r.pivot = Vector2.one; r.anchoredPosition = new Vector2(-24,-24); r.sizeDelta = Vector2.one * size; r.localScale = Vector3.one;
        foreach (TMP_Text t in r.GetComponentsInChildren<TMP_Text>(true))
        {
            t.rectTransform.anchorMin = Vector2.zero; t.rectTransform.anchorMax = Vector2.one; t.rectTransform.offsetMin = t.rectTransform.offsetMax = Vector2.zero;
            t.rectTransform.pivot = new Vector2(.5f,.5f); t.alignment = TextAlignmentOptions.Center; t.fontSize = size * .58f; t.raycastTarget = false; Wiggle(t,1.8f);
        }
    }
    static void Wiggle(TMP_Text t,float strength)
    {
        var other = t.GetComponent<WigglyTMP>(); if (other != null) other.enabled = false;
        var w = t.GetComponent<WigglyText>(); if (w == null) w = t.gameObject.AddComponent<WigglyText>(); w.enabled = true; w.kekuatanGoyang = strength; w.kecepatanAnimasi = 7f;
    }
    static RectTransform Border(Transform parent,string name)
    {
        var r = Rect(parent,name); Stretch(r); r.offsetMin = new Vector2(-14,-14); r.offsetMax = new Vector2(14,14); Image(r,Orange,false);
        var outline = r.GetComponent<UnityEngine.UI.Outline>(); if (outline == null) outline = r.gameObject.AddComponent<UnityEngine.UI.Outline>(); outline.effectColor = Ink; outline.effectDistance = new Vector2(2,-2); return r;
    }
    static RectTransform Rect(Transform parent,string name)
    {
        var t = parent.Find(name); if (t != null) return (RectTransform)t;
        var r = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); return r;
    }
    static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; r.pivot = new Vector2(.5f,.5f); }
    static UnityEngine.UI.Image Image(RectTransform r,Color color,bool raycast) { var i = r.GetComponent<UnityEngine.UI.Image>(); if (i == null) i = r.gameObject.AddComponent<UnityEngine.UI.Image>(); i.color = color; i.raycastTarget = raycast; return i; }
}
