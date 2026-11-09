// PANDUAN PROYEK - Runtime / Dunia
// Fungsi: Mengatur siklus burung terbang, durasi terbang, dan jeda kemunculan.
// Panduan: Assets/Scripts/Runtime/Dunia/README.md

using System.Collections;
using UnityEngine;

public class BurungTerbang : MonoBehaviour
{
    [Header("Pengaturan Penerbangan")]
    public float kecepatan = 10f; 
    public float durasiTerbang = 15f; 
    public float jedaWaktu = 45f; 

    private Vector3 posisiAwal;

    // Awake dipanggil paling awal SATU KALI saja untuk mencatat sarang
    void Awake()
    {
        posisiAwal = transform.position;
    }

    // OnEnable dipanggil SETIAP KALI area depan dinyalakan!
    void OnEnable()
    {
        // Reset posisi burung ke sarang tiap kali Arthur keluar asrama
        transform.position = posisiAwal;
        
        // Nyalakan mesin terbangnya lagi!
        StartCoroutine(SiklusTerbang());
    }

    IEnumerator SiklusTerbang()
    {
        while (true) 
        {
            float waktu = 0f;
            while (waktu < durasiTerbang)
            {
                transform.Translate(Vector3.forward * kecepatan * Time.deltaTime);
                waktu += Time.deltaTime;
                yield return null; 
            }

            transform.position = new Vector3(posisiAwal.x, -1000f, posisiAwal.z);
            yield return new WaitForSeconds(jedaWaktu);
            transform.position = posisiAwal;
        }
    }
}