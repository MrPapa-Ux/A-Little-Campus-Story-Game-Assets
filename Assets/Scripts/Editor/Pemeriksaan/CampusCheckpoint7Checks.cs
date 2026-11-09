// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Checkpoint 7; alat pengujian Editor, bukan komponen gameplay.
// Panduan: Assets/Scripts/Editor/Pemeriksaan/README.md

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public static class CampusCheckpoint7Checks
{
    const string Key = "Campus.Checkpoint7.Test";
    static IEnumerator routine;
    static readonly List<string> checks = new List<string>();
    static readonly List<string> errors = new List<string>();
    static string FilePath(string name) => Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",name);
    [InitializeOnLoadMethod] static void Register() { EditorApplication.update -= Tick; EditorApplication.update += Tick; }
    public static void Begin()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Mulai dari Edit Mode.");
        SessionState.SetBool(Key,true); EditorApplication.isPlaying = true;
    }
    static void Tick()
    {
        if (!SessionState.GetBool(Key,false) || EditorApplication.isCompiling || EditorApplication.isUpdating || !EditorApplication.isPlaying) return;
        if (routine == null) routine = Test();
        try { if (!routine.MoveNext()) Finish(null); } catch (Exception e) { Finish(e); }
    }
    static void Finish(Exception e)
    {
        Application.logMessageReceived -= CaptureError;
        if (e == null && errors.Count > 0) e = new InvalidOperationException(string.Join("\n", errors));
        File.WriteAllText(FilePath("checkpoint7-checks.txt"), (e == null ? "PASS\n" : "FAIL\n" + e + "\n") + string.Join("\n", checks));
        SessionState.SetBool(Key,false); routine = null; EditorApplication.isPlaying = false;
    }
    static void CaptureError(string message,string trace,LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message); }
    static void Check(bool ok,string note) { if (!ok) throw new InvalidOperationException(note); checks.Add("OK: " + note); }
    static GameObject TargetAt(RectTransform r)
    {
        Canvas.ForceUpdateCanvases();
        var e = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null,r.position), button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(e,hits);
        if (hits.Count == 0) return null;
        return ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
    }
    static void Click(RectTransform r)
    {
        GameObject target = TargetAt(r); if (target == null) throw new InvalidOperationException("Sentuhan terhalang: " + r.name);
        var e = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null,r.position), button = PointerEventData.InputButton.Left };
        ExecuteEvents.Execute(target,e,ExecuteEvents.pointerClickHandler);
    }
    static void Capture(string file) { ScreenCapture.CaptureScreenshot(FilePath(file)); }
    static IEnumerator Test()
    {
        checks.Clear(); errors.Clear(); Application.logMessageReceived += CaptureError; Application.runInBackground = true;
        EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();
        yield return null; yield return null;
        var hp = UnityEngine.Object.FindFirstObjectByType<SistemHP>(); var map = hp.petaKampus;
        var player = UnityEngine.Object.FindFirstObjectByType<KarakterController>(); var body = player.GetComponent<Rigidbody2D>();
        player.GetComponent<LazyBarArthur>().enabled = false; body.gravityScale = 0f; body.linearVelocity = Vector2.zero;
        foreach (Collider2D c in player.GetComponentsInChildren<Collider2D>()) c.enabled = false;
        var manager = UnityEngine.Object.FindFirstObjectByType<DialogManager>(); var dialog = manager.GetComponent<SistemPilihan>();
        Check(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1,"Scene memakai satu EventSystem");
        hp.BukaHPDefault(); float until = Time.realtimeSinceStartup + 1f; while (Time.realtimeSinceStartup < until) yield return null;
        var icon = hp.daftarIkon.First(g=>g.name=="IkonMap"); Click(icon.GetComponent<RectTransform>());
        until = Time.realtimeSinceStartup + 1.3f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(map.isActiveAndEnabled && hp.AplikasiSedangBuka && player.kunciPergerakan,"Ketukan ikon membuka peta lewat transisi TV dan mengunci Arthur");
        Check(map.lokasi.Length == 14 && map.isiPeta.GetComponent<UnityEngine.UI.Image>().sprite.texture.height >= 1448,"14 lokasi memakai peta final tanpa penurunan resolusi sumber");
        Capture("checkpoint7-map-overview.png"); yield return null; yield return null;
        int index = map.IndeksTerpilih; hp.TekanTombolUIKanan(); Check(map.IndeksTerpilih == (index + 1) % 14,"Tombol kanan ponsel memilih lokasi berikutnya");
        hp.TekanTombolUIKiri(); Check(map.IndeksTerpilih == index,"Tombol kiri ponsel kembali ke lokasi sebelumnya");
        hp.TekanTombolUIInteract(); until = Time.realtimeSinceStartup + .5f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(map.SedangFokus && Mathf.Abs(map.Zoom - map.zoomFokus) < .01f,"Interaksi melakukan zoom halus ke lokasi terpilih");
        map.LihatSemua(); until = Time.realtimeSinceStartup + .5f; while (Time.realtimeSinceStartup < until) yield return null;
        for (int i = 0; i < map.lokasi.Length; i++) Check(TargetAt(map.lokasi[i].lingkaran.rectTransform) == map.lokasi[i].lingkaran.gameObject,"Lingkaran dapat disentuh: " + map.lokasi[i].nama);
        Click(map.lokasi[8].lingkaran.rectTransform); until = Time.realtimeSinceStartup + .5f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(map.IndeksTerpilih == 8 && map.SedangFokus,"Sentuhan langsung memilih GA dan memusatkan zoom");
        Capture("checkpoint7-map-zoom.png"); yield return null; yield return null;
        map.Geser(new Vector2(90000,-90000)); Vector2 max = (map.isiPeta.rect.size * map.Zoom - map.viewport.rect.size) * .5f;
        Check(Mathf.Abs(map.isiPeta.anchoredPosition.x) <= max.x + .01f && Mathf.Abs(map.isiPeta.anchoredPosition.y) <= max.y + .01f,"Geser ekstrem dibatasi agar tepi peta tidak keluar bingkai");
        map.AturZoomLangsung(100f); Check(Mathf.Abs(map.Zoom - 4f) < .001f,"Zoom cubit dibatasi maksimum 4x");
        Click((RectTransform)map.transform.Find("BingkaiHP/Kertas/Ikhtisar")); until = Time.realtimeSinceStartup + .5f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(Mathf.Abs(map.Zoom - 1f) < .001f && map.isiPeta.anchoredPosition.sqrMagnitude < .001f,"Tombol Semua memulihkan peta utuh");
        Click((RectTransform)map.transform.Find("BingkaiHP/KembaliKeHP")); until = Time.realtimeSinceStartup + 1f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(!map.gameObject.activeInHierarchy && hp.hpSedangBuka && player.kunciPergerakan,"Tutup aplikasi kembali ke ponsel; kontrol dunia tetap terkunci");
        hp.TutupHP(); until = Time.realtimeSinceStartup + .5f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(!player.kunciPergerakan,"Menutup ponsel memulihkan kontrol Arthur");
        var arthurFace = player.GetComponentInChildren<EkspresiWajah2D>();
        var nara = GameObject.Find("NPC_Nara_Ekspresi"); var naraFace = nara.GetComponentInChildren<EkspresiWajah2D>();
        foreach (EmosiKarakter emosi in Enum.GetValues(typeof(EmosiKarakter)))
        {
            arthurFace.gunakanEkspresi = true; arthurFace.ekspresi = emosi; naraFace.ekspresi = emosi;
            yield return null; yield return null;
            Check(arthurFace.EkspresiAktif == emosi && naraFace.EkspresiAktif == emosi,"Inspector mengatur Arthur dan NPC: " + emosi);
        }
        arthurFace.UcapkanHuruf(); arthurFace.PaksaKedip(); yield return null;
        Check(arthurFace.MulutTerbuka && arthurFace.SedangKedip,"Mulut bicara dan kedip tetap berfungsi bersama ekspresi");
        until = Time.realtimeSinceStartup + .25f; while (Time.realtimeSinceStartup < until) yield return null;
        arthurFace.gunakanEkspresi = false; naraFace.ekspresi = EmosiKarakter.Senyum; yield return null;
        var bimo = GameObject.Find("NPC_Bimo_Joget"); var dancer = bimo.GetComponent<NPCJogetGoofy>();
        Vector3 bubblePos = bimo.GetComponent<NPCBubbleTrigger>().bubbleChatTransform.localPosition;
        Quaternion hand = dancer.tanganKiri.localRotation; until = Time.realtimeSinceStartup + .19f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(Quaternion.Angle(hand,dancer.tanganKiri.localRotation) > 1f,"Joget mengayunkan bagian tubuh");
        Check(bimo.GetComponent<NPCBubbleTrigger>().bubbleChatTransform.localPosition == bubblePos,"Bubble tetap terpisah dari gerakan joget");
        manager.bisaInteraksi = true; manager.idInteraksiAktif = nara.name; manager.skenarioAktif = nara.GetComponent<AreaInteraksi>().skenarioDialog; manager.opsiInteraksiAktif = null; manager.statusTombolKeluar = true;
        dialog.AksiTombolInteraksi(); yield return null; yield return null;
        Check(naraFace.EkspresiAktif == EmosiKarakter.Sedih,"Baris dialog Nara menerapkan ekspresi sedih pada pembicara");
        manager.TutupDialog(); yield return null; yield return null;
        Check(naraFace.EkspresiAktif == EmosiKarakter.Senyum,"Ekspresi kembali ke pilihan Inspector ketika dialog ditutup");
        until = Time.realtimeSinceStartup + .3f; while (Time.realtimeSinceStartup < until) yield return null;
        manager.bisaInteraksi = true; manager.idInteraksiAktif = bimo.name; manager.skenarioAktif = bimo.GetComponent<AreaInteraksi>().skenarioDialog;
        dialog.AksiTombolInteraksi(); until = Time.realtimeSinceStartup + 1f; while (Time.realtimeSinceStartup < until) yield return null;
        var shout = dialog.teksCeritaUtama.GetComponent<DialogTeriakGoofy>();
        Check(shout.SedangTeriak && shout.JumlahDentuman > 0,"Dialog Bimo memakai teks tebal, pantulan dan audio dentuman");
        Capture("checkpoint7-dialog-teriak.png"); yield return null; yield return null;
        manager.TutupDialog(); yield return null;
        Check(!shout.SedangTeriak && !player.kunciPergerakan,"Efek teriak berhenti dan gerakan dibuka setelah dialog");
        // Kontrol lama tetap diuji melalui API tombol yang sama dengan sentuhan Android.
        player.JalanKeKanan(); Check(!player.SedangBerlari,"Tekanan pertama tetap berjalan"); player.BerhentiJalan();
        player.JalanKeKanan(); Check(player.SedangBerlari,"Ketuk ganda lalu tahan tetap berlari"); player.BerhentiBergerak();
        foreach (Collider2D c in player.GetComponentsInChildren<Collider2D>()) c.enabled = true;
        body.position = new Vector2(nara.transform.position.x - .7f,0f); Physics2D.SyncTransforms();
        until = Time.realtimeSinceStartup + .3f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(manager.bisaInteraksi && manager.idInteraksiAktif == nara.name,"Memasuki area fisik Nara mengaktifkan dialog NPC yang tepat");
        body.position = new Vector2(0f,0f); Physics2D.SyncTransforms();
        until = Time.realtimeSinceStartup + .3f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(!manager.bisaInteraksi,"Keluar area Nara menghapus akses interaksi");
        body.position = new Vector2(bimo.transform.position.x - .7f,0f); Physics2D.SyncTransforms();
        until = Time.realtimeSinceStartup + .3f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(manager.bisaInteraksi && manager.idInteraksiAktif == bimo.name,"Memasuki area fisik Bimo mengaktifkan dialog joget");
        bimo.SetActive(false); yield return null; yield return null;
        Check(!dancer.gameObject.activeInHierarchy,"Mematikan NPC mengakhiri joget dengan aman"); bimo.SetActive(true);
        Capture("checkpoint7-npc-world.png"); yield return null; yield return null;
        CampusCheckpoint7Preview.Prepare();
        until = Time.realtimeSinceStartup + .3f; while (Time.realtimeSinceStartup < until) yield return null;
        CampusCheckpoint7Preview.Capture(); yield return null; yield return null;
    }
}
