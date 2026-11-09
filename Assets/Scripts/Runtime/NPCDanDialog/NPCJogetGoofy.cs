// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Menggerakkan bagian tubuh NPC untuk tarian goofy tanpa menggeser bubble.
// Panduan: Assets/Scripts/Runtime/NPCDanDialog/README.md

using UnityEngine;

public class NPCJogetGoofy : MonoBehaviour
{
    [Header("Joget dengan bagian tubuh; bubble tetap di tempat")]
    public bool sedangJoget = true;
    [Range(.25f, 3f)] public float kecepatan = 1.5f;
    [Range(.1f, 2f)] public float kekuatan = 1f;
    public Transform badan, kepala, tanganKiri, tanganKanan, kakiKiri, kakiKanan;
    public Transform rambut, telinga;
    Transform[] parts;
    Vector3[] posisi;
    Quaternion[] rotasi;
    void Awake(){SimpanPoseDasar();}
    public void SimpanPoseDasar()
    {
        parts = new[] { badan, kepala, tanganKiri, tanganKanan, kakiKiri, kakiKanan, rambut, telinga };
        posisi = new Vector3[parts.Length]; rotasi = new Quaternion[parts.Length];
        for (int i = 0; i < parts.Length; i++) if (parts[i] != null) { posisi[i] = parts[i].localPosition; rotasi[i] = parts[i].localRotation; }
    }
    void LateUpdate()
    {
        if (!sedangJoget) { ResetPose(); return; }
        float phase = Time.time * kecepatan * Mathf.PI * 2f;
        float sway = Mathf.Sin(phase);
        Vector3 hip = new Vector3(sway * .11f, -(1f - Mathf.Cos(phase * 2f)) * .035f, 0f) * kekuatan;
        Quaternion headTilt = Quaternion.Euler(0f, 0f, sway * 7f * kekuatan);
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == null) continue;
            bool foot = i == 4 || i == 5, headPart = i == 1 || i >= 6;
            if (foot)
            {
                // Kedua telapak tetap menapak. Pergeseran kecil memberi shuffle, tanpa mengangkat pose keseluruhan.
                parts[i].localPosition = posisi[i] + new Vector3(sway * .025f * (i == 4 ? 1f : -1f) * kekuatan, 0f, 0f);
                parts[i].localRotation = rotasi[i];
            }
            else if (headPart)
            {
                parts[i].localPosition = posisi[1] + hip + headTilt * (posisi[i] - posisi[1]);
                parts[i].localRotation = headTilt * rotasi[i];
            }
            else if (i == 2 || i == 3)
            {
                float wave = Mathf.Sin(phase + (i == 2 ? 0f : Mathf.PI));
                parts[i].localPosition = posisi[i] + hip + new Vector3(wave * .13f, Mathf.Sin(phase * 2f) * .07f, 0f) * kekuatan;
                parts[i].localRotation = rotasi[i] * Quaternion.Euler(0f, 0f, wave * 32f * kekuatan);
            }
            else { parts[i].localPosition = posisi[i] + hip; parts[i].localRotation = rotasi[i] * Quaternion.Euler(0f, 0f, sway * 9f * kekuatan); }
        }
    }
    void ResetPose() { if (parts == null) return; for (int i = 0; i < parts.Length; i++) if (parts[i] != null) { parts[i].localPosition = posisi[i]; parts[i].localRotation = rotasi[i]; } }
    void OnDisable() { ResetPose(); }
}
