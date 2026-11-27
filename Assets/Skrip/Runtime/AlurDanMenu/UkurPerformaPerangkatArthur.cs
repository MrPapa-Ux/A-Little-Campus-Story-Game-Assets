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
        bool paksaHujan, ukurTidur, ukurCerita, lihatQuizzo;
        // Opsi diagnosis untuk membandingkan jalur render pada driver perangkat,
        // hanya pada peluncuran pengukuran yang memakai extra ADB.
        using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        {
            paksaHujan = intent.Call<bool>("getBooleanExtra", "arthur_ukur_hujan", false);
            ukurTidur = intent.Call<bool>("getBooleanExtra", "arthur_ukur_tidur", false);
            ukurCerita = intent.Call<bool>("getBooleanExtra", "arthur_ukur_cerita", false);
            lihatQuizzo=intent.Call<bool>("getBooleanExtra","arthur_lihat_quizzo",false);
            int chunk=intent.Call<int>("getIntExtra","arthur_ukur_chunk",-1);if(chunk>=0)JangkauanChunkArthur.PaksaUji=Mathf.Clamp(chunk,0,3);
            var rp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (rp != null && intent.Call<bool>("getBooleanExtra", "arthur_srp_off", false)) rp.useSRPBatcher = false;
            if (rp != null && intent.Call<bool>("getBooleanExtra", "arthur_msaa_off", false)) rp.msaaSampleCount = 1;
        }
        // Jalur pengujian tidak mengubah titik awal maupun scene cerita yang tersimpan.
        string sceneUji = ukurCerita ? "Penataan Kampus - Jalur NPC dan Bubble" : "Explorer";
        // Sesi ukur tidak menimpa lokasi eksplorasi milik pemain. Tutorial hanya
        // disembunyikan pada peluncuran ADB ini agar tidak menutupi pengukuran.
        UnityEngine.Events.UnityAction<UnityEngine.SceneManagement.Scene,UnityEngine.SceneManagement.LoadSceneMode> siapkan = (scene, mode) => {
            foreach(var root in scene.GetRootGameObjects()) {
                foreach(var save in root.GetComponentsInChildren<SimpananEksplorasiArthur>(true))save.enabled=false;
                if(!ukurCerita)continue;
                foreach(var story in root.GetComponentsInChildren<CeritaKamar201Arthur>(true)) {
                    story.StopAllCoroutines();story.enabled=false;
                    if(story.panel!=null)story.panel.SetActive(false);
                    if(story.timer!=null)story.timer.gameObject.SetActive(false);
                }
                foreach(var bar in root.GetComponentsInChildren<LazyBarArthur>(true))bar.tanpaBatasWaktu=true;
                foreach(var story in root.GetComponentsInChildren<PerjalananChapelChapter1>(true)){story.StopAllCoroutines();story.enabled=false;}
                foreach(var story in root.GetComponentsInChildren<MalamGAChapter1>(true)){story.StopAllCoroutines();story.enabled=false;}
            }
        };
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += siapkan;
        try {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != sceneUji)
                yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneUji);
            else siapkan(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),UnityEngine.SceneManagement.LoadSceneMode.Single);
        } finally { UnityEngine.SceneManagement.SceneManager.sceneLoaded -= siapkan; }
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
        if(!ukurTidur&&cuaca!=null){cuaca.berjalanOtomatis=false;cuaca.AturJam(9);}
        if (paksaHujan && cuaca != null)
        {
            cuaca.berjalanOtomatis = false;
            cuaca.PratinjauHujan(true);
        }
        yield return new WaitForSecondsRealtime(8);
        if(ukurCerita) {
            var quizzes=FindObjectsByType<MrQuizoChapter1>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            int titikQuizza=0;foreach(var q in quizzes)if(q.namaPenjaga=="Mrs. Quizza")titikQuizza=q.posisiJaga!=null?q.posisiJaga.Length:0;
            Debug.Log("ARTHUR_KUIS scene="+sceneUji+" penjaga="+quizzes.Length+" titikQuizza="+titikQuizza);
        }
        if(ukurTidur)
        {
            yield return UjiTidur(arthur,cuaca);
            Destroy(tujuan.gameObject);Destroy(gameObject);yield break;
        }
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
        kontrol.BerhentiBergerak();
        for (int putaran = 0; putaran < (lihatQuizzo?0:3); putaran++)
        {
            kontrol.BerhentiBergerak();jalur.Sampel(jalur.Panjang*.5f,out var tengahUji,out _,out _);tujuan.transform.position=tengahUji;arthur.PindahKe(tujuan.transform);yield return new WaitForSecondsRealtime(1);
            waktu.Clear(); gcAwal = GC.CollectionCount(0);
            // Diam, berjalan kiri, lalu berjalan kanan; kamera mengikuti seperti saat bermain.
            if (putaran == 1) kontrol.JalanKeKiri();
            if (putaran == 2) kontrol.JalanKeKanan();
            float selesai = Time.realtimeSinceStartup + 10;
            while (Time.realtimeSinceStartup < selesai)
            {
                if(putaran>0){float kiri=Mathf.Max(.5f,jalur.Panjang*.5f-6),kanan=Mathf.Min(jalur.Panjang-.5f,jalur.Panjang*.5f+6);if(arthur.JarakDiJalur<=kiri)kontrol.JalanKeKanan();else if(arthur.JarakDiJalur>=kanan)kontrol.JalanKeKiri();}
                if(arthur.JalurAktif!=jalur || KawasanKampus.Instance.KawasanAktif!=KawasanKampus.IdUntuk(jalur)){
                    kontrol.BerhentiBergerak();Debug.LogError("ARTHUR_PERF gagal rute berpindah: "+(arthur.JalurAktif!=null?arthur.JalurAktif.name:"null")+" di "+KawasanKampus.Instance.KawasanAktif+", diharapkan "+namaJalur);Destroy(gameObject);yield break;
                }
                yield return null; waktu.Add(Time.unscaledDeltaTime * 1000);
            }
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
            Debug.Log("ARTHUR_CHUNK pilihan="+JangkauanChunkArthur.Pilihan+" jarak="+JangkauanChunkArthur.Jarak+" terlihat="+JangkauanChunkArthur.Terlihat+" total="+JangkauanChunkArthur.Total);
        }
        if(namaJalur=="GK2_JalurUtama_BelokOtomatis"){
            foreach(var q in FindObjectsByType<MrQuizoChapter1>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(q.namaPenjaga=="Mr. Quizzo"){
                if(q.posisiJaga.Length==1){var pilih=FindFirstObjectByType<SistemPilihan>();if(pilih!=null){if(pilih.grupFisikDepan!=null)pilih.grupFisikDepan.SetActive(true);if(pilih.grupFisikBelakang!=null)pilih.grupFisikBelakang.SetActive(false);if(pilih.grupFisikRuangan!=null)pilih.grupFisikRuangan.SetActive(false);}tujuan.jalur=null;tujuan.transform.position=q.posisiJaga[0].position+Vector3.up*ArthurJalurKampus.JarakKaki+Vector3.left*1.3f;yield return KawasanKampus.Instance.Pindahkan(tujuan.transform,()=>arthur.PindahKe(tujuan.transform));yield return new WaitForSecondsRealtime(1);}
                var n=q.GetComponent<NpcYangBisaDiatur>();float jarak=Vector3.Distance(arthur.transform.position-Vector3.up*ArthurJalurKampus.JarakKaki,q.transform.position);
                Debug.Log("ARTHUR_QUIZZO aktif="+q.isActiveAndEnabled+" jarak="+jarak+" dalamJangkauan="+(jarak<n.jarakInteraksi)+" route="+(n.jalurInteraksi==arthur.JalurAktif)+" titik="+q.posisiJaga.Length);
            }
        }
        var hpUji=FindFirstObjectByType<SistemHP>();
        if(lihatQuizzo){Debug.Log("ARTHUR_PRECHECK fotoQuizzo=siap");Destroy(tujuan.gameObject);Destroy(gameObject);yield break;}
        if(hpUji!=null){hpUji.BukaHPDefault();yield return new WaitForSecondsRealtime(2);hpUji.BukaPengaturan();yield return new WaitForSecondsRealtime(2);
            var pm=hpUji.panelPengaturan.GetComponent<PengaturanManager>();int saved=PlayerPrefs.GetInt(JangkauanChunkArthur.Kunci,1);var paksa=JangkauanChunkArthur.PaksaUji;
            bool ada=pm!=null&&pm.sliderChunk!=null&&pm.teksChunk!=null;
            if(ada){pm.UbahChunk(0);bool dekat=PlayerPrefs.GetInt(JangkauanChunkArthur.Kunci,-1)==0;pm.UbahChunk(3);bool utuh=PlayerPrefs.GetInt(JangkauanChunkArthur.Kunci,-1)==3;pm.UbahChunk(saved);pm.SinkronkanPengaturan();JangkauanChunkArthur.PaksaUji=paksa;Debug.Log("ARTHUR_PRECHECK pengaturan="+ada+" simpanDekat="+dekat+" simpanUtuh="+utuh+" volume="+(pm.sliderVolume!=null)+" cahaya="+(pm.sliderCahaya!=null));}
        }
        Destroy(tujuan.gameObject);
        Destroy(gameObject);
    }

    IEnumerator UjiTidur(ArthurJalurKampus arthur,SiklusWaktuKampus cuaca)
    {
        var malam=MalamEksplorasiArthur.Instance;
        if(malam==null || cuaca==null){Debug.LogError("ARTHUR_MALAM gagal pengatur tidak ada");yield break;}
        var luar=arthur.JalurAktif;
        cuaca.berjalanOtomatis=false;cuaca.AturJam(21);
        float batas=Time.realtimeSinceStartup+45;
        while((arthur.JalurAktif==luar || malam.SedangPulang) && Time.realtimeSinceStartup<batas)yield return null;
        yield return new WaitForSecondsRealtime(1);
        bool pulang=arthur.JalurAktif!=null&&arthur.JalurAktif.name=="GH_Kamar201"&&malam.KamarDikunci&&cuaca.Jam==21;
        Debug.Log("ARTHUR_MALAM pulang21="+pulang);
        malam.MintaTidur();yield return new WaitForSecondsRealtime(8);
        Debug.Log("ARTHUR_MALAM tidurPilihan="+(cuaca.Jam==7&&!malam.KamarDikunci&&!arthur.GetComponent<KarakterController>().kunciPergerakan));
        cuaca.AturJam(21);yield return new WaitForSecondsRealtime(1);cuaca.AturJam(23);yield return new WaitForSecondsRealtime(8);
        Debug.Log("ARTHUR_MALAM tidur23="+(cuaca.Jam==7&&!malam.KamarDikunci&&!malam.SedangTidur));
    }

    static Transform CariPayung(ArthurJalurKampus arthur)
    {
        var kontrol = arthur.GetComponent<KarakterController>();
        return kontrol.wadahVisual != null ? kontrol.wadahVisual.Find("Payung Arthur") : null;
    }
#endif
}
