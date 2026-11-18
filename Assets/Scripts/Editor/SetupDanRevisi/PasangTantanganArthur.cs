using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PasangTantanganArthur
{
    public const string Chapter = "Assets/Chapter1Preview/Chapter1_Kamar201.unity";
    const string FotoDir = "Assets/FotoTantanganArthur";
    const string Cadangan = "work/tantangan-arthur-20261114";

    [MenuItem("Arthur/Chapter 1/Pasang tantangan samping")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Keluar dari Play Mode dulu.");
        var fotoPuzzle = ImporFoto("foto-kampus.jpg");
        var fotoDekan = ImporFoto("foto-ruang-dekan.png");
        var sebelumnya = SceneManager.GetActiveScene();
        try
        {
            AturScene(Chapter, true, fotoPuzzle, fotoDekan);
            AturScene("Assets/Scenes/Explorer.unity", false, null, null);
            AturScene("Assets/Scenes/CharacterLab.unity", false, null, null);
            AssetDatabase.SaveAssets();
        }
        finally { if (sebelumnya.IsValid() && sebelumnya.isLoaded) SceneManager.SetActiveScene(sebelumnya); }
    }

    internal static Texture2D ImporFoto(string nama)
    {
        string path = FotoDir + "/" + nama;
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Foto belum diimpor: " + path);
        importer.maxTextureSize = 1024;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static void AturScene(string path, bool chapter, Texture2D puzzle, Texture2D dekan)
    {
        var scene = SceneManager.GetSceneByPath(path);
        bool buka = !scene.isLoaded;
        if (buka) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        try
        {
            if (scene.isDirty) throw new InvalidOperationException("Simpan perubahan pengguna terlebih dulu: " + path);
            Directory.CreateDirectory(Cadangan);
            string salinan = Cadangan + "/" + Path.GetFileNameWithoutExtension(path) + "-sebelum.unity";
            if (!File.Exists(salinan)) File.Copy(path, salinan);
            SceneManager.SetActiveScene(scene);
            var semua = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            var buku = semua.Select(t => t.GetComponent<BukuArthur>()).First(b => b != null);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NpcKampus/Template/NPC_Mahasiswa.prefab");
            JalurKampus Jalur(string nama) => semua.First(t => t.name == nama && t.GetComponent<JalurKampus>() != null).GetComponent<JalurKampus>();
            Transform Objek(string nama) => semua.First(t => t.name == nama);
            foreach (var titipan in semua.Select(t => t.GetComponent<QuestTitipanArthur>()).Where(q => q != null && q.ikon == null))
            {
                string gambarBarang = titipan.namaBarang switch
                {
                    "Buku sketsa" => "Buku_Sketsa.png",
                    "Hard disk praktikum" => "Harddisk_Praktikum.png",
                    _ => null
                };
                if (gambarBarang == null) continue;
                titipan.ikon = AssetDatabase.LoadAllAssetsAtPath("Assets/QuestArthur/" + gambarBarang).OfType<Sprite>().First();
                Simpan(titipan);
            }
            foreach (var area in semua.Select(t => t.GetComponent<AreaInteraksi>()).Where(a => a != null))
            {
                var npc = area.GetComponent<NpcYangBisaDiatur>();
                string nama = NamaMahasiswa(area.gameObject.name);
                if (npc != null && npc.namaNpc.Trim() == "Mahasiswa")
                {
                    npc.namaNpc = nama;
                    npc.Terapkan();
                    Simpan(npc);
                }
                foreach (var baris in area.skenarioDialog ?? Array.Empty<BarisDialog>())
                    if (baris.namaKarakter.Trim() == "Mahasiswa") { baris.namaKarakter = nama; Simpan(area); }
            }

            var puzzleNpc = BuatNpc(scene, prefab, "Raka Jepret", "JasmineGK2", Jalur("GK2_JalurBelakang"), .44f);
            Pasang(puzzleNpc, buku, JenisTantanganArthur.Puzzle, "Raka Jepret", puzzle,
                "Keripik", "potatochip_yellow", !chapter,
                "apa kau bisa bantu aku dengan puzzle ini?", "Coba puzzle", chapter);

            var kasir = Objek("Penjual_Kantin_1");
            var penjual = kasir.GetComponent<AreaInteraksi>() != null ? kasir.gameObject :
                BuatNpc(scene, prefab, "Kak Meli", "JasmineGK2", Jalur("Kantin_LorongTengah"), .64f, kasir.position);
            Pasang(penjual, buku, JenisTantanganArthur.Buah, "Kak Meli", null,
                "Pisang", "banana", !chapter, "Arthur, bantu aku dengan tantangan tebas buah, ya? Enam buah saja!", "Tebas buah");

            var bimo = Objek("NPC_Bimo_Joget").gameObject;
            Pasang(bimo, buku, JenisTantanganArthur.Joget, "Bimo", null,
                "Air Minum", "water", !chapter, "Arthur, ikut joget denganku, yuk! Ikuti panahnya. Kita coba tiga sesi.", "Joget bareng");

            var jalurGa = Objek("GA_PintuPioneerChapel").GetComponent<InteraksiJalurKampus>()?.jalur ?? Jalur("GA_Lobby");
            var hitung = BuatNpc(scene, prefab, "Oki Kode", "GA", jalurGa, .65f, Objek("GA_PintuPioneerChapel").position);
            Pasang(hitung, buku, JenisTantanganArthur.Hitung, "Oki Kode", null,
                "Pena", "ball_pen", !chapter,
                "Aku Oki, mahasiswa tahun kedua. Aku baru bikin kalkulator dari kodeku sendiri. Mau coba? Aku kasih tiga soal, kamu hitung pakai kalkulator ini.",
                "Coba kalkulator");

            if (chapter)
            {
                var pintuDekan = Objek("GK1_Masuk_RuanganDekanFilkom");
                var info = BuatNpc(scene, prefab, "Tari", "GK1",
                    pintuDekan.GetComponent<InteraksiJalurKampus>()?.jalur ?? Jalur("GK1_Lantai3_Kiri"), .6f, pintuDekan.position);
                Pasang(info, buku, JenisTantanganArthur.Foto, "Tari", dekan, "", null, false,
                    "Arthur, temanku foto ruang dekan 10 menit yang lalu. Sir Semmy mungkin tidak ada di ruangan saat itu. Aku belum tahu apakah beliau sudah kembali.",
                    "Lihat foto");
                var cerita = semua.Select(t => t.GetComponent<CeritaKamar201Arthur>()).First(c => c != null);
                AturHardisk(cerita);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        finally { if (buka) EditorSceneManager.CloseScene(scene, true); }
    }

    static string NamaMahasiswa(string nama)
    {
        if (nama.Contains("Jasmine_1")) return "Rizky";
        if (nama.Contains("Jasmine")) return "Niko";
        return "Farel";
    }

    internal static GameObject BuatNpc(Scene scene, GameObject prefab, string nama, string kawasan, JalurKampus jalur, float fraksi, Vector3? dekat = null)
    {
        string id = "NPC Samping - " + nama;
        var grup = scene.GetRootGameObjects().FirstOrDefault(r => r.name == id);
        if (grup == null)
        {
            grup = new GameObject(id);
            grup.AddComponent<KelompokNpcManual>().kawasan = kawasan;
            PrefabUtility.InstantiatePrefab(prefab, grup.transform);
        }
        var npc = grup.transform.GetChild(0).gameObject;
        npc.name = nama;
        float jarak = jalur.Panjang * fraksi;
        if (dekat.HasValue)
        {
            float acuan = jalur.JarakTerdekat(dekat.Value);
            jarak = Mathf.Clamp(acuan + (acuan > jalur.Panjang - 3f ? -2.5f : 2.5f), .5f, Mathf.Max(.5f, jalur.Panjang - .5f));
        }
        jalur.Sampel(jarak, out var posisi, out _, out float yaw);
        var samping = Quaternion.Euler(0, yaw, 0) * Vector3.forward * .55f;
        grup.transform.position = posisi - Vector3.up * (ArthurJalurKampus.JarakKaki - .04f) + samping;
        npc.transform.localPosition = Vector3.zero;
        if (npc.transform is RectTransform rect) rect.anchoredPosition3D = Vector3.zero;
        var pengaturan = npc.GetComponent<NpcYangBisaDiatur>();
        pengaturan.jalurInteraksi = jalur;
        pengaturan.geserTitikInteraksi = -samping;
        pengaturan.jarakInteraksi = 1.7f;
        Simpan(pengaturan);
        Simpan(npc.transform);
        return npc;
    }

    internal static void Pasang(GameObject npc, BukuArthur buku, JenisTantanganArthur jenis, string nama, Texture2D foto,
        string hadiah, string ikon, bool itemSaja, string dialog, string tombol, bool pratinjau = false)
    {
        var area = npc.GetComponent<AreaInteraksi>();
        if (area == null) throw new InvalidOperationException("NPC belum punya interaksi: " + npc.name);
        var pengaturan = npc.GetComponent<NpcYangBisaDiatur>();
        if (pengaturan != null)
        {
            pengaturan.namaNpc = nama;
            pengaturan.dialogSederhana = false;
            Simpan(pengaturan);
        }
        var game = npc.GetComponent<TantanganSampingArthur>() ?? npc.AddComponent<TantanganSampingArthur>();
        game.jenis = jenis;
        game.namaNpc = nama;
        game.area = area;
        game.buku = buku;
        game.hadiah = hadiah;
        game.ikonHadiah = ikon == null ? null : AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ContohTasArthur/" + ikon + ".png");
        game.gambar = foto;
        game.buah = jenis == JenisTantanganArthur.Buah && !itemSaja
            ? new[] { "red_apple", "strawberry", "watermelon1" }.Select(file =>
                AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/All Asset/Sprites/Icon/Pixel_Mart/" + file + ".png")).ToArray()
            : null;
        game.itemSaja = itemSaja;
        game.pratinjauSaja = pratinjau;
        area.skenarioDialog = new[] { new BarisDialog {
            siapaYangBicara = Pembicara.Kanan_NPC,
            namaKarakter = nama,
            teksDialog = itemSaja ? "Hei, Arthur! Aku punya sesuatu buatmu." : dialog,
            teksTombolLanjut = "Lanjut"
        }};
        area.daftarOpsi = new[] { new OpsiInteraksi { teksOpsi = itemSaja ? "Terima" : tombol, tantanganArthur = game } };
        area.tampilkanTombolKeluar = true;
        Simpan(game);
        Simpan(area);
    }

    internal static void AturHardisk(CeritaKamar201Arthur cerita)
    {
        var penanda = cerita.areaHardisk.GetComponent<InteraksiJalurKampus>();
        if (penanda == null || penanda.jalur == null) throw new InvalidOperationException("Jalur hard disk belum tersedia.");
        // Gunakan tepi bawah lemari, di luar bidang kayu yang menutupi sprite.
        penanda.radius = 1.5f;
        penanda.ikutiKepalaArthur = false;
        cerita.areaHardisk.transform.position = new Vector3(1793.5f, 3.74033f, 1.7f);
        cerita.visualHardisk.transform.position = new Vector3(1792.48f, 2.17f, 2.45f);
        cerita.visualHardisk.transform.rotation = Quaternion.Euler(55, 0, 0);
        cerita.visualHardisk.transform.localScale = Vector3.one * .39f;
        Simpan(penanda);
        Simpan(cerita.areaHardisk.transform);
        Simpan(cerita.visualHardisk.transform);
    }

    static void Simpan(UnityEngine.Object objek)
    {
        PrefabUtility.RecordPrefabInstancePropertyModifications(objek);
        EditorUtility.SetDirty(objek);
    }
}
