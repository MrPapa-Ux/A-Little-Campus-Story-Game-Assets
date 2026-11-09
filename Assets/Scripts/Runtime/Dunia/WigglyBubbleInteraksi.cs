using UnityEngine;

// Visual jitter only: the interaction trigger and its destination never move.
[DisallowMultipleComponent]
[RequireComponent(typeof(InteraksiJalurKampus))]
[DefaultExecutionOrder(250)]
public sealed class WigglyBubbleInteraksi : MonoBehaviour
{
    [Range(0,3)] public float kekuatanPiksel=1.6f;
    [Range(1,15)] public float kecepatanGoyang=8f;
    InteraksiJalurKampus penanda;
    Camera kamera;
    Vector3 geserTerakhir;
    Vector2 goyang;
    float timer;
    void Start(){penanda=GetComponent<InteraksiJalurKampus>();var a=FindFirstObjectByType<ArthurJalurKampus>();if(a!=null)kamera=a.kamera;}
    void Pulihkan(){if(geserTerakhir==Vector3.zero)return;if(penanda!=null&&penanda.ikon!=null)penanda.ikon.position-=geserTerakhir;geserTerakhir=Vector3.zero;}
    void Update(){Pulihkan();}
    void LateUpdate()
    {
        if(penanda==null||!penanda.SedangDitampilkan||penanda.ikon==null||kamera==null||penanda.ikon.localScale.sqrMagnitude<.00001f){timer=0;goyang=Vector2.zero;return;}
        timer+=Time.deltaTime;
        if(timer>=1f/Mathf.Max(1,kecepatanGoyang)){timer=0;goyang=new Vector2(Random.Range(-kekuatanPiksel,kekuatanPiksel),Random.Range(-kekuatanPiksel,kekuatanPiksel));}
        var titik=penanda.ikon.position;var layar=kamera.WorldToScreenPoint(titik);if(layar.z<=0)return;
        float skala=kamera.pixelHeight/720f;layar.x+=goyang.x*skala;layar.y+=goyang.y*skala;
        var baru=kamera.ScreenToWorldPoint(layar);geserTerakhir=baru-titik;penanda.ikon.position=baru;
    }
    void OnDisable(){Pulihkan();timer=0;goyang=Vector2.zero;}
}
