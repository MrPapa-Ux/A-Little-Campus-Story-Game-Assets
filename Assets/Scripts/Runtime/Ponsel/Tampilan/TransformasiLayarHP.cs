// PANDUAN PROYEK - Runtime / Ponsel/Tampilan
// Fungsi: Mengubah HP portrait menjadi landscape dan mengembalikannya dengan animasi.
// Panduan: Assets/Scripts/Runtime/Ponsel/Tampilan/README.md

using System.Collections;
using UnityEngine;

// Memakai panel HP yang sama: ikon ikut bergerak, tanpa salinan Canvas setiap klik.
public class TransformasiLayarHP : MonoBehaviour
{
    public RectTransform ponsel, layar;
    public UnityEngine.UI.Image badan;
    [Tooltip("Area bingkai nyata dalam aset HP, di luar padding transparan")]
    public Vector2 fraksiBingkai = new Vector2(.49f,.82f);
    public CanvasGroup kelompok;
    public PonselAplikasiBersama bingkaiBersama;
    [Range(.15f,.8f)] public float durasi = .25f;
    [Range(0f,.8f)] public float jedaTV = 0f;
    public bool SedangBerjalan { get; private set; }
    readonly Vector3[] corners = new Vector3[4];
    Vector3 awalPos, awalScale;
    Vector3 offsetLayar, awalCenter;
    Quaternion awalRot;

    public IEnumerator KeLandscape(RectTransform panel)
    {
        SedangBerjalan = true;
        Canvas.ForceUpdateCanvases();
        awalPos=ponsel.localPosition; awalScale=ponsel.localScale; awalRot=ponsel.localRotation;
        kelompok.blocksRaycasts=false; kelompok.interactable=false;
        panel.GetWorldCorners(corners);
        var parent=(RectTransform)ponsel.parent;
        Vector3 min=parent.InverseTransformPoint(corners[0]), max=parent.InverseTransformPoint(corners[2]);
        Vector3 center=(min+max)*.5f;
        Vector3 offset=ponsel.InverseTransformPoint(layar.TransformPoint(layar.rect.center));
        offsetLayar=offset;awalCenter=awalPos+awalRot*Vector3.Scale(offset,awalScale);
        Vector3 size=layar.rect.size;
        if(badan!=null)
        {
            Vector2 body=badan.rectTransform.rect.size;
            if(badan.preserveAspect && badan.sprite!=null)
            {
                float ratio=badan.sprite.rect.width/badan.sprite.rect.height;
                if(body.x/body.y>ratio)body.x=body.y*ratio;else body.y=body.x/ratio;
            }
            size=new Vector3(body.x*fraksiBingkai.x,body.y*fraksiBingkai.y,1);
        }
        Quaternion rot=Quaternion.Euler(0,0,-90);
        Vector3 scale=new Vector3((max.y-min.y)/size.x,(max.x-min.x)/size.y,1);
        Vector3 pos=center-rot*Vector3.Scale(offset,scale);
        float elapsed=0;
        while(elapsed<durasi)
        {
            elapsed+=Time.unscaledDeltaTime;float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/durasi));
            ponsel.localRotation=Quaternion.Slerp(awalRot,rot,t);
            ponsel.localScale=Vector3.Lerp(awalScale,scale,t);
            ponsel.localPosition=Vector3.Lerp(awalCenter,center,t)-ponsel.localRotation*Vector3.Scale(offset,ponsel.localScale);
            kelompok.alpha=1f;
            yield return null;
        }
        ponsel.localPosition=pos;ponsel.localRotation=rot;ponsel.localScale=scale;
        elapsed=0;
        while(elapsed<jedaTV)
        {
            elapsed+=Time.unscaledDeltaTime;
            kelompok.alpha=1f-Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed/Mathf.Max(.001f,jedaTV)-.6f)/.4f));
            yield return null;
        }
        if(bingkaiBersama!=null)
        {
            bingkaiBersama.gameObject.SetActive(true);bingkaiBersama.Sesuaikan();bingkaiBersama.kelompok.alpha=0;
            elapsed=0;while(elapsed<.10f){elapsed+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(elapsed/.10f);kelompok.alpha=1-t;bingkaiBersama.kelompok.alpha=t;yield return null;}
            bingkaiBersama.kelompok.alpha=1;
        }
        kelompok.alpha=0;
        ponsel.gameObject.SetActive(false);SedangBerjalan=false;
    }

    public IEnumerator KePortrait()
    {
        SedangBerjalan=true;ponsel.gameObject.SetActive(true);
        Vector3 pos=ponsel.localPosition,scale=ponsel.localScale;Quaternion rot=ponsel.localRotation;
        Vector3 center=pos+rot*Vector3.Scale(offsetLayar,scale);
        if(bingkaiBersama!=null)
        {
            float fade=0;while(fade<.10f){fade+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(fade/.10f);bingkaiBersama.kelompok.alpha=1-t;kelompok.alpha=t;yield return null;}
            bingkaiBersama.gameObject.SetActive(false);
        }
        float elapsed=0;
        while(elapsed<durasi)
        {
            elapsed+=Time.unscaledDeltaTime;float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/durasi));
            ponsel.localScale=Vector3.Lerp(scale,awalScale,t);
            ponsel.localRotation=Quaternion.Slerp(rot,awalRot,t);
            ponsel.localPosition=Vector3.Lerp(center,awalCenter,t)-ponsel.localRotation*Vector3.Scale(offsetLayar,ponsel.localScale);
            kelompok.alpha=t;yield return null;
        }
        ponsel.localPosition=awalPos;ponsel.localScale=awalScale;ponsel.localRotation=awalRot;
        kelompok.alpha=1;kelompok.blocksRaycasts=true;kelompok.interactable=true;SedangBerjalan=false;
    }
}
