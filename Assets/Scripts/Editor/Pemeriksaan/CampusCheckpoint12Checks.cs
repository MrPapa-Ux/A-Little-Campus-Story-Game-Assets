// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Checkpoint 12; alat pengujian Editor, bukan komponen gameplay.
// Panduan: Assets/Scripts/Editor/Pemeriksaan/README.md

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

public static class CampusCheckpoint12Checks
{
    const string Key="Campus.Checkpoint12.Tests";
    static IEnumerator running;
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static string Work(string name)=>Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",name);
    static float oldVolume;static bool hadVolume;
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    public static void Begin()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Mulai dari Edit Mode.");
        EditorSceneManager.SaveOpenScenes();EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        File.WriteAllText(Work("checkpoint12-checks.txt"),"RUNNING\n");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false) || EditorApplication.isCompiling || EditorApplication.isUpdating || !EditorApplication.isPlaying)return;
        if(running==null){results.Clear();errors.Clear();Application.logMessageReceived+=Log;hadVolume=PlayerPrefs.HasKey("ArthurMusicVolume");oldVolume=PlayerPrefs.GetFloat("ArthurMusicVolume",.65f);running=Run();stack.Clear();stack.Push(running);}
        try{if(!Advance())Finish(null);}catch(Exception e){Finish(e);}
    }
    static bool Advance(){while(stack.Count>0){var next=stack.Peek();if(!next.MoveNext()){stack.Pop();continue;}if(next.Current is IEnumerator nested){stack.Push(nested);continue;}return true;}return false;}
    static void Log(string s,string trace,LogType type){if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert)errors.Add(s);}
    static void Finish(Exception e)
    {
        Application.logMessageReceived-=Log;if(e==null&&errors.Count>0)e=new Exception(string.Join("\n",errors));
        File.WriteAllText(Work("checkpoint12-checks.txt"),(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));
        if(hadVolume)PlayerPrefs.SetFloat("ArthurMusicVolume",oldVolume);else PlayerPrefs.DeleteKey("ArthurMusicVolume");PlayerPrefs.Save();
        running=null;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;
    }
    static void Check(bool ok,string note){if(!ok)throw new Exception(note);results.Add("OK: "+note);}
    static IEnumerator Wait(float seconds){float until=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<until)yield return null;}
    static GameObject Hit(RectTransform rect)
    {
        Canvas.ForceUpdateCanvases();var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.position)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
        return hits.Count==0?null:ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
    }
    static void Click(GameObject obj){Check(Hit(obj.GetComponent<RectTransform>())==obj,"Sentuhan tidak terhalang: "+obj.name);ExecuteEvents.Execute(obj,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);}
    static void Capture(string name){ScreenCapture.CaptureScreenshot(Work("checkpoint12-"+name+".png"));}
    static AudioClip TestClip(string name)
    {
        var clip=AudioClip.Create(name,22050*12,1,22050,false);float[] data=new float[clip.samples];for(int i=0;i<data.Length;i++)data[i]=Mathf.Sin(i*Mathf.PI*2*330/22050)*.002f;clip.SetData(data,0);return clip;
    }
    static IEnumerator Run()
    {
        Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();yield return null;
        var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();var player=UnityEngine.Object.FindFirstObjectByType<KarakterController>();
        var lazy=player.GetComponent<LazyBarArthur>();if(lazy!=null)lazy.enabled=false;
        player.GetComponent<Rigidbody2D>().gravityScale=0;player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
        var music=MusikArthur.Instance;var ui=hp.musik;var buku=hp.GetComponent<BukuArthur>();
        Check(music!=null && UnityEngine.Object.FindObjectsByType<MusikArthur>(FindObjectsSortMode.None).Length==1,"Satu pengelola musik aktif");
        Check(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length==1,"Satu AudioListener aktif");
        Check(music.playlist.lagu.Length==5 && music.playlist.lagu.All(l=>l.audio==null),"Lima slot Arthur kosong tanpa lagu final palsu");
        Check(!music.sumber.isPlaying && !music.tema.isPlaying,"Tidak ada musik atau tema lokasi diputar otomatis");
        Check(UnityEngine.Object.FindObjectsByType<PemutarMusikUI>(FindObjectsInactive.Include,FindObjectsSortMode.None).All(x=>!x.enabled && !x.GetComponent<AudioSource>().playOnAwake),"Player lagu uji lama berhenti otomatis tanpa menghapus aset");
        Check(buku.tugas.Count==0&&buku.pesan.Count==0&&buku.barang.Count==0,"To-do, pesan dan tas kosong sesuai permintaan");
        hp.BukaHPDefault();yield return Wait(1f);
        Check(hp.daftarIkon.Count==7 && hp.daftarIkon.Count(x=>x.name=="IkonMusic")==1,"Tujuh ikon HP termasuk Music, tanpa duplikat");
        foreach(var icon in hp.daftarIkon)Check(Hit((RectTransform)icon.transform)==icon,"Ikon HP dapat disentuh: "+icon.name);
        var musicIcon=hp.daftarIkon.First(x=>x.name=="IkonMusic");var note=(RectTransform)musicIcon.transform.Find("GambarNot");
        Check(musicIcon.GetComponent<UnityEngine.UI.Image>().color.a==0 && musicIcon.GetComponent<UnityEngine.UI.Outline>()==null,"Ikon Music hanya sprite not tanpa kotak warna");
        Check(note.GetComponent<WigglyBox>()!=null,"Ikon not memiliki wiggly");
        var iconPos=note.anchoredPosition;var iconRot=note.localRotation;yield return Wait(.3f);
        Check(Vector2.Distance(iconPos,note.anchoredPosition)>.01f || Quaternion.Angle(iconRot,note.localRotation)>.01f,"Sprite not benar-benar bergerak di HP");
        Check(hp.tugas.transform.Find("BingkaiHP/Kertas/KertasDetail/Selotip")!=null,"Desain To-do berselotip dipertahankan");
        Check(hp.tas.transform.Find("BingkaiHP/Kertas/BagianDalamTas/Ritsleting")!=null && hp.tas.tombolBaris.Length==6,"Tas mempunyai lapisan kain, ritsleting dan enam slot");
        Check(hp.pesan.transform.Find("BingkaiHP/Kertas/LayarPercakapan")!=null && hp.pesan.avatarBaris.Length==5,"Pesan mempunyai layar LCD dan lima avatar kontak");
        Capture("phone");yield return null;
        Click(hp.daftarIkon.First(x=>x.name=="IkonMusic"));yield return Wait(1.3f);
        Check(ui.gameObject.activeInHierarchy&&!hp.hpUtama.activeSelf,"Music terbuka dalam panel landscape dan portrait disembunyikan");
        Check(ui.transform.Find("BingkaiHP/Kertas/Subjudul")==null,"Slogan Lima Lagu Satu Kaset sudah dihapus");
        Check(ui.pita.GetComponent<UnityEngine.UI.Image>().color.r>.9f && ui.pita.GetComponent<UnityEngine.UI.Image>().color.b<.1f,"Mixtape Arthur berwarna oranye");
        foreach(var text in new[]{ui.labelPutar,ui.labelAcak,ui.labelUlang,ui.transform.Find("BingkaiHP/Kertas/VolumeLabel").GetComponent<TMPro.TMP_Text>()})Check(text.GetComponent<WigglyText>()?.isActiveAndEnabled==true,"Wiggly aktif: "+text.transform.parent.name+"/"+text.name);
        int idleSteps=ui.coretan.JumlahLangkah;yield return Wait(.3f);Check(ui.coretan.JumlahLangkah==idleSteps,"Coretan kaset diam ketika tidak ada lagu diputar");
        Check(ui.namaSlot.All(x=>x.text.Contains("belum diisi")),"Kelima baris lagu menampilkan keadaan kosong");
        ui.judulLagu.ForceMeshUpdate();Check(ui.judulLagu.textInfo.characterInfo.Take(ui.judulLagu.textInfo.characterCount).Any(x=>x.isVisible),"Judul lagu terlihat dan tidak tersembunyi oleh tinggi kotak teks");
        var tapeArt=ui.GetComponentInChildren<KasetCoretanUI>();Check(tapeArt.GetComponent<CanvasRenderer>()!=null,"Gulungan kaset memiliki CanvasRenderer");
        Click(ui.transform.Find("BingkaiHP/Kertas/Putar").gameObject);Check(!music.Memutar,"Putar slot kosong tidak menghasilkan audio atau error");
        Click(ui.transform.Find("BingkaiHP/Kertas/Acak").gameObject);Check(music.acak,"Tombol acak terhubung");
        Click(ui.transform.Find("BingkaiHP/Kertas/Ulang").gameObject);Check(music.ulang,"Tombol ulang terhubung");
        ui.volume.value=.37f;Check(Mathf.Abs(music.Volume-.37f)<.001f,"Slider volume mengatur sumber musik");
        Capture("music");yield return null;
        var playlist=UnityEngine.Object.Instantiate(music.playlist);music.playlist=playlist;
        AudioClip clipA=TestClip("Fixture A"),clipB=TestClip("Fixture B"),theme=TestClip("Fixture Tema");
        playlist.lagu[0].audio=clipA;playlist.lagu[3].audio=clipB;
        music.acak=false;music.ulang=false;music.Pilih(0);ui.Putar();yield return Wait(.1f);
        int movingSteps=ui.coretan.JumlahLangkah;Canvas.ForceUpdateCanvases();var vertices12=ui.coretan.canvasRenderer.GetMesh().vertices;
        yield return Wait(.35f);Canvas.ForceUpdateCanvases();var verticesAfter12=ui.coretan.canvasRenderer.GetMesh().vertices;
        Check(ui.coretan.JumlahLangkah>movingSteps && !vertices12.SequenceEqual(verticesAfter12),"Coretan kaset benar-benar berubah geometri ketika lagu diputar");
        Check(music.Memutar&&music.sumber.clip==clipA,"Player memutar AudioClip yang ditugaskan");
        ui.Putar();Check(music.Dijeda&&!music.Memutar,"Jeda menghentikan sementara lagu");int pausedSteps=ui.coretan.JumlahLangkah;yield return Wait(.35f);Check(ui.coretan.JumlahLangkah==pausedSteps,"Jeda lagu menghentikan wiggly coretan kaset");ui.Putar();yield return null;Check(music.Memutar&&!music.Dijeda,"Lagu dapat dilanjutkan");
        ui.Geser(.4f);Check(music.Progres>.38f&&music.Progres<.5f,"Posisi lagu dapat digeser");
        hp.TekanTombolUIKanan();yield return null;Check(music.Pilihan==3&&music.sumber.clip==clipB,"Navigasi lagu melewati slot yang kosong");
        hp.TekanTombolUIKiri();yield return null;Check(music.Pilihan==0,"Navigasi sebelumnya kembali ke lagu tersedia");
        music.ulang=true;music.Geser(.9999f);yield return Wait(.35f);Check(music.Pilihan==0&&music.Memutar&&music.Progres<.2f,"Akhir lagu benar-benar mengulang saat ulang aktif");
        music.ulang=false;music.Geser(.9999f);yield return Wait(.35f);Check(music.Pilihan==3&&music.Memutar,"Akhir lagu otomatis melanjutkan ke slot berisi audio");
        music.acak=true;music.Berikutnya();yield return null;Check(music.Pilihan==0,"Acak memilih lagu tersedia lain dan melewati slot kosong");music.acak=false;
        music.MulaiTema(theme);yield return null;Check(music.TemaAktif&&music.tema.isPlaying&&!music.sumber.isPlaying,"Tema karakter mendapat prioritas tanpa dua lagu bersamaan");
        music.SelesaiTema();yield return null;Check(!music.TemaAktif&&music.Memutar,"Lagu pilihan dilanjutkan setelah tema selesai");
        Click(ui.transform.Find("BingkaiHP/Kertas/KembaliKeHP").gameObject);yield return Wait(1f);Check(hp.hpUtama.activeSelf&&music.Memutar,"Menutup aplikasi tidak menghentikan lagu");
        hp.TutupHP();yield return Wait(.5f);Check(!hp.hpSedangBuka&&music.Memutar,"Menutup HP tetap membiarkan musik berjalan");
        music.Pilih(1);Check(!music.Memutar,"Memilih slot kosong menghentikan lagu dengan aman");
        // Data uji hanya ada di runtime; asset playlist asli tidak pernah ditulis.
        music.playlist=AssetDatabase.LoadAssetAtPath<PlaylistArthur>("Assets/AudioArthur/Playlist_Arthur.asset");music.Pilih(0);
        UnityEngine.Object.Destroy(playlist);UnityEngine.Object.Destroy(clipA);UnityEngine.Object.Destroy(clipB);UnityEngine.Object.Destroy(theme);
        hp.BukaHPDefault();yield return Wait(1f);
        foreach(var app in new[]{hp.tugas,hp.pesan,hp.tas})
        {
            string icon=app==hp.tugas?"IkonToDoLIst":app==hp.pesan?"IkonPesan":"IkonTas";
            Click(hp.daftarIkon.First(x=>x.name==icon));yield return Wait(1.3f);
            Check(app.gameObject.activeInHierarchy&&!app.aksi.interactable,"Halaman kosong dan aksi aman: "+app.jenis);
            Capture(app.jenis.ToString().ToLowerInvariant());yield return null;
            for(int i=0;i<7;i++)buku.Tambah(app.jenis,new CatatanArthur{id="test_"+i,judul="Uji "+i,isi="Data runtime untuk pengujian.",jumlah=i+1});
            Check(app.detailJudul.text=="Uji 0"&&app.tombolBaris[0].interactable,"Data baru muncul di halaman: "+app.jenis);
            hp.TekanTombolUIKanan();Check(app.Pilihan==1,"Navigasi catatan kanan: "+app.jenis);
            hp.TekanTombolUIInteract();Check(app.jenis==JenisBukuArthur.Tas?!buku.Daftar(app.jenis)[1].selesai:buku.Daftar(app.jenis)[1].selesai,"Aksi catatan sesuai jenis: "+app.jenis);
            for(int i=0;i<5;i++)app.Kanan();Check(app.Pilihan==6&&app.labelBaris[6%app.labelBaris.Length].text.Contains("Uji 6"),"Halaman berikutnya bekerja: "+app.jenis);
            if(app.jenis==JenisBukuArthur.Tas)
            {
                app.Kiri();Check(app.Pilihan==5,"Slot keenam tas berada di halaman pertama");Click(app.tombolBaris[5].gameObject);Check(app.Pilihan==5,"Sentuhan slot keenam memilih item yang benar");
                var sprite=player.GetComponentInChildren<EkspresiWajah2D>().kepala.sprite;buku.barang[5].ikon=sprite;buku.Segarkan();Check(app.ikonBaris[5].gameObject.activeInHierarchy && app.ikonBaris[5].sprite==sprite,"Gambar item tampil di slot kotak tas");
                Capture("tas-isi-uji");yield return null;
            }
            if(app.jenis==JenisBukuArthur.Pesan)
            {
                var sprite=player.GetComponentInChildren<EkspresiWajah2D>().kepala.sprite;
                buku.pesan[0].pengirim="Kontak Uji";buku.pesan[0].ikon=sprite;buku.pesan[1].pengirim="Dirahasiakan";buku.pesan[1].anonim=true;buku.pesan[1].ikon=sprite;
                app.Kanan();buku.Segarkan();Check(app.detailJudul.text=="Kontak Uji" && app.fotoKontak.gameObject.activeInHierarchy && !app.avatarKontak.gameObject.activeInHierarchy,"Kontak bernama menampilkan foto sendiri");
                app.Kanan();Check(app.detailJudul.text=="Anonim" && !app.fotoKontak.gameObject.activeInHierarchy && app.avatarKontak.gameObject.activeInHierarchy,"Kontak anonim menyembunyikan nama dan foto dengan avatar kepala-bahu");
                Check(app.ikonBaris[0].gameObject.activeInHierarchy && app.avatarBaris[1].gameObject.activeInHierarchy,"Avatar kontak juga dibedakan pada daftar kiri");
                Capture("pesan-isi-uji");yield return null;
            }
            buku.Daftar(app.jenis).Clear();buku.Segarkan();Check(!app.aksi.interactable,"Menghapus data kembali ke keadaan kosong: "+app.jenis);
            Click(app.transform.Find("BingkaiHP/Kertas/KembaliKeHP").gameObject);yield return Wait(1f);
            Check(hp.hpUtama.activeSelf,"X kembali ke HP: "+app.jenis);
        }
        hp.BukaAplikasi("IkonMap");yield return Wait(1.3f);Check(hp.petaKampus.gameObject.activeInHierarchy,"Peta lama masih dapat dibuka");
        for(int i=0;i<14;i++)Check(Hit(hp.petaKampus.lokasi[i].lingkaran.rectTransform)==hp.petaKampus.lokasi[i].lingkaran.gameObject,"Marker map tetap dapat disentuh: "+i);
        hp.TekanTombolUIKanan();hp.TekanTombolUIInteract();yield return Wait(.5f);Check(hp.petaKampus.Zoom>2f,"Zoom peta tetap berfungsi");
        hp.TutupAplikasi();yield return Wait(1f);hp.BukaPengaturan();yield return Wait(1.3f);Check(hp.panelPengaturan.activeInHierarchy,"Pengaturan tetap dapat dibuka");
        hp.TutupAplikasi();yield return Wait(1f);hp.TutupHP();yield return Wait(.5f);
        Check(!player.kunciPergerakan&&!hp.poseKarakter.SedangMemegang,"Menutup HP mengembalikan gerak dan pose Arthur");
        var voices=UnityEngine.Object.FindObjectsByType<SuaraDialogKertas>(FindObjectsSortMode.None);
        Check(voices.Length==4&&voices.Select(x=>x.profil.nada).Distinct().Count()==4,"Empat karakter mempunyai profil nada berbeda");
        foreach(var voice in voices)
        {
            int before=voice.JumlahBunyi;voice.Huruf('A');int first=voice.JumlahBunyi;voice.Huruf('B');voice.Huruf('C');
            Check(first==before+1&&voice.JumlahBunyi==first,"Bunyi huruf dibatasi ritme: "+voice.profil.namaKarakter);
            voice.Huruf('.');Check(voice.JumlahBunyi==first,"Tanda baca tidak membuat bunyi: "+voice.profil.namaKarakter);voice.Hentikan();Check(!voice.sumber.isPlaying,"Suara berhenti: "+voice.profil.namaKarakter);
        }
        var dialog=UnityEngine.Object.FindFirstObjectByType<DialogManager>();var pilihan=UnityEngine.Object.FindFirstObjectByType<SistemPilihan>(FindObjectsInactive.Include);
        dialog.idInteraksiAktif="NPC_Nara_Ekspresi";
        foreach(var speaker in new[]{Pembicara.Kiri_Arthur,Pembicara.Kanan_NPC})
        {
            var voice=speaker==Pembicara.Kiri_Arthur?player.GetComponent<SuaraDialogKertas>():GameObject.Find("NPC_Nara_Ekspresi").GetComponent<SuaraDialogKertas>();int count=voice.JumlahBunyi;
            dialog.skenarioAktif=new[]{new BarisDialog{siapaYangBicara=speaker,namaKarakter=voice.profil.namaKarakter,teksDialog="Abcdefghijklmnopqrstuvwxyz abcdefghijklmnopqrstuvwxyz",teksTombolLanjut="Lanjut"}};
            dialog.canvasDialog.SetActive(true);typeof(SistemPilihan).GetMethod("MulaiPercakapan",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(pilihan,null);yield return Wait(.35f);
            Check(voice.JumlahBunyi>count,"Mesin tik dialog memicu profil pembicara: "+voice.profil.namaKarakter);
            pilihan.AkhiriPercakapanVisual();dialog.TutupDialog();Check(!voice.sumber.isPlaying,"Menutup dialog menghentikan suara: "+voice.profil.namaKarakter);
        }
        Check(AssetDatabase.LoadAssetAtPath<PlaylistArthur>("Assets/AudioArthur/Playlist_Arthur.asset").lagu.All(l=>l.audio==null),"Asset playlist asli tetap kosong setelah fixture pengujian");
        UnityEngine.Object.Instantiate(music.gameObject);yield return null;yield return null;Check(UnityEngine.Object.FindObjectsByType<MusikArthur>(FindObjectsSortMode.None).Length==1,"Pengelola audio menolak duplikat saat pemuatan ulang scene");
    }
}
