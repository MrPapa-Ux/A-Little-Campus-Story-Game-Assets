// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Checkpoint 8; alat pengujian Editor, bukan komponen gameplay.
// Panduan: Assets/Scripts/Editor/Pemeriksaan/README.md

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class CampusCheckpoint8Checks
{
    const string Key = "Campus.Checkpoint8.Checks";
    static IEnumerator routine;
    static readonly List<string> checks = new List<string>(), errors = new List<string>();
    static string PathTo(string file) => Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",file);
    [InitializeOnLoadMethod] static void Register() { EditorApplication.update -= Tick; EditorApplication.update += Tick; }
    public static void Begin()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Mulai pemeriksaan dari Edit Mode.");
        EditorSceneManager.SaveOpenScenes(); EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        File.WriteAllText(PathTo("checkpoint8-checks.txt"),"RUNNING\n");
        SessionState.SetString(Key,"GAME"); EditorApplication.isPlaying = true;
    }
    static void Tick()
    {
        string mode = SessionState.GetString(Key,"");
        if (mode == "" || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode != EditorApplication.isPlaying) return;
        if (!EditorApplication.isPlaying)
        {
            if (mode == "TO_MENU") { EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity"); SessionState.SetString(Key,"MENU"); EditorApplication.isPlaying = true; }
            else if (mode == "RESTORE") { EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity"); SessionState.SetString(Key,""); }
            return;
        }
        if (routine == null) { checks.Clear(); errors.Clear(); Application.logMessageReceived += LogError; routine = mode == "GAME" ? GameTest() : MenuTest(); }
        try { if (!routine.MoveNext()) Finish(null,mode); } catch (Exception e) { Finish(e,mode); }
    }
    static void LogError(string message,string trace,LogType type) { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) errors.Add(message); }
    static void Finish(Exception error,string mode)
    {
        Application.logMessageReceived -= LogError;
        if (error == null && errors.Count > 0) error = new InvalidOperationException(string.Join("\n",errors));
        string report = string.Join("\n",checks) + "\n";
        if (error != null) { File.AppendAllText(PathTo("checkpoint8-checks.txt"),"FAIL\n" + error + "\n" + report); SessionState.SetString(Key,"RESTORE"); }
        else if (mode == "GAME") { File.WriteAllText(PathTo("checkpoint8-checks.txt"),"GAME PASS\n" + report); SessionState.SetString(Key,"TO_MENU"); }
        else { string old = File.ReadAllText(PathTo("checkpoint8-checks.txt")); File.WriteAllText(PathTo("checkpoint8-checks.txt"),"PASS\n" + old + report); SessionState.SetString(Key,"RESTORE"); }
        routine = null; EditorApplication.isPaused = false; EditorApplication.isPlaying = false;
    }
    static void Check(bool ok,string note) { if (!ok) throw new InvalidOperationException(note); checks.Add("OK: " + note); }
    static GameObject Hit(RectTransform r)
    {
        Canvas.ForceUpdateCanvases(); var e = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null,r.position), button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(e,hits);
        return hits.Count == 0 ? null : ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
    }
    static void Click(RectTransform r) { var target = Hit(r); if (target == null) throw new InvalidOperationException("Ketukan terhalang: " + r.name); ExecuteEvents.Execute(target,new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left },ExecuteEvents.pointerClickHandler); }
    static void Capture(string file) { ScreenCapture.CaptureScreenshot(PathTo(file)); }
    static bool SameRGB(Color a,Color b) => Mathf.Abs(a.r-b.r) + Mathf.Abs(a.g-b.g) + Mathf.Abs(a.b-b.b) < .01f;
    static IEnumerator GameTest()
    {
        Application.runInBackground = true; EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus(); yield return null; yield return null;
        var player = UnityEngine.Object.FindFirstObjectByType<KarakterController>(); player.GetComponent<LazyBarArthur>().enabled = false;
        var rb = player.GetComponent<Rigidbody2D>(); rb.gravityScale = 0f; rb.linearVelocity = Vector2.zero;
        foreach (Collider2D c in player.GetComponentsInChildren<Collider2D>()) c.enabled = false;
        var hp = UnityEngine.Object.FindFirstObjectByType<SistemHP>(); var pose = hp.poseKarakter; var map = hp.petaKampus;
        Check(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1,"Satu EventSystem pada gameplay");
        pose.poseBerdiri.SampleAnimation(player.wadahVisual.gameObject,0f); Quaternion head = pose.kepala.localRotation;
        hp.BukaHPDefault(); float until = Time.realtimeSinceStartup + 1f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(pose.SedangMemegang && pose.hpKecil.gameObject.activeInHierarchy && pose.KekuatanPose > .99f,"Membuka HP memunculkan pose tangan dan HP kecil");
        Check(pose.hpKecil.sprite == hp.badanHP.sprite && SameRGB(pose.hpKecil.color,hp.badanHP.color),"HP dalam genggaman memakai aset dan warna ponsel yang sama");
        Check(Quaternion.Angle(head,pose.kepala.localRotation) > 4f && pose.hpKecil.bounds.size.y < .45f,"Arthur menunduk sedikit dan ukuran HP tetap kecil");
        Capture("checkpoint8-phone.png"); yield return null; yield return null;
        Click(hp.daftarIkon.First(g=>g.name=="IkonMap").GetComponent<RectTransform>());
        until = Time.realtimeSinceStartup + 1f; while (Time.realtimeSinceStartup < until) yield return null;
        var frame = (RectTransform)map.transform.Find("BingkaiHP"); var sheet = frame.Find("Kertas"); var x = (RectTransform)sheet.Find("KembaliKeHP");
        Check(frame.rect.width * frame.localScale.x < ((RectTransform)map.transform).rect.width * .81f,"Peta berupa panel mengambang dengan ruang di luar bingkai");
        Check(x.anchorMin == Vector2.one && x.pivot == Vector2.one && x.anchoredPosition.x < 0 && x.anchoredPosition.y < 0,"X terikat pada sudut dalam kertas dengan margin rapi");
        foreach (string name in new[] { "JudulApp", "LokasiTerpilih", "Interaksi/Tulisan", "Ikhtisar/Tulisan", "KembaliKeHP/Tulisan" })
            Check(sheet.Find(name).GetComponent<WigglyText>()?.isActiveAndEnabled == true,"Wiggly aktif: " + name);
        var title = sheet.Find("JudulApp").GetComponent<TMP_Text>(); title.ForceMeshUpdate(); Vector3[] before = (Vector3[])title.textInfo.meshInfo[0].vertices.Clone();
        until = Time.realtimeSinceStartup + .2f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(title.textInfo.meshInfo[0].vertices.Where((v,i)=>i<before.Length && Vector3.Distance(v,before[i])>.05f).Any(),"Huruf kapital benar-benar berubah bentuk di runtime");
        for (int i=0;i<map.lokasi.Length;i++) Check(Hit(map.lokasi[i].lingkaran.rectTransform)==map.lokasi[i].lingkaran.gameObject,"Lingkaran tetap dapat disentuh: " + map.lokasi[i].nama);
        Capture("checkpoint8-map.png"); yield return null; yield return null;
        hp.TekanTombolUIKanan(); Check(map.IndeksTerpilih==1,"Navigasi kanan masih memilih lokasi"); hp.TekanTombolUIInteract();
        until = Time.realtimeSinceStartup + .5f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(map.Zoom > 2f && map.labelInteraksi.text=="KEMBALI","Zoom dan label tombol kembali tetap bekerja");
        Click(x); until = Time.realtimeSinceStartup + 1f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(!map.gameObject.activeInHierarchy && hp.hpSedangBuka && pose.SedangMemegang,"X kembali ke HP tanpa mengakhiri pose memegang");
        hp.BukaPengaturan(); until = Time.realtimeSinceStartup + 1f; while (Time.realtimeSinceStartup < until) yield return null;
        var settings = hp.panelPengaturan.GetComponent<PengaturanManager>(); var box = (RectTransform)settings.transform.Find("Box_Pengaturan");
        Check(box.localScale.x < .85f && box.GetComponent<UnityEngine.UI.Image>().color.a == 1f,"Pengaturan ponsel memakai panel kertas mengambang");
        Check(settings.sliderVolume != null && settings.sliderCahaya != null && settings.panelHitamFilter.blocksRaycasts == false,"Slider dan filter kecerahan tetap terhubung");
        Capture("checkpoint8-settings.png"); yield return null; yield return null;
        Click((RectTransform)box.Find("Btn_TutupX")); until = Time.realtimeSinceStartup + 1f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(!hp.panelPengaturan.activeInHierarchy,"X pengaturan tetap kembali ke ponsel");
        hp.BukaAplikasi("IkonTas"); until = Time.realtimeSinceStartup + 1f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(hp.panelSegeraHadir.activeInHierarchy && hp.panelSegeraHadir.transform.Find("KertasAplikasi").localScale.x < .85f,"Aplikasi lain memakai bingkai mengambang juga");
        Click((RectTransform)hp.panelSegeraHadir.transform.Find("KertasAplikasi/Btn_KembaliPonsel")); until = Time.realtimeSinceStartup + 1f; while (Time.realtimeSinceStartup < until) yield return null;
        hp.TutupHP(); until = Time.realtimeSinceStartup + .6f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(!pose.SedangMemegang && !pose.hpKecil.gameObject.activeInHierarchy && !player.kunciPergerakan,"Menutup HP mengembalikan pose dan membuka kontrol jalan");
        Color blue = new Color(.25f,.6f,1f); hp.BukaHPDenganWarna(blue); until = Time.realtimeSinceStartup + 1f; while (Time.realtimeSinceStartup < until) yield return null;
        Check(SameRGB(pose.hpKecil.color,blue),"Profil warna berbeda ikut diterapkan pada HP kecil");
        hp.TutupHP(); until = Time.realtimeSinceStartup + .6f; while (Time.realtimeSinceStartup < until) yield return null;
        var arthurFace = player.GetComponentInChildren<EkspresiWajah2D>(); var nara = GameObject.Find("NPC_Nara_Ekspresi").GetComponentInChildren<EkspresiWajah2D>();
        foreach (var face in new[] { arthurFace,nara })
        {
            face.AturEkspresiDialog(EmosiKarakter.Sedih); yield return null;
            Check(face.transform.Find("Ekspresi_Sedih/MataEkspresi/TutupKiri") == null && face.transform.Find("Ekspresi_Sedih/MataEkspresi/AlisKiri") != null,"Sedih mempertahankan sprite mata asli: " + face.transform.parent.parent.name);
            face.PaksaKedip(); yield return null;
            Check(face.SedangKedip && face.transform.Find("Ekspresi_Sedih/MataEkspresi").gameObject.activeSelf,"Alis sedih tetap konsisten ketika karakter berkedip");
            face.KembalikanEkspresi();
        }
        var dancer = GameObject.Find("NPC_Bimo_Joget").GetComponent<NPCJogetGoofy>();
        float leftGround = dancer.kakiKiri.GetComponent<SpriteRenderer>().bounds.min.y, rightGround = dancer.kakiKanan.GetComponent<SpriteRenderer>().bounds.min.y;
        Quaternion hand = dancer.tanganKiri.localRotation; bool armsMoved = false; until = Time.realtimeSinceStartup + 1.3f;
        while (Time.realtimeSinceStartup < until)
        {
            Check(Mathf.Abs(dancer.kakiKiri.GetComponent<SpriteRenderer>().bounds.min.y-leftGround)<.002f && Mathf.Abs(dancer.kakiKanan.GetComponent<SpriteRenderer>().bounds.min.y-rightGround)<.002f,"FRAME: Telapak joget tetap menapak");
            if (Quaternion.Angle(hand,dancer.tanganKiri.localRotation)>10f) armsMoved = true; yield return null;
        }
        checks.RemoveAll(s=>s.Contains("FRAME:")); Check(armsMoved,"Tangan, badan dan kepala berdansa sementara telapak tetap menapak");
        player.JalanKeKanan(); Check(!player.SedangBerlari,"Tekan pertama tetap jalan"); player.BerhentiJalan(); player.JalanKeKanan(); Check(player.SedangBerlari,"Ketuk ganda tetap berlari"); player.BerhentiBergerak();
        EditorApplication.isPaused = true; TrafficTest(); EditorApplication.isPaused = false;
        CampusCheckpoint7Preview.Prepare(); until = Time.realtimeSinceStartup + .3f; while (Time.realtimeSinceStartup < until) yield return null;
        CampusCheckpoint7Preview.Capture(); File.Copy(PathTo("checkpoint7-ekspresi.png"),PathTo("checkpoint8-ekspresi.png"),true); yield return null;
    }
    static Bounds BodyBounds(MobilKampus car)
    {
        Renderer[] rs = car.GetComponentsInChildren<Renderer>().Where(r=>(r is MeshRenderer || r is SkinnedMeshRenderer) && r.enabled).ToArray();
        Bounds b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b;
    }
    static void TrafficTest()
    {
        var cars = UnityEngine.Object.FindObjectsByType<MobilKampus>(FindObjectsSortMode.None).OrderBy(c=>c.name).ToArray();
        Check(cars.Length==3 && cars.All(c=>Mathf.Abs(c.peluangKlakson-.03f)<.00001f),"Ketiga mobil memakai peluang klakson tepat 3%");
        var rng = UnityEngine.Random.state; UnityEngine.Random.InitState(8128);
        FieldInfo origin = typeof(MobilKampus).GetField("posisiAwal",BindingFlags.NonPublic|BindingFlags.Instance);
        foreach (var c in cars) c.gameObject.SetActive(false);
        for(int i=0;i<cars.Length;i++)
        {
            var c=cars[i]; origin.SetValue(c,new Vector3(100f,c.PosisiAwal.y,0f)); c.kecepatanMin=c.kecepatanMax=i==0?3f:18f+i*6f;
            c.durasiJalan=5f; c.jedaWaktuMin=.1f; c.jedaWaktuMax=1.3f; c.jedaKemunculanAwal=Vector2.zero; c.peluangKlakson=0f; c.gameObject.SetActive(true);
        }
        bool slowed=false; int appearances=0; var last=new bool[3]; float minGap=float.PositiveInfinity;
        for(int step=0;step<1200;step++)
        {
            MobilKampus.SimulasikanLangkah(.05f);
            for(int i=0;i<cars.Length;i++)
            {
                var c=cars[i]; if(c.SedangJalan && !last[i]) appearances++; last[i]=c.SedangJalan;
                if(!c.SedangJalan) continue;
                var front=c.MobilDiDepan(out float gap); if(front==null) continue;
                Bounds rearBounds=BodyBounds(c), frontBounds=BodyBounds(front); float visualGap=rearBounds.min.x-frontBounds.max.x;
                if(visualGap < 2.19f) throw new InvalidOperationException("Badan mobil terlalu dekat: "+c.name+" gap="+visualGap);
                minGap=Mathf.Min(minGap,visualGap);
                if(c.KecepatanSaatIni < c.kecepatanMin-1f) slowed=true;
            }
        }
        Check(slowed && minGap>=2.19f,"60 detik simulasi: mobil belakang melambat dan badan mobil tidak saling menembus");
        Check(appearances>6,"Mobil tetap muncul kembali berulang dengan jeda acak dan antrean masuk aman");
        for(int chance=0;chance<=1;chance++)
        {
            foreach(var c in cars)c.gameObject.SetActive(false);
            var lead=cars[0]; var rear=cars[1]; int horns=rear.JumlahKlakson, trials=rear.PercobaanKlakson;
            origin.SetValue(lead,new Vector3(100,lead.PosisiAwal.y,0)); origin.SetValue(rear,new Vector3(114,rear.PosisiAwal.y,0));
            lead.kecepatanMin=lead.kecepatanMax=6f; rear.kecepatanMin=rear.kecepatanMax=24f; lead.durasiJalan=rear.durasiJalan=50f; rear.peluangKlakson=chance;
            lead.gameObject.SetActive(true); rear.gameObject.SetActive(true);
            for(int i=0;i<14;i++)MobilKampus.SimulasikanLangkah(.05f);
            Check(rear.PercobaanKlakson-trials==1 && rear.JumlahKlakson-horns==chance,"Satu undian per episode tertahan; cabang peluang "+chance);
            if(chance==1)Check(lead.SisaNgebut>0f && lead.KecepatanSaatIni>6.5f,"Klakson mempercepat mobil depan ketika jalurnya aman");
        }
        foreach(var c in cars)c.gameObject.SetActive(false); UnityEngine.Random.state=rng;
    }
    static IEnumerator MenuTest()
    {
        Application.runInBackground=true; EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus(); yield return null; yield return null;
        var menu=UnityEngine.Object.FindFirstObjectByType<TitleMenuManager>(); menu.KlikTombolPengaturan(); yield return null;
        var box=(RectTransform)menu.pengaturanPanel.transform.Find("Box_Pengaturan");
        Check(box.localScale.x<.85f && box.GetComponent<UnityEngine.UI.Image>().color.a==1f,"Pengaturan Main Menu juga memakai panel mengambang");
        Capture("checkpoint8-mainmenu-settings.png"); yield return null; yield return null;
        Click((RectTransform)box.Find("Btn_TutupX")); yield return null;
        Check(!menu.pengaturanPanel.activeInHierarchy,"X pengaturan Main Menu tetap menutup panel");
    }
}
