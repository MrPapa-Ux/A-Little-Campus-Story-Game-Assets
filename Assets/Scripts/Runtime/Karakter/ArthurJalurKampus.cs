using UnityEngine;

[DefaultExecutionOrder(150)]
[RequireComponent(typeof(KarakterController))]
public sealed class ArthurJalurKampus : MonoBehaviour
{
    public const float JarakKaki = 1.71533f;
    [Header("Kamera Jasmine (bisa diatur saat Play untuk mencoba)")]
    [Min(3f)] public float jarakKameraJasmine = 11f;
    public float batasKameraKiriJasmine = -10000f;
    public float batasKameraKananJasmine = 92f;
    public Transform wadahArahVisual;
    public Camera kamera;
    public GameObject fisikDepanJasmine;
    public UnityEngine.CharacterController colliderJalur;
    public JalurKampus JalurAktif { get; private set; }
    public float JarakDiJalur { get; private set; }
    public float KecepatanAktual { get; private set; }
    public bool DiTanah => JalurAktif != null && (fisikJalur.isGrounded || Mathf.Abs(transform.position.y - posisiJalur.y) < .12f);
    Rigidbody2D fisik2D;
    UnityEngine.CharacterController fisikJalur;
    KameraGoyang goyang;
    CascadeTransition transisi;
    Vector3 posisiKameraAwal, posisiJalur;
    Quaternion rotasiKameraAwal;
    float yaw;
    float bidangDekatSemula;
    float sudutPandanganSemula;
    float radiusColliderSemula;
    float tinggiColliderSemula;
    Vector3 pusatColliderSemula;
    BingkaiKameraTetap bingkai;
    // Kamera tetap anak Arthur untuk HP, tetapi penghalusannya bekerja di ruang world.
    Vector3 posisiKameraHalus;
    Quaternion rotasiKameraHalus;
    bool kameraHalusSiap;
    bool kameraMasukAktif, kameraMasukMenunggu;
    float waktuKameraMasuk, durasiKameraMasuk, geserKameraMasuk;
    public bool KameraSinematikAktif => kameraMasukAktif;

    void Awake()
    {
        fisik2D = GetComponent<Rigidbody2D>();
        fisikJalur = colliderJalur;
        if (fisikJalur != null)
        {
            radiusColliderSemula = fisikJalur.radius;
            tinggiColliderSemula = fisikJalur.height;
            pusatColliderSemula = fisikJalur.center;
        }
        if (kamera == null) kamera = GetComponentInChildren<Camera>();
        posisiKameraAwal = kamera.transform.localPosition;
        rotasiKameraAwal = kamera.transform.localRotation;
        bidangDekatSemula = kamera.nearClipPlane;
        sudutPandanganSemula = kamera.fieldOfView;
        bingkai = kamera.GetComponent<BingkaiKameraTetap>();
        goyang = kamera.GetComponent<KameraGoyang>();
        var pilihan = FindFirstObjectByType<SistemPilihan>();
        if (pilihan != null) transisi = pilihan.efekTransisi;
        if (fisikJalur != null) fisikJalur.enabled = false;
    }
    public void PindahKe(Transform tujuan)
    {
        if (goyang != null) goyang.AturUlang();
        var penanda = tujuan.GetComponent<TujuanJalurKampus>();
        kameraMasukAktif = penanda != null && penanda.jalur != null && penanda.kameraMasukSinematik;
        kameraMasukMenunggu = kameraMasukAktif && transisi != null && transisi.SedangBerjalan;
        waktuKameraMasuk = Time.unscaledTime;
        durasiKameraMasuk = penanda != null ? Mathf.Max(.2f, penanda.durasiKameraMasuk) : 1;
        geserKameraMasuk = penanda != null ? penanda.geserAwalKamera : 0;
        if (fisikJalur != null) fisikJalur.enabled = false;
        AktifkanJalur(penanda != null ? penanda.jalur : null);
        kameraHalusSiap = false;
        KecepatanAktual = 0;
        fisik2D.linearVelocity = Vector2.zero;
        fisik2D.simulated = JalurAktif == null;
        transform.position = tujuan.position;
        if (JalurAktif != null)
        {
            JarakDiJalur = JalurAktif.JarakTerdekat(tujuan.position);
            if(JalurAktif.gerbangSatuArah!=null)foreach(var gate in JalurAktif.gerbangSatuArah)if(gate!=null)gate.AturKedatangan(JarakDiJalur);
            JalurAktif.Sampel(JarakDiJalur, out posisiJalur, out _, out yaw);
            transform.position = posisiJalur;
            if (fisikJalur != null) { fisikJalur.transform.position = posisiJalur; fisikJalur.enabled = true; }
            AturKamera(true);
        }
        else
        {
            if (wadahArahVisual != null) wadahArahVisual.localRotation = Quaternion.identity;
            kamera.transform.localRotation = rotasiKameraAwal;
            kamera.nearClipPlane = bidangDekatSemula;
            kamera.fieldOfView = sudutPandanganSemula;
            if (bingkai != null) bingkai.sudutAcuan = sudutPandanganSemula;
            kamera.transform.localPosition = new Vector3(posisiKameraAwal.x, posisiKameraAwal.y, -jarakKameraJasmine);
        }
    }
    void AktifkanJalur(JalurKampus berikutnya)
    {
        if (JalurAktif != null && JalurAktif.pijakanKhususJalur != null) JalurAktif.pijakanKhususJalur.SetActive(false);
        JalurAktif = berikutnya;
        if (fisikJalur != null)
        {
            float radius = berikutnya != null && berikutnya.radiusColliderKhusus > 0
                ? berikutnya.radiusColliderKhusus : radiusColliderSemula;
            if (Mathf.Abs(fisikJalur.radius - radius) > .001f) fisikJalur.radius = radius;
            float tinggi = berikutnya != null && berikutnya.tinggiColliderKhusus > 0
                ? Mathf.Max(radius * 2, berikutnya.tinggiColliderKhusus) : tinggiColliderSemula;
            Vector3 pusat = pusatColliderSemula;
            pusat.y += (tinggi - tinggiColliderSemula) * .5f;
            if (Mathf.Abs(fisikJalur.height - tinggi) > .001f) fisikJalur.height = tinggi;
            if ((fisikJalur.center - pusat).sqrMagnitude > .000001f) fisikJalur.center = pusat;
        }
        if (JalurAktif != null && JalurAktif.pijakanKhususJalur != null) JalurAktif.pijakanKhususJalur.SetActive(true);
    }
    public bool Gerakkan(float input, float kecepatan, bool terkunci, float deltaWaktu)
    {
        if (JalurAktif == null) return false;
        if (deltaWaktu <= 0) { KecepatanAktual = 0; return true; }
        Vector3 lama = transform.position;
        if (terkunci || (transisi != null && transisi.SedangBerjalan)) input = 0;
        JalurAktif.Sampel(JarakDiJalur, out _, out Vector3 arah, out _);
        float tanda = JalurAktif.kananMengikutiUrutanTitik ? 1 : (Vector3.Dot(arah, kamera.transform.right) >= 0 ? 1 : -1);
        if (JalurAktif.kiriMendekatiKamera)
        {
            Vector3 depanKamera = kamera.transform.forward; depanKamera.y = 0;
            tanda = Vector3.Dot(arah, depanKamera) >= 0 ? 1 : -1;
        }
        // Menjaga kecepatan mendatar pada tanjakan, bukan melambat karena panjang diagonal.
        float mendatar = new Vector2(arah.x, arah.z).magnitude;
        float tujuan = JarakDiJalur + input * tanda * kecepatan * deltaWaktu / Mathf.Max(.25f, mendatar);
        tujuan = MalamGAChapter1.Batasi(JalurAktif, tujuan);
        if(JalurAktif.gerbangSatuArah!=null)foreach(var gate in JalurAktif.gerbangSatuArah)if(gate!=null)tujuan=gate.Batasi(JarakDiJalur,tujuan);
        bool berpindahJalur = false;
        if (tujuan > JalurAktif.Panjang && JalurAktif.lanjutanAkhir != null && BatasMalamChapter1.BolehJalur(JalurAktif.lanjutanAkhir))
        {
            tujuan -= JalurAktif.Panjang;
            AktifkanJalur(JalurAktif.lanjutanAkhir);
            berpindahJalur = true;
        }
        else if (tujuan < 0 && JalurAktif.lanjutanAwal != null && BatasMalamChapter1.BolehJalur(JalurAktif.lanjutanAwal))
        {
            AktifkanJalur(JalurAktif.lanjutanAwal);
            tujuan += JalurAktif.Panjang;
            berpindahJalur = true;
        }
        tujuan = Mathf.Clamp(tujuan, 0, JalurAktif.Panjang);
        // Memasuki ruas baru adalah kunjungan baru pada batas satu arahnya.
        // Status kunjungan sebelumnya tidak boleh melompatkan Arthur ke batas.
        if (berpindahJalur && JalurAktif.gerbangSatuArah != null)
            foreach (var gate in JalurAktif.gerbangSatuArah)
                if (gate != null) gate.AturKedatangan(tujuan);
        JalurAktif.Sampel(tujuan, out posisiJalur, out _, out yaw);
        if (fisikJalur != null && fisikJalur.enabled)
        {
            fisikJalur.Move(posisiJalur - fisikJalur.transform.position);
            transform.position = fisikJalur.transform.position;
        }
        else transform.position = posisiJalur;
        Vector3 selisih = transform.position - posisiJalur; selisih.y = 0;
        // Dorongan kecil capsule pada sambungan pijakan tidak boleh mengurangi langkah
        // setiap tick. Halangan nyata yang menahan lebih jauh tetap menghentikan gerak.
        JarakDiJalur = selisih.sqrMagnitude <= .09f ? tujuan : JalurAktif.JarakTerdekat(transform.position);
        KecepatanAktual = Vector3.Distance(lama, transform.position) / deltaWaktu;
        return true;
    }
    void LateUpdate()
    {
        if (kamera == null) return;
        if (JalurAktif != null) AturKamera(false);
        else
        {
            Vector3 p = kamera.transform.localPosition;
            p.z = -jarakKameraJasmine;
            p.x = posisiKameraAwal.x;
            if (fisikDepanJasmine != null && fisikDepanJasmine.activeInHierarchy)
                p.x = Mathf.Clamp(transform.position.x + p.x, batasKameraKiriJasmine, batasKameraKananJasmine) - transform.position.x;
            kamera.transform.localPosition = p;
        }
    }
    void AturKamera(bool langsung)
    {
        kamera.nearClipPlane = JalurAktif.bidangDekatKamera;
        float t = langsung ? 1 : 1 - Mathf.Exp(-JalurAktif.kehalusanKamera * Time.deltaTime);
        var lintasan=JalurAktif.lintasanKamera;
        bool kameraBertahap=JalurAktif.kameraGK1!=null;
        bool halusPadaLintasan=!kameraBertahap&&lintasan!=null&&lintasan.halusDiLintasan;
        if(!halusPadaLintasan)kamera.fieldOfView = Mathf.Lerp(kamera.fieldOfView, JalurAktif.sudutPandangan, t);
        if (bingkai != null) bingkai.sudutAcuan = kamera.fieldOfView;
        float jarakIkut = JalurAktif.batasJarakIkutKamera < 0 ? JarakDiJalur : Mathf.Min(JarakDiJalur, JalurAktif.batasJarakIkutKamera);
        // Acuan dari jalur menghindari getaran naik-turun karena penyelesaian kontak capsule.
        JalurAktif.Sampel(jarakIkut, out Vector3 acuan, out _, out float yawKamera);
        Quaternion sudut = Quaternion.Euler(0, yawKamera, 0);
        float jarak = JalurAktif.jarakKamera;
        if (JalurAktif.jarakKameraKoridorUjung > 0)
        {
            float bagian = Mathf.SmoothStep(0, 1, 1 - (JalurAktif.Panjang - jarakIkut) / JalurAktif.zonaJarakKoridorUjung);
            jarak = Mathf.Lerp(jarak, JalurAktif.jarakKameraKoridorUjung, bagian);
        }
        // Mendekatkan kamera mempertahankan sudut bidikan yang dipilih pengguna.
        float tinggi = JalurAktif.tinggiBidikan + (JalurAktif.tinggiKamera - JalurAktif.tinggiBidikan) * jarak / JalurAktif.jarakKamera;
        Vector3 target = acuan + sudut * new Vector3(JalurAktif.geserKameraSamping, tinggi, -jarak);
        Vector3 bidikan = acuan + sudut * new Vector3(JalurAktif.geserBidikanSamping, JalurAktif.tinggiBidikan, 0);
        Vector3 geserBatas = JalurAktif.BatasiPosisiKamera(target) - target;
        target += geserBatas;
        bidikan += geserBatas;
        if (JalurAktif.posisiKameraUjung != null && JalurAktif.bidikanKameraUjung != null)
        {
            // Bingkai akhir harus sudah tercapai ketika kamera berhenti mengikuti,
            // meskipun Arthur masih punya sisa jalan menuju bubble di ujung.
            float akhirIkut = JalurAktif.batasJarakIkutKamera < 0 ? JalurAktif.Panjang : Mathf.Min(JalurAktif.Panjang, JalurAktif.batasJarakIkutKamera);
            float bagian = JalurAktif.kameraTetap ? 1 : Mathf.SmoothStep(0, 1, 1 - (akhirIkut - jarakIkut) / JalurAktif.zonaKameraUjung);
            if (JalurAktif.posisiKameraAwal != null && JalurAktif.bidikanKameraAwal != null)
            {
                float kemajuan = JarakDiJalur / Mathf.Max(.01f, JalurAktif.Panjang);
                float mulai = Mathf.Clamp(JalurAktif.awalTransisiBingkai, 0, .99f);
                float selesai = Mathf.Clamp(JalurAktif.akhirTransisiBingkai, mulai + .01f, 1);
                bagian = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(mulai, selesai, kemajuan));
                target = Vector3.Lerp(JalurAktif.posisiKameraAwal.position, JalurAktif.posisiKameraUjung.position, bagian);
                bidikan = Vector3.Lerp(JalurAktif.bidikanKameraAwal.position, JalurAktif.bidikanKameraUjung.position, bagian);
            }
            else
            {
                target = Vector3.Lerp(target, JalurAktif.posisiKameraUjung.position, bagian);
                bidikan = Vector3.Lerp(bidikan, JalurAktif.bidikanKameraUjung.position, bagian);
            }
        }
        if (JalurAktif.bingkaiLokal != null)
            foreach (var lokal in JalurAktif.bingkaiLokal)
            {
                if (lokal == null || lokal.pusat == null || lokal.posisiKamera == null || lokal.bidikan == null) continue;
                float jauh = Mathf.Abs(JarakDiJalur - JalurAktif.JarakTerdekat(lokal.pusat.position));
                float bagian = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(lokal.radiusPenuh, Mathf.Max(lokal.radiusPenuh + .01f, lokal.radius), jauh));
                target = Vector3.Lerp(target, lokal.posisiKamera.position, bagian);
                bidikan = Vector3.Lerp(bidikan, lokal.bidikan.position, bagian);
            }
        if (!kameraBertahap && JalurAktif.lintasanKamera != null)
            JalurAktif.lintasanKamera.Sampel(acuan, out target, out bidikan, langsung);
        if(kameraBertahap)
        {
            JalurAktif.kameraGK1.Bingkai(acuan,out target,out bidikan,out float sudutTahap,langsung);
            kamera.fieldOfView=sudutTahap;
            if(bingkai!=null)bingkai.sudutAcuan=sudutTahap;
        }
        if(halusPadaLintasan)
            bidikan=Vector3.Lerp(bidikan,transform.position-Vector3.up*(JarakKaki-.9f),lintasan.Kedekatan);
        // Gerakan kedatangan terlihat setelah fade; jangan menghabiskannya di layar hitam.
        // Offset relatif bingkai tujuan menghindari sapuan jauh melewati world lain.
        if (kameraMasukAktif && !kameraBertahap)
        {
            if (kameraMasukMenunggu && (transisi == null || !transisi.SedangBerjalan))
            { kameraMasukMenunggu = false; waktuKameraMasuk = Time.unscaledTime; }
            float kemajuan = kameraMasukMenunggu ? 0 : Mathf.Clamp01((Time.unscaledTime-waktuKameraMasuk)/durasiKameraMasuk);
            float halus = kemajuan*kemajuan*kemajuan*(kemajuan*(kemajuan*6-15)+10);
            if(!halusPadaLintasan)target += sudut * new Vector3(geserKameraMasuk, .12f, .55f) * (1-halus);
            target = JalurAktif.BatasiPosisiKamera(target);
            if (kemajuan >= 1) kameraMasukAktif = false;
        }
        Quaternion rotasi = Quaternion.LookRotation(bidikan - target, Vector3.up);
        // Camera tetap anak Arthur agar fitur HP dan ketuk kepala tetap menemukan kamera yang sama.
        bool langsungKeTarget = langsung || !kameraHalusSiap || JalurAktif.kameraTetap || kameraBertahap;
        Vector3 posisiKamera = langsungKeTarget||halusPadaLintasan||JalurAktif.ikutiPosisiKameraTanpaJeda ? target : Vector3.Lerp(posisiKameraHalus, target, t);
        if (!langsungKeTarget && !halusPadaLintasan && JalurAktif.lintasanKamera != null)
            posisiKamera = Vector3.MoveTowards(posisiKameraHalus, posisiKamera,
                JalurAktif.lintasanKamera.kecepatanMaksimum * Time.deltaTime);
        posisiKamera = JalurAktif.BatasiPosisiKamera(posisiKamera);
        // Kamera anak Arthur ikut terdorong oleh geraknya; tahan sisa pergeseran
        // sepanjang jalur setelah bingkai batas tercapai, termasuk saat lerp.
        if (jarakIkut < JarakDiJalur)
        {
            JalurAktif.Sampel(jarakIkut, out _, out Vector3 arahBatas, out _);
            float lewat = Vector3.Dot(posisiKamera - target, arahBatas);
            if (lewat > 0) posisiKamera -= arahBatas * lewat;
        }
        kamera.transform.position = posisiKamera;
        bool bingkaiTerikat=halusPadaLintasan&&lintasan.batasKetertinggalan>0;
        var rotasiBerikut = langsungKeTarget||bingkaiTerikat ? rotasi : Quaternion.Slerp(rotasiKameraHalus, rotasi, t);
        if (!langsungKeTarget && !bingkaiTerikat && JalurAktif.lintasanKamera != null)
            rotasiBerikut = Quaternion.RotateTowards(rotasiKameraHalus, rotasiBerikut,
                JalurAktif.lintasanKamera.putaranMaksimum * Time.deltaTime);
        kamera.transform.rotation = rotasiBerikut;
        if(halusPadaLintasan)
        {
            Vector3 pusat=transform.position-Vector3.up*(JarakKaki-.9f);
            float kedalaman=Mathf.Max(.5f,Vector3.Dot(pusat-posisiKamera,kamera.transform.forward));
            float closeFov=Mathf.Clamp(2*Mathf.Atan(2f/(2*kedalaman*lintasan.tinggiPemainSaatDekat))*Mathf.Rad2Deg,18,JalurAktif.sudutPandangan);
            float fov=Mathf.Lerp(JalurAktif.sudutPandangan,closeFov,lintasan.Kedekatan);
            kamera.fieldOfView=langsung?fov:Mathf.Lerp(kamera.fieldOfView,fov,1-Mathf.Exp(-3*Time.deltaTime));
            if(bingkai!=null)bingkai.sudutAcuan=kamera.fieldOfView;
        }
        posisiKameraHalus = posisiKamera;
        rotasiKameraHalus = kamera.transform.rotation;
        kameraHalusSiap = true;
        if (wadahArahVisual != null)
        {
            JalurAktif.Sampel(JarakDiJalur, out _, out Vector3 arahJalan, out _, out float pose);
            float arahSamping = Mathf.Atan2(-arahJalan.z, arahJalan.x) * Mathf.Rad2Deg;
            float selisih = Mathf.DeltaAngle(kamera.transform.eulerAngles.y, arahSamping);
            float belok = Mathf.Clamp(selisih * .6f, -JalurAktif.putaranVisualSaatBelok, JalurAktif.putaranVisualSaatBelok);
            var hadap = Quaternion.Euler(0, kamera.transform.eulerAngles.y + belok + pose, 0);
            wadahArahVisual.rotation = Quaternion.Slerp(wadahArahVisual.rotation, hadap, langsung ? 1 : 1-Mathf.Exp(-8*Time.deltaTime));
        }
    }
}
