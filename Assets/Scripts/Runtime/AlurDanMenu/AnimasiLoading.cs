// PANDUAN PROYEK - Runtime / AlurDanMenu
// Fungsi: Menganimasikan titik-titik tulisan loading.
// Panduan: Assets/Scripts/Runtime/AlurDanMenu/README.md

using System.Collections;
using UnityEngine;
using TMPro;

public class AnimasiLoading : MonoBehaviour
{
    public TextMeshProUGUI teksLoading;
    private string kataDasar = "Loading";

    void OnEnable()
    {
        if (teksLoading == null) teksLoading = GetComponent<TextMeshProUGUI>();
        if (teksLoading == null) return;
        StartCoroutine(MainkanTitikTitik());
    }
    void OnDisable() { StopAllCoroutines(); }

    IEnumerator MainkanTitikTitik()
    {
        while (true)
        {
            teksLoading.text = kataDasar;
            yield return new WaitForSecondsRealtime(0.4f);
            
            teksLoading.text = kataDasar + ".";
            yield return new WaitForSecondsRealtime(0.4f);
            
            teksLoading.text = kataDasar + "..";
            yield return new WaitForSecondsRealtime(0.4f);
            
            teksLoading.text = kataDasar + "...";
            yield return new WaitForSecondsRealtime(0.4f);
        }
    }
}
