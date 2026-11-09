// PANDUAN PROYEK - Editor / SetupDanRevisi
// Fungsi: Menerapkan dan memvalidasi susunan gameplay/checkpoint 4.
// Panduan: Assets/Scripts/Editor/SetupDanRevisi/README.md

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CampusGameplayRevision
{
    const string PrefabPath = "Assets/UI/Prefabs/FullScreenSettings.prefab";
    const string MenuPath = "Assets/Scenes/MainMenu.unity";
    const string LabPath = "Assets/Scenes/CharacterLab.unity";
    public const string ExitMessage = "Sekalinya keluar dari permainan, berarti main lagi dari awal. Semua progress yang telah dimainkan akan hilang. Apa kau yakin?";
    static readonly Color Paper = new Color(0.97f, 0.95f, 0.91f);
    static readonly Color Ink = new Color(0.15f, 0.16f, 0.13f);
    static readonly Color Orange = new Color(1f, 0.52f, 0.16f);
    static TMP_FontAsset HandFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/AmaticSC-Regular SDF.asset");
    static TMP_FontAsset BodyFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

    [InitializeOnLoadMethod]
    static void Register()
    {
        EditorApplication.update -= ProcessRequest;
        EditorApplication.update += ProcessRequest;
    }

    static void ProcessRequest()
    {
        string request = Absolute("work/gameplay-revision.request");
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Report("gameplay-revision-state.txt", "WAITING: Hentikan Play mode untuk menerapkan perubahan scene.");
            return;
        }
        File.Delete(request);
        try { Apply(); }
        catch (Exception error) { Report("gameplay-revision-error.txt", error.ToString()); Debug.LogException(error); }
    }

    [MenuItem("Tools/A Little Campus Story/Terapkan Revisi Gameplay Check point 4")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Hentikan Play mode dahulu.");
        CaptureDirtyScenes();
        GameObject prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        try { CleanSettings(prefab); PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath); }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }

        Scene menu = EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Single);
        PengaturanManager menuSettings = Find<PengaturanManager>(menu);
        if (menuSettings == null) throw new InvalidOperationException("Pengaturan main menu tidak ditemukan.");
        CleanSettings(menuSettings.gameObject);
        EditorSceneManager.SaveScene(menu);

        Scene lab = EditorSceneManager.OpenScene(LabPath, OpenSceneMode.Single);
        SistemHP phone = Find<SistemHP>(lab);
        if (phone == null || phone.panelPengaturan == null) throw new InvalidOperationException("Pengaturan ponsel belum terhubung.");
        CleanSettings(phone.panelPengaturan);
        AddGameplaySettings(phone);
        AddTV(phone, lab);
        AddComingSoon(phone);
        WireIcons(phone);
        SistemPilihan dialog = Find<SistemPilihan>(lab);
        if (dialog == null) throw new InvalidOperationException("SistemPilihan tidak ditemukan.");
        dialog.jedaSetelahTeks = 0.3f;
        dialog.jedaAntarInput = 0.12f;
        EditorUtility.SetDirty(dialog);
        LazyBarArthur bar = Find<LazyBarArthur>(lab);
        if (bar == null) throw new InvalidOperationException("Parameter Arthur tidak ditemukan.");
        bar.ambangPanik = 0.3f;
        EditorUtility.SetDirty(bar);
        EditorUtility.SetDirty(phone);
        PrefabUtility.RecordPrefabInstancePropertyModifications(phone.panelPengaturan.GetComponent<PengaturanManager>());
        EditorSceneManager.MarkSceneDirty(lab);
        EditorSceneManager.SaveScene(lab);
        AssetDatabase.SaveAssets();
        Validate();
        Report("gameplay-revision-state.txt", "COMPLETE: Dialog, parameter Arthur, pengaturan gameplay, konfirmasi keluar dan transisi TV terpasang dan referensi UI diverifikasi.");
    }

    static void CleanSettings(GameObject panel)
    {
        foreach (string name in new[] { "SettingsEyebrow", "SettingsSavedHint", "SettingsSubtitle", "VolumeDescription", "BrightnessDescription", "SettingsClosingNote" })
        {
            Transform unwanted = Child(panel.transform, name);
            if (unwanted != null) UnityEngine.Object.DestroyImmediate(unwanted.gameObject);
        }
        foreach (TMP_Text label in panel.GetComponentsInChildren<TMP_Text>(true))
        {
            WigglyTMP other = label.GetComponent<WigglyTMP>();
            if (other != null) other.enabled = false;
            WigglyText wiggle = label.GetComponent<WigglyText>();
            if (wiggle == null) wiggle = label.gameObject.AddComponent<WigglyText>();
            wiggle.enabled = label.name != "ExitMessage" && label.name != "ComingSoonMessage";
            EditorUtility.SetDirty(wiggle);
        }
    }

    static void AddGameplaySettings(SistemHP phone)
    {
        GameObject panel = phone.panelPengaturan;
        PengaturanGameplay gameplay = panel.GetComponent<PengaturanGameplay>();
        if (gameplay == null) gameplay = panel.AddComponent<PengaturanGameplay>();
        gameplay.sistemHP = phone;
        Transform box = Child(panel.transform, "Box_Pengaturan");
        if (box == null) throw new InvalidOperationException("Box_Pengaturan tidak ditemukan.");
        UnityEngine.UI.Button exit = MakeButton(box, "Btn_KeluarPermainan", "Keluar dari permainan", new Vector2(0.27f, 0.095f), new Vector2(490f, 60f), Paper, gameplay.MintaKeluar);
        exit.GetComponentInChildren<TMP_Text>().fontSize = 35f;
        RectTransform confirmation = Rect(panel.transform, "KonfirmasiKeluar");
        Stretch(confirmation);
        UnityEngine.UI.Image scrim = Image(confirmation, new Color(0f, 0f, 0f, 0.68f));
        scrim.raycastTarget = true;
        confirmation.SetAsLastSibling();
        RectTransform card = Rect(confirmation, "KartuKonfirmasi");
        card.anchorMin = new Vector2(0.16f, 0.22f);
        card.anchorMax = new Vector2(0.84f, 0.78f);
        card.offsetMin = card.offsetMax = Vector2.zero;
        Image(card, Paper).raycastTarget = true;
        Outline(card.gameObject);
        Label(card, "ExitTitle", "KELUAR DARI PERMAINAN?", new Vector2(0.5f, 0.82f), new Vector2(750f, 65f), HandFont, 45f, true);
        TMP_Text message = Label(card, "ExitMessage", ExitMessage, new Vector2(0.5f, 0.50f), new Vector2(720f, 150f), BodyFont, 25f, false);
        message.textWrappingMode = TextWrappingModes.Normal;
        MakeButton(card, "Btn_Ya", "Ya", new Vector2(0.32f, 0.15f), new Vector2(205f, 60f), Orange, gameplay.YaKeluar);
        MakeButton(card, "Btn_Batal", "Batal", new Vector2(0.68f, 0.15f), new Vector2(205f, 60f), Paper, gameplay.BatalKeluar);
        gameplay.panelKonfirmasi = confirmation.gameObject;
        confirmation.gameObject.SetActive(false);
        UnityEngine.UI.Button close = Child(panel.transform, "Btn_TutupX").GetComponent<UnityEngine.UI.Button>();
        close.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(close.onClick, gameplay.TutupPengaturan);
        EditorUtility.SetDirty(close);
        EditorUtility.SetDirty(gameplay);
        panel.SetActive(false);
    }

    static void AddTV(SistemHP phone, Scene scene)
    {
        Canvas canvas = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Canvas>(true)).FirstOrDefault(item => item.name == "CanvasTransisiAplikasi");
        if (canvas == null)
        {
            GameObject root = new GameObject("CanvasTransisiAplikasi", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvas = root.GetComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;
        UnityEngine.UI.CanvasScaler scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        RectTransform overlay = Rect(canvas.transform, "EfekTV");
        Stretch(overlay);
        Image(overlay, Ink).raycastTarget = true;
        CanvasGroup group = overlay.GetComponent<CanvasGroup>();
        if (group == null) group = overlay.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        RectTransform light = Rect(overlay, "BidangCahaya");
        Stretch(light);
        Image(light, Paper).raycastTarget = false;
        TransisiAplikasiTV tv = overlay.GetComponent<TransisiAplikasiTV>();
        if (tv == null) tv = overlay.gameObject.AddComponent<TransisiAplikasiTV>();
        tv.bidangCahaya = light;
        phone.transisiAplikasi = tv;
        overlay.gameObject.SetActive(false);
        EditorUtility.SetDirty(tv);
    }

    static void AddComingSoon(SistemHP phone)
    {
        RectTransform panel = Rect(phone.panelPengaturan.transform.parent, "PanelAplikasiSegeraHadir");
        Stretch(panel);
        Image(panel, Paper).raycastTarget = true;
        phone.panelSegeraHadir = panel.gameObject;
        phone.judulSegeraHadir = Label(panel, "AppTitle", "APLIKASI", new Vector2(0.5f, 0.62f), new Vector2(800f, 85f), HandFont, 65f, true);
        Label(panel, "ComingSoonMessage", "Segera hadir", new Vector2(0.5f, 0.43f), new Vector2(800f, 65f), BodyFont, 27f, false);
        MakeButton(panel, "Btn_KembaliPonsel", "Kembali", new Vector2(0.5f, 0.24f), new Vector2(300f, 65f), Orange, phone.TutupAplikasi);
        panel.gameObject.SetActive(false);
    }

    static void WireIcons(SistemHP phone)
    {
        foreach (GameObject icon in phone.daftarIkon)
        {
            if (icon == null) continue;
            UnityEngine.UI.Button button = icon.GetComponent<UnityEngine.UI.Button>();
            if (button == null) button = icon.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = icon.GetComponent<UnityEngine.UI.Image>();
            button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            UnityEventTools.AddStringPersistentListener(button.onClick, phone.BukaAplikasi, icon.name);
            EditorUtility.SetDirty(button);
        }
    }

    public static void Validate()
    {
        var checks = new List<string>();
        Scene scene = EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Single);
        PengaturanManager menu = Find<PengaturanManager>(scene);
        Check(menu != null && menu.GetComponent<PengaturanGameplay>() == null && Child(menu.transform, "Btn_KeluarPermainan") == null, "MainMenu tanpa fungsi keluar gameplay", checks);
        VerifySettings(menu, checks);
        scene = EditorSceneManager.OpenScene(LabPath, OpenSceneMode.Single);
        SistemHP phone = Find<SistemHP>(scene);
        PengaturanManager settings = phone.panelPengaturan.GetComponent<PengaturanManager>();
        VerifySettings(settings, checks);
        PengaturanGameplay gameplay = settings.GetComponent<PengaturanGameplay>();
        Check(gameplay != null && gameplay.panelKonfirmasi != null && !gameplay.panelKonfirmasi.activeSelf, "Konfirmasi gameplay tersambung dan awalnya tertutup", checks);
        Check(Child(gameplay.panelKonfirmasi.transform, "ExitMessage").GetComponent<TMP_Text>().text == ExitMessage, "Pesan konfirmasi sesuai permintaan", checks);
        VerifyButton(Child(gameplay.panelKonfirmasi.transform, "Btn_Ya"), gameplay, "YaKeluar", checks);
        VerifyButton(Child(gameplay.panelKonfirmasi.transform, "Btn_Batal"), gameplay, "BatalKeluar", checks);
        VerifyButton(Child(settings.transform, "Btn_KeluarPermainan"), gameplay, "MintaKeluar", checks);
        VerifyButton(Child(settings.transform, "Btn_TutupX"), gameplay, "TutupPengaturan", checks);
        Check(phone.transisiAplikasi != null && phone.transisiAplikasi.bidangCahaya != null && phone.panelSegeraHadir != null, "Transisi TV dan panel aplikasi terhubung", checks);
        foreach (GameObject icon in phone.daftarIkon.Where(icon => icon != null)) VerifyButton(icon.transform, phone, "BukaAplikasi", checks);
        Check(Find<SistemPilihan>(scene).jedaSetelahTeks >= 0.3f, "Jeda input dialog tersimpan", checks);
        Check(Mathf.Approximately(Find<LazyBarArthur>(scene).ambangPanik, 0.3f), "Bar panik hanya pada 30 persen terakhir", checks);
        Check(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true)).Count() == 1, "Satu EventSystem pada CharacterLab", checks);
        EditorSceneManager.OpenScene(MenuPath, OpenSceneMode.Single);
        Report("gameplay-validation.txt", string.Join("\n", checks));
    }

    static void VerifySettings(PengaturanManager settings, List<string> checks)
    {
        Check(settings != null && settings.sliderVolume != null && settings.sliderCahaya != null && settings.panelHitamFilter != null, "Slider dan filter tetap terhubung", checks);
        foreach (string name in new[] { "SettingsSubtitle", "VolumeDescription", "BrightnessDescription", "SettingsEyebrow", "SettingsSavedHint" }) Check(Child(settings.transform, name) == null, name + " dihapus", checks);
        foreach (TMP_Text text in new[] { settings.teksVolume, settings.teksCahaya, Child(settings.transform, "SettingsHeading").GetComponent<TMP_Text>() })
            Check(text.GetComponent<WigglyText>() != null && text.GetComponent<WigglyText>().enabled, text.name + " tetap wiggly", checks);
    }

    static void VerifyButton(Transform target, UnityEngine.Object owner, string method, List<string> checks)
    {
        UnityEngine.UI.Button button = target.GetComponent<UnityEngine.UI.Button>();
        Check(button != null && button.onClick.GetPersistentEventCount() == 1 && button.onClick.GetPersistentTarget(0) == owner && button.onClick.GetPersistentMethodName(0) == method, target.name + " -> " + method, checks);
    }

    static void CaptureDirtyScenes()
    {
        string checkpoint = Absolute("Checkpoints/Check point 4");
        if (!File.Exists(Path.Combine(checkpoint, "checkpoint.json"))) throw new InvalidOperationException("Cadangan Check point 4 belum tersedia.");
        if (File.Exists(Path.Combine(checkpoint, "editor-state-captured.txt"))) { EditorSceneManager.SaveOpenScenes(); return; }
        var paths = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isDirty || string.IsNullOrEmpty(scene.path)) continue;
            EditorSceneManager.SaveScene(scene);
            string destination = Path.Combine(checkpoint, scene.path);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(Absolute(scene.path), destination, true);
            paths.Add(scene.path);
        }
        File.WriteAllText(Path.Combine(checkpoint, "editor-state-captured.txt"), "Scene belum tersimpan ikut dicadangkan:\n" + string.Join("\n", paths));
    }

    static RectTransform Rect(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }
    static UnityEngine.UI.Image Image(RectTransform rect, Color color)
    {
        UnityEngine.UI.Image image = rect.GetComponent<UnityEngine.UI.Image>();
        if (image == null) image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = color;
        return image;
    }
    static TMP_Text Label(Transform parent, string name, string text, Vector2 anchor, Vector2 size, TMP_FontAsset font, float fontSize, bool wiggle)
    {
        RectTransform rect = Rect(parent, name);
        Place(rect, anchor, size);
        TMP_Text label = rect.GetComponent<TextMeshProUGUI>();
        if (label == null) label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font; label.fontSize = fontSize; label.text = text; label.color = Ink;
        label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        if (wiggle && label.GetComponent<WigglyText>() == null) label.gameObject.AddComponent<WigglyText>();
        return label;
    }
    static UnityEngine.UI.Button MakeButton(Transform parent, string name, string text, Vector2 anchor, Vector2 size, Color color, UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = Rect(parent, name);
        Place(rect, anchor, size);
        UnityEngine.UI.Image image = Image(rect, color);
        image.raycastTarget = true;
        Outline(rect.gameObject);
        UnityEngine.UI.Button button = rect.GetComponent<UnityEngine.UI.Button>();
        if (button == null) button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = image;
        button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(button.onClick, action);
        TMP_Text label = Label(rect, "Label", text, new Vector2(0.5f, 0.5f), size - new Vector2(16f, 8f), HandFont, 38f, true);
        return button;
    }
    static void Outline(GameObject go)
    {
        UnityEngine.UI.Outline outline = go.GetComponent<UnityEngine.UI.Outline>();
        if (outline == null) outline = go.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Ink; outline.effectDistance = new Vector2(2f, -2f);
    }
    static void Place(RectTransform rect, Vector2 anchor, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = anchor; rect.pivot = new Vector2(0.5f, 0.5f); rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size; rect.localScale = Vector3.one;
    }
    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one;
    }
    static Transform Child(Transform root, string name) => root.GetComponentsInChildren<Transform>(true).FirstOrDefault(item => item.name == name);
    static T Find<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>(true)).FirstOrDefault();
    static void Check(bool ok, string message, List<string> checks) { if (!ok) throw new InvalidOperationException(message); checks.Add("OK: " + message); }
    static string Absolute(string path) => Path.Combine(Directory.GetParent(Application.dataPath).FullName, path);
    static void Report(string name, string message) { Directory.CreateDirectory(Absolute("work")); File.WriteAllText(Absolute("work/" + name), message); }
}
