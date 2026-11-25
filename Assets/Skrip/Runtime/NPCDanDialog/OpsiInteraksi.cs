// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Menyimpan struktur data BarisDialog, OpsiInteraksi, pembicara, dan jenis interaksi; bukan komponen untuk dipasang langsung.
// Panduan: Assets/Skrip/Runtime/NPCDanDialog/README.md

using UnityEngine;

public enum TipeInteraksi { NPC_Karakter, Tempat_Teleportasi, Barang_Item }
public enum Pembicara { Kiri_Arthur, Kanan_NPC, Tidak_Ada } // Ini Checkbox yang kapten maksud!

[System.Serializable]
public class BarisDialog
{
    [Header("1. Identitas Pembicara")]
    [Tooltip("Pilih siapa yang ngomong. Ini otomatis nyalain potret Kiri/Kanan!")]
    public Pembicara siapaYangBicara = Pembicara.Kiri_Arthur;
    
    [Tooltip("Nama yang muncul di UI (Lyra, Arthur, dll)")]
    public string namaKarakter = "Arthur"; 
    [Tooltip("Opsional: ganti potret kanan hanya pada baris ini, misalnya mahasiswa yang berbicara di dalam kelompok dosen.")]
    public Sprite potretPembicara;
    
    [Header("2. Teks Percakapan (Muncul Per Huruf)")]
    [TextArea(2, 4)]
    public string teksDialog;

    [Header("3. Pengaturan Tombol Next")]
    [Tooltip("Bebas diganti! Misal: 'Selanjutnya', 'Lalu...', 'Next'")]
    public string teksTombolLanjut = "Next ->";

    [Header("4. Sutradara Emote (Opsional)")]
    public bool picuEmoteSetelahIni = false;
    public Sprite spriteEmote; 
    public float durasiEmote = 1.5f;
    public bool akhiriSetelahEmote;
    [Header("5. Ekspresi pada baris ini (opsional)")]
    [Tooltip("Centang untuk mengganti ekspresi pada baris ini. Potret Arthur: Senyum/Netral = senyum biasa, Senang = antusias, Sedih = sedih.")]
    public bool ubahEkspresi;
    public EmosiKarakter ekspresi = EmosiKarakter.Netral;
    [Header("6. Dialog berteriak (opsional)")]
    public bool berteriak;
    [Range(.25f, 2f)] public float kekuatanTeriak = 1f;
}

[System.Serializable]
public class OpsiInteraksi
{
    public AksesArenaKoleksiArthur arenaKoleksi;
    [Header("Asrama Jasmine dan pertukaran tubuh (opsional)")]
    public PertukaranTubuhArthur pertukaranTubuh;
    public bool kembaliKeArthur;
    public AksesAsramaJasmine aksesJasmine;
    [Header("Kuis Mr.Quizo (opsional)")]
    public MrQuizoChapter1 mrQuizo;
    public int aksiQuizo;
    [Header("Percobaan minigame (opsional)")]
    public RapidTapArthur rapidTap;
    public TantanganSampingArthur tantanganArthur;
    public int levelJigsaw;
    [Header("Pilihan arah berjalan (opsional, tanpa teleportasi)")]
    public PengaturCabangKampus cabangJalur;
    public bool pilihJalanDepan;
    [Header("Teks di Panel Pilihan")]
    public string teksOpsi = "Tanya kabar";
    
    [Header("Khusus Teleportasi")]
    public Transform titikTujuan;
    [Tooltip("Ditampilkan bila titik tujuan belum tersedia. Mengisi Titik Tujuan mengaktifkan perpindahan tanpa menghapus pesan ini.")]
    [TextArea] public string pesanBelumTersedia;
    public bool nyalakanGrupDepan = false;
    public bool nyalakanGrupBelakang = true;
    public bool nyalakanGrupRuangan = false;
}
