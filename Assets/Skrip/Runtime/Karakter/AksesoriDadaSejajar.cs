using UnityEngine;

// Tetap mengikuti dada saat badan bergerak, dengan kemiringan yang tenang.
[DefaultExecutionOrder(500)]
public sealed class AksesoriDadaSejajar : MonoBehaviour
{
    public SpriteRenderer badan;
    public Transform kerangka;
    [Range(-.2f,.3f)] public float naikDariTengah = .10f;
    [Range(-.3f,.3f)] public float geserMendatar;
    public float koreksiSudut = -13f;
    [Header("Tambatan leher Arthur")]
    public bool tambatKeLeher;
    [Tooltip("Titik leher pada gambar baju, dalam satuan sprite. Mengikuti kemiringan badan saat jalan dan lari.")]
    public Vector2 titikLeherSprite = new Vector2(.053f, .374f);
    [Tooltip("Pusat lubang tali dalam gambar lanyard, dari kiri bawah.")]
    public Vector2 pusatLubangTali = new Vector2(.42f, .75f);
    [Range(.3f,1.2f)] public float ukuranTerhadapBadan = .96f;
    SpriteRenderer gambar;

    void LateUpdate() => Selaraskan();
    public void Selaraskan()
    {
        if (badan == null || badan.sprite == null || kerangka == null) return;
        if (tambatKeLeher) { IkutiLeher(); return; }
        var batas = badan.sprite.bounds;
        float tinggi = badan.transform.TransformVector(Vector3.up * batas.size.y).magnitude;
        transform.position = badan.transform.TransformPoint(batas.center)
            + kerangka.up * (tinggi * naikDariTengah)
            + kerangka.TransformVector(Vector3.right).normalized * (tinggi * geserMendatar)
            - kerangka.forward * .032f;
        transform.rotation = kerangka.rotation * Quaternion.Euler(0, 0, koreksiSudut);
    }

    void IkutiLeher()
    {
        if (gambar == null) gambar = GetComponent<SpriteRenderer>();
        if (gambar == null || gambar.sprite == null) return;
        // Satu induk pencerminan; rotasi badan tidak lagi mencerminkan tali dua kali.
        if (transform.parent != kerangka) transform.SetParent(kerangka, true);
        var titik = new Vector3(titikLeherSprite.x, titikLeherSprite.y, 0);
        if (badan.flipX) titik.x = -titik.x;
        if (badan.flipY) titik.y = -titik.y;
        var leher = kerangka.InverseTransformPoint(badan.transform.TransformPoint(titik));
        var batas = gambar.sprite.bounds;
        float skala = badan.sprite.bounds.size.y * Mathf.Abs(badan.transform.localScale.y)
            * ukuranTerhadapBadan / Mathf.Max(.001f, batas.size.y);
        gambar.flipX = badan.flipX;
        gambar.flipY = badan.flipY;
        var lubang = batas.min + new Vector3(batas.size.x * pusatLubangTali.x, batas.size.y * pusatLubangTali.y, 0);
        lubang.z = 0;
        if (gambar.flipX) lubang.x = -lubang.x;
        if (gambar.flipY) lubang.y = -lubang.y;
        var putar = Quaternion.Euler(0, 0, badan.flipX ? -koreksiSudut : koreksiSudut);
        transform.localScale = Vector3.one * skala;
        transform.localRotation = putar;
        transform.localPosition = leher - putar * (lubang * skala) - Vector3.forward * .0064f;
    }
}
