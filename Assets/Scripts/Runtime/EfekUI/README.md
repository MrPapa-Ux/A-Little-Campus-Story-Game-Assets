# EfekUI

**Runtime** — berjalan bersama game.

[Kembali ke panduan utama](<../../MULAI DI SINI.md>)

## EfekPilihanUI.cs

[Buka skrip](<EfekPilihanUI.cs>)

Menganimasikan pointer, kedipan teks, dan panel yang sedang dipilih.

**Tipe di dalam berkas:** `EfekPilihanUI`.

**Kelompok Inspector:** Pengaturan Pointer, Pengaturan Teks Berkedip, Pengaturan Efek Container (Panel).

## FramePlayer.cs

[Buka skrip](<FramePlayer.cs>)

Memutar daftar sprite sebagai animasi pada komponen UI Image; bukan pengendali gerak Arthur.

**Tipe di dalam berkas:** `FramePlayer`.

## BunyiTombolUI.cs

[Buka skrip](<BunyiTombolUI.cs>)

Memainkan satu ketukan pendek hasil sintesis baru, tanpa rangkaian nada berulang. Pemutar dibuat satu kali, lalu menghubungkan tombol UI saat scene dimuat; kontrol rhythm dikecualikan supaya permainan tetap bersih. Nama ketiga pilihan lama dipertahankan untuk kompatibilitas pengaturan, dengan bunyi ketukan baru yang sama.

Kontrol arah/interact Arthur serta pembuka HP dikecualikan dari bunyi universal. `SistemHP` memanggil ketukan khusus hanya saat HP terbuka dan navigasi siap, serta efek buka singkat saat HP berhasil dibuka. HP memakai AudioSource 2D tersendiri dengan volume yang tidak mengikuti peredaman klik global. Efek baru berada di `Assets/Resources/ArthurSfx/`; komposisi dan asal bahan revisi ini dicatat dalam `work/audio18/`.

**Tipe di dalam berkas:** `BunyiTombolUI`.

**Pilihan lama:** `KertasLembut`, `PianoLipatan`, `KetikKertas` melalui `PilihVarian()`. Ketiganya kini memakai ketukan pendek tanpa nada berulang.

## WigglyBox.cs

[Buka skrip](<WigglyBox.cs>)

Menggoyangkan posisi dan rotasi kotak setelah layout awal selesai.

**Tipe di dalam berkas:** `WigglyBox`.

**Kelompok Inspector:** Pengaturan Wiggly Kotak.

## WigglyContainer.cs

[Buka skrip](<WigglyContainer.cs>)

Menggoyangkan posisi container UI dari posisi awal.

**Tipe di dalam berkas:** `WigglyContainer`.

**Kelompok Inspector:** Pengaturan Goyang Kotak.

## WigglyTMP.cs

[Buka skrip](<WigglyTMP.cs>)

Varian efek huruf TMP yang memperbarui mesh pada interval tertentu; dipertahankan untuk sambungan yang ada.

**Tipe di dalam berkas:** `WigglyTMP`.

**Kelompok Inspector:** Pengaturan Goyang Teks.

## WigglyText.cs

[Buka skrip](<WigglyText.cs>)

Menggoyangkan huruf TMP dengan basis vertex yang diambil dari `OnPreRenderText` sesudah TMP menyelesaikan rebuild. Basis diperbarui saat teks, font, atau layout berubah, lalu semua vertex dipulihkan sebelum efek diterapkan; geometri dari ukuran layar lama tidak dipakai ulang.

**Tipe di dalam berkas:** `WigglyText`.
