// PANDUAN PROYEK - Runtime / AlurDanMenu
// Fungsi: Mengatur urutan teks chapter/alasan, fade, dan mengirim tujuan ke proses loading.
// Panduan: Assets/Scripts/Runtime/AlurDanMenu/README.md

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SutradaraTransisi : MonoBehaviour
{
    [Header("Aktor Teks (Seret dari Hierarchy)")]
    public TextMeshProUGUI teksChapter;
    public TextMeshProUGUI teksAlasan;
    
    [Header("Pengaturan Sutradara")]
    public float kecepatanFade = 1.5f;
    [Min(0)] public float durasiBacaChapter = 3f;
    [Min(0)] public float durasiBacaPertanyaan = 6f;
    [Header("Kenangan Arthur sebelum adegan mengetik")]
    public KenanganPembukaArthur kenanganPembuka;

    [Header("Tujuan Lemparan Scene")]
    [Tooltip("Ketik nama scene tujuan di sini (Contoh: CharacterLab atau Chapter1_Arthur)")]
    public string namaSceneTujuan = "CharacterLab";

    // ✅ JEMBATAN DATA: Variabel statis untuk dibaca oleh LoadingScene
    public static string sceneTujuanUtama;

    bool lewatiTeks; LewatiCutsceneArthur lewati;
    IEnumerator Tunggu(float detik){float t=0;while(!lewatiTeks&&t<detik){t+=Time.unscaledDeltaTime;yield return null;}}
    void Start()
    {
        // Langsung mulai adegan saat layar ini terbuka
        lewati=GetComponent<LewatiCutsceneArthur>()??gameObject.AddComponent<LewatiCutsceneArthur>();lewati.Mulai(()=>lewatiTeks=true,teksAlasan.font);
        StartCoroutine(AdeganMetaMessage());
    }

    IEnumerator AdeganMetaMessage()
    {
        yield return Tunggu(1f); // Jeda gelap awal

        // Muncul Teks "Chapter 1"
        yield return LakukanFade(teksChapter, 0f, 1f);
        yield return Tunggu(durasiBacaChapter); 

        // Teks "Chapter 1" Menghilang
        yield return LakukanFade(teksChapter, 1f, 0f);
        yield return Tunggu(1f); 

        // Muncul Teks Tamparan Realita
        yield return LakukanFade(teksAlasan, 0f, 1f);
        yield return Tunggu(durasiBacaPertanyaan); 

        // Teks Tamparan Realita Menghilang
        yield return LakukanFade(teksAlasan, 1f, 0f);
        yield return Tunggu(1f);

        // ✅ REVISI OPTIMASI: Titip nama tujuan, lalu lempar ke LoadingScene dulu!
        // Delapan gambar dimainkan dalam scene ini, sebelum adegan mengetik.
        lewati.Selesai();teksChapter.alpha=0;teksAlasan.alpha=0;
        if (kenanganPembuka != null)
            yield return kenanganPembuka.Mainkan();
        sceneTujuanUtama = namaSceneTujuan;
        LoadingManager.sceneTujuan = namaSceneTujuan;
        SceneManager.LoadScene("LoadingScene");
    }

    IEnumerator LakukanFade(TextMeshProUGUI aktorTeks, float alphaAwal, float alphaTujuan)
    {
        float waktu = 0f;
        Color warnaTeks = aktorTeks.color;

        while (!lewatiTeks && waktu < 1f)
        {
            waktu += Time.deltaTime * kecepatanFade;
            warnaTeks.a = Mathf.Lerp(alphaAwal, alphaTujuan, waktu);
            aktorTeks.color = warnaTeks;
            yield return null; 
        }
    }
}
