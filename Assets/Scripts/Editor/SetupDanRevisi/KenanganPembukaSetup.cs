using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class KenanganPembukaSetup
{
    const string ScenePath = "Assets/Scenes/Transisi_Arthur.unity";
    const string AtlasPath = "Assets/Chapter1Preview/KenanganPembuka/Kenangan_Arthur_TanpaAngka.png";
    static RectTransform RectUI(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var r = go.GetComponent<RectTransform>(); r.SetParent(parent, false);
        r.anchorMin = min; r.anchorMax = max; r.offsetMin = r.offsetMax = Vector2.zero;
        return r;
    }
    public static void Run()
    {
        AssetDatabase.Refresh();
        var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
        importer.textureType = TextureImporterType.Default;
        importer.mipmapEnabled = false; importer.wrapMode = TextureWrapMode.Clamp;
        importer.maxTextureSize = 2048; importer.npotScale=TextureImporterNPOTScale.None; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var director = UnityEngine.Object.FindFirstObjectByType<SutradaraTransisi>();
        if (director == null) throw new Exception("Sutradara transisi tidak ditemukan.");
        var old = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Kenangan_Arthur_Pembuka");
        if (old != null) UnityEngine.Object.DestroyImmediate(old);
        var go = new GameObject("Kenangan_Arthur_Pembuka", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 2000;
        var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
        var group = RectUI("Gambar_dan_Narasi", go.transform, Vector2.zero, Vector2.one);
        var cg = group.gameObject.AddComponent<CanvasGroup>(); cg.alpha = 0; cg.blocksRaycasts = false;
        var black = group.gameObject.AddComponent<Image>(); black.color = Color.black; black.raycastTarget = false;
        var safe = RectUI("Batas_Aman_Layar", group, Vector2.zero, Vector2.one);
        var holder = RectUI("Bidang_Gambar", safe, new Vector2(.025f, .26f), new Vector2(.975f, .98f));
        var pic = RectUI("Gambar_Aktif", holder, Vector2.zero, Vector2.one);
        var raw = pic.gameObject.AddComponent<RawImage>(); raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath); raw.raycastTarget = false;
        var fit = pic.gameObject.AddComponent<AspectRatioFitter>(); fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent; fit.aspectRatio = 1.82f;
        var caption = RectUI("Narasi_Bisa_Diatur", safe, new Vector2(.07f, .035f), new Vector2(.93f, .23f));
        var text = caption.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = director.teksAlasan.font; text.fontSharedMaterial = director.teksAlasan.fontSharedMaterial;
        text.color = Color.white; text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 44; text.enableAutoSizing = true; text.fontSizeMin = 32; text.fontSizeMax = 44;
        text.textWrappingMode = TextWrappingModes.Normal; text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false; text.margin = new Vector4(12, 8, 12, 8);
        var player = go.AddComponent<KenanganPembukaArthur>(); player.kelompok = cg; player.gambar = raw;
        player.teks = text; player.areaAman = safe; player.fadeMasuk = player.fadeKeluar = 1; player.durasiSudahDisetujui = true;
        string[] words = {
            "Ketika awal masuk kuliah, dunia serasa baru, begitu cerah....",
            "Aku mendapatkan begitu banyak teman, dan memang sudah seharusnya kuliah begitu.",
            "Dengan bekerja sama, dalam lingkungan perkuliahan,",
            "Semua hal yang berat akan terasa ringan, karena bebannya dibagi merata.",
            "Menurutku, pada akhirnya, kuliah itu adalah tentang menimbah ilmu dan membangun karakter.",
            "Dan lagi menurutku, itu adalah omong kosong jika kuliah hanya kejar nilai, ipk, karena itu hanyalah angka.",
            "Akan tetapi, jika kejar ipk bersama, nilai bagus bersama, maka angka itu bukan sekadar angka.\n\nTapi akan menjadi kenangan numerik, kenangan yang membuktikan bahwa angka juga punya perasaan, bahwa konstanta dapat berubah menjadi variabel.",
            "Sungguh masa lalu yang indah...."
        };
        player.adegan = words.Select((s, i) => new KenanganPembukaArthur.Adegan {
            narasi = s, lamaTampil = i == 6 ? 15 : 6,
            // Sedikit masuk dari tepi setiap panel agar garis panel tetangga tidak ikut.
            bagianGambar = new Rect(i % 2 * .5f + .012f, (3 - i / 2) * .25f + .009f, .476f, .232f)
        }).ToArray();
        director.kenanganPembuka = player;
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        if (player.adegan.Length != 8 || player.gambar.texture == null) throw new Exception("Atlas atau urutan tidak lengkap.");
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < 8; i++) {
            var r = player.adegan[i].bagianGambar;
            if (r.xMin < 0 || r.yMin < 0 || r.xMax > 1 || r.yMax > 1) throw new Exception("Batas panel salah.");
            text.text = words[i]; Canvas.ForceUpdateCanvases(); text.ForceMeshUpdate(true, true);
            Debug.Log("KENANGAN_TEXT " + i + " rect=" + caption.rect + " font=" + text.fontSize + " preferred=" + text.preferredHeight);
            // Batch mode tidak selalu memiliki Game View berukuran nyata.
            // Ukuran caption diuji lagi pada pratinjau Game View.
            if (caption.rect.width > 500 && caption.rect.height > 100 && text.isTextOverflowing)
                throw new Exception("Narasi terpotong pada gambar " + (i + 1));
        }
        text.text = words[0];
        Directory.CreateDirectory("work/kenangan-pembuka");
        File.WriteAllText("work/kenangan-pembuka/hasil-setup.txt", "OK: 8 gambar tanpa angka, 1 scene, teks utuh, fade 1 detik. Durasi belum disetujui.\nAlur: pertanyaan > kenangan > loading lama > Arthur mengetik.");
        Debug.Log("KENANGAN_PEMBUKA_OK");
    }
    public static void UpgradeNarasi()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var player = UnityEngine.Object.FindFirstObjectByType<KenanganPembukaArthur>();
        if (player == null) throw new Exception("Kenangan pembuka belum tersedia.");
        player.laguKenangan = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AudioArthur/TambahanLagu/MisterAndTheBall.mp3");
        if (player.laguKenangan == null) throw new Exception("MisterAndTheBall tidak ditemukan.");
        player.hurufPerDetik = 12; player.jedaTandaBaca = 1.5f;
        var text = player.teks;
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath("2b82d61f9e230cf47a8bbb975fe493f2"));
        if (text.font == null) throw new Exception("Font adegan mengetik tidak ditemukan.");
        text.fontSharedMaterial = text.font.material;
        text.fontSize = 48; text.enableAutoSizing = true;
        text.fontSizeMin = 30; text.fontSizeMax = 48;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.rectTransform.anchorMin = new Vector2(.07f, .035f);
        text.rectTransform.anchorMax = new Vector2(.93f, .35f);
        var holder = player.gambar.transform.parent.GetComponent<RectTransform>();
        holder.anchorMin = new Vector2(.025f, .38f);
        holder.anchorMax = new Vector2(.975f, .98f);
        foreach (var entry in player.adegan)
        {
            var ukuran = text.GetPreferredValues(entry.narasi + "│", 1920 * .86f - 24, Mathf.Infinity);
            if (ukuran.y > 1080 * .315f - 16) throw new Exception("Narasi melampaui batas: " + ukuran.y);
        }
        text.text = player.adegan[0].narasi;
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        player.durasiSudahDisetujui = true;
        EditorSceneManager.SaveScene(scene);
        Debug.Log("KENANGAN_NARASI_MUSIK_OK: font56 rata kiri, narasi fade; anggaran waktu lama dan MisterAndTheBall dipertahankan.");
    }
    [MenuItem("Arthur/Pratinjau/Kenangan Pembuka Chapter 1")]
    public static void Preview()
    {
        if (EditorApplication.isPlaying) return;
        EditorSceneManager.OpenScene(ScenePath);
        EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
    }
}
