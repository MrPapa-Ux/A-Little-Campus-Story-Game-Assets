using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public sealed class PotonganJigsawArthur : RawImage
{
    int indeks;
    bool dipilih;
    const int Segmen = 14;
    internal int UkuranPapan { get; set; } = 7;

    public void Atur(int potongan, bool sorot)
    {
        indeks = potongan;
        dipilih = sorot;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        int akhir = UkuranPapan - 1;
        int kolom = indeks % UkuranPapan;
        int baris = akhir - indeks / UkuranPapan;
        var ukuran = rectTransform.rect.size;
        Color32 rona = dipilih ? new Color32(255, 215, 141, 255) : color;
        mesh.AddVert(Vector3.zero, rona, new Vector2((kolom + .5f) / UkuranPapan, (baris + .5f) / UkuranPapan));
        for (int sisi = 0; sisi < 4; sisi++)
        {
            for (int langkah = 0; langkah < Segmen; langkah++)
            {
                float t = (float)langkah / Segmen;
                float profil = Mathf.Max(0, 1 - Mathf.Abs(t - .5f) / .23f);
                float tab = Mathf.SmoothStep(0, .145f, profil);
                float tandaX = ((kolom + baris) & 1) == 0 ? 1f : -1f;
                float batasKiri = kolom == 0 ? 0 : -tandaX * tab;
                float batasKanan = kolom == akhir ? 0 : tandaX * tab;
                float batasBawah = baris == 0 ? 0 : tandaX * tab;
                float batasAtas = baris == akhir ? 0 : -tandaX * tab;
                Vector2 titik = sisi switch
                {
                    0 => new Vector2(t, batasBawah),
                    1 => new Vector2(1 + batasKanan, t),
                    2 => new Vector2(1 - t, 1 + batasAtas),
                    _ => new Vector2(batasKiri, 1 - t)
                };
                var p = Vector2.Scale(titik - Vector2.one * .5f, ukuran) * .965f;
                mesh.AddVert(p, rona, new Vector2((kolom + titik.x) / UkuranPapan, (baris + titik.y) / UkuranPapan));
            }
        }
        for (int i = 0; i < 4 * Segmen; i++)
            mesh.AddTriangle(0, 1 + (i + 1) % (4 * Segmen), i + 1);
    }
}

public sealed class SeretJigsawArthur : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Action<Vector2, Camera> Lepas;
    RectTransform petak;
    Vector2 asal, titikAwal;

    public void OnBeginDrag(PointerEventData e)
    {
        petak = (RectTransform)transform;
        asal = petak.anchoredPosition;
        petak.SetAsLastSibling();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)petak.parent, e.pressPosition, e.pressEventCamera, out titikAwal);
    }

    public void OnDrag(PointerEventData e)
    {
        if (petak != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)petak.parent, e.position, e.pressEventCamera, out var titik))
            petak.anchoredPosition = asal + titik - titikAwal;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (petak == null) return;
        petak.anchoredPosition = asal;
        petak = null;
        Lepas?.Invoke(e.position, e.pressEventCamera);
    }
}
