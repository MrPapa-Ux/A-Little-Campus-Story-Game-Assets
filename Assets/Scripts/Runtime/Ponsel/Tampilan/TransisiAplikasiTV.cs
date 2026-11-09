// PANDUAN PROYEK - Runtime / Ponsel/Tampilan
// Fungsi: Menjalankan efek TV nyala/mati pada bidang aplikasi yang ditentukan.
// Panduan: Assets/Scripts/Runtime/Ponsel/Tampilan/README.md

using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TransisiAplikasiTV : MonoBehaviour
{
    public RectTransform bidangCahaya;
    [Min(0.1f)] public float durasi = 0.5f;
    [Min(0.05f)] public float durasiMati = 0.22f;
    [Min(0.01f)] public float durasiFadeKeluar = 0.3f;
    public bool SedangBerjalan { get; private set; }
    public void Batalkan()
    {
        StopAllCoroutines();SedangBerjalan=false;
        var group=GetComponent<CanvasGroup>();group.alpha=0;group.blocksRaycasts=false;
        gameObject.SetActive(false);
    }
    readonly Vector3[] sudut = new Vector3[4];
    public void AturBidang(RectTransform panel)
    {
        if (panel == null || SedangBerjalan) return;
        panel.ForceUpdateRectTransforms(); panel.GetWorldCorners(sudut);
        RectTransform effect = (RectTransform)transform, parent = (RectTransform)effect.parent;
        Vector2 min = parent.InverseTransformPoint(sudut[0]), max = min;
        for (int i = 1; i < 4; i++) { Vector2 p = parent.InverseTransformPoint(sudut[i]); min = Vector2.Min(min,p); max = Vector2.Max(max,p); }
        effect.anchorMin = effect.anchorMax = effect.pivot = new Vector2(.5f,.5f);
        effect.localScale = Vector3.one; effect.localRotation = Quaternion.identity;
        effect.sizeDelta = max-min; effect.anchoredPosition = (max+min)*.5f-parent.rect.center;
    }

    public void Buka(Action tampilkanAplikasi)
    {
        if (SedangBerjalan) return;
        gameObject.SetActive(true);
        StartCoroutine(Nyalakan(tampilkanAplikasi));
    }

    public void Tutup(Action saatGelap, bool pindahScene = false)
    {
        if (SedangBerjalan) return;
        gameObject.SetActive(true);
        StartCoroutine(Matikan(saatGelap, pindahScene));
    }

    private IEnumerator Matikan(Action saatGelap, bool pindahScene)
    {
        SedangBerjalan = true;
        CanvasGroup group = GetComponent<CanvasGroup>();
        group.alpha = 1f;
        group.blocksRaycasts = true;
        group.interactable = false;
        UnityEngine.UI.Image background = GetComponent<UnityEngine.UI.Image>();
        if (background != null) background.color = Color.black;
        if (bidangCahaya != null) bidangCahaya.localScale = Vector3.one;
        // Canvas efek harus bertahan sampai fade selesai saat berpindah scene.
        GameObject root = transform.root.gameObject;
        if (pindahScene) DontDestroyOnLoad(root);
        float waktu = 0f;
        while (waktu < durasiMati)
        {
            waktu += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(waktu / durasiMati);
            if (bidangCahaya != null)
            {
                float tinggi = Mathf.SmoothStep(1f, 0.004f, Mathf.Clamp01(t / 0.65f));
                float lebar = Mathf.SmoothStep(1f, 0f, Mathf.Clamp01((t - 0.6f) / 0.4f));
                bidangCahaya.localScale = new Vector3(lebar, tinggi, 1f);
            }
            yield return null;
        }
        if (bidangCahaya != null) bidangCahaya.localScale = Vector3.zero;
        saatGelap?.Invoke();
        // Tampilkan satu frame hitam dan beri kesempatan scene baru diaktifkan.
        yield return null;
        waktu = 0f;
        while (waktu < durasiFadeKeluar)
        {
            waktu += Time.unscaledDeltaTime;
            group.alpha = 1f - Mathf.SmoothStep(0f, 1f, waktu / durasiFadeKeluar);
            yield return null;
        }
        group.alpha = 0f;
        group.blocksRaycasts = false;
        SedangBerjalan = false;
        if (pindahScene) Destroy(root);
        else gameObject.SetActive(false);
    }

    private IEnumerator Nyalakan(Action tampilkanAplikasi)
    {
        SedangBerjalan = true;
        CanvasGroup group = GetComponent<CanvasGroup>();
        group.alpha = 1f;
        group.blocksRaycasts = true;
        group.interactable = false;
        if (bidangCahaya != null) bidangCahaya.localScale = new Vector3(0.015f, 0.004f, 1f);
        float waktu = 0f;
        while (waktu < durasi)
        {
            waktu += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(waktu / durasi);
            if (bidangCahaya != null)
            {
                float lebar = Mathf.SmoothStep(0.015f, 1f, Mathf.Clamp01(t / 0.35f));
                float tinggi = Mathf.SmoothStep(0.004f, 1f, Mathf.Clamp01((t - 0.3f) / 0.7f));
                bidangCahaya.localScale = new Vector3(lebar, tinggi, 1f);
            }
            yield return null;
        }
        if (bidangCahaya != null) bidangCahaya.localScale = Vector3.one;
        tampilkanAplikasi?.Invoke();
        waktu = 0f;
        const float durasiFade = 0.16f;
        while (waktu < durasiFade)
        {
            waktu += Time.unscaledDeltaTime;
            group.alpha = 1f - Mathf.SmoothStep(0f, 1f, waktu / durasiFade);
            yield return null;
        }
        group.alpha = 0f;
        group.blocksRaycasts = false;
        SedangBerjalan = false;
        gameObject.SetActive(false);
    }
}
