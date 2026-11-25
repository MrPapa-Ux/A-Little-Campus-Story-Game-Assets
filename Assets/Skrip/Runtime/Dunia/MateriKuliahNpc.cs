using UnityEngine;

// Materi ditulis pada AreaInteraksi sehingga pemilik game bisa mengedit setiap baris.
[RequireComponent(typeof(AreaInteraksi))]
public sealed class MateriKuliahNpc : MonoBehaviour
{
    public string idCatatan, judulCatatan;
    [TextArea(3, 12)] public string ringkasan;
    public BukuArthur buku;
    SistemPilihan pilihan;
    AreaInteraksi area;
    void OnEnable()
    {
        area = GetComponent<AreaInteraksi>();
        pilihan = FindFirstObjectByType<SistemPilihan>();
        if (pilihan != null) pilihan.PercakapanSelesai += Selesai;
    }
    void OnDisable() { if (pilihan != null) pilihan.PercakapanSelesai -= Selesai; }
    void Selesai(AreaInteraksi pemilik, bool tuntas)
    {
        if (pemilik != area || !tuntas || buku == null || string.IsNullOrWhiteSpace(idCatatan)) return;
        buku.Tambah(JenisBukuArthur.Tugas, new CatatanArthur {
            id = idCatatan, judul = judulCatatan, isi = ringkasan, selesai = true
        });
    }
}
