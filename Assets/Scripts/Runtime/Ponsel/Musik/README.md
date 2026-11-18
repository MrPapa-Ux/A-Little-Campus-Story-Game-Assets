# Ponsel / Musik

**Runtime** — berjalan bersama game.

Playlist aktif: **Cloudny, Pozy Mozy, Blue Neon, Jou Castle**. Judul dan slot disimpan di `Assets/AudioArthur/Playlist_Arthur.asset`.

**Jou Castle** adalah nama baru Velvet Farewell. Versi terbaru mempertahankan band melankolis dengan piano, biola, gitar bersih, bass melodis dan drum, lalu menambahkan pasangan denyut bass rendah “dam-dam”. Durasi 37,16 detik; 155 BPM. Aset game: `Assets/AudioArthur/DemoOrisinal/Jou_Castle.ogg`, stereo Streaming. GUID sebelumnya tetap dipertahankan.

**Afterglow sudah dihapus dari playlist dan aset audio game.** Sumber dan ekspor sebelumnya diarsipkan di `work/jou-castle-damdam/sebelum-revisi`. Tiga lagu lainnya tetap. Master, stems dan skor Jou Castle ada di `ArtExports/Jou Castle`. Aktifkan **ULANG** agar AudioSource menyambung loop secara native.

Katalog terbaru dan lokasi master seluruh lagu: `ArtExports/Indeks Musik Arthur.md` di akar proyek.

[Kembali ke panduan utama](<../../../MULAI DI SINI.md>)

## AplikasiMusikHP.cs

[Buka skrip](<AplikasiMusikHP.cs>)

Menghubungkan tampilan kaset, judul, progres, volume, dan tombol ke pemutar MusikArthur.

**Tipe di dalam berkas:** `AplikasiMusikHP`.

**Pintu fungsi publik:** `Segarkan()`, `PilihSlot()`, `Kiri()`, `Kanan()`, `Putar()`, `Acak()`, `Ulang()`, `Volume()`, `Geser()`.

## KasetCoretanUI.cs

[Buka skrip](<KasetCoretanUI.cs>)

Menggambar kaset dan menggerakkan coretannya saat musik diputar.

**Tipe di dalam berkas:** `KasetCoretanUI`.

**Pintu fungsi publik:** `AturGerak()`.

## MusikArthur.cs

[Buka skrip](<MusikArthur.cs>)

Memutar playlist Arthur, lagu tema karakter, acak/ulang, jeda, serta menahan musik saat minigame. `HentikanSemua()` membersihkan status pemutaran/lanjut otomatis pada kegagalan. Pemeriksaan akhir lagu dilewati saat game kehilangan fokus agar sumber yang dijeda tidak dianggap sudah selesai.

**Tipe di dalam berkas:** `MusikArthur`.

**Pintu fungsi publik:** `Pilih()`, `PutarJeda()`, `Berikutnya()`, `Sebelumnya()`, `UbahAcak()`, `UbahUlang()`, `UbahVolume()`, `Geser()`, `MulaiTema()`, `SelesaiTema()`, `TahanMinigame()`.

## PemutarMusikUI.cs

[Buka skrip](<PemutarMusikUI.cs>)

Menampilkan notifikasi judul dan ikon berputar berdasarkan pemutar musik yang aktif.

**Tipe di dalam berkas:** `PemutarMusikUI`.

## PlaylistArthur.cs

[Buka skrip](<PlaylistArthur.cs>)

Aset data ScriptableObject yang menyimpan pemilik dan slot LaguArthur.

**Tipe di dalam berkas:** `LaguArthur`, `PlaylistArthur`.

