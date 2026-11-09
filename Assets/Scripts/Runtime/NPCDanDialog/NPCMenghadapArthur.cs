// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Membalik visual NPC agar menghadap Arthur, lalu kembali ke arah semula.
// Panduan: Assets/Scripts/Runtime/NPCDanDialog/README.md

using System.Collections;
using UnityEngine;

public class NPCMenghadapArthur : MonoBehaviour
{
    public Transform wadahVisual;
    [Min(0.05f)] public float durasiPutar = 0.24f;
    private Vector3 skalaSemula;
    private Coroutine putaran;
    private bool siap;

    void Awake()
    {
        if (wadahVisual == null) return;
        skalaSemula = wadahVisual.localScale;
        siap = true;
    }
    public void HadapArthur(Transform arthur)
    {
        if (!siap || arthur == null || !isActiveAndEnabled) return;
        var kamera = Camera.main;
        var selisihDunia = arthur.position - transform.position;
        float selisih = kamera != null ? Vector3.Dot(selisihDunia, kamera.transform.right) : selisihDunia.x;
        if (Mathf.Abs(selisih) < 0.02f) return;
        PutarKe(Mathf.Abs(skalaSemula.x) * Mathf.Sign(selisih));
    }
    public void KembaliSemula()
    {
        if (!siap) return;
        if (!gameObject.activeInHierarchy || !enabled) { wadahVisual.localScale = skalaSemula; return; }
        PutarKe(skalaSemula.x);
    }
    private void PutarKe(float x)
    {
        if (putaran != null) StopCoroutine(putaran);
        if (Mathf.Abs(wadahVisual.localScale.x - x) < 0.001f) return;
        putaran = StartCoroutine(PutarKertas(x));
    }
    private IEnumerator PutarKertas(float tujuan)
    {
        float awal = wadahVisual.localScale.x, waktu = 0f;
        bool balik = Mathf.Sign(awal) != Mathf.Sign(tujuan);
        while (waktu < durasiPutar)
        {
            waktu += Time.deltaTime;
            float t = Mathf.Clamp01(waktu / durasiPutar);
            float x = balik
                ? (t < 0.5f ? Mathf.Lerp(awal, Mathf.Sign(awal) * 0.015f, Mathf.SmoothStep(0f, 1f, t * 2f)) : Mathf.Lerp(Mathf.Sign(tujuan) * 0.015f, tujuan, Mathf.SmoothStep(0f, 1f, (t - 0.5f) * 2f)))
                : Mathf.Lerp(awal, tujuan, Mathf.SmoothStep(0f, 1f, t));
            wadahVisual.localScale = new Vector3(x, skalaSemula.y, skalaSemula.z);
            yield return null;
        }
        wadahVisual.localScale = new Vector3(tujuan, skalaSemula.y, skalaSemula.z);
        putaran = null;
    }
    void OnDisable()
    {
        if (putaran != null) StopCoroutine(putaran);
        putaran = null;
        if (siap && wadahVisual != null) wadahVisual.localScale = skalaSemula;
    }
}
