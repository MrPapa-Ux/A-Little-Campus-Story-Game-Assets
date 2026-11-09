// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menyiapkan dan mengambil pratinjau checkpoint 7.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class CampusCheckpoint7Preview
{
    static Camera camera;
    static GameObject stage;
    static string Root => Directory.GetParent(Application.dataPath).FullName;
    public static void Prepare()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Preview dari Play Mode.");
        Application.runInBackground = true;
        stage = new GameObject("PreviewEkspresi_Sementara");
        var playerFace = UnityEngine.Object.FindFirstObjectByType<KarakterController>().GetComponentInChildren<EkspresiWajah2D>();
        var npcFace = GameObject.Find("NPC_Nara_Ekspresi").GetComponentInChildren<EkspresiWajah2D>();
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/AmaticSC-Regular SDF.asset");
        EmosiKarakter[] emotions = { EmosiKarakter.Senyum, EmosiKarakter.Senang, EmosiKarakter.Sedih, EmosiKarakter.Marah };
        for (int row = 0; row < 2; row++) for (int col = 0; col < 4; col++)
        {
            var source = row == 0 ? playerFace : npcFace;
            var pivot = new GameObject("Contoh_" + row + "_" + emotions[col]); pivot.transform.SetParent(stage.transform,false);
            pivot.SetActive(false);
            pivot.transform.position = new Vector3(-2.85f + col * 1.9f, row == 0 ? 1.25f : -1.15f, 0f);
            var faceObj = UnityEngine.Object.Instantiate(source.gameObject,pivot.transform); faceObj.name = "Wajah";
            faceObj.transform.localPosition = Vector3.zero; faceObj.transform.localRotation = Quaternion.identity; faceObj.transform.localScale = Vector3.one;
            // Clone juga memuat mesh runtime; hanya sumber sprite dan komponen wajah yang diperlukan.
            foreach (Transform child in faceObj.transform.Cast<UnityEngine.Transform>().ToArray()) UnityEngine.Object.DestroyImmediate(child.gameObject);
            var face = faceObj.GetComponent<EkspresiWajah2D>();
            face.kepala = faceObj.GetComponent<SpriteRenderer>(); face.mataKiri = source.mataKiri; face.mataKanan = source.mataKanan; face.posisiMulut = source.posisiMulut; face.ukuranMaskMulut = source.ukuranMaskMulut; face.maskMataKiri = source.maskMataKiri; face.maskMataKanan = source.maskMataKanan;
            face.gunakanEkspresi = true; face.ekspresi = emotions[col]; face.jedaKedip = new Vector2(100f,100f);
            var hair = source.transform.parent.Find("Rambut 1");
            if (hair != null) { var h = UnityEngine.Object.Instantiate(hair.gameObject,pivot.transform); h.transform.localPosition = hair.localPosition - source.transform.localPosition; h.transform.localScale = Vector3.one; }
            Layer(pivot.transform,31); faceObj.SetActive(true); pivot.SetActive(true);
            var caption = new GameObject("Label",typeof(TextMeshPro)); caption.transform.SetParent(pivot.transform,false); caption.layer = 31;
            caption.transform.localPosition = new Vector3(0,-.90f,-.1f); var label = caption.GetComponent<TextMeshPro>(); label.font = font; label.fontSize = 4f;
            label.text = (row == 0 ? "Arthur" : "Nara") + " - " + emotions[col]; label.alignment = TextAlignmentOptions.Center; label.color = new Color32(43,47,38,255);
            label.rectTransform.sizeDelta = new Vector2(1.9f,.6f);
        }
        camera = new GameObject("KameraPreview",typeof(Camera)).GetComponent<Camera>(); camera.transform.SetParent(stage.transform,false);
        camera.transform.position = new Vector3(0,0,-10); camera.orthographic = true; camera.orthographicSize = 2.65f; camera.aspect = 16f / 9f;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(255,246,221,255); camera.cullingMask = 1 << 31; camera.enabled = false;
    }
    static void Layer(Transform root,int layer) { root.gameObject.layer = layer; foreach (Transform child in root) Layer(child,layer); }
    public static void Capture()
    {
        var rt = new RenderTexture(1920,1080,24); camera.targetTexture = rt; camera.Render(); var old = RenderTexture.active; RenderTexture.active = rt;
        var image = new Texture2D(1920,1080,TextureFormat.RGB24,false); image.ReadPixels(new Rect(0,0,1920,1080),0,0); image.Apply();
        File.WriteAllBytes(Path.Combine(Root,"work/checkpoint7-ekspresi.png"),image.EncodeToPNG()); RenderTexture.active = old; camera.targetTexture = null;
        rt.Release(); UnityEngine.Object.Destroy(rt); UnityEngine.Object.Destroy(image); UnityEngine.Object.Destroy(stage);
    }
}
