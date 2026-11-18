# Pengaturan

**Runtime** — berjalan bersama game.

[Kembali ke panduan utama](<../../MULAI DI SINI.md>)

## FilterKecerahan.cs

[Buka skrip](<FilterKecerahan.cs>)

Menerapkan filter kecerahan berdasarkan pengaturan yang tersimpan.

**Tipe di dalam berkas:** `FilterKecerahan`.

## PengaturanGameplay.cs

[Buka skrip](<PengaturanGameplay.cs>)

Mengurus konfirmasi keluar dari gameplay ke menu dan penutupan pengaturan HP.

**Tipe di dalam berkas:** `PengaturanGameplay`.

**Pintu fungsi publik:** `MintaKeluar()`, `BatalKeluar()`, `YaKeluar()`, `TutupPengaturan()`.

## PengaturanManager.cs

[Buka skrip](<PengaturanManager.cs>)

Menghubungkan slider volume/kecerahan, label persentase, dan preferensi tersimpan.

**Tipe di dalam berkas:** `PengaturanManager`.

**Kelompok Inspector:** Sambungan Slider, Sambungan Filter Cahaya, Nilai di Layar.

**Pintu fungsi publik:** `SinkronkanPengaturan()`, `UbahVolume()`, `UbahCahaya()`, `HitungKegelapan()`.
