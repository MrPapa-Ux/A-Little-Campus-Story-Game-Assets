// PANDUAN PROYEK - Runtime / EfekUI
// Fungsi: Varian efek huruf TMP yang memperbarui mesh pada interval tertentu; dipertahankan untuk sambungan yang ada.
// Panduan: Assets/Scripts/Runtime/EfekUI/README.md

using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class WigglyTMP : MonoBehaviour
{
    [Header("Pengaturan Goyang Teks")]
    public float kekuatanGoyang = 1.5f; // Seberapa jauh urat huruf mleyot
    public float kecepatanGoyang = 10f; // FPS goyangnya (set ke 7-10 biar boiling)

    private TMP_Text komponenTeks;
    private float timer;

    void Start()
    {
        komponenTeks = GetComponent<TMP_Text>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < 1f / kecepatanGoyang) return; // Penahan FPS biar estetik
        timer = 0;

        komponenTeks.ForceMeshUpdate();
        TMP_TextInfo textInfo = komponenTeks.textInfo;

        // Loop setiap karakter
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            // Ambil titik kordinat (vertices) huruf ini
            Vector3[] vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            // Goyang 4 sudut huruf secara acak
            for (int j = 0; j < 4; j++)
            {
                Vector3 origin = vertices[charInfo.vertexIndex + j];
                vertices[charInfo.vertexIndex + j] = origin + new Vector3(Random.Range(-kekuatanGoyang, kekuatanGoyang), Random.Range(-kekuatanGoyang, kekuatanGoyang), 0);
            }
        }

        komponenTeks.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}