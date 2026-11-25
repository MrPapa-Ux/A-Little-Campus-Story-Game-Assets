using UnityEngine;

[RequireComponent(typeof(AreaInteraksi))]
public sealed class BarangLepasArthur : MonoBehaviour
{
    public BukuArthur buku;
    public string idBarang="explorer_catatan_kuliah",namaBarang="Catatan kuliah";
    [TextArea]public string deskripsi="Selembar catatan kuliah yang ditemukan Arthur.";
    public Sprite ikon;
    public GameObject visual;
    public bool SudahDiambil {get;private set;}
    SistemPilihan pilihan;AreaInteraksi area;
    void Start(){area=GetComponent<AreaInteraksi>();pilihan=FindFirstObjectByType<SistemPilihan>();if(pilihan!=null)pilihan.PercakapanSelesai+=Selesai;}
    void Selesai(AreaInteraksi a,bool tuntas){if(a!=area||!tuntas||SudahDiambil||buku==null)return;SudahDiambil=true;buku.Tambah(JenisBukuArthur.Tas,new CatatanArthur{id=idBarang,judul=namaBarang,isi=deskripsi,ikon=ikon,jumlah=1});if(visual!=null)visual.SetActive(false);gameObject.SetActive(false);}
    void OnDestroy(){if(pilihan!=null)pilihan.PercakapanSelesai-=Selesai;}
}
