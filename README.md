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

