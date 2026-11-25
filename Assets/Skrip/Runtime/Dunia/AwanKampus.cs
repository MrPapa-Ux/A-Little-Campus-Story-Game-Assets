using UnityEngine;

public sealed class AwanKampus : MonoBehaviour
{
    public float kecepatan = .22f, bentang = 85;
    public float bentuk;
    Vector3 awal;
    Transform kamera;
    Renderer gambar;
    MaterialPropertyBlock data;
    Color warnaAsli;
    Vector3 skalaAsli;
    float awanTerakhir=-1, mendungTerakhir=-1;
    bool hujanTerakhir;
    void Start()
    {
        awal = transform.localPosition;
        gambar=GetComponent<Renderer>();
        skalaAsli=transform.localScale;
        if(gambar!=null){data=new MaterialPropertyBlock();gambar.GetPropertyBlock(data);data.SetFloat("_Seed",bentuk);warnaAsli=gambar.sharedMaterial!=null&&gambar.sharedMaterial.HasProperty("_Color")?gambar.sharedMaterial.GetColor("_Color"):Color.white;gambar.SetPropertyBlock(data);}
        var a = FindFirstObjectByType<ArthurJalurKampus>();
        if (a != null && a.kamera != null) kamera = a.kamera.transform;
    }
    void LateUpdate()
    {
        var cuaca=SiklusWaktuKampus.Instance;
        float awan=cuaca!=null?cuaca.KepadatanAwan:0, mendung=cuaca!=null?cuaca.KadarMendung:0;
        bool hujan=cuaca!=null&&cuaca.Hujan;
        if(gambar!=null&&(Mathf.Abs(awan-awanTerakhir)>.002f||Mathf.Abs(mendung-mendungTerakhir)>.002f||hujan!=hujanTerakhir))
        {
            Color warna=Color.Lerp(warnaAsli,new Color(.25f,.29f,.34f,warnaAsli.a),mendung);
            warna.a=warnaAsli.a*(hujan?1f:Mathf.Max(awan,1f-mendung));
            data.SetColor("_Color",warna);gambar.SetPropertyBlock(data);
            transform.localScale=skalaAsli*Mathf.Lerp(1f,1.6f,awan);
            awanTerakhir=awan;mendungTerakhir=mendung;hujanTerakhir=hujan;
        }
        var p = awal; p.x += Mathf.PingPong(Time.time * kecepatan, bentang * 2) - bentang;
        transform.localPosition = p;
        if (kamera != null)
        {
            Vector3 d = transform.position - kamera.position; d.y = 0;
            if (d.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(d, Vector3.up);
        }
    }
}
