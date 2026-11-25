# Pioneer Chapel — interior crayon, revisi 18

Aset ini dibangun dari video referensi 1, 2, 3, 4, 11, 22, dan 33, lalu disesuaikan dengan denah bertanda terbaru. Video menjadi acuan bentuk, bukan tekstur. Kayu, ubin, plester, dan layar memakai tekstur ilustratif bergaya crayon.

Aula berukuran perkiraan **41,6 × 72 meter**. Nave tengah memiliki atap pelana paling tinggi; sayap kiri dan kanan lebih rendah. Model memiliki enam pilar nave yang berhenti sekitar 7,5 meter, jendela sisi bawah, lima akses luar belakang, balkon belakang, dan dua akses tangga balkon berbentuk L yang tertutup.

Revisi 13 memperbaiki bidang pintu dekat layar, hubungan kolom dan balok balkon, pola jendela, serta lebar foyer. Cuplikan baru bertanggal waktu dari ketujuh video tersedia pada `work/chapel-analysis/revision13-evidence`. Tanda keberhasilan pembangunan Unity disimpan dalam `unity-build-receipt.json`, termasuk hash sumber untuk membedakan model terbaru dari pratinjau lama.

## Susunan ruang

- Bangku kayu utama berjumlah 80 dalam 20 baris dan empat kelompok. Kursi merah berjumlah 72: dua jejer, 18 baris pada setiap sisi, masing-masing objek terpisah. Jejer terdalam yang ditandai hijau dihapus pada kedua sisi. Tiang kecil di tengah sayap dan semua model AC dihapus. Enam pilar utama berada di batas nave dan sayap.
- Layar hitam tetap pada dinding belakang mimbar. Dua layar putih dipasang pada muka blok sayap. Dua proyektor hitam bertiang tripod logam berada di depannya dan mengarah ke masing-masing layar. Akses U melewati sisi luar blok, tanpa pintu di depan layar.
- Lima kursi panggung berderet di tengah dengan material kayu. Dua mimbar kayu berada di kanan dan kiri; piano dimajukan 8 m menuju bagian depan kiri panggung.
- Tangga mimbar terdiri dari empat undakan batu yang naik ke tengah podium. Tidak ada tangga mimbar di kiri atau kanan.
- Tiap sayap mimbar memakai blok layar persegi. Lorong terbuka setinggi ruangan berputar melalui sisi luarnya, lalu berbelok menuju bukaan persegi selebar 2,8 m pada sisi panggung. Jalur berbentuk U ini tidak mempunyai pintu di muka layar atau daun pintu pada bukaan panggung. Empat undakan penghubung lantai berada di dalam jalur samping.
- Dua speaker berdiri di bawah layar putih. Tiap proyektor berada pada tiang tripod di depan layarnya, terpisah dari tiang struktur dan kursi.
- Rangkaian bunga rendah memanjang 11,5 meter pada masing-masing sisi depan podium, total 23 meter, menyisakan akses tangga tengah.
- Balkon belakang memakai dua baris bangku kayu. Tangga L ke balkon memiliki tiga tahap: ambang pendek, bordes samping, lalu 24 anak tangga.
- Kaca sisi lantai atas dan panel hitam tambahan di belakang balkon tidak dipakai.
- Panel atap memiliki ketebalan dan penutup tepi. Dinding ujung mengikuti profil atap tanpa celah segitiga. Tiga jendela lengkung berada di depan dan lima di belakang, masing-masing dengan satu jendela bulat.
- Tiga pintu dalam berlintel persegi menghubungkan foyer dengan aula; daun terbuka ke foyer dan tidak mempunyai collider balok yang menutup jalur. Lima akses luar pada area belakang terdiri dari tiga pintu fasad dan dua pintu ujung samping. Dua pintu area mimbar berada pada dinding samping setelah belokan U, bukan pada bidang depan dekat layar. Ambang dan platformnya mengikuti lantai jalur U yang naik 0,82 m.
- Dua ruang berkaca gelap berada di antara tiga pintu aula, menghadap bangku kayu, dengan kusen kayu dan dinding bawah. Ruangannya mempunyai dinding samping serta pintu servis menghadap lorong belakang. Fungsi/isi ruangan tidak dikarang karena belum diketahui.
- Foyer memakai plafon panel kayu, lampu kotak, kolom putih dan detail tempat sampah. Ruang silang belakang diperlebar menjadi sekitar 6,2 m dan penghubung tengah 4,7 m sambil mempertahankan dua ruangan dan detailnya. Vestibule ujung bercabang ke aula dan ke pintu luar. Tangga tersembunyi di balik tembok balik, dengan ambang, bordes menyamping dan flight naik.
- Jendela bawah memakai empat daun kayu berpetak, kepala lengkung kecil, kusen dan gagang; sebagian daun terbuka. Transom atas berbeda: satu baris petak pendek, bukan kisi berlapis. Jendela foyer lebih rendah dengan ambang dinding, sedangkan jendela aula tetap sampai lantai sesuai instruksi sebelumnya.
- Dek balkon menaungi baris bangku belakang dan ditopang enam pilar tambahan yang menyambung ke balok menerus di bawah dek. Pagar memakai parapet rendah dan bilah kecil, tanpa kolom besar menggantung di atasnya. Lubang tangga serta bukaan pintu keluar atas tetap terbuka.
- Lantai podium menopang semua furnitur, dengan lima kursi kayu di tengah serta mimbar yang berada di atas lantainya.

## Membuka di Unity

Saat aset selesai diimpor, pemeriksa revisi 18 membangun prefab dan memeriksa hasil tanpa memindahkan fokus jendela. Ranting pinus berada pada muka foyer dari pintu ungu sehingga tidak muncul di area kursi. Bukaan pembatas tangga sejajar dengan pintu keluar luar pada kedua ujung, sehingga pintunya terlihat dari foyer. Pilar utama memakai pasangan cermin pada x ±15,65 m dan tidak menonjol sejauh sebelumnya. Scene berada di `Assets/Aset/Dunia/Pioneer_Chapel/Scenes/PioneerChapel_Preview.unity`.

Menu **Tools → A Little Campus Story → Chapel** menyediakan **0 - Pulihkan Kursor**, **2 - Buka Scene Contoh**, **3 - Lihat Belakang**, **4 - Lihat Mimbar**, **5 - Lihat Foyer Belakang**, **6 - Lihat Cabang Keluar**, dan **7 - Lihat Kaca Hitam**. Pemulihan kursor hanya mengubah penunjuk Unity. Gunakan tab Scene untuk menavigasi model; tab Game menampilkan kamera tetap.

Prefab utama berada di `Prefabs/PioneerChapel_Interior`; versi atap terbuka berada di `Prefabs/PioneerChapel_Cutaway`. Sumber prefab adalah `PioneerChapel_Mesh.bytes` dan `PioneerChapel_Data.json`. Model OBJ menyertakan normal permukaan; importer prefab juga menghitung ulang normal mesh.

## Batas akurasi

Ukuran adalah perkiraan dari video dan denah, bukan pengukuran bangunan. Lima pintu area belakang mengacu pada tiga pintu fasad pada foto luar dan dua pintu lateral ujung; pintu dalam serta pintu area mimbar dibedakan. Panjang, tinggi dan undakan di area yang tidak tampak penuh masih merupakan perkiraan.

Generator: `work/chapel-analysis/revision13_transform.py`. Laporan sumber: `revision13-geometry-checks.txt`; pemeriksaannya meliputi tujuh jalur foyer dengan ruang tubuh 72 cm, percabangan, pintu tanpa kaca, tembok penutup tangga, jalur U, furnitur, 435 sinar vertikal ke plafon, dan 777 sinar ke dinding ujung. Laporan prefab Unity: `unity-checks.txt`; gambar: `unity-*.png`, pada folder analisis yang sama. Hasil teknis bukan jaminan kecocokan visual sempurna.

Format untuk aplikasi 3D lain tersedia sebagai `PioneerChapel_Interior.obj` dengan `PioneerChapel.mtl`, serta `PioneerChapel_Interior.glb` dengan tekstur tertanam. Tekstur berukuran 512 × 512 dengan mipmap dan kompresi ASTC 6×6 untuk Android.
