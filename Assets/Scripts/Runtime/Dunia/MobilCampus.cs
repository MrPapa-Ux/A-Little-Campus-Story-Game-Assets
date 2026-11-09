// PANDUAN PROYEK - Runtime / Dunia
// Fungsi: Mengatur kemunculan acak mobil, jarak aman, penyesuaian kecepatan, klakson, dan asap knalpot.
// Panduan: Assets/Scripts/Runtime/Dunia/README.md

using System.Collections.Generic;
using UnityEngine;

public class MobilKampus : MonoBehaviour
{
    [Header("Pengaturan Lalu Lintas Organik")]
    public float kecepatanMin = 15f, kecepatanMax = 30f, durasiJalan = 15f;
    public float jedaWaktuMin = 10f, jedaWaktuMax = 25f;
    public Vector2 jedaKemunculanAwal = new Vector2(.4f, 7f);
    [Header("Satu jalur, tetap acak tetapi tidak saling menembus")]
    public string jalur = "JalanKampusDepan";
    public Vector3 arahJalur = Vector3.left;
    [Min(.1f)] public float jarakMinimum = 2.2f;
    [Min(.1f)] public float waktuMengikuti = .8f;
    [Min(.1f)] public float percepatan = 8f, pengereman = 22f;
    [Header("Klakson - peluang sekali ketika mulai tertahan")]
    [Range(0f, 1f)] public float peluangKlakson = .03f;
    [Range(1f, 2f)] public float pengaliNgebut = 1.45f;
    [Min(.1f)] public float durasiNgebut = 2.5f;
    [Range(0f, 1f)] public float volumeKlakson = .30f;
    public AudioClip suaraKlakson;
    [Header("Efek Knalpot")]
    public ParticleSystem asapKnalpot;
    public bool SedangJalan { get; private set; }
    public float KecepatanSaatIni { get; private set; }
    public float PanjangDepan { get; private set; }
    public float PanjangBelakang { get; private set; }
    public int JumlahKlakson { get; private set; }
    public int PercobaanKlakson { get; private set; }
    public float SisaNgebut => ngebut;
    public Vector3 Arah => arahJalur.sqrMagnitude > .001f ? arahJalur.normalized : transform.forward;
    public Vector3 PosisiAwal => posisiAwal;
    static readonly List<MobilKampus> mobil = new List<MobilKampus>();
    static readonly List<MobilKampus> urutan = new List<MobilKampus>();
    static int frameTerakhir = -1;
    Vector3 posisiAwal;
    float tunggu, kecepatanPilihan, jarakTempuh, ngebut;
    MobilKampus pemimpinEpisode;
    bool sudahUndi;
    AudioSource sumberSuara;
    AudioClip klaksonBuatan;

    void Awake()
    {
        posisiAwal = transform.position; PanjangDepan = PanjangBelakang = .5f;
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            if (!(r is MeshRenderer || r is SkinnedMeshRenderer) || !r.enabled) continue;
            Bounds b = r.bounds; Vector3 f = Arah;
            float center = Vector3.Dot(b.center - posisiAwal, f);
            float extent = Mathf.Abs(f.x) * b.extents.x + Mathf.Abs(f.y) * b.extents.y + Mathf.Abs(f.z) * b.extents.z;
            PanjangDepan = Mathf.Max(PanjangDepan, center + extent); PanjangBelakang = Mathf.Max(PanjangBelakang, -center + extent);
        }
        // Asap/trail tidak ikut dihitung sebagai panjang badan mobil.
    }
    void OnEnable()
    {
        if (!mobil.Contains(this)) mobil.Add(this);
        SedangJalan = false; KecepatanSaatIni = 0f; ngebut = 0f; sudahUndi = false; pemimpinEpisode = null;
        transform.position = new Vector3(posisiAwal.x, -1000f, posisiAwal.z);
        tunggu = Random.Range(Mathf.Max(0f,jedaKemunculanAwal.x), Mathf.Max(jedaKemunculanAwal.x,jedaKemunculanAwal.y));
        if (asapKnalpot != null) asapKnalpot.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    void OnDisable()
    {
        mobil.Remove(this); SedangJalan = false; KecepatanSaatIni = 0f;
        if (asapKnalpot != null) asapKnalpot.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        if (sumberSuara != null) sumberSuara.Stop();
    }
    void OnDestroy() { if (klaksonBuatan != null) Destroy(klaksonBuatan); }
    void LateUpdate()
    {
        if (frameTerakhir == Time.frameCount) return;
        frameTerakhir = Time.frameCount; SimulasikanLangkah(Time.deltaTime);
    }
    public static void SimulasikanLangkah(float dt)
    {
        dt = Mathf.Clamp(dt,0f,.25f); if (dt <= 0f) return;
        mobil.RemoveAll(c => c == null || !c.isActiveAndEnabled);
        foreach (MobilKampus c in mobil)
        {
            if (c.SedangJalan) continue;
            c.tunggu -= dt; if (c.tunggu <= 0f && c.PintuMasukKosong()) c.MulaiPerjalanan();
        }
        urutan.Clear(); urutan.AddRange(mobil);
        // Mobil depan bergerak dulu; mobil belakang memakai posisi depan yang terbaru.
        urutan.Sort((a,b) => { int lane = string.CompareOrdinal(a.jalur,b.jalur); return lane != 0 ? lane : Vector3.Dot(b.transform.position,b.Arah).CompareTo(Vector3.Dot(a.transform.position,a.Arah)); });
        foreach (MobilKampus c in urutan) if (c.SedangJalan) c.Jalan(dt);
    }
    bool PintuMasukKosong()
    {
        foreach (MobilKampus other in mobil)
        {
            if (other == this || !other.SedangJalan || !Sejalur(other)) continue;
            float delta = Vector3.Dot(other.transform.position - posisiAwal,Arah);
            float minimum = Mathf.Max(jarakMinimum,other.jarakMinimum);
            if (delta >= 0f ? delta < PanjangDepan + other.PanjangBelakang + minimum : -delta < PanjangBelakang + other.PanjangDepan + minimum) return false;
        }
        return true;
    }
    bool Sejalur(MobilKampus other) => other.jalur == jalur && Vector3.Dot(Arah,other.Arah) > .99f;
    void MulaiPerjalanan()
    {
        transform.position = posisiAwal; SedangJalan = true; jarakTempuh = 0f; ngebut = 0f;
        kecepatanPilihan = Random.Range(Mathf.Max(.1f,kecepatanMin),Mathf.Max(kecepatanMin,kecepatanMax));
        KecepatanSaatIni = kecepatanPilihan; sudahUndi = false; pemimpinEpisode = null;
        if (asapKnalpot != null) asapKnalpot.Play();
    }
    public MobilKampus MobilDiDepan(out float gap)
    {
        MobilKampus closest = null; gap = float.PositiveInfinity;
        foreach (MobilKampus other in mobil)
        {
            if (other == this || !other.SedangJalan || !Sejalur(other)) continue;
            float delta = Vector3.Dot(other.transform.position - transform.position,Arah); if (delta <= .001f) continue;
            float clearance = delta - PanjangDepan - other.PanjangBelakang;
            if (clearance < gap) { gap = clearance; closest = other; }
        }
        return closest;
    }
    void Jalan(float dt)
    {
        ngebut = Mathf.Max(0f,ngebut - dt);
        float desired = kecepatanPilihan * (ngebut > 0f ? pengaliNgebut : 1f);
        MobilKampus leader = MobilDiDepan(out float gap);
        float following = jarakMinimum + KecepatanSaatIni * Mathf.Max(.1f,waktuMengikuti);
        if (leader != pemimpinEpisode) { pemimpinEpisode = leader; sudahUndi = false; }
        if (leader != null)
        {
            bool tertahan = gap < following && kecepatanPilihan > leader.KecepatanSaatIni + .5f;
            if (tertahan && !sudahUndi)
            {
                sudahUndi = true; PercobaanKlakson++;
                // Satu undian per episode, bukan 3% setiap frame.
                if (Random.value < peluangKlakson) { BunyikanKlakson(); leader.ngebut = Mathf.Max(leader.ngebut,leader.durasiNgebut); }
            }
            if (gap > following * 2f) sudahUndi = false;
            desired = Mathf.Min(desired, leader.KecepatanSaatIni + Mathf.Max(0f,gap - following) / Mathf.Max(.1f,waktuMengikuti));
        }
        float speed = Mathf.MoveTowards(KecepatanSaatIni,desired,(desired < KecepatanSaatIni ? pengereman : percepatan) * dt);
        float move = Mathf.Max(0f,speed) * dt;
        if (leader != null) move = Mathf.Min(move,Mathf.Max(0f,gap - Mathf.Max(jarakMinimum,leader.jarakMinimum)));
        transform.position += Arah * move; KecepatanSaatIni = move / dt; jarakTempuh += move;
        if (jarakTempuh >= Mathf.Max(1f,durasiJalan) * kecepatanPilihan)
        {
            SedangJalan = false; KecepatanSaatIni = 0f; transform.position = new Vector3(posisiAwal.x,-1000f,posisiAwal.z);
            tunggu = Random.Range(Mathf.Max(0f,jedaWaktuMin),Mathf.Max(jedaWaktuMin,jedaWaktuMax));
            if (asapKnalpot != null) asapKnalpot.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    void BunyikanKlakson()
    {
        JumlahKlakson++;
        if (sumberSuara == null) { sumberSuara = gameObject.AddComponent<AudioSource>(); sumberSuara.playOnAwake = false; sumberSuara.spatialBlend = 1f; sumberSuara.minDistance = 8f; sumberSuara.maxDistance = 65f; }
        if (suaraKlakson == null && klaksonBuatan == null)
        {
            const int rate = 22050; var samples = new float[(int)(rate * .42f)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)rate; float local = t < .18f ? t : t - .24f;
                float envelope = local >= 0f && local < .18f ? Mathf.Min(1f,local * 150f) * Mathf.Min(1f,(.18f - local) * 150f) : 0f;
                samples[i] = (Mathf.Sin(t * 380f * Mathf.PI * 2f) + Mathf.Sin(t * 480f * Mathf.PI * 2f)) * .35f * envelope;
            }
            klaksonBuatan = AudioClip.Create("KlaksonDuaNada",samples.Length,1,rate,false); klaksonBuatan.SetData(samples,0);
        }
        sumberSuara.PlayOneShot(suaraKlakson != null ? suaraKlakson : klaksonBuatan,volumeKlakson);
    }
}
