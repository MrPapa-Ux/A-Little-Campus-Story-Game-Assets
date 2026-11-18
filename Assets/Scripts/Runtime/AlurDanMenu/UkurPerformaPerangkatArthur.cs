using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Hanya berjalan bila pengujian diminta lewat intent ADB. Tidak aktif saat bermain biasa.
public sealed class UkurPerformaPerangkatArthur : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    string namaJalur;
    readonly List<float> waktu = new List<float>(2400);
    int gcAwal;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Mulai()
    {
        string jalur;
        using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
            jalur = intent.Call<string>("getStringExtra", "arthur_ukur_jalur");
        if (string.IsNullOrEmpty(jalur)) return;
        var go = new GameObject("Pengukuran perangkat - sesi ADB");
        DontDestroyOnLoad(go);
        go.AddComponent<UkurPerformaPerangkatArthur>().namaJalur = jalur;
    }

    IEnumerator Start()
    {
        bool paksaHujan;
        // Opsi diagnosis untuk membandingkan jalur render pada driver perangkat,
        // hanya pada peluncuran pengukuran yang memakai extra ADB.
        using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        {
            paksaHujan = intent.Call<bool>("getBooleanExtra", "arthur_ukur_hujan", false);
            var rp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (rp != null && intent.Call<bool>("getBooleanExtra", "arthur_srp_off", false)) rp.useSRPBatcher = false;
            if (rp != null && intent.Call<bool>("getBooleanExtra", "arthur_msaa_off", false)) rp.msaaSampleCount = 1;
        }
        // Jalur pengujian tidak mengubah titik awal maupun scene cerita yang tersimpan.
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Explorer")
            yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Explorer");
        float batas = Time.realtimeSinceStartup + 90;
        while ((KawasanKampus.Instance == null || !KawasanKampus.Instance.Siap) && Time.realtimeSinceStartup < batas)
            yield return null;
        var arthur = FindFirstObjectByType<ArthurJalurKampus>();
        JalurKampus jalur = null;
        foreach (var calon in FindObjectsByType<JalurKampus>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (calon.name == namaJalur) { jalur = calon; break; }
        if (arthur == null || jalur == null || KawasanKampus.Instance == null)
        { Debug.LogError("ARTHUR_PERF gagal menemukan jalur " + namaJalur); Destroy(gameObject); yield break; }
        var tujuan = new GameObject("Tujuan sementara pengukuran").AddComponent<TujuanJalurKampus>();
        tujuan.transform.SetParent(jalur.transform, false);
        jalur.Sampel(jalur.Panjang * .5f, out var posisi, out _, out _);
        tujuan.transform.position = posisi;
        tujuan.jalur = jalur;
        yield return KawasanKampus.Instance.Pindahkan(tujuan.transform, () => arthur.PindahKe(tujuan.transform));
        var cuaca = SiklusWaktuKampus.Instance;
        if (paksaHujan && cuaca != null)
        {
            cuaca.berjalanOtomatis = false;
            cuaca.PratinjauHujan(true);
        }
        yield return new WaitForSecondsRealtime(8);
        if (paksaHujan && cuaca != null)
        {
            var payung = CariPayung(arthur);
            var perlengkapan = cuaca.GetComponent<PerlengkapanHujanArthur>();
            string suara = "tidak ada";
            if (perlengkapan != null)
                foreach (var audio in perlengkapan.GetComponents<AudioSource>())
                    if (audio.isPlaying && audio.clip != null && audio.volume > .05f) suara = audio.clip.name;
            Debug.Log("ARTHUR_CUACA hujan=" + cuaca.Hujan + " diLuar=" + cuaca.DiLuarRuangan
                + " payung=" + (payung != null && payung.gameObject.activeInHierarchy) + " suara=" + suara);
        }
        var kontrol = arthur.GetComponent<KarakterController>();
        for (int putaran = 0; putaran < 3; putaran++)
        {
            waktu.Clear(); gcAwal = GC.CollectionCount(0);
            // Diam, berjalan kiri, lalu berjalan kanan; kamera mengikuti seperti saat bermain.
            if (putaran == 1) kontrol.JalanKeKiri();
            if (putaran == 2) kontrol.JalanKeKanan();
            float selesai = Time.realtimeSinceStartup + 10;
            while (Time.realtimeSinceStartup < selesai)
            { yield return null; waktu.Add(Time.unscaledDeltaTime * 1000); }
            kontrol.BerhentiBergerak();
            waktu.Sort(); float jumlah = 0; int lambat = 0;
            foreach (float ms in waktu) { jumlah += ms; if (ms > 33.5f) lambat++; }
            var rp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            Debug.Log("ARTHUR_PERF jalur=" + namaJalur + " mode=" + putaran + " frames=" + waktu.Count
                + " fps=" + (waktu.Count * 1000 / jumlah).ToString("F1", System.Globalization.CultureInfo.InvariantCulture)
                + " p95ms=" + waktu[Mathf.Min(waktu.Count - 1, (int)(waktu.Count * .95f))].ToString("F1", System.Globalization.CultureInfo.InvariantCulture)
                + " maxms=" + waktu[waktu.Count - 1].ToString("F1", System.Globalization.CultureInfo.InvariantCulture)
                + " over33ms=" + lambat + " gc=" + (GC.CollectionCount(0) - gcAwal)
                + " scale=" + (rp != null ? rp.renderScale : 1) + " target=" + Application.targetFrameRate
                + " screen=" + Screen.width + "x" + Screen.height);
        }
        Destroy(tujuan.gameObject);
        Destroy(gameObject);
    }

    static Transform CariPayung(ArthurJalurKampus arthur)
    {
        var kontrol = arthur.GetComponent<KarakterController>();
        return kontrol.wadahVisual != null ? kontrol.wadahVisual.Find("Payung Arthur") : null;
    }
#endif
}
