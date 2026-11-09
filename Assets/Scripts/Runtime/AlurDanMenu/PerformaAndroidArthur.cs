using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Resolusi dunia menyesuaikan beban. Canvas layar tetap tajam pada resolusi penuh.
public sealed class PerformaAndroidArthur : MonoBehaviour
{
    public const int TargetFPS = 60;
    public const int BatasSisiPendekDunia = 720;
    public const int SisiPendekAwal = 600;
    public const int SisiPendekMinimum = 540;
    public static float HitungSkala(int lebar, int tinggi) => SkalaUntuk(lebar, tinggi, BatasSisiPendekDunia);
    public static float SkalaUntuk(int lebar, int tinggi, int sisiPendek)
    {
        int pendek = Mathf.Min(lebar, tinggi);
        return pendek > 0 ? Mathf.Clamp((float)sisiPendek / pendek, .25f, .8f) : .8f;
    }
    // Turun ketika lambat berkelanjutan, pulih setelah empat jendela stabil.
    public static float SesuaikanSkala(float skala, float minimum, float maksimum, float rataMs, ref int stabil)
    {
        if (rataMs > 19.5f) { stabil = 0; return Mathf.Max(minimum, skala - .05f); }
        if (rataMs < 17.2f) { if (++stabil >= 4) { stabil = 0; return Mathf.Min(maksimum, skala + .025f); } }
        else stabil = 0;
        return Mathf.Clamp(skala, minimum, maksimum);
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    static PerformaAndroidArthur instance;
    UniversalRenderPipelineAsset pipeline;
    float skalaSemula, minimum, maksimum, jumlahDetik, lewatiSampai;
    int lebarLama, tinggiLama, bingkai, stabil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSesi() { instance = null; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Mulai()
    {
        if (instance != null) return;
        var go = new GameObject("Performa Android - target 60 FPS");
        DontDestroyOnLoad(go); go.AddComponent<PerformaAndroidArthur>();
    }
    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this; Application.targetFrameRate = TargetFPS;
        pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (pipeline != null) skalaSemula = pipeline.renderScale;
        AturResolusiDunia();
    }
    void AturResolusiDunia()
    {
        lebarLama = Screen.width; tinggiLama = Screen.height;
        if (pipeline != null)
        {
            maksimum = Mathf.Min(skalaSemula, HitungSkala(lebarLama, tinggiLama));
            minimum = Mathf.Min(maksimum, SkalaUntuk(lebarLama, tinggiLama, SisiPendekMinimum));
            pipeline.renderScale = Mathf.Clamp(SkalaUntuk(lebarLama, tinggiLama, SisiPendekAwal), minimum, maksimum);
        }
        jumlahDetik = 0; bingkai = stabil = 0; lewatiSampai = Time.unscaledTime + 3f;
    }
    void Update()
    {
        if (Screen.width != lebarLama || Screen.height != tinggiLama) AturResolusiDunia();
        if (pipeline == null || Time.unscaledTime < lewatiSampai) return;
        var kawasan = KawasanKampus.Instance;
        // Loading dan resume tidak dianggap beban render yang berkelanjutan.
        if ((kawasan != null && kawasan.SedangMemuat) || Time.unscaledDeltaTime > .1f)
        { jumlahDetik = 0; bingkai = stabil = 0; lewatiSampai = Time.unscaledTime + 2f; return; }
        jumlahDetik += Time.unscaledDeltaTime; bingkai++;
        if (jumlahDetik < 2f || bingkai < 20) return;
        float skala = SesuaikanSkala(pipeline.renderScale, minimum, maksimum, jumlahDetik * 1000f / bingkai, ref stabil);
        if (!Mathf.Approximately(skala, pipeline.renderScale)) pipeline.renderScale = skala;
        jumlahDetik = 0; bingkai = 0;
    }
    void OnApplicationFocus(bool fokus) { if (fokus) { Application.targetFrameRate = TargetFPS; AturResolusiDunia(); } }
    void OnDestroy() { if (instance != this) return; if (pipeline != null) pipeline.renderScale = skalaSemula; instance = null; }
#endif
}
