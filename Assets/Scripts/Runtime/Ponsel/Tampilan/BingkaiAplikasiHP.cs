// PANDUAN PROYEK - Runtime / Ponsel/Tampilan
// Fungsi: Menyamakan warna bingkai aplikasi dengan warna badan HP saat diperlukan.
// Panduan: Assets/Scripts/Runtime/Ponsel/Tampilan/README.md

using UnityEngine;

public class BingkaiAplikasiHP : MonoBehaviour
{
    public SistemHP sistemHP;
    public UnityEngine.UI.Image bingkai;
    void OnEnable() { if (sistemHP != null && sistemHP.ponselBersama==null && sistemHP.badanHP != null && bingkai != null) bingkai.color = sistemHP.badanHP.color; }
}
