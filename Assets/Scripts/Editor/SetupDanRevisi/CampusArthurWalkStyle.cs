// PANDUAN PROYEK - Editor / SetupDanRevisi
// Fungsi: Menerapkan susunan visual/gaya jalan Arthur.
// Panduan: Assets/Scripts/Editor/SetupDanRevisi/README.md

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CampusArthurWalkStyle
{
    static string Root => Directory.GetParent(Application.dataPath).FullName;
    static string Work(string name) => Path.Combine(Root, "work/" + name);
    [InitializeOnLoadMethod]
    static void Register() { EditorApplication.update -= Tick; EditorApplication.update += Tick; }
    static void Tick()
    {
        if (!File.Exists(Work("arthur-walk-style.request")) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete(Work("arthur-walk-style.request"));
        try { Apply(); } catch (Exception error) { File.WriteAllText(Work("arthur-walk-style-state.txt"), "ERROR: " + error); Debug.LogException(error); }
    }
    [MenuItem("Tools/A Little Campus Story/Terapkan Gaya Jalan Arthur")]
    public static void Apply()
    {
        EditorSceneManager.SaveOpenScenes();
        string previous = SceneManager.GetActiveScene().path;
        EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity", OpenSceneMode.Single);
        KarakterController player = UnityEngine.Object.FindFirstObjectByType<KarakterController>();
        AnimatorController controller = player.animator.runtimeAnimatorController as AnimatorController;
        string backup = Work("before-arthur-walk-style.controller");
        if (!File.Exists(backup)) File.Copy(Path.Combine(Root, AssetDatabase.GetAssetPath(controller)), backup);
        AnimationClip source = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/CharacterWalk.anim");
        AnimationClip idle = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Referensi/Rayman_Reference/ArthurIdle.anim");
        const string path = "Assets/Animations/ArthurPersonalWalk.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null) { clip = UnityEngine.Object.Instantiate(source); clip.name = "ArthurPersonalWalk"; AssetDatabase.CreateAsset(clip, path); }
        else EditorUtility.CopySerialized(source, clip);
        clip.name = "ArthurPersonalWalk";
        string[] upper = { "Badan", "TanganKiri", "TanganKanan", "KepalaArthur", "Rambut 1", "Telinga" };
        string[] head = { "KepalaArthur", "Rambut 1", "Telinga" };
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(idle))
        {
            if (!upper.Contains(binding.path)) continue;
            float neutral = AnimationUtility.GetEditorCurve(idle, binding).Evaluate(0f);
            var keys = new Keyframe[41];
            for (int i = 0; i < keys.Length; i++)
            {
                float t = source.length * i / (keys.Length - 1), phase = t / source.length * Mathf.PI * 2f;
                float bob = 0.06f * (1f - Mathf.Cos(phase * 2f));
                float value = neutral;
                if (binding.propertyName == "m_LocalPosition.y") value += bob;
                if (head.Contains(binding.path) && binding.propertyName == "m_LocalPosition.x") value += Mathf.Sin(phase) * 0.025f;
                if (binding.propertyName.StartsWith("localEulerAngles") && binding.propertyName.EndsWith(".z"))
                {
                    if (binding.path == "TanganKiri") value += Mathf.Sin(phase) * 7f + Mathf.Sin(phase * 2f) * 2f;
                    else if (binding.path == "TanganKanan") value -= Mathf.Sin(phase) * 9f + Mathf.Sin(phase * 2f) * 1.5f;
                    else if (head.Contains(binding.path)) value += Mathf.Sin(phase) * 1.5f;
                    else if (binding.path == "Badan") value += Mathf.Sin(phase * 2f) * 0.75f;
                }
                keys[i] = new Keyframe(t, value);
            }
            AnimationCurve curve = new AnimationCurve(keys);
            for (int i = 0; i < keys.Length; i++) AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            for (int i = 0; i < keys.Length; i++) AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }
        AnimatorState state = controller.layers[0].stateMachine.states.Select(child => child.state).First(item => item.name == "CampusWalk");
        state.motion = clip;
        EditorUtility.SetDirty(state); EditorUtility.SetDirty(clip); EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AnimationCurve bobCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("Badan", typeof(Transform), "m_LocalPosition.y"));
        if (bobCurve.keys.Max(key => key.value) - bobCurve.keys.Min(key => key.value) < 0.1f) throw new InvalidOperationException("Naik-turun badan belum terpasang.");
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(source).Where(item => item.path == "KakiKiri" || item.path == "KakiKanan"))
        {
            AnimationCurve a = AnimationUtility.GetEditorCurve(source, binding), b = AnimationUtility.GetEditorCurve(clip, binding);
            if (a.keys.Where((key, i) => Mathf.Abs(key.value - b.keys[i].value) > 0.0001f).Any()) throw new InvalidOperationException("Pijakan kaki berubah.");
        }
        File.WriteAllText(Work("arthur-walk-style-state.txt"), "COMPLETE: Clip khusus Arthur dengan bob dua kali per siklus, kepala mengikuti dan ayunan tangan asimetris. Kurva kaki serta pengaturan kecepatan/kontrol dipertahankan.");
        EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
    }
}
