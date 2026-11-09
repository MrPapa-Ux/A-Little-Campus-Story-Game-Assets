// PANDUAN PROYEK - Runtime / AlurDanMenu
// Fungsi: Memutar intro video dan berpindah ke menu; jalur alternatif yang masih disimpan.
// Panduan: Assets/Scripts/Runtime/AlurDanMenu/README.md

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroVideo : MonoBehaviour
{
    [Header("Colokan Video")]
    public VideoPlayer pemutarVideo;
    
    [Header("Pengaturan Rute")]
    public string namaSceneMenu = "MainMenu"; 

    private int tapCount = 0;
    private float tapTimer = 0f;
    private float timeLimit = 1.0f;

    void Start()
    {
        // Begitu video tamat, langsung gaskeun ke Menu!
        if (pemutarVideo != null)
        {
            pemutarVideo.loopPointReached += (vp) => PindahKeMenu();
        }
    }

    void Update()
    {
        // SISTEM SKIP 3X -> LANGSUNG KE SAMPLE SCENE
        bool isTapped = Input.GetMouseButtonDown(0);
        for (int i = 0; i < Input.touchCount; i++)
            if (Input.GetTouch(i).phase == TouchPhase.Began) isTapped = true;

        if (isTapped) { tapCount++; if (tapCount == 1) tapTimer = 0f; }

        if (tapCount > 0)
        {
            tapTimer += Time.deltaTime;
            if (tapTimer <= timeLimit) { if (tapCount >= 3) PindahKeMenu(); }
            else { tapCount = 0; tapTimer = 0f; }
        }
    }

    void PindahKeMenu()
    {
        SceneManager.LoadScene(namaSceneMenu);
    }
}
