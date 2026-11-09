// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Phone Apps; alat pengujian Editor, bukan komponen gameplay.
// Panduan: Assets/Scripts/Editor/Pemeriksaan/README.md

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class CampusPhoneAppsChecks
{
    const string Key="Campus.PhoneApps.Checks";
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    static bool started;
    static string Work(string name)=>Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",name);
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    public static void Begin()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Mulai dari Edit Mode.");
        EditorSceneManager.SaveOpenScenes();EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        File.WriteAllText(Work("phone-apps-checks.txt"),"RUNNING\n");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false)||EditorApplication.isCompiling||EditorApplication.isUpdating||!EditorApplication.isPlaying)return;
        if(!started){started=true;results.Clear();errors.Clear();Application.logMessageReceived+=Log;stack.Clear();stack.Push(Run());}
        try{while(stack.Count>0){var next=stack.Peek();if(!next.MoveNext()){stack.Pop();continue;}if(next.Current is IEnumerator child){stack.Push(child);continue;}return;}Finish(null);}catch(Exception e){Finish(e);}
    }
    static void Log(string s,string trace,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(s);}
    static void Finish(Exception e)
    {
        Application.logMessageReceived-=Log;if(e==null&&errors.Count>0)e=new Exception(string.Join("\n",errors));
        File.WriteAllText(Work("phone-apps-checks.txt"),(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));started=false;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;
    }
    static void Check(bool ok,string note){if(!ok)throw new Exception(note);results.Add("OK: "+note);}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}

    static void Click(UnityEngine.UI.Button b)
    {
        Canvas.ForceUpdateCanvases();var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,b.transform.position),button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
        Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==b.gameObject,"Tombol tidak terhalang: "+b.name);ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerClickHandler);
    }
    static void Frame(SistemHP h,string app)
    {
        var common=h.ponselBersama;var c=new Vector3[4];((RectTransform)common.transform).GetWorldCorners(c);
        Check(c.All(p=>p.x>=Screen.safeArea.xMin-1&&p.x<=Screen.safeArea.xMax+1&&p.y>=Screen.safeArea.yMin-1&&p.y<=Screen.safeArea.yMax+1),"Seluruh HP muat dalam area aman: "+app);
        common.layar.GetWorldCorners(c);var t=new Vector3[4];((RectTransform)h.transisiAplikasi.transform).GetWorldCorners(t);
        Check(Vector3.Distance(c[0],t[0])<2&&Vector3.Distance(c[2],t[2])<2,"Efek TV mengikuti layar isi, bukan seluruh HP: "+app);
        Check(common.gameObject.activeInHierarchy&&common.kelompok.alpha>.99f&&!h.hpUtama.activeSelf,"Bingkai HP tetap tampil setelah transisi: "+app);
    }
    static IEnumerator Close(SistemHP h)
    {
        Click(h.ponselBersama.keluar);yield return Wait(1.4f);
        Check(h.hpSedangBuka&&!h.AplikasiSedangBuka&&h.hpUtama.activeInHierarchy&&!h.ponselBersama.gameObject.activeSelf,"X mengembalikan menu ikon portrait");
        Check(h.poseKarakter.SedangMemegang&&DialogManager.Instance.arthurScript.kunciPergerakan,"Arthur tetap memegang HP dan terkunci pada menu ikon");
    }
    static IEnumerator Run()
    {
        Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();yield return Wait(.4f);
        foreach(var m in EventSystem.current.GetComponents<BaseInputModule>())m.enabled=false;
        var h=UnityEngine.Object.FindFirstObjectByType<SistemHP>();var g=h.gameRhythm;g.jedaSaatKehilanganFokus=false;
        Check(GameObject.Find("NPC_Raka_RapidTap")==null,"NPC khusus tes Raka sudah dihapus");
        Check(GameObject.Find("NPC_Nara_Ekspresi")!=null&&GameObject.Find("NPC_Bimo_Joget")!=null,"NPC lain tetap ada");
        Check(h.daftarIkon.Count==8&&h.daftarIkon.Count(i=>i.name=="IkonGame")==1,"Delapan ikon, satu ikon Game");
        Check(h.daftarIkon.All(i=>((RectTransform)i.transform).sizeDelta==new Vector2(78,78)),"Ikon Game berukuran setara dengan ikon lainnya");
        Check(h.daftarIkon.First(i=>i.name=="IkonGame").GetComponent<WigglyContainer>()!=null,"Ikon Game memiliki wiggly");
        Check(PonselAplikasiBersama.SkalaPas(new Vector2(1280,720),new Vector2(1830,1010))<.7f,"Bingkai otomatis mengecil pada ruang kecil");
        h.BukaHPDefault();yield return Wait(1f);ScreenCapture.CaptureScreenshot(Work("phone-home-game.png"));yield return Wait(.2f);
        for(int i=0;i<6;i++)h.TekanTombolUIKanan();
        Check(h.daftarIkon.First(i=>i.name=="IkonGame").transform.Find("TelahTerpilih").gameObject.activeSelf,"Navigasi kanan/kiri dapat memilih ikon Game");
        foreach(string icon in new[]{"IkonMap","IkonSetting","IkonTas","IkonToDoLIst","IkonPesan","IkonMusic"})
        {
            Click(h.daftarIkon.First(i=>i.name==icon).GetComponent<UnityEngine.UI.Button>());yield return Wait(1.5f);Frame(h,icon);
            if(icon=="IkonMap"){h.petaKampus.PilihKanan();h.petaKampus.Interaksi();yield return Wait(.4f);Check(h.petaKampus.SedangFokus&&h.petaKampus.Zoom>1,"Peta tetap bisa dipilih dan diperbesar");}
            if(icon=="IkonSetting")
            {
                var p=h.panelPengaturan.GetComponent<PengaturanManager>();float v=p.sliderVolume.value;p.sliderVolume.value=Mathf.Clamp(v-.05f,p.sliderVolume.minValue,p.sliderVolume.maxValue);Check(Mathf.Abs(AudioListener.volume-p.sliderVolume.value)<.01f,"Slider volume Pengaturan tetap terhubung");p.sliderVolume.value=v;
                var settings=h.panelPengaturan.GetComponent<PengaturanGameplay>();settings.MintaKeluar();Check(settings.panelKonfirmasi.activeSelf,"Konfirmasi keluar tetap bisa dibuka");settings.BatalKeluar();
            }
            if(icon=="IkonTas"){int old=h.tas.Pilihan;h.tas.Kanan();Check(h.tas.Pilihan==old+1,"Tas masih bisa berganti barang");}
            ScreenCapture.CaptureScreenshot(Work("phone-app-"+icon+".png"));yield return Wait(.2f);yield return Close(h);
        }
        h.TekanTombolUIKanan();h.TekanTombolUIKiri();
        Click(h.daftarIkon.First(i=>i.name=="IkonGame").GetComponent<UnityEngine.UI.Button>());yield return Wait(1.5f);Frame(h,"Rhythm");
        Check(g.Status==RhythmArthur.Fase.Siap&&h.GameSedangBuka&&g.panel.activeInHierarchy,"Ikon Game membuka rhythm tanpa NPC");
        Check(DialogManager.Instance.minigameAktif,"Minigame menahan Lazy Bar lewat status yang sama");
        var lanes=g.panel.GetComponentsInChildren<RhythmJalurInput>();var c1=new Vector3[4];((RectTransform)lanes[0].transform).GetWorldCorners(c1);
        Check(c1[2].x-c1[0].x>320&&c1[2].y-c1[0].y>120,"Tombol sentuh rhythm lebar dan tinggi");
        Check(h.daftarIkon.First(i=>i.name=="IkonGame").GetComponent<Image>().color.a==0,"Ikon controller tanpa latar kotak");
        Check(lanes.All(l=>l.GetComponentInChildren<TMP_Text>().text=="KETUK"),"Empat tombol memakai label sentuh, tanpa huruf keyboard");
        Check(h.ponselBersama.layar.rect.width/((RectTransform)h.ponselBersama.transform).rect.width>.90f,"Layar isi memakai lebih dari 90 persen lebar HP");
        ScreenCapture.CaptureScreenshot(Work("phone-rhythm-siap.png"));yield return Wait(.2f);Click(g.tombolUtama);
        int next=0;bool shot=false;float limit=Time.realtimeSinceStartup+25;
        while(g.Status==RhythmArthur.Fase.Bermain&&Time.realtimeSinceStartup<limit)
        {
            double t=g.WaktuLagu;
            while(next<g.Ronde.not.Length&&t>=g.Ronde.not[next].waktu-.02)
            {
                Canvas.ForceUpdateCanvases();var lane=lanes.First(i=>i.jalur==g.Ronde.not[next].jalur);var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,lane.transform.position),button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
                Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject)==lane.gameObject,"Jalur rhythm dapat ditekan: "+next+" / "+g.Status+" / "+(hits.Count>0?hits[0].gameObject.name:"tanpa hit")+" / t="+t.ToString("0.00"));ExecuteEvents.Execute(lane.gameObject,e,ExecuteEvents.pointerDownHandler);next++;
            }
            // Capture Editor dapat menahan main thread; hentikan clock audio bersamanya.
            if(!shot&&g.Ronde.Terkena>=10){shot=true;g.Jeda();g.menu.SetActive(false);ScreenCapture.CaptureScreenshot(Work("phone-rhythm-main.png"));yield return Wait(.4f);g.Lanjutkan();}
            yield return null;
        }
        Check(g.Status==RhythmArthur.Fase.Menang&&g.Ronde.Terkena==42,"Rhythm lengkap dapat dimainkan dari ikon HP");
        ScreenCapture.CaptureScreenshot(Work("phone-rhythm-menang.png"));yield return Wait(.2f);yield return Close(h);
        Check(!DialogManager.Instance.minigameAktif&&!g.audioBeat.isPlaying,"Kembali ke ikon menghentikan rhythm dan melepas status minigame");
        h.TutupHP();yield return Wait(.4f);Check(!h.hpSedangBuka&&!DialogManager.Instance.arthurScript.kunciPergerakan,"Menutup HP memulihkan kontrol Arthur");
        Check(errors.Count==0,"Tidak ada error baru sepanjang pengujian seluruh aplikasi dan rhythm");
    }
}
