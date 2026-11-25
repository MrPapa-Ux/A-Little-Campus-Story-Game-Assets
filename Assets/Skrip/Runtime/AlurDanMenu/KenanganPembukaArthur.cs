using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class KenanganPembukaArthur : MonoBehaviour
{
    [Serializable]
    public sealed class Adegan
    {
        [TextArea(2, 6)] public string narasi;
        [Min(1)] public float lamaTampil = 6f;
        public Rect bagianGambar;
        public Texture2D gambarTerpisah;
    }
    public CanvasGroup kelompok;
    public RawImage gambar;
    public TMP_Text teks;
    public RectTransform areaAman;
    public Adegan[] adegan;
    [Min(.1f)] public float fadeMasuk = 1f;
    [Min(.1f)] public float fadeKeluar = 1f;
    [Tooltip("Durasi sementara sampai disetujui pemilik game.")]
    public bool durasiSudahDisetujui;
    public int AdeganAktif { get; private set; } = -1;
    public bool SedangBerjalan { get; private set; }
    [Header("Musik dan narasi fade")]
    public AudioClip laguKenangan;
    [Range(0, 1)] public float volumeLagu = .55f;
    // Parameter lama hanya mempertahankan anggaran waktu tiap gambar yang disetujui.
    [HideInInspector] public float hurufPerDetik = 12f;
    [HideInInspector] public float jedaTandaBaca = 1.5f;
    public float DurasiTahan(Adegan entry)
    {
        string narasi = entry.narasi ?? "";
        float lama = narasi.Length / Mathf.Max(1, hurufPerDetik);
        foreach (char c in narasi) if (c == ',' || c == '?') lama += jedaTandaBaca;
        return Mathf.Max(entry.lamaTampil, lama + 2f);
    }
    public float DurasiTotal
    {
        get { float total = 0; if (adegan != null) foreach (var entry in adegan) total += DurasiTahan(entry) + fadeMasuk + fadeKeluar; return total; }
    }
    public void TerapkanPanel(Adegan entry)
    {
        if (teksturAwal == null) teksturAwal = gambar.texture;
        gambar.texture = entry.gambarTerpisah != null ? entry.gambarTerpisah : teksturAwal;
        Rect bagian = entry.gambarTerpisah != null ? new Rect(0, 0, 1, 1) : entry.bagianGambar;
        gambar.uvRect = bagian;
        var fit = gambar.GetComponent<AspectRatioFitter>();
        if (fit != null && gambar.texture != null)
            fit.aspectRatio = gambar.texture.width * bagian.width / Mathf.Max(1f, gambar.texture.height * bagian.height);
    }
    AudioSource musik;
    Texture teksturAwal;
    LewatiCutsceneArthur lewati;
    bool dilewati;
    bool lanjutAdegan;
    Rect safeTerakhir;
    int lebarTerakhir, tinggiTerakhir;
    void Awake() { Sembunyikan(); AturAreaAman(); }
    void Update()
    {
        if (safeTerakhir != Screen.safeArea || lebarTerakhir != Screen.width || tinggiTerakhir != Screen.height)
            AturAreaAman();
    }
    void AturAreaAman()
    {
        if (areaAman == null || Screen.width <= 0 || Screen.height <= 0) return;
        safeTerakhir = Screen.safeArea; lebarTerakhir = Screen.width; tinggiTerakhir = Screen.height;
        areaAman.anchorMin = new Vector2(safeTerakhir.xMin / Screen.width, safeTerakhir.yMin / Screen.height);
        areaAman.anchorMax = new Vector2(safeTerakhir.xMax / Screen.width, safeTerakhir.yMax / Screen.height);
        areaAman.offsetMin = areaAman.offsetMax = Vector2.zero;
    }
    public IEnumerator Mainkan()
    {
        if (SedangBerjalan || kelompok == null || gambar == null || teks == null || adegan == null) yield break;
        SedangBerjalan = true;
        dilewati = false;
        if (lewati == null) lewati = gameObject.GetComponent<LewatiCutsceneArthur>() ?? gameObject.AddComponent<LewatiCutsceneArthur>();
        bool pembuka=gameObject.scene.name=="Transisi_Arthur";
        lewati.Mulai(() => {if(pembuka)lanjutAdegan=true;else dilewati=true;}, teks.font, pembuka);
        kelompok.gameObject.SetActive(true);
        kelompok.blocksRaycasts = false; kelompok.interactable = false;
        musik = GetComponent<AudioSource>();
        if (musik == null) musik = gameObject.AddComponent<AudioSource>();
        musik.playOnAwake = false; musik.spatialBlend = 0; musik.loop = true;
        musik.clip = laguKenangan; musik.volume = volumeLagu;
        if (laguKenangan != null) musik.Play();
        for (int i = 0; i < adegan.Length && !dilewati; i++)
        {
            lanjutAdegan=false;
            AdeganAktif = i; TerapkanPanel(adegan[i]);
            teks.text = adegan[i].narasi ?? ""; teks.alpha = 1;
            kelompok.alpha = 0;
            yield return Fade(0, 1, fadeMasuk);
            float durasi = DurasiTahan(adegan[i]);
            float waktu = 0;
            while (waktu < durasi && !dilewati&&!lanjutAdegan)
            {
                waktu += Mathf.Min(Time.unscaledDeltaTime, .05f);
                yield return null;
            }
            yield return Fade(1, 0, lanjutAdegan?Mathf.Min(.15f,fadeKeluar):fadeKeluar);
        }
        Sembunyikan();
    }
    IEnumerator Fade(float dari, float ke, float durasi)
    {
        float t = 0;
        while (t < durasi && !dilewati&&!(dari==0&&lanjutAdegan))
        {
            t += Mathf.Min(Time.unscaledDeltaTime, .05f);
            if (musik != null && ke == 0 && AdeganAktif == adegan.Length - 1) musik.volume = volumeLagu * (1 - Mathf.Clamp01(t / durasi));
            kelompok.alpha = Mathf.Lerp(dari, ke, Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / durasi)));
            yield return null;
        }
        kelompok.alpha = ke;
    }
    void Sembunyikan()
    {
        SedangBerjalan = false; AdeganAktif = -1;
        if (lewati != null) lewati.Selesai();
        if (musik != null) musik.Stop();
        if (kelompok != null) kelompok.alpha = 0;
    }
    void OnDisable() { Sembunyikan(); }
}
