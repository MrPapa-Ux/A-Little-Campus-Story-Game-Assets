// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menerapkan susunan/revisi yang dibuat pada checkpoint 5.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CampusCheckpoint5Revision
{
    static string Root => Directory.GetParent(Application.dataPath).FullName;

    [InitializeOnLoadMethod]
    static void Register()
    {
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }
    static void Tick()
    {
        string refresh = Path.Combine(Root, "work/checkpoint5-refresh.request");
        if (File.Exists(refresh) && !EditorApplication.isCompiling && !EditorApplication.isUpdating && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            File.Delete(refresh);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return;
        }
        string request = Path.Combine(Root, "work/checkpoint5-revision-v3.request");
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Report("WAITING: Hentikan Play untuk memasang tombol sentuh.");
            return;
        }
        File.Delete(request);
        try { Apply(); }
        catch (Exception error) { Report("ERROR: " + error); Debug.LogException(error); }
    }

    [MenuItem("Tools/A Little Campus Story/Terapkan Revisi Check point 5")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Hentikan Play mode dahulu.");
        string checkpoint = Path.Combine(Root, "Checkpoints/Check point 5");
        if (!File.Exists(Path.Combine(checkpoint, "checkpoint.json"))) throw new InvalidOperationException("Check point 5 belum tersedia.");
        string capture = Path.Combine(checkpoint, "editor-state-captured.txt");
        if (!File.Exists(capture))
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene open = SceneManager.GetSceneAt(i);
                if (!open.isDirty || string.IsNullOrEmpty(open.path)) continue;
                EditorSceneManager.SaveScene(open);
                string destination = Path.Combine(checkpoint, open.path);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(Path.Combine(Root, open.path), destination, true);
            }
            File.WriteAllText(capture, "Scene yang belum tersimpan dicadangkan sebelum memasang revisi.");
        }
        EditorSceneManager.SaveOpenScenes();
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity", OpenSceneMode.Single);
        SistemHP phone = Find<SistemHP>(scene);
        SistemPilihan dialog = Find<SistemPilihan>(scene);
        if (phone == null || dialog == null || phone.transisiAplikasi == null) throw new InvalidOperationException("Komponen ponsel/dialog/transisi tidak lengkap.");
        phone.transisiAplikasi.durasiMati = 0.22f;
        phone.transisiAplikasi.durasiFadeKeluar = 0.3f;
        EditorUtility.SetDirty(phone.transisiAplikasi);
        if (phone.layarGelap != null) { phone.layarGelap.raycastTarget = false; EditorUtility.SetDirty(phone.layarGelap); }
        LazyBarArthur bar = Find<LazyBarArthur>(scene);
        if (bar != null && bar.lazyBarUtama != null)
            foreach (UnityEngine.UI.Graphic graphic in bar.lazyBarUtama.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
            {
                graphic.raycastTarget = false;
                EditorUtility.SetDirty(graphic);
            }
        foreach (GameObject icon in phone.daftarIkon.Where(item => item != null))
        {
            UnityEngine.UI.Button button = TouchButton(icon);
            button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            UnityEventTools.AddStringPersistentListener(button.onClick, phone.BukaAplikasi, icon.name);
            EditorUtility.SetDirty(button);
        }
        for (int i = 0; i < dialog.daftarPilihanUtama.Length; i++)
        {
            if (dialog.daftarPilihanUtama[i] == null) continue;
            UnityEngine.UI.Button button = TouchButton(dialog.daftarPilihanUtama[i].gameObject);
            button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            UnityEventTools.AddIntPersistentListener(button.onClick, dialog.TekanPilihanLangsung, i);
            EditorUtility.SetDirty(button);
        }
        if (dialog.panelKeluarKhusus == null) throw new InvalidOperationException("Panel Keluar dialog tidak ditemukan.");
        foreach (TMPro.TMP_Text text in dialog.dialogManager.canvasDialog.GetComponentsInChildren<TMPro.TMP_Text>(true))
        {
            text.raycastTarget = false;
            EditorUtility.SetDirty(text);
        }
        UnityEngine.UI.Button exit = TouchButton(dialog.panelKeluarKhusus.gameObject);
        exit.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(exit.onClick, dialog.TekanKeluarLangsung);
        EditorUtility.SetDirty(exit);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        foreach (GameObject icon in phone.daftarIkon.Where(item => item != null)) VerifyButton(icon, "BukaAplikasi");
        foreach (EfekPilihanUI choice in dialog.daftarPilihanUtama.Where(item => item != null)) VerifyButton(choice.gameObject, "TekanPilihanLangsung");
        VerifyButton(dialog.panelKeluarKhusus.gameObject, "TekanKeluarLangsung");
        if (phone.layarGelap != null && phone.layarGelap.raycastTarget) throw new InvalidOperationException("Layar gelap masih menghalangi sentuhan.");
        Report("COMPLETE: TV mati 0.22 detik + fade 0.3 detik; tombol sentuh ikon dan pilihan/Keluar dialog tersambung.");
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
    }

    static UnityEngine.UI.Button TouchButton(GameObject go)
    {
        UnityEngine.UI.Image graphic = go.GetComponent<UnityEngine.UI.Image>();
        if (graphic == null) { graphic = go.AddComponent<UnityEngine.UI.Image>(); graphic.color = Color.clear; }
        graphic.raycastTarget = true;
        foreach (UnityEngine.UI.Graphic child in go.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
        {
            if (child.gameObject == go) continue;
            child.raycastTarget = false;
            EditorUtility.SetDirty(child);
        }
        UnityEngine.UI.Button button = go.GetComponent<UnityEngine.UI.Button>();
        if (button == null) button = go.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = graphic;
        button.transition = UnityEngine.UI.Selectable.Transition.None;
        button.interactable = true;
        EditorUtility.SetDirty(graphic);
        return button;
    }
    static void VerifyButton(GameObject go, string method)
    {
        UnityEngine.UI.Button button = go.GetComponent<UnityEngine.UI.Button>();
        if (button == null || button.onClick.GetPersistentEventCount() != 1 || button.onClick.GetPersistentMethodName(0) != method)
            throw new InvalidOperationException("Tombol tidak terhubung: " + go.name);
    }
    static T Find<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).FirstOrDefault();
    static void Report(string text) => File.WriteAllText(Path.Combine(Root, "work/checkpoint5-revision-state.txt"), text);
}
