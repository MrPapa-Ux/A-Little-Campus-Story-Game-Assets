// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Gameplay; alat pengujian Editor, bukan komponen gameplay.
// Panduan: Assets/Scripts/Editor/Pemeriksaan/README.md

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Pemeriksaan perilaku di Editor; tidak disertakan dalam build Android.
public static class CampusGameplayChecks
{
    const string Session = "CampusCheckpoint5Checks.Running";
    static IEnumerator routine;
    static readonly List<string> checks = new List<string>();
    static string Root => Directory.GetParent(Application.dataPath).FullName;

    [InitializeOnLoadMethod]
    static void Register()
    {
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    static void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        string shakeRequest = Path.Combine(Root, "work/arthur-shake-checks.request");
        bool shakeOnly = File.Exists(shakeRequest);
        string request = shakeOnly ? shakeRequest : Path.Combine(Root, "work/checkpoint5-checks.request");
        if (!SessionState.GetBool(Session, false))
        {
            if (!File.Exists(request)) return;
            if (File.Exists(Path.Combine(Root, "work/settings-revision.request")) || File.Exists(Path.Combine(Root, "work/settings-revision.validate"))) return;
            string applied = Path.Combine(Root, "work/checkpoint5-revision-state.txt");
            if (!File.Exists(applied) || !File.ReadAllText(applied).StartsWith("COMPLETE")) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                File.WriteAllText(Path.Combine(Root, "work/gameplay-checks-state.txt"), "WAITING: Editor masih Play Mode. Pengujian dimulai setelah Play berhenti.");
                return;
            }
            File.Delete(request);
            SessionState.SetBool(Session + ".ShakeOnly", shakeOnly);
            File.WriteAllText(Path.Combine(Root, "work/gameplay-checks-state.txt"), "RUNNING: Memasuki Play Mode untuk pemeriksaan otomatis.");
            SessionState.SetString(Session + ".Scene", SceneManager.GetActiveScene().path);
            RememberPref("MasterVolume");
            RememberPref("MasterBrightness");
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity", OpenSceneMode.Single);
            SessionState.SetBool(Session, true);
            EditorApplication.isPlaying = true;
            return;
        }
        if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorApplication.isPlaying)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            RestorePref("MasterVolume");
            RestorePref("MasterBrightness");
            PlayerPrefs.Save();
            SessionState.SetBool(Session, false);
            string scene = SessionState.GetString(Session + ".Scene", "Assets/Scenes/MainMenu.unity");
            if (File.Exists(Path.Combine(Root, scene))) EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);
            return;
        }
        if (routine == null) routine = Run();
        try
        {
            if (!routine.MoveNext()) Finish(null);
        }
        catch (Exception error) { Finish(error); }
    }

    static IEnumerator Run()
    {
        checks.Clear();
        Application.runInBackground = true;
        Time.timeScale = 1f;
        EditorWindow gameView = EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));
        gameView.Focus();
        yield return null;
        yield return null;
        LazyBarArthur bar = UnityEngine.Object.FindFirstObjectByType<LazyBarArthur>();
        SistemHP phone = UnityEngine.Object.FindFirstObjectByType<SistemHP>();
        SistemPilihan dialog = UnityEngine.Object.FindFirstObjectByType<SistemPilihan>();
        DialogManager manager = UnityEngine.Object.FindFirstObjectByType<DialogManager>();
        Check(bar != null && phone != null && dialog != null && manager != null, "Komponen runtime tersedia");
        bar.enabled = false;
        foreach (AreaInteraksi area in UnityEngine.Object.FindObjectsByType<AreaInteraksi>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            area.enabled = false;
            foreach (Collider2D collider in area.GetComponents<Collider2D>()) collider.enabled = false;
        }

        Set(bar, "sisaWaktu", bar.batasWaktuMalas);
        Invoke(bar, "TerapkanEfekVisual");
        Check(Vector3.Distance(bar.lazyBarUtama.localScale, Get<Vector3>(bar, "skalaAwal")) < 0.0001f && Quaternion.Angle(bar.lazyBarUtama.localRotation, Get<Quaternion>(bar, "rotasiAwal")) < 0.001f, "Bar penuh tidak bergetar saat mulai");
        Set(bar, "sisaWaktu", bar.batasWaktuMalas * 0.1f);
        float end = Time.realtimeSinceStartup + 0.5f;
        float maxAngle = 0f;
        while (Time.realtimeSinceStartup < end)
        {
            Invoke(bar, "TerapkanEfekVisual");
            maxAngle = Mathf.Max(maxAngle, Quaternion.Angle(bar.lazyBarUtama.localRotation, Get<Quaternion>(bar, "rotasiAwal")));
            yield return null;
        }
        Check(maxAngle > 0f && maxAngle <= bar.intensitasGoyang * bar.pengaliGoyangKritis + 0.01f, "Getaran bar rendah dibatasi intensitasnya");
        if (SessionState.GetBool(Session + ".ShakeOnly", false))
        {
            Set(bar, "sisaWaktu", bar.batasWaktuMalas * 0.01f);
            end = Time.realtimeSinceStartup + 0.75f;
            float criticalAngle = 0f;
            float maxShift = 0f;
            while (Time.realtimeSinceStartup < end)
            {
                Invoke(bar, "TerapkanEfekVisual");
                criticalAngle = Mathf.Max(criticalAngle, Quaternion.Angle(bar.lazyBarUtama.localRotation, Get<Quaternion>(bar, "rotasiAwal")));
                maxShift = Mathf.Max(maxShift, Vector2.Distance(bar.lazyBarUtama.anchoredPosition, Get<Vector2>(bar, "posisiAwal")));
                yield return null;
            }
            Check(criticalAngle > maxAngle && criticalAngle > 3f, "Ayunan mendekati habis lebih brutal daripada peringatan awal");
            Check(criticalAngle <= bar.intensitasGoyang * bar.pengaliGoyangKritis + 0.01f && maxShift > 8f && maxShift <= bar.jarakGuncang * 1.15f, "Guncangan menarik perhatian tanpa offset yang menumpuk");
            phone.hpSedangBuka = true;
            float remaining = Get<float>(bar, "sisaWaktu");
            Invoke(bar, "Update");
            Check(Mathf.Approximately(Get<float>(bar, "sisaWaktu"), remaining) && Vector2.Distance(bar.lazyBarUtama.anchoredPosition, Get<Vector2>(bar, "posisiAwal")) < 0.001f, "Ponsel menjeda parameter dan menenangkan guncangan");
            phone.hpSedangBuka = false;
        }
        Set(bar, "sisaWaktu", bar.batasWaktuMalas);
        end = Time.realtimeSinceStartup + 1.5f;
        while (Time.realtimeSinceStartup < end) { Invoke(bar, "TerapkanEfekVisual"); yield return null; }
        Check(Vector3.Distance(bar.lazyBarUtama.localScale, Get<Vector3>(bar, "skalaAwal")) < 0.0001f && Quaternion.Angle(bar.lazyBarUtama.localRotation, Get<Quaternion>(bar, "rotasiAwal")) < 0.001f, "Bar kembali ke posisi normal setelah pulih");
        Check(Vector2.Distance(bar.lazyBarUtama.anchoredPosition, Get<Vector2>(bar, "posisiAwal")) < 0.001f, "Posisi bar pulih tanpa drift");
        if (SessionState.GetBool(Session + ".ShakeOnly", false)) yield break;

        Check(dialog.dialogManager == manager && DialogManager.Instance == manager, "DialogManager runtime konsisten");
        Set(dialog, "inputBerikutnya", 0f);
        Set(dialog, "sedangMenutup", false);
        Set(dialog, "sedangMainkanEmote", false);
        manager.bisaInteraksi = true;
        manager.skenarioAktif = new[] { new BarisDialog { teksDialog = "Satu dua terakhir." }, new BarisDialog { teksDialog = "Baris berikutnya." } };
        manager.statusTombolKeluar = false;
        manager.opsiInteraksiAktif = new[] { new OpsiInteraksi { teksOpsi = "Pertama" }, new OpsiInteraksi { teksOpsi = "Kedua" } };
        dialog.AksiTombolInteraksi();
        Check(manager.canvasDialog.activeInHierarchy && Get<int>(dialog, "indeksCerita") == 0,
            "Dialog dibuka sekali pada baris pertama (aktif=" + manager.canvasDialog.activeInHierarchy + ", bisa=" + manager.bisaInteraksi + ", HP=" + phone.hpSedangBuka + ", baris=" + Get<int>(dialog, "indeksCerita") + ")");
        for (int i = 0; i < 30; i++) dialog.AksiTombolInteraksi();
        Check(Get<int>(dialog, "indeksCerita") == 0, "Spam satu frame tidak melompati baris");
        end = Time.realtimeSinceStartup + 0.16f;
        while (Time.realtimeSinceStartup < end) yield return null;
        dialog.AksiTombolInteraksi();
        Check(Get<bool>(dialog, "sedangNgetik") && Get<int>(dialog, "indeksCerita") == 0 && dialog.teksCeritaUtama.text.StartsWith("Satu dua ") && dialog.teksCeritaUtama.text.Length < manager.skenarioAktif[0].teksDialog.Length, "Skip menyisakan kata terakhir untuk diketik");
        end = Time.realtimeSinceStartup + 5f;
        while (Get<bool>(dialog, "sedangNgetik") && Time.realtimeSinceStartup < end) yield return null;
        Check(!Get<bool>(dialog, "sedangNgetik"), "Kata terakhir selesai secara normal");
        dialog.AksiTombolInteraksi();
        Check(Get<int>(dialog, "indeksCerita") == 0, "Jeda setelah teks menahan tekanan langsung");
        end = Time.realtimeSinceStartup + 0.35f;
        while (Time.realtimeSinceStartup < end) yield return null;
        Tap(dialog.daftarPilihanUtama[0].GetComponent<UnityEngine.UI.Button>());
        Check(Get<int>(dialog, "indeksCerita") == 1, "Tekanan baru setelah jeda melanjutkan satu baris");
        end = Time.realtimeSinceStartup + 0.16f;
        while (Time.realtimeSinceStartup < end) yield return null;
        dialog.AksiTombolInteraksi();
        end = Time.realtimeSinceStartup + 5f;
        while (Get<bool>(dialog, "sedangNgetik") && Time.realtimeSinceStartup < end) yield return null;
        Check(!Get<bool>(dialog, "sedangNgetik") && !Get<bool>(dialog, "sedangBacaCerita") && !dialog.panelKeluarKhusus.gameObject.activeSelf,
            "Pilihan terakhir tampil tanpa tombol Keluar (mengetik=" + Get<bool>(dialog, "sedangNgetik") + ", baris=" + Get<int>(dialog, "indeksCerita") + ", waktu=" + Time.time + ")");
        end = Time.realtimeSinceStartup + 0.35f;
        while (Time.realtimeSinceStartup < end) yield return null;
        GameObject destination = new GameObject("TujuanPengujian");
        destination.transform.position = manager.arthurScript.transform.position + new Vector3(1f, 0f, 0f);
        manager.opsiInteraksiAktif[1].titikTujuan = destination.transform;
        Tap(dialog.daftarPilihanUtama[1].GetComponent<UnityEngine.UI.Button>());
        for (int i = 0; i < 20; i++) dialog.AksiTombolInteraksi();
        Check(Get<bool>(dialog, "sedangMenutup"), "Input dikunci selama menutup dialog");
        end = Time.realtimeSinceStartup + 4f;
        while (Time.realtimeSinceStartup < end) yield return null;
        Check(Mathf.Abs(manager.arthurScript.transform.position.x - destination.transform.position.x) < 0.05f, "Opsi terakhir menjalankan teleport tanpa salah dianggap Keluar");
        UnityEngine.Object.Destroy(destination);

        manager.statusTombolKeluar = true;
        manager.skenarioAktif = new[] { new BarisDialog { teksDialog = "Tes.", teksTombolLanjut = "Lanjut" }, new BarisDialog { teksDialog = "Berikutnya." } };
        Set(dialog, "inputBerikutnya", 0f);
        dialog.AksiTombolInteraksi();
        end = Time.realtimeSinceStartup + 4f;
        while (Get<bool>(dialog, "sedangNgetik") && Time.realtimeSinceStartup < end) yield return null;
        end = Time.realtimeSinceStartup + 0.35f;
        while (Time.realtimeSinceStartup < end) yield return null;
        Tap(dialog.panelKeluarKhusus.GetComponent<UnityEngine.UI.Button>());
        end = Time.realtimeSinceStartup + 0.45f;
        while (Time.realtimeSinceStartup < end) yield return null;
        Check(!manager.canvasDialog.activeInHierarchy, "Ketuk Keluar dialog menutup langsung tanpa navigasi kursor");

        Tap(phone.tombolIkonHP);
        end = Time.realtimeSinceStartup + 1f;
        while (Time.realtimeSinceStartup < end) yield return null;
        Tap(Icon(phone, "IkonSetting"));
        Check(phone.transisiAplikasi.SedangBerjalan, "Transisi TV mulai saat membuka pengaturan");
        for (int i = 0; i < 20; i++) phone.BukaPengaturan();
        end = Time.realtimeSinceStartup + 0.85f;
        while (Time.realtimeSinceStartup < end) yield return null;
        Check(phone.panelPengaturan.activeInHierarchy && !phone.transisiAplikasi.SedangBerjalan, "Transisi selesai dan pengaturan terbuka sekali");
        Capture("gameplay-settings.png");
        yield return null;
        yield return null;
        PengaturanGameplay gameplay = phone.panelPengaturan.GetComponent<PengaturanGameplay>();
        Tap(ButtonNamed(phone.panelPengaturan, "Btn_KeluarPermainan"));
        yield return null;
        yield return null;
        Check(gameplay.panelKonfirmasi.activeInHierarchy, "Keluar membuka konfirmasi");
        Capture("gameplay-exit-confirmation.png");
        yield return null;
        yield return null;
        Tap(ButtonNamed(gameplay.panelKonfirmasi, "Btn_Batal"));
        Check(!gameplay.panelKonfirmasi.activeSelf && phone.panelPengaturan.activeInHierarchy && manager.arthurScript.kunciPergerakan, "Batal tetap di gameplay dan Arthur terkunci saat ponsel terbuka");
        Tap(ButtonNamed(phone.panelPengaturan, "Btn_TutupX"));
        Check(phone.transisiAplikasi.SedangBerjalan, "X memulai TV mati");
        phone.BukaAplikasi("IkonMap");
        Check(!phone.panelSegeraHadir.activeSelf, "Klik berulang selama TV mati tidak membuka aplikasi lain");
        end = Time.realtimeSinceStartup + 0.65f;
        while (Time.realtimeSinceStartup < end) yield return null;
        Check(!phone.panelPengaturan.activeSelf && phone.hpSedangBuka, "X kembali ke ponsel");
        Check(Mathf.Approximately(phone.transisiAplikasi.durasiFadeKeluar, 0.3f), "Fade setelah TV mati berdurasi 0.3 detik");
        foreach (string name in new[] { "IkonMap", "IkonTas", "IkonToDoLIst", "IkonPesan" })
        {
            Tap(Icon(phone, name));
            end = Time.realtimeSinceStartup + 0.85f;
            while (Time.realtimeSinceStartup < end) yield return null;
            Check(phone.panelSegeraHadir.activeInHierarchy, name + " terbuka melalui sentuhan");
            Tap(ButtonNamed(phone.panelSegeraHadir, "Btn_KembaliPonsel"));
            Check(phone.transisiAplikasi.SedangBerjalan, name + " ditutup dengan TV mati");
            end = Time.realtimeSinceStartup + 0.65f;
            while (Time.realtimeSinceStartup < end) yield return null;
            Check(!phone.panelSegeraHadir.activeSelf && !phone.AplikasiSedangBuka, "Ponsel menerima sentuhan lagi setelah fade");
        }
        Tap(Icon(phone, "IkonKeluar"));
        Check(!phone.transisiAplikasi.SedangBerjalan, "IkonKeluar menutup ponsel tanpa TV mati");
        end = Time.realtimeSinceStartup + 0.65f;
        while (Time.realtimeSinceStartup < end) yield return null;
        Check(!phone.hpSedangBuka && !manager.arthurScript.kunciPergerakan, "Menutup ponsel mengembalikan kendali Arthur");
        Tap(phone.tombolIkonHP);
        end = Time.realtimeSinceStartup + 1f;
        while (Time.realtimeSinceStartup < end) yield return null;
        Tap(Icon(phone, "IkonSetting"));
        end = Time.realtimeSinceStartup + 0.85f;
        while (Time.realtimeSinceStartup < end) yield return null;
        float volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float brightness = PlayerPrefs.GetFloat("MasterBrightness", 0.5f);
        Tap(ButtonNamed(phone.panelPengaturan, "Btn_KeluarPermainan"));
        yield return null;
        Tap(ButtonNamed(gameplay.panelKonfirmasi, "Btn_Ya"));
        Check(phone.transisiAplikasi.SedangBerjalan, "Ya keluar memakai TV mati sebelum MainMenu");
        end = Time.realtimeSinceStartup + 5f;
        while (SceneManager.GetActiveScene().name != "MainMenu" && Time.realtimeSinceStartup < end) yield return null;
        Check(SceneManager.GetActiveScene().name == "MainMenu" && UnityEngine.Object.FindFirstObjectByType<LazyBarArthur>() == null, "Ya kembali ke MainMenu dan membuang sesi gameplay");
        Check(Mathf.Approximately(PlayerPrefs.GetFloat("MasterVolume", 1f), volume) && Mathf.Approximately(PlayerPrefs.GetFloat("MasterBrightness", 0.5f), brightness), "Volume dan kecerahan tetap tersimpan setelah keluar");
        end = Time.realtimeSinceStartup + 0.65f;
        while (Time.realtimeSinceStartup < end) yield return null;
        Check(UnityEngine.Object.FindFirstObjectByType<TransisiAplikasiTV>() == null, "Overlay transisi dibersihkan setelah fade di MainMenu");
    }

    static void Capture(string name)
    {
        ScreenCapture.CaptureScreenshot(Path.Combine(Root, "work/" + name));
    }
    static UnityEngine.UI.Button Icon(SistemHP phone, string name)
    {
        return phone.daftarIkon.Find(item => item != null && item.name == name).GetComponent<UnityEngine.UI.Button>();
    }
    static UnityEngine.UI.Button ButtonNamed(GameObject root, string name)
    {
        foreach (UnityEngine.UI.Button button in root.GetComponentsInChildren<UnityEngine.UI.Button>(true)) if (button.name == name) return button;
        throw new InvalidOperationException("Tombol tidak ditemukan: " + name);
    }
    static void Tap(UnityEngine.UI.Button button)
    {
        Canvas.ForceUpdateCanvases();
        Canvas canvas = button.GetComponentInParent<Canvas>().rootCanvas;
        RectTransform rect = button.GetComponent<RectTransform>();
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 position = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center));
        var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = position, button = UnityEngine.EventSystems.PointerEventData.InputButton.Left };
        var hits = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, hits);
        GameObject handler = hits.Count > 0 ? UnityEngine.EventSystems.ExecuteEvents.GetEventHandler<UnityEngine.EventSystems.IPointerClickHandler>(hits[0].gameObject) : null;
        Check(handler == button.gameObject, "Raycast sentuh mencapai " + button.name + " (teratas: " + (hits.Count > 0 ? hits[0].gameObject.name : "tidak ada") + ", posisi=" + position + ", canvas=" + canvas.renderMode + ")");
        UnityEngine.EventSystems.ExecuteEvents.Execute(handler, pointer, UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
    }
    static void Check(bool ok, string message)
    {
        if (!ok) throw new InvalidOperationException(message);
        checks.Add("OK: " + message);
    }
    static FieldInfo Field(object target, string name) => target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
    static T Get<T>(object target, string name) => (T)Field(target, name).GetValue(target);
    static void Set(object target, string name, object value) => Field(target, name).SetValue(target, value);
    static void Invoke(object target, string name) => target.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, null);
    static void RememberPref(string key)
    {
        SessionState.SetBool(Session + key + ".Exists", PlayerPrefs.HasKey(key));
        SessionState.SetFloat(Session + key, PlayerPrefs.GetFloat(key));
    }
    static void RestorePref(string key)
    {
        if (SessionState.GetBool(Session + key + ".Exists", false)) PlayerPrefs.SetFloat(key, SessionState.GetFloat(Session + key, 0f));
        else PlayerPrefs.DeleteKey(key);
    }
    static void Finish(Exception error)
    {
        if (error != null)
        {
            try
            {
                Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(Root, "work/touch-check-failure.png"), screenshot.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(screenshot);
            }
            catch (Exception) { }
        }
        File.WriteAllText(Path.Combine(Root, "work/gameplay-checks-state.txt"), error == null ? "COMPLETE" : "FAILED");
        File.WriteAllText(Path.Combine(Root, "work/gameplay-runtime-checks.txt"), (error == null ? "PASS\n" : "FAIL\n" + error + "\n") + string.Join("\n", checks));
        if (SessionState.GetBool(Session + ".ShakeOnly", false))
            File.WriteAllText(Path.Combine(Root, "work/arthur-shake-checks.txt"), (error == null ? "PASS\n" : "FAIL\n" + error + "\n") + string.Join("\n", checks));
        routine = null;
        EditorApplication.isPlaying = false;
    }
}
