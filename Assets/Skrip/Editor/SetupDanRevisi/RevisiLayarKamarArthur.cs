using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RevisiLayarKamarArthur
{
    [MenuItem("Arthur/Chapter 1/Rapikan kamar dan hard disk")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Keluar dari Play Mode terlebih dulu.");
        var awal = SceneManager.GetActiveScene();
        foreach (string path in new[] { PasangTantanganArthur.Chapter, "Assets/Scenes/Explorer.unity", "Assets/Scenes/CharacterLab.unity" })
        {
            var scene = SceneManager.GetSceneByPath(path);
            bool buka = !scene.isLoaded;
            if (buka) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                if (scene.isDirty) throw new InvalidOperationException("Simpan perubahan di " + scene.name);
                const string folder = "work/fit-hujan-kamar";
                Directory.CreateDirectory(folder);
                string cadangan = Path.Combine(folder, scene.name + "-sebelum.unity");
                if (!File.Exists(cadangan)) File.Copy(path, cadangan);
                var objek = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
                var jalur = objek.Single(t => t.name == "GH_Kamar201" && t.GetComponent<JalurKampus>() != null).GetComponent<JalurKampus>();
                var titikKiri = jalur.titik.OrderBy(t => t.position.x).First();
                var titikKanan = jalur.titik.OrderBy(t => t.position.x).Last();
                titikKiri.position = new Vector3(1794.6f, titikKiri.position.y, titikKiri.position.z);
                titikKanan.position = new Vector3(1805.4f, titikKanan.position.y, titikKanan.position.z);
                jalur.batasKiriKamera = 1796.3f;
                jalur.batasKananKamera = 1803.7f;
                var cerita = objek.Select(t => t.GetComponent<CeritaKamar201Arthur>()).FirstOrDefault(c => c != null);
                if (cerita != null)
                {
                    PasangTantanganArthur.AturHardisk(cerita);
                    var atan = objek.Select(t => t.GetComponent<TemanSekamarChapter1>()).Single(t => t != null);
                    var posisi = atan.transform.position;
                    posisi.x = 1797.15f;
                    if (atan.transform.parent != null && atan.transform.parent.childCount == 1)
                        atan.transform.parent.position += posisi - atan.transform.position;
                    else atan.transform.position = posisi;
                    if (atan.transform is RectTransform rect) rect.anchoredPosition3D = rect.localPosition;
                    atan.petunjuk = "Hard diskmu belum ketemu? Coba lihat lantai di bawah lemari kiri.";
                    PrefabUtility.RecordPrefabInstancePropertyModifications(atan);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(atan.transform);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Gagal menyimpan " + scene.name);
            }
            finally
            {
                if (buka) EditorSceneManager.CloseScene(scene, true);
                if (awal.IsValid() && awal.isLoaded) SceneManager.SetActiveScene(awal);
            }
        }
    }
}
