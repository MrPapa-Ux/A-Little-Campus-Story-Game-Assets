using System;
using UnityEngine;

public enum TahapTitipanArthur { BelumDiterima, CariBarang, BawaKembali, Selesai }

// Status hidup bersama CharacterLab. Pergantian kawasan tidak menggandakan barang.
// Ini quest eksplorasi percobaan; tidak mengaktifkan atau menamatkan Chapter 1.
public sealed class QuestTitipanArthur : MonoBehaviour
{
    public string idQuest, idBarang, judul, namaBarang, pemberi;
    [TextArea] public string petunjukLokasi;
    public Sprite ikon;
    public BukuArthur buku;
    public TitikQuestTitipanArthur npc, barang;
    public TahapTitipanArthur Tahap { get; private set; }
    SistemPilihan pilihan;
    void Start()
    {
        pilihan = FindFirstObjectByType<SistemPilihan>();
        if (pilihan != null) pilihan.PercakapanSelesai += PercakapanSelesai;
        Segarkan();
    }
    void OnDestroy() { if (pilihan != null) pilihan.PercakapanSelesai -= PercakapanSelesai; }
    void PercakapanSelesai(AreaInteraksi area, bool tuntas)
    {
        if (!tuntas) return;
        if (npc != null && area == npc.GetComponent<AreaInteraksi>()) {
            if (Tahap == TahapTitipanArthur.BelumDiterima) Terima();
            else if (Tahap == TahapTitipanArthur.BawaKembali) Serahkan();
        }
        else if (barang != null && area == barang.GetComponent<AreaInteraksi>()) Ambil();
    }
    public bool Terima()
    {
        if (Tahap != TahapTitipanArthur.BelumDiterima || buku == null) return false;
        Tahap = TahapTitipanArthur.CariBarang; Catat(); Segarkan(); return true;
    }
    public bool Ambil()
    {
        if (Tahap != TahapTitipanArthur.CariBarang || buku == null) return false;
        buku.Tambah(JenisBukuArthur.Tas, new CatatanArthur {
            id = idBarang, judul = namaBarang, ikon = ikon, jumlah = 1,
            isi = "Titipan " + pemberi + ". Kembalikan kepada pemiliknya."
        });
        Tahap = TahapTitipanArthur.BawaKembali; Catat(); Segarkan(); return true;
    }
    public bool Serahkan()
    {
        if (Tahap != TahapTitipanArthur.BawaKembali || buku == null || !buku.barang.Exists(x => x != null && x.id == idBarang)) return false;
        buku.barang.RemoveAll(x => x != null && x.id == idBarang);
        Tahap = TahapTitipanArthur.Selesai; Catat(); Segarkan(); return true;
    }
    void Catat()
    {
        buku.Tambah(JenisBukuArthur.Tugas, new CatatanArthur {
            id = idQuest, judul = judul, dikendalikanQuest = true,
            selesai = Tahap == TahapTitipanArthur.Selesai,
            isi = Tahap == TahapTitipanArthur.CariBarang ? petunjukLokasi :
                Tahap == TahapTitipanArthur.BawaKembali ? "Barang sudah ada di Tas. Kembali ke " + pemberi + "." :
                "Selesai. " + namaBarang + " sudah diserahkan kepada " + pemberi + "."
        });
    }
    public void Segarkan()
    {
        npc?.Segarkan(); barang?.Segarkan();
    }
}
