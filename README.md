# A Little Campus Story — Kode Unity

Repositori kode game **A Little Campus Story**, terpisah dari repositori website.

## Isi unggahan 9 November 2026

- Seluruh 233 berkas C# yang berada di folder Assets, termasuk runtime, editor, alat penataan, dan pemeriksaan proyek.
- 21 shader, 4 berkas CG include, dan 1 berkas HLSL: total **259 berkas sumber**.
- Berkas `.meta` sumber dan folder terkait untuk mempertahankan GUID Unity.
- `Packages/manifest.json`, `Packages/packages-lock.json`, dan identitas versi Unity di `ProjectSettings/ProjectVersion.txt`.
- `SOURCE_MANIFEST.json` berisi daftar berkas, ukuran, dan SHA-256 untuk memeriksa kelengkapan snapshot.

## Lokasi utama

| Folder | Isi |
|---|---|
| `Assets/Scripts/Runtime` | Logika permainan, NPC dan dialog, ponsel, cerita, dunia, dan antarmuka |
| `Assets/Scripts/Editor` | Alat penataan dan pemeriksaan dalam Unity Editor |
| Folder lain dalam `Assets` | Kode pendukung dan shader yang memang tersimpan di proyek |
| `Packages` | Daftar dan versi dependensi |

Unity proyek: **6000.3.11f1**. Distribusi aplikasi Android dilakukan melalui website dan tautan unduhan, bukan dari kode sumber ini.

## Batas cakupan

Unggahan ini adalah **arsip kode sumber**, bukan salinan lengkap proyek siap dijalankan. Model 3D, tekstur, gambar, audio, scene, prefab, serta seluruh konfigurasi proyek tidak diunggah dalam snapshot kode ini. Untuk membuka game secara utuh, diperlukan aset dan pengaturan dari proyek asli. Dependensi di Package Manager didokumentasikan melalui berkas manifest; cache paket hasil unduhan tidak disalin.

Folder `Library`, `Temp`, `Logs`, `Builds`, hasil pengujian sementara, kredensial, serta kode website tidak disertakan. Hak dan ketentuan kode pihak ketiga yang sudah ada tetap mengikuti pemilik asalnya.
