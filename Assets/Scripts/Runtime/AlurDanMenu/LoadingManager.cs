// PANDUAN PROYEK - Runtime / AlurDanMenu
// Fungsi: Memuat scene tujuan di belakang layar dan mengatur perpindahan setelah proses loading.
// Panduan: Assets/Scripts/Runtime/AlurDanMenu/README.md

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static string sceneTujuan = "CharacterLab"; 

    void Start()
    {
        StartCoroutine(MuatDuniaBelakangLayar());
    }

    IEnumerator MuatDuniaBelakangLayar()
    {
        // 1. BEKERJA OPTIMAL: Perintahkan Unity memuat dunia secepat mungkin di latar belakang
        AsyncOperation operasi = SceneManager.LoadSceneAsync(sceneTujuan);
        
        // Kunci pintunya! Jangan izinkan Unity otomatis pindah scene walau loading sudah kelar
        operasi.allowSceneActivation = false;

        // 2. PANTAU PEKERJAAN: Tunggu sampai mesin Unity benar-benar selesai memuat (mentok di 0.9)
        while (operasi.progress < 0.9f)
        {
            yield return null; 
        }

        // 3. WAKTU TAMBAHAN: 
        // Memori dunia baru SUDAH 100% SIAP. 
        // Kita tahan 4 detik penuh di sini agar efek Wiggly-mu puas dinikmati pemain!
        yield return new WaitForSecondsRealtime(4f);

        // 4. EKSEKUSI MULUS: Buka tirai layarnya sekarang!
        operasi.allowSceneActivation = true;
    }
}