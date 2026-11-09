// PANDUAN PROYEK - Editor / SetupDanRevisi
// Fungsi: Menerapkan susunan jalan dan ekspresi wajah karakter.
// Panduan: Assets/Scripts/Editor/SetupDanRevisi/README.md

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CampusCharacterLifeRevision
{
    const string Session = "CampusCharacterLife.Checking";
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
        if (File.Exists(Work("character-life-v3.request")))
        {
            File.Delete(Work("character-life-v3.request"));
            try { Apply(); } catch (Exception error) { File.WriteAllText(Work("character-life-state.txt"), "ERROR: " + error); Debug.LogException(error); }
        }
        if (File.Exists(Work("character-life-checks.request")) && File.Exists(Work("character-life-state.txt")) && File.ReadAllText(Work("character-life-state.txt")).StartsWith("COMPLETE"))
        {
            File.Delete(Work("character-life-checks.request"));
            SessionState.SetString(Session + ".Scene", SceneManager.GetActiveScene().path);
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity", OpenSceneMode.Single);
            SessionState.SetBool(Session, true); EditorApplication.isPlaying = true;
        }
    }
    [MenuItem("Tools/A Little Campus Story/Terapkan Jalan Baru dan Ekspresi Wajah")]
    public static void Apply()
    {
        EditorSceneManager.SaveOpenScenes();
        string previous = SceneManager.GetActiveScene().path;
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity", OpenSceneMode.Single);
        Backup("Assets/Scenes/CharacterLab.unity");
        Backup("Assets/Animations/CharacterWalk.anim");
        AnimationClip idle = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Referensi/Rayman_Reference/ArthurIdle.anim");
        AnimationClip walk = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/CharacterWalk.anim");
        walk.ClearCurves(); walk.frameRate = 12f;
        const float period = 0.68f;
        const float swingFraction = 0.45f;
        KarakterController player = UnityEngine.Object.FindFirstObjectByType<KarakterController>();
        float strideDistance = player.kecepatanJalan * player.pengaliKecepatanJalan * period * (1f - swingFraction) / Mathf.Max(0.01f, Mathf.Abs(player.wadahVisual.lossyScale.x));
        float hipCenter = (AnimationUtility.GetEditorCurve(idle, EditorCurveBinding.FloatCurve("KakiKiri", typeof(Transform), "m_LocalPosition.x")).Evaluate(0f) + AnimationUtility.GetEditorCurve(idle, EditorCurveBinding.FloatCurve("KakiKanan", typeof(Transform), "m_LocalPosition.x")).Evaluate(0f)) * 0.5f;
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(idle))
        {
            float neutral = AnimationUtility.GetEditorCurve(idle, binding).Evaluate(0f);
            var keys = new Keyframe[41];
            for (int i = 0; i < keys.Length; i++)
            {
                float t = i * period / (keys.Length - 1), phase = t / period * Mathf.PI * 2f;
                bool left = binding.path == "KakiKiri", right = binding.path == "KakiKanan";
                float step = Mathf.Repeat(t / period + (right ? 0.5f : 0f), 1f);
                bool swinging = step < swingFraction;
                float progress = swinging ? step / swingFraction : (step - swingFraction) / (1f - swingFraction);
                float stride = swinging ? Mathf.SmoothStep(-0.5f, 0.5f, progress) : Mathf.Lerp(0.5f, -0.5f, progress);
                float value = neutral;
                if ((left || right) && binding.propertyName == "m_LocalPosition.x") value = hipCenter + (left ? -0.1f : 0.1f) + stride * strideDistance;
                if ((left || right) && binding.propertyName == "m_LocalPosition.y") value += swinging ? Mathf.Pow(Mathf.Sin(progress * Mathf.PI), 1.5f) * 0.30f : 0f;
                // Sepatu tetap mendatar; gerakan berasal dari langkah, bukan rotasi ujung kaki.
                if (binding.path == "Badan" && binding.propertyName == "m_LocalPosition.y") value -= Mathf.Cos(phase * 2f) * 0.04f;
                if ((binding.path == "KepalaArthur" || binding.path == "Rambut 1" || binding.path == "Telinga") && binding.propertyName == "m_LocalPosition.y") value -= Mathf.Cos(phase * 2f) * 0.026f;
                if ((binding.path == "TanganKiri" || binding.path == "TanganKanan") && binding.propertyName.StartsWith("localEulerAngles") && binding.propertyName.EndsWith(".z"))
                    value += Mathf.Sin(phase + (binding.path == "TanganKanan" ? Mathf.PI : 0f)) * 11f;
                keys[i] = new Keyframe(t, value);
            }
            var curve = new AnimationCurve(keys);
            for (int i = 0; i < keys.Length; i++) { AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear); AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear); }
            AnimationUtility.SetEditorCurve(walk, binding, curve);
        }
        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(walk);
        clipSettings.startTime = 0f; clipSettings.stopTime = period; clipSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(walk, clipSettings);
        EditorUtility.SetDirty(walk);
        int count = 0;
        foreach (SpriteRenderer head in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<SpriteRenderer>(true)).Where(item => item.name == "KepalaArthur"))
        {
            EkspresiWajah2D face = head.GetComponent<EkspresiWajah2D>();
            if (face == null) face = head.gameObject.AddComponent<EkspresiWajah2D>();
            face.kepala = head;
            if (head.sprite.name.EndsWith("_2"))
            {
                face.mataKiri = new Vector2(-0.18f, -0.065f); face.mataKanan = new Vector2(0.27f, -0.015f);
                face.posisiMulut = new Vector2(0.11f, -0.37f);
                face.ukuranMaskMulut = new Vector2(0.14f, 0.055f);
                face.maskMataKiri = new Vector4(-0.36f, -0.145f, 0.01f, 0.055f);
                face.maskMataKanan = new Vector4(0.18f, -0.135f, 0.42f, 0.065f);
            }
            EditorUtility.SetDirty(face); count++;
        }
        if (count != 2) throw new InvalidOperationException("Diharapkan dua kepala karakter, ditemukan " + count);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        File.WriteAllText(Work("character-life-state.txt"), "COMPLETE: Jalan 0.68 detik dengan fase menapak/ayun, kecepatan kaki menapak mengikuti gerakan karakter; wajah tetap terhubung.");
        EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
    }
    static void Backup(string path)
    {
        string target = Work("before-character-life/" + path); if (File.Exists(target)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(target)); File.Copy(Path.Combine(Root, path), target);
    }
    static IEnumerator Test()
    {
        checks.Clear(); Application.runInBackground = true;
        EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
        yield return null; yield return null;
        KarakterController player = UnityEngine.Object.FindFirstObjectByType<KarakterController>();
        player.GetComponent<Rigidbody2D>().gravityScale = 0f;
        player.GetComponent<LazyBarArthur>().enabled = false;
        foreach (Collider2D collider in player.GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        EkspresiWajah2D[] faces = UnityEngine.Object.FindObjectsByType<EkspresiWajah2D>(FindObjectsSortMode.None);
        Check(faces.Length == 2, "Ekspresi Arthur dan NPC terpasang");
        foreach (EkspresiWajah2D face in faces) face.PaksaKedip();
        Check(faces.All(face => face.SedangKedip), "Arthur dan NPC bisa menutup mata");
        float until = Time.realtimeSinceStartup + 0.25f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(faces.All(face => !face.SedangKedip), "Mata terbuka kembali setelah kedip");
        EkspresiWajah2D arthur = player.GetComponentInChildren<EkspresiWajah2D>();
        arthur.UcapkanHuruf();
        Check(arthur.MulutTerbuka && faces.Where(face => face != arthur).All(face => !face.MulutTerbuka), "Mulut bergerak hanya pada pembicara aktif");
        until = Time.realtimeSinceStartup + 0.2f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(!arthur.MulutTerbuka, "Mulut berhenti ketika tidak ada huruf baru");
        player.JalanKeKanan();
        until = Time.realtimeSinceStartup + 0.2f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(player.animator.GetCurrentAnimatorStateInfo(0).IsName("CampusWalk"), "Kontrol satu tekan memakai animasi jalan baru");
        AnimationClip walk = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/CharacterWalk.anim");
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(walk).Where(binding => (binding.path == "KakiKiri" || binding.path == "KakiKanan") && binding.propertyName.StartsWith("localEulerAngles")))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(walk, binding);
            Check(curve.keys.Max(key => key.value) - curve.keys.Min(key => key.value) < 0.01f, binding.path + ": rotasi sepatu tidak membuat jinjit");
        }
        player.BerhentiBergerak();
        until = Time.realtimeSinceStartup + 0.15f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Preview(player.animator.transform, "character-life-normal.png");
        player.animator.Play("CampusWalk", 0, 0.125f); player.animator.Update(0f);
        Preview(player.animator.transform, "character-walk-pose.png");
        player.animator.Play("ArthurIdle", 0, 0f); player.animator.Update(0f);
        arthur.PaksaKedip(); arthur.UcapkanHuruf();
        Preview(arthur.transform, "character-life-expression.png");
        EkspresiWajah2D npcFace = faces.First(face => face != arthur);
        npcFace.PaksaKedip(); npcFace.UcapkanHuruf();
        Preview(npcFace.transform, "npc-life-expression.png");
        foreach (EkspresiWajah2D face in faces) face.HentikanBicara();
        DialogManager manager = UnityEngine.Object.FindFirstObjectByType<DialogManager>();
        SistemPilihan dialog = UnityEngine.Object.FindFirstObjectByType<SistemPilihan>();
        manager.bisaInteraksi = true; manager.idInteraksiAktif = "NPC_Lyra";
        manager.skenarioAktif = new[] { new BarisDialog { siapaYangBicara = Pembicara.Kiri_Arthur, teksDialog = "Halo teman", teksTombolLanjut = "Lanjut" }, new BarisDialog { siapaYangBicara = Pembicara.Kanan_NPC, teksDialog = "Halo Arthur" } };
        dialog.AksiTombolInteraksi();
        Check(arthur.MulutTerbuka && !npcFace.MulutTerbuka, "Dialog Arthur memicu mulut Arthur saja");
        until = Time.realtimeSinceStartup + 2f;
        while (Time.realtimeSinceStartup < until) yield return null;
        Check(!arthur.MulutTerbuka, "Mulut berhenti setelah kalimat selesai");
        dialog.AksiTombolInteraksi();
        Check(!arthur.MulutTerbuka && npcFace.MulutTerbuka, "Giliran NPC memicu mulut NPC saja");
        Check(Mathf.Abs(walk.length - 0.68f) < 0.02f, "Siklus jalan berdurasi 0.68 detik");
        foreach (string foot in new[] { "KakiKiri", "KakiKanan" })
        {
            AnimationCurve x = AnimationUtility.GetEditorCurve(walk, EditorCurveBinding.FloatCurve(foot, typeof(Transform), "m_LocalPosition.x"));
            AnimationCurve y = AnimationUtility.GetEditorCurve(walk, EditorCurveBinding.FloatCurve(foot, typeof(Transform), "m_LocalPosition.y"));
            Check(y.keys.Max(key => key.value) - y.keys.Min(key => key.value) > 0.25f, foot + ": terangkat jelas saat mengayun");
            float t = (foot == "KakiKiri" ? 0.7f : 0.2f) * walk.length;
            float footVelocity = (x.Evaluate(t + 0.01f) - x.Evaluate(t - 0.01f)) / 0.02f * Mathf.Abs(player.wadahVisual.lossyScale.x);
            Check(Mathf.Abs(footVelocity + player.kecepatanJalan * player.pengaliKecepatanJalan) < 0.05f, foot + ": fase menapak tidak terseret relatif terhadap tanah");
        }
    }
    static void Preview(Transform root, string name)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        Renderer[] visible = renderers.Where(renderer => renderer.enabled && renderer.gameObject.activeInHierarchy).ToArray();
        Bounds bounds = visible[0].bounds; foreach (Renderer renderer in visible) bounds.Encapsulate(renderer.bounds);
        int[] layers = renderers.Select(renderer => renderer.gameObject.layer).ToArray();
        foreach (Renderer renderer in renderers) renderer.gameObject.layer = 31;
        GameObject cameraObject = new GameObject("CharacterPreview_Runtime", typeof(Camera));
        Camera camera = cameraObject.GetComponent<Camera>(); camera.enabled = false;
        camera.orthographic = true; camera.orthographicSize = Mathf.Max(bounds.size.y, bounds.size.x) * 0.62f;
        camera.transform.position = bounds.center + Vector3.back * 10f;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.85f, 0.87f, 0.9f, 1f); camera.cullingMask = 1 << 31;
        var rt = new RenderTexture(512, 512, 24); camera.targetTexture = rt;
        RenderTexture previous = RenderTexture.active;
        camera.Render(); RenderTexture.active = rt;
        var texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0f, 0f, 512f, 512f), 0, 0); texture.Apply();
        File.WriteAllBytes(Work(name), texture.EncodeToPNG());
        RenderTexture.active = previous;
        for (int i = 0; i < renderers.Length; i++) renderers[i].gameObject.layer = layers[i];
        UnityEngine.Object.Destroy(cameraObject); UnityEngine.Object.Destroy(rt); UnityEngine.Object.Destroy(texture);
    }
    static void Check(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); checks.Add("OK: " + message); }
    static void Finish(Exception error)
    {
        File.WriteAllText(Work("character-life-checks.txt"), (error == null ? "PASS\n" : "FAIL\n" + error + "\n") + string.Join("\n", checks));
        routine = null; EditorApplication.isPlaying = false;
    }
}
