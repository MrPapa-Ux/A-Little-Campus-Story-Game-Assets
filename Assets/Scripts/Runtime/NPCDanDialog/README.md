# NPCDanDialog

**Runtime** — berjalan bersama game.

[Kembali ke panduan utama](<../../MULAI DI SINI.md>)

## AreaIInteraksi.cs

[Buka skrip](<AreaIInteraksi.cs>)

Mendeteksi pemain dalam area, lalu memberikan data NPC/tempat, skenario, dan opsi kepada DialogManager.

**Tipe di dalam berkas:** `AreaInteraksi`.

**Kelompok Inspector:** Identitas & Tipe Interaksi, Pengaturan Tombol Keluar (Kiri Bawah), 1. SUTRADARA CERITA (Muncul Duluan), 2. Daftar Panel Opsi (Muncul Terakhir).

## DialogManager.cs

[Buka skrip](<DialogManager.cs>)

Membuka/menutup dialog, menyimpan interaksi aktif, dan mengunci atau melepas gerakan pemain.

**Tipe di dalam berkas:** `DialogManager`.

**Kelompok Inspector:** UI Referensi, Status Jarak NPC & Area, Data Titipan Trigger (JANGAN DIISI MANUAL).

**Pintu fungsi publik:** `BukaDialog()`, `TutupDialog()`.

## DialogTeriakGoofy.cs

[Buka skrip](<DialogTeriakGoofy.cs>)

Memberi getaran dan dentuman pada dialog berteriak.

**Tipe di dalam berkas:** `DialogTeriakGoofy`.

**Kelompok Inspector:** Dentuman dialog.

**Pintu fungsi publik:** `Atur()`, `Huruf()`.

## EfekMunculDialog.cs

[Buka skrip](<EfekMunculDialog.cs>)

Menganimasikan alpha dialog dan skala panel isinya. Skala root Canvas tetap dimiliki CanvasScaler agar dialog tidak hilang saat Game View berubah ukuran. Layout CharacterLab memakai ukuran acuan 1920×1080 dan Expand; panel serta kontrol tetap masuk layar.

**Tipe di dalam berkas:** `EfekMunculDialog`.

**Kelompok Inspector:** Pengaturan Animasi.

**Pintu fungsi publik:** `TutupDialog()`.

## NPCBubbleTrigger.cs

[Buka skrip](<NPCBubbleTrigger.cs>)

Memunculkan dan menggoyangkan bubble/emote NPC ketika pemain mendekat.

**Tipe di dalam berkas:** `NPCBubbleTrigger`.

**Kelompok Inspector:** Referensi Bubble, Referensi Visual (Teks vs Gambar), Pengaturan Animasi Skala, Pengaturan Goyang (Wiggly).

**Pintu fungsi publik:** `MulaiAnimasiBubble()`, `MulaiAnimasiBubbleKhusus()`.

## NPCJogetGoofy.cs

[Buka skrip](<NPCJogetGoofy.cs>)

Menggerakkan bagian tubuh NPC untuk tarian goofy tanpa menggeser bubble.

**Tipe di dalam berkas:** `NPCJogetGoofy`.

**Kelompok Inspector:** Joget dengan bagian tubuh; bubble tetap di tempat.

## NPCMenghadapArthur.cs

[Buka skrip](<NPCMenghadapArthur.cs>)

Membalik visual NPC agar menghadap Arthur, lalu kembali ke arah semula.

**Tipe di dalam berkas:** `NPCMenghadapArthur`.

**Pintu fungsi publik:** `HadapArthur()`, `KembaliSemula()`.

## OpsiInteraksi.cs

[Buka skrip](<OpsiInteraksi.cs>)

Menyimpan struktur data BarisDialog, OpsiInteraksi, pembicara, dan jenis interaksi; bukan komponen untuk dipasang langsung.

**Tipe di dalam berkas:** `TipeInteraksi`, `Pembicara`, `BarisDialog`, `OpsiInteraksi`.

**Kelompok Inspector:** 1. Identitas Pembicara, 2. Teks Percakapan (Muncul Per Huruf), 3. Pengaturan Tombol Next, 4. Sutradara Emote (Opsional), 5. Ekspresi pada baris ini (opsional), 6. Dialog berteriak (opsional), Percobaan minigame (opsional), Teks di Panel Pilihan, Khusus Teleportasi.

## ProfilSuaraKertas.cs

[Buka skrip](<ProfilSuaraKertas.cs>)

Aset data ScriptableObject untuk identitas dan potongan suara kertas setiap karakter.

**Tipe di dalam berkas:** `ProfilSuaraKertas`.

## SistemPilihan.cs

[Buka skrip](<SistemPilihan.cs>)

Menjalankan baris percakapan, mesin tik, potret, pilihan, ekspresi, emote, dan teleportasi.

**Tipe di dalam berkas:** `SistemPilihan`.

**Kelompok Inspector:** Referensi Sistem Utama, --- SUTRADARA UI ---, Referensi Potret & Nama, Daftar Panel Opsi Utama (Panel 1 & 2), Panel Tombol Keluar, --- EFEK MESIN TIK ---, Pelindung Tekanan Berulang, --- GRUP FISIK / CHUNK ---.

**Pintu fungsi publik:** `CariAwalKataTerakhir()`, `AkhiriPercakapanVisual()`, `AksiTombolInteraksi()`, `TekanPilihanLangsung()`, `TekanKeluarLangsung()`, `PindahBawah()`, `PindahAtas()`, `UpdateVisual()`.

## SuaraDialogKertas.cs

[Buka skrip](<SuaraDialogKertas.cs>)

Memutar potongan suara per huruf dialog menggunakan profil karakter. Lyra/Nara memakai `DialogCewek1–3`, Arthur/Bimo memakai `DialogCowok1–3`; pitch dan jeda tetap disesuaikan lewat profil. Potongan berasal dari aset NPC Talking dan Video Game Text pengguna yang difilter, diolah ulang dan diberi envelope. Satu suara aktif pada satu waktu agar bunyi tidak menumpuk. Asal dan hash dicatat dalam `work/bug19/voice-report.json`.

**Tipe di dalam berkas:** `SuaraDialogKertas`.

**Pintu fungsi publik:** `Huruf()`, `Hentikan()`, `CobaSuara()`.

## TempatBubbleTrigger.cs

[Buka skrip](<TempatBubbleTrigger.cs>)

Memunculkan ikon interaksi tempat tanpa menggeser pemilik collider.

**Tipe di dalam berkas:** `TempatBubbleTrigger`.

**Kelompok Inspector:** Referensi Ikon Tempat, Pengaturan Goyang (Wiggly).

## Tata letak dialog revisi 20

Panel dialog berada dekat bagian atas area permainan 16:9. `DialogManager.PercakapanAktif` menyimpan status seluruh percakapan, termasuk selingan emote saat Canvas sementara disembunyikan. Dua CanvasGroup pada `hudSaatPercakapan` (LazyBar_Utama dan Btn_HandPhone) menyimpan alpha/interactable/blocksRaycasts sebelum dialog, disembunyikan selama dialog, lalu dipulihkan tepat saat TutupDialog. Penghitung parameter tetap berhenti selama percakapan. Jangan menonaktifkan root UI_Gameplay karena tombol melanjutkan dialog tetap diperlukan.
