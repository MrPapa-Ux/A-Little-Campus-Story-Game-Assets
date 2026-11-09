using System;
using UnityEngine;

// Hanya menyimpan lokasi Eksplorasi. Tidak mengubah progres atau checkpoint cerita.
public sealed class SimpananEksplorasiArthur : MonoBehaviour
{
    public const string Kunci = "Explorer.LokasiTerakhir.v1";
    [Serializable] public sealed class Lokasi
    {
        public int versi = 1;
        public string jalur;
        public float jarak;
        public Vector3 posisi;
        public bool depan, belakang, ruangan;
    }
    ArthurJalurKampus arthur;
    SistemPilihan pilihan;
    KawasanKampus kawasan;
    float simpanBerikut;
    bool siapMenyimpan;
    GameObject penanda;

    bool ModeEksplorasi => gameObject.scene.name == "Explorer";
    static string Alamat(Transform t)
    {
        string s = t.name;
        while (t.parent != null) { t = t.parent; s = t.name + "/" + s; }
        return s;
    }
    static bool AngkaValid(float n) => !float.IsNaN(n) && !float.IsInfinity(n);
    public static bool Valid(Lokasi l) => l != null && l.versi == 1 && AngkaValid(l.jarak)
        && AngkaValid(l.posisi.x) && AngkaValid(l.posisi.y) && AngkaValid(l.posisi.z);
    void Referensi()
    {
        if (arthur == null) arthur = FindFirstObjectByType<ArthurJalurKampus>();
        if (pilihan == null) pilihan = FindFirstObjectByType<SistemPilihan>();
        if (kawasan == null) kawasan = GetComponent<KawasanKampus>();
    }
    public Transform TujuanTersimpan()
    {
        if (!ModeEksplorasi || !PlayerPrefs.HasKey(Kunci)) return null;
        Referensi();
        Lokasi l;
        try { l = JsonUtility.FromJson<Lokasi>(PlayerPrefs.GetString(Kunci)); }
        catch (Exception) { return null; }
        if (!Valid(l)) return null;
        JalurKampus jalur = null;
        if (!string.IsNullOrEmpty(l.jalur))
        {
            foreach (var j in FindObjectsByType<JalurKampus>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (j.gameObject.scene == gameObject.scene && Alamat(j.transform) == l.jalur) { jalur = j; break; }
            // Rute yang dihapus/diganti: gunakan awal normal, jangan muncul di world kosong.
            if (jalur == null || jalur.titik == null || jalur.titik.Length < 2 || Array.Exists(jalur.titik, t => t == null)) return null;
            jalur.Sampel(Mathf.Clamp(l.jarak, 0, jalur.Panjang), out l.posisi, out _, out _);
        }
        else if (!l.depan && !l.belakang && !l.ruangan) return null;
        penanda = new GameObject("Lokasi terakhir Eksplorasi");
        penanda.transform.SetParent(transform, false);
        penanda.transform.position = l.posisi;
        penanda.AddComponent<TujuanJalurKampus>().jalur = jalur;
        return penanda.transform;
    }
    public void TerapkanTujuan(Transform tujuan)
    {
        Referensi();
        arthur.PindahKe(tujuan);
        // Kelompok fisika Jasmine diperlukan saat kembali ke ruas tanpa JalurKampus.
        Lokasi l = null;
        try { l = JsonUtility.FromJson<Lokasi>(PlayerPrefs.GetString(Kunci)); } catch (Exception) { }
        if (pilihan != null && Valid(l))
        {
            if (pilihan.grupFisikDepan != null) pilihan.grupFisikDepan.SetActive(l.depan);
            if (pilihan.grupFisikBelakang != null) pilihan.grupFisikBelakang.SetActive(l.belakang);
            if (pilihan.grupFisikRuangan != null) pilihan.grupFisikRuangan.SetActive(l.ruangan);
        }
    }
    void Update()
    {
        if (!ModeEksplorasi) return;
        Referensi();
        if (kawasan == null || !kawasan.Siap || kawasan.SedangMemuat) return;
        siapMenyimpan = true;
        if (Time.unscaledTime >= simpanBerikut) { Simpan(); simpanBerikut = Time.unscaledTime + 10f; }
    }
    public void Simpan()
    {
        if (!ModeEksplorasi || !siapMenyimpan || arthur == null || kawasan == null || kawasan.SedangMemuat) return;
        var malam = GetComponent<MalamEksplorasiArthur>();
        if (malam == null) malam = FindFirstObjectByType<MalamEksplorasiArthur>();
        if (malam != null && malam.SedangPulang) return;
        var transisi = pilihan != null ? pilihan.efekTransisi : null;
        if (transisi != null && transisi.SedangBerjalan) return;
        var rb = arthur.GetComponent<Rigidbody2D>();
        if (arthur.JalurAktif == null && rb != null && Mathf.Abs(rb.linearVelocity.y) > .1f) return;
        var l = new Lokasi {
            jalur = arthur.JalurAktif != null ? Alamat(arthur.JalurAktif.transform) : "",
            jarak = arthur.JarakDiJalur, posisi = arthur.transform.position,
            depan = pilihan != null && pilihan.grupFisikDepan != null && pilihan.grupFisikDepan.activeSelf,
            belakang = pilihan != null && pilihan.grupFisikBelakang != null && pilihan.grupFisikBelakang.activeSelf,
            ruangan = pilihan != null && pilihan.grupFisikRuangan != null && pilihan.grupFisikRuangan.activeSelf
        };
        if (!Valid(l)) return;
        PlayerPrefs.SetString(Kunci, JsonUtility.ToJson(l));
        PlayerPrefs.Save();
    }
    void OnApplicationPause(bool jeda) { if (jeda) Simpan(); }
    void OnApplicationQuit() { Simpan(); }
    void OnDisable() { if (Application.isPlaying) Simpan(); }
}
