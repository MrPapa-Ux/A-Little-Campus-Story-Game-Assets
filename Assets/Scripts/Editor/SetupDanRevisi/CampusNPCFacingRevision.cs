// PANDUAN PROYEK - Editor / SetupDanRevisi
// Fungsi: Menerapkan perbaikan arah hadap NPC.
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

public static class CampusNPCFacingRevision
{
    const string Session = "CampusNPCFacing.Checking";
    static IEnumerator routine;
    static readonly List<string> checks = new List<string>();
    static string Root => Directory.GetParent(Application.dataPath).FullName;
    static string Work(string name) => Path.Combine(Root, "work/" + name);
    [InitializeOnLoadMethod]
    static void Register() { EditorApplication.update -= Tick; EditorApplication.update += Tick; }
    static void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (SessionState.GetBool(Session, false))
        {
            if (!EditorApplication.isPlaying)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                SessionState.SetBool(Session, false);
                EditorSceneManager.OpenScene(SessionState.GetString(Session + ".Scene", "Assets/Scenes/MainMenu.unity"), OpenSceneMode.Single); return;
            }
            if (routine == null) routine = Test();
            try { if (!routine.MoveNext()) Finish(null); } catch (Exception error) { Finish(error); }
            return;
        }
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (File.Exists(Work("npc-facing.request")))
        {
            File.Delete(Work("npc-facing.request"));
            try { Apply(); } catch (Exception error) { File.WriteAllText(Work("npc-facing-state.txt"), "ERROR: " + error); Debug.LogException(error); }
        }
        if (File.Exists(Work("npc-facing-checks.request")) && File.Exists(Work("npc-facing-state.txt")) && File.ReadAllText(Work("npc-facing-state.txt")).StartsWith("COMPLETE"))
        {
            File.Delete(Work("npc-facing-checks.request"));
            SessionState.SetString(Session + ".Scene", SceneManager.GetActiveScene().path);
            EditorSceneManager.SaveOpenScenes(); EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity", OpenSceneMode.Single);
            SessionState.SetBool(Session, true); EditorApplication.isPlaying = true;
        }
    }
    static void Apply()
    {
        EditorSceneManager.SaveOpenScenes(); string previous = SceneManager.GetActiveScene().path;
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity", OpenSceneMode.Single);
        string backup = Work("before-npc-facing.unity"); if (!File.Exists(backup)) File.Copy(Path.Combine(Root, scene.path), backup);
        int count = 0;
        foreach (NPCBubbleTrigger npc in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<NPCBubbleTrigger>(true)))
        {
            Animator animator = npc.GetComponentInChildren<Animator>(true);
            if (animator == null || (npc.bubbleChatTransform != null && npc.bubbleChatTransform.IsChildOf(animator.transform))) throw new InvalidOperationException("Wadah visual/bubble NPC belum terpisah: " + npc.name);
            NPCMenghadapArthur facing = npc.GetComponent<NPCMenghadapArthur>(); if (facing == null) facing = npc.gameObject.AddComponent<NPCMenghadapArthur>();
            facing.wadahVisual = animator.transform; EditorUtility.SetDirty(facing); count++;
        }
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        File.WriteAllText(Work("npc-facing-state.txt"), "COMPLETE: " + count + " NPC mengikuti arah Arthur selama percakapan, putar kertas 0.24 detik; bubble berada di luar wadah yang dibalik.");
        EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
    }
    static IEnumerator Test()
    {
        checks.Clear(); Application.runInBackground = true; EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus(); yield return null; yield return null;
        DialogManager manager = UnityEngine.Object.FindFirstObjectByType<DialogManager>(); SistemPilihan dialog = manager.GetComponent<SistemPilihan>();
        KarakterController player = manager.arthurScript; Rigidbody2D body = player.GetComponent<Rigidbody2D>(); body.gravityScale = 0f;
        player.GetComponent<LazyBarArthur>().enabled = false; foreach (Collider2D c in player.GetComponentsInChildren<Collider2D>()) c.enabled = false;
        NPCMenghadapArthur npc = UnityEngine.Object.FindFirstObjectByType<NPCMenghadapArthur>();
        Transform bubble = npc.GetComponent<NPCBubbleTrigger>().bubbleChatTransform; Vector3 bubblePosition = bubble.localPosition; Vector3 bubbleScale = bubble.localScale; Vector3 original = npc.wadahVisual.localScale;
        body.position = new Vector2(npc.transform.position.x - 2f, body.position.y); Physics2D.SyncTransforms();
        player.transform.position = new Vector3(npc.transform.position.x - 2f, player.transform.position.y, player.transform.position.z);
        Physics2D.SyncTransforms();
        manager.bisaInteraksi = true; manager.idInteraksiAktif = npc.name; manager.statusTombolKeluar = true; manager.skenarioAktif = new[] { new BarisDialog { siapaYangBicara = Pembicara.Kanan_NPC, teksDialog = "Hai" } }; manager.opsiInteraksiAktif = null;
        dialog.AksiTombolInteraksi(); float until = Time.realtimeSinceStartup + 0.1f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(Mathf.Abs(npc.wadahVisual.localScale.x) < Mathf.Abs(original.x), "Putar kertas menyempitkan visual di tengah transisi");
        until = Time.realtimeSinceStartup + 0.5f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(npc.wadahVisual.localScale.x < 0f, "NPC menghadap Arthur yang berada di kiri");
        Check(Vector3.Distance(bubble.localPosition, bubblePosition) < 0.001f && Vector3.Distance(bubble.localScale, bubbleScale) < 0.001f, "Bubble tidak ikut dibalik atau berpindah");
        dialog.TekanKeluarLangsung(); until = Time.realtimeSinceStartup + 0.7f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(!manager.canvasDialog.activeInHierarchy && Vector3.Distance(npc.wadahVisual.localScale, original) < 0.001f, "NPC kembali menghadap semula setelah percakapan ditutup");
        body.position = new Vector2(npc.transform.position.x + 2f, body.position.y); Physics2D.SyncTransforms();
        player.transform.position = new Vector3(npc.transform.position.x + 2f, player.transform.position.y, player.transform.position.z);
        Physics2D.SyncTransforms();
        manager.bisaInteraksi = true; manager.idInteraksiAktif = npc.name;
        dialog.AksiTombolInteraksi(); until = Time.realtimeSinceStartup + 0.3f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(npc.wadahVisual.localScale.x > 0f, "NPC juga menghadap Arthur di sisi kanan");
    }
    static void Check(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); checks.Add("OK: " + message); }
    static void Finish(Exception error) { File.WriteAllText(Work("npc-facing-checks.txt"), (error == null ? "PASS\n" : "FAIL\n" + error + "\n") + string.Join("\n", checks)); routine = null; EditorApplication.isPlaying = false; }
}
