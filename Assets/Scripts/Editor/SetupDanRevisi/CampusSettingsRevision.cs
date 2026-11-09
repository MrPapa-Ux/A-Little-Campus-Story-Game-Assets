// PANDUAN PROYEK - Editor / SetupDanRevisi
// Fungsi: Menerapkan revisi menu/pengaturan, wiggly, dan rute nama scene.
// Panduan: Assets/Scripts/Editor/SetupDanRevisi/README.md

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CampusSettingsRevision
{
    const string OldMenu = "Assets/Scenes/SampleScene.unity";
    const string NewMenu = "Assets/Scenes/MainMenu.unity";
    const string PhoneScene = "Assets/Scenes/CharacterLab.unity";
    const string SettingsPrefab = "Assets/UI/Prefabs/FullScreenSettings.prefab";
    static readonly Color Paper = new Color(0.97f, 0.95f, 0.91f, 1f);
    static readonly Color Ink = new Color(0.15f, 0.16f, 0.13f, 1f);
    static readonly Color Muted = new Color(0.38f, 0.40f, 0.34f, 1f);
    static readonly Color Orange = new Color(1f, 0.52f, 0.16f, 1f);
    static TMP_FontAsset HandFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/AmaticSC-Regular SDF.asset");
    static TMP_FontAsset BodyFont => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

    [InitializeOnLoadMethod]
    static void QueueRequestedRevision()
    {
        EditorApplication.update -= RunRequestedRevision;
        EditorApplication.update += RunRequestedRevision;
    }

    static void RunRequestedRevision()
    {
        string request = Absolute("work/settings-revision.request");
        string pendingValidation = Absolute("work/settings-revision.validate");
        string checkpoint2Request = Absolute("work/checkpoint2.request");
        string wiggleRequest = Absolute("work/settings-wiggle-refine.request");
        if (!File.Exists(request) && !File.Exists(pendingValidation) && !File.Exists(checkpoint2Request) && !File.Exists(wiggleRequest)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            WriteReport("settings-revision-state.txt", "Menunggu Play mode berhenti sebelum mengubah scene.");
            return;
        }
        EditorApplication.update -= RunRequestedRevision;
        try
        {
            if (File.Exists(wiggleRequest))
            {
                File.Delete(wiggleRequest);
                ApplySettingsWiggle();
            }
            else if (File.Exists(checkpoint2Request))
            {
                File.Delete(checkpoint2Request);
                ApplyCheckpoint2();
            }
            else if (File.Exists(request))
            {
                File.Delete(request);
                WriteReport("settings-revision-state.txt", "Menerapkan pengaturan melalui Editor aktif.");
                ApplyMainMenu();
                ConnectPhone();
                File.WriteAllText(pendingValidation, "Validasi setelah kompilasi ulang selesai.");
                RenameMainMenu();
                EditorApplication.update += RunRequestedRevision;
            }
            else
            {
                Validate();
                File.Delete(pendingValidation);
                WriteReport("settings-revision-state.txt", "Selesai: tampilan, hubungan slider, tombol X, IkonSetting, dan nama scene sudah diverifikasi.");
            }
        }
        catch (Exception exception)
        {
            WriteReport("settings-revision-error.txt", exception.ToString());
            Debug.LogException(exception);
        }
    }

    [MenuItem("Tools/A Little Campus Story/Pasang Wiggly Text Pengaturan")]
    public static void ApplySettingsWiggle()
    {
        RequireEditMode();
        EditorSceneManager.SaveOpenScenes();
        GameObject contents = PrefabUtility.LoadPrefabContents(SettingsPrefab);
        try
        {
            AddSettingsWiggle(contents);
            PrefabUtility.SaveAsPrefabAsset(contents, SettingsPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(contents); }
        var report = new List<string>();
        foreach (string path in new[] { PhoneScene, MenuPath() })
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            PengaturanManager settings = Find<PengaturanManager>(scene);
            if (settings == null) throw new InvalidOperationException("Pengaturan tidak ditemukan: " + path);
            AddSettingsWiggle(settings.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            TMP_Text[] labels = settings.GetComponentsInChildren<TMP_Text>(true);
            if (labels.Length == 0 || labels.Any(label => label.GetComponent<WigglyText>() == null || label.GetComponent<WigglyText>().enabled != ShouldWiggle(label)))
                throw new InvalidOperationException("Wiggly Text belum lengkap: " + path);
            if (FindChild(settings.transform, "SettingsEyebrow") != null || FindChild(settings.transform, "SettingsSavedHint") != null)
                throw new InvalidOperationException("Teks tambahan masih ada: " + path);
            report.Add(path + ": header kecil/footer dihapus; teks kecil diam; judul, label utama dan persentase tetap WigglyText.");
        }
        AssetDatabase.SaveAssets();
        WriteReport("settings-wiggle-state.txt", "COMPLETE\n" + string.Join("\n", report));
    }

    static void AddSettingsWiggle(GameObject panel)
    {
        foreach (string name in new[] { "SettingsEyebrow", "SettingsSavedHint", "SettingsSubtitle", "VolumeDescription", "BrightnessDescription" })
        {
            Transform extra = FindChild(panel.transform, name);
            if (extra != null) UnityEngine.Object.DestroyImmediate(extra.gameObject);
        }
        foreach (TMP_Text label in panel.GetComponentsInChildren<TMP_Text>(true))
        {
            WigglyTMP other = label.GetComponent<WigglyTMP>();
            if (other != null) other.enabled = false;
            WigglyText wiggle = label.GetComponent<WigglyText>();
            if (wiggle == null) wiggle = label.gameObject.AddComponent<WigglyText>();
            wiggle.enabled = ShouldWiggle(label);
            EditorUtility.SetDirty(wiggle);
        }
    }

    static bool ShouldWiggle(TMP_Text label)
    {
        return label.name != "VolumeDescription" && label.name != "BrightnessDescription" && label.name != "SettingsSubtitle";
    }

    [MenuItem("Tools/A Little Campus Story/Terapkan Revisi Check point 2")]
    public static void ApplyCheckpoint2FromMenu()
    {
        ApplyCheckpoint2();
    }

    public static string ApplyCheckpoint2()
    {
        RequireEditMode();
        EditorSceneManager.SaveOpenScenes();
        GameObject contents = PrefabUtility.LoadPrefabContents(SettingsPrefab);
        try
        {
            RemoveSettingsQuote(contents);
            PrefabUtility.SaveAsPrefabAsset(contents, SettingsPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(contents); }

        foreach (string path in new[] { MenuPath(), PhoneScene })
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            PengaturanManager settings = Find<PengaturanManager>(scene);
            if (settings == null) throw new InvalidOperationException("Pengaturan tidak ditemukan di " + path);
            RemoveSettingsQuote(settings.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        const string videoScene = "Assets/Scenes/IntroVideoScene.unity";
        EditorBuildSettings.scenes = EditorBuildSettings.scenes.Where(entry => entry.path != videoScene).ToArray();
        Scene introScene = EditorSceneManager.OpenScene("Assets/Scenes/IntroTeksScene.unity", OpenSceneMode.Single);
        IntroTitle intro = Find<IntroTitle>(introScene);
        if (intro == null) throw new InvalidOperationException("IntroTitle tidak ditemukan.");
        intro.namaSceneMenu = "MainMenu";
        EditorUtility.SetDirty(intro);
        EditorSceneManager.MarkSceneDirty(introScene);
        EditorSceneManager.SaveScene(introScene);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(videoScene) != null && !AssetDatabase.DeleteAsset(videoScene))
            throw new InvalidOperationException("IntroVideoScene tidak berhasil dihapus.");
        AssetDatabase.SaveAssets();

        Validate();
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(videoScene) != null) throw new InvalidOperationException("IntroVideoScene masih ada.");
        if (EditorBuildSettings.scenes.Any(entry => entry.path == videoScene)) throw new InvalidOperationException("Intro video masih ada pada daftar build.");
        Scene menuScene = SceneManager.GetActiveScene();
        PengaturanManager menuSettings = Find<PengaturanManager>(menuScene);
        if (FindChild(menuSettings.transform, "SettingsClosingNote") != null) throw new InvalidOperationException("Kutipan pengaturan masih ada.");
        string result = "Selesai: intro teks langsung ke MainMenu, IntroVideoScene dihapus dari aset dan build, kutipan pengaturan dihapus, aksen oranye serta fungsi slider/X dipertahankan.";
        WriteReport("checkpoint2-state.txt", result);
        return result;
    }

    static void RemoveSettingsQuote(GameObject panel)
    {
        Transform quote = FindChild(panel.transform, "SettingsClosingNote");
        if (quote != null) UnityEngine.Object.DestroyImmediate(quote.gameObject);
    }

    [MenuItem("Tools/A Little Campus Story/Terapkan Revisi Pengaturan")]
    public static void ApplyAll()
    {
        ApplyMainMenu();
        ConnectPhone();
        RenameMainMenu();
        Debug.Log("Pengaturan selesai. Cadangan: Checkpoints/Check point 1.");
    }

    public static string ApplyMainMenu()
    {
        RequireEditMode();
        CaptureOriginalEditorScenes();
        Scene scene = EditorSceneManager.OpenScene(MenuPath(), OpenSceneMode.Single);
        TitleMenuManager menu = Find<TitleMenuManager>(scene);
        if (menu == null || menu.pengaturanPanel == null) throw new InvalidOperationException("Panel pengaturan main menu tidak ditemukan.");
        GameObject panel = menu.pengaturanPanel;
        PengaturanManager settings = panel.GetComponent<PengaturanManager>();
        if (settings == null || settings.sliderVolume == null || settings.sliderCahaya == null) throw new InvalidOperationException("Slider asli belum terhubung.");
        CanvasGroup originalFilter = settings.panelHitamFilter;
        RectTransform box = FindChild(panel.transform, "Box_Pengaturan") as RectTransform;
        Button close = FindChild(panel.transform, "Btn_TutupX")?.GetComponent<Button>();
        if (box == null || close == null) throw new InvalidOperationException("Wadah pengaturan atau tombol X tidak ditemukan.");

        Undo.RegisterFullObjectHierarchyUndo(panel, "Pengaturan layar penuh");
        Stretch(panel.GetComponent<RectTransform>());
        panel.transform.SetAsLastSibling();
        Image background = panel.GetComponent<Image>();
        if (background == null) background = panel.AddComponent<Image>();
        background.color = Paper;
        background.raycastTarget = true;
        Button outside = panel.GetComponent<Button>();
        if (outside != null) UnityEngine.Object.DestroyImmediate(outside);
        Stretch(box);
        WigglyContainer wiggle = box.GetComponent<WigglyContainer>();
        if (wiggle != null) wiggle.enabled = false;
        Image boxImage = box.GetComponent<Image>();
        if (boxImage != null) { boxImage.color = Color.clear; boxImage.raycastTarget = false; }
        UnityEngine.UI.Outline boxOutline = box.GetComponent<UnityEngine.UI.Outline>();
        if (boxOutline != null) boxOutline.enabled = false;

        Label(box, "SettingsHeading", "PENGATURAN", new Vector2(0.07f, 0.85f), new Vector2(700f, 80f), HandFont, 65, Ink);
        Rule(box, "SettingsDivider", 0.70f, new Color(Ink.r, Ink.g, Ink.b, 0.28f));
        Image underline = Decoration(box, "SettingsUnderline", new Color(Orange.r, Orange.g, Orange.b, 0.75f));
        Place(underline.rectTransform, new Vector2(0.07f, 0.79f), new Vector2(235f, 7f), new Vector2(0f, 0.5f));
        underline.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -1f);

        ThemeSlider(settings.sliderVolume, box, 0.56f);
        ThemeSlider(settings.sliderCahaya, box, 0.33f);
        ThemeExistingLabel(panel.transform, box, "Judul_Volume", "VOLUME", 0.57f);
        ThemeExistingLabel(panel.transform, box, "Judul_Cahaya", "KECERAHAN", 0.34f);
        settings.teksVolume = Label(box, "VolumeValue", Mathf.RoundToInt(settings.sliderVolume.value * 100f) + "%", new Vector2(0.87f, 0.56f), new Vector2(105f, 55f), HandFont, 43, Ink);
        settings.teksCahaya = Label(box, "BrightnessValue", Mathf.RoundToInt(settings.sliderCahaya.value * 100f) + "%", new Vector2(0.87f, 0.33f), new Vector2(105f, 55f), HandFont, 43, Ink);
        Rule(box, "SettingsFooterDivider", 0.17f, new Color(Ink.r, Ink.g, Ink.b, 0.20f));
        RemoveSettingsQuote(panel);
        AddSettingsWiggle(panel);

        RectTransform closeRect = close.GetComponent<RectTransform>();
        closeRect.SetParent(box, false);
        closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(0.5f, 0.5f);
        closeRect.anchoredPosition = new Vector2(-72f, -65f);
        closeRect.sizeDelta = new Vector2(60f, 60f);
        closeRect.localScale = Vector3.one;
        Image closeImage = close.GetComponent<Image>();
        if (closeImage != null) closeImage.color = Orange;
        AddOutline(close.gameObject);
        foreach (TMP_Text text in close.GetComponentsInChildren<TMP_Text>(true)) { text.text = "X"; text.color = Ink; text.fontSize = 33; text.alignment = TextAlignmentOptions.Center; }

        if (originalFilter != null)
        {
            originalFilter.blocksRaycasts = false;
            originalFilter.interactable = false;
            Image filterImage = originalFilter.GetComponent<Image>();
            if (filterImage != null) filterImage.raycastTarget = false;
            if (originalFilter.GetComponent<FilterKecerahan>() == null) originalFilter.gameObject.AddComponent<FilterKecerahan>();
            originalFilter.transform.SetAsLastSibling();
        }

        EnsureFolder("Assets/UI");
        EnsureFolder("Assets/UI/Prefabs");
        settings.panelHitamFilter = null;
        panel.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(panel, SettingsPrefab);
        settings.panelHitamFilter = originalFilter;
        EditorUtility.SetDirty(settings);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        string result = "Main menu: pengaturan penuh layar, slider dan tombol X tetap terhubung; tap luar dinonaktifkan.";
        WriteReport("main-menu-settings.txt", result);
        return result;
    }

    public static string ConnectPhone()
    {
        RequireEditMode();
        GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(SettingsPrefab);
        if (template == null) throw new InvalidOperationException("Prefab pengaturan belum dibuat.");
        EditorSceneManager.SaveOpenScenes();
        Scene scene = EditorSceneManager.OpenScene(PhoneScene, OpenSceneMode.Single);
        SistemHP phone = Find<SistemHP>(scene);
        if (phone == null) throw new InvalidOperationException("SistemHP tidak ditemukan di CharacterLab.");
        Canvas canvas = FindNamed<Canvas>(scene, "CanvasPengaturan");
        if (canvas == null)
        {
            GameObject go = new GameObject("CanvasPengaturan", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
        }
        GameObject instance = phone.panelPengaturan;
        if (instance == null)
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(template, scene);
            instance.name = "Panel_Pengaturan";
            instance.transform.SetParent(canvas.transform, false);
        }
        Stretch(instance.GetComponent<RectTransform>());
        PengaturanManager settings = instance.GetComponent<PengaturanManager>();
        Canvas filterCanvas = FindNamed<Canvas>(scene, "CanvasKecerahan");
        if (filterCanvas == null)
        {
            GameObject go = new GameObject("CanvasKecerahan", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            filterCanvas = go.GetComponent<Canvas>();
            filterCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            filterCanvas.sortingOrder = 100;
            Image image = Decoration(go.transform, "FilterCahaya", Color.black);
            Stretch(image.rectTransform);
            image.raycastTarget = false;
            CanvasGroup group = image.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            image.gameObject.AddComponent<FilterKecerahan>();
        }
        settings.panelHitamFilter = filterCanvas.GetComponentInChildren<CanvasGroup>(true);
        phone.panelPengaturan = instance;
        Transform icon = phone.daftarIkon.FirstOrDefault(go => go != null && go.name == "IkonSetting")?.transform;
        if (icon == null) throw new InvalidOperationException("IkonSetting tidak ditemukan.");
        Button button = icon.GetComponent<Button>();
        if (button == null) button = icon.gameObject.AddComponent<Button>();
        button.targetGraphic = icon.GetComponent<Image>();
        bool alreadyWired = Enumerable.Range(0, button.onClick.GetPersistentEventCount()).Any(i => button.onClick.GetPersistentMethodName(i) == "BukaPengaturan");
        if (!alreadyWired) UnityEventTools.AddPersistentListener(button.onClick, phone.BukaPengaturan);
        instance.SetActive(false);
        EditorUtility.SetDirty(settings);
        EditorUtility.SetDirty(phone);
        EditorUtility.SetDirty(button);
        PrefabUtility.RecordPrefabInstancePropertyModifications(settings);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        string result = "CharacterLab: IkonSetting membuka pengaturan yang sama; tombol X kembali ke ponsel.";
        WriteReport("phone-settings.txt", result);
        return result;
    }

    public static string RenameMainMenu()
    {
        RequireEditMode();
        EditorSceneManager.SaveOpenScenes();
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(OldMenu) != null)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(NewMenu) != null) throw new InvalidOperationException("MainMenu sudah ada; tidak akan ditimpa.");
            string error = AssetDatabase.RenameAsset(OldMenu, "MainMenu");
            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
        }
        EditorBuildSettings.scenes = EditorBuildSettings.scenes.Select(entry => entry.path == OldMenu ? new EditorBuildSettingsScene(NewMenu, entry.enabled) : entry).ToArray();
        foreach (string path in new[] { "Assets/Scenes/IntroTeksScene.unity", "Assets/Scenes/IntroVideoScene.unity" }.Where(candidate => AssetDatabase.LoadAssetAtPath<SceneAsset>(candidate) != null))
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (GameObject go in scene.GetRootGameObjects())
            {
                foreach (IntroTitle intro in go.GetComponentsInChildren<IntroTitle>(true)) { intro.namaSceneMenu = "MainMenu"; EditorUtility.SetDirty(intro); }
                foreach (IntroVideo intro in go.GetComponentsInChildren<IntroVideo>(true)) { intro.namaSceneMenu = "MainMenu"; EditorUtility.SetDirty(intro); }
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        foreach (string path in new[] { "Assets/Scripts/Runtime/AlurDanMenu/IntroTitle.cs", "Assets/Scripts/Runtime/AlurDanMenu/IntroVideo.cs" })
        {
            string source = File.ReadAllText(Absolute(path));
            File.WriteAllText(Absolute(path), source.Replace("\"SampleScene\"", "\"MainMenu\""));
        }
        EditorSceneManager.OpenScene(NewMenu, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        WriteReport("main-menu-rename.txt", "SampleScene -> MainMenu. GUID dipertahankan; rujukan intro dan daftar build diperbarui.");
        AssetDatabase.Refresh();
        return "MainMenu dan rujukan scene sudah diperbarui.";
    }

    public static string Validate()
    {
        RequireEditMode();
        List<string> checks = new List<string>();
        Scene menuScene = EditorSceneManager.OpenScene(MenuPath(), OpenSceneMode.Single);
        TitleMenuManager menu = Find<TitleMenuManager>(menuScene);
        PengaturanManager menuSettings = menu.pengaturanPanel.GetComponent<PengaturanManager>();
        Check(menuSettings.sliderVolume != null && menuSettings.sliderCahaya != null, "Slider asli terhubung", checks);
        Check(menu.pengaturanPanel.GetComponent<Button>() == null, "Tap di luar tidak menutup pengaturan", checks);
        Check(menuSettings.panelHitamFilter != null && !menuSettings.panelHitamFilter.blocksRaycasts, "Filter cahaya tidak menghalangi kontrol", checks);
        Check(menuSettings.teksVolume != null && menuSettings.teksCahaya != null, "Indikator nilai tersedia", checks);
        Check(menuSettings.sliderVolume.onValueChanged.GetPersistentEventCount() > 0 && menuSettings.sliderCahaya.onValueChanged.GetPersistentEventCount() > 0, "Callback slider tetap terpasang", checks);
        Button close = FindChild(menu.pengaturanPanel.transform, "Btn_TutupX").GetComponent<Button>();
        Check(close.onClick.GetPersistentEventCount() > 0 && close.onClick.GetPersistentTarget(0) == menu.pengaturanPanel, "Tombol X menutup panel yang benar", checks);
        Scene phoneScene = EditorSceneManager.OpenScene(PhoneScene, OpenSceneMode.Single);
        SistemHP phone = Find<SistemHP>(phoneScene);
        Check(phone.panelPengaturan != null && phone.panelPengaturan.GetComponent<PengaturanManager>() != null, "IkonSetting terhubung dengan pengaturan", checks);
        PengaturanManager phoneSettings = phone.panelPengaturan.GetComponent<PengaturanManager>();
        Check(phoneSettings.panelHitamFilter != null && !phoneSettings.panelHitamFilter.blocksRaycasts, "Kecerahan CharacterLab terhubung", checks);
        Button phoneClose = FindChild(phone.panelPengaturan.transform, "Btn_TutupX").GetComponent<Button>();
        PengaturanGameplay gameplaySettings = phone.panelPengaturan.GetComponent<PengaturanGameplay>();
        UnityEngine.Object expectedCloseTarget = gameplaySettings != null ? (UnityEngine.Object)gameplaySettings : phone.panelPengaturan;
        Check(phoneClose.onClick.GetPersistentTarget(0) == expectedCloseTarget, "X di ponsel menutup instance yang benar", checks);
        EditorSceneManager.OpenScene(MenuPath(), OpenSceneMode.Single);
        string result = string.Join("\n", checks);
        WriteReport("settings-validation.txt", result);
        return result;
    }

    static void CaptureOriginalEditorScenes()
    {
        string checkpoint = Absolute("Checkpoints/Check point 1");
        if (!File.Exists(Path.Combine(checkpoint, "checkpoint.json"))) throw new InvalidOperationException("Check point 1 belum dibuat.");
        string flag = Path.Combine(checkpoint, "editor-state-captured.txt");
        if (File.Exists(flag)) return;
        List<string> dirty = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isDirty || string.IsNullOrEmpty(scene.path)) continue;
            string path = scene.path;
            EditorSceneManager.SaveScene(scene);
            string destination = Path.Combine(checkpoint, path);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(Absolute(path), destination, true);
            dirty.Add(path);
        }
        File.WriteAllText(flag, "Scene yang belum tersimpan ikut dicadangkan sebelum revisi:\n" + string.Join("\n", dirty));
    }

    static void ThemeSlider(Slider slider, Transform parent, float y)
    {
        RectTransform rect = slider.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.36f, y);
        rect.anchorMax = new Vector2(0.82f, y);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 42f);
        rect.localScale = Vector3.one;
        Transform track = FindChild(slider.transform, "Background");
        if (track != null)
        {
            RectTransform trackRect = track.GetComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0f, 0.5f); trackRect.anchorMax = new Vector2(1f, 0.5f);
            trackRect.anchoredPosition = Vector2.zero; trackRect.sizeDelta = new Vector2(0f, 12f);
            Image image = track.GetComponent<Image>();
            if (image != null) { image.color = new Color(0.81f, 0.79f, 0.72f, 1f); image.raycastTarget = false; }
            AddOutline(track.gameObject);
        }
        if (slider.fillRect != null)
        {
            RectTransform area = slider.fillRect.parent as RectTransform;
            area.anchorMin = new Vector2(0f, 0.5f); area.anchorMax = new Vector2(1f, 0.5f);
            area.anchoredPosition = Vector2.zero; area.sizeDelta = new Vector2(-20f, 12f);
            Image fill = slider.fillRect.GetComponent<Image>();
            if (fill != null) { fill.color = Orange; fill.raycastTarget = false; }
        }
        if (slider.handleRect != null)
        {
            RectTransform area = slider.handleRect.parent as RectTransform;
            area.anchorMin = new Vector2(0f, 0.5f); area.anchorMax = new Vector2(1f, 0.5f);
            area.anchoredPosition = Vector2.zero; area.sizeDelta = new Vector2(-28f, 40f);
            slider.handleRect.sizeDelta = new Vector2(31f, 36f);
            Image handle = slider.handleRect.GetComponent<Image>();
            if (handle != null) { handle.color = Orange; handle.raycastTarget = true; }
            AddOutline(slider.handleRect.gameObject);
        }
        slider.SetValueWithoutNotify(slider.value);
    }

    static void ThemeExistingLabel(Transform root, Transform parent, string name, string text, float y)
    {
        Transform existing = FindChild(root, name);
        TMP_Text label = existing?.GetComponent<TMP_Text>();
        if (label == null) throw new InvalidOperationException(name + " tidak ditemukan.");
        label.transform.SetParent(parent, false);
        Place(label.rectTransform, new Vector2(0.07f, y), new Vector2(320f, 65f), new Vector2(0f, 0.5f));
        label.text = text; label.color = Ink; label.fontSize = 44; label.alignment = TextAlignmentOptions.MidlineLeft; label.raycastTarget = false;
    }

    static TMP_Text Label(Transform parent, string name, string text, Vector2 anchor, Vector2 size, TMP_FontAsset font, float fontSize, Color color)
    {
        Transform child = parent.Find(name);
        GameObject go = child != null ? child.gameObject : new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
        if (label == null) label = go.AddComponent<TextMeshProUGUI>();
        Place(label.rectTransform, anchor, size, new Vector2(0f, 0.5f));
        label.text = text; label.font = font; label.fontSize = fontSize; label.color = color;
        label.alignment = TextAlignmentOptions.MidlineLeft; label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    static Image Decoration(Transform parent, string name, Color color)
    {
        Transform child = parent.Find(name);
        GameObject go = child != null ? child.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.color = color; image.raycastTarget = false;
        return image;
    }

    static void Rule(Transform parent, string name, float y, Color color)
    {
        RectTransform rect = Decoration(parent, name, color).rectTransform;
        rect.anchorMin = new Vector2(0.07f, y); rect.anchorMax = new Vector2(0.93f, y);
        rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(0f, 2f);
    }

    static void AddOutline(GameObject go)
    {
        UnityEngine.UI.Outline outline = go.GetComponent<UnityEngine.UI.Outline>();
        if (outline == null) outline = go.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Ink; outline.effectDistance = new Vector2(2f, -2f);
    }

    static void Place(RectTransform rect, Vector2 anchor, Vector2 size, Vector2 pivot)
    {
        rect.anchorMin = rect.anchorMax = anchor; rect.pivot = pivot; rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size; rect.localScale = Vector3.one;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one;
    }

    static Transform FindChild(Transform parent, string name) => parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(child => child.name == name);
    static T Find<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>(true)).FirstOrDefault();
    static T FindNamed<T>(Scene scene, string name) where T : Component => scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>(true)).FirstOrDefault(component => component.gameObject.name == name);
    static string MenuPath() => AssetDatabase.LoadAssetAtPath<SceneAsset>(NewMenu) != null ? NewMenu : OldMenu;
    static void RequireEditMode() { if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Hentikan Play mode sebelum menerapkan revisi."); }
    static void EnsureFolder(string path) { if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\', '/'), Path.GetFileName(path)); }
    static void Check(bool success, string message, List<string> checks) { if (!success) throw new InvalidOperationException(message); checks.Add("OK: " + message); }
    static string Absolute(string path) => Path.Combine(Directory.GetParent(Application.dataPath).FullName, path);
    static void WriteReport(string name, string text) { Directory.CreateDirectory(Absolute("work")); File.WriteAllText(Absolute(Path.Combine("work", name)), text); }
}
