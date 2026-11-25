using UnityEngine;

public sealed class NotifikasiBukuHP : MonoBehaviour
{
    public BukuArthur buku;
    public GameObject tugasBaru,pesanBaru,barangBaru,ringkasanHP;
    void OnEnable(){if(buku!=null)buku.Berubah+=Segarkan;Segarkan();}
    void OnDisable(){if(buku!=null)buku.Berubah-=Segarkan;}
    public void Segarkan(){
        bool tugas=buku!=null&&buku.AdaYangBaru(JenisBukuArthur.Tugas),pesan=buku!=null&&buku.AdaYangBaru(JenisBukuArthur.Pesan),barang=buku!=null&&buku.AdaYangBaru(JenisBukuArthur.Tas);
        if(tugasBaru!=null)tugasBaru.SetActive(tugas);if(pesanBaru!=null)pesanBaru.SetActive(pesan);if(barangBaru!=null)barangBaru.SetActive(barang);if(ringkasanHP!=null)ringkasanHP.SetActive(tugas||pesan||barang);
    }
}
