# ParameterDanGlitch

**Runtime** — berjalan bersama game.

[Kembali ke panduan utama](<../../MULAI DI SINI.md>)

## EfekGlitchLayar.cs

[Buka skrip](<EfekGlitchLayar.cs>)

Mengatur lapisan gangguan layar, teks acak, dan kotak glitch.

**Tipe di dalam berkas:** `EfekGlitchLayar`.

**Kelompok Inspector:** Banjir gangguan layar.

**Pintu fungsi publik:** `Mulai()`, `Tampilkan()`, `Gelapkan()`.

## KotakGlitchUI.cs

[Buka skrip](<KotakGlitchUI.cs>)

Menggambar potongan kotak gangguan untuk efek glitch melalui mesh UI.

**Tipe di dalam berkas:** `KotakGlitchUI`, `Potongan`.

**Pintu fungsi publik:** `Acak()`.

## LazyBarParameterArthur.cs

[Buka skrip](<LazyBarParameterArthur.cs>)

Mengatur parameter lazy bar Arthur dan hubungan kondisi pemain dengan efek glitch. Ketika bar habis, musik album/tema dihentikan dan seluruh penanda lanjut otomatis dibersihkan sebelum transisi kegagalan. Arthur juga berhenti bergerak.

**Tipe di dalam berkas:** `LazyBarArthur`.

**Kelompok Inspector:** Referensi UI Utama, Referensi Efek Glitch, Deteksi UI Otomatis, Pengaturan Teks Glitch, Pengaturan Waktu, Status Pergerakan & Sistem, Pengaturan Efek Juicy, Guncangan Kritis, Pengaturan Anomali Warna.
