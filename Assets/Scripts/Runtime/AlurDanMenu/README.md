# AlurDanMenu

**Runtime** — berjalan bersama game.

`JedaSaatTidakAktif` menghentikan waktu simulasi dan menjeda sumber audio yang sedang berbunyi ketika fokus hilang. Saat kembali, hanya sumber yang sebelumnya berjalan yang dilanjutkan dan volume pengguna dipertahankan. Run In Background dimatikan. Di Editor, `JedaEditorSaatTidakAktif` juga menjeda player loop saat Game View/aplikasi kehilangan fokus; pengujian otomatis memiliki pengecualian sementara yang dilepas sesudah selesai.

[Kembali ke panduan utama](<../../MULAI DI SINI.md>)

## AnimasiLoading.cs

[Buka skrip](<AnimasiLoading.cs>)

Menganimasikan titik-titik tulisan loading.

**Tipe di dalam berkas:** `AnimasiLoading`.

## CascadeTransiton.cs

[Buka skrip](<CascadeTransiton.cs>)

Menggerakkan garis penutup layar dan menjalankan perpindahan saat layar tertutup.

**Tipe di dalam berkas:** `CascadeTransition`.

**Kelompok Inspector:** Masukkan UI Image Garis.

**Pintu fungsi publik:** `MulaiTransisiTeleport()`.

## IntroTitle.cs

[Buka skrip](<IntroTitle.cs>)

Menampilkan intro teks, fade, dan pindah ke MainMenu; mendukung ketuk tiga kali untuk melewati intro.

**Tipe di dalam berkas:** `IntroTitle`.

**Kelompok Inspector:** Colokan UI, Pengaturan Rute.

## IntroVideo.cs

[Buka skrip](<IntroVideo.cs>)

Memutar intro video dan berpindah ke menu; jalur alternatif yang masih disimpan.

**Tipe di dalam berkas:** `IntroVideo`.

**Kelompok Inspector:** Colokan Video, Pengaturan Rute.

## LoadingManager.cs

[Buka skrip](<LoadingManager.cs>)

Memuat scene tujuan di belakang layar dan mengatur perpindahan setelah proses loading.

**Tipe di dalam berkas:** `LoadingManager`.

## Sutradara Transisi.cs

[Buka skrip](<Sutradara Transisi.cs>)

Mengatur urutan teks chapter/alasan, fade, dan mengirim tujuan ke proses loading.

**Tipe di dalam berkas:** `SutradaraTransisi`.

**Kelompok Inspector:** Aktor Teks (Seret dari Hierarchy), Pengaturan Sutradara, Tujuan Lemparan Scene.

## TitleMenuManager.cs

[Buka skrip](<TitleMenuManager.cs>)

Mengurus tombol mulai, popup, pengaturan, eksplorasi, penghargaan, dan status tamat pada menu. Saat menu dibuka, label kredit logo ditata sebagai kertas kecil bergaris tinta dengan font Amatic SC agar menutup teks kredit yang tertanam di frame akhir logo.

**Tipe di dalam berkas:** `TitleMenuManager`.

**Kelompok Inspector:** Panel Antarmuka (GDD 4.11), Tombol Terkunci (GDD 4.7.3.1).

**Pintu fungsi publik:** `KlikTombolMulai()`, `BatalMain()`, `KonfirmasiMain()`, `KlikTombolEksplorasi()`, `KlikTombolPengaturan()`, `KlikTombolPenghargaan()`, `BukaNovella()`, `KlikTombolKeluar()`.

## Transisi perpindahan revisi 21

`CascadeTransition.MulaiTransisiTeleport(Action)` mempertahankan sambungan SistemPilihan yang sudah ada. Bilah lama tidak dianimasikan lagi; satu Image yang sudah tersedia diregangkan menutup Canvas penuh dan memakai fade hitam quintic (0,38 detik tutup, 0,12 detik jeda gelap, 0,48 detik buka). Perpindahan dipanggil tepat sekali setelah frame hitam penuh, sehingga perubahan posisi tidak terlihat. Panggilan ganda selama transisi diabaikan; lapisan tidak menghalangi input setelah selesai. Fade memakai waktu game agar ikut berhenti ketika aplikasi kehilangan fokus. Tidak menambah audio, teks, kamera atau efek kilat.

## Menu revisi 22

Latar animasi judul menggunakan AspectRatioFitter EnvelopeParent untuk memenuhi layar tanpa distorsi, dengan pemotongan tepi seperlunya. Label kredit tetap satu di kanan bawah, tidak bergoyang agar kredit tertanam tidak bocor.

## Revisi pengganti checkpoint 25

Video menu asli 300 frame dipulihkan, termasuk judul yang menyatu dalam gambar. Image diregangkan ke seluruh Canvas tanpa crop dan tanpa judul/latar tambahan. Proporsi mengikuti layar sesuai permintaan fleksibel pengguna. Perubahan lapisan, filter frame, tata tombol adaptif, interaksi dan bayangan pada checkpoint 25 telah dibatalkan ke checkpoint 23.

## Penghargaan demo — 2 November 2026
Panel MainMenu memakai empat baris dengan gaya Pengaturan dan tombol X yang sama. Baris pertama memakai ikon `BanArthur_Penghargaan_Bintang.png` yang disetujui pemilik, dengan deskripsi “Sang Pengemban Keraguan Yang Memilih Tetap Maju”. Tiga baris lain sengaja kosong untuk pengembangan. Tidak ada status diperoleh, tanggal, persentase pemain, potret, atau tombol cerita terpisah. Baris pertama sendiri dapat ditekan untuk membuka cerita di Wattpad setelah demo selesai. Penghargaan dibuka oleh `GameFinished`, terpisah dari penyimpanan progres cerita. Pratinjau Editor tidak mengubah nilai penghargaan yang tersimpan.
