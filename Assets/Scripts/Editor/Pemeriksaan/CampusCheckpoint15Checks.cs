// PANDUAN PROYEK - Editor / Pemeriksaan
// Fungsi: Memeriksa Checkpoint 15; alat pengujian Editor, bukan komponen gameplay.
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

public static class CampusCheckpoint15Checks
{
    const string Key="Campus.Checkpoint15.Tests";
    static readonly List<string> results=new List<string>(),errors=new List<string>();
    static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();
    static bool started;
    static string Work(string name)=>Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",name);
    [InitializeOnLoadMethod]static void Init(){EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
    public static void Begin()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Mulai dari Edit Mode.");
        EditorSceneManager.SaveOpenScenes();EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        File.WriteAllText(Work("checkpoint15-checks.txt"),"RUNNING\n");SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;
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
        File.WriteAllText(Work("checkpoint15-checks.txt"),(e==null?"PASS":"FAIL\n"+e)+"\n"+string.Join("\n",results));started=false;SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;
    }
    static void Check(bool ok,string note){if(!ok)throw new Exception(note);results.Add("OK: "+note);}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
    static void Capture(string name){ScreenCapture.CaptureScreenshot(Work("checkpoint15-"+name+".png"));}
    static GameObject Hit(RectTransform r){Canvas.ForceUpdateCanvases();var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.position)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);return hits.Count==0?null:ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);}
    static void Click(Button b){Check(Hit((RectTransform)b.transform)==b.gameObject,"Tombol tidak terhalang: "+b.name);ExecuteEvents.Execute(b.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);}
    static Button Button(Transform t,string name)=>t.Find(name).GetComponent<Button>();

    static IEnumerator Typography(AplikasiCatatanHP app)
    {
        var sheet=app.transform.Find("BingkaiHP/Kertas");
        foreach(var t in new[]{app.jumlah,app.labelAksi})
        {
            t.ForceMeshUpdate();Check(t.font.name.Contains("Amatic")&&!t.enableAutoSizing,"Font khas game dipakai tanpa AutoSize: "+app.jenis+"/"+t.name);
            Check(!t.isTextOverflowing&&t.textInfo.characterInfo.Take(t.textInfo.characterCount).Any(c=>c.isVisible),"Tulisan tetap terlihat lengkap: "+app.jenis+"/"+t.name);
        }
        var icon=(RectTransform)sheet.Find("IkonNavigasi");Check(Mathf.Abs(app.jumlah.rectTransform.anchoredPosition.x-icon.anchoredPosition.x)<3,"Penghitung tetap sejajar dengan ikon: "+app.jenis);
        float bottom=app.jumlah.rectTransform.TransformPoint(new Vector3(0,app.jumlah.rectTransform.rect.yMin,0)).y;float iconTop=icon.TransformPoint(new Vector3(0,icon.rect.yMax,0)).y;Check(bottom>iconTop+3,"Bidang penghitung mempunyai jarak aman dari ikon: "+app.jenis);
        Check(app.labelAksi.GetComponent<WigglyText>()?.isActiveAndEnabled==true,"Wiggly tetap aktif pada aksi: "+app.jenis);
        foreach(string n in new[]{"Kiri","Kanan"}){var arrow=sheet.Find(n).GetComponentInChildren<TMP_Text>(true);arrow.ForceMeshUpdate();Check(arrow.font.name.Contains("Amatic"),"Font panah serasi: "+app.jenis+"/"+n);Check(!arrow.isTextOverflowing&&arrow.textInfo.characterInfo.Take(arrow.textInfo.characterCount).Any(c=>c.isVisible),"Simbol panah terlihat utuh: "+app.jenis+"/"+n);}
        yield return Wait(.2f);
    }
    static IEnumerator Run()
    {
        Application.runInBackground=true;EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();yield return Wait(.3f);
        var h=UnityEngine.Object.FindFirstObjectByType<SistemHP>();var buku=h.GetComponent<BukuArthur>();var demo=buku.GetComponent<ContohIsiTasHP>();var bag=h.tas;
        var player=UnityEngine.Object.FindFirstObjectByType<KarakterController>();var lazy=player.GetComponent<LazyBarArthur>();if(lazy!=null)lazy.enabled=false;player.GetComponent<Rigidbody2D>().gravityScale=0;player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
        Check(buku.barang.Count==8&&demo.hanyaDiEditor,"Delapan contoh tetap tersedia di Editor untuk mencoba ritsleting");
        h.BukaHPDefault();yield return Wait(1f);h.BukaAplikasi("IkonTas");yield return Wait(1.3f);yield return Typography(bag);
        Check(bag.tombolBaris.Length==1&&bag.labelBaris.Length==1&&bag.ikonBaris.Length==1&&bag.JumlahHalaman==8,"Satu barang per tampilan, delapan barang berarti delapan halaman");
        var rows=bag.transform.Find("BingkaiHP/Kertas").Cast<Transform>().Where(t=>t.name.StartsWith("Catatan_")).ToArray();Check(rows.Count(t=>t.gameObject.activeSelf)==1,"Hanya satu objek barang terlihat, slot lama disembunyikan");
        Check(bag.latarBaris[0].color.a==0&&bag.latarBaris[0].GetComponent<Outline>()==null,"Item tidak mempunyai latar kotak atau outline persegi");
        var hint=bag.transform.Find("BingkaiHP/Kertas/BagianDalamTas/LabelKompartemen").GetComponent<TMP_Text>();hint.ForceMeshUpdate();Check(hint.font.name.Contains("Amatic")&&hint.fontSize>=48&&!hint.isTextOverflowing,"Petunjuk ritsleting memakai Amatic besar dan tidak terpotong");
        Check(hint.text.Contains("PINDAH BARANG"),"Petunjuk sesuai fungsi baru berpindah satu barang");
        Click(bag.tombolBaris[0]);yield return Wait(.25f);Check(bag.detailJudul.text=="Pisang"&&bag.ikonBaris[0].sprite==demo.contoh[0].ikon,"Barang pertama beserta gambar dan detailnya benar");Capture("tas");yield return null;
        bag.Kanan();yield return Wait(.25f);Check(bag.Pilihan==1&&bag.Halaman==1&&bag.detailJudul.text=="Plester"&&bag.jumlah.text=="2 / 8 BARANG","Panah berpindah ke barang berikut, penghitung dan detail mengikuti");
        Check(bag.ikonBaris[0].sprite==demo.contoh[1].ikon&&bag.pratinjauBarang.sprite==demo.contoh[1].ikon,"Gambar utama dan pratinjau berpindah bersama");Capture("tas-barang2");yield return null;
        var zip=bag.ritsleting;var r=(RectTransform)zip.transform;
        var hits=new List<RaycastResult>();var hitPull=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,zip.tarikan.position)};EventSystem.current.RaycastAll(hitPull,hits);Check(hits.Count>0&&ExecuteEvents.GetEventHandler<IDragHandler>(hits[0].gameObject)==zip.gameObject,"Ritsleting tidak terhalang tampilan item besar");
        var e=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(new Vector3(Mathf.Lerp(r.rect.xMin+zip.margin,r.rect.xMax-zip.margin,.62f),0,0))),button=PointerEventData.InputButton.Left};
        ExecuteEvents.Execute(zip.gameObject,e,ExecuteEvents.beginDragHandler);ExecuteEvents.Execute(zip.gameObject,e,ExecuteEvents.dragHandler);ExecuteEvents.Execute(zip.gameObject,e,ExecuteEvents.endDragHandler);yield return Wait(.25f);Check(bag.Pilihan==4&&bag.detailJudul.text=="Keripik","Seret ritsleting di tengah memilih barang sesuai posisinya");
        e.position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(new Vector3(r.rect.xMax+500,0,0)));ExecuteEvents.Execute(zip.gameObject,e,ExecuteEvents.dragHandler);yield return Wait(.2f);Check(bag.Pilihan==7&&bag.detailJudul.text=="Struk","Seret ke ujung berhenti di barang kedelapan");bag.Kanan();Check(bag.Pilihan==7,"Panah tidak melewati barang terakhir");
        e.scrollDelta=new Vector2(0,-8);ExecuteEvents.Execute(zip.gameObject,e,ExecuteEvents.scrollHandler);Check(bag.Pilihan==7,"Scroll tidak melingkar dari akhir ke awal");Capture("tas-barang8");yield return null;
        e.position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(new Vector3(r.rect.xMin-500,0,0)));ExecuteEvents.Execute(zip.gameObject,e,ExecuteEvents.dragHandler);bag.Kiri();Check(bag.Pilihan==0&&bag.detailJudul.text=="Pisang","Ujung kiri berhenti pada barang pertama");
        bag.GeserHalaman(7);buku.barang.RemoveRange(2,6);buku.Segarkan();Check(bag.Pilihan==1&&bag.JumlahHalaman==2&&bag.detailJudul.text=="Plester","Menghapus barang menyesuaikan batas dan detail ke dua barang tersisa");
        buku.barang.Clear();buku.Segarkan();yield return Wait(.25f);Check(bag.JumlahHalaman==1&&bag.Pilihan==0&&!bag.tombolBaris[0].gameObject.activeSelf&&!bag.pratinjauBarang.gameObject.activeSelf,"Tas kosong tidak menampilkan gambar atau kotak sisa");Capture("tas-kosong");yield return null;
        buku.Tambah(JenisBukuArthur.Tas,demo.contoh[0]);bag.GeserHalaman(50);Check(bag.JumlahHalaman==1&&bag.Pilihan==0,"Jika total hanya satu barang, ritsleting tetap dibatasi");
        h.TutupAplikasi();yield return Wait(1f);h.BukaAplikasi("IkonPesan");yield return Wait(1.3f);yield return Typography(h.pesan);var inbox=h.pesan.transform.Find("BingkaiHP/Kertas/Subjudul").GetComponent<TMP_Text>();inbox.ForceMeshUpdate();Check(inbox.font.name.Contains("Amatic")&&!inbox.isTextOverflowing,"Kotak Masuk kembali ke font game dan terlihat utuh");Capture("pesan");yield return null;
        buku.Tambah(JenisBukuArthur.Pesan,new CatatanArthur{id="cek15",judul="Kelas",pengirim="Nara",isi="Arthur, jangan lupa bawa hardiskmu."});h.pesan.Kanan();Check(h.pesan.detailJudul.text=="Nara"&&h.pesan.jumlah.text=="1 / 1 PESAN","Pesan dan penghitung masih terhubung dengan data");
        h.TutupAplikasi();yield return Wait(1f);h.BukaAplikasi("IkonToDoLIst");yield return Wait(1.3f);yield return Typography(h.tugas);Capture("tugas");yield return null;
        buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{id="cek15",judul="Bawa hardisk",isi="Cek tas sebelum berangkat."});h.tugas.Interaksi();Check(buku.tugas[0].selesai&&h.tugas.jumlah.text=="1 / 1 CATATAN","Aksi dan penghitung To-do tetap bekerja");
        h.TutupAplikasi();yield return Wait(1f);h.TutupHP();yield return Wait(.5f);Check(!player.kunciPergerakan,"Menutup HP mengembalikan gerakan pemain");
    }
}
