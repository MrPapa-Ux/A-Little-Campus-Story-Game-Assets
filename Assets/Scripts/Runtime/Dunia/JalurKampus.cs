using UnityEngine;

// Titik jalur menyimpan posisi Arthur, bukan mengubah mesh bangunan.
public sealed class JalurKampus : MonoBehaviour
{
    [Tooltip("Centang untuk ruangan tertutup. Variasi langit hijau dan latar gunung luar tidak digunakan di jalur ini.")]
    public bool diDalamRuangan;
    public Transform[] titik;
    public float[] arahKamera;
    [Tooltip("Putaran tambahan badan Arthur terhadap kamera pada setiap titik. Kosong mempertahankan pose biasa; berguna saat melewati sudut sempit.")]
    public float[] putaranBadan;
    [Tooltip("Kanan mengikuti urutan titik, termasuk saat lintasan mengarah ke depan/belakang kamera. Khusus jalur dengan gerak kedalaman.")]
    public bool kananMengikutiUrutanTitik;
    [Tooltip("Khusus dua akses Dining: KIRI mendekati kamera, KANAN menjauhi kamera, terlepas dari urutan titik.")]
    public bool kiriMendekatiKamera;
    [Tooltip("Opsional: batas yang mengunci jalan belakang setelah dilewati. Kosong mempertahankan jalur dua arah.")]
    public GerbangSatuArahJalur[] gerbangSatuArah;
    [Tooltip("Pijakan fisika yang hanya aktif ketika jalur ini dipakai. Pisahkan antar lantai agar capsule lantai bawah tidak tersangkut sisi bawah pijakan balkon.")]
    public GameObject pijakanKhususJalur;
    [Tooltip("Radius badan fisik khusus jalur ini. Nol memakai ukuran semula. Radius lebih kecil membantu kaki mengikuti anak tangga pendek.")]
    [Min(0)] public float radiusColliderKhusus;
    [Tooltip("Tinggi capsule khusus untuk pintu rendah. Nol memakai ukuran semula; posisi dasar kaki tetap dipertahankan.")]
    [Min(0)] public float tinggiColliderKhusus;
    [Header("Sambungan jalan tanpa teleportasi")]
    public JalurKampus lanjutanAwal;
    public JalurKampus lanjutanAkhir;
    [Header("Bingkai kamera khusus (opsional)")]
    public Transform posisiKameraUjung;
    public Transform bidikanKameraUjung;
    [Tooltip("Opsional: awal perpindahan kamera terarah menuju Kamera Ujung, misalnya dari lorong ke lobby.")]
    public Transform posisiKameraAwal;
    public Transform bidikanKameraAwal;
    [Tooltip("Bagian panjang jalur saat perpindahan bingkai mulai. Nol mempertahankan perilaku lama.")]
    [Range(0, 1)] public float awalTransisiBingkai;
    [Tooltip("Bagian panjang jalur saat perpindahan bingkai selesai. Gunakan bersama Awal Transisi untuk mempercepat belokan kamera di akses tangga.")]
    [Range(0, 1)] public float akhirTransisiBingkai = 1f;
    [Tooltip("Gunakan kedua penanda kamera sebagai bingkai tetap untuk seluruh jalur, misalnya tampak depan kantin.")]
    public bool kameraTetap;
    [System.Serializable]
    public sealed class BingkaiLokal
    {
        public Transform pusat;
        public Transform posisiKamera;
        public Transform bidikan;
        [Min(.1f)] public float radius = 5f;
        [Min(0)] public float radiusPenuh = 1f;
    }
    [Tooltip("Bingkai tambahan di suatu akses, misalnya melihat pintu tangga dari dalam ceruk GH. Kosong berarti memakai kamera jalur biasa.")]
    public BingkaiLokal[] bingkaiLokal;
    [Tooltip("Lintasan kamera khusus untuk melewati sudut bangunan. Kosong mempertahankan kamera jalur biasa.")]
    public LintasanKameraKampus lintasanKamera;
    [Tooltip("Opsional: kamera GK1 dengan zoom sebelum pergantian sisi pandang.")]
    public KameraGK1Bertahap kameraGK1;
    [Min(.1f)] public float zonaKameraUjung = 8f;
    [Tooltip("Jarak kamera khusus menjelang koridor sempit. Nol memakai jarak utama; arah kamera tetap dari Arah Kamera.")]
    [Min(0)] public float jarakKameraKoridorUjung;
    [Min(.1f)] public float zonaJarakKoridorUjung = 35f;
    [Min(3f)] public float jarakKamera = 11f;
    [Range(35f,90f)] public float sudutPandangan = 60f;
    public float tinggiKamera = 1.6f;
    public float tinggiBidikan = .55f;
    [Tooltip("Menggeser posisi kamera ke samping; samakan dengan Geser Bidikan Samping untuk pandangan tetap lurus.")]
    public float geserKameraSamping;
    [Tooltip("Batas posisi X kamera di ujung kiri. Batas kamera tidak membatasi langkah Arthur.")]
    public float batasKiriKamera = -10000f;
    [Tooltip("Batas posisi X kamera di ujung kanan; nilai besar membiarkan kamera mengikuti sepanjang jalur.")]
    public float batasKananKamera = 10000f;
    // Batas mengikuti orientasi world yang diputar; tidak bergantung pada pegangan EditorOnly.
    [HideInInspector] public bool batasDalamAcuanWorld;
    [HideInInspector] public Vector3 pusatAcuanBatas;
    [HideInInspector] public Quaternion rotasiAcuanBatas = Quaternion.identity;
    public Vector3 BatasiPosisiKamera(Vector3 posisi)
    {
        if(!batasDalamAcuanWorld){posisi.x=Mathf.Clamp(posisi.x,batasKiriKamera,batasKananKamera);return posisi;}
        Vector3 lokal=Quaternion.Inverse(rotasiAcuanBatas)*(posisi-pusatAcuanBatas);
        lokal.x=Mathf.Clamp(lokal.x,batasKiriKamera,batasKananKamera);
        return pusatAcuanBatas+rotasiAcuanBatas*lokal;
    }
    [Tooltip("Kamera berhenti mengikuti setelah jarak ini pada jalur. Arthur tetap dapat berjalan sampai ujung. Nilai negatif menonaktifkan batas.")]
    public float batasJarakIkutKamera = -1f;
    [Tooltip("Menggeser pusat pandangan ke samping tanpa memindahkan bangunan.")]
    public float geserBidikanSamping;
    [Min(.01f)] public float bidangDekatKamera = .3f;
    [Range(0,30)] public float putaranVisualSaatBelok;
    [Range(1f, 15f)] public float kehalusanKamera = 6f;
    [Tooltip("Ikuti posisi Arthur tanpa tertinggal. Berguna untuk pandangan menyamping di ruangan sempit; arah kamera tetap memakai pengaturan jalur.")]
    public bool ikutiPosisiKameraTanpaJeda;
    public Color warnaPanduan = Color.green;
    public float Panjang
    {
        get { float n = 0; if (titik != null) for (int i = 1; i < titik.Length; i++) if (titik[i - 1] && titik[i]) n += Vector3.Distance(titik[i - 1].position, titik[i].position); return n; }
    }
    public void Sampel(float jarak, out Vector3 posisi, out Vector3 arah, out float yaw)
    {
        Sampel(jarak, out posisi, out arah, out yaw, out _);
    }
    public void Sampel(float jarak, out Vector3 posisi, out Vector3 arah, out float yaw, out float pose)
    {
        posisi = transform.position; arah = Vector3.right; yaw = 0; pose = 0;
        if (titik == null || titik.Length < 2) return;
        jarak = Mathf.Clamp(jarak, 0, Panjang);
        for (int i = 1; i < titik.Length; i++)
        {
            Vector3 delta = titik[i].position - titik[i - 1].position;
            float panjang = delta.magnitude;
            if (jarak > panjang && i < titik.Length - 1) { jarak -= panjang; continue; }
            float t = panjang > .0001f ? Mathf.Clamp01(jarak / panjang) : 0;
            posisi = Vector3.Lerp(titik[i - 1].position, titik[i].position, t);
            arah = delta.normalized;
            float a = arahKamera != null && arahKamera.Length > i - 1 ? arahKamera[i - 1] : Mathf.Atan2(-arah.z, arah.x) * Mathf.Rad2Deg;
            float b = arahKamera != null && arahKamera.Length > i ? arahKamera[i] : a;
            yaw = Mathf.LerpAngle(a, b, t);
            float poseA = putaranBadan != null && putaranBadan.Length > i - 1 ? putaranBadan[i - 1] : 0;
            float poseB = putaranBadan != null && putaranBadan.Length > i ? putaranBadan[i] : poseA;
            pose = Mathf.LerpAngle(poseA, poseB, t);
            return;
        }
    }
    public float JarakTerdekat(Vector3 posisi)
    {
        float hasil = 0, total = 0, terbaik = float.PositiveInfinity;
        if (titik == null) return 0;
        for (int i = 1; i < titik.Length; i++)
        {
            Vector3 a = titik[i - 1].position, d = titik[i].position - a;
            float panjang = d.magnitude;
            // Capsule naik sedikit di atas lereng. Tinggi itu tidak boleh mendorong
            // jarak jalur kembali ke atas saat Arthur mencoba menuruni tangga.
            Vector3 datar = d; datar.y = 0;
            Vector3 relatif = posisi - a; relatif.y = 0;
            float t = datar.sqrMagnitude > .0001f ? Mathf.Clamp01(Vector3.Dot(relatif, datar) / datar.sqrMagnitude) : 0;
            float selisih = (relatif - datar * t).sqrMagnitude;
            if (selisih < terbaik) { terbaik = selisih; hasil = total + t * panjang; }
            total += panjang;
        }
        return hasil;
    }
    void OnDrawGizmos()
    {
        if (titik == null) return;
        Gizmos.color = warnaPanduan;
        for (int i = 0; i < titik.Length; i++) if (titik[i])
        {
            Gizmos.DrawWireSphere(titik[i].position - Vector3.up * ArthurJalurKampus.JarakKaki, .3f);
            if (i > 0 && titik[i - 1]) Gizmos.DrawLine(titik[i - 1].position - Vector3.up * ArthurJalurKampus.JarakKaki, titik[i].position - Vector3.up * ArthurJalurKampus.JarakKaki);
        }
    }
}
