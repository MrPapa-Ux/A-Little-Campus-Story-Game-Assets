using TMPro;
using UnityEngine;

public sealed class NotifikasiMomenTerekam : MonoBehaviour
{
    public RectTransform parameterBar;
    public TMP_Text teks;
    public CanvasGroup grup;
    [Tooltip("Bagian gambar bar yang terlihat, tanpa ruang transparan aset.")]
    public Rect bagianTerlihat=new Rect(0,0,1,1);
    [Tooltip("Geser posisi tulisan: X positif ke kanan, Y positif ke atas. Bisa disesuaikan tanpa memindahkan bar.")]
    public Vector2 geserPosisi=new Vector2(0,-12);
    [Min(.5f)] public float durasi=3f;
    [Min(.1f)] public float durasiPudar=.6f;
    float akhir;
    readonly Vector3[] sudut=new Vector3[4];
    void Awake(){grup.alpha=0;}
    public void Tampilkan(int nomor){teks.text="Momen "+nomor+" terekam";akhir=Time.unscaledTime+durasi;Canvas.ForceUpdateCanvases();Posisikan();grup.alpha=1;}
    [ContextMenu("Coba tampilan Momen 1")]
    public void CobaTampilan(){Tampilkan(1);}
    void LateUpdate(){
        float sisa=akhir-Time.unscaledTime;grup.alpha=Mathf.Clamp01(sisa/durasiPudar);
        if(grup.alpha<=0)return;
        Posisikan();
    }
    void Posisikan(){
        if(parameterBar==null)return;
        parameterBar.GetWorldCorners(sudut);var asal=parameterBar.GetComponentInParent<Canvas>();
        var cam=asal!=null&&asal.renderMode!=RenderMode.ScreenSpaceOverlay?asal.worldCamera:null;
        Vector3 titik=sudut[0]+(sudut[3]-sudut[0])*bagianTerlihat.center.x+(sudut[1]-sudut[0])*bagianTerlihat.yMin;
        Vector2 posisi=RectTransformUtility.WorldToScreenPoint(cam,titik);
        var r=teks.rectTransform;var parent=r.parent as RectTransform;
        var tujuan=r.GetComponentInParent<Canvas>();
        var kameraTujuan=tujuan!=null&&tujuan.renderMode!=RenderMode.ScreenSpaceOverlay?tujuan.worldCamera:null;
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,posisi,kameraTujuan,out var local))r.position=parent.TransformPoint(local+geserPosisi);
    }
}
