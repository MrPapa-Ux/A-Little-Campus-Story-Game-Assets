# Karakter

Berjalan Arthur sepenuhnya senyap. Lari memakai empat potongan pendek beton dalam `Resources/ArthurSfx`. Delapan event `PijakanLari` dipasang pada kontak kaki dalam clip lari 4,235 detik; `PijakanAnimasiArthur` pada objek Animator meneruskannya ke pengendali. Bunyi mengikuti animasi, bukan jarak atau timer terpisah. Bunyi hanya keluar saat benar-benar berlari, bergerak dan menyentuh lantai; berhenti/jalan/terkunci tetap senyap.

**Runtime** — berjalan bersama game.

[Kembali ke panduan utama](<../../MULAI DI SINI.md>)

## EkspresiWajah2D.cs

[Buka skrip](<EkspresiWajah2D.cs>)

Mengatur kedipan, mulut bicara, serta ekspresi netral, senyum, senang, sedih, dan marah.

**Tipe di dalam berkas:** `EmosiKarakter`, `EkspresiWajah2D`, `CacheEmosi`.

**Kelompok Inspector:** Ekspresi bebas dari Inspector.

**Pintu fungsi publik:** `PaksaKedip()`, `UcapkanHuruf()`, `HentikanBicara()`, `AturEkspresiDialog()`, `KembalikanEkspresi()`.

## KarakterController.cs

[Buka skrip](<KarakterController.cs>)

Mengatur jalan, lari, pengereman, arah visual, dan penguncian gerak Arthur. Jalur kampus 3D memakai CharacterController di Update, kemudian kamera di LateUpdate, agar lari tidak bergetar akibat perbedaan waktu frame dan tick fisika. Jasmine memakai Rigidbody2D di FixedUpdate seperti sebelumnya.

**Tipe di dalam berkas:** `KarakterController`.

**Kelompok Inspector:** Pengaturan Pergerakan, Wadah Utama & Animasi.

**Pintu fungsi publik:** `JalanKeKiri()`, `JalanKeKanan()`, `LepasKiri()`, `LepasKanan()`, `BerhentiJalan()`, `BerhentiBergerak()`.

## PoseMemegangHP.cs

[Buka skrip](<PoseMemegangHP.cs>)

Mengatur pose kepala/tangan dan HP kecil saat karakter membuka atau menutup ponsel.

**Tipe di dalam berkas:** `PoseMemegangHP`.

**Kelompok Inspector:** Sambungan karakter, Gaya memegang HP.

**Pintu fungsi publik:** `Buka()`, `Tutup()`.

## KetukKepalaArthur.cs

Dua ketukan dalam 0,3 detik pada proyeksi sprite kepala memulai NPCJogetGoofy selama dua detik. Berjalan berhenti dan Animator dijeda selama joget, kemudian pose dan kendali pulih. Ketukan UI/di luar kepala, interaksi, HP, minigame dan transisi tidak memulai joget; interaksi baru membatalkannya dengan aman. Mouse dan sentuhan didukung. Aksi tidak mengubah NPC Bimo, audio, ataupun posisi dunia Arthur.

Revisi 23: selama joget, Arthur memakai ekspresi Senang (mata tertutup) yang sama seperti Bimo. Pilihan ekspresi sebelumnya dipulihkan sesudah joget atau pembatalan, tanpa menimpa override ekspresi dialog baru.

## Revisi pengganti checkpoint 25

Volume sumber lari kini 0,4, turun sepertiga dari revisi 25 (0,6), sekitar 3,52 dB. Klip, pitch dan event animasi tetap sama; berjalan tetap senyap. Joget dua detik dan mata tertutup checkpoint 23 dipertahankan. Bayangan/kontrol tambahan checkpoint 25 dibatalkan.

## PijakanTanggaArthur.cs

Menyesuaikan ketinggian visual badan/sepatu pada zona tangga CharacterLab setelah Animator dan arah visual selesai. Data permukaan tapak diambil dari mesh asli saat penyuntingan; tidak menambah collider atau mengubah rute, kamera, maupun model. Koreksi dipulihkan sebelum animasi frame berikutnya dan saat komponen dinonaktifkan. Di luar zona tangga, pose asli tetap dipakai. Perbarui data tapak bila model world/tangga diganti.
