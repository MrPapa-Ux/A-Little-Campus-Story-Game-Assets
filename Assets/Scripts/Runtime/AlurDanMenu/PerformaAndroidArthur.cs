using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Resolusi dunia menyesuaikan beban. Canvas layar tetap tajam pada resolusi penuh.
public sealed class PerformaAndroidArthur : MonoBehaviour
{
    public const int TargetFPS = 60;
    public const int BatasSisiPendekDunia = 1080;
    public const int SisiPendekAwal = 960;
    public const int SisiPendekMinimum = 720;
    public static float HitungSkala(int lebar, int tinggi) => SkalaUntuk(lebar, tinggi, BatasSisiPendekDunia);
    public static float SkalaUntuk(int lebar, int tinggi, int sisiPendek)
    {
        int pendek = Mathf.Min(lebar, tinggi);
        return pendek > 0 ? Mathf.Clamp((float)sisiPendek / pendek, .1f, 1f) : 1f;
    }
    // Headroom sebelum 16.67 ms; naik perlahan agar kualitas tidak bolak-balik.
    public static float SesuaikanSkala(float skala, float minimum, float maksimum, float rataMs, ref int stabil)
    {
        if (rataMs > 18f) { stabil = 0; return Mathf.Clamp(skala - .075f, minimum, maksimum); }
        if (rataMs < 16.9f) { if (++stabil >= 6) { stabil = 0; return Mathf.Clamp(skala + .025f, minimum, maksimum); } }
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
        instance = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TargetFPS;
        pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (pipeline != null) skalaSemula = pipeline.renderScale;
        RenderPipelineManager.beginCameraRendering += SebelumKamera;
        AturResolusiDunia();
    }
    static void SebelumKamera(ScriptableRenderContext context, Camera kamera)
    {
        if (kamera.TryGetComponent<UniversalAdditionalCameraData>(out var data))
        {
            data.renderPostProcessing = false;
            data.antialiasing = AntialiasingMode.None;
            data.renderShadows = false;
        }
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
        if (JedaSaatTidakAktif.Dijeda || (kawasan != null && kawasan.SedangMemuat))
        { jumlahDetik = 0; bingkai = stabil = 0; lewatiSampai = Time.unscaledTime + 2f; return; }
        jumlahDetik += Time.unscaledDeltaTime; bingkai++;
        if (jumlahDetik < 2f || bingkai < 20) return;
        float rataMs = jumlahDetik * 1000f / bingkai;
        // Target tetap 60 saat berpindah dari area berat ke area yang lebih ringan.
        float skala = SesuaikanSkala(pipeline.renderScale, minimum, maksimum, rataMs, ref stabil);
        if (!Mathf.Approximately(skala, pipeline.renderScale)) pipeline.renderScale = skala;
        jumlahDetik = 0; bingkai = 0;
    }
    void OnApplicationFocus(bool fokus)
    {
        if (!fokus) return;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = TargetFPS;
        AturResolusiDunia();
    }
    void OnDestroy() { if (instance != this) return; Screen.sleepTimeout = SleepTimeout.SystemSetting; RenderPipelineManager.beginCameraRendering -= SebelumKamera; if (pipeline != null) pipeline.renderScale = skalaSemula; instance = null; }
#endif
}
