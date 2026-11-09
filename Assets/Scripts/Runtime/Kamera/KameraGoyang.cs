// PANDUAN PROYEK - Runtime / Kamera
// Fungsi: Ayunan vertikal kecil hanya selama Arthur benar-benar berlari.
// Panduan: Assets/Scripts/Runtime/Kamera/README.md

using UnityEngine;

[DefaultExecutionOrder(300)]
public class KameraGoyang : MonoBehaviour
{
    [Header("Tarik Arthur_Player ke kotak ini!")]
    public Rigidbody2D rbArthur;
    
    [Header("Pengaturan Goyangan")]
    public float bobSpeed = 14f;
    public float bobAmount = 0.05f;

    public float OffsetSaatIni { get; private set; }
    private float timer, kekuatan;
    private KarakterController arthur;
    private ArthurJalurKampus jalur;
    private CascadeTransition transisi;

    void Awake()
    {
        arthur = rbArthur != null ? rbArthur.GetComponent<KarakterController>() : GetComponentInParent<KarakterController>();
        if (arthur != null)
        {
            jalur = arthur.GetComponent<ArthurJalurKampus>();
            if (rbArthur == null) rbArthur = arthur.GetComponent<Rigidbody2D>();
        }
        var pilihan = FindFirstObjectByType<SistemPilihan>();
        if (pilihan != null) transisi = pilihan.efekTransisi;
    }

    void Update()
    {
        // Lepaskan offset frame lalu sebelum pengaturan kamera dasar menghitung bingkai baru.
        LepaskanOffset();
    }

    void LateUpdate()
    {
        if (arthur == null || Time.timeScale <= 0 || arthur.kunciPergerakan || (transisi != null && transisi.SedangBerjalan))
        { AturUlang(); return; }
        bool diJalur = jalur != null && jalur.JalurAktif != null;
        bool bergerak = diJalur ? jalur.KecepatanAktual > .1f && jalur.DiTanah
            : rbArthur != null && rbArthur.simulated && Mathf.Abs(rbArthur.linearVelocity.x) > .1f;
        bool lari = arthur.SedangBerlari && bergerak;
        kekuatan = Mathf.MoveTowards(kekuatan, lari ? 1 : 0, Time.deltaTime * 8);
        if (lari) timer += Time.deltaTime * bobSpeed;
        if (kekuatan == 0) timer = 0;
        // Tambahkan setelah kamera jalur selesai. Offset tidak masuk cache penghalusan,
        // tidak mengubah arah/X/Z, dan tetap tersedia pada kamera tetap seperti kantin.
        OffsetSaatIni = Mathf.Sin(timer) * bobAmount * kekuatan;
        transform.position += Vector3.up * OffsetSaatIni;
    }

    void LepaskanOffset()
    {
        transform.position -= Vector3.up * OffsetSaatIni;
        OffsetSaatIni = 0;
    }
    public void AturUlang()
    {
        LepaskanOffset(); timer = kekuatan = 0;
    }
    void OnDisable() => AturUlang();
}
