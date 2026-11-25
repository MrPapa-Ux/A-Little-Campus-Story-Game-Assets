# Kamera

**Runtime** â€” berjalan bersama game.

[Kembali ke panduan utama](<../../MULAI DI SINI.md>)

## KameraGoyang.cs

[Buka skrip](<KameraGoyang.cs>)

Memberi ayunan vertikal kecil hanya ketika Arthur benar-benar **berlari**, memakai status lari dan kecepatan aktual. Berlaku untuk Rigidbody2D Jasmine/ruangan maupun CharacterController di GH, GK2 dan kantin. Jalan dan diam tetap tenang.

Offset diterapkan setelah kamera dasar pada LateUpdate, dilepas sebelum perhitungan frame berikutnya, dan tidak masuk cache penghalusan jalur. Amplitudo/frekuensi tetap mengikuti `bobAmount`/`bobSpeed` pada kamera; CharacterLab memakai 0,05 dan 10. Masuk/keluar halus sekitar 0,125 detik. Tidak mengayun saat terkunci, dialog/transisi, jeda, atau tertahan di batas. Teleportasi mereset offset.

**Tipe di dalam berkas:** `KameraGoyang`.

**Kelompok Inspector:** Tarik Arthur_Player ke kotak ini!, Pengaturan Goyangan.

## KameraNgekor.cs

[Buka skrip](<KameraNgekor.cs>)

Membuat kamera mengikuti target dengan offset dan gerakan yang halus.

**Tipe di dalam berkas:** `KameraNgekor`.

**Kelompok Inspector:** Pengaturan Kamera.

## BingkaiKameraTetap.cs

Revisi gameplay mengikuti perilaku video main menu yang disetujui: seluruh komposisi dunia acuan 16:9 dipaskan ke viewport layar penuh. FOV 60 derajat dan proyeksi 16:9 tetap, sementara viewport selalu (0,0,1,1). Karena itu sisi dunia tidak hilang saat Game View menyempit; gambar mengikuti rasio tampilan seperti video menu. Tidak ada letterbox, crop, pergeseran kamera, atau render texture tambahan. Pada rasio yang berbeda, gambar meregang sesuai layar, mengikuti pilihan pengguna untuk menu. Pengaturan menu, audio dan scene tidak diubah.
