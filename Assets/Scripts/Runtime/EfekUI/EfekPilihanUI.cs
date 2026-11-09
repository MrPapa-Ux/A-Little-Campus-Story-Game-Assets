// PANDUAN PROYEK - Runtime / EfekUI
// Fungsi: Menganimasikan pointer, kedipan teks, dan panel yang sedang dipilih.
// Panduan: Assets/Scripts/Runtime/EfekUI/README.md

using UnityEngine;
using TMPro;

public class EfekPilihanUI : MonoBehaviour
{
    [Header("Pengaturan Pointer")]
    public RectTransform pointerTransform;
    public float jarakGeserX = -15f; 
    [Tooltip("Kecepatan pointer maju-mundur")]
    public float kecepatanGerakPointer = 6f; 
    [Tooltip("Jarak ayunan pointer maju-mundur")]
    public float jarakMajuMundur = 5f;       

    [Header("Pengaturan Teks Berkedip")]
    public TextMeshProUGUI teksPilihan;
    public float kecepatanKedip = 4f;

    [Header("Pengaturan Efek Container (Panel)")]
    public float skalaMembesar = 1.05f;
    [Tooltip("Kecepatan container naik-turun")]
    public float kecepatanNaikTurunPanel = 5f; 
    [Tooltip("Jarak ayunan container naik-turun")]
    public float jarakNaikTurunPanel = 3f;

    public bool sedangDipilih = false;

    private RectTransform rectPanel;
    private Vector2 posisiAwalPanel;
    private Vector3 skalaAwalPanel;
    private Color warnaAwalTeks;
    private Vector2 posisiAwalPointer;
    private Vector2 posisiPanelAsli;
    private bool posisiPanelDisimpan;
    private float tambahanTinggi;

    // Pilihan ketiga memakai ruang di atas dua panel lama. Titik dasar animasi
    // ikut dipindahkan agar efek panel tidak menariknya kembali setiap frame.
    public void AturTambahanTinggi(float tinggi)
    {
        if (rectPanel == null) rectPanel = GetComponent<RectTransform>();
        if (!posisiPanelDisimpan)
        {
            posisiPanelAsli = rectPanel.anchoredPosition;
            posisiPanelDisimpan = true;
        }
        tambahanTinggi = tinggi;
        posisiAwalPanel = posisiPanelAsli + Vector2.up * tambahanTinggi;
        rectPanel.anchoredPosition = posisiAwalPanel;
    }

    void Start()
    {
        rectPanel = GetComponent<RectTransform>();
        AturTambahanTinggi(tambahanTinggi);
        skalaAwalPanel = rectPanel.localScale;

        if (teksPilihan != null) warnaAwalTeks = teksPilihan.color;
        if (pointerTransform != null) posisiAwalPointer = pointerTransform.anchoredPosition;
    }

    void Update()
    {
        if (sedangDipilih)
        {
            // 1. Teks Berkedip
            if (teksPilihan != null)
            {
                Color warnaKedip = warnaAwalTeks;
                warnaKedip.a = Mathf.PingPong(Time.time * kecepatanKedip, 1f);
                teksPilihan.color = warnaKedip;
            }

            // 2. Pointer Maju-Mundur (Sumbu X)
            if (pointerTransform != null)
            {
                pointerTransform.gameObject.SetActive(true);
                float gerakX = Mathf.Sin(Time.time * kecepatanGerakPointer) * jarakMajuMundur;
                pointerTransform.anchoredPosition = new Vector2(posisiAwalPointer.x + jarakGeserX + gerakX, posisiAwalPointer.y);
            }

            // 3. Container Membesar & Naik-Turun (Sumbu Y)
            rectPanel.localScale = skalaAwalPanel * skalaMembesar;
            
            float gerakY = Mathf.Sin(Time.time * kecepatanNaikTurunPanel) * jarakNaikTurunPanel;
            rectPanel.anchoredPosition = new Vector2(posisiAwalPanel.x, posisiAwalPanel.y + gerakY);
        }
        else
        {
            // Reset Semua ke Kondisi Awal
            if (teksPilihan != null) teksPilihan.color = warnaAwalTeks;
            
            if (pointerTransform != null)
            {
                pointerTransform.anchoredPosition = posisiAwalPointer;
                pointerTransform.gameObject.SetActive(false);
            }
            
            rectPanel.localScale = skalaAwalPanel;
            rectPanel.anchoredPosition = posisiAwalPanel;
        }
    }
}
