using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum JenisTantanganArthur { Puzzle, Buah, Joget, Hitung, Foto, Jigsaw, Album, Hadiah }

public sealed class TantanganSampingArthur : MonoBehaviour
{
    public JenisTantanganArthur jenis;
    public string namaNpc;
    public string hadiah;
    public Sprite ikonHadiah;
    public Texture2D gambar;
    public Texture2D[] buah;
    public AreaInteraksi area;
    public BukuArthur buku;
    public bool itemSaja;
    public bool pratinjauSaja;
    public Texture2D[] fotoAlbum;
    public string[] keteranganAlbum;
    public NPCBubbleTrigger[] temanTertawa;
    public Sprite emoteTertawa;

    public bool Selesai => buku != null && buku.barang.Exists(item => item != null && item.id == IdHadiah);
    public string IdHadiah => "hadiah_tantangan_" + jenis;

    internal const int LevelJigsawTerakhir = 3;
    internal static int UkuranJigsaw(int level) => level * 2 + 1;

    int levelJigsawTuntas;
    int LevelJigsawTerbuka => Selesai ? LevelJigsawTerakhir : Mathf.Min(levelJigsawTuntas + 1, LevelJigsawTerakhir);

    void Awake()
    {
#if !UNITY_EDITOR
        if (pratinjauSaja) { gameObject.SetActive(false); return; }
#endif
        if (jenis == JenisTantanganArthur.Jigsaw && !itemSaja) SiapkanDialogJigsaw();
    }

    public void Mulai() => Mulai(0);

    public void Mulai(int levelJigsaw)
    {
        if (buku == null || (!Application.isEditor && pratinjauSaja)) return;
        if (jenis == JenisTantanganArthur.Album)
        {
            if (DialogManager.Instance == null || DialogManager.Instance.minigameAktif || fotoAlbum == null || fotoAlbum.Length == 0) return;
            var album = new GameObject("Album kampus").AddComponent<LayarTantanganArthur>();
            album.BukaAlbum(fotoAlbum, keteranganAlbum, TertawaBersama);
            return;
        }
        if (jenis == JenisTantanganArthur.Foto)
        {
            if (!itemSaja && gambar != null)
            {
                KirimPesan("Kabar ruang dekan", "Temanku mengambil foto ini 10 menit yang lalu. Sir Semmy mungkin sedang tidak ada. Aku belum tahu apakah beliau sudah kembali.", gambar);
                BukaFoto(gambar);
            }
            return;
        }
        bool jigsaw = jenis == JenisTantanganArthur.Jigsaw && !itemSaja;
        bool jogetBisaDiulang = jenis == JenisTantanganArthur.Joget && !itemSaja;
        if (Selesai && !jigsaw && !jogetBisaDiulang) return;
        if (itemSaja)
        {
            BeriHadiah(false);
            return;
        }
        var dialog = DialogManager.Instance;
        if (dialog == null || dialog.minigameAktif) return;
        int level = levelJigsaw == 0 ? LevelJigsawTerbuka : levelJigsaw;
        if (jigsaw && (level < 1 || level > LevelJigsawTerbuka || gambar == null)) return;
        if (!Selesai) KirimPesan("Petunjuk tantangan", Petunjuk());
        var layar = new GameObject("Tantangan " + jenis).AddComponent<LayarTantanganArthur>();
        if (jigsaw) layar.Buka(jenis, gambar, () => SelesaikanLevelJigsaw(level), levelJigsaw: level);
        else layar.Buka(jenis, gambar, () => BeriHadiah(true), buah,
            mitraJoget: jogetBisaDiulang ? GetComponent<NPCJogetGoofy>() : null);
    }

    public static void BukaFoto(Texture2D foto)
    {
        var dialog = DialogManager.Instance;
        if (foto == null || (dialog != null && dialog.minigameAktif)) return;
        var layar = new GameObject("Foto ruang dekan").AddComponent<LayarTantanganArthur>();
        layar.Buka(JenisTantanganArthur.Foto, foto, null);
    }

    public void PulihkanHadiah()
    {
        if (buku == null || Selesai || string.IsNullOrWhiteSpace(hadiah)) return;
        BeriHadiah(false, false);
        if (jenis == JenisTantanganArthur.Jigsaw && !itemSaja) SiapkanDialogJigsaw();
    }

    void SelesaikanLevelJigsaw(int level)
    {
        if (Selesai || level <= levelJigsawTuntas) return;
        levelJigsawTuntas = level;
        if (level == LevelJigsawTerakhir) BeriHadiah(true);
        SiapkanDialogJigsaw();
    }

    void SiapkanDialogJigsaw()
    {
        if (area == null) return;
        int terbuka = LevelJigsawTerbuka;
        string pesan = Selesai ? "Ketiga level sudah tuntas! Kalau mau, kamu bisa menyusun fotonya lagi."
            : levelJigsawTuntas == 0 ? "Arthur, mau bantu susun foto Taman UNKLAB? Kita mulai dari sembilan potongan."
            : $"Mantap, Arthur! Level {levelJigsawTuntas} selesai. Mau coba level {terbuka}?";
        area.skenarioDialog = new[] { new BarisDialog {
            siapaYangBicara = Pembicara.Kanan_NPC, namaKarakter = namaNpc, teksDialog = pesan, teksTombolLanjut = "Lanjut"
        }};
        var opsi = new OpsiInteraksi[terbuka];
        for (int i = 0; i < opsi.Length; i++)
        {
            int level = i + 1;
            int ukuran = UkuranJigsaw(level);
            opsi[i] = new OpsiInteraksi {
                teksOpsi = $"Level {level} ({ukuran}×{ukuran})", tantanganArthur = this, levelJigsaw = level
            };
        }
        area.daftarOpsi = opsi;
    }

    void BeriHadiah(bool dariGame, bool baru = true)
    {
        if (Selesai) return;
        buku.Tambah(JenisBukuArthur.Tas, new CatatanArthur {
            id = IdHadiah, judul = hadiah, ikon = ikonHadiah, jumlah = 1,
            isi = jenis switch
            {
                JenisTantanganArthur.Jigsaw => "Kukis renyah dari Nara, teman menyusun jigsaw.",
                JenisTantanganArthur.Puzzle => "Keripik gurih pemberian Raka.",
                JenisTantanganArthur.Buah => "Pisang manis dari kantin.",
                JenisTantanganArthur.Joget => "Air minum untuk rehat setelah joget.",
                _ => "Pena pemberian Oki Kode."
            }
        }, tampilkanPenerimaan: baru);
        string terimaKasih = jenis switch {
            JenisTantanganArthur.Puzzle => "Fotonya utuh lagi! Makasih, Arthur. Keripiknya buat kamu.",
            JenisTantanganArthur.Buah => "Wah, buahnya habis kamu tebas! Terima kasih sudah membantu. Ini pisang gratis.",
            JenisTantanganArthur.Joget => "Arthur, tiga sesi kamu ikuti semua! Keren banget. Makasih sudah joget bareng. Ini Air Minum buat kamu.",
            JenisTantanganArthur.Jigsaw => "Semua potongannya pas! Terima kasih, Arthur. Ini kukis buatmu.",
            _ => "Kalkulatorku lulus uji coba! Makasih, Arthur. Pena ini buat kamu."
        };
        if (area != null)
        {
            area.skenarioDialog = new[] { new BarisDialog {
                siapaYangBicara = Pembicara.Kanan_NPC, namaKarakter = namaNpc,
                teksDialog = dariGame ? terimaKasih : "Semoga " + hadiah + " tadi berguna, Arthur!", teksTombolLanjut = "Selesai"
            }};
            bool jogetBisaDiulang = jenis == JenisTantanganArthur.Joget && !itemSaja;
            if (jogetBisaDiulang) area.skenarioDialog[0].teksTombolLanjut = "Lanjut";
            area.daftarOpsi = jogetBisaDiulang
                ? new[] { new OpsiInteraksi { teksOpsi = "Main lagi", tantanganArthur = this } }
                : Array.Empty<OpsiInteraksi>();
        }
        if (dariGame) KirimPesan("Terima kasih", terimaKasih);
        if (baru) ProgresSampingArthur.SimpanJikaEksplorasi(gameObject.scene);
    }

    void TertawaBersama()
    {
        if (emoteTertawa == null || temanTertawa == null) return;
        foreach (var npc in temanTertawa)
            if (npc != null) npc.MulaiAnimasiBubbleKhusus(emoteTertawa, 2.5f);
    }

    void KirimPesan(string judul, string isi, Texture2D lampiran = null)
    {
        buku.Tambah(JenisBukuArthur.Pesan, new CatatanArthur {
            id = "kontak_tantangan_" + jenis, pengirim = namaNpc, judul = judul, isi = isi, lampiranFoto = lampiran
        });
    }

    string Petunjuk() => jenis switch {
        JenisTantanganArthur.Puzzle => "Lihat foto kecil sebagai acuan. Geser potongan yang menyentuh kotak kosong.",
        JenisTantanganArthur.Buah => "Usap buah yang naik ke layar. Potong enam buah untuk menyelesaikan tantangan.",
        JenisTantanganArthur.Joget => "Ikuti panah dari Bimo. Tuntaskan tiga sesi, satu ketukan pada satu waktu.",
        JenisTantanganArthur.Jigsaw => "Seret dua potongan untuk menukar tempat. Ketuk dua potongan juga bisa. Cocokkan dengan foto acuan.",
        _ => "Pakai kalkulator buatanku untuk mengerjakan tiga soal. Tekan = sebelum mengirim jawaban."
    };
}
