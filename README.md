# A Little Campus Story — Arsip Proyek Unity

Arsip pengembangan untuk penelitian pengenalan lingkungan Universitas Klabat melalui permainan berbasis cerita. Snapshot ini mencakup kode permainan, scene, prefab, model, tekstur, audio, dan konfigurasi proyek yang tersedia untuk publikasi. [Website proyek](https://alittlecampusstorywebsite.vercel.app/) dikelola terpisah.

## Meninjau proyek

1. Pasang Git LFS dan jalankan `git lfs install` sebelum clone. Setelah clone, jalankan `git lfs pull` untuk memastikan aset besar tersedia.
2. Buka folder repositori di Unity Hub menggunakan Unity **6000.3.11f1**. Unity akan memulihkan dependensi dalam `Packages/manifest.json`.
3. Impor dependensi aset pihak ketiga yang diperlukan ke tempat asalnya. Scene yang memakai paket tersebut dapat menampilkan referensi kosong sebelum paket dipulihkan.

`SOURCE_MANIFEST.json` mencatat tanggal snapshot, jalur, ukuran, dan SHA-256 file. Folder `Assets/Skrip` berisi logika permainan dan alat editor. Folder `Assets/Aset/Dunia/Model_Kampus` dan `Assets/WorldStreaming` memuat model kawasan dan scene. Arsip publik mengecualikan cache Unity, pemulihan editor, hasil uji sementara, kredensial, dan dokumen penelitian yang terpisah.

## Dependensi aset yang diperoleh terpisah

- [Ezereal Car Controller](https://assetstore.unity.com/packages/tools/physics/ezereal-car-controller-302577) pada `Assets/Aset/Pihak_Ketiga/Ezereal`.
- [Bird oleh Zacxophone](https://assetstore.unity.com/packages/3d/characters/animals/birds/bird-330238) pada `Assets/Aset/Pihak_Ketiga/Zacxophone`.
- Paket ikon BTN's Gorpaki, CasualGameButtonsVol02, Garden cozy kit, Paper Keys Sprite Pack V2, dan Ui Pack Version 5 By Zxgly. Folder asalnya berada di `Assets/Aset/Sumber_Asli/Sprites/Icon`. Gunakan aset dan `.meta` dari salinan yang sesuai agar GUID referensi cocok.

Ketentuan aset pihak ketiga mengikuti lisensi masing-masing. Atribusi yang ada dipertahankan dalam arsip.

## Aplikasi Android

APK yang dapat dipasang tersedia di [GitHub Releases](https://github.com/MrPapa-Ux/A-Little-Campus-Story-Game-Assets/releases). Catat tanggal build dan SHA-256 pada halaman rilis ketika mendokumentasikan versi yang diuji. Cabang sumber terus berkembang setelah APK dibangun.


## Snapshot terbaru — 26 November 2026

Rilis **v2026.11.26** memperbarui kamera dan panah turun ruang dekan, pemisahan kelas 3/4 GK2, pemilihan teks bubble, pembatas ujung jalur, transisi cahaya GA/Taman, jam ponsel kanan bawah dan pemulihan NPC malam. Geometri asli kawasan dipulihkan sesuai kebutuhan imersi. Pengaman alat kelas lama mencegah penataan manual tertimpa.

APK utama diperbarui terpisah dari MOD. Uji singkat realme RMX3491: Dining 41,0–43,7 FPS; Jasmine lantai 2 55,9–59,7 FPS; GK2 21,4–26,6 FPS; arah Taman 30,9–32,1 FPS. Tidak ada exception yang terdeteksi pada empat sesi tersebut. **Performa belum stabil pada target 60 FPS.** Suhu baterai tercatat 50°C; hasil ini bukan pembandingan terkontrol terhadap versi lama atau kelulusan seluruh alur cerita. Metode serta angka tiap sampel ada pada `Pengujian/hasil-android-2026-11-26.json`.

APK: **A Little Campus Story.apk**, 306475587 byte. SHA-256: `bd87044c345657d2960eacd3276c4ba5e327b212bb1af4fd6eec55bfe0d8e2f8`.

Versi dan rilis lama tetap dipertahankan. APK MOD terakhir tetap versi 22 November dan belum memuat seluruh revisi sumber tanggal ini. Biner utama masih memakai Active Input Handling Both; peringatan Android dicatat untuk regresi input berikutnya.

## Snapshot 25 November 2026

Versi **v2026.11.25** mempertahankan versi terdahulu dan menambahkan:

- Struktur proyek dipisahkan menjadi `Assets/Skrip` dan `Assets/Aset`, dengan kelompok fungsi dan jenis aset. GUID serta rujukan aset dipertahankan.
- Scene penataan diganti dari `Chapter1_Kamar201` menjadi `Assets/Scenes/Penataan Kampus - Jalur NPC dan Bubble.unity`. Rujukan pemuatan dan daftar build diperbarui.
- TextMeshPro anak panah dapat dipilih langsung; alat penggeser bubble tidak lagi memaksa pemilihan kembali ke induknya.
- Posisi NPC Dining diselaraskan pada scene penataan, Explorer, dan CharacterLab. Lima NPC tambahan dipertahankan sehingga masing-masing mempunyai 17 NPC Dining. NPC Dining 12 dikembalikan ke interior.
- Posisi RectTransform disimpan melalui anchored position agar tidak kembali ke susunan lama setelah scene dibuka ulang.
- Alat penambahan NPC menggunakan scene permainan aktif atau scene penataan, dengan acuan lokasi yang sesuai.

APK utama build 25 November 2026: **A Little Campus Story.apk**. SHA-256: `ea9f37281db1ce6ab0973d0479fe641bba88d5c91cb9aab00a2d0ae62ae260a5`.
Uji singkat Dining pada realme RMX3491 menghasilkan 59,7–59,9 FPS dengan render scale adaptif. Hasil ini terbatas pada sesi Dining yang dicatat, bukan sertifikasi seluruh alur permainan.

MOD uji tetap tersedia pada [rilis MOD 22 November 2026](https://github.com/MrPapa-Ux/A-Little-Campus-Story-Game-Assets/releases/tag/mod-uji-2026-11-22). APK MOD tersebut belum dibangun ulang pada snapshot ini; tanggal buildnya berbeda dari kode terbaru.

Cadangan scene, cache, berkas pengujian, dokumen penelitian, dan dependensi berlisensi yang memerlukan impor terpisah tidak dimasukkan ke arsip publik.

## Snapshot 24 November 2026

Versi **v2026.11.24** mencakup perubahan permainan setelah snapshot 23 November:

- Joget Bareng Bimo memakai tombol kiri–kanan bawaan. Bimo mengikuti aba-aba; Arthur bergerak serasi ketika benar dan berlawanan arah ketika salah, dengan langkah dan perubahan arah hadap.
- Skip pembuka bertahan setelah ditampilkan dan melanjutkan gambar/narasi satu per satu. Bagian “Aku tertekan” melengkapi kalimat terlebih dahulu. Permainan tap-tap tetap harus dimainkan.
- Pengatur Scene menyediakan mode **GAMBAR BUBBLE SAJA**. Offset gambar dapat disunting tanpa memindahkan titik, radius, jalur, atau tujuan interaksi.
- Pelintas Dining mengikuti lekukan lorong, reaksi NPC diperbesar, kamera mendukung zoom dua jari, dan sesi baru mulai pukul 07.00 dengan jam dunia pada HP Arthur.
- Nama dosen pada label percakapan dipersingkat; nama lengkap tersedia melalui perkenalan.

APK utama snapshot ini disediakan sebagai **A Little Campus Story.apk** pada [halaman rilis](https://github.com/MrPapa-Ux/A-Little-Campus-Story-Game-Assets/releases). Versi ini mempertahankan identitas aplikasi utama dan terpisah dari MOD uji. Tanggal snapshot/tag GitHub tidak mengubah penomoran internal aplikasi Android.

Tag dan rilis sebelumnya, termasuk `v2026.11.23`, `source-2026-11-18`, dan `mod-uji-2026-11-22`, dipertahankan. Paket yang memerlukan lisensi/impor terpisah tetap mengikuti daftar dependensi di atas.

## Snapshot 23 November 2026

Seluruh kode permainan terkini dan aset proyek yang dapat dipublikasikan disinkronkan pada snapshot ini. Termasuk revisi quest Kontak, lorong Jasmine, akses asrama, cutscene dan Skip, tampilan MISI UTAMA, serta tautan Wattpad https://www.wattpad.com/story/419725182-a-little-campus-story.

Nama pengembang: Chipppz.

- [APK MOD uji 22 November 2026](https://github.com/MrPapa-Ux/A-Little-Campus-Story-Game-Assets/releases/tag/mod-uji-2026-11-22): aplikasi Android terpisah dari versi utama. APK ini dibangun sebelum snapshot kode terbaru; tanggal build dicantumkan agar tidak disamakan dengan versi sumber.
- APK utama untuk distribusi melalui Google Drive menggunakan nama **A Little Campus Story.apk**.

Cache Library/Temp, build sementara, kredensial, dan paket pihak ketiga yang memerlukan impor/lisensi terpisah tidak ikut diunggah. Daftar dependensi di atas tetap berlaku.
