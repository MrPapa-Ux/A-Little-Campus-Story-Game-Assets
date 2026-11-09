// PANDUAN PROYEK - Runtime / Ponsel/CatatanDanPesan
// Fungsi: Menyimpan daftar tugas, pesan, dan barang Arthur, serta memberi notifikasi saat data berubah.
// Panduan: Assets/Scripts/Runtime/Ponsel/CatatanDanPesan/README.md

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CatatanArthur
{
    public string id, judul;
    public Sprite ikon;
    public string pengirim;
    public bool anonim;
    [TextArea(2,6)] public string isi;
    public bool selesai;
    public bool dikendalikanQuest;
    [Min(1)] public int jumlah = 1;
}
public enum JenisBukuArthur { Tugas, Pesan, Tas }

public class BukuArthur : MonoBehaviour
{
    public List<CatatanArthur> tugas=new List<CatatanArthur>();
    public List<CatatanArthur> pesan=new List<CatatanArthur>();
    public List<CatatanArthur> barang=new List<CatatanArthur>();
    public event Action Berubah;
    // Session-only knowledge: never save quest progress or read state in PlayerPrefs.
    readonly HashSet<string> sudahDilihat=new HashSet<string>();
    string Kunci(JenisBukuArthur jenis,string id)=>jenis+":"+id;
    public bool AdaYangBaru(JenisBukuArthur jenis)=>Daftar(jenis).Exists(x=>x!=null&&!string.IsNullOrWhiteSpace(x.id)&&!sudahDilihat.Contains(Kunci(jenis,x.id))&&(jenis!=JenisBukuArthur.Pesan||!x.selesai));
    public void TandaiDilihat(JenisBukuArthur jenis,string id){if(!string.IsNullOrWhiteSpace(id)&&sudahDilihat.Add(Kunci(jenis,id)))Segarkan();}
    public void TandaiBelumDilihat(JenisBukuArthur jenis,string id){if(sudahDilihat.Remove(Kunci(jenis,id)))Segarkan();}
    public List<CatatanArthur> Daftar(JenisBukuArthur jenis)=>jenis==JenisBukuArthur.Tugas?tugas:jenis==JenisBukuArthur.Pesan?pesan:barang;
    public void Tambah(JenisBukuArthur jenis,CatatanArthur catatan)
    {
        if(catatan==null || string.IsNullOrWhiteSpace(catatan.id))return;
        var list=Daftar(jenis);int found=list.FindIndex(x=>x.id==catatan.id);
        bool berubah=found<0||list[found].judul!=catatan.judul||list[found].isi!=catatan.isi||list[found].jumlah!=catatan.jumlah||list[found].pengirim!=catatan.pengirim;
        if(berubah)sudahDilihat.Remove(Kunci(jenis,catatan.id));
        if(found>=0)list[found]=catatan;else list.Add(catatan);Segarkan();
    }
    public void SelesaikanTugas(string id){var item=tugas.Find(x=>x.id==id);if(item!=null){item.selesai=true;Segarkan();}}
    public void Segarkan(){Berubah?.Invoke();}
}
