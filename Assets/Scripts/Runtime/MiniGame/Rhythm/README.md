# MiniGame / Rhythm

Nama yang ditampilkan kepada pemain adalah **TapTIpTup**, dengan font judul **AmaticSC-Regular SDF** yang sudah dipakai HP Arthur. Nama kelas dan folder internal tetap dipertahankan agar referensi scene tidak rusak.

**Runtime** — berjalan bersama game.

Tersedia tepat tiga level tap: **1 Mudah** (12 lingkaran, jeda 1,4 detik, waktu tampil 2,4 detik), **2 Normal** (20 lingkaran, jeda 0,85 detik, waktu tampil 1,45 detik), dan **3 Sulit** (32 lingkaran, jeda 0,5 detik, waktu tampil 0,85 detik). Mudah dipilih saat awal sesi. Ketuk lingkaran kapan saja selama terlihat; tidak ada garis pemindai atau syarat presisi beat. Pilihan level tersedia sebelum mulai dan setelah hasil, serta disembunyikan saat jeda.

[Kembali ke panduan utama](<../../../MULAI DI SINI.md>)

## ArthurRhythmAudio.cs

[Buka skrip](<ArthurRhythmAudio.cs>)

Membuat audio ronde dari bagian instrumental komposisi baru **Jam Pulang - Campus Groove** dalam `Resources/ArthurSfx/RhythmCampus`. Musik memakai 112 BPM; jadwal tap tidak dibatasi beat. Beat mayor sederhana tersedia sebagai cadangan bila aset tidak ditemukan.

**Tipe di dalam berkas:** `ArthurRhythmAudio`.

**Pintu fungsi publik:** `Buat()`.

## ArthurRhythmRound.cs

[Buka skrip](<ArthurRhythmRound.cs>)

Logika penilaian not, skor, akurasi, energi, combo, dan hasil ronde; kelas data/logika biasa.

**Tipe di dalam berkas:** `NotArthur`, `ArthurRhythmRound`, `Nilai`.

**Pintu fungsi publik:** `Lewatkan()`, `Lulus()`.

## PapanRhythmArthur.cs

[Buka skrip](<PapanRhythmArthur.cs>)

Menggambar lingkaran diam dengan cincin sisa waktu. Lingkaran dapat langsung diketuk sejak muncul sampai menghilang. Posisi berpindah pada kisi tersebar tanpa tumpang tindih target aktif. Sentuhan di area kosong diabaikan. Empat tombol jalur lama tidak dipakai.

**Tipe di dalam berkas:** `PapanRhythmArthur`.

**Pintu fungsi publik:** `Segarkan()`, `PosisiNot()`, `Terlihat()`, `OnPointerDown()`.

## RhythmArthur.cs

[Buka skrip](<RhythmArthur.cs>)

Mengatur menu/ronde rhythm, jadwal audio DSP, input not bulat, jeda, hasil, dan glitch kegagalan. Menu menggunakan instruksi singkat dan label formal: Mulai, Jeda, Lanjut, Selesai, Gagal, dan Ulangi. Hasil menampilkan skor, akurasi, Perfect/Good/Miss, serta max combo.

**Tipe di dalam berkas:** `RhythmArthur`, `Fase`.

**Kelompok Inspector:** Rhythm Arthur — not ketuk, tanpa hold, Referensi.

**Pintu fungsi publik:** `Buka()`, `TekanUtama()`, `MulaiRonde()`, `PukulJalur()`, `Jeda()`, `Lanjutkan()`, `Tutup()`, `TutupDariHP()`.

## RhythmJalurInput.cs

[Buka skrip](<RhythmJalurInput.cs>)

Mengirim sentuhan pada tombol jalur ke RhythmArthur.

**Tipe di dalam berkas:** `RhythmJalurInput`.

**Pintu fungsi publik:** `OnPointerDown()`.
