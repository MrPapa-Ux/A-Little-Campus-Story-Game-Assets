// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Checkpoint 14; alat pengujian Editor, bukan komponen gameplay.
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

public static class CampusCheckpoint14Checks
{
    const string Key="Campus.Checkpoint14.Tests";
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    static bool started;
    static string Work(string name)=>Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",name);
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    public static void Begin()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Mulai dari Edit Mode.");
        EditorSceneManager.SaveOpenScenes();EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        File.WriteAllText(Work("checkpoint14-checks.txt"),"RUNNING\n");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
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
        File.WriteAllText(Work("checkpoint14-checks.txt"),(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));started=false;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;
    }
    static void Check(bool ok,string note){if(!ok)throw new Exception(note);results.Add("OK: "+note);}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
    static void Capture(string name){ScreenCapture.CaptureScreenshot(Work("checkpoint14-"+name+".png"));}
    static GameObject Hit(RectTransform r){Canvas.ForceUpdateCanvases();var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.position)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);return hits.Count==0?null:ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);}
    static void Click(Button b){Check(Hit((RectTransform)b.transform)==b.gameObject,"Tombol tidak terhalang: "+b.name);ExecuteEvents.Execute(b.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);}
    static Button Button(Transform t,string name)=>t.Find(name).GetComponent<Button>();
    static IEnumerator Footer(SistemHP h,AplikasiCatatanHP app,string source)
    {
        var sheet=app.transform.Find("BingkaiHP/Kertas");var icon=(RectTransform)sheet.Find("IkonNavigasi");var left=(RectTransform)sheet.Find("Kiri");var right=(RectTransform)sheet.Find("Kanan");
        Check(!sheet.Find("Pemilik").gameObject.activeSelf,"Tulisan pemilik/SMS diganti ikon: "+app.jenis);
        Check(icon.GetComponent<Image>().sprite==h.daftarIkon.First(x=>x.name==source).GetComponent<Image>().sprite,"Ikon footer sama dengan ikon HP: "+app.jenis);
        Check(Mathf.Abs(icon.anchoredPosition.x-(left.anchoredPosition.x+right.anchoredPosition.x)/2)<3,"Ikon tepat di tengah panah: "+app.jenis);
        Check(Mathf.Abs(app.jumlah.rectTransform.anchoredPosition.x-icon.anchoredPosition.x)<3,"Penghitung terpusat terhadap ikon: "+app.jenis);
        Check(app.jumlah.rectTransform.anchoredPosition.y-app.jumlah.rectTransform.rect.height/2>icon.anchoredPosition.y+icon.rect.height/2+5,"Penghitung tidak menempel ikon/footer: "+app.jenis);
        Check(left.sizeDelta==right.sizeDelta&&Mathf.Abs(left.anchoredPosition.y-right.anchoredPosition.y)<.1f,"Panah simetris dan sejajar: "+app.jenis);
        foreach(var t in new[]{app.jumlah,app.labelAksi}){t.ForceMeshUpdate();Check(t.font.name.Contains("Liberation")&&t.fontSize>=33&&t.textInfo.characterInfo.Take(t.textInfo.characterCount).Any(c=>c.isVisible),"Label terbaca dengan ukuran tetap: "+app.jenis+"/"+t.name);Check(!t.isTextOverflowing,"Label tidak terpotong: "+app.jenis+"/"+t.name);}
        Check(app.labelAksi.GetComponent<WigglyText>()?.isActiveAndEnabled==true,"Wiggly label aksi tetap berjalan: "+app.jenis);
        var pos=icon.anchoredPosition;var rot=icon.localRotation;yield return Wait(.3f);Check(Vector2.Distance(pos,icon.anchoredPosition)>.01f||Quaternion.Angle(rot,icon.localRotation)>.01f,"Ikon footer bergerak wiggly: "+app.jenis);
        foreach(string n in new[]{"Kiri","Kanan"})Check(Hit((RectTransform)sheet.Find(n))==sheet.Find(n).gameObject,"Ikon tidak menghalangi panah: "+app.jenis+"/"+n);
    }
    static IEnumerator Run()
    {
        Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();yield return Wait(.3f);
        var h=UnityEngine.Object.FindFirstObjectByType<SistemHP>();var buku=h.GetComponent<BukuArthur>();var demo=buku.GetComponent<ContohIsiTasHP>();
        var player=UnityEngine.Object.FindFirstObjectByType<KarakterController>();var lazy=player.GetComponent<LazyBarArthur>();if(lazy!=null)lazy.enabled=false;
        player.GetComponent<Rigidbody2D>().gravityScale=0;player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
        Check(demo!=null&&demo.aktifkanContoh&&demo.hanyaDiEditor&&buku.barang.Count==8,"Delapan item contoh muncul di Editor tanpa diberikan pada build Android");
        Check(demo.contoh.All(x=>x.ikon!=null&&AssetDatabase.GetAssetPath(x.ikon).StartsWith("Assets/ContohTasArthur/")),"Semua contoh memakai sprite barang dari folder proyek");
        Check(((TextureImporter)AssetImporter.GetAtPath("Assets/All Asset/Sprites/Icon/Pixel_Mart/banana.png")).textureType==TextureImporterType.Default,"Pengaturan aset download asli tidak diubah");
        Check(buku.pesan.Count==0&&buku.tugas.Count==0,"Contoh tas tidak mengisi pesan maupun tugas cerita");
        h.BukaHPDefault();yield return Wait(1f);h.BukaAplikasi("IkonPesan");yield return Wait(1.3f);yield return Footer(h,h.pesan,"IkonPesan");
        var sms=h.pesan;var sheet=sms.transform.Find("BingkaiHP/Kertas");var inbox=sheet.Find("Subjudul").GetComponent<TMP_Text>();
        Check(inbox.font.name.Contains("Liberation")&&inbox.fontSize>=38,"Kotak Masuk memakai huruf tebal yang jelas");
        var row=(RectTransform)sms.tombolBaris[0].transform;Check(inbox.rectTransform.anchoredPosition.y-inbox.rectTransform.rect.height/2>row.anchoredPosition.y+row.rect.height/2+5,"Kotak Masuk tidak menempel baris kontak pertama");
        var panel=(RectTransform)sheet.Find("LayarKontak");Check(sms.jumlah.rectTransform.anchoredPosition.y+sms.jumlah.rectTransform.rect.height/2<panel.anchoredPosition.y-panel.rect.height/2-15,"0 Pesan berada di bawah kotak kontak dengan jarak lega");
        Check(!sheet.Find("LayarPercakapan/FooterLayar").gameObject.activeSelf&&sms.statusKontak.text=="","Label SMS kecil yang berulang disembunyikan");
        Capture("pesan");yield return null;
        buku.Tambah(JenisBukuArthur.Pesan,new CatatanArthur{id="cek14",pengirim="Nara",judul="Pengingat",isi="Arthur, jangan lupa bawa hardiskmu ke kelas. Sampai ketemu!"});yield return Wait(.3f);Check(sms.jumlah.text=="1 / 1 PESAN"&&sms.detailIsi.text.Contains("hardisk"),"Pesan terisi dan penghitung tetap terhubung");Capture("pesan-isi-uji");yield return null;
        buku.pesan.Clear();buku.Segarkan();Click(Button(sheet,"KembaliKeHP"));yield return Wait(1f);
        h.BukaAplikasi("IkonTas");yield return Wait(1.3f);var bag=h.tas;yield return Footer(h,bag,"IkonTas");
        Check(bag.JumlahHalaman==2&&bag.tombolBaris.Count(b=>b.gameObject.activeSelf)==6,"Delapan contoh dibagi dalam dua halaman, enam kotak pada awal");
        var hint=bag.transform.Find("BingkaiHP/Kertas/BagianDalamTas/LabelKompartemen").GetComponent<TMP_Text>();Check(hint.font.name.Contains("Liberation")&&hint.fontSize>=36&&hint.color.r>.9f,"Petunjuk ritsleting besar dan terang");
        Click(bag.tombolBaris[1]);yield return Wait(.3f);Check(bag.pratinjauBarang.sprite==demo.contoh[1].ikon&&bag.detailJudul.text=="Plester","Memilih barang menampilkan gambar dan nama Plester");
        var preview=bag.pratinjauBarang.rectTransform;var pocket=(RectTransform)preview.parent;Check(Mathf.Abs(preview.anchoredPosition.y)+preview.rect.height/2<pocket.rect.height/2-5,"Pratinjau barang tetap di dalam saku kanan dan tidak menimpa aksi");
        Check(bag.labelBaris.All(x=>x.fontSize>=28),"Nama barang contoh diperbesar tanpa AutoSize");Capture("tas");yield return null;
        var zip=bag.ritsleting;var zr=(RectTransform)zip.transform;
        var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,zr.TransformPoint(new Vector3(zr.rect.xMax+300,0,0))),button=PointerEventData.InputButton.Left};
        var hits=new List<RaycastResult>();var hitPull=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,zip.tarikan.position)};EventSystem.current.RaycastAll(hitPull,hits);Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IDragHandler>(hits[0].gameObject)==zip.gameObject,"Kepala ritsleting dapat menerima seret nyata");
        ExecuteEvents.Execute(zip.gameObject,e,ExecuteEvents.beginDragHandler);ExecuteEvents.Execute(zip.gameObject,e,ExecuteEvents.dragHandler);ExecuteEvents.Execute(zip.gameObject,e,ExecuteEvents.endDragHandler);yield return Wait(.3f);
        Check(bag.Halaman==1&&bag.Pilihan==6&&bag.tombolBaris.Count(b=>b.gameObject.activeSelf)==2,"Ritsleting membuka halaman dua dan menyembunyikan empat kotak kosong");
        Check(bag.labelBaris.Skip(2).All(x=>x.text==""),"Tidak ada tulisan Kosong pada halaman tas");Capture("tas-halaman2");yield return null;
        bag.Kanan();bag.Kanan();Check(bag.Pilihan==7,"Panah kanan berhenti pada barang terakhir");
        e.scrollDelta=new Vector2(0,-5);ExecuteEvents.Execute(zip.gameObject,e,ExecuteEvents.scrollHandler);Check(bag.Halaman==1,"Scroll tidak melewati halaman terakhir");
        buku.barang.Clear();buku.Segarkan();yield return Wait(.3f);Check(bag.tombolBaris.All(x=>!x.gameObject.activeSelf)&&!bag.pratinjauBarang.gameObject.activeSelf,"Tas kosong tidak menampilkan kotak pengisi ataupun gambar sisa");
        Check(bag.jumlah.text=="0 BARANG"&&bag.Halaman==0&&bag.JumlahHalaman==1,"Menghapus isi mengembalikan jumlah dan batas halaman dengan benar");Capture("tas-kosong");yield return null;
        foreach(var item in demo.contoh)buku.Tambah(JenisBukuArthur.Tas,item);bag.GeserHalaman(0);Check(bag.tombolBaris.Count(x=>x.gameObject.activeSelf)==6,"Kotak muncul lagi ketika barang dimasukkan");
        Click(Button(bag.transform.Find("BingkaiHP/Kertas"),"KembaliKeHP"));yield return Wait(1f);
        h.BukaAplikasi("IkonToDoLIst");yield return Wait(1.3f);yield return Footer(h,h.tugas,"IkonToDoLIst");Capture("tugas");yield return null;
        h.TutupAplikasi();yield return Wait(1f);h.BukaAplikasi("IkonMap");yield return Wait(1.3f);
        var map=h.petaKampus;map.Interaksi();yield return Wait(.5f);Check(map.Zoom>2,"Peta masih dapat diperbesar");
        var overview=map.GetComponentsInChildren<Button>(true).First(x=>x.name=="Ikhtisar");var mapIcon=overview.transform.Find("IkonMapIkhtisar").GetComponent<Image>();
        Check(mapIcon.sprite==h.daftarIkon.First(x=>x.name=="IkonMap").GetComponent<Image>().sprite&&overview.GetComponent<Image>().color.a>0,"Ikon Map berada di dalam kotak tombol Semua yang lama");
        Check(overview.GetComponentsInChildren<TMP_Text>(true).All(x=>!x.gameObject.activeSelf),"Tulisan Semua diganti ikon Map");Click(overview);yield return Wait(.5f);Check(Mathf.Abs(map.Zoom-1)<.01f&&!map.SedangFokus,"Tombol ikon Map tetap memperlihatkan seluruh peta");Capture("peta");yield return null;
        h.TutupAplikasi();yield return Wait(1f);h.BukaPengaturan();yield return Wait(1.3f);Check(h.panelPengaturan.GetComponentsInChildren<Transform>(true).All(x=>x.name!="IkonNavigasi"),"Pengaturan tidak ditambah ikon aplikasi");
        h.TutupAplikasi();yield return Wait(1f);h.BukaAplikasi("IkonMusic");yield return Wait(1.3f);h.musik.Putar();yield return Wait(.2f);Check(MusikArthur.Instance.Memutar,"Music masih memainkan lagu tes setelah perubahan layout");h.musik.Putar();
        h.TutupAplikasi();yield return Wait(1f);h.TutupHP();yield return Wait(.5f);Check(!player.kunciPergerakan&&!h.poseKarakter.SedangMemegang,"Gerakan dan pose pemain pulih setelah HP ditutup");
    }
}

