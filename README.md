# A Little Campus Story — Arsip Proyek Unity

Arsip pengembangan untuk penelitian pengenalan lingkungan Universitas Klabat melalui permainan berbasis cerita. Snapshot ini mencakup kode permainan, scene, prefab, model, tekstur, audio, dan konfigurasi proyek yang tersedia untuk publikasi. [Website proyek](https://alittlecampusstorywebsite.vercel.app/) dikelola terpisah.

## Meninjau proyek

1. Pasang Git LFS dan jalankan `git lfs install` sebelum clone. Setelah clone, jalankan `git lfs pull` untuk memastikan aset besar tersedia.
2. Buka folder repositori di Unity Hub menggunakan Unity **6000.3.11f1**. Unity akan memulihkan dependensi dalam `Packages/manifest.json`.
3. Impor dependensi aset pihak ketiga yang diperlukan ke tempat asalnya. Scene yang memakai paket tersebut dapat menampilkan referensi kosong sebelum paket dipulihkan.

`SOURCE_MANIFEST.json` mencatat tanggal snapshot, jalur, ukuran, dan SHA-256 file. Folder `Assets/Scripts` berisi logika permainan dan alat editor. Folder `Assets/WorldExplorer` dan `Assets/WorldStreaming` memuat model kawasan dan scene. Arsip publik mengecualikan cache Unity, pemulihan editor, hasil uji sementara, kredensial, dan dokumen penelitian yang terpisah.

## Dependensi aset yang diperoleh terpisah

- [Ezereal Car Controller](https://assetstore.unity.com/packages/tools/physics/ezereal-car-controller-302577) pada `Assets/Ezereal Assets`.
- [Bird oleh Zacxophone](https://assetstore.unity.com/packages/3d/characters/animals/birds/bird-330238) pada `Assets/Zacxophone`.
- Paket ikon BTN's Gorpaki, CasualGameButtonsVol02, Garden cozy kit, Paper Keys Sprite Pack V2, dan Ui Pack Version 5 By Zxgly. Folder asalnya berada di `Assets/All Asset/Sprites/Icon`. Gunakan aset dan `.meta` dari salinan yang sesuai agar GUID referensi cocok.

Ketentuan aset pihak ketiga mengikuti lisensi masing-masing. Atribusi yang ada dipertahankan dalam arsip.

## Aplikasi Android

APK yang dapat dipasang tersedia di [GitHub Releases](https://github.com/MrPapa-Ux/A-Little-Campus-Story-Game-Assets/releases). Catat tanggal build dan SHA-256 pada halaman rilis ketika mendokumentasikan versi yang diuji. Cabang sumber terus berkembang setelah APK dibangun.


## Snapshot terbaru — 24 November 2026

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
