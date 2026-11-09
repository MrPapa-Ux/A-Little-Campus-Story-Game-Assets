// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Rapid Tap; alat pengujian Editor, bukan komponen gameplay.
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

public static class CampusRapidTapChecks
{
    const string Key="Campus.RapidTap.Checks";
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    static bool started;
    static string Work(string name)=>Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",name);
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    public static void Begin()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Mulai dari Edit Mode.");
        EditorSceneManager.SaveOpenScenes();EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        File.WriteAllText(Work("rapid-tap-checks.txt"),"RUNNING\n");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
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
        File.WriteAllText(Work("rapid-tap-checks.txt"),(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));started=false;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;
    }
    static void Check(bool ok,string note){if(!ok)throw new Exception(note);results.Add("OK: "+note);}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}

    static void Click(UnityEngine.UI.Button b)
    {
        Canvas.ForceUpdateCanvases();var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,b.transform.position),button=PointerEventData.InputButton.Left};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
        Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==b.gameObject,"Tombol dapat disentuh: "+b.name);
        ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerClickHandler);
    }
    static IEnumerator Run()
    {
        Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();yield return Wait(.5f);
        var game=UnityEngine.Object.FindFirstObjectByType<RapidTapArthur>();var dm=game.dialog;var choices=dm.GetComponent<SistemPilihan>();var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();var player=dm.arthurScript;
        var npc=GameObject.Find("NPC_Raka_RapidTap");var area=npc.GetComponent<AreaInteraksi>();player.GetComponent<Rigidbody2D>().gravityScale=0;
        Check(game.Status==RapidTapArthur.Fase.Tutup&&!game.panel.activeSelf,"Minigame tersembunyi saat mulai game");
        Check(UnityEngine.Object.FindObjectsByType<AreaInteraksi>(FindObjectsSortMode.None).Count(a=>a.idTempat==npc.name)==1,"Hanya satu NPC Raka ditambahkan");
        Check(game.isiBar.sprite!=null,"Indikator memiliki sprite agar fillAmount benar-benar terlihat");
        player.transform.position=new Vector3(-3.6f,.1f,player.transform.position.z);Physics2D.SyncTransforms();yield return Wait(.3f);
        Check(dm.bisaInteraksi&&dm.idInteraksiAktif==npc.name,"Trigger NPC Raka terdeteksi oleh Arthur melalui fisika");
        ScreenCapture.CaptureScreenshot(Work("rapid-tap-npc.png"));yield return Wait(.2f);
        choices.kecepatanKetik=.001f;choices.AksiTombolInteraksi();
        float limit=Time.realtimeSinceStartup+12;
        while(choices.teksPilihanUtama[0].text!="Coba rapid tap"&&Time.realtimeSinceStartup<limit)yield return null;
        yield return Wait(.4f);
        Check(dm.canvasDialog.activeInHierarchy,"Interaksi membuka dialog NPC");
        Check(choices.teksNamaKanan.text=="Raka","Nama NPC di dialog benar");
        choices.TekanPilihanLangsung(0);yield return Wait(.8f);
        Check(game.Status==RapidTapArthur.Fase.Siap&&!dm.canvasDialog.activeSelf,"Pilihan Coba rapid tap menutup dialog lalu membuka instruksi");
        Check(dm.minigameAktif&&player.kunciPergerakan,"Minigame mengunci gerak dan sistem interaksi");
        hp.BukaHPDefault();choices.AksiTombolInteraksi();Check(!hp.hpSedangBuka&&!dm.canvasDialog.activeSelf,"HP dan dialog tidak terbuka di atas minigame");
        var field=typeof(LazyBarArthur).GetField("sisaWaktu",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);float before=(float)field.GetValue(game.lazyBar);
        yield return Wait(.6f);Check(Mathf.Abs(before-(float)field.GetValue(game.lazyBar))<.001f,"Lazy Bar tidak terkuras selama instruksi");
        ScreenCapture.CaptureScreenshot(Work("rapid-tap-siap.png"));yield return Wait(.2f);
        Check(game.panel.transform.Find("KertasLatihan/KartuArthur/Potret_Rambut 1")!=null,"Potret Arthur lengkap dengan lapisan rambut");
        int wins=0,losses=0;game.saatMenang.AddListener(()=>wins++);game.saatGagal.AddListener(()=>losses++);
        Click(game.tombolMulai);yield return Wait(.2f);
        Check(game.Status==RapidTapArthur.Fase.Bermain&&game.Ketukan==0,"Timer dimulai hanya setelah tombol Mulai");
        var down=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};ExecuteEvents.Execute(game.tombolKetuk.gameObject,down,ExecuteEvents.pointerDownHandler);yield return Wait(.25f);ExecuteEvents.Execute(game.tombolKetuk.gameObject,down,ExecuteEvents.pointerUpHandler);
        Check(game.Ketukan==0,"Menahan tombol tidak menghasilkan ketukan otomatis");
        for(int i=0;i<20;i++)
        {
            Click(game.tombolKetuk);game.Ketuk();Check(game.Ketukan==i+1,"Satu klik dihitung sekali walau ada input ganda pada frame sama: "+i);
            if(i==9){ScreenCapture.CaptureScreenshot(Work("rapid-tap-main.png"));yield return Wait(.2f);}
            yield return Wait(.035f);
        }
        Check(game.Status==RapidTapArthur.Fase.Menang&&wins==1&&losses==0,"Dua puluh ketukan menghasilkan satu kemenangan");
        Check(Mathf.Approximately(game.isiBar.fillAmount,1),"Indikator penuh saat menang");
        game.Ketuk();Check(wins==1&&game.Ketukan==20,"Ketukan setelah menang tidak menambah hasil");
        ScreenCapture.CaptureScreenshot(Work("rapid-tap-menang.png"));yield return Wait(.2f);
        game.targetKetukan=3;game.durasiDetik=.5f;Click(game.tombolMulai);yield return Wait(.6f);
        Check(game.Status==RapidTapArthur.Fase.Glitch&&losses==1,"Waktu habis menghasilkan glitch percobaan");
        ScreenCapture.CaptureScreenshot(Work("rapid-tap-gagal-glitch.png"));yield return Wait(.85f);
        Check(game.Status==RapidTapArthur.Fase.Gagal&&!game.lazyBar.panelGlitch.activeSelf,"Glitch singkat berakhir dan hasil gagal bisa dicoba ulang");
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="CharacterLab"&&!game.lazyBar.waktuHabis,"Gagal latihan tidak memuat ulang cerita atau memicu Lazy Bar");
        ScreenCapture.CaptureScreenshot(Work("rapid-tap-gagal.png"));yield return Wait(.2f);
        game.targetKetukan=2;game.durasiDetik=2;Click(game.tombolMulai);yield return Wait(.2f);Click(game.tombolKetuk);yield return null;Click(game.tombolKetuk);
        Check(game.Status==RapidTapArthur.Fase.Menang&&wins==2,"Percobaan ulang dapat dimenangkan");
        Click(game.tombolKeluar);yield return null;Check(!dm.minigameAktif&&!player.kunciPergerakan&&!game.panel.activeSelf,"Keluar mengembalikan gerakan dan interaksi");
        for(int i=0;i<2;i++){game.Buka();Check(game.Ketukan==0&&game.Status==RapidTapArthur.Fase.Siap,"Buka ulang bersih: "+i);game.MulaiRonde();game.Tutup();Check(!dm.minigameAktif&&!player.kunciPergerakan,"Batal saat ronde memulihkan kontrol: "+i);}
        game.Buka();game.MulaiRonde();yield return Wait(.15f);
        typeof(RapidTapArthur).GetField("akhir",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(game,Time.unscaledTime-.01f);game.Ketuk();
        Check(game.Ketukan==0&&game.Status==RapidTapArthur.Fase.Glitch,"Ketukan setelah deadline tidak bisa menyelamatkan ronde");
        game.enabled=false;yield return null;Check(!dm.minigameAktif&&!player.kunciPergerakan&&!game.lazyBar.panelGlitch.activeSelf,"Menonaktifkan komponen saat glitch memulihkan kontrol dan overlay");game.enabled=true;
        player.transform.position=new Vector3(7.6f,.1f,player.transform.position.z);Physics2D.SyncTransforms();yield return Wait(.3f);
        Check(dm.idInteraksiAktif=="NPC_Nara_Ekspresi","NPC lama masih dapat diinteraksi setelah latihan");
        choices.AksiTombolInteraksi();yield return Wait(.2f);Check(dm.canvasDialog.activeSelf&&game.Status==RapidTapArthur.Fase.Tutup,"Dialog NPC lama tidak membuka minigame");dm.TutupDialog();
        Check(errors.Count==0,"Tidak ada error baru selama dialog, menang, gagal, dan coba ulang");
    }
}
