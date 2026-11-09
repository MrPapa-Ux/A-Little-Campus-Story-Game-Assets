// PANDUAN PROYEK - Runtime / Kamera
// Fungsi: Membuat kamera mengikuti target dengan offset dan gerakan yang halus.
// Panduan: Assets/Scripts/Runtime/Kamera/README.md

using UnityEngine;

public class KameraNgekor : MonoBehaviour
{
    [Header("Pengaturan Kamera")]
    public Transform targetKarakter; // Siapa yang mau diikutin?
    public float kecepatanKamera = 5f; // Seberapa mulus ngikutinnya
    public Vector3 jarakOffset = new Vector3(0, 1f, -10f); // -10 wajib biar layar nggak ketutup!

    void LateUpdate()
    {
        if (targetKarakter != null)
        {
            // Menghitung posisi tujuan kamera
            Vector3 targetPosisi = targetKarakter.position + jarakOffset;
            
            // Kamera jalan ngikutin secara mulus (smooth) pakai Lerp
            transform.position = Vector3.Lerp(transform.position, targetPosisi, kecepatanKamera * Time.deltaTime);
        }
    }
}