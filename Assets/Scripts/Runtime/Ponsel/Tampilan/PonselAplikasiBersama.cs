// PANDUAN PROYEK - Runtime / Ponsel/Tampilan
// Fungsi: Menyesuaikan bingkai aplikasi terhadap ukuran layar, safe area, dan warna HP.
// Panduan: Assets/Scripts/Runtime/Ponsel/Tampilan/README.md

using UnityEngine;

// Satu bingkai untuk semua aplikasi. Tidak mengubah tekstur atau metadata sprite HP.
public class PonselAplikasiBersama : MonoBehaviour
{
    public RectTransform layar;
    public CanvasGroup kelompok;
    public BingkaiPonselUI gambar;
    public UnityEngine.UI.Button keluar;
    int lebar,tinggi;
    Rect aman;
    void OnEnable(){Sesuaikan();}
    void Update(){if(lebar!=Screen.width||tinggi!=Screen.height||aman!=Screen.safeArea)Sesuaikan();}
    public static float SkalaPas(Vector2 ruang,Vector2 ukuran)=>Mathf.Min(1,Mathf.Min(Mathf.Max(1,ruang.x-32)/ukuran.x,Mathf.Max(1,ruang.y-32)/ukuran.y));
    public static Rect KotakAman(RectTransform parent)
    {
        var canvas = parent.GetComponentInParent<Canvas>()?.rootCanvas;
        var kamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Screen.safeArea.min, kamera, out var min);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Screen.safeArea.max, kamera, out var max);
        return max.x > min.x && max.y > min.y ? Rect.MinMaxRect(min.x, min.y, max.x, max.y) : parent.rect;
    }

    public static void PasTengah(RectTransform panel, Vector2 bagianLayar)
    {
        if (!(panel.parent is RectTransform parent)) return;
        Rect aman = KotakAman(parent);
        Vector2 ruang = Vector2.Scale(aman.size, bagianLayar);
        Vector2 ukuran = panel.rect.size;
        if (ukuran.x <= 0 || ukuran.y <= 0) return;
        float skala = Mathf.Min(1, ruang.x / ukuran.x, ruang.y / ukuran.y);
        panel.localScale = new Vector3(skala, skala, 1);
        Vector3 posisi = panel.localPosition;
        panel.localPosition = new Vector3(aman.center.x - panel.rect.center.x * skala,
            aman.center.y - panel.rect.center.y * skala, posisi.z);
    }
    public void Sesuaikan()
    {
        lebar=Screen.width;tinggi=Screen.height;aman=Screen.safeArea;
        var r=(RectTransform)transform;var parent=(RectTransform)r.parent;
        if(parent==null)return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,aman.min,null,out var min);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,aman.max,null,out var max);
        if(max.x<=min.x||max.y<=min.y){min=parent.rect.min;max=parent.rect.max;}
        r.localScale=Vector3.one*SkalaPas(max-min,r.rect.size);r.anchoredPosition=(min+max)/2-parent.rect.center;
    }
    public void Warna(Color color){if(gambar!=null)gambar.color=color;}
}
