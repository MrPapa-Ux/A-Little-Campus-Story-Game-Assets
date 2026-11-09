// PANDUAN PROYEK - Runtime / Ponsel/CatatanDanPesan
// Fungsi: Menampilkan daftar/detail tugas, pesan, atau tas; mengurus pilihan, halaman, dan aksi sesuai jenis buku.
// Panduan: Assets/Scripts/Runtime/Ponsel/CatatanDanPesan/README.md

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AplikasiCatatanHP : MonoBehaviour
{
    public SistemHP hp;
    public BukuArthur buku;
    public JenisBukuArthur jenis;
    public TMP_Text detailJudul,detailIsi,jumlah,labelAksi;
    public TMP_Text[] labelBaris;
    public UnityEngine.UI.Image[] latarBaris;
    public UnityEngine.UI.Button[] tombolBaris;
    public UnityEngine.UI.Button aksi;
    public UnityEngine.UI.Image[] ikonBaris;
    public AvatarKontakUI[] avatarBaris;
    public UnityEngine.UI.Image fotoKontak;
    public AvatarKontakUI avatarKontak;
    public TMP_Text statusKontak;
    public Image pratinjauBarang;
    public RitsletingTasUI ritsleting;
    public ScrollRect gulirPesan;
    public Color warnaPilihan = new Color32(209,222,195,255);
    public int Pilihan {get;private set;}
    public int Halaman => Pilihan/Mathf.Max(1,labelBaris.Length);
    public int JumlahHalaman => Mathf.Max(1,Mathf.CeilToInt(buku.Daftar(jenis).Count/(float)Mathf.Max(1,labelBaris.Length)));
    void OnEnable(){LihatPilihan();buku.Berubah+=Segarkan;Segarkan();}
    void OnDisable(){buku.Berubah-=Segarkan;}
    void Update(){if(Input.GetKeyDown(KeyCode.Escape))hp.TutupAplikasi();}
    public void PilihBaris(int index){Pilih(Pilihan/labelBaris.Length*labelBaris.Length+index);}
    public void Kiri(){Pilih(Pilihan-1);}
    public void Kanan(){Pilih(Pilihan+1);}
    void Pilih(int index){var list=buku.Daftar(jenis);if(list.Count==0)return;Pilihan=jenis==JenisBukuArthur.Tas?Mathf.Clamp(index,0,list.Count-1):(index%list.Count+list.Count)%list.Count;LihatPilihan();Segarkan();}
    void LihatPilihan(){if(buku==null)return;var list=buku.Daftar(jenis);if(list.Count==0)return;Pilihan=Mathf.Clamp(Pilihan,0,list.Count-1);if(jenis==JenisBukuArthur.Pesan)list[Pilihan].selesai=true;buku.TandaiDilihat(jenis,list[Pilihan].id);}
    public void GeserHalaman(int halaman)
    {
        if(jenis!=JenisBukuArthur.Tas)return;
        int target=Mathf.Clamp(halaman,0,JumlahHalaman-1);
        if(target!=Halaman)Pilih(target*labelBaris.Length);
    }
    public void Interaksi()
    {
        var list=buku.Daftar(jenis);if(list.Count==0 || jenis==JenisBukuArthur.Tas)return;
        if(list[Pilihan].dikendalikanQuest)return;
        list[Pilihan].selesai=!list[Pilihan].selesai;
        if(jenis==JenisBukuArthur.Pesan){if(list[Pilihan].selesai)buku.TandaiDilihat(jenis,list[Pilihan].id);else buku.TandaiBelumDilihat(jenis,list[Pilihan].id);}buku.Segarkan();
    }
    public void Segarkan()
    {
        var list=buku.Daftar(jenis);Pilihan=Mathf.Clamp(Pilihan,0,Mathf.Max(0,list.Count-1));
        bool empty=list.Count==0;
        int page=Pilihan/labelBaris.Length*labelBaris.Length;
        jumlah.text=empty?(jenis==JenisBukuArthur.Tas?"0 BARANG":jenis==JenisBukuArthur.Pesan?"0 PESAN":"0 CATATAN"):(Pilihan+1)+" / "+list.Count+(jenis==JenisBukuArthur.Tas?" BARANG":jenis==JenisBukuArthur.Pesan?" PESAN":" CATATAN");
        detailJudul.text=empty?(jenis==JenisBukuArthur.Tugas?"BELUM ADA TUGAS":jenis==JenisBukuArthur.Pesan?"BELUM ADA PESAN":"TAS MASIH KOSONG"):Nama(list[Pilihan]);
        detailIsi.text=empty?(jenis==JenisBukuArthur.Tugas?"Untuk sekarang, halaman ini masih bersih.\n\nTugas perjalanan Arthur akan tercatat di sini.":jenis==JenisBukuArthur.Pesan?"":"Belum ada barang yang dibawa."):list[Pilihan].isi+(jenis==JenisBukuArthur.Tas?"\n\nJumlah: "+list[Pilihan].jumlah:"");
        if(gulirPesan!=null){gulirPesan.StopMovement();gulirPesan.verticalNormalizedPosition=1;detailIsi.rectTransform.anchoredPosition=Vector2.zero;}
        if(pratinjauBarang!=null){pratinjauBarang.sprite=empty?null:list[Pilihan].ikon;pratinjauBarang.gameObject.SetActive(pratinjauBarang.sprite!=null);}
        if(ritsleting!=null)ritsleting.Segarkan();
        aksi.interactable=!empty && jenis!=JenisBukuArthur.Tas && !list[Pilihan].dikendalikanQuest;
        labelAksi.text=empty?"BELUM ADA CATATAN":jenis==JenisBukuArthur.Tas?"ISI KANTONG":jenis==JenisBukuArthur.Pesan?(list[Pilihan].selesai?"TANDAI BELUM DIBACA":"TANDAI DIBACA"):(list[Pilihan].selesai?"TANDAI BELUM SELESAI":"TANDAI SELESAI");
        if(!empty&&list[Pilihan].dikendalikanQuest)labelAksi.text="SELESAIKAN LEWAT MISI";
        for(int i=0;i<labelBaris.Length;i++)
        {
            int index=page+i;bool exists=index<list.Count;
            // Tidak menggambar kantong kosong; slot terisi tetap mempunyai kotak seleksi.
            if(jenis==JenisBukuArthur.Tas)tombolBaris[i].gameObject.SetActive(exists);
            tombolBaris[i].interactable=exists;
            labelBaris[i].text=exists?(jenis==JenisBukuArthur.Tugas?(list[index].selesai?"[ X ]  ":"[   ]  "):jenis==JenisBukuArthur.Pesan?(list[index].selesai?"DIBACA  ":"BARU  "):"x"+list[index].jumlah+"  ")+Nama(list[index]):(jenis==JenisBukuArthur.Tas?"":"- - - - - - - -");
            latarBaris[i].color=exists&&index==Pilihan?warnaPilihan:jenis==JenisBukuArthur.Tas?new Color32(202,172,135,255):jenis==JenisBukuArthur.Pesan?new Color32(205,219,181,255):new Color32(237,229,209,255);
            bool hasImage=exists && list[index].ikon!=null && !(jenis==JenisBukuArthur.Pesan && list[index].anonim);
            if(ikonBaris!=null && i<ikonBaris.Length && ikonBaris[i]!=null){ikonBaris[i].sprite=hasImage?list[index].ikon:null;ikonBaris[i].gameObject.SetActive(hasImage);}
            if(avatarBaris!=null && i<avatarBaris.Length && avatarBaris[i]!=null)avatarBaris[i].gameObject.SetActive(!hasImage);
        }
        if(jenis==JenisBukuArthur.Pesan)
        {
            bool photo=!empty&&!list[Pilihan].anonim&&list[Pilihan].ikon!=null;
            if(fotoKontak!=null){fotoKontak.sprite=photo?list[Pilihan].ikon:null;fotoKontak.gameObject.SetActive(photo);}
            if(avatarKontak!=null)avatarKontak.gameObject.SetActive(!photo);
            if(statusKontak!=null)statusKontak.text=empty?"":list[Pilihan].anonim?"Nomor tidak dikenal":list[Pilihan].selesai?"Sudah dibaca":"Pesan baru";
        }
    }
    string Nama(CatatanArthur item)=>jenis!=JenisBukuArthur.Pesan?item.judul:item.anonim?"Anonim":string.IsNullOrWhiteSpace(item.pengirim)?item.judul:item.pengirim;
}
