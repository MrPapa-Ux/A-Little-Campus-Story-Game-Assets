// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Checkpoint 13; alat pengujian Editor, bukan komponen gameplay.
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

public static class CampusCheckpoint13Checks
{
    const string Key="Campus.Checkpoint13.Tests";
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
        File.WriteAllText(Work("checkpoint13-checks.txt"),"RUNNING\n");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
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
        File.WriteAllText(Work("checkpoint13-checks.txt"),(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));
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
    static void Capture(string name){ScreenCapture.CaptureScreenshot(Work("checkpoint13-"+name+".png"));}
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
        Check(music.playlist.lagu.Length==5 && music.playlist.lagu[0].audio!=null && music.playlist.lagu.Skip(1).All(l=>l.audio==null),"Satu lagu tes terpasang, empat slot tersisa kosong");
        Check(!music.sumber.isPlaying && !music.tema.isPlaying,"Tidak ada musik atau tema lokasi diputar otomatis");
        Check(UnityEngine.Object.FindObjectsByType<PemutarMusikUI>(FindObjectsInactive.Include,FindObjectsSortMode.None).All(x=>x.enabled && !x.GetComponent<AudioSource>().playOnAwake && !x.GetComponent<AudioSource>().isPlaying),"Notifikasi lama aktif sebagai UI tanpa pemutar audio ganda");
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
        yield return Structural(hp);Capture("phone");yield return null;
        Click(hp.daftarIkon.First(x=>x.name=="IkonMusic"));yield return Wait(1.3f);
        Check(ui.gameObject.activeInHierarchy&&!hp.hpUtama.activeSelf,"Music terbuka dalam panel landscape dan portrait disembunyikan");
        Check(ui.transform.Find("BingkaiHP/Kertas/Subjudul")==null,"Slogan Lima Lagu Satu Kaset sudah dihapus");
        Check(ui.pita.GetComponent<UnityEngine.UI.Image>().color.r>.9f && ui.pita.GetComponent<UnityEngine.UI.Image>().color.b<.1f,"Mixtape Arthur berwarna oranye");
        foreach(var text in new[]{ui.labelPutar,ui.labelAcak,ui.labelUlang,ui.transform.Find("BingkaiHP/Kertas/VolumeLabel").GetComponent<TMPro.TMP_Text>()})Check(text.GetComponent<WigglyText>()?.isActiveAndEnabled==true,"Wiggly aktif: "+text.transform.parent.name+"/"+text.name);
        int idleSteps=ui.coretan.JumlahLangkah;yield return Wait(.3f);Check(ui.coretan.JumlahLangkah==idleSteps,"Coretan kaset diam ketika tidak ada lagu diputar");
        Check(ui.namaSlot[0].text.Contains("Heartbeat") && ui.namaSlot.Skip(1).All(x=>x.text.Contains("belum diisi")),"Lagu tes ditampilkan di slot pertama");
        ui.judulLagu.ForceMeshUpdate();Check(ui.judulLagu.textInfo.characterInfo.Take(ui.judulLagu.textInfo.characterCount).Any(x=>x.isVisible),"Judul lagu terlihat dan tidak tersembunyi oleh tinggi kotak teks");
        var tapeArt=ui.GetComponentInChildren<KasetCoretanUI>();Check(tapeArt.GetComponent<CanvasRenderer>()!=null,"Gulungan kaset memiliki CanvasRenderer");
        Click(ui.transform.Find("BingkaiHP/Kertas/Putar").gameObject);Check(music.Memutar && music.sumber.clip==music.playlist.lagu[0].audio,"Tombol Putar benar-benar memainkan lagu proyek");
        yield return Notification(music);ui.Putar();yield return null;
        Click(ui.transform.Find("BingkaiHP/Kertas/Acak").gameObject);Check(music.acak,"Tombol acak terhubung");
        Click(ui.transform.Find("BingkaiHP/Kertas/Ulang").gameObject);Check(music.ulang,"Tombol ulang terhubung");
        ui.volume.value=.37f;Check(Mathf.Abs(music.Volume-.37f)<.001f,"Slider volume mengatur sumber musik");
        ui.Putar();yield return Wait(.3f);Capture("music");yield return null;ui.Putar();
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
        yield return Wait(.2f);Capture("notifikasi");yield return null;var visibleToast=UnityEngine.Object.FindFirstObjectByType<PemutarMusikUI>();Check(visibleToast.GetComponent<CanvasGroup>().alpha>0,"Notifikasi muncul setelah kembali ke permainan");var toastRot=visibleToast.ikonMusik.localRotation;yield return Wait(.25f);Check(Quaternion.Angle(toastRot,visibleToast.ikonMusik.localRotation)>1,"Ikon notifikasi berputar mengikuti lagu saat terlihat di permainan");music.PutarJeda();yield return null;toastRot=visibleToast.ikonMusik.localRotation;yield return Wait(.25f);Check(Quaternion.Angle(toastRot,visibleToast.ikonMusik.localRotation)<.01f,"Ikon notifikasi terlihat ikut berhenti saat dijeda");music.PutarJeda();
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
            if(app.jenis==JenisBukuArthur.Pesan)Check(app.detailIsi.text=="","Pesan kosong tidak berisi teks penjelasan pengisi");
            if(app.jenis==JenisBukuArthur.Tas){app.GeserHalaman(999);app.Kiri();Check(app.Halaman==0&&app.Pilihan==0&&app.JumlahHalaman==1,"Tas kosong tidak dapat maju maupun mundur melewati batas");}
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
                Check(app.pratinjauBarang.gameObject.activeInHierarchy && app.pratinjauBarang.sprite==sprite,"Pratinjau besar menampilkan sprite item terpilih");
                Capture("tas-isi-uji");yield return null;yield return Zipper(app,buku);
            }
            if(app.jenis==JenisBukuArthur.Pesan)
            {
                var sprite=player.GetComponentInChildren<EkspresiWajah2D>().kepala.sprite;
                buku.pesan[0].pengirim="Kontak Uji";buku.pesan[0].ikon=sprite;buku.pesan[1].pengirim="Dirahasiakan";buku.pesan[1].anonim=true;buku.pesan[1].ikon=sprite;
                app.Kanan();buku.Segarkan();Check(app.detailJudul.text=="Kontak Uji" && app.fotoKontak.gameObject.activeInHierarchy && !app.avatarKontak.gameObject.activeInHierarchy,"Kontak bernama menampilkan foto sendiri");
                app.Kanan();Check(app.detailJudul.text=="Anonim" && !app.fotoKontak.gameObject.activeInHierarchy && app.avatarKontak.gameObject.activeInHierarchy,"Kontak anonim menyembunyikan nama dan foto dengan avatar kepala-bahu");
                Check(app.ikonBaris[0].gameObject.activeInHierarchy && app.avatarBaris[1].gameObject.activeInHierarchy,"Avatar kontak juga dibedakan pada daftar kiri");
                yield return Wait(.3f);Capture("pesan-isi-uji");yield return null;
                buku.pesan[1].isi=string.Join("\n",Enumerable.Repeat("Pesan panjang harus dapat digulir tanpa mengecilkan huruf.",14));buku.Segarkan();yield return Wait(.2f);
                var scroll=app.detailIsi.GetComponentInParent<UnityEngine.UI.ScrollRect>();Check(scroll!=null && scroll.content.rect.height>scroll.viewport.rect.height,"Pesan panjang memiliki area gulir dengan huruf tetap besar");scroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();Check(scroll.content.anchoredPosition.y>0,"Isi pesan dapat digulir sampai bawah");app.Kanan();yield return null;Check(Mathf.Abs(scroll.content.anchoredPosition.y)<1,"Kontak berikutnya selalu dibuka dari awal pesannya");
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
        Check(AssetDatabase.LoadAssetAtPath<PlaylistArthur>("Assets/AudioArthur/Playlist_Arthur.asset").lagu[0].audio!=null && AssetDatabase.LoadAssetAtPath<PlaylistArthur>("Assets/AudioArthur/Playlist_Arthur.asset").lagu.Skip(1).All(l=>l.audio==null),"Playlist asli tetap satu lagu tes sesudah fixture pengujian");
        UnityEngine.Object.Instantiate(music.gameObject);yield return null;yield return null;Check(UnityEngine.Object.FindObjectsByType<MusikArthur>(FindObjectsSortMode.None).Length==1,"Pengelola audio menolak duplikat saat pemuatan ulang scene");
    }
    static IEnumerator Notification(MusikArthur music)
    {
        var toast=UnityEngine.Object.FindFirstObjectByType<PemutarMusikUI>();yield return null;
        Check(toast!=null&&toast.pemutar==music&&toast.judul.text.Contains("Heartbeat"),"Notifikasi lagu membaca judul dari Music Arthur");
        Check(!toast.GetComponent<AudioSource>().isPlaying&&music.Memutar,"Hanya sumber Music memainkan lagu tes");
        var circle=(RectTransform)toast.transform.Find("LatarBulat");Check(toast.ikonMusik.parent==circle&&circle.anchoredPosition.x+circle.rect.width/2<toast.judul.rectTransform.anchoredPosition.x-toast.judul.rectTransform.rect.width/2,"Not berputar di dalam lingkaran tanpa menimpa judul lagu");
        var rot=toast.ikonMusik.localRotation;yield return Wait(.25f);Check(Quaternion.Angle(rot,toast.ikonMusik.localRotation)<.01f&&toast.GetComponent<CanvasGroup>().alpha==0,"Notifikasi disembunyikan dan tidak beranimasi di belakang aplikasi");
        music.PutarJeda();yield return null;rot=toast.ikonMusik.localRotation;yield return Wait(.25f);Check(Quaternion.Angle(rot,toast.ikonMusik.localRotation)<.01f,"Notifikasi berhenti berputar saat lagu dijeda");music.PutarJeda();
    }
    static IEnumerator Structural(SistemHP h)
    {
        foreach(var icon in h.daftarIkon){Check(icon.transform.parent==h.hpUtama.transform&&icon.transform.GetSiblingIndex()<h.layarGelap.transform.GetSiblingIndex(),"Ikon berada di dalam HP dan di bawah lapisan layar: "+icon.name);Check(((RectTransform)icon.transform).sizeDelta==new Vector2(78,78),"Ukuran kotak ikon seragam: "+icon.name);}
        var note=(RectTransform)h.daftarIkon.First(x=>x.name=="IkonMusic").transform.Find("GambarNot");Check(note.sizeDelta==new Vector2(70,70)&&Mathf.Abs(note.anchoredPosition.x)<4&&Mathf.Abs(note.anchoredPosition.y)<4,"Sprite Music dan wiggly tetap di dalam kotak ikon");
        var reference=h.panelPengaturan.transform.Find("Box_Pengaturan/Btn_TutupX").GetComponent<UnityEngine.UI.Image>();
        foreach(var root in new[]{h.musik.gameObject,h.tugas.gameObject,h.pesan.gameObject,h.tas.gameObject,h.petaKampus.gameObject,h.panelSegeraHadir}){var b=root.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(x=>x.name=="KembaliKeHP"||x.name=="Btn_KembaliPonsel");var im=b.GetComponent<UnityEngine.UI.Image>();Check(im.sprite==reference.sprite&&im.type==reference.type&&im.color==reference.color&&b.GetComponentInChildren<TMPro.TMP_Text>(true).font==reference.GetComponentInChildren<TMPro.TMP_Text>(true).font,"Gaya X serasi dengan Pengaturan: "+root.name);}
        foreach(var app in new[]{h.tas,h.tugas,h.pesan})
        {
            Check(app.jumlah.font.name.Contains("Liberation")&&app.jumlah.fontSize>=34&&!app.jumlah.text.StartsWith("00"),"Angka jumlah memakai font jelas tanpa nol ganda: "+app.jenis);
            foreach(var t in new[]{app.detailJudul,app.labelAksi,app.transform.Find("BingkaiHP/Kertas/Pemilik").GetComponent<TMPro.TMP_Text>()})Check(t.GetComponent<WigglyText>()!=null&&t.GetComponent<WigglyText>().kekuatanGoyang>=2,"Wiggly terpasang pada "+app.jenis+" / "+t.name);
        }
        Check(h.tugas.transform.Find("BingkaiHP/Kertas/Subjudul")==null,"Halaman Rencana dihapus");
        Check(h.tas.transform.Find("BingkaiHP/Kertas/KertasDetail/LabelSaku")==null&&h.tas.transform.Find("BingkaiHP/Kertas/KertasDetail/SakuDepan/TulisanSaku")==null,"Label Isi Slot dan Simpan Baik-baik dihapus");
        Check(h.tas.GetComponentInChildren<BentukTasUI>(true)!=null,"Tas mempunyai siluet ransel dengan gagang");
        Check(h.pesan.detailIsi.fontSize>=35&&h.pesan.transform.Find("BingkaiHP/Kertas/Subjudul").GetComponent<TMPro.TMP_Text>().fontSize>=44,"Isi pesan dan Kotak Masuk diperbesar");
        // Wiggly is verified on actual vertices, not merely the component's presence.
        h.BukaAplikasi("IkonToDoLIst");yield return Wait(1.3f);
        var label=h.tugas.detailJudul;label.ForceMeshUpdate();yield return Wait(.16f);var v=label.textInfo.meshInfo[0].vertices.ToArray();yield return Wait(.24f);Check(!v.SequenceEqual(label.textInfo.meshInfo[0].vertices),"Judul To-do benar-benar berubah vertex saat wiggly");
        h.TutupAplikasi();yield return Wait(1f);h.BukaAplikasi("IkonPesan");yield return Wait(1.3f);
        var signal=(RectTransform)h.pesan.transform.Find("BingkaiHP/Kertas/BatangSinyal");var pos=signal.anchoredPosition;var rot=signal.localRotation;yield return Wait(.3f);Check(Vector2.Distance(pos,signal.anchoredPosition)>.01f||Quaternion.Angle(rot,signal.localRotation)>.01f,"Batang sinyal Pesan benar-benar bergerak wiggly");
        h.TutupAplikasi();yield return Wait(1f);
    }
    static IEnumerator Zipper(AplikasiCatatanHP app,BukuArthur buku)
    {
        var zip=app.ritsleting;Check(zip!=null&&zip.tarikan!=null,"Ritsleting interaktif terhubung ke tas");
        foreach(int count in new[]{1,6,7,10,13})
        {
            while(buku.barang.Count>count)buku.barang.RemoveAt(buku.barang.Count-1);
            while(buku.barang.Count<count){int n=buku.barang.Count;buku.Tambah(JenisBukuArthur.Tas,new CatatanArthur{id="zip_"+n,judul="Item uji "+n,isi="Hanya data uji runtime."});}buku.Segarkan();
            int pages=Mathf.Max(1,Mathf.CeilToInt(count/6f));Check(app.JumlahHalaman==pages,"Jumlah halaman sesuai "+count+" barang");
            var rect=(RectTransform)zip.transform;
            var eventData=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector3(rect.rect.xMax+500,0,0))),button=PointerEventData.InputButton.Left};
            ExecuteEvents.Execute(zip.gameObject,eventData,ExecuteEvents.beginDragHandler);ExecuteEvents.Execute(zip.gameObject,eventData,ExecuteEvents.dragHandler);ExecuteEvents.Execute(zip.gameObject,eventData,ExecuteEvents.endDragHandler);
            Check(app.Halaman==pages-1,"Tarik melewati ujung berhenti pada halaman terakhir: "+count);
            eventData.scrollDelta=new Vector2(0,-8);ExecuteEvents.Execute(zip.gameObject,eventData,ExecuteEvents.scrollHandler);Check(app.Halaman==pages-1,"Scroll melewati batas tidak memutar halaman: "+count);
            eventData.position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector3(rect.rect.xMin-500,0,0)));ExecuteEvents.Execute(zip.gameObject,eventData,ExecuteEvents.dragHandler);Check(app.Halaman==0,"Tarik ke awal kembali halaman pertama: "+count);
            app.Kiri();Check(app.Pilihan==0,"Panah kiri berhenti di item pertama: "+count);
            for(int i=0;i<count+2;i++)app.Kanan();Check(app.Pilihan==count-1,"Panah kanan berhenti di item terakhir: "+count);
            yield return null;
        }
    }

}
