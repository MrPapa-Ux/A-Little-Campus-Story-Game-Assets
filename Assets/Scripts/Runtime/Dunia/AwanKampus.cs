using UnityEngine;

public sealed class AwanKampus : MonoBehaviour
{
    public float kecepatan = .22f, bentang = 85;
    public float bentuk;
    Vector3 awal;
    Transform kamera;
    void Start()
    {
        awal = transform.localPosition;
        var gambar=GetComponent<Renderer>();
        if(gambar!=null){var data=new MaterialPropertyBlock();gambar.GetPropertyBlock(data);data.SetFloat("_Seed",bentuk);gambar.SetPropertyBlock(data);}
        var a = FindFirstObjectByType<ArthurJalurKampus>();
        if (a != null && a.kamera != null) kamera = a.kamera.transform;
    }
    void LateUpdate()
    {
        var p = awal; p.x += Mathf.PingPong(Time.time * kecepatan, bentang * 2) - bentang;
        transform.localPosition = p;
        if (kamera != null)
        {
            Vector3 d = transform.position - kamera.position; d.y = 0;
            if (d.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(d, Vector3.up);
        }
    }
}
