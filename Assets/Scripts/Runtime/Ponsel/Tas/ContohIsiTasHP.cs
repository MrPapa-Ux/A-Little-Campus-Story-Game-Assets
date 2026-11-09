// PANDUAN PROYEK - Runtime / Ponsel/Tas
// Fungsi: Mengisi barang contoh untuk pengujian; memiliki pengaturan aktif dan hanya di Editor.
// Panduan: Assets/Scripts/Runtime/Ponsel/Tas/README.md

using UnityEngine;

// Contoh visual lokal di CharacterLab; bukan barang hadiah dari cerita.
public class ContohIsiTasHP : MonoBehaviour
{
    public BukuArthur buku;
    [Tooltip("Isi contoh otomatis ketika mencoba scene CharacterLab. Matikan untuk menguji tas kosong.")]
    public bool aktifkanContoh=true;
    [Tooltip("Contoh tidak ikut diberikan pada pemain build Android.")]
    public bool hanyaDiEditor=true;
    public CatatanArthur[] contoh;
    void Start()
    {
        if(!aktifkanContoh || (hanyaDiEditor&&!Application.isEditor) || buku==null || buku.barang.Count!=0)return;
        foreach(var item in contoh)buku.Tambah(JenisBukuArthur.Tas,item);
    }
}
