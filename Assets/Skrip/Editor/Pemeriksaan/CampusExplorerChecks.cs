using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Uji terarah, hanya dimulai setelah pengguna menyetujui Play otomatis.
[InitializeOnLoad]
public static class CampusExplorerChecks
{
    const string Folder = "work/explorer/";
    const string Key = "Campus.Explorer.Checks";
    [Serializable] class Susunan { public SceneSetup[] scenes; }
    static CampusExplorerChecks() { EditorApplication.update += Tick; }
    static void Log(string s) { File.AppendAllText(Folder + "pengujian.txt", s + "\n"); }
    static void Check(bool ok, string pesan) { if (!ok) throw new Exception(pesan); Log("PASS: " + pesan); }
    static void Stage(int stage) { SessionState.SetInt(Key, stage); SessionState.SetFloat(Key + ".deadline", (float)EditorApplication.timeSinceStartup + 75f); }
    static void Tick()
    {
        if (AssetDatabase.IsAssetImportWorkerProcess()) return;
        int stage = SessionState.GetInt(Key, 0);
        try
        {
            if (stage == 0)
            {
                if (!File.Exists(Folder + "uji.txt") || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
                File.Delete(Folder + "uji.txt");
                if (EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Play sudah aktif; pengujian tidak dimulai.");
                foreach (var s in EditorSceneManager.GetSceneManagerSetup())
                    if (s.isLoaded && SceneManager.GetSceneByPath(s.path).isDirty) throw new Exception("Ada scene belum disimpan; pengujian dibatalkan.");
                File.WriteAllText(Folder + "pengujian.txt", "Pengujian Explorer\n");
                SessionState.SetBool(Key + ".loading", false);
                SessionState.SetString(Key + ".scenes", JsonUtility.ToJson(new Susunan { scenes = EditorSceneManager.GetSceneManagerSetup() }));
                SessionState.SetFloat(Key + ".volume", AudioListener.volume);
                SessionState.SetBool(Key + ".mute", EditorUtility.audioMasterMute);
                SessionState.SetBool(Key + ".background", Application.runInBackground);
                EditorUtility.audioMasterMute = true;
                EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
                Stage(1);
                EditorApplication.isPlaying = true;
                return;
            }
            if (stage == 9)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                var json = SessionState.GetString(Key + ".scenes", "");
                if (!string.IsNullOrEmpty(json)) EditorSceneManager.RestoreSceneManagerSetup(JsonUtility.FromJson<Susunan>(json).scenes);
                AudioListener.volume = SessionState.GetFloat(Key + ".volume", .5f);
                EditorUtility.audioMasterMute = SessionState.GetBool(Key + ".mute", false);
                Application.runInBackground = SessionState.GetBool(Key + ".background", false);
                JedaSaatTidakAktif.PengujianLatarBelakang = false;
                SessionState.SetInt(Key, 0);
                Log("SELESAI: Play mati; susunan scene sebelumnya dipulihkan.");
                return;
            }
            if (EditorApplication.timeSinceStartup > SessionState.GetFloat(Key + ".deadline", 0f)) throw new Exception("Batas tunggu pengujian terlampaui.");
            if (!EditorApplication.isPlaying) return;
            JedaSaatTidakAktif.PengujianLatarBelakang = true;
            Application.runInBackground = true;
            EditorApplication.isPaused = false;
            var fokus = UnityEngine.Object.FindFirstObjectByType<JedaSaatTidakAktif>();
            if (fokus != null) fokus.AturJeda(false);
            if (stage == 1)
            {
                var menu = UnityEngine.Object.FindFirstObjectByType<TitleMenuManager>();
                if (menu == null || Time.timeSinceLevelLoad < .5f) return;
                Check(menu.eksplorasiButton != null && menu.eksplorasiButton.IsInteractable(), "Tombol Eksplorasi aktif");
                Check(menu.namaSceneExplorer == "Explorer", "Tujuan menu adalah Explorer");
                Check(menu.warningPopup != null && !menu.warningPopup.activeSelf, "Popup Mulai tidak muncul sendiri");
                Stage(2);
                menu.eksplorasiButton.onClick.Invoke();
                Check(LoadingManager.sceneTujuan == "Explorer", "Loading diarahkan ke Explorer");
            }
            else if (stage == 2)
            {
                if (SceneManager.GetActiveScene().name == "LoadingScene" && !SessionState.GetBool(Key + ".loading", false))
                {
                    Check(true, "Eksplorasi melewati LoadingScene");
                    SessionState.SetBool(Key + ".loading", true);
                }
                if (SceneManager.GetActiveScene().name != "Explorer" || Time.timeSinceLevelLoad < 1f) return;
                Check(SessionState.GetBool(Key + ".loading", false), "Loading tampil sebelum Explorer");
                var bar = UnityEngine.Object.FindFirstObjectByType<LazyBarArthur>();
                Check(bar != null && bar.tanpaBatasWaktu, "Klik Eksplorasi memuat mode bebas waktu");
                Check(UnityEngine.Object.FindFirstObjectByType<KarakterController>() != null && UnityEngine.Object.FindFirstObjectByType<SistemHP>() != null && DialogManager.Instance != null, "Arthur, HP, dan dialog tersedia");
                PeriksaLayout();
                SessionState.SetFloat(Key + ".time", Time.time);
                SessionState.SetFloat(Key + ".duration", Mathf.Max(18f, bar.batasWaktuMalas + 2f));
                Stage(3);
            }
            else if (stage == 3)
            {
                float elapsed = Time.time - SessionState.GetFloat(Key + ".time", 0f);
                if (elapsed < SessionState.GetFloat(Key + ".duration", 18f)) return;
                var bar = UnityEngine.Object.FindFirstObjectByType<LazyBarArthur>();
                Check(SceneManager.GetActiveScene().name == "Explorer", "Tetap di Explorer setelah melewati batas waktu normal");
                Check(bar != null && !bar.waktuHabis && bar.lazyBarFill != null && Mathf.Approximately(bar.lazyBarFill.fillAmount, 1f), "Bar tetap penuh; gagal karena waktu tidak terpicu");
                PeriksaLayout();
                Log("Waktu simulasi teruji: " + elapsed.ToString("F2") + " detik.");
                ScreenCapture.CaptureScreenshot(Folder + "explorer.png");
                Stage(4);
            }
            else if (stage == 4)
            {
                if (!File.Exists(Folder + "explorer.png")) return;
                Log("PASS: Tampilan Game View tersimpan.");
                Stage(9);
                EditorApplication.isPlaying = false;
            }
        }
        catch (Exception e)
        {
            Log("GAGAL: " + e);
            if (stage > 0) { Stage(9); EditorApplication.isPlaying = false; }
        }
    }

    static void PeriksaLayout()
    {
        var rects = UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var kiri = rects.First(x => x.name == "Btn_Kiri");
        var kanan = rects.First(x => x.name == "Btn_Kanan");
        var hp = rects.First(x => x.name == "Btn_HandPhone");
        Check(Mathf.Abs(kiri.anchoredPosition.x - 106f) < 5f && Mathf.Abs(kanan.anchoredPosition.x - 381f) < 5f, "Kedua tombol arah tetap di posisi desain dan tidak bertumpuk");
        Check(Mathf.Abs(hp.anchoredPosition.x + 74f) < 5f && Mathf.Abs(hp.anchoredPosition.y + 65f) < 5f, "Ikon HP tetap pada posisi desain");
        foreach (var r in new[] { kiri, kanan, hp })
        {
            var corners = new Vector3[4]; r.GetWorldCorners(corners);
            var canvas = r.GetComponentInParent<Canvas>();
            var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            foreach (var corner in corners)
            {
                var p = RectTransformUtility.WorldToScreenPoint(cam, corner);
                Check(p.x >= -3f && p.x <= Screen.width + 3f && p.y >= -3f && p.y <= Screen.height + 3f, r.name + " berada dalam layar");
            }
        }
    }
}
