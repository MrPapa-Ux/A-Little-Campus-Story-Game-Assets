using UnityEngine;
using UnityEngine.UI;

// Satu mesh UI untuk semua tetes. Tidak memakai prefab, fisika, atau kamera kedua.
public sealed class HujanKampusArthur : MaskableGraphic
{
    const int JumlahTetes = 84;

    internal static HujanKampusArthur Buat(Transform induk)
    {
        var lapisan = new GameObject("Hujan kampus", typeof(RectTransform), typeof(Canvas));
        lapisan.transform.SetParent(induk, false);
        var canvas = lapisan.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -5;
        var go = new GameObject("Tetes hujan", typeof(RectTransform));
        go.transform.SetParent(lapisan.transform, false);
        var hujan = go.AddComponent<HujanKampusArthur>();
        hujan.raycastTarget = false;
        hujan.rectTransform.anchorMin = Vector2.zero;
        hujan.rectTransform.anchorMax = Vector2.one;
        hujan.rectTransform.offsetMin = hujan.rectTransform.offsetMax = Vector2.zero;
        return hujan;
    }

    void Update() => SetVerticesDirty();

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect area = rectTransform.rect;
        float waktu = Time.time;
        for (int i = 0; i < JumlahTetes; i++)
        {
            float x = area.xMin + area.width * Mathf.Repeat(i * .618034f - waktu * .048f, 1);
            float y = area.yMin + area.height * (1.05f - Mathf.Repeat(i * .414214f + waktu * (.67f + (i % 5) * .067f), 1.12f));
            float panjang = area.height * (.025f + (i % 7) * .002f);
            float lebar = area.height * .0018f;
            var atas = new Vector2(x, y);
            var bawah = atas + new Vector2(-panjang * .18f, -panjang);
            Color32 terang = new Color32(202, 226, 250, (byte)(120 + (i % 6) * 12));
            Color32 bening = new Color32(196, 221, 247, 0);
            int indeks = mesh.currentVertCount;
            mesh.AddVert(atas, bening, Vector2.zero);
            mesh.AddVert(atas + Vector2.right * lebar, bening, Vector2.zero);
            mesh.AddVert(bawah + Vector2.right * lebar, terang, Vector2.zero);
            mesh.AddVert(bawah, terang, Vector2.zero);
            mesh.AddTriangle(indeks, indeks + 1, indeks + 2);
            mesh.AddTriangle(indeks, indeks + 2, indeks + 3);
        }
    }
}
