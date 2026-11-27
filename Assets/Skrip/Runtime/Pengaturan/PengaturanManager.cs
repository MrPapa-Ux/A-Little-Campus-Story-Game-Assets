// PANDUAN PROYEK - Runtime / Pengaturan
// Fungsi: Menghubungkan slider volume/kecerahan, label persentase, dan preferensi tersimpan.
// Panduan: Assets/Skrip/Runtime/Pengaturan/README.md

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
    public Slider sliderChunk;
    public TMP_Text teksChunk;

    void OnEnable()
    {
        SinkronkanPengaturan();
    }

    public void SinkronkanPengaturan()
    {
        SiapkanChunk();
        if(sliderChunk!=null){sliderChunk.SetValueWithoutNotify(JangkauanChunkArthur.Pilihan);teksChunk.text=JangkauanChunkArthur.Nama;}
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
    void SiapkanChunk(){
        if(sliderChunk!=null||sliderCahaya==null||teksVolume==null)return;
        var box=sliderCahaya.transform.parent;
        void Baris(RectTransform t,float y){var a=t.anchorMin;var b=t.anchorMax;a.y=b.y=y;t.anchorMin=a;t.anchorMax=b;t.anchoredPosition=new Vector2(t.anchoredPosition.x,0);}
        Baris((RectTransform)sliderVolume.transform,.61f);Baris((RectTransform)teksVolume.transform,.61f);
        Baris((RectTransform)sliderCahaya.transform,.44f);Baris((RectTransform)teksCahaya.transform,.44f);
        var title=box.Find("Judul_Cahaya");var vol=box.Find("Judul_Volume");
        if(vol!=null)Baris((RectTransform)vol,.62f);if(title!=null)Baris((RectTransform)title,.45f);
        sliderChunk=Instantiate(sliderCahaya,box);sliderChunk.name="Slider_JangkauanChunk";
        sliderChunk.onValueChanged=new Slider.SliderEvent();sliderChunk.minValue=0;sliderChunk.maxValue=3;sliderChunk.wholeNumbers=true;
        Baris((RectTransform)sliderChunk.transform,.27f);
        var rt=(RectTransform)sliderChunk.transform;rt.anchorMax=new Vector2(.72f,.27f);
        teksChunk=Instantiate(teksVolume,box);teksChunk.name="ChunkValue";Baris(teksChunk.rectTransform,.27f);
        teksChunk.rectTransform.anchorMin=teksChunk.rectTransform.anchorMax=new Vector2(.84f,.27f);teksChunk.rectTransform.pivot=new Vector2(.5f,.5f);teksChunk.rectTransform.sizeDelta=new Vector2(230,65);teksChunk.fontSize=42;teksChunk.fontStyle|=FontStyles.Bold;teksChunk.enableAutoSizing=false;
        if(title!=null){var label=Instantiate(title.gameObject,box);label.name="Judul_Chunk";Baris((RectTransform)label.transform,.28f);var text=label.GetComponent<TMP_Text>();text.text="JANGKAUAN CHUNK";text.fontSize=52;text.enableAutoSizing=false;text.fontStyle|=FontStyles.Bold;}
        var hint=Instantiate(teksVolume,box);hint.name="PetunjukChunk";hint.text="Dekat lebih ringan · Utuh menampilkan seluruh kawasan";hint.fontSize=26;hint.enableAutoSizing=false;hint.alignment=TextAlignmentOptions.Center;hint.rectTransform.pivot=new Vector2(.5f,.5f);hint.rectTransform.anchorMin=hint.rectTransform.anchorMax=new Vector2(.54f,.205f);hint.rectTransform.anchoredPosition=Vector2.zero;hint.rectTransform.sizeDelta=new Vector2(1100,38);
        sliderChunk.onValueChanged.AddListener(UbahChunk);
    }
    public void UbahChunk(float nilai){JangkauanChunkArthur.Ubah(Mathf.RoundToInt(nilai));if(teksChunk!=null)teksChunk.text=JangkauanChunkArthur.Nama;}

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
