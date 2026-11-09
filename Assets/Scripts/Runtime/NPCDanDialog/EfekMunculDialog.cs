// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Menganimasikan muncul dan tutupnya panel dialog.
// Panduan: Assets/Scripts/Runtime/NPCDanDialog/README.md

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class EfekMunculDialog : MonoBehaviour
{
    [Header("Pengaturan Animasi")]
    [Tooltip("Waktu yang dibutuhkan sampai dialog muncul penuh (detik)")]
    public float durasiMuncul = 0.3f; 
    public float durasiTutup = 0.2f; 
    
    [Tooltip("Aktifkan untuk efek membesar perlahan saat muncul")]
    public bool pakaiEfekZoom = true;
    public Vector3 skalaAwal = new Vector3(0.9f, 0.9f, 0.9f);
    private Vector3 skalaAkhir = Vector3.one;
    public void PerbesarBidang(Transform target,float faktor)
    {
        if(Bidang!=target)return;
        skalaAwal*=faktor;
        skalaAkhir*=faktor;
    }
    public Transform bidangAnimasi;
    Transform Bidang => bidangAnimasi!=null?bidangAnimasi:transform;

    private CanvasGroup canvasGroup;
    private Coroutine rutinAnimasi;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // A root Canvas scale belongs to CanvasScaler. Animate its content instead.
        if(bidangAnimasi==null&&GetComponent<Canvas>()!=null&&transform.childCount>0)bidangAnimasi=transform.GetChild(0);
    }

    void OnEnable()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        
        if (rutinAnimasi != null) StopCoroutine(rutinAnimasi);
        rutinAnimasi = StartCoroutine(AnimasiMasuk());
    }

    IEnumerator AnimasiMasuk()
    {
        canvasGroup.alpha = 0f;
        if (pakaiEfekZoom) Bidang.localScale = skalaAwal;
        else Bidang.localScale = skalaAkhir;

        float waktu = 0f;
        while (waktu < durasiMuncul)
        {
            waktu += Time.deltaTime;
            float persentase = waktu / durasiMuncul;
            float nilaiSmooth = Mathf.SmoothStep(0f, 1f, persentase);

            canvasGroup.alpha = nilaiSmooth;
            if (pakaiEfekZoom)
                Bidang.localScale = Vector3.Lerp(skalaAwal, skalaAkhir, nilaiSmooth);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        Bidang.localScale = skalaAkhir;
    }

    // Fungsi yang dipanggil oleh SistemPilihan.cs untuk menutup UI
    public void TutupDialog()
    {
        if (gameObject.activeInHierarchy)
        {
            if (rutinAnimasi != null) StopCoroutine(rutinAnimasi);
            rutinAnimasi = StartCoroutine(AnimasiKeluar());
        }
    }

    IEnumerator AnimasiKeluar()
    {
        float waktu = 0f;
        float alphaAwal = canvasGroup.alpha;
        Vector3 skalaSekarang = Bidang.localScale;

        while (waktu < durasiTutup)
        {
            waktu += Time.deltaTime;
            float persentase = waktu / durasiTutup;
            float nilaiSmooth = Mathf.SmoothStep(0f, 1f, persentase);

            canvasGroup.alpha = Mathf.Lerp(alphaAwal, 0f, nilaiSmooth);
            if (pakaiEfekZoom)
                Bidang.localScale = Vector3.Lerp(skalaSekarang, skalaAwal, nilaiSmooth);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false); // Matikan canvas di detik terakhir animasi
    }
}
