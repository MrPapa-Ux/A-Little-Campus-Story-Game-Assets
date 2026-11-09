// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Memberi getaran dan dentuman pada dialog berteriak.
// Panduan: Assets/Scripts/Runtime/NPCDanDialog/README.md

using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class DialogTeriakGoofy : MonoBehaviour
{
    [Header("Dentuman dialog")]
    [Range(0f, 1f)] public float volumeDentuman = .18f;
    public AudioClip suaraPengganti;
    public bool SedangTeriak { get; private set; }
    public int JumlahDentuman { get; private set; }
    TMP_Text text;
    Vector3 scale;
    bool bold;
    AudioSource audioSource;
    AudioClip generated;
    float nextBeat, startBeat = -10f, strength;
    void Awake()
    {
        text = GetComponent<TMP_Text>(); scale = transform.localScale; bold = (text.fontStyle & FontStyles.Bold) != 0;
        audioSource = gameObject.AddComponent<AudioSource>(); audioSource.playOnAwake = false; audioSource.spatialBlend = 0f;
        const int rate = 22050; var samples = new float[(int)(rate * .09f)];
        for (int i = 0; i < samples.Length; i++) { float t = i / (float)rate; samples[i] = Mathf.Sin(2f * Mathf.PI * (120f * t - 400f * t * t)) * Mathf.Exp(-t * 60f) * Mathf.Min(1f, t * 1000f); }
        generated = AudioClip.Create("DentumanKertas", samples.Length, 1, rate, false); generated.SetData(samples, 0);
        if (SedangTeriak) text.fontStyle |= FontStyles.Bold;
    }
    public void Atur(bool teriak, float kekuatan = 1f)
    {
        SedangTeriak = teriak; strength = Mathf.Clamp(kekuatan, .25f, 2f); nextBeat = 0f; startBeat = -10f;
        if (text == null) return;
        if (teriak) text.fontStyle |= FontStyles.Bold;
        else { if (!bold) text.fontStyle &= ~FontStyles.Bold; transform.localScale = scale; }
    }
    public void Huruf(char huruf)
    {
        if (!SedangTeriak || !char.IsLetterOrDigit(huruf) || Time.unscaledTime < nextBeat) return;
        startBeat = Time.unscaledTime; nextBeat = startBeat + .42f; JumlahDentuman++;
        audioSource.PlayOneShot(suaraPengganti != null ? suaraPengganti : generated, volumeDentuman * strength);
    }
    void LateUpdate()
    {
        if (!SedangTeriak) return;
        float t = Time.unscaledTime - startBeat;
        float pulse = t < .28f ? Mathf.Sin(t / .28f * Mathf.PI) * Mathf.Exp(-t * 8f) : 0f;
        transform.localScale = Vector3.Scale(scale, new Vector3(1f + pulse * .08f * strength, 1f + pulse * .18f * strength, 1f));
    }
    void OnDisable() { Atur(false); if (audioSource != null) audioSource.Stop(); }
    void OnDestroy() { if (generated != null) Destroy(generated); }
}
