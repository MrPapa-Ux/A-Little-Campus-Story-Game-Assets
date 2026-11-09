using System;
using UnityEngine;

// Menempelkan sepatu 2D pada tapak tangga asli setelah Animator memperbarui pose.
// Data tapak dibaca dari mesh saat penyuntingan; mesh dan lintasan tidak diubah.
[DefaultExecutionOrder(200)]
public sealed class PijakanTanggaArthur : MonoBehaviour
{
    [Serializable]
    public struct Tapak
    {
        public Vector3 a, b, c;
        public bool Tinggi(Vector3 p, out float y)
        {
            y = a.y;
            float d = (b.x-a.x)*(c.z-a.z)-(b.z-a.z)*(c.x-a.x);
            if (Mathf.Abs(d) < .00001f) return false;
            float u = ((p.x-a.x)*(c.z-a.z)-(p.z-a.z)*(c.x-a.x))/d;
            float v = ((b.x-a.x)*(p.z-a.z)-(b.z-a.z)*(p.x-a.x))/d;
            return u >= -.0001f && v >= -.0001f && u+v <= 1.0001f;
        }
    }
    [Serializable]
    public struct Zona
    {
        public JalurKampus jalur;
        public float awal, akhir;
        public Tapak[] tapak;
    }
    public ArthurJalurKampus gerak;
    public Transform visual;
    public SpriteRenderer kakiKiri, kakiKanan;
    public Zona[] zona;
    Vector3 koreksiVisual, koreksiKiri, koreksiKanan;

    void Update() => Pulihkan();
    void OnDisable() => Pulihkan();
    void Pulihkan()
    {
        if (visual != null && koreksiVisual != Vector3.zero) visual.localPosition -= koreksiVisual;
        if (kakiKiri != null && koreksiKiri != Vector3.zero) kakiKiri.transform.localPosition -= koreksiKiri;
        if (kakiKanan != null && koreksiKanan != Vector3.zero) kakiKanan.transform.localPosition -= koreksiKanan;
        koreksiVisual = koreksiKiri = koreksiKanan = Vector3.zero;
    }
    void LateUpdate()
    {
        if (gerak == null || gerak.JalurAktif == null || visual == null || zona == null) return;
        foreach (var z in zona)
        {
            if (z.jalur != gerak.JalurAktif || gerak.JarakDiJalur < z.awal || gerak.JarakDiJalur > z.akhir) continue;
            float kiri = Angkat(kakiKiri, z), kanan = Angkat(kakiKanan, z);
            // Angkat badan bersama kedua sepatu jika keduanya perlu naik, lalu
            // letakkan masing-masing sepatu pada anak tangga yang diinjaknya.
            float bersama = Mathf.Min(kiri, kanan);
            Geser(visual, bersama, out koreksiVisual);
            if (kakiKiri != null) Geser(kakiKiri.transform, kiri-bersama, out koreksiKiri);
            if (kakiKanan != null) Geser(kakiKanan.transform, kanan-bersama, out koreksiKanan);
            break;
        }
    }
    float Angkat(SpriteRenderer kaki, Zona z)
    {
        if (kaki == null || kaki.sprite == null || z.tapak == null) return 0;
        var b = kaki.sprite.bounds;
        float lantai = float.NegativeInfinity;
        float alas = float.PositiveInfinity;
        for (int i=0; i<=8; i++)
        {
            // Pakai tepi bawah sepatu dalam bidang sprite, bukan ketebalan
            // bounds renderer yang dapat melebar saat Arthur berbelok.
            var p = kaki.transform.TransformPoint(new Vector3(Mathf.Lerp(b.min.x,b.max.x,i/8f),b.min.y,b.center.z));
            alas = Mathf.Min(alas,p.y);
            foreach (var t in z.tapak)
                if (t.Tinggi(p,out float y)) lantai = Mathf.Max(lantai,y);
        }
        return float.IsNegativeInfinity(lantai) ? 0 : Mathf.Clamp(lantai+.025f-alas,0,1.1f);
    }
    static void Geser(Transform t, float tinggi, out Vector3 koreksi)
    {
        koreksi = t.parent != null ? t.parent.InverseTransformVector(Vector3.up*tinggi) : Vector3.up*tinggi;
        t.localPosition += koreksi;
    }
}
