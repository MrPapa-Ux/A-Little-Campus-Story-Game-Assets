// PANDUAN PROYEK - Runtime / Pengaturan
// Fungsi: Menghubungkan slider volume/kecerahan, label persentase, dan preferensi tersimpan.
// Panduan: Assets/Scripts/Runtime/Pengaturan/README.md

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PengaturanManager : MonoBehaviour
{
    [Header("Sambungan Slider")]
    public Slider sliderVolume;
    public Slider sliderCahaya;

    [Header("Sambungan Filter Cahaya")]
    public CanvasGroup panelHitamFilter; // Kita pakai CanvasGroup di PanelHitam

    [Header("Nilai di Layar")]
    public TMP_Text teksVolume;
    public TMP_Text teksCahaya;

    void OnEnable()
    {
        SinkronkanPengaturan();
    }

    public void SinkronkanPengaturan()
    {
        if (sliderVolume != null)
        {
            float nilai = Mathf.Clamp(PlayerPrefs.GetFloat("MasterVolume", 1f), sliderVolume.minValue, sliderVolume.maxValue);
            sliderVolume.SetValueWithoutNotify(nilai);
            UbahVolume(nilai);
        }

        if (sliderCahaya != null)
        {
            float nilai = Mathf.Clamp(PlayerPrefs.GetFloat("MasterBrightness", 0.5f), sliderCahaya.minValue, sliderCahaya.maxValue);
            sliderCahaya.SetValueWithoutNotify(nilai);
            UbahCahaya(nilai);
        }
    }

    public void UbahVolume(float nilai)
    {
        PlayerPrefs.SetFloat("MasterVolume", nilai);
        AudioListener.volume = nilai;
        if (teksVolume != null) teksVolume.text = Mathf.RoundToInt(nilai * 100f) + "%";
    }

    public void UbahCahaya(float nilai)
    {
        PlayerPrefs.SetFloat("MasterBrightness", nilai);

        // LOGIKA CAHAYA UI/UX:
        // Jika nilai 0.7 (Paling Terang) -> Filter Hitam hilang (Alpha 0)
        // Jika nilai 0.3 (Paling Gelap) -> Filter Hitam tebal (Alpha 0.6)
        if (panelHitamFilter != null)
        {
            // Rumus konversi pintar biar geser ke kanan = makin terang
            panelHitamFilter.alpha = HitungKegelapan(nilai);
        }
        if (teksCahaya != null) teksCahaya.text = Mathf.RoundToInt(nilai * 100f) + "%";
    }

    public static float HitungKegelapan(float nilai)
    {
        return Mathf.Clamp01((0.7f - nilai) * 1.5f);
    }

    void OnDisable()
    {
        PlayerPrefs.Save();
    }
}
