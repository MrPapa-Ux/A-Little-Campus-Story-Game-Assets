using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

internal static class GayaMinigameBayanganEditor
{
    const string Folder = "Assets/Chapter1Preview/MinigameRahasia/";
    const int VersiGaya = 3;
    const string Chapter = "Assets/Chapter1Preview/Chapter1_Kamar201.unity";

    internal static GayaMinigameBayangan Ambil()
    {
        string path = Folder + "Gaya Arthur.asset";
        var tersimpan = AssetDatabase.LoadAssetAtPath<GayaMinigameBayangan>(path);
        if (tersimpan != null && tersimpan.versi == VersiGaya) return tersimpan;

        var scene = EditorSceneManager.GetSceneByPath(Chapter);
        bool buka = !scene.isLoaded;
        if (buka) scene = EditorSceneManager.OpenScene(Chapter, OpenSceneMode.Additive);
        GameObject duplikat = null;
        try
        {
            var objek = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true)).ToArray();
            var cerita = objek.Select(x => x.GetComponent<CeritaKamar201Arthur>()).First(x => x != null);
            Sprite AmbilSprite(string nama) => objek.Where(x => x.name == nama)
                .Select(x => x.GetComponent<Image>()?.sprite).First(x => x != null);

            var visual = cerita.pemain.wadahVisual;
            duplikat = UnityEngine.Object.Instantiate(visual.gameObject);
            var sumber = duplikat.transform;
            sumber.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            sumber.localScale = Vector3.one;
            var animator = duplikat.GetComponent<Animator>();
            var klip = animator.runtimeAnimatorController.animationClips;
            var diam = klip.Single(x => x.name == "ArthurIdle");
            var lari = klip.Single(x => x.name == "ArthurWalk");
            var render = sumber.GetComponentsInChildren<SpriteRenderer>(true)
                .Where(x => x.gameObject.activeInHierarchy && x.enabled && x.sprite != null && x.name != "Lanyard")
                .OrderBy(x => x.sortingOrder).ThenByDescending(x => x.transform.position.z).ToArray();

            diam.SampleAnimation(duplikat, 0);
            var diamAsli = Sampel(sumber, render);
            var kepala = Sampel(sumber, render.Where(x =>
                x.name == "KepalaArthur" || x.name == "Rambut 1" || x.name == "Telinga").ToArray());
            var batas = Ukuran(diamAsli);
            var batasKepala = Ukuran(kepala);

            var gaya = ScriptableObject.CreateInstance<GayaMinigameBayangan>();
            gaya.versi = VersiGaya;
            gaya.tanganKanan = Array.FindIndex(render, x => x.name == "TanganKanan");
            gaya.badan = Array.FindIndex(render, x => x.name == "Badan");
            gaya.font = cerita.judul.font;
            gaya.kertas = AmbilSprite("Kertas_Dialog_Asli");
            gaya.kiri = AmbilSprite("Btn_Kiri");
            gaya.kanan = AmbilSprite("Btn_Kanan");
            gaya.aksi = AmbilSprite("Btn_Interact");
            gaya.sabit = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "Art/SabitKata.png");
            gaya.diam = Skalakan(diamAsli, batas, 126);
            gaya.kepala = Skalakan(kepala, batasKepala, 65);
            gaya.durasiLari = lari.length;
            int jumlah = Mathf.Max(2, Mathf.CeilToInt(lari.length * 30));
            gaya.lari = new GayaMinigameBayangan.Pose[jumlah];
            for (int i = 0; i < jumlah; i++)
            {
                lari.SampleAnimation(duplikat, lari.length * i / jumlah);
                gaya.lari[i] = Skalakan(Sampel(sumber, render), batas, 126);
            }
            if (gaya.font == null || gaya.sabit == null || kepala.Length != 3 || render.Length < 8 || gaya.tanganKanan < 0 || gaya.badan < 0)
                throw new IOException("Aset gaya Arthur belum lengkap.");
            if (tersimpan == null)
            {
                AssetDatabase.CreateAsset(gaya, path);
                return gaya;
            }
            EditorUtility.CopySerialized(gaya, tersimpan);
            EditorUtility.SetDirty(tersimpan);
            UnityEngine.Object.DestroyImmediate(gaya);
            return tersimpan;
        }
        finally
        {
            if (duplikat != null) UnityEngine.Object.DestroyImmediate(duplikat);
            if (buka) EditorSceneManager.CloseScene(scene, true);
        }
    }

    static GayaMinigameBayangan.Bagian[] Sampel(Transform sumber, SpriteRenderer[] render)
    {
        return render.Select(x =>
        {
            Vector3 pusat = sumber.InverseTransformPoint(x.transform.TransformPoint(x.sprite.bounds.center));
            Vector3 sumbuX = sumber.InverseTransformVector(x.transform.TransformVector(Vector3.right));
            Vector3 sumbuY = sumber.InverseTransformVector(x.transform.TransformVector(Vector3.up));
            return new GayaMinigameBayangan.Bagian
            {
                sprite = x.sprite,
                posisi = pusat,
                ukuran = Vector2.Scale(x.sprite.rect.size / x.sprite.pixelsPerUnit, new Vector2(sumbuX.magnitude, sumbuY.magnitude)),
                sudut = Mathf.Atan2(sumbuX.y, sumbuX.x) * Mathf.Rad2Deg,
                warna = x.color
            };
        }).ToArray();
    }

    static Rect Ukuran(GayaMinigameBayangan.Bagian[] bagian)
    {
        return Rect.MinMaxRect(bagian.Min(x => x.posisi.x - x.ukuran.x / 2), bagian.Min(x => x.posisi.y - x.ukuran.y / 2),
            bagian.Max(x => x.posisi.x + x.ukuran.x / 2), bagian.Max(x => x.posisi.y + x.ukuran.y / 2));
    }

    static GayaMinigameBayangan.Pose Skalakan(GayaMinigameBayangan.Bagian[] bagian, Rect batas, float tinggi)
    {
        for (int i = 0; i < bagian.Length; i++)
        {
            float skala = tinggi / batas.height;
            bagian[i].posisi = (bagian[i].posisi - batas.center) * skala;
            bagian[i].ukuran *= skala;
        }
        return new GayaMinigameBayangan.Pose { bagian = bagian };
    }
}
