using UnityEngine;

// Scale the direction wrapper, outside the animated/mirrored visual hierarchy.
// Physics, camera and UI retain their original scale; the feet are the scale pivot.
[DefaultExecutionOrder(175)]
[RequireComponent(typeof(ArthurJalurKampus))]
public sealed class UkuranArthurPadaJalur : MonoBehaviour
{
    ArthurJalurKampus pemain;
    Transform visual;
    Vector3 ukuranAwal, posisiAwal;
    JalurKampus jalurTerakhir;
    float terakhir = -1;
    bool siap;
    void Awake()
    {
        pemain = GetComponent<ArthurJalurKampus>(); visual = pemain.wadahArahVisual;
        if (visual == null || visual == transform || (pemain.kamera != null && pemain.kamera.transform.IsChildOf(visual))) { enabled=false; return; }
        ukuranAwal=visual.localScale; posisiAwal=visual.localPosition; siap=true;
    }
    void LateUpdate()
    {
        var route=pemain.JalurAktif;
        float factor=route!=null ? Mathf.Clamp(route.ukuranVisualArthur,.4f,1.2f) : 1f;
        if (Mathf.Approximately(factor,terakhir) && route==jalurTerakhir) return;
        var kaki=visual.parent.InverseTransformPoint(transform.position-Vector3.up*ArthurJalurKampus.JarakKaki);
        visual.localScale=ukuranAwal*factor;
        visual.localPosition=kaki+(posisiAwal-kaki)*factor;
        terakhir=factor; jalurTerakhir=route;
        if(route!=null && (route.name=="GK1_RuanganDekanFilkom" || route.name=="GK1_RuanganDosen"))
        {
            var kepala=GetComponent<KetukKepalaArthur>();
            if(kepala!=null && kepala.kepala!=null)
                foreach(var marker in route.GetComponentsInChildren<InteraksiJalurKampus>(true))
                    marker.tinggiKepalaDiTitik=kepala.kepala.bounds.max.y-transform.position.y;
        }
    }
    void OnDisable(){if(siap&&visual!=null){visual.localScale=ukuranAwal;visual.localPosition=posisiAwal;}terakhir=-1;}
}
