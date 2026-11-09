// PANDUAN PROYEK - Runtime / Ponsel/Tas
// Fungsi: Mengubah drag, tekan, atau scroll ritsleting menjadi perpindahan halaman/item tas.
// Panduan: Assets/Scripts/Runtime/Ponsel/Tas/README.md

using UnityEngine;
using UnityEngine.EventSystems;

// Banyak halaman berasal dari isi tas, bukan jumlah katalog atau slot kosong.
public class RitsletingTasUI : MonoBehaviour,IPointerDownHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,IScrollHandler
{
    public AplikasiCatatanHP tas;
    public RectTransform tarikan;
    public BukaanRitsletingUI bukaan;
    [Range(0,40)]public float margin=22;
    RectTransform Jalur=>(RectTransform)transform;
    public void Segarkan()
    {
        if(tarikan==null||tas==null)return;
        float t=tas.JumlahHalaman<=1?0:tas.Halaman/(float)(tas.JumlahHalaman-1);
        Tampilkan(t);
    }
    void Tampilkan(float t)
    {
        var p=tarikan.anchoredPosition;p.x=Mathf.Lerp(Jalur.rect.xMin+margin,Jalur.rect.xMax-margin,t);tarikan.anchoredPosition=p;
        if(bukaan!=null)bukaan.AturKemajuan(t);
    }
    void Tarik(PointerEventData e)
    {
        if(tas==null||tas.JumlahHalaman<=1){Segarkan();return;}
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(Jalur,e.position,e.pressEventCamera,out var p))return;
        float t=Mathf.InverseLerp(Jalur.rect.xMin+margin,Jalur.rect.xMax-margin,p.x);
        tas.GeserHalaman(Mathf.RoundToInt(t*(tas.JumlahHalaman-1)));Tampilkan(t);
    }
    public void OnPointerDown(PointerEventData e){Tarik(e);}
    public void OnBeginDrag(PointerEventData e){Tarik(e);}
    public void OnDrag(PointerEventData e){Tarik(e);}
    public void OnEndDrag(PointerEventData e){Tarik(e);Segarkan();}
    public void OnScroll(PointerEventData e){if(tas!=null)tas.GeserHalaman(tas.Halaman+(e.scrollDelta.y<0?1:e.scrollDelta.y>0?-1:0));Segarkan();}
}
