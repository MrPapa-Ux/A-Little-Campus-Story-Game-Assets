// PANDUAN PROYEK - Runtime / EfekUI
// Fungsi: Menggoyangkan posisi container UI dari posisi awal.
// Panduan: Assets/Scripts/Runtime/EfekUI/README.md

using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class WigglyContainer : MonoBehaviour
{
    [Header("Pengaturan Goyang Kotak")]
    public float kekuatanGoyang = 2.5f; // Seberapa jauh kotak lari dari posisi asli
    public float kecepatanGoyang = 10f; // FPS goyangnya

    private RectTransform rectTransform;
    private Vector2 posisiAsli;
    private float timer;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        posisiAsli = rectTransform.anchoredPosition; // Catat posisi awal biar ga kabur ke pojok lain wkwk
    }

    public void AturPosisiDasar()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        posisiAsli = rectTransform.anchoredPosition;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < 1f / kecepatanGoyang) return;
        timer = 0;

        // Ubah posisi anchored sedikit secara acak dari posisi asli
        rectTransform.anchoredPosition = posisiAsli + new Vector2(Random.Range(-kekuatanGoyang, kekuatanGoyang), Random.Range(-kekuatanGoyang, kekuatanGoyang));
    }
}
