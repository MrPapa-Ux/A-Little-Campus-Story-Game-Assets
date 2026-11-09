// PANDUAN PROYEK - Editor / SetupDanRevisi
// Fungsi: Menerapkan sambungan jalan dan lari.
// Panduan: Assets/Scripts/Editor/SetupDanRevisi/README.md

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public static class CampusWalkRevision
{
    const string WalkPath = "Assets/Animations/CharacterWalk.anim";
    const string Session = "CampusWalkRevision.Checking";
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
                EditorSceneManager.OpenScene(SessionState.GetString(Session + ".Scene", "Assets/Scenes/MainMenu.unity"), OpenSceneMode.Single);
                return;
            }
            if (routine == null) routine = Test();
            try { if (!routine.MoveNext()) Finish(null); } catch (Exception error) { Finish(error); }
            return;
        }
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (File.Exists(Work("walk-revision-v4.request")))
        {
            File.Delete(Work("walk-revision-v4.request"));
            try { Apply(); } catch (Exception error) { File.WriteAllText(Work("walk-revision-state.txt"), "ERROR: " + error); Debug.LogException(error); }
        }
        if (File.Exists(Work("walk-checks.request")) && File.Exists(Work("walk-revision-state.txt")) && File.ReadAllText(Work("walk-revision-state.txt")).StartsWith("COMPLETE"))
        {
            File.Delete(Work("walk-checks.request"));
            SessionState.SetString(Session + ".Scene", SceneManager.GetActiveScene().path);
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity", OpenSceneMode.Single);
            SessionState.SetBool(Session, true);
            EditorApplication.isPlaying = true;
        }
    }

    [MenuItem("Tools/A Little Campus Story/Terapkan Jalan dan Lari")]
    public static void Apply()
    {
        EditorSceneManager.SaveOpenScenes();
        string previous = SceneManager.GetActiveScene().path;
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity", OpenSceneMode.Single);
        Backup("Assets/Scenes/CharacterLab.unity");
        KarakterController player = UnityEngine.Object.FindFirstObjectByType<KarakterController>();
        AnimatorController controller = player.animator.runtimeAnimatorController as AnimatorController;
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState run = machine.states.Select(item => item.state).First(item => item.name == "ArthurWalk");
        AnimationClip runClip = run.motion as AnimationClip;
        if (runClip == null) throw new InvalidOperationException("Animasi lari yang ada tidak ditemukan.");
        if (!AssetDatabase.IsValidFolder("Assets/Animations")) AssetDatabase.CreateFolder("Assets", "Animations");
        AnimationClip walk = AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkPath);
        if (walk == null)
        {
            walk = new AnimationClip { name = "CharacterWalk", frameRate = runClip.frameRate };
            foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(runClip))
            {
                AnimationCurve source = AnimationUtility.GetEditorCurve(runClip, binding);
                float center = (source.keys.Min(key => key.value) + source.keys.Max(key => key.value)) * 0.5f;
                bool soften = binding.propertyName.StartsWith("localEulerAngles") || binding.propertyName.StartsWith("m_LocalPosition");
                float amplitude = soften ? 0.55f : 1f;
                Keyframe[] keys = source.keys.Select(key =>
                {
                    key.time *= 2f;
                    key.value = center + (key.value - center) * amplitude;
                    key.inTangent *= amplitude * 0.5f;
                    key.outTangent *= amplitude * 0.5f;
                    return key;
                }).ToArray();
                AnimationUtility.SetEditorCurve(walk, binding, new AnimationCurve(keys) { preWrapMode = source.preWrapMode, postWrapMode = source.postWrapMode });
            }
            foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(runClip))
                AnimationUtility.SetObjectReferenceCurve(walk, binding, AnimationUtility.GetObjectReferenceCurve(runClip, binding).Select(key => { key.time *= 2f; return key; }).ToArray());
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(runClip);
            settings.startTime *= 2f; settings.stopTime *= 2f; settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(walk, settings);
            AssetDatabase.CreateAsset(walk, WalkPath);
        }
        var configured = new HashSet<AnimatorController>();
        foreach (Animator animator in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Animator>(true)))
        {
            AnimatorController target = animator.runtimeAnimatorController as AnimatorController;
            if (target == null || !target.parameters.Any(parameter => parameter.name == "isJalan")) continue;
            if (animator.transform.Find("TanganKiri") == null)
            {
                Transform leftHand = animator.GetComponentsInChildren<Transform>(true).FirstOrDefault(part => part.name == "TanganKiri");
                if (leftHand != null) leftHand.SetParent(animator.transform, true);
            }
            if (!configured.Add(target)) continue;
            Backup(AssetDatabase.GetAssetPath(target));
            SetupController(target, walk, runClip);
        }
        player.pengaliKecepatanJalan = 0.5f;
        player.jedaKetukGanda = 0.3f;
        EditorUtility.SetDirty(player);
        WireButton(scene, "Btn_Kiri", player.JalanKeKiri, player.LepasKiri);
        WireButton(scene, "Btn_Kanan", player.JalanKeKanan, player.LepasKanan);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        File.WriteAllText(Work("walk-revision-state.txt"), "COMPLETE: Kecepatan jalan 50%; dua ketukan arah sama lalu tahan untuk lari; clip jalan dan " + configured.Count + " Animator controller terhubung.");
        EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
    }

    static void SetupController(AnimatorController controller, AnimationClip walk, AnimationClip runClip)
    {
        if (!controller.parameters.Any(parameter => parameter.name == "isLari")) controller.AddParameter("isLari", AnimatorControllerParameterType.Bool);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idle = machine.defaultState;
        AnimatorState run = machine.states.Select(item => item.state).FirstOrDefault(state => state.name == "ArthurWalk") ?? machine.AddState("ArthurWalk");
        run.motion = runClip;
        AnimatorState walking = machine.states.Select(item => item.state).FirstOrDefault(state => state.name == "CampusWalk") ?? machine.AddState("CampusWalk", new Vector3(540f, 180f));
        walking.motion = walk;
        // Hanya ubah transisi gerak di antara tiga state ini.
        foreach (AnimatorState state in new[] { idle, run, walking })
            foreach (AnimatorStateTransition transition in state.transitions.ToArray())
                if (transition.destinationState == idle || transition.destinationState == run || transition.destinationState == walking) state.RemoveTransition(transition);
        Transition(idle, walking, true, false);
        Transition(idle, run, true, true);
        Transition(walking, run, true, true);
        Transition(run, walking, true, false);
        Transition(walking, idle, false, null);
        Transition(run, idle, false, null);
        EditorUtility.SetDirty(controller);
    }
    static void Transition(AnimatorState from, AnimatorState to, bool moving, bool? running)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false; transition.hasFixedDuration = true; transition.duration = 0.08f;
        transition.interruptionSource = TransitionInterruptionSource.SourceThenDestination;
        transition.AddCondition(moving ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, "isJalan");
        if (running.HasValue) transition.AddCondition(running.Value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, "isLari");
    }
    static void WireButton(Scene scene, string name, UnityEngine.Events.UnityAction down, UnityEngine.Events.UnityAction up)
    {
        Transform target = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).First(item => item.name == name);
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null) trigger = target.gameObject.AddComponent<EventTrigger>();
        foreach (EventTriggerType type in new[] { EventTriggerType.PointerDown, EventTriggerType.PointerUp, EventTriggerType.PointerExit })
        {
            trigger.triggers.RemoveAll(entry => entry.eventID == type);
            var entry = new EventTrigger.Entry { eventID = type };
            UnityEventTools.AddVoidPersistentListener(entry.callback, type == EventTriggerType.PointerDown ? down : up);
            trigger.triggers.Add(entry);
        }
        EditorUtility.SetDirty(trigger);
    }
    static void Backup(string path)
    {
        string target = Work("before-walk/" + path);
        if (File.Exists(target)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(target));
        File.Copy(Path.Combine(Root, path), target);
    }

    static IEnumerator Test()
    {
        checks.Clear();
        Application.runInBackground = true;
        EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
        yield return null; yield return null;
        KarakterController player = UnityEngine.Object.FindFirstObjectByType<KarakterController>();
        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        LazyBarArthur bar = player.GetComponent<LazyBarArthur>(); if (bar != null) bar.enabled = false;
        foreach (Collider2D collider in player.GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        body.gravityScale = 0f;
        GameObject left = GameObject.Find("Btn_Kiri");
        GameObject right = GameObject.Find("Btn_Kanan");
        Down(right); float until = Time.realtimeSinceStartup + 0.15f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(!player.SedangBerlari && Mathf.Abs(body.linearVelocity.x - player.kecepatanJalan * 0.5f) < 0.01f, "Sekali tahan berjalan dengan setengah kecepatan");
        Check(player.animator.GetCurrentAnimatorStateInfo(0).IsName("CampusWalk"), "Animasi jalan dipakai saat berjalan");
        Up(right); player.BerhentiBergerak();
        Down(left); yield return null; Up(left); yield return null; Down(left);
        until = Time.realtimeSinceStartup + 0.25f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(player.SedangBerlari && Mathf.Abs(body.linearVelocity.x + player.kecepatanJalan) < 0.01f, "Dua ketukan kiri lalu tahan berlari dengan kecepatan lama");
        Check(player.animator.GetCurrentAnimatorStateInfo(0).IsName("ArthurWalk"), "Animasi lari lama dipertahankan");
        Down(right); Up(left);
        until = Time.realtimeSinceStartup + 0.15f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(!player.SedangBerlari && body.linearVelocity.x > 0f, "Berganti arah berjalan; melepas tombol lama tidak menghentikan arah baru");
        Up(right); player.BerhentiBergerak();
        Down(right); yield return null; Up(right);
        until = Time.realtimeSinceStartup + 0.4f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Down(right);
        Check(!player.SedangBerlari, "Ketukan yang terlalu jauh waktunya tidak memicu lari");
        player.kunciPergerakan = true;
        until = Time.realtimeSinceStartup + 0.12f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(body.linearVelocity.sqrMagnitude < 0.001f && !player.SedangBerlari && !player.animator.GetBool("isJalan"), "Kunci dialog/ponsel menghentikan gerakan dan animasi");
        player.kunciPergerakan = false;
        Down(right); yield return null; Up(right); yield return null; Down(right);
        Check(player.SedangBerlari, "Dua ketukan kanan juga memicu lari");
        Up(right);
        Check(!player.SedangBerlari && !player.animator.GetBool("isJalan"), "Melepas tombol mengakhiri lari");
        Check(AssetDatabase.LoadAssetAtPath<AnimationClip>(WalkPath).length > 0f, "Asset animasi jalan tersimpan");
        foreach (Animator npc in UnityEngine.Object.FindObjectsByType<Animator>(FindObjectsSortMode.None).Where(item => item != player.animator && item.parameters.Any(parameter => parameter.name == "isLari")))
        {
            Check(new[] { "TanganKiri", "TanganKanan", "KakiKiri", "KakiKanan", "Badan" }.All(path => npc.transform.Find(path) != null), npc.name + ": bagian tubuh untuk animasi jalan tersedia");
            npc.SetBool("isJalan", true); npc.SetBool("isLari", false);
            npc.Update(0.01f); npc.Update(0.2f);
            Check(npc.GetCurrentAnimatorStateInfo(0).IsName("CampusWalk"), npc.name + ": Animator NPC bisa memainkan animasi jalan");
        }
    }
    static void Down(GameObject button) => ExecuteEvents.Execute(button, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);
    static void Up(GameObject button) => ExecuteEvents.Execute(button, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
    static void Check(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); checks.Add("OK: " + message); }
    static void Finish(Exception error)
    {
        File.WriteAllText(Work("walk-runtime-checks.txt"), (error == null ? "PASS\n" : "FAIL\n" + error + "\n") + string.Join("\n", checks));
        routine = null; EditorApplication.isPlaying = false;
    }
}
