using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Satu kali menjalankan tahap Explorer yang disetujui pengguna; tanpa Play atau kursor.
[InitializeOnLoad]
public static class CampusExplorerSetup
{
    const string Request = "work/explorer/buat-explorer.txt";
    const string Report = "work/explorer/hasil.txt";
    const string Explorer = "Assets/Scenes/Explorer.unity";
    const string Menu = "Assets/Scenes/MainMenu.unity";

    static CampusExplorerSetup() { EditorApplication.update += Tunggu; }

    static void Tunggu()
    {
        if (AssetDatabase.IsAssetImportWorkerProcess()) return;
        if (!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= Tunggu;
        File.Delete(Request);
        Directory.CreateDirectory("work/explorer");
        try { Buat(); }
        catch (Exception e) { File.WriteAllText(Report, "BELUM SELESAI\n" + e); }
    }

    static T[] DalamScene<T>(Scene scene) where T : Component
    {
        return scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<T>(true)).ToArray();
    }

    public static void Buat()
    {
        if (File.Exists(Explorer)) throw new Exception("Explorer sudah ada. Tidak ditimpa.");
        Scene menu = SceneManager.GetSceneByPath(Menu);
        bool menuSudahTerbuka = menu.IsValid() && menu.isLoaded;
        if (menuSudahTerbuka && menu.isDirty) throw new Exception("MainMenu punya perubahan belum disimpan; simpan dahulu.");
        Scene asal = SceneManager.GetSceneByPath("Assets/Scenes/CharacterLab.unity");
        if (asal.IsValid() && asal.isLoaded && asal.isDirty) throw new Exception("CharacterLab punya perubahan belum disimpan; simpan dahulu.");
        if (!AssetDatabase.CopyAsset("Assets/Scenes/CharacterLab.unity", Explorer)) throw new Exception("Salinan CharacterLab gagal dibuat.");
        Scene dunia = EditorSceneManager.OpenScene(Explorer, OpenSceneMode.Additive);
        try
        {
            LazyBarArthur[] bars = DalamScene<LazyBarArthur>(dunia);
            if (bars.Length != 1) throw new Exception("Explorer harus memiliki tepat satu pengatur bar; ditemukan " + bars.Length);
            bars[0].tanpaBatasWaktu = true;
            EditorUtility.SetDirty(bars[0]);
            EditorSceneManager.MarkSceneDirty(dunia);
            if (!EditorSceneManager.SaveScene(dunia)) throw new Exception("Explorer gagal disimpan.");
        }
        finally { EditorSceneManager.CloseScene(dunia, true); }

        if (!menuSudahTerbuka) menu = EditorSceneManager.OpenScene(Menu, OpenSceneMode.Additive);
        try
        {
            var daftar = DalamScene<TitleMenuManager>(menu);
            if (daftar.Length != 1 || daftar[0].eksplorasiButton == null) throw new Exception("Tombol Eksplorasi tidak ditemukan dengan pasti.");
            TitleMenuManager pengatur = daftar[0];
            pengatur.namaSceneExplorer = "Explorer";
            var button = pengatur.eksplorasiButton;
            bool terhubung = false;
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                if (button.onClick.GetPersistentTarget(i) == pengatur && button.onClick.GetPersistentMethodName(i) == "KlikTombolEksplorasi")
                {
                    button.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.RuntimeOnly);
                    terhubung = true;
                }
            if (!terhubung) UnityEventTools.AddPersistentListener(button.onClick, pengatur.KlikTombolEksplorasi);
            button.interactable = true;
            if (pengatur.eksplorasiCanvasGroup != null)
            {
                pengatur.eksplorasiCanvasGroup.alpha = 1f;
                pengatur.eksplorasiCanvasGroup.interactable = true;
                pengatur.eksplorasiCanvasGroup.blocksRaycasts = true;
                EditorUtility.SetDirty(pengatur.eksplorasiCanvasGroup);
            }
            EditorUtility.SetDirty(button);
            EditorUtility.SetDirty(pengatur);
            EditorSceneManager.MarkSceneDirty(menu);
            if (!EditorSceneManager.SaveScene(menu)) throw new Exception("Menu gagal disimpan.");
        }
        finally { if (!menuSudahTerbuka) EditorSceneManager.CloseScene(menu, true); }

        var scenes = EditorBuildSettings.scenes.ToList();
        int index = scenes.FindIndex(x => x.path == Explorer);
        if (index < 0) scenes.Add(new EditorBuildSettingsScene(Explorer, true));
        else scenes[index] = new EditorBuildSettingsScene(Explorer, true);
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        File.WriteAllText(Report, "SIAP UNTUK DIUJI\nExplorer disalin dari CharacterLab.\nTanpa batas waktu; bar tetap penuh.\nEksplorasi membuka Explorer tanpa perlu tamat.\nScene Explorer terdaftar untuk build.\nCharacterLab asli tidak diubah.\n");
    }
}
