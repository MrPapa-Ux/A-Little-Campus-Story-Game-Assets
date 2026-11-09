// PANDUAN PROYEK - Runtime / Ponsel/Peta
// Fungsi: Mengatur titik peta, pilihan kiri/kanan, fokus lokasi, zoom, dan batas geser.
// Panduan: Assets/Scripts/Runtime/Ponsel/Peta/README.md

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class TitikPetaKampus
{
    public string nama;
    [TextArea] public string keterangan;
    [Tooltip("Koordinat 0–1; (0,0) berada di kiri atas gambar.")]
    public Vector2 posisi;
    public LingkaranPeta lingkaran;
}

public class PetaKampusHP : MonoBehaviour
{
    public SistemHP sistemHP;
    public RectTransform viewport, isiPeta;
    public TMP_Text namaLokasi, keterangan, nomorLokasi;
    public TMP_Text labelInteraksi;
    public TitikPetaKampus[] lokasi = Array.Empty<TitikPetaKampus>();
    [Range(1.5f, 4f)] public float zoomFokus = 2.5f;
    [Range(.1f, 1f)] public float durasiZoom = .35f;
    public Color warnaPilihan = new Color(1f, .37f, .03f);
    public Color warnaLingkaran = new Color(.22f, .32f, .25f, .85f);
    public Color warnaLokasiArthur = new Color(.92f,.08f,.1f,1f);
    public string AreaArthur {get;private set;}="";
    int indeksNapas=-1;
    float awalNapas=-10;
    public void TandaiAreaArthur(string area){if(AreaArthur==area)return;AreaArthur=area;pilihanMarker=-1;}
    public void NapaskanLokasi(int indeks){indeksNapas=indeks;awalNapas=Time.unscaledTime;waktuMarker=0;}
    public int IndeksTerpilih { get; private set; }
    public float Zoom => isiPeta != null ? isiPeta.localScale.x : 1f;
    public bool SedangFokus { get; private set; }
    public bool BisaInput => isActiveAndEnabled && (sistemHP == null || sistemHP.transisiAplikasi == null || !sistemHP.transisiAplikasi.SedangBerjalan);
    float mulai, targetZoom = 1f, awalZoom;
    Vector2 targetPosisi, awalPosisi;
    bool animasi;
    float waktuMarker;
    float zoomMarker = -1f;
    int pilihanMarker = -1;

    void OnEnable()
    {
        SedangFokus = false; animasi = false; targetZoom = 1f; targetPosisi = Vector2.zero;
        if (isiPeta != null) { isiPeta.localScale = Vector3.one; isiPeta.anchoredPosition = Vector2.zero; }
        PerbaruiLabel();
        zoomMarker = -1f; pilihanMarker = -1; waktuMarker = 0f;
    }
    void Update()
    {
        if (labelInteraksi != null)
        {
            string label = SedangFokus ? "KEMBALI" : "LIHAT DEKAT";
            if (labelInteraksi.text != label) labelInteraksi.text = label;
        }
        if (BisaInput)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) PilihKanan();
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) PilihKiri();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Interaksi();
            if (Input.GetKeyDown(KeyCode.Escape)) { if (SedangFokus) LihatSemua(); else Tutup(); }
            if (Input.touchCount == 2)
            {
                Touch a = Input.GetTouch(0), b = Input.GetTouch(1);
                if (RectTransformUtility.RectangleContainsScreenPoint(viewport, a.position) && RectTransformUtility.RectangleContainsScreenPoint(viewport, b.position))
                {
                    float old = Vector2.Distance(a.position - a.deltaPosition, b.position - b.deltaPosition);
                    float now = Vector2.Distance(a.position, b.position);
                    if (old > 1f) AturZoomLangsung(Zoom * now / old);
                }
            }
        }
        if (animasi && isiPeta != null)
        {
            float t = Mathf.Clamp01((Time.unscaledTime - mulai) / Mathf.Max(.05f, durasiZoom));
            float ease = t * t * (3f - 2f * t);
            isiPeta.localScale = Vector3.one * Mathf.Lerp(awalZoom, targetZoom, ease);
            isiPeta.anchoredPosition = Vector2.Lerp(awalPosisi, targetPosisi, ease);
            if (t >= 1f) animasi = false;
        }
        bool berubah = pilihanMarker != IndeksTerpilih || !Mathf.Approximately(zoomMarker, Zoom);
        bool bernapas=indeksNapas>=0&&Time.unscaledTime-awalNapas<=.95f;
        bool pulse = bernapas||Time.unscaledTime >= waktuMarker;
        if (!berubah && !pulse) return;
        if (pulse) waktuMarker = Time.unscaledTime + 1f / 7f;
        for (int i = 0; i < lokasi.Length; i++)
        {
            LingkaranPeta ring = lokasi[i].lingkaran; if (ring == null) continue;
            ring.color = lokasi[i].nama==AreaArthur ? warnaLokasiArthur : warnaLingkaran;
            // Ukuran target sentuh tetap sekitar 56 pixel bahkan ketika gambar di-zoom.
            float napas=i==indeksNapas?Mathf.Sin(Mathf.Clamp01((Time.unscaledTime-awalNapas)/.95f)*Mathf.PI)*.16f:0;
            ring.rectTransform.localScale = Vector3.one * ((1+napas) / Mathf.Max(1f, Zoom));
        }
        zoomMarker = Zoom; pilihanMarker = IndeksTerpilih;
    }
    public void PilihKanan() => Pindah(1);
    public void PilihKiri() => Pindah(-1);
    void Pindah(int delta)
    {
        if (!BisaInput || lokasi.Length == 0) return;
        IndeksTerpilih = (IndeksTerpilih + delta + lokasi.Length) % lokasi.Length;
        PerbaruiLabel(); if (SedangFokus) Fokus();
    }
    public void SentuhLokasi(int indeks)
    {
        if (!BisaInput || indeks < 0 || indeks >= lokasi.Length) return;
        IndeksTerpilih = indeks; PerbaruiLabel(); Fokus();
        NapaskanLokasi(indeks);
    }
    public void Interaksi() { if (!BisaInput) return; if (SedangFokus) LihatSemua(); else Fokus(); }
    void Fokus()
    {
        if (lokasi.Length == 0 || isiPeta == null) return;
        SedangFokus = true;
        Vector2 p = lokasi[IndeksTerpilih].posisi;
        Vector2 local = new Vector2((p.x - .5f) * isiPeta.rect.width, (.5f - p.y) * isiPeta.rect.height);
        AnimasiKe(zoomFokus, Batasi(-local * zoomFokus, zoomFokus));
    }
    public void LihatSemua() { if (!BisaInput) return; SedangFokus = false; AnimasiKe(1f, Vector2.zero); }
    public void Perbesar() { if (BisaInput) { SedangFokus = true; AnimasiKe(Mathf.Min(4f, Zoom + .5f), Batasi(isiPeta.anchoredPosition, Mathf.Min(4f, Zoom + .5f))); } }
    public void Perkecil() { if (BisaInput) { float z = Mathf.Max(1f, Zoom - .5f); SedangFokus = z > 1.01f; AnimasiKe(z, Batasi(isiPeta.anchoredPosition, z)); } }
    public void Tutup() { if (BisaInput && sistemHP != null) sistemHP.TutupAplikasi(); }
    void AnimasiKe(float z, Vector2 p) { awalZoom = Zoom; awalPosisi = isiPeta.anchoredPosition; targetZoom = z; targetPosisi = p; mulai = Time.unscaledTime; animasi = true; }
    public void Geser(Vector2 delta) { if (!BisaInput || Zoom <= 1.01f) return; animasi = false; isiPeta.anchoredPosition = Batasi(isiPeta.anchoredPosition + delta, Zoom); }
    public void AturZoomLangsung(float z) { if (!BisaInput) return; animasi = false; z = Mathf.Clamp(z, 1f, 4f); isiPeta.anchoredPosition = Batasi(isiPeta.anchoredPosition * z / Mathf.Max(1f, Zoom), z); isiPeta.localScale = Vector3.one * z; SedangFokus = z > 1.01f; }
    Vector2 Batasi(Vector2 p, float z)
    {
        Vector2 limit = Vector2.Max(Vector2.zero, (isiPeta.rect.size * z - viewport.rect.size) * .5f);
        return new Vector2(Mathf.Clamp(p.x, -limit.x, limit.x), Mathf.Clamp(p.y, -limit.y, limit.y));
    }
    void PerbaruiLabel()
    {
        if (lokasi.Length == 0) return; IndeksTerpilih = Mathf.Clamp(IndeksTerpilih, 0, lokasi.Length - 1);
        if (namaLokasi != null) namaLokasi.text = lokasi[IndeksTerpilih].nama;
        if (keterangan != null) keterangan.text = lokasi[IndeksTerpilih].keterangan;
        if (nomorLokasi != null) nomorLokasi.text = (IndeksTerpilih + 1).ToString("00") + " / " + lokasi.Length;
    }
}
