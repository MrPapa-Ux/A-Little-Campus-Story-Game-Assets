using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PasangCeritaFotoArthur
{
    [MenuItem("Arthur/Chapter 1/Pasang cerita foto dan jigsaw")]
    public static void Run()
    {
        var scene = SceneManager.GetSceneByPath(PasangTantanganArthur.Chapter);
        if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(PasangTantanganArthur.Chapter, OpenSceneMode.Additive);
        if (scene.isDirty || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Simpan scene dan hentikan Play sebelum pemasangan.");
        Directory.CreateDirectory("work/cerita-foto-20261115");
        const string backup = "work/cerita-foto-20261115/Chapter1-sebelum.unity";
        if (!File.Exists(backup)) File.Copy(scene.path, backup);
        var aktif = SceneManager.GetActiveScene();
        SceneManager.SetActiveScene(scene);
        try
        {
            var objek = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true)).ToArray();
            Transform Titik(string nama) => objek.Single(x => x.name == nama);
            JalurKampus Jalur(string nama) => Titik(nama).GetComponent<JalurKampus>();
            var buku = objek.Select(x => x.GetComponent<BukuArthur>()).First(x => x != null);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NpcKampus/Template/NPC_Mahasiswa.prefab");
            GameObject Npc(string nama, string kawasan, JalurKampus jalur, float fraksi) =>
                PasangTantanganArthur.BuatNpc(scene, prefab, nama, kawasan, jalur, fraksi);
            Texture2D Foto(string nama) => PasangTantanganArthur.ImporFoto(nama + ".png");

            var kucing = Npc("Mika", "TamanUNKLAB", Jalur("UNK_Bundaran_JalurKananCrystal"), .52f);
            PasangTantanganArthur.Pasang(kucing, buku, JenisTantanganArthur.Album, "Mika", null, "", null, false,
                "Arthur, tadi malam aku menemukan kucing unik di jalan menuju Crystal. Ini fotonya!", "Lihat foto kucing");
            var albumKucing = kucing.GetComponent<TantanganSampingArthur>();
            albumKucing.fotoAlbum = new[] { Foto("kucing-malam") };
            albumKucing.keteranganAlbum = new[] { "Mika • Kucing yang kutemukan tadi malam." };

            var jalurGK = Jalur("GK1_Lantai5_Kanan");
            var aiko = Npc("Aiko", "GK1", jalurGK, .54f);
            var ralf = Npc("Ralf", "GK1", jalurGK, .54f);
            jalurGK.Sampel(jalurGK.Panjang * .54f, out _, out _, out float yaw);
            Vector3 samping = Quaternion.Euler(0, yaw, 0) * Vector3.right;
            GeserNpc(aiko, -samping * 1.45f);
            GeserNpc(ralf, samping * .85f);
            // Aiko memulai percakapan; Ralf ikut dalam album dan emote yang sama.
            PasangTantanganArthur.Pasang(aiko, buku, JenisTantanganArthur.Album, "Aiko", null, "", null, false,
                "Arthur, sini lihat foto kami bareng di UVICS, UNKLAB Virtue in Computer Science!", "Lihat foto UVICS");
            PasangTantanganArthur.Pasang(ralf, buku, JenisTantanganArthur.Album, "Ralf", null, "", null, false,
                "Aiko lagi buka foto UVICS. Yuk lihat bareng, Arthur.", "Lihat foto UVICS");
            var pendamping = ralf.GetComponent<NpcYangBisaDiatur>();
            pendamping.geserTitikInteraksi = Vector3.zero;
            pendamping.jarakInteraksi = .5f;
            PrefabUtility.RecordPrefabInstancePropertyModifications(pendamping);
            var fotoUVICS = new[] { Foto("uvics-bersama"), Foto("uvics-lab"), Foto("uvics-webinar") };
            var ucapan = new[] { "Aiko • Lihat, Ralf! Ini foto kita bareng anak-anak UVICS.",
                "Ralf • Aku juga punya dua foto. Ini yang pertama.",
                "Ralf • Minimal hadir saat WEBINAR lah!" };
            var pasangan = new[] { aiko.GetComponent<NPCBubbleTrigger>(), ralf.GetComponent<NPCBubbleTrigger>() };
            foreach (var bubble in pasangan)
            {
                bubble.arahkanEkorKeKepala = false;
                bubble.tinggiBubbleDiLayar = .105f;
                if (bubble.bubbleChatTransform is RectTransform rect)
                    rect.anchoredPosition = new Vector2(0, 1.58f);
                if (bubble.rendererEmote != null)
                {
                    bubble.rendererEmote.sortingOrder = 11;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(bubble.rendererEmote);
                }
                PrefabUtility.RecordPrefabInstancePropertyModifications(bubble);
                PrefabUtility.RecordPrefabInstancePropertyModifications(bubble.bubbleChatTransform);
            }
            foreach (var npc in new[] { aiko, ralf })
            {
                var album = npc.GetComponent<TantanganSampingArthur>();
                album.fotoAlbum = fotoUVICS;
                album.keteranganAlbum = ucapan;
                album.temanTertawa = pasangan;
                album.emoteTertawa = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/NpcKampus/EmoteTaman/1f92d.png");
                EditorUtility.SetDirty(album);
            }

            var jalurGA = Jalur("GA_TerasLobby_LantaiDua");
            var tengah = (Titik("GA_LobbyLantaiDua_TurunKanan").position + Titik("GA_TanggaTengah_KeLantaiSatu").position) / 2;
            var jigsaw = Npc("Nara", "GA", jalurGA, jalurGA.JarakTerdekat(tengah) / jalurGA.Panjang);
            PasangTantanganArthur.Pasang(jigsaw, buku, JenisTantanganArthur.Jigsaw, "Nara", Foto("jigsaw-taman"),
                "Kukis", "cookies", false, "Arthur, boleh bantu susun foto Taman UNKLAB ini? Ada 49 potongan.", "Susun jigsaw");
            EditorUtility.SetDirty(albumKucing);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Scene Chapter 1 gagal disimpan.");
        }
        finally
        {
            if (aktif.IsValid() && aktif.isLoaded) SceneManager.SetActiveScene(aktif);
        }
    }

    static void GeserNpc(GameObject npc, Vector3 geser)
    {
        npc.transform.parent.position += geser;
        var pengaturan = npc.GetComponent<NpcYangBisaDiatur>();
        pengaturan.geserTitikInteraksi -= geser;
        PrefabUtility.RecordPrefabInstancePropertyModifications(pengaturan);
    }
}
