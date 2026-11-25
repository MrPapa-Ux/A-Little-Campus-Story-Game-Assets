// PANDUAN PROYEK - Runtime / AlurDanMenu
// Fungsi: Menampilkan intro teks, fade, dan pindah ke MainMenu; mendukung ketuk tiga kali untuk melewati intro.
// Panduan: Assets/Skrip/Runtime/AlurDanMenu/README.md

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class IntroTitle : MonoBehaviour 
{
    [Header("Colokan UI")]
    public CanvasGroup panelHitam;
    public TextMeshProUGUI teksJudul;
    
    [Header("Pengaturan Rute")]
    public string namaSceneMenu = "MainMenu";      

    LewatiCutsceneArthur lewati; AsyncOperation operasiMuat;

    void Start()
    {
        // Pastikan teks terang, tapi tertutup selimut hitam
        if (teksJudul != null) teksJudul.color = new Color(teksJudul.color.r, teksJudul.color.g, teksJudul.color.b, 1f);
        if (panelHitam != null) panelHitam.alpha = 1f; 
        
        lewati=GetComponent<LewatiCutsceneArthur>()??gameObject.AddComponent<LewatiCutsceneArthur>();
        lewati.Mulai(()=>{StopAllCoroutines();if(operasiMuat!=null)operasiMuat.allowSceneActivation=true;else SceneManager.LoadScene(namaSceneMenu);},teksJudul!=null?teksJudul.font:null);
        StartCoroutine(AturanMutlak7Detik());
    }

    IEnumerator AturanMutlak7Detik()
    {
        // ⏳ FASE 1 (2 DETIK): Selimut hitam perlahan dibuka
        yield return StartCoroutine(FadeSelimutHitam(1f, 0f, 2f));
        
        // ⏳ FASE 2 (3 DETIK): Teks nangkring dengan gagah
        yield return new WaitForSeconds(3f);
        
        // 🔥 FASE 3 (2 DETIK): Fade Out + Nyicil Load Scene Menu (ASYNC)
        operasiMuat = SceneManager.LoadSceneAsync(namaSceneMenu);
        operasiMuat.allowSceneActivation = false; // Tahan dulu, jangan pindah sampai layar gelap!

        float t = 0; 
        while (t < 2f) { 
            t += Time.deltaTime; 
            if (panelHitam != null) panelHitam.alpha = Mathf.Lerp(0f, 1f, t / 2f); 
            yield return null; 
        }

        // Layar sudah hitam pekat, pindah ke Menu sekarang!
        operasiMuat.allowSceneActivation = true; 
    }

    IEnumerator FadeSelimutHitam(float start, float end, float duration)
    {
        float t = 0; 
        while (t < duration) { 
            t += Time.deltaTime; 
            if (panelHitam != null) panelHitam.alpha = Mathf.Lerp(start, end, t / duration); 
            yield return null; 
        }
        if (panelHitam != null) panelHitam.alpha = end;
    }
}
