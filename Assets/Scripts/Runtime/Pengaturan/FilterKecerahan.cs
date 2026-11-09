// PANDUAN PROYEK - Runtime / Pengaturan
// Fungsi: Menerapkan filter kecerahan berdasarkan pengaturan yang tersimpan.
// Panduan: Assets/Scripts/Runtime/Pengaturan/README.md

using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FilterKecerahan : MonoBehaviour
{
    void Start()
    {
        CanvasGroup filter = GetComponent<CanvasGroup>();
        filter.blocksRaycasts = false;
        filter.interactable = false;
        filter.alpha = PengaturanManager.HitungKegelapan(PlayerPrefs.GetFloat("MasterBrightness", 0.5f));
    }
}
