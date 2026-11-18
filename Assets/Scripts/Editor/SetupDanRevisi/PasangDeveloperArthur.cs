using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PasangDeveloperArthur
{
    [MenuItem("Arthur/Chapter 1/Pasang Developer dan kesinambungan Eksplorasi")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Hentikan Play sebelum menyunting scene.");
        var sebelumnya = SceneManager.GetActiveScene();
        foreach (var path in new[] { PasangTantanganArthur.Chapter, "Assets/Scenes/Explorer.unity" })
        {
            var scene = SceneManager.GetSceneByPath(path);
            bool tambahan = !scene.isLoaded;
            if (tambahan) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                if (scene.isDirty) throw new InvalidOperationException("Simpan perubahan pengguna di " + scene.name);
                string arsip = "work/developer-tutorial-20261116";
                Directory.CreateDirectory(arsip);
                string cadangan = Path.Combine(arsip, scene.name + "-sebelum.unity");
                if (!File.Exists(cadangan)) File.Copy(path, cadangan);
                SceneManager.SetActiveScene(scene);
                Pasang(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Gagal menyimpan " + scene.name);
            }
            finally
            {
                if (tambahan) EditorSceneManager.CloseScene(scene, true);
                if (sebelumnya.IsValid() && sebelumnya.isLoaded) SceneManager.SetActiveScene(sebelumnya);
            }
        }
    }

    static void Pasang(Scene scene)
    {
        var objek = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true)).ToArray();
        Transform Titik(string nama) => objek.First(x => x.name == nama);
        var jalur = Titik("GH_LantaiSatu").GetComponent<JalurKampus>();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NpcKampus/Template/NPC_Mahasiswa.prefab");
        float masuk = jalur.JarakTerdekat(Titik("TitikMasuk_GH").position);
        float arah = masuk > jalur.Panjang / 2 ? -1 : 1;
        float posisi = Mathf.Clamp01((masuk + arah * 6f) / jalur.Panjang);
        var npc = PasangTantanganArthur.BuatNpc(scene, prefab, "Developer", "GH", jalur, posisi);
        var pengaturan = npc.GetComponent<NpcYangBisaDiatur>();
        pengaturan.namaNpc = "Developer";
        pengaturan.dialogSederhana = true;
        pengaturan.dialog = "Hei, Arthur! Selamat datang di Guest House. Kalau lagi jalan-jalan di kampus, coba sapa orang-orang yang kamu temui.";
        pengaturan.Terapkan();
        PrefabUtility.RecordPrefabInstancePropertyModifications(pengaturan);
        PrefabUtility.RecordPrefabInstancePropertyModifications(npc.GetComponent<AreaInteraksi>());

        var cerita = objek.Select(x => x.GetComponent<CeritaKamar201Arthur>()).FirstOrDefault(x => x != null);
        if (cerita != null)
        {
            cerita.tutorial[14].gambar = cerita.tutorial[0].gambar;
            cerita.tutorial[14].isi = "Cari hard disk Arthur di Kamar 201. Kamu punya empat menit. Petunjuknya tercatat di Task.";
            cerita.tutorial[3].isi = "Kalau Arthur diam selama tujuh detik, bar mulai turun. Bergerak untuk memulihkannya. Bar dijeda saat berdialog atau memakai HP. Jika habis, Arthur mengulang momen terekam.";
            cerita.tutorial[11].isi = "Atur volume dan kecerahan di sini. Kembali ke menu perlu konfirmasi. Tuntaskan Chapter 1 agar tugas sampingan bisa diteruskan di Eksplorasi.";
            EditorUtility.SetDirty(cerita);
        }

        if (scene.name != "Explorer") return;
        var buku = objek.Select(x => x.GetComponent<BukuArthur>()).First(x => x != null);
        var raka = objek.Select(x => x.GetComponent<TantanganSampingArthur>())
            .FirstOrDefault(x => x != null && x.jenis == JenisTantanganArthur.Puzzle);
        if (raka != null)
        {
            raka.pratinjauSaja = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(raka);
            EditorUtility.SetDirty(raka);
        }
        var jalurGA = Titik("GA_TerasLobby_LantaiDua").GetComponent<JalurKampus>();
        var tangga = (Titik("GA_LobbyLantaiDua_TurunKanan").position + Titik("GA_TanggaTengah_KeLantaiSatu").position) * .5f;
        var nara = PasangTantanganArthur.BuatNpc(scene, prefab, "Nara", "GA", jalurGA, jalurGA.JarakTerdekat(tangga) / jalurGA.Panjang);
        PasangTantanganArthur.Pasang(nara, buku, JenisTantanganArthur.Jigsaw, "Nara", null, "Kukis", "cookies",
            true, "", "Terima");
    }
}
