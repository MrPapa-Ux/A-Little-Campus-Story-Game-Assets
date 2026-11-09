// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Rhythm; alat pengujian Editor, bukan komponen gameplay.
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

public static class CampusRhythmChecks
{
    const string Key="Campus.Rhythm.Checks";
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    static bool started;
    static string Work(string name)=>Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",name);
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    public static void Begin()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Mulai dari Edit Mode.");
        EditorSceneManager.SaveOpenScenes();EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        File.WriteAllText(Work("rhythm-checks.txt"),"RUNNING\n");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
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
        File.WriteAllText(Work("rhythm-checks.txt"),(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));started=false;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;
    }
    static void Check(bool ok,string note){if(!ok)throw new Exception(note);results.Add("OK: "+note);}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}

    static void ModelChecks()
    {
        var notes=new[]{new NotArthur(0,1),new NotArthur(1,1.5f),new NotArthur(2,2)};
        var a=new ArthurRhythmRound(notes);Check(a.Tekan(0,1)==ArthurRhythmRound.Nilai.Perfect,"Timing tepat dinilai Perfect");
        Check(a.Tekan(1,1.68)==ArthurRhythmRound.Nilai.Good,"Timing meleset kecil dinilai Good");
        Check(a.Tekan(3,2)==ArthurRhythmRound.Nilai.Kosong&&a.Salah==1,"Salah jalur tidak mengenai not");
        Check(a.Combo==0&&a.MaxCombo==2,"Salah input memutus combo tetapi menyimpan max combo");
        a.Lewatkan(2.23);Check(a.Miss==1,"Not terlewat melewati jendela waktu dihitung Miss");
        Check(!a.Lulus(.7f),"Tidak cukup mengenai not menghasilkan gagal");
        var b=new ArthurRhythmRound(notes);b.Tekan(0,1);b.Tekan(0,1);Check(b.Perfect==1&&b.Salah==1,"Satu not tidak dapat diklaim dua kali");
        var c=new ArthurRhythmRound(new[]{new NotArthur(0,1),new NotArthur(1,1)});c.Tekan(0,1);c.Tekan(1,1);Check(c.Combo==2,"Dua jalur bersamaan dapat dinilai terpisah");
        var d=new ArthurRhythmRound(notes);for(int i=0;i<13;i++)d.Tekan(3,0);Check(d.Energi==0&&!d.Lulus(.7f),"Spam ketukan kosong menghabiskan fokus");
        var edge=new ArthurRhythmRound(notes);Check(edge.Tekan(0,1.219)==ArthurRhythmRound.Nilai.Good,"Tepi dalam jendela Good diterima");
        var outside=new ArthurRhythmRound(notes);Check(outside.Tekan(0,1.23)==ArthurRhythmRound.Nilai.Kosong,"Di luar jendela Good tidak diterima");
        var chart=RhythmArthur.BuatChart(110);Check(chart.Length==42&&chart.All(n=>n.jalur>=0&&n.jalur<4),"Chart latihan memiliki 42 not ketuk di empat jalur");
        Check(chart.Zip(chart.Skip(1),(x,y)=>x.waktu<=y.waktu).All(x=>x),"Urutan chart meningkat mengikuti tempo");
    }
    static void Click(UnityEngine.UI.Button b)
    {
        Canvas.ForceUpdateCanvases();var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,b.transform.position),button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
        Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==b.gameObject,"Tombol dapat disentuh: "+b.name);ExecuteEvents.Execute(b.gameObject,e,ExecuteEvents.pointerClickHandler);
    }
    static void Lane(RhythmArthur g,int lane)
    {
        var input=g.panel.GetComponentsInChildren<RhythmJalurInput>().First(x=>x.jalur==lane);Canvas.ForceUpdateCanvases();var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,input.transform.position),button=PointerEventData.InputButton.Left};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
        Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject)==input.gameObject,"Jalur menerima sentuhan pointer down: "+lane);ExecuteEvents.Execute(input.gameObject,e,ExecuteEvents.pointerDownHandler);
    }
    static IEnumerator Run()
    {
        ModelChecks();Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();yield return Wait(.5f);
        // Bot mengirim event sentuh sendiri; input fisik pengguna di aplikasi lain tidak ikut diuji.
        foreach(var module in EventSystem.current.GetComponents<BaseInputModule>())module.enabled=false;
        var g=UnityEngine.Object.FindFirstObjectByType<RhythmArthur>();var bridge=g.GetComponent<RapidTapArthur>();var dm=g.dialog;var choices=dm.GetComponent<SistemPilihan>();var p=dm.arthurScript;var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();
        Check(g.jedaSaatKehilanganFokus,"Auto-pause saat kehilangan fokus aktif secara default");g.jedaSaatKehilanganFokus=false;
        p.GetComponent<Rigidbody2D>().gravityScale=0;Check(!g.panel.activeSelf&&!bridge.panel.activeSelf,"Rhythm dan panel ketuk lama tersembunyi saat mulai");
        p.transform.position=new Vector3(-3.6f,.1f,p.transform.position.z);Physics2D.SyncTransforms();yield return Wait(.3f);
        Check(dm.idInteraksiAktif=="NPC_Raka_RapidTap","NPC yang sama tetap menjadi pintu masuk");
        var music=MusikArthur.Instance;bool musicTest=music!=null&&music.Lagu?.audio!=null&&!music.TemaAktif;
        if(musicTest&&!music.Memutar){music.PutarJeda();yield return Wait(.2f);}
        AudioClip beforeClip=musicTest?music.sumber.clip:null;
        choices.kecepatanKetik=.001f;choices.AksiTombolInteraksi();float limit=Time.realtimeSinceStartup+12;while(choices.teksPilihanUtama[0].text!="Main rhythm di HP"&&Time.realtimeSinceStartup<limit)yield return null;yield return Wait(.4f);
        choices.TekanPilihanLangsung(0);yield return Wait(.7f);
        Check(g.Status==RhythmArthur.Fase.Siap&&!dm.canvasDialog.activeSelf,"NPC membuka game rhythm, bukan panel rapid tap lama");
        Check(!bridge.panel.activeSelf&&g.panel.activeSelf,"Panel lama tetap tidak tampil");
        Check(dm.minigameAktif&&p.kunciPergerakan,"Gerakan terkunci selama bermain di HP");
        if(musicTest)Check(!music.Memutar,"Musik playlist dijeda agar tidak menimpa beat rhythm");
        hp.BukaHPDefault();Check(!hp.hpSedangBuka,"Menu HP biasa tidak menumpuk di atas game rhythm");
        Check(g.pose!=null&&g.pose.SedangMemegang,"Arthur memakai pose memegang HP saat minigame");
        ScreenCapture.CaptureScreenshot(Work("rhythm-siap.png"));yield return Wait(.2f);
        int wins=0,losses=0;g.saatMenang.AddListener(()=>wins++);g.saatGagal.AddListener(()=>losses++);
        Click(g.tombolUtama);yield return Wait(.65f);
        Check(g.Ronde.not.Length==42&&g.audioBeat.clip!=null,"Chart dan audio beat disiapkan saat mulai");
        Check(g.audioBeat.isPlaying&&Math.Abs(g.audioBeat.time-g.WaktuLagu)<.15,"Waktu not mengikuti clock audio yang sedang dimainkan");
        var samples=new float[2205];g.audioBeat.clip.GetData(samples,22050*2);Check(samples.Any(x=>Math.Abs(x)>.01f),"Beat berisi sampel instrumental, bukan audio kosong");
        Click(g.tombolJeda);double paused=g.WaktuLagu;yield return Wait(.4f);Check(Math.Abs(paused-g.WaktuLagu)<.001&&!g.audioBeat.isPlaying,"Jeda menghentikan waktu not dan audio bersama");
        Click(g.tombolUtama);yield return Wait(.2f);Check(g.Status==RhythmArthur.Fase.Bermain&&g.WaktuLagu>paused,"Lanjut mempertahankan posisi lagu");
        g.jedaSaatKehilanganFokus=true;g.SendMessage("OnApplicationFocus",false);Check(g.Status==RhythmArthur.Fase.Jeda,"Kehilangan fokus memicu jeda yang aman");g.jedaSaatKehilanganFokus=false;Click(g.tombolUtama);
        int objects=g.panel.GetComponentsInChildren<Transform>(true).Length;bool shot=false;limit=Time.realtimeSinceStartup+28;
        while(g.Status==RhythmArthur.Fase.Bermain&&Time.realtimeSinceStartup<limit)
        {
            double t=g.WaktuLagu;for(int i=0;i<g.Ronde.not.Length;i++)if(g.Ronde.hasil[i]==0&&t>=g.Ronde.not[i].waktu-.025&&t<=g.Ronde.not[i].waktu+.15)Lane(g,g.Ronde.not[i].jalur);
            if(!shot&&g.Ronde.Terkena>=10){shot=true;ScreenCapture.CaptureScreenshot(Work("rhythm-main.png"));}
            yield return null;
        }
        results.Add("STATE: "+g.Status+" / hit="+g.Ronde.Terkena+" miss="+g.Ronde.Miss+" wrong="+g.Ronde.Salah+" fokus="+g.Ronde.Energi+" waktu="+g.WaktuLagu);
        Check(g.Status==RhythmArthur.Fase.Menang&&wins==1,"Satu ronde nyata dapat diselesaikan dengan mengikuti not");
        Check(g.Ronde.Terkena==42&&g.Ronde.MaxCombo==42,"Semua not bisa dipukul dan combo penuh tercatat");
        Check(g.panel.GetComponentsInChildren<Transform>(true).Length==objects,"Not digambar tanpa membuat objek baru sepanjang ronde");
        ScreenCapture.CaptureScreenshot(Work("rhythm-menang.png"));yield return Wait(.2f);Click(g.tombolUtama);yield return Wait(.5f);
        Check(g.Status==RhythmArthur.Fase.Bermain&&g.Ronde.Skor==0&&g.Ronde.Combo==0,"Main ulang mengosongkan skor dan combo");
        for(int i=0;i<13&&g.Status==RhythmArthur.Fase.Bermain;i++){Lane(g,3);yield return Wait(.02f);}
        yield return Wait(.85f);Check(g.Status==RhythmArthur.Fase.Gagal&&losses==1,"Spam ketukan kosong menghasilkan gagal dan glitch singkat");
        Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="CharacterLab"&&!g.lazyBar.waktuHabis,"Percobaan rhythm tidak mereset progres cerita");
        ScreenCapture.CaptureScreenshot(Work("rhythm-gagal.png"));yield return Wait(.2f);Click(g.tombolKeluar);
        Check(!dm.minigameAktif&&!p.kunciPergerakan&&!g.panel.activeSelf,"Keluar memulihkan gerak dan sistem interaksi");
        if(musicTest){yield return Wait(.15f);Check(music.Memutar&&music.sumber.clip==beforeClip,"Playlist dilanjutkan dari lagu sebelumnya setelah keluar");}
        g.Buka();g.MulaiRonde();yield return Wait(.2f);g.Jeda();g.enabled=false;yield return null;Check(!dm.minigameAktif&&!p.kunciPergerakan&&!g.audioBeat.isPlaying,"Menonaktifkan game saat jeda membersihkan audio dan kunci");g.enabled=true;
        Check(errors.Count==0,"Tidak ada error baru selama rhythm, jeda, menang, gagal, dan kembali");
    }
}
