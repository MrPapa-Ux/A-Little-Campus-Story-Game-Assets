// PANDUAN PROYEK - Runtime / AlurDanMenu
// Fungsi: Menampilkan intro teks, fade, dan pindah ke MainMenu; mendukung ketuk tiga kali untuk melewati intro.
// Panduan: Assets/Scripts/Runtime/AlurDanMenu/README.md

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

    private int tapCount = 0;
    private float tapTimer = 0f;
    private float timeLimit = 1.0f;

    void Start()
    {
        // Pastikan teks terang, tapi tertutup selimut hitam
        if (teksJudul != null) teksJudul.color = new Color(teksJudul.color.r, teksJudul.color.g, teksJudul.color.b, 1f);
        if (panelHitam != null) panelHitam.alpha = 1f; 
        
        StartCoroutine(AturanMutlak7Detik());
    }

    IEnumerator AturanMutlak7Detik()
    {
        // ⏳ FASE 1 (2 DETIK): Selimut hitam perlahan dibuka
        yield return StartCoroutine(FadeSelimutHitam(1f, 0f, 2f));
        
        // ⏳ FASE 2 (3 DETIK): Teks nangkring dengan gagah
        yield return new WaitForSeconds(3f);
        
        // 🔥 FASE 3 (2 DETIK): Fade Out + Nyicil Load Scene Menu (ASYNC)
        AsyncOperation operasiMuat = SceneManager.LoadSceneAsync(namaSceneMenu);
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

    void Update()
    {
        // SISTEM SKIP 3X -> LANGSUNG TERBANG KE SAMPLE SCENE
        bool isTapped = Input.GetMouseButtonDown(0);
        for (int i = 0; i < Input.touchCount; i++)
            if (Input.GetTouch(i).phase == TouchPhase.Began) isTapped = true;

        if (isTapped) { tapCount++; if (tapCount == 1) tapTimer = 0f; }

        if (tapCount > 0)
        {
            tapTimer += Time.deltaTime;
            if (tapTimer <= timeLimit) { if (tapCount >= 3) SceneManager.LoadScene(namaSceneMenu); }
            else { tapCount = 0; tapTimer = 0f; }
        }
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
