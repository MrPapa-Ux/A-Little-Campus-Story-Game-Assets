using System;
using UnityEngine;

// Requests are accepted and reported through the existing Contacts screen.
public sealed class QuestKontakArthur : MonoBehaviour
{
    [Serializable] public sealed class Misi
    {
        public string id, pengirim, judul, permintaan, laporan, balasan, namaHadiah, deskripsiHadiah;
        public Sprite ikon;
        public NpcYangBisaDiatur tujuan;
        [NonSerialized] public int tahap;
    }
    public Misi[] misi;
    public BukuArthur buku;
    SistemPilihan dialog;
    void Start()
    {
        if(buku==null)buku=FindFirstObjectByType<BukuArthur>();
        dialog=FindFirstObjectByType<SistemPilihan>();
        if(dialog!=null)dialog.PercakapanSelesai+=SelesaiBicara;
        foreach(var m in misi){m.tahap=ProgresSampingArthur.TahapKontak(gameObject.scene,m.id);Catat(m,false);}
    }
    void OnDestroy(){if(dialog!=null)dialog.PercakapanSelesai-=SelesaiBicara;}
    public Misi Cari(string id)=>Array.Find(misi,m=>m.id==id);
    public bool Aksi(string id)
    {
        var m=Cari(id);if(m==null||m.tahap==1||m.tahap==3)return false;
        if(m.tahap==0)m.tahap=1;
        else {m.tahap=3;buku.Tambah(JenisBukuArthur.Tas,new CatatanArthur{id="hadiah_"+m.id,judul=m.namaHadiah,isi=m.deskripsiHadiah,ikon=m.ikon});}
        Catat(m,true);return true;
    }
    void SelesaiBicara(AreaInteraksi area,bool tuntas)
    {
        if(!tuntas)return;
        foreach(var m in misi)if(m.tahap==1&&m.tujuan!=null&&m.tujuan.GetComponent<AreaInteraksi>()==area){m.tahap=2;Catat(m,true);}
    }
    void Catat(Misi m,bool simpan)
    {
        string obrolan=m.pengirim+": "+m.permintaan;
        if(m.tahap>0)obrolan+="\n\nArthur: Oke, kubantu. Nanti kukabari.";
        if(m.tahap==2)obrolan+="\n\nSudah kutemui. Tinggal mengirim kabar.";
        if(m.tahap==3)obrolan+="\n\nArthur: "+m.laporan+"\n\n"+m.pengirim+": "+m.balasan;
        buku.Tambah(JenisBukuArthur.Pesan,new CatatanArthur{id=m.id,pengirim=m.pengirim,judul=m.judul,isi=obrolan,dikendalikanQuest=true});
        if(m.tahap>0)buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{id=m.id,judul=m.judul,isi=m.tahap==1?m.permintaan:m.tahap==2?"Buka Kontak dan kabari "+m.pengirim+".":"Sudah dikabari. Selesai.",dikendalikanQuest=true,selesai=m.tahap==3});
        if(m.tahap==3&&!buku.barang.Exists(b=>b.id=="hadiah_"+m.id))buku.Tambah(JenisBukuArthur.Tas,new CatatanArthur{id="hadiah_"+m.id,judul=m.namaHadiah,isi=m.deskripsiHadiah,ikon=m.ikon},false);
        if(simpan)ProgresSampingArthur.SimpanKontak(m.id,m.tahap);
    }
    public static QuestKontakArthur Untuk(BukuArthur buku)
    {
        foreach(var q in FindObjectsByType<QuestKontakArthur>(FindObjectsSortMode.None))if(q.buku==buku)return q;
        return null;
    }
}
