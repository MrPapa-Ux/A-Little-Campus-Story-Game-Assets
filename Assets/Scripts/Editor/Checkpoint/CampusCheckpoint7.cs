// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menerapkan susunan/revisi yang dibuat pada checkpoint 7.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CampusCheckpoint7
{
    static string Root => Directory.GetParent(Application.dataPath).FullName;
    static string Work(string file) => Path.Combine(Root, "work", file);
    static readonly Color Paper = new Color32(255, 246, 221, 255);
    static readonly Color Ink = new Color32(43, 47, 38, 255);
    static TMP_FontAsset Font => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/AmaticSC-Regular SDF.asset");
    static TMP_FontAsset BodyFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
    public static void Apply()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Pasang ketika Play berhenti.");
        EditorSceneManager.SaveOpenScenes();
        if (SceneManager.GetActiveScene().path != "Assets/Scenes/CharacterLab.unity") EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        var hp = UnityEngine.Object.FindFirstObjectByType<SistemHP>();
        if (hp == null) throw new InvalidOperationException("Sistem ponsel tidak ditemukan.");
        if (hp.petaKampus == null) BuatPeta(hp);
        foreach (Graphic graphic in hp.petaKampus.GetComponentsInChildren<Graphic>(true))
            if (graphic.GetComponent<CanvasRenderer>() == null) graphic.gameObject.AddComponent<CanvasRenderer>();
        hp.petaKampus.transform.Find("BingkaiHP").localScale = Vector3.one * .7f;
        hp.petaKampus.labelInteraksi = hp.petaKampus.transform.Find("BingkaiHP/Kertas/Interaksi/Tulisan").GetComponent<TMP_Text>();
        var npc = GameObject.Find("NPC_Lyra");
        if (npc == null) throw new InvalidOperationException("NPC contoh asli tidak ditemukan.");
        BuatNPC(npc, "NPC_Nara_Ekspresi", "Nara", 8f, false);
        BuatNPC(npc, "NPC_Bimo_Joget", "Bimo", 14f, true);
        var dialog = UnityEngine.Object.FindFirstObjectByType<SistemPilihan>();
        if (dialog.teksCeritaUtama.GetComponent<DialogTeriakGoofy>() == null) dialog.teksCeritaUtama.gameObject.AddComponent<DialogTeriakGoofy>();
        EditorUtility.SetDirty(hp);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets();
        File.WriteAllText(Work("checkpoint7-apply.txt"), "COMPLETE: peta 14 lokasi, zoom/touch/keyboard, ekspresi Inspector dan dialog, NPC Nara dan Bimo.\n");
    }
    static void BuatPeta(SistemHP hp)
    {
        const string path = "Assets/AllAssetPeta/PetaKampusGoofy.png";
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.maxTextureSize = 2048; importer.mipmapEnabled = false; importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        Transform parent = hp.panelPengaturan.transform.parent;
        var panel = Rect(parent, "AplikasiPeta", Vector2.zero, Vector2.zero); Stretch(panel);
        var backing = panel.gameObject.AddComponent<UnityEngine.UI.Image>(); backing.color = new Color(0f, 0f, 0f, .65f); backing.raycastTarget = true;
        var peta = panel.gameObject.AddComponent<PetaKampusHP>(); peta.sistemHP = hp; hp.petaKampus = peta;
        var frame = Rect(panel, "BingkaiHP", Vector2.zero, new Vector2(1720, 990)); Image(frame, new Color32(226, 112, 27, 255), false); Outline(frame.gameObject, 4f);
        var sheet = Rect(frame, "Kertas", Vector2.zero, new Vector2(1670, 946)); Image(sheet, Paper, false); Outline(sheet.gameObject, 2f);
        var viewport = Rect(sheet, "ViewportPeta", new Vector2(-450, 0), new Vector2(667, 890));
        Image(viewport, Paper, true); viewport.gameObject.AddComponent<RectMask2D>(); peta.viewport = viewport;
        var gestures = viewport.gameObject.AddComponent<GeserPetaHP>(); gestures.peta = peta;
        var content = Rect(viewport, "GambarPeta", Vector2.zero, new Vector2(667, 890));
        var picture = Image(content, Color.white, false); picture.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path); peta.isiPeta = content;
        Label(sheet, "JudulApp", "PETA", new Vector2(345, 375), new Vector2(670, 100), 88);
        peta.nomorLokasi = Label(sheet, "Nomor", "01 / 14", new Vector2(345, 290), new Vector2(670, 60), 36);
        peta.namaLokasi = Label(sheet, "LokasiTerpilih", "Jasmine", new Vector2(345, 202), new Vector2(690, 100), 70);
        peta.keterangan = Label(sheet, "CatatanLokasi", "", new Vector2(345, 97), new Vector2(650, 105), 26, true);
        Label(sheet, "Petunjuk", "Kiri / kanan: pilih tempat\nInteraksi: zoom / kembali\nSentuh lingkaran untuk melihat dekat\nGeser atau cubit saat diperbesar", new Vector2(345, -78), new Vector2(650, 135), 25, true);
        Button(sheet, "Kiri", "<", new Vector2(105, -239), new Vector2(120, 95), hp.TekanTombolUIKiri);
        Button(sheet, "Interaksi", "LIHAT DEKAT", new Vector2(345, -239), new Vector2(315, 95), hp.TekanTombolUIInteract);
        Button(sheet, "Kanan", ">", new Vector2(585, -239), new Vector2(120, 95), hp.TekanTombolUIKanan);
        Button(sheet, "ZoomMinus", "-", new Vector2(120, -368), new Vector2(95, 85), peta.Perkecil);
        Button(sheet, "Ikhtisar", "SEMUA", new Vector2(340, -368), new Vector2(235, 85), peta.LihatSemua);
        Button(sheet, "ZoomPlus", "+", new Vector2(560, -368), new Vector2(95, 85), peta.Perbesar);
        Button(frame, "KembaliKeHP", "X", new Vector2(783, 437), new Vector2(65, 65), peta.Tutup);
        string[] names = { "Jasmine", "GK 2", "Guest House", "Crystal", "Parlot Crystal", "Kafetaria", "Study Garden", "Edelweiss", "GA", "GK 1", "GK 3", "Chapel", "Taman Doa", "Kantin" };
        Vector2[] points = { new Vector2(.11f,.4f), new Vector2(.24f,.65f), new Vector2(.10f,.59f), new Vector2(.42f,.74f), new Vector2(.36f,.835f), new Vector2(.56f,.898f), new Vector2(.66f,.805f), new Vector2(.80f,.77f), new Vector2(.56f,.60f), new Vector2(.775f,.43f), new Vector2(.76f,.19f), new Vector2(.55f,.29f), new Vector2(.55f,.12f), new Vector2(.27f,.49f) };
        string[] notes = { "Area asrama Jasmine.", "Gedung Kuliah 2.", "Area Guest House.", "Area asrama Crystal.", "Area parkir kecil di sisi selatan Crystal.", "Kafetaria kampus.", "Taman belajar di antara area asrama.", "Area asrama Edelweiss.", "Gedung Administrasi.", "Gedung Kuliah 1.", "Gedung Kuliah 3 pada latar waktu game.", "Chapel pada peta game.", "Area Taman Doa.", "Area kantin kampus." };
        peta.lokasi = new TitikPetaKampus[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            var r = Rect(content, "Lingkaran_" + names[i], new Vector2((points[i].x - .5f) * 667, (.5f - points[i].y) * 890), new Vector2(56, 56));
            var ring = r.gameObject.AddComponent<LingkaranPeta>(); ring.color = peta.warnaLingkaran; ring.raycastTarget = true;
            var button = r.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = ring; button.transition = Selectable.Transition.None;
            UnityEventTools.AddIntPersistentListener(button.onClick, peta.SentuhLokasi, i);
            peta.lokasi[i] = new TitikPetaKampus { nama = names[i], keterangan = notes[i], posisi = points[i], lingkaran = ring };
        }
        // Warna layar ponsel tetap milik app lain; warna kertas disamakan pada app peta ini.
        panel.gameObject.SetActive(false);
    }
    static void BuatNPC(GameObject source, string id, string nama, float x, bool dance)
    {
        if (GameObject.Find(id) != null) return;
        GameObject clone = UnityEngine.Object.Instantiate(source, source.transform.parent); clone.name = id;
        clone.transform.position = new Vector3(x, source.transform.position.y, source.transform.position.z);
        clone.SetActive(true);
        var area = clone.GetComponent<AreaInteraksi>(); area.idTempat = id; area.daftarOpsi = Array.Empty<OpsiInteraksi>(); area.tampilkanTombolKeluar = true;
        var face = clone.GetComponentInChildren<EkspresiWajah2D>(true); face.gunakanEkspresi = true; face.ekspresi = dance ? EmosiKarakter.Senang : EmosiKarakter.Senyum;
        var animator = clone.GetComponentInChildren<Animator>(true);
        Transform visual = animator.transform;
        var shirt = visual.Find("Badan").GetComponent<SpriteRenderer>(); shirt.color = dance ? new Color(1f,.75f,.25f) : new Color(.70f,1f,.80f);
        if (dance)
        {
            // Joget menggerakkan bagian tubuh dari pose yang stabil.
            animator.enabled = false;
            var dancer = clone.AddComponent<NPCJogetGoofy>();
            dancer.badan = visual.Find("Badan"); dancer.kepala = visual.Find("KepalaArthur"); dancer.rambut = visual.Find("Rambut 1"); dancer.telinga = visual.Find("Telinga");
            dancer.tanganKiri = visual.Find("TanganKiri"); dancer.tanganKanan = visual.Find("TanganKanan"); dancer.kakiKiri = visual.Find("KakiKiri"); dancer.kakiKanan = visual.Find("KakiKanan");
            var bubble = clone.GetComponent<NPCBubbleTrigger>(); bubble.teksSimbolAsli.text = "HEI!"; bubble.teksSimbolAsli.fontStyle = FontStyles.Bold;
            area.skenarioDialog = new[] {
                Line(Pembicara.Kanan_NPC,nama,"HEI, ARTHUR! IKUT JOGET, YUK!",EmosiKarakter.Senang,true),
                Line(Pembicara.Kiri_Arthur,"Arthur","Wah, semangatmu sampai ke seluruh asrama!",EmosiKarakter.Senyum),
                Line(Pembicara.Kanan_NPC,nama,"SATU, DUA! JANGAN LUPA SENYUM!",EmosiKarakter.Senang,true) };
        }
        else area.skenarioDialog = new[] {
            Line(Pembicara.Kanan_NPC,nama,"Aduh... aku tadi kehilangan catatanku.",EmosiKarakter.Sedih),
            Line(Pembicara.Kiri_Arthur,"Arthur","Tenang, kita cari bersama.",EmosiKarakter.Senyum),
            Line(Pembicara.Kanan_NPC,nama,"Ternyata ada di tasku! Aku senang sekali!",EmosiKarakter.Senang),
            Line(Pembicara.Kanan_NPC,nama,"Tapi siapa yang mengerjaiku tadi?!",EmosiKarakter.Marah),
            Line(Pembicara.Kiri_Arthur,"Arthur","Hei! Jangan marah dulu.",EmosiKarakter.Marah),
            Line(Pembicara.Kanan_NPC,nama,"Hehe, iya. Terima kasih sudah menemani.",EmosiKarakter.Senyum) };
        EditorUtility.SetDirty(area); EditorUtility.SetDirty(face);
    }
    static BarisDialog Line(Pembicara speaker, string name, string text, EmosiKarakter emotion, bool shout = false) => new BarisDialog { siapaYangBicara = speaker, namaKarakter = name, teksDialog = text, ubahEkspresi = true, ekspresi = emotion, berteriak = shout, teksTombolLanjut = "Lanjut ->" };
    static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f,.5f); r.anchoredPosition = position; r.sizeDelta = size; return r;
    }
    static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    static UnityEngine.UI.Image Image(RectTransform r, Color c, bool raycast) { var i = r.gameObject.AddComponent<UnityEngine.UI.Image>(); i.color = c; i.raycastTarget = raycast; return i; }
    static void Outline(GameObject go, float thickness) { var o = go.AddComponent<UnityEngine.UI.Outline>(); o.effectColor = Ink; o.effectDistance = new Vector2(thickness,-thickness); }
    static TMP_Text Label(Transform parent, string name, string text, Vector2 position, Vector2 size, float fontSize, bool body = false)
    {
        var r = Rect(parent, name, position, size); var t = r.gameObject.AddComponent<TextMeshProUGUI>(); t.font = body ? BodyFont : Font;
        t.text = text; t.fontSize = fontSize; t.color = Ink; t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
        return t;
    }
    static void Button(Transform parent, string name, string text, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        var r = Rect(parent, name, position, size); var image = Image(r, Paper, true); Outline(r.gameObject, 2);
        var b = r.gameObject.AddComponent<UnityEngine.UI.Button>(); b.targetGraphic = image; UnityEventTools.AddPersistentListener(b.onClick, action);
        Label(r, "Tulisan", text, Vector2.zero, size - new Vector2(10, 8), 38);
    }
}
