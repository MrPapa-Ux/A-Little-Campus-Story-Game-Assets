// PANDUAN PROYEK - Runtime / EfekUI
// Fungsi: Menggoyangkan posisi dan rotasi kotak setelah layout awal selesai.
// Panduan: Assets/Scripts/Runtime/EfekUI/README.md

using UnityEngine;
using System.Collections; 

public class WigglyBox : MonoBehaviour
{
    [Header("Pengaturan Wiggly Kotak")]
    public float kecepatanAnimasi = 7f; 
    public float kekuatanGoyangPosisi = 2f; 
    public float kekuatanGoyangRotasi = 0.5f; 

    private RectTransform rectTransform;
    private float timer;
    private Vector2 posisiAsli;
    private Quaternion rotasiAsli;
    private bool posisiSiap;

    // Pakai IEnumerator Start biar bisa nunggu 1 frame
    IEnumerator Start() 
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Update dapat berjalan pada frame pemuatan yang panjang. Jangan izinkan
        // animasi menulis posisi nol sebelum layout dan posisi dasar siap.
        posisiSiap = false;
        yield return null;
        Canvas.ForceUpdateCanvases();
        
        // Baru catat posisinya setelah posisinya diatur rapi oleh bapaknya (Layout Group)
        posisiAsli = rectTransform.anchoredPosition;
        rotasiAsli = rectTransform.localRotation;
        timer = 0f;
        posisiSiap = true;
    }

    void Update()
    {
        if (!posisiSiap) return;
        timer += Time.deltaTime;
        
        // Animasi jalan di 7 FPS
        if (timer >= 1f / kecepatanAnimasi)
        {
            timer = 0;
            
            // Goyang Posisi (X dan Y)
            rectTransform.anchoredPosition = posisiAsli + new Vector2(
                Random.Range(-kekuatanGoyangPosisi, kekuatanGoyangPosisi),
                Random.Range(-kekuatanGoyangPosisi, kekuatanGoyangPosisi)
            );
            
            // Goyang Rotasi (Miring kiri-kanan dikit)
            rectTransform.localRotation = rotasiAsli * Quaternion.Euler(0, 0, Random.Range(-kekuatanGoyangRotasi, kekuatanGoyangRotasi));
        }
    }
}
