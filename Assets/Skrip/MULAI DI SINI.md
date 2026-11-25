# Mulai di sini — panduan skrip game

Folder ini memisahkan **Runtime** (logika yang dipakai game) dan **Editor** (alat pembentuk/revisi/pemeriksa proyek). Nama kelas, field Inspector, dan fungsi tetap dipertahankan. Skrip dipindahkan lewat AssetDatabase Unity sehingga GUID dan sambungan scene/prefab tetap dijaga.

## Cara membaca yang paling mudah

1. Pilih fitur pada daftar di bawah.
2. Buka README di folder fitur itu; setiap skrip dijelaskan satu per satu.
3. Baca komentar `PANDUAN PROYEK` di bagian atas skrip.
4. Baca field di bawah `Header` untuk sambungan Inspector, lalu fungsi publik untuk pintu interaksi.
5. Untuk mencari nama berkas lama, gunakan [katalog lengkap](<KATALOG SCRIPT.md>).

## Kalau mau mengubah fitur ini, mulai dari sini

| Tujuan | Folder dan skrip awal |
|---|---|
| Jalan/lari dan ekspresi Arthur | [Karakter](Runtime/Karakter/README.md): KarakterController, EkspresiWajah2D |
| Kamera mengikuti atau bergoyang | [Kamera](Runtime/Kamera/README.md) |
| Dialog, pilihan, emote, suara NPC | [NPC dan dialog](Runtime/NPCDanDialog/README.md): DialogManager → SistemPilihan; data BarisDialog/OpsiInteraksi |
| Buka HP dan berpindah aplikasi | [Sistem HP](Runtime/Ponsel/Sistem/README.md): SistemHP |
| Portrait/landscape, bingkai dan TV | [Tampilan HP](Runtime/Ponsel/Tampilan/README.md) |
| Peta dan penanda lokasi | [Peta](Runtime/Ponsel/Peta/README.md): PetaKampusHP |
| Tugas/pesan dan data barang | [Catatan dan pesan](Runtime/Ponsel/CatatanDanPesan/README.md): BukuArthur → AplikasiCatatanHP |
| Tas dan ritsleting | [Tas](Runtime/Ponsel/Tas/README.md): RitsletingTasUI; data barang tetap di BukuArthur |
| Playlist, putar/jeda, volume musik HP | [Musik HP](Runtime/Ponsel/Musik/README.md): PlaylistArthur → MusikArthur → AplikasiMusikHP |
| Rhythm yang dibuka dari ikon game | [Rhythm](Runtime/MiniGame/Rhythm/README.md): RhythmArthur → ArthurRhythmRound |
| Percobaan rapid tap lama | [Rapid tap](Runtime/MiniGame/RapidTap/README.md) |
| Mobil dan burung | [Dunia](Runtime/Dunia/README.md) |
| Volume/kecerahan dan keluar ke menu | [Pengaturan](Runtime/Pengaturan/README.md) |
| Wiggly, pointer, animasi sprite, dan bunyi tombol | [Efek UI](Runtime/EfekUI/README.md) |
| Lazy bar dan glitch | [Parameter dan glitch](Runtime/ParameterDanGlitch/README.md) |
| Intro, loading, perpindahan chapter/menu | [Alur dan menu](Runtime/AlurDanMenu/README.md) |

## Skrip Editor

- [Setup dan revisi](Editor/SetupDanRevisi/README.md): pembentuk dan revisi fitur tertentu.
- [Checkpoint](Editor/Checkpoint/README.md): alat penerapan susunan dari versi/checkpoint sebelumnya.
- [Pemeriksaan](Editor/Pemeriksaan/README.md): alat pengujian dan pemeriksaan hasil.
- [Profiling](Editor/Profiling/README.md): pengukuran performa di Editor.

Tidak semua skrip harus diseret ke Hierarchy. Kelas data, enum, helper statis, dan alat Editor bukan komponen GameObject. Aset ScriptableObject seperti PlaylistArthur/ProfilSuaraKertas diisi sebagai aset data.

## Aset chapel, musik, dan skrip bawaan paket

Chapel tetap menjadi paket utuh di [Assets/Aset/Dunia/Pioneer_Chapel](<../ChapelPioneer/BACA DULU.md>), berstatus **ASSET SANGAT PENTING — BELUM FINAL**. Tiga skripnya tercantum di katalog. Mempertahankan susunan paket membuat ekspor chapel tetap membawa script dan aset yang diperlukan.

Pembuatan OST lewat Python berada di [work/music-crayon](<../../work/music-crayon/README.md>). Berkas Python itu alat pembuatan audio offline, bukan script C# yang berjalan di Android. MainMenu saat ini memakai AudioSource yang merujuk audio fusion; musik menu tidak memerlukan script baru tersendiri.

Skrip bawaan aset pihak ketiga dan TutorialInfo dipertahankan di dalam paket asalnya. Folder `Library` dan `Packages` bukan tempat mengedit skrip gameplay milik proyek.

## Catatan nama lama

Beberapa nama berkas memiliki ejaan/spasi lama: `AreaIInteraksi.cs` berisi `AreaInteraksi`, `CascadeTransiton.cs` berisi `CascadeTransition`, `MobilCampus.cs` berisi `MobilKampus`, `LazyBarParameterArthur.cs` berisi `LazyBarArthur`, dan `Sutradara Transisi.cs` berisi `SutradaraTransisi`. Nama dipertahankan pada perapian ini; lihat kolom tipe di README agar tidak tertukar.

Riwayat jalur lama → baru serta salinan sumber sebelum perapian disimpan di `Checkpoints/Organisasi Script 2026-10-18`. Penjelasan ini merupakan peta baca; nilai Inspector yang tersimpan pada scene/prefab tetap menjadi nilai yang digunakan pada objek tersebut.
