using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PratinjauMinigameBayanganEditor
{
    const string Folder = "Assets/Chapter1Preview/MinigameRahasia/";
    const string Istana = Folder + "Pratinjau Pedang Harapan.unity";
    const string Langit = Folder + "Pratinjau Langit Edel.unity";

    [MenuItem("Arthur/Pratinjau Acara Rahasia/Buka Pedang Harapan - GK1")]
    public static void BukaIstana() => Buka(Istana);

    [MenuItem("Arthur/Pratinjau Acara Rahasia/Buka Langit Edel - Edelweiss")]
    public static void BukaLangit() => Buka(Langit);

    public static void BuatScene()
    {
        var gaya = GayaMinigameBayanganEditor.Ambil();
        Buat(Istana, PratinjauMinigameBayangan.Arena.Istana, "PadangIstana", gaya);
        Buat(Langit, PratinjauMinigameBayangan.Arena.Edelweiss, "LangitEdel", gaya);
        AssetDatabase.SaveAssets();
    }

    static void Buka(string scene)
    {
        if (EditorApplication.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuatScene();
        EditorSceneManager.OpenScene(scene);
    }

    static void Buat(string path, PratinjauMinigameBayangan.Arena arena, string latar, GayaMinigameBayangan gaya)
    {
        Texture2D Art(string nama) => AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "Art/" + nama + ".png");
        var laguArena = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Aset/Audio/Musik_dan_Suara_Arthur/DemoOrisinal/Jou_Castle.ogg");
        if (laguArena == null) throw new IOException("Aset Jou Castle belum tersedia.");
        var scene = EditorSceneManager.GetSceneByPath(path);
        bool buka = !scene.isLoaded;
        if (buka)
            scene = File.Exists(path) ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            foreach (var akar in scene.GetRootGameObjects())
            {
                var lama = akar.GetComponent<PratinjauMinigameBayangan>();
                if (lama == null) continue;
                if (lama.gaya != gaya || lama.laguArena != laguArena)
                {
                    lama.gaya = gaya;
                    lama.laguArena = laguArena;
                    if (lama.arena == PratinjauMinigameBayangan.Arena.Istana) lama.tokoh = null;
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene, path);
                }
                return;
            }

            var kamera = new GameObject("Kamera arena", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(kamera.gameObject, scene);
            kamera.orthographic = true;
            kamera.clearFlags = CameraClearFlags.SolidColor;
            kamera.backgroundColor = new Color32(17, 26, 39, 255);
            kamera.cullingMask = 0;
            var game = new GameObject(arena == PratinjauMinigameBayangan.Arena.Istana ? "Pedang Harapan" : "Langit Edel")
                .AddComponent<PratinjauMinigameBayangan>();
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(game.gameObject, scene);
            game.arena = arena;
            game.gaya = gaya;
            game.laguArena = laguArena;
            game.latar = Art(latar);
            if (arena == PratinjauMinigameBayangan.Arena.Edelweiss) game.tokoh = Art("PesawatArthur");
            game.stress = Art("ShadowStress");
            game.burnout = Art("ShadowBurnout");
            game.pedang = Art("PedangHarapan");
            if (game.latar == null || (arena == PratinjauMinigameBayangan.Arena.Edelweiss && game.tokoh == null) || game.stress == null || game.burnout == null || game.pedang == null)
                throw new IOException("Aset visual pratinjau belum lengkap.");
            if (!EditorSceneManager.SaveScene(scene, path))
                throw new IOException("Tidak dapat menyimpan scene pratinjau.");
        }
        finally { if (buka) EditorSceneManager.CloseScene(scene, true); }
    }
}
