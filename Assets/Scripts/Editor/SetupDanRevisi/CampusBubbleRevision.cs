// PANDUAN PROYEK - Editor / SetupDanRevisi
// Fungsi: Menerapkan revisi bubble area interaksi.
// Panduan: Assets/Scripts/Editor/SetupDanRevisi/README.md

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CampusBubbleRevision
{
    const string Session = "CampusBubbleRevision.Testing";
    static IEnumerator routine;
    static readonly List<string> checks = new List<string>();
    static int inactiveCoroutineErrors;
    static void CaptureLog(string message, string stack, LogType type)
    {
        if (message.Contains("Coroutine couldn't be started")) inactiveCoroutineErrors++;
    }
    static string Root => Directory.GetParent(Application.dataPath).FullName;
    static string FilePath(string name) => Path.Combine(Root, "work/" + name);

    [InitializeOnLoadMethod]
    static void Register()
    {
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }
    static void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (SessionState.GetBool(Session, false))
        {
            if (!EditorApplication.isPlaying)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                SessionState.SetBool(Session, false);
                string previous = SessionState.GetString(Session + ".Scene", "Assets/Scenes/MainMenu.unity");
                EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
                return;
            }
            if (routine == null) routine = RunChecks();
            try { if (!routine.MoveNext()) Finish(null); }
            catch (Exception error) { Finish(error); }
            return;
        }
        if (File.Exists(FilePath("bubble-fix.request")))
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) { Report("WAITING: Hentikan Play Mode untuk memperbaiki referensi bubble."); return; }
            File.Delete(FilePath("bubble-fix.request"));
            try { Apply(); }
            catch (Exception error) { Report("ERROR: " + error); Debug.LogException(error); }
        }
        if (File.Exists(FilePath("bubble-checks.request")) && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(FilePath("bubble-fix-state.txt")) && File.ReadAllText(FilePath("bubble-fix-state.txt")).StartsWith("COMPLETE"))
        {
            File.Delete(FilePath("bubble-checks.request"));
            SessionState.SetString(Session + ".Scene", SceneManager.GetActiveScene().path);
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity", OpenSceneMode.Single);
            SessionState.SetBool(Session, true);
            EditorApplication.isPlaying = true;
        }
    }

    [MenuItem("Tools/A Little Campus Story/Perbaiki Bubble Area Interaksi")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Hentikan Play Mode dahulu.");
        EditorSceneManager.SaveOpenScenes();
        string path = "Assets/Scenes/CharacterLab.unity";
        string previous = SceneManager.GetActiveScene().path;
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        string backup = FilePath("before-bubble-fix/CharacterLab-editor.unity");
        if (!File.Exists(backup)) File.Copy(Path.Combine(Root, path), backup);
        int count = 0;
        foreach (TempatBubbleTrigger bubble in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<TempatBubbleTrigger>(true)))
        {
            if (bubble.ikonTransform == bubble.transform)
            {
                Transform image = bubble.transform.Find("IkonGambar");
                if (image == null) throw new InvalidOperationException("IkonGambar tidak ditemukan: " + bubble.name);
                bubble.ukuranMaksimal = Vector3.Scale(bubble.ukuranMaksimal, image.localScale);
                bubble.ikonTransform = image;
                EditorUtility.SetDirty(bubble);
                count++;
            }
            if (bubble.ikonTransform == null || bubble.ikonTransform == bubble.transform)
                throw new InvalidOperationException("Bubble masih menunjuk ke collider: " + bubble.name);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Report("COMPLETE: " + count + " referensi bubble dipisahkan dari transform area; ukuran visual dipertahankan.");
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
    }

    static IEnumerator RunChecks()
    {
        checks.Clear();
        inactiveCoroutineErrors = 0;
        Application.logMessageReceived -= CaptureLog;
        Application.logMessageReceived += CaptureLog;
        Application.runInBackground = true;
        EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
        yield return null;
        yield return null;
        LazyBarArthur bar = UnityEngine.Object.FindFirstObjectByType<LazyBarArthur>();
        if (bar != null) bar.enabled = false;
        DialogManager manager = UnityEngine.Object.FindFirstObjectByType<DialogManager>();
        KarakterController player = manager.arthurScript;
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        player.BerhentiBergerak();
        player.enabled = false;
        body.gravityScale = 0f;
        body.linearVelocity = Vector2.zero;
        TempatBubbleTrigger[] bubbles = UnityEngine.Object.FindObjectsByType<TempatBubbleTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Check(bubbles.Length == 7 && bubbles.All(bubble => bubble.ikonTransform != null && bubble.ikonTransform != bubble.transform), "Tujuh bubble memakai gambar terpisah dari area trigger");
        TempatBubbleTrigger bubble = bubbles.First(item => item.gameObject.activeInHierarchy && item.GetComponent<Collider2D>() != null);
        Collider2D zone = bubble.GetComponent<Collider2D>();
        Vector3 initialScale = bubble.transform.localScale;
        Vector3 initialPosition = bubble.transform.position;
        Vector3 far = new Vector3(-500f, -500f, player.transform.position.z);
        player.transform.position = far;
        body.position = far;
        Physics2D.SyncTransforms();
        float until = Time.realtimeSinceStartup + 0.7f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(bubble.ikonTransform.localScale.sqrMagnitude < 0.00001f && !manager.bisaInteraksi, "Di luar area: bubble tersembunyi dan interaksi tidak aktif");
        body.position = zone.bounds.center;
        player.transform.position = new Vector3(body.position.x, body.position.y, far.z);
        Physics2D.SyncTransforms();
        until = Time.realtimeSinceStartup + bubble.durasiAnimasi + 0.3f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(bubble.ikonTransform.localScale.sqrMagnitude > 0.00001f && manager.bisaInteraksi, "Masuk area: bubble dan interaksi muncul");
        Check(Vector3.Distance(bubble.transform.position, initialPosition) < 0.00001f && Vector3.Distance(bubble.transform.localScale, initialScale) < 0.00001f, "Animasi tidak mengubah posisi atau ukuran collider");
        bubble.gameObject.SetActive(false);
        Check(bubble.ikonTransform.localScale.sqrMagnitude < 0.00001f, "Menonaktifkan area langsung membersihkan bubble");
        body.position = far;
        player.transform.position = far;
        Physics2D.SyncTransforms();
        bubble.gameObject.SetActive(true);
        until = Time.realtimeSinceStartup + 0.7f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(bubble.ikonTransform.localScale.sqrMagnitude < 0.00001f, "Aktif kembali setelah teleport: bubble tidak bocor di luar area");
        body.position = zone.bounds.center;
        player.transform.position = new Vector3(body.position.x, body.position.y, far.z);
        Physics2D.SyncTransforms();
        until = Time.realtimeSinceStartup + bubble.durasiAnimasi + 0.3f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(bubble.ikonTransform.localScale.sqrMagnitude > 0.00001f, "Bubble tetap berfungsi setelah area diaktifkan kembali");
        body.position = far;
        player.transform.position = far;
        Physics2D.SyncTransforms();
        until = Time.realtimeSinceStartup + bubble.durasiAnimasi + 0.3f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(bubble.ikonTransform.localScale.sqrMagnitude < 0.00001f && !manager.bisaInteraksi, "Keluar area: bubble hilang dan interaksi berhenti");
        foreach (TempatBubbleTrigger target in bubbles)
        {
            GameObject group = target.transform.parent.gameObject;
            for (Transform ancestor = target.transform.parent; ancestor != null; ancestor = ancestor.parent)
                ancestor.gameObject.SetActive(true);
            target.gameObject.SetActive(true);
            Check(target.isActiveAndEnabled, target.name + ": pemicu aktif sebelum pengujian grup");
            Collider2D collider = target.GetComponent<Collider2D>();
            Physics2D.SyncTransforms();
            body.position = collider.bounds.center;
            player.transform.position = new Vector3(body.position.x, body.position.y, far.z);
            Physics2D.SyncTransforms();
            until = Time.realtimeSinceStartup + 0.12f;
            while (Time.realtimeSinceStartup < until) yield return null;
            group.SetActive(false);
            Check(target.ikonTransform.localScale.sqrMagnitude < 0.00001f, target.name + ": pematian grup membersihkan ikon");
            body.position = far;
            player.transform.position = far;
            Physics2D.SyncTransforms();
            group.SetActive(true);
            until = Time.realtimeSinceStartup + 0.12f;
            while (Time.realtimeSinceStartup < until) yield return null;
        }
        Check(inactiveCoroutineErrors == 0, "Tujuh pemicu: tidak ada coroutine dijalankan saat grup tidak aktif");
    }

    static void Check(bool ok, string text) { if (!ok) throw new InvalidOperationException(text); checks.Add("OK: " + text); }
    static void Finish(Exception error)
    {
        Application.logMessageReceived -= CaptureLog;
        File.WriteAllText(FilePath("bubble-runtime-checks.txt"), (error == null ? "PASS\n" : "FAIL\n" + error + "\n") + string.Join("\n", checks));
        routine = null;
        EditorApplication.isPlaying = false;
    }
    static void Report(string text) => File.WriteAllText(FilePath("bubble-fix-state.txt"), text);
}
