using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CampusRuteKampusChecks
{
    const string Dir = "work/rute-kampus/";
    const string Key = "Campus.RuteKampus.Test";
    static readonly Stack<IEnumerator> stack = new Stack<IEnumerator>();
    static double tunggu;
    static KarakterController arthur;
    static ArthurJalurKampus jalur;
    static SistemPilihan pilihan;
    static CampusRuteKampusChecks() { EditorApplication.update += Tick; }
    static bool HanyaGK2 => SessionState.GetBool(Key + ".hanyaGK2", false);
    static bool HanyaPulang => SessionState.GetBool(Key + ".hanyaPulang", false);
    static bool HanyaAkses => SessionState.GetBool(Key + ".hanyaAkses", false);
    static bool HanyaPenanda => SessionState.GetBool(Key + ".hanyaPenanda", false);
    static bool HanyaTeras => SessionState.GetBool(Key + ".hanyaTeras", false);
    static bool HanyaBelok => SessionState.GetBool(Key + ".hanyaBelok", false);
    static bool HanyaBatas => SessionState.GetBool(Key + ".hanyaBatas", false);
    static bool HanyaKameraBelakang => SessionState.GetBool(Key + ".hanyaKameraBelakang", false);
    static bool HanyaLantaiMerah => SessionState.GetBool(Key + ".hanyaLantaiMerah", false);
    static bool HanyaLagu => SessionState.GetBool(Key + ".hanyaLagu", false);
    static bool HanyaEmosional => SessionState.GetBool(Key + ".hanyaEmosional", false);
    static bool HanyaKantin => SessionState.GetBool(Key + ".hanyaKantin", false);
    static bool HanyaPeta => SessionState.GetBool(Key + ".hanyaPeta", false);
    static bool HanyaGH => SessionState.GetBool(Key + ".hanyaGH", false);
    static bool HanyaLariGH => SessionState.GetBool(Key + ".lariGH", false);
    static bool HanyaBob => SessionState.GetBool(Key + ".bob", false);
    static bool HanyaGarisPagar => SessionState.GetBool(Key + ".garisPagar", false);
    static string Laporan => HanyaGarisPagar ? "pengujian-garis-pagar.txt" : HanyaBob ? "pengujian-bob-panah.txt" : HanyaGH ? "pengujian-GH.txt" : HanyaPeta ? "pengujian-peta-20261025.txt" : HanyaKantin ? "pengujian-kantin.txt" : HanyaEmosional ? "pengujian-musik-emosional.txt" : HanyaLagu ? "pengujian-musik-neon.txt" : HanyaGK2 ? "pengujian-gk2-wide.txt" : "pengujian.txt";
    static void Log(string s) => File.AppendAllText(Dir + Laporan, s + "\n");
    static void Check(bool ok, string s) { if (!ok) throw new Exception(s); Log("PASS: " + s); }
    static void Tick()
    {
        if (AssetDatabase.IsAssetImportWorkerProcess() || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        try
        {
            if (File.Exists(Dir + "uji.request") || File.Exists(Dir + "uji-gk2.request"))
            {
                // Penulis berkas permintaan bisa masih memegang file sesaat.
                // Baca sekali setelah tersedia, lalu pilih mode secara konsisten.
                string permintaanGK2 = null;
                try { if (File.Exists(Dir + "uji-gk2.request")) permintaanGK2 = File.ReadAllText(Dir + "uji-gk2.request"); }
                catch (IOException) { return; }
                SessionState.SetBool(Key + ".hanyaGK2", permintaanGK2 != null);
                SessionState.SetBool(Key + ".hanyaPulang", permintaanGK2 != null && permintaanGK2.Contains("pulang saja"));
                SessionState.SetBool(Key + ".hanyaAkses", permintaanGK2 != null && permintaanGK2.Contains("akses jasmine"));
                SessionState.SetBool(Key + ".hanyaPenanda", permintaanGK2 != null && permintaanGK2.Contains("penanda kepala"));
                SessionState.SetBool(Key + ".hanyaTeras", permintaanGK2 != null && permintaanGK2.Contains("teras atas"));
                SessionState.SetBool(Key + ".hanyaBelok", permintaanGK2 != null && permintaanGK2.Contains("belokan tersambung"));
                SessionState.SetBool(Key + ".hanyaBatas", permintaanGK2 != null && permintaanGK2.Contains("batas GA"));
                SessionState.SetBool(Key + ".hanyaKameraBelakang", permintaanGK2 != null && permintaanGK2.Contains("batas kamera belakang"));
                SessionState.SetBool(Key + ".hanyaLantaiMerah", permintaanGK2 != null && permintaanGK2.Contains("lantai merah"));
                SessionState.SetBool(Key + ".hanyaLagu", permintaanGK2 != null && permintaanGK2.Contains("lagu saja"));
                SessionState.SetBool(Key + ".hanyaEmosional", permintaanGK2 != null && permintaanGK2.Contains("lagu emosional"));
                SessionState.SetBool(Key + ".hanyaKantin", permintaanGK2 != null && permintaanGK2.Contains("kantin merah"));
                SessionState.SetBool(Key + ".hanyaPeta", permintaanGK2 != null && permintaanGK2.Contains("peta 20261025"));
                SessionState.SetBool(Key + ".hanyaGH", permintaanGK2 != null && permintaanGK2.Contains("guest house"));
                SessionState.SetBool(Key + ".lariGH", permintaanGK2 != null && permintaanGK2.Contains("lari GH"));
                SessionState.SetBool(Key + ".bob", permintaanGK2 != null && permintaanGK2.Contains("bob lari"));
                SessionState.SetBool(Key + ".garisPagar", permintaanGK2 != null && permintaanGK2.Contains("minor garis pagar"));
                SessionState.SetBool(Key + ".selesai", false);
                if (File.Exists(Dir + "uji.request")) File.Delete(Dir + "uji.request");
                if (File.Exists(Dir + "uji-gk2.request")) File.Delete(Dir + "uji-gk2.request");
                if (EditorApplication.isPlayingOrWillChangePlaymode || SceneManager.GetActiveScene().isDirty) throw new Exception("Pengujian perlu Edit Mode dengan scene tersimpan.");
                if (SceneManager.GetActiveScene().path != "Assets/Scenes/CharacterLab.unity") throw new Exception("Pengujian hanya CharacterLab.");
                SessionState.SetBool(Key + ".mute", EditorUtility.audioMasterMute);
                SessionState.SetBool(Key + ".background", Application.runInBackground);
                EditorUtility.audioMasterMute = true;
                File.WriteAllText(Dir + Laporan, "Uji perjalanan CharacterLab\n");
                File.WriteAllText(Dir + "running.flag", "running");
                EditorApplication.isPlaying = true;
                return;
            }
            if (!File.Exists(Dir + "running.flag")) return;
            if (!EditorApplication.isPlaying)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
                EditorUtility.audioMasterMute = SessionState.GetBool(Key + ".mute", false);
                Application.runInBackground = SessionState.GetBool(Key + ".background", false);
                JedaSaatTidakAktif.PengujianLatarBelakang = false;
                File.Delete(Dir + "running.flag"); stack.Clear();
                Log("SELESAI: Play mati; mute dan pengaturan latar belakang dipulihkan.");
                return;
            }
            JedaSaatTidakAktif.PengujianLatarBelakang = true;
            if(SessionState.GetBool(Key + ".selesai",false))return;
            Application.runInBackground = true; EditorApplication.isPaused = false;
            var jeda = UnityEngine.Object.FindFirstObjectByType<JedaSaatTidakAktif>(); if (jeda != null) jeda.AturJeda(false);
            if (Time.timeScale == 0) Time.timeScale = 1;
            if (Time.timeSinceLevelLoad < .8f) return;
            if (stack.Count == 0) { stack.Push(Jalankan()); tunggu = 0; }
            if (EditorApplication.timeSinceStartup < tunggu) return;
            var e = stack.Peek();
            if (!e.MoveNext()) { stack.Pop(); if (stack.Count == 0) {SessionState.SetBool(Key + ".selesai",true);EditorApplication.isPlaying = false;} return; }
            if (e.Current is IEnumerator nested) stack.Push(nested);
            else if (e.Current is float delay) tunggu = EditorApplication.timeSinceStartup + delay;
        }
        catch (Exception e)
        {
            Log("GAGAL: " + e);
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            stack.Clear();
        }
    }
    static IEnumerator Tunggu(Func<bool> kondisi, float detik, string pesan)
    {
        float selesai = Time.realtimeSinceStartup + detik;
        while (!kondisi()) { if (Time.realtimeSinceStartup > selesai) throw new Exception("Batas tunggu: " + pesan + ", posisi=" + arthur.transform.position + ", jarak=" + jalur.JarakDiJalur); yield return null; }
    }
    static Transform Titik(string name) => GameObject.Find("RuteKampus_CharacterLab/" + name).transform;
    static IEnumerator TahanArah(float arah, float detik)
    {
        float akhir = Time.realtimeSinceStartup + detik;
        while (Time.realtimeSinceStartup < akhir) { if (arah > 0) arthur.JalanKeKanan(); else arthur.JalanKeKiri(); yield return null; }
        arthur.BerhentiBergerak();
    }
    static IEnumerator Pilih(string id, int indeks, string target)
    {
        yield return Tunggu(() => DialogManager.Instance.idInteraksiAktif == id, 3, "Interaksi " + id);
        pilihan.AksiTombolInteraksi(); yield return .35f;
        Check(DialogManager.Instance.PercakapanAktif, "Dialog " + id + " terbuka melalui tombol interaksi");
        pilihan.TekanPilihanLangsung(indeks); yield return 1.4f;
        Check(!DialogManager.Instance.PercakapanAktif && !pilihan.efekTransisi.SedangBerjalan, "Transisi " + id + " selesai");
        Check(target == null ? jalur.JalurAktif == null : jalur.JalurAktif != null && jalur.JalurAktif.name == target, "Tujuan " + id + " benar");
    }
    static IEnumerator Jalan(float arah, float target, float speed = 30)
    {
        arthur.kecepatanJalan = speed;
        arthur.BerhentiBergerak();
        if (arah > 0) arthur.JalanKeKanan(); else arthur.JalanKeKiri();
        float deadline = Time.realtimeSinceStartup + Mathf.Max(22, Mathf.Abs(target-jalur.JarakDiJalur)/Mathf.Max(.1f,speed*.5f)*1.6f+5);
        float jarakTerakhir = jalur.JarakDiJalur, terakhirMaju = Time.realtimeSinceStartup;
        while (!(arah > 0 ? jalur.JarakDiJalur >= target - .12f : jalur.JarakDiJalur <= target + .12f))
        {
            if(Mathf.Abs(jalur.JarakDiJalur-jarakTerakhir)>.02f){jarakTerakhir=jalur.JarakDiJalur;terakhirMaju=Time.realtimeSinceStartup;}
            if(Time.realtimeSinceStartup > deadline || Time.realtimeSinceStartup-terakhirMaju>3) throw new Exception("Gerak tertahan: target="+target+", jarak="+jalur.JarakDiJalur+", posisi="+arthur.transform.position+", lock="+arthur.kunciPergerakan+", speed="+jalur.KecepatanAktual+", input="+typeof(KarakterController).GetField("arahJalan",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(arthur));
            // Menjaga tombol uji tetap tertahan ketika fokus aplikasi berpindah.
            if(arah>0)arthur.JalanKeKanan();else arthur.JalanKeKiri();
            yield return null;
        }
        arthur.BerhentiBergerak(); yield return .5f;
        jalur.JalurAktif.Sampel(jalur.JarakDiJalur, out Vector3 p, out _, out _);
        Check(Vector3.Distance(p, arthur.transform.position) < .18f, "Arthur tetap pada pijakan " + jalur.JalurAktif.name+" (posisi="+arthur.transform.position+", jalur="+p+")");
    }
    static IEnumerator Jalankan()
    {
        arthur = UnityEngine.Object.FindFirstObjectByType<KarakterController>();
        jalur = arthur.GetComponent<ArthurJalurKampus>(); pilihan = UnityEngine.Object.FindFirstObjectByType<SistemPilihan>();
        // Hanya sesi pengujian: mencegah waktu idle menghentikan audit rute panjang.
        arthur.GetComponent<LazyBarArthur>().tanpaBatasWaktu = true;
        Check(jalur != null && jalur.colliderJalur != null, "Penggerak jalur dan fisika 3D tersedia");
        Check(GameObject.Find("NPC_Nara_Ekspresi") == null && GameObject.Find("NPC_Lyra") != null && GameObject.Find("NPC_Bimo_Joget") != null, "NPC bertanda dihapus; Lyra dan Bimo tetap ada");
        int frame = Time.frameCount; float waktu = Time.time; yield return .6f;
        Check(Time.frameCount > frame && Time.time > waktu + .25f, "Frame dan simulasi benar-benar berjalan di latar belakang");
        if(HanyaGarisPagar){yield return UjiGarisPagar();yield break;}
        if(HanyaBob){yield return UjiBobPanah();yield break;}
        if(HanyaLariGH){yield return UjiLariGH();yield break;}
        if(HanyaGH){yield return UjiGuestHouse();yield break;}
        if(HanyaLagu){yield return UjiLaguPianoHarpa();yield break;}
        if(HanyaEmosional){yield return UjiLaguEmosional();yield break;}
        if(HanyaPeta){yield return UjiPeta20261025();yield break;}
        if(HanyaKantin){yield return UjiKantinMerah();yield break;}
        if(HanyaLantaiMerah){yield return UjiLantaiMerah();yield break;}
        if(HanyaKameraBelakang){yield return UjiKameraBelakang();yield break;}
        if(HanyaBatas){yield return UjiBatasGA();yield break;}
        if(HanyaBelok){yield return UjiSambungan();yield break;}
        if(HanyaTeras){yield return UjiTeras();yield break;}
        if(HanyaPenanda){yield return UjiPenanda();yield break;}
        if(HanyaAkses){yield return UjiAkses();yield break;}
        if(HanyaPulang)
        {
            var utamaPulang=Titik("GK2_JalurUtama_BelokOtomatis").GetComponent<JalurKampus>();
            var marker=new GameObject("UjiPulang_Sementara");marker.transform.position=utamaPulang.titik[11].position;
            marker.AddComponent<TujuanJalurKampus>().jalur=utamaPulang;
            if(pilihan.grupFisikDepan!=null)pilihan.grupFisikDepan.SetActive(false);
            if(pilihan.grupFisikBelakang!=null)pilihan.grupFisikBelakang.SetActive(false);
            if(pilihan.grupFisikRuangan!=null)pilihan.grupFisikRuangan.SetActive(false);
            jalur.PindahKe(marker.transform);UnityEngine.Object.Destroy(marker);yield return .5f;
            yield return Jalan(-1,0,5);
            Foto("pulang-01-penanda-jasmine");
            yield return Pilih("GK2_KembaliKeJasmine",0,null);
            Check(arthur.GetComponent<Rigidbody2D>().simulated&&!jalur.colliderJalur.enabled,"Berjalan turun sampai pulang ke Jasmine melalui interaksi");
            Foto("pulang-02-tiba-jasmine");
            Log("LULUS: jalan pulang normal melewati turunan, penanda pulang, dialog, dan tiba di Jasmine.");yield break;
        }
        Foto("01-Jasmine");
        arthur.transform.position = new Vector3(6.98f,.443f,2.17f); arthur.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; yield return .6f;
        yield return Pilih("Menuju_GK2", 0, "GK2_JalurUtama_BelokOtomatis");
        Check(!arthur.GetComponent<Rigidbody2D>().simulated && jalur.colliderJalur.enabled, "Fisika beralih ke jalur GK2"); Foto("02-GK2-depan");
        var utama = jalur.JalurAktif;
        if (HanyaGK2)
        {
            Check(Mathf.Abs(jalur.kamera.fieldOfView - 78) < .05f, "Kamera utama GK2 memakai sudut lebar 78 derajat");
            yield return Jalan(-1, 0, 5);
            yield return TahanArah(-1, .8f);
            Vector3 batasMasuk = arthur.transform.position;
            yield return TahanArah(-1, .8f);
            Check(jalur.JarakDiJalur < .02f && Vector3.Distance(batasMasuk, arthur.transform.position) < .04f, "Arthur berhenti pada batas sisi masuk bertanda hijau");
            Foto("wide-01-batas-hijau");
        }
        float cabang = utama.JarakTerdekat(Titik("TitikPersimpangan_GK2").position);
        yield return Jalan(1, cabang - .3f);
        Check(DialogManager.Instance.opsiInteraksiAktif.Length == 2, "Persimpangan mempunyai tepat dua pilihan tujuan"); Foto("03-GK2-persimpangan");
        yield return Pilih("GK2_Persimpangan", 0, "GK2_TerasAtas"); Foto("04-GK2-teras");
        yield return Pilih("GK2_TurunDariTeras", 0, "GK2_JalurUtama_BelokOtomatis");
        yield return Pilih("GK2_Persimpangan", 1, "GK2_JalurBelakang"); Foto("05-GK2-belakang");
        yield return Jalan(-1, 0); Foto("06-GK2-batas-belakang");
        yield return Jalan(1, jalur.JalurAktif.Panjang);
        yield return Pilih("GK2_KembaliDariBelakang", 0, "GK2_JalurUtama_BelokOtomatis");
        float yawAwal = jalur.kamera.transform.eulerAngles.y;
        if (HanyaGK2)
        {
            float setelahTangga = utama.JarakTerdekat(new Vector3(6.5f+3.80034f,-58.5f+57.7f+ArthurJalurKampus.JarakKaki+.04f,45.5315f-107.5f));
            yield return Jalan(1, setelahTangga, 5);
            utama.Sampel(jalur.JarakDiJalur, out Vector3 tinggiPijakan, out _, out _);
            Check(arthur.transform.position.y >= tinggiPijakan.y - .025f, "Arthur menaiki paving tanpa tertinggal di bawahnya");
            Foto("wide-02-pijakan-tikungan");
        }
        yield return Jalan(1, cabang + 20, 5);
        Check(Mathf.Abs(Mathf.DeltaAngle(yawAwal, jalur.kamera.transform.eulerAngles.y)) > 15, "Kamera berbelok bersama jalan tanpa teleport"); Foto("07-GK2-tikungan");
        yield return Jalan(1, utama.Panjang); Foto("08-GK2-ujung-jalur");
        float sebelum = jalur.JarakDiJalur; arthur.JalanKeKanan(); yield return 1f; arthur.BerhentiBergerak();
        Check(Mathf.Abs(jalur.JarakDiJalur - sebelum) < .15f, "Hidden wall membatasi ujung jalur GK2");
        if (HanyaGK2) yield return Jalan(-1, 0, 5);
        else { jalur.PindahKe(Titik("TitikMasuk_GK2")); yield return .5f; yield return Jalan(-1, 0); }
        yield return Pilih("GK2_KembaliKeJasmine", 0, null);
        Check(arthur.GetComponent<Rigidbody2D>().simulated && !jalur.colliderJalur.enabled, "Fisika Jasmine pulih setelah kembali");
        Check(Mathf.Abs(jalur.kamera.transform.localPosition.z + 11) < .02f && Quaternion.Angle(jalur.kamera.transform.localRotation, Quaternion.identity) < .05f, "Kamera Jasmine kembali lurus dan berjarak 11");
        if (HanyaGK2)
        {
            Check(Mathf.Abs(jalur.kamera.fieldOfView - 60) < .05f && Mathf.Abs(jalur.kamera.GetComponent<BingkaiKameraTetap>().sudutAcuan - 60) < .05f, "Sudut kamera Jasmine pulih ke 60 setelah keluar GK2");
            Log("LULUS: batas hijau, dua cabang, paving naik, belokan, sudut lebar, dan kamera pulang.");
            yield break;
        }
        arthur.transform.position = new Vector3(100.5f,.443f,2.17f); arthur.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; yield return .6f;
        Foto("09-Jasmine-menuju-kantin");
        yield return Pilih("Menuju_Kantin", 0, "Kantin_LorongTengah"); Foto("10-Kantin");
        yield return Jalan(1, jalur.JalurAktif.Panjang, 5); Foto("11-Kantin-batas-kanan");
        yield return Jalan(-1, 0, 5); Foto("12-Kantin-jalur-pulang");
        yield return Jalan(1, jalur.JalurAktif.Panjang, 5);
        yield return Pilih("Kantin_KembaliKeJasmine", 0, null);
        arthur.kecepatanJalan = 5; yield return .5f;
        Check(arthur.GetComponent<Rigidbody2D>().simulated && Mathf.Abs(arthur.transform.position.z - 2.17f) < .05f, "Pulang dari Kantin ke jalan Jasmine");
        arthur.JalanKeKanan(); yield return 2f; arthur.BerhentiBergerak();
        Check(arthur.transform.position.x < 102f && jalur.kamera.transform.position.x <= 92.01f, "Batas ungu dan batas kamera menahan Arthur sebelum ujung world");
        Foto("13-Jasmine-hidden-wall");
        Log("LULUS: seluruh alur interaksi, tikungan, pijakan, batas, kamera pulang, dan perpindahan mode fisika.");
    }
    static IEnumerator DiJalan(float x)
    {
        if(pilihan.grupFisikDepan!=null)pilihan.grupFisikDepan.SetActive(true);
        if(pilihan.grupFisikBelakang!=null)pilihan.grupFisikBelakang.SetActive(false);
        var g=new GameObject("TitikUjiAkses_Sementara");g.transform.position=new Vector3(x,.443f,2.17f);
        jalur.PindahKe(g.transform);UnityEngine.Object.Destroy(g);arthur.BerhentiBergerak();yield return .6f;
    }
    static void KlikNavigasiHP(UnityEngine.UI.Button button)
    {
        Canvas.ForceUpdateCanvases();
        var e=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position),button=UnityEngine.EventSystems.PointerEventData.InputButton.Left};
        var hits=new List<UnityEngine.EventSystems.RaycastResult>();UnityEngine.EventSystems.EventSystem.current.RaycastAll(e,hits);
        Check(hits.Count>0&&UnityEngine.EventSystems.ExecuteEvents.GetEventHandler<UnityEngine.EventSystems.IPointerClickHandler>(hits[0].gameObject)==button.gameObject,"Tombol dapat diketuk tanpa terhalang: "+button.transform.parent.parent.parent.name+"/"+button.name);
        UnityEngine.EventSystems.ExecuteEvents.Execute(button.gameObject,e,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
    }
    static IEnumerator UjiGarisPagar()
    {
        var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();hp.BukaHPDefault();yield return 1f;
        foreach(string icon in new[]{"IkonToDoLIst","IkonTas","IkonMap"})
        {
            hp.BukaAplikasi(icon);yield return 1.4f;Canvas.ForceUpdateCanvases();
            Transform app=icon=="IkonMap"?hp.petaKampus.transform:icon=="IkonTas"?hp.tas.transform:hp.tugas.transform;
            var buttons=app.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            var left=buttons.Single(b=>b.name=="Kiri");var right=buttons.Single(b=>b.name=="Kanan");
            int before=icon=="IkonMap"?hp.petaKampus.IndeksTerpilih:icon=="IkonTas"?hp.tas.Pilihan:hp.tugas.Pilihan;
            KlikNavigasiHP(right);yield return .2f;
            if(icon!="IkonToDoLIst")Check((icon=="IkonMap"?hp.petaKampus.IndeksTerpilih:hp.tas.Pilihan)==before+1,"Ketukan kanan memilih isi berikut: "+icon);
            KlikNavigasiHP(left);yield return .2f;
            int after=icon=="IkonMap"?hp.petaKampus.IndeksTerpilih:icon=="IkonTas"?hp.tas.Pilihan:hp.tugas.Pilihan;
            Check(after==before,"Navigasi dua arah tetap bekerja: "+icon);
            ScreenCapture.CaptureScreenshot("work/garis-pagar-20261025/"+icon+".png");yield return .3f;
            hp.TutupAplikasi();yield return 1f;
        }
        hp.TutupHP();yield return .5f;
        Check(!arthur.kunciPergerakan,"Kendali pulih setelah menutup HP");
        var upper=Titik("GH_BalkonLantaiDua").GetComponent<JalurKampus>();
        TempatkanUjiJalur(upper,3);yield return .5f;yield return Jalan(-1,0,5);yield return TahanArah(-1,.5f);
        Check(Mathf.Abs(arthur.transform.position.x-280.4f)<.04f,"Berjalan berhenti pada batas kiri baru");
        yield return Jalan(1,4,5);yield return MulaiLari(-1);
        float end=Time.realtimeSinceStartup+1.5f,min=float.PositiveInfinity,minRoot=float.PositiveInfinity;int samples=0;
        var sprites=jalur.wadahArahVisual.GetComponentsInChildren<SpriteRenderer>(true);
        while(Time.realtimeSinceStartup<end)
        {
            arthur.JalanKeKiri();
            foreach(var s in sprites)if(s.enabled&&s.gameObject.activeInHierarchy)min=Mathf.Min(min,s.bounds.min.x);
            minRoot=Mathf.Min(minRoot,arthur.transform.position.x);samples++;yield return null;
        }
        arthur.BerhentiBergerak();yield return .3f;
        Log("PAGAR minSpriteX="+min+", sisiDalamPagar=278.125, samples="+samples);
        Check(minRoot>=280.39f,"Lari tidak melewati batas kiri");
        Check(samples>10&&min>278.225f,"Seluruh pose badan/kaki tetap di sisi dalam pagar ketika berlari ke kiri");
        Check(Mathf.Abs(jalur.kamera.transform.position.x-281.2f)<.01f,"Batas kamera kiri tetap seperti sebelumnya");
        Foto("pagar-GH-kiri-baru");yield return FotoMenu("pagar-GH-kiri-HUD");
        yield return Jalan(1,upper.JarakTerdekat(Titik("TitikNaik_GH").position),5);yield return Pilih("GH_TurunLantaiSatu",0,"GH_LantaiSatu");
        yield return DiJalan(6.98f);
        Log("LULUS: garis UI diperiksa dari foto, navigasi HP, batas badan saat jalan/lari, kamera, dan akses turun.");
    }
    static IEnumerator UjiBobPanah()
    {
        yield return DiJalan(6.98f);PeriksaPasangan("TriggerTempatKeGK2","↓","↑");Foto("panah-01-GK2-dua-arah");yield return FotoMenu("panah-01-HUD");
        yield return DiJalan(59.42f);PeriksaPasangan("TriggerTempatKeHalamanAsramaJasmine2","↑","↓");Foto("panah-02-Jasmine-dua-arah");yield return FotoMenu("panah-02-HUD");
        foreach(string n in new[]{"GK2_KembaliKeJasmine"})
        {
            var access=Titik(n).GetComponent<InteraksiJalurKampus>();
            TempatkanUjiJalur(access.jalur,access.jalur.JarakTerdekat(access.transform.position));yield return .7f;
            var label=access.ikon.GetComponentInChildren<TMPro.TMP_Text>(true);label.ForceMeshUpdate(true,true);
            Check(label.text=="←","Bubble pada screenshot ketiga memakai panah kiri");
            Foto("panah-03-"+n);yield return FotoMenu("panah-03-HUD-"+n);
        }
        yield return DiJalan(40);yield return UkurBob(1,false,"Jasmine-jalan");
        yield return DiJalan(40);yield return MulaiLari(1);yield return UkurBob(1,true,"Jasmine-lari");
        foreach(string n in new[]{"GH_LantaiSatu","GH_BalkonLantaiDua","GK2_JalurBelakang","GK2_TerasAtas","Kantin_LorongTengah"})
        {
            var r=Titik(n).GetComponent<JalurKampus>();
            float d=n=="GH_LantaiSatu"?13:n=="GH_BalkonLantaiDua"?6:n=="Kantin_LorongTengah"?.3f:n=="GK2_JalurBelakang"?r.JarakTerdekat(new Vector3(5.3f,0,-89)):5;
            TempatkanUjiJalur(r,d);yield return .5f;yield return UkurBob(1,false,n+"-jalan");
            TempatkanUjiJalur(r,d);yield return .5f;yield return MulaiLari(1);yield return UkurBob(1,true,n+"-lari");
        }
        var upper=Titik("GH_BalkonLantaiDua").GetComponent<JalurKampus>();
        TempatkanUjiJalur(upper,upper.Panjang);yield return .5f;yield return MulaiLari(1);yield return TahanArah(1,.7f);
        Check(Mathf.Abs(jalur.kamera.GetComponent<KameraGoyang>().OffsetSaatIni)<.0001f,"Tidak mengayun ketika tombol lari ditekan di batas tanpa berpindah");
        yield return DiJalan(6.98f);pilihan.AksiTombolInteraksi();yield return .4f;
        Check(DialogManager.Instance.PercakapanAktif&&Mathf.Abs(jalur.kamera.GetComponent<KameraGoyang>().OffsetSaatIni)<.0001f,"Dialog tetap tenang");
        pilihan.TekanKeluarLangsung();yield return .4f;
        Log("LULUS: panah seimbang, ayunan lari di Jasmine/GH/GK2/Kantin, jalan/diam/dialog/batas tenang.");
    }
    static void PeriksaPasangan(string name,string first,string second)
    {
        var icon=GameObject.Find(name).GetComponent<TempatBubbleTrigger>().ikonTransform;
        var labels=icon.GetComponentsInChildren<TMPro.TMP_Text>(true);
        Check(labels.Length==2&&labels[0].text==first&&labels[1].text==second,"Arah pasangan sesuai screenshot: "+name);
        Check(labels.All(t=>t.GetComponent<MeshRenderer>().sortingOrder>icon.GetComponent<SpriteRenderer>().sortingOrder),"Kedua panah tampil di depan isi bubble: "+name);
        var centers=labels.Select(t=>{t.ForceMeshUpdate(true,true);return icon.InverseTransformPoint(t.transform.TransformPoint(t.textBounds.center));}).ToArray();
        Check(Mathf.Abs(centers[0].x+centers[1].x)<.001f&&Mathf.Abs(centers[0].y-centers[1].y)<.001f&&Mathf.Abs(centers[0].y-1)<.001f,"Dua glyph sama tinggi dan simetris terhadap tengah bubble: "+name);
    }
    static IEnumerator UkurBob(float input,bool run,string name)
    {
        var camera=jalur.kamera;var bob=camera.GetComponent<KameraGoyang>();
        Check(bob.enabled,"Ayunan tersedia pada "+name);
        float min=float.PositiveInfinity,max=float.NegativeInfinity,baseMin=float.PositiveInfinity,baseMax=float.NegativeInfinity;
        int frame=-1,count=0;var rows=new List<string>{"frame,time,bob,cameraY,baseY,running"};
        Action<Camera> capture=cam=>
        {
            if(cam!=camera||Time.frameCount==frame)return;frame=Time.frameCount;count++;
            float offset=bob.OffsetSaatIni,y=cam.transform.position.y-offset;
            min=Mathf.Min(min,offset);max=Mathf.Max(max,offset);baseMin=Mathf.Min(baseMin,y);baseMax=Mathf.Max(baseMax,y);
            rows.Add(string.Join(",",new[]{(float)frame,Time.time,offset,cam.transform.position.y,y,arthur.SedangBerlari?1:0}.ConvertAllInvariant()));
        };
        Camera.CameraCallback builtin=cam=>capture(cam);
        Action<UnityEngine.Rendering.ScriptableRenderContext,Camera> srp=(ctx,cam)=>capture(cam);
        Camera.onPreCull+=builtin;UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering+=srp;
        try{yield return TahanArah(input,run?1.2f:.8f);}
        finally{Camera.onPreCull-=builtin;UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering-=srp;File.WriteAllLines("work/panah-bob-lari-20261025/"+name+".csv",rows);}
        Log("BOB "+name+" frame="+count+" min="+min+" max="+max+" baseRange="+(baseMax-baseMin));
        Check(count>10,"Kamera benar-benar dirender selama "+name);
        Check(run?min<-.02f&&max>.02f&&Mathf.Max(-min,max)<=bob.bobAmount+.00001f:Mathf.Max(-min,max)<.0001f,"Hanya lari berayun kecil, jalan tenang: "+name);
        Check(baseMax-baseMin<.001f,"Ayunan tidak masuk ke bingkai dasar atau menumpuk: "+name);
        yield return .25f;
        Check(Mathf.Abs(bob.OffsetSaatIni)<.0001f,"Kamera kembali tenang setelah berhenti: "+name);
    }
    static IEnumerator UjiLariGH()
    {
        var lower=Titik("GH_LantaiSatu").GetComponent<JalurKampus>();
        var upper=Titik("GH_BalkonLantaiDua").GetComponent<JalurKampus>();
        yield return LariTerukur(upper,4,1,1.8f,"balkon-kanan");
        yield return LariTerukur(upper,17,-1,1.8f,"balkon-kiri");
        yield return LariTerukur(lower,12,1,1.8f,"bawah-kanan");
        yield return LariTerukur(lower,22,-1,1.8f,"bawah-kiri");
        yield return LariTerukur(lower,lower.Panjang,-1,1.5f,"sudut-naik");
        yield return LariTerukur(lower,20,1,1.5f,"sudut-turun");
        TempatkanUjiJalur(upper,upper.Panjang-2.5f);yield return .6f;
        yield return MulaiLari(1);yield return TahanArah(1,1);
        Check(Mathf.Abs(arthur.transform.position.x-299)<.04f,"Lari berhenti 2,1 satuan sebelum ujung pagar lama");
        Check(jalur.kamera.transform.position.x<=296.301f,"Bingkai kanan balkon tetap dibatasi");
        Foto("gh-batas-langkah-kanan-baru");yield return FotoMenu("gh-HUD-batas-langkah-kanan-baru");
        foreach(float x in new[]{300.8f,301.5f,302.2f})
        {
            TempatkanUjiJalur(lower,lower.JarakTerdekat(new Vector3(x,0,-18.45f)));yield return .7f;
            Foto("gh-sudut-"+x.ToString("F1",System.Globalization.CultureInfo.InvariantCulture));
            Check(arthur.transform.position.z< -18.3f,"Sudut mendapat ruang lewat di depan tembok pada X="+x);
        }
        var back=Titik("GK2_JalurBelakang").GetComponent<JalurKampus>();
        yield return LariTerukur(back,back.JarakTerdekat(new Vector3(5.3f,0,-89)),1,1.8f,"GK2-maju");
        yield return LariTerukur(back,back.JarakTerdekat(new Vector3(5.3f,0,-79)),-1,1.8f,"GK2-mundur");
        yield return DiJalan(Titik("TitikKembaliJasmine_DariGH").position.x);
        Log("LULUS: mode lari asli pada dua lantai GH dan dua arah sudut tangga.");
    }
    static IEnumerator LariTerukur(JalurKampus route,float distance,float input,float seconds,string name)
    {
        TempatkanUjiJalur(route,distance);yield return .6f;
        yield return MulaiLari(input);
        Check(arthur.SedangBerlari,"Mode lari ketuk ganda aktif: "+name);
        route.Sampel(jalur.JarakDiJalur,out _,out var tangent,out _);
        var direction=tangent*input*(Vector3.Dot(tangent,jalur.kamera.transform.right)>=0?1:-1);
        var trace=new JejakGerakKamera(jalur,direction,"work/gh-lari-20261025/"+name+".csv");
        try { yield return TahanArah(input,seconds); } finally { trace.Dispose(); }
        Log("LARI "+name+": frames="+trace.Frames+", planarRunSpeed="+trace.AverageRunSpeed+", reverse="+trace.CameraReverse+", maxRunSpeedError="+trace.MaxRunSpeedError);
        Check(trace.Frames>10&&trace.AverageRunSpeed>4.9f&&trace.AverageRunSpeed<5.1f,"Lari diukur pada kecepatan asli, termasuk ketika jalur menyamping: "+name);
        Check(trace.RunningFrames>10&&trace.MaxRunSpeedError<1f,"Tidak ada langkah fisika terputus atau badan tertahan lalu menyusul: "+name);
        yield return .3f;
        Foto("lari-"+name);
    }
    static void TempatkanUjiJalur(JalurKampus route,float distance)
    {
        pilihan.grupFisikDepan.SetActive(false);if(pilihan.grupFisikBelakang)pilihan.grupFisikBelakang.SetActive(false);if(pilihan.grupFisikRuangan)pilihan.grupFisikRuangan.SetActive(false);
        var g=new GameObject("TitikUjiLari_Sementara");route.Sampel(distance,out var pos,out _,out _);g.transform.position=pos;g.AddComponent<TujuanJalurKampus>().jalur=route;
        jalur.PindahKe(g.transform);UnityEngine.Object.Destroy(g);arthur.BerhentiBergerak();arthur.kecepatanJalan=5;
    }
    static IEnumerator MulaiLari(float input)
    {
        arthur.BerhentiBergerak();
        // Gerakan lari menggunakan ketuk ganda yang sama dengan tombol pemain.
        if(input>0)arthur.JalanKeKanan();else arthur.JalanKeKiri();yield return .06f;
        if(input>0)arthur.LepasKanan();else arthur.LepasKiri();yield return .06f;
        if(input>0)arthur.JalanKeKanan();else arthur.JalanKeKiri();
    }
    static IEnumerator UjiGuestHouse()
    {
        yield return DiJalan(Titik("TitikKembaliJasmine_DariGH").position.x);
        arthur.kecepatanJalan=5;yield return TahanArah(-1,.4f);
        Check(arthur.transform.position.x < -25,"Arthur dapat berjalan melewati pohon ke akses GH yang diperpanjang");
        Foto("gh-00-akses-Jasmine-terlihat");yield return FotoMenu("gh-00-HUD-akses-Jasmine");
        Check(GameObject.Find("TriggerTempatKeAsramaGH").GetComponent<AreaInteraksi>().daftarOpsi[0].titikTujuan==Titik("TitikMasuk_GH"),"Ke Asrama GH sudah aktif pada akses lama Jasmine");
        yield return Pilih("Menuju_AsramaGH",0,"GH_LantaiSatu");
        var lower=jalur.JalurAktif;var upper=Titik("GH_BalkonLantaiDua").GetComponent<JalurKampus>();
        Check(Vector3.Distance(arthur.transform.position,Titik("TitikMasuk_GH").position)<.08f,"Arthur tiba di ujung kanan garis merah GH");
        Check(!arthur.GetComponent<Rigidbody2D>().simulated&&jalur.colliderJalur.enabled,"GH memakai penggerak jalur 3D");
        Check(Vector3.Distance(jalur.kamera.transform.position,new Vector3(301.65f,3.18f,-24.77f))<.03f,"Kamera tiba pada bingkai screenshot pertama, bukan mengikuti sampai ujung paving");
        Foto("gh-01-tiba");yield return FotoMenu("gh-01-HUD-tiba");
        var bubble=Titik("GH_KembaliKeJasmine").GetComponent<InteraksiJalurKampus>().ikon;var tetap=bubble.position;
        Check(bubble.GetComponentInChildren<TMPro.TMP_Text>(true).text=="↑","Bubble kembali dari GH memakai panah atas");
        yield return TahanArah(1,.45f);Check(jalur.JarakDiJalur>lower.Panjang-.05f,"Batas kanan GH menahan Arthur");
        float stair=lower.JarakTerdekat(lower.titik[3].position);
        var stepTrace=new JejakGerakKamera(jalur,Vector3.left,"work/gh-stabil-20261025/langkah-GH-naik.csv");
        try { yield return Jalan(-1,stair-1,5); } finally { stepTrace.Dispose(); }
        Log("UKUR langkah GH: kecepatan="+stepTrace.Speed+", berbalik="+stepTrace.ActorReverse+", frame="+stepTrace.Frames);
        Check(stepTrace.Speed>1.7f&&stepTrace.ActorReverse<.001f,"Arthur melewati sambungan GH dengan mulus pada kecepatan jalan normal");Foto("gh-02-step");
        float access=lower.JarakTerdekat(Titik("TitikTurun_GH").position);
        yield return Jalan(-1,13.8f,5);Foto("gh-02a-blok-tiang-utuh");yield return FotoMenu("gh-02a-HUD-blok-tiang");
        yield return Jalan(-1,access,5);yield return FotoMenu("gh-03-HUD-akses-naik");Foto("gh-03-akses-naik");
        Check(Vector3.Distance(bubble.position,tetap)<.001f,"Bubble pulang menetap di titik masuk GH");
        Check(DialogManager.Instance.idInteraksiAktif=="GH_NaikLantaiDua","Akses naik berada di ceruk tengah");
        yield return Pilih("GH_NaikLantaiDua",0,"GH_BalkonLantaiDua");
        Check(Vector3.Distance(arthur.transform.position,Titik("TitikNaik_GH").position)<.1f,"Arthur tiba pada balkon lantai dua");
        Foto("gh-04-balkon-tiba");yield return FotoMenu("gh-04-HUD-balkon");
        Check(Titik("GH_TurunLantaiSatu").GetComponent<InteraksiJalurKampus>().ukuranIkon.x>=.08f,"Bubble turun balkon diperbesar");
        yield return UjiGerakMulus(upper,15,-1,Vector3.left,"kamera-GH-kiri");
        jalur.PindahKe(Titik("TitikNaik_GH"));yield return .6f;
        yield return Jalan(-1,0,5);yield return TahanArah(-1,.5f);Check(jalur.JarakDiJalur<.03f,"Batas kiri balkon menahan Arthur");Foto("gh-05-balkon-kiri");
        Check(Mathf.Abs(jalur.kamera.transform.position.x-upper.batasKiriKamera)<.03f,"Kamera balkon berhenti pada batas kiri screenshot");yield return FotoMenu("gh-05-HUD-batas-kiri");
        yield return Jalan(1,upper.Panjang,5);yield return TahanArah(1,.5f);Check(jalur.JarakDiJalur>upper.Panjang-.03f,"Batas kanan balkon menahan Arthur");Foto("gh-06-balkon-kanan");
        Check(Mathf.Abs(jalur.kamera.transform.position.x-upper.batasKananKamera)<.03f,"Kamera balkon berhenti pada batas kanan screenshot");yield return FotoMenu("gh-06-HUD-batas-kanan");
        yield return Jalan(-1,upper.JarakTerdekat(Titik("TitikNaik_GH").position),5);yield return Pilih("GH_TurunLantaiSatu",0,"GH_LantaiSatu");
        Check(Vector3.Distance(arthur.transform.position,Titik("TitikTurun_GH").position)<.1f,"Turun kembali ke ceruk lantai satu");
        yield return Jalan(-1,3.4f,5);Foto("gh-07a-lewat-belakang-parkir");yield return FotoMenu("gh-07a-HUD-belakang-parkir");
        yield return Jalan(-1,0,5);yield return TahanArah(-1,.5f);Check(jalur.JarakDiJalur<.03f,"Batas kiri lantai satu menahan Arthur di tanda merah");Foto("gh-07-batas-kiri");
        Check(GameObject.Find("Parkir_GuestHouse/Motor_Parkir")!=null,"Parkir terbuka dan motor terpasang");
        yield return Jalan(1,lower.Panjang,5);yield return Pilih("GH_KembaliKeJasmine",0,null);
        Check(pilihan.grupFisikDepan.activeSelf&&arthur.GetComponent<Rigidbody2D>().simulated&&!jalur.colliderJalur.enabled,"Pulang memulihkan fisika depan Jasmine");
        Check(Mathf.Abs(arthur.transform.position.x-Titik("TitikKembaliJasmine_DariGH").position.x)<.1f&&Mathf.Abs(arthur.transform.position.z-2.17f)<.1f,"Pulang tepat ke akses GH yang digeser di depan Jasmine");Foto("gh-08-pulang-Jasmine");yield return FotoMenu("gh-08-HUD-pulang-Jasmine");
        var back=Titik("GK2_JalurBelakang").GetComponent<JalurKampus>();
        yield return UjiGerakMulus(back,back.JarakTerdekat(new Vector3(5.3f,0,-89)),1,Vector3.forward,"kamera-GK2-maju");
        yield return UjiGerakMulus(back,back.JarakTerdekat(new Vector3(5.3f,0,-79)),-1,Vector3.back,"kamera-GK2-mundur");
        yield return DiJalan(Titik("TitikKembaliJasmine_DariGH").position.x);
        Log("LULUS: masuk GH, langkah teras, dua lantai, semua batas, penanda statis, parkir, dan pulang Jasmine.");
    }
    static IEnumerator UjiGerakMulus(JalurKampus route,float distance,float input,Vector3 direction,string name)
    {
        pilihan.grupFisikDepan.SetActive(false);if(pilihan.grupFisikBelakang)pilihan.grupFisikBelakang.SetActive(false);if(pilihan.grupFisikRuangan)pilihan.grupFisikRuangan.SetActive(false);
        var g=new GameObject("TitikUjiKamera_Sementara");route.Sampel(distance,out var pos,out _,out _);g.transform.position=pos;g.AddComponent<TujuanJalurKampus>().jalur=route;
        jalur.PindahKe(g.transform);UnityEngine.Object.Destroy(g);arthur.BerhentiBergerak();arthur.kecepatanJalan=5;yield return .8f;
        var trace=new JejakGerakKamera(jalur,direction,"work/gh-stabil-20261025/"+name+".csv");
        try { yield return TahanArah(input,1.8f); } finally { trace.Dispose(); }
        Log("UKUR "+name+": frame="+trace.Frames+", tarikan balik="+trace.CameraReverse+", getar vertikal="+trace.VerticalRange+", laju Arthur="+trace.Speed);
        Check(trace.Frames>10,"Frame kamera benar-benar dirender pada "+name);
        Check(trace.CameraReverse<.001f&&trace.VerticalRange<.001f,"Tidak ada tarikan balik atau getar vertikal pada "+name);
        Check(trace.Speed>2.1f&&trace.ActorReverse<.001f,"Laju Arthur normal dan tidak tersendat pada "+name);
    }
    static IEnumerator UjiTeras()
    {
        yield return DiJalan(6.98f);yield return Pilih("Menuju_GK2",0,"GK2_JalurUtama_BelokOtomatis");
        var utama=jalur.JalurAktif;
        Check(!UnityEngine.Resources.FindObjectsOfTypeAll<InteraksiJalurKampus>().Any(t=>t.gameObject.scene.IsValid()&&t.ikutiKepalaArthur)&&!UnityEngine.Resources.FindObjectsOfTypeAll<TempatBubbleTrigger>().Any(t=>t.gameObject.scene.IsValid()&&t.pusatkanDiAtasKepalaArthur),"Penanda teleportasi tidak mengikuti Arthur berjalan");
        yield return Jalan(-1,0,5);Foto("gk2-penanda-awal-statis");PeriksaPenandaJalurTetap("GK2_KembaliKeJasmine");
        var tetap=Titik("GK2_KembaliKeJasmine").GetComponent<InteraksiJalurKampus>().ikon;Vector3 posisiTetap=tetap.position;
        yield return TahanArah(1,.4f);Check(Vector3.Distance(tetap.position,posisiTetap)<.001f,"Penanda awal tetap di titik teleportasi saat Arthur berjalan");
        float cabang=utama.JarakTerdekat(Titik("TitikPersimpangan_GK2").position);yield return Jalan(1,cabang+1.65f,5);
        PeriksaPenandaJalurTetap("GK2_Persimpangan");Foto("gk2-penanda-persimpangan-statis");
        Check(DialogManager.Instance.opsiInteraksiAktif.Length==2,"Percabangan bawah tetap mempunyai dua pilihan");
        yield return Pilih("GK2_Persimpangan",0,"GK2_TerasAtas");var teras=jalur.JalurAktif;
        Check(Vector3.Distance(arthur.transform.position,teras.titik[0].position)<.08f,"Teleport langsung tiba di batas kiri pilar teras");
        Check(Mathf.Abs(arthur.transform.position.x-14.80034f)<.08f&&Mathf.Abs(arthur.transform.position.z+56.9685f)<.08f,"Arthur berada pada teras depan sesuai gambar");
        Foto("teras-01-tiba-pilar");
        Check(Mathf.Abs(Mathf.DeltaAngle(jalur.kamera.transform.eulerAngles.y,0))<.05f,"Kamera teras menghadap lurus seperti screenshot");
        Check(jalur.kamera.transform.position.y>3.5f&&jalur.kamera.transform.position.y<3.8f&&Mathf.Abs(jalur.kamera.fieldOfView-60)<.05f,"Kamera teras diturunkan ke tinggi yang direvisi dengan sudut 60 derajat");
        PeriksaBubbleTeras();
        posisiTetap=Titik("GK2_TurunDariTeras").GetComponent<InteraksiJalurKampus>().ikon.position;arthur.kecepatanJalan=5;
        yield return TahanArah(1,1);Check(arthur.transform.position.x>17,"Arthur dapat berjalan ke kanan melewati pilar yang sebelumnya terhalang");
        Check(Vector3.Distance(Titik("GK2_TurunDariTeras").GetComponent<InteraksiJalurKampus>().ikon.position,posisiTetap)<.001f,"Penanda turun menetap di akses teras, tidak mengikuti Arthur");Foto("teras-02-jalan-terus-kanan");
        yield return Jalan(1,teras.Panjang,30);Foto("teras-03-ujung-kanan");Check(arthur.transform.position.x>47.6f&&arthur.transform.position.x<48,"Jalur teras memanjang sampai sebelum dinding asli sayap gedung");
        Check(jalur.kamera.transform.position.x<45.02f,"Kamera berhenti sebelum masuk ke sayap kanan gedung");
        yield return Jalan(-1,0,30);yield return TahanArah(-1,1);Check(jalur.JarakDiJalur<.03f&&Mathf.Abs(arthur.transform.position.y-teras.titik[0].position.y)<.06f,"Batas kiri di pilar menahan Arthur agar tidak turun melalui jalan");
        Foto("teras-02-batas-kiri");PeriksaBubbleTeras();
        var kembali=Titik("GK2_TurunDariTeras");
        Check(kembali.GetComponent<AreaInteraksi>().daftarOpsi[0].teksOpsi=="Ke bawah","Menu pulang menampilkan Ke bawah");
        var ikon=kembali.GetComponent<InteraksiJalurKampus>().ikon;
        Check(ikon.GetComponentInChildren<TMPro.TMP_Text>(true).text=="↓","Penanda turun memakai panah bawah");
        yield return Pilih("GK2_TurunDariTeras",0,"GK2_JalurUtama_BelokOtomatis");
        Check(Mathf.Abs(jalur.JarakDiJalur-cabang)<.1f,"Ke bawah mengembalikan Arthur ke persimpangan semula");Foto("teras-03-kembali-bawah");
        yield return Pilih("GK2_Persimpangan",1,"GK2_JalurBelakang");
        float aksesBelakang=jalur.JalurAktif.JarakTerdekat(Titik("GK2_KembaliDariBelakang").position);
        yield return Jalan(jalur.JarakDiJalur<aksesBelakang?1:-1,aksesBelakang,5);yield return .6f;Foto("gk2-penanda-belakang-pas");
        Check(Titik("GK2_KembaliDariBelakang").GetComponent<InteraksiJalurKampus>().ikon.localScale.x>.1f,"Bubble belakang memakai ukuran revisi dan tetap terlihat");
        yield return Pilih("GK2_KembaliDariBelakang",0,"GK2_JalurUtama_BelokOtomatis");
        Log("LULUS: penanda statis dekat kepala, akses kiri teras, jalan terus ke kanan, dan turun melalui interaksi.");
    }
    static IEnumerator KeNode(JalurKampus r,int i,float arah=1,float speed=10)
    {
        yield return Jalan(arah,r.JarakTerdekat(r.titik[i].position),speed);
    }
    static IEnumerator PeriksaGA(string id,string foto,string label="Ke GA")
    {
        yield return Tunggu(()=>DialogManager.Instance.idInteraksiAktif==id,3,"Penanda GA "+id);
        pilihan.AksiTombolInteraksi();yield return .4f;yield return FotoMenu(foto);
        Check(DialogManager.Instance.opsiInteraksiAktif.Length==1&&DialogManager.Instance.opsiInteraksiAktif[0].teksOpsi==label,"Menu ujung menawarkan "+label);
        var posisi=arthur.transform.position;pilihan.TekanPilihanLangsung(0);yield return .3f;
        Check(DialogManager.Instance.PercakapanAktif&&pilihan.teksCeritaUtama.text.Contains("belum")&&Vector3.Distance(posisi,arthur.transform.position)<.05f,"GA menampilkan keterangan sampai world tersedia");
        pilihan.TekanKeluarLangsung();yield return .8f;
    }
    static IEnumerator UjiKameraBelakang()
    {
        yield return DiJalan(6.98f);yield return Pilih("Menuju_GK2",0,"GK2_JalurUtama_BelokOtomatis");
        float cabang=jalur.JalurAktif.JarakTerdekat(Titik("TitikPersimpangan_GK2").position);yield return Jalan(1,cabang-1,10);yield return Jalan(1,cabang+1.65f,5);yield return Pilih("GK2_Persimpangan",1,"GK2_JalurBelakang");
        var r=jalur.JalurAktif;var arah=(float[])r.arahKamera.Clone();
        float bingkai=r.JarakTerdekat(new Vector3(r.batasKananKamera,0,-104.4685f));yield return Jalan(-1,bingkai,12);yield return .5f;Foto("kamera-belakang-01-bingkai-acuan");
        yield return Jalan(-1,0,5);yield return TahanArah(-1,.4f);Foto("kamera-belakang-02-Arthur-sampai-GA");
        Check(Mathf.Abs(arthur.transform.position.x-40.80034f)<.08f,"Batas gerak Arthur tetap pada penanda GA");
        var c=jalur.kamera;Check(Mathf.Abs(c.transform.position.x-r.batasKananKamera)<.005f,"Kamera berhenti tepat pada X bingkai pilihan pengguna");
        Check(Mathf.Abs(c.transform.position.z+96.4685f)<.03f&&Mathf.Abs(c.transform.position.y-3.08533f)<.04f,"Tinggi dan kedalaman bingkai acuan tetap");
        Check(Mathf.Abs(Mathf.DeltaAngle(c.transform.eulerAngles.y,180))<.05f&&Mathf.Abs(c.transform.eulerAngles.x-7.47733f)<.05f&&Mathf.Abs(c.fieldOfView-74)<.05f,"Arah dan lebar kamera pilihan pengguna dipertahankan");
        var b=Titik("GK2_Belakang_KeGA").GetComponent<InteraksiJalurKampus>().ikon.GetComponent<SpriteRenderer>().bounds;
        foreach(float x in new[]{b.min.x,b.max.x})foreach(float y in new[]{b.min.y,b.max.y})foreach(float z in new[]{b.min.z,b.max.z}){var v=c.WorldToViewportPoint(new Vector3(x,y,z));Check(v.x>.02f&&v.x<.98f&&v.y>.02f&&v.y<.98f,"Bubble GA utuh meskipun kamera sudah berhenti");}
        yield return PeriksaGA("GK2_Belakang_KeGA","kamera-belakang-menu-GA");
        yield return Jalan(1,r.JarakTerdekat(new Vector3(29,0,-104.4685f)),10);
        Check(c.transform.position.x<30&&c.transform.position.x<=r.batasKananKamera,"Kamera kembali mengikuti ketika Arthur berjalan menjauh dari batas");
        Check(arah.SequenceEqual(r.arahKamera),"Semua arah kamera jalur tetap seperti pilihan pengguna");Foto("kamera-belakang-03-kembali-mengikuti");
        Log("LULUS: hanya kamera dibatasi pada bingkai pengguna; Arthur tetap mencapai GA dan kembali.");
    }
    static void PeriksaLantaiMerah()
    {
        var p=arthur.transform.position;bool ada=false;
        foreach(var f in GameObject.Find("World_JasmineKantinGK2").GetComponentsInChildren<MeshFilter>().Where(f=>f.name=="Waxed_Cut_Copper"))
        {var m=f.sharedMesh;var v=m.vertices;var ids=m.triangles;for(int i=0;i<ids.Length;i+=3){var a=f.transform.TransformPoint(v[ids[i]]);var b=f.transform.TransformPoint(v[ids[i+1]]);var c=f.transform.TransformPoint(v[ids[i+2]]);if(Mathf.Abs(a.y+2.3f)>.01f||Mathf.Abs(a.y-b.y)>.001f||Mathf.Abs(a.y-c.y)>.001f)continue;float d=(b.x-a.x)*(c.z-a.z)-(b.z-a.z)*(c.x-a.x);if(Mathf.Abs(d)<.0001f)continue;float u=((p.x-a.x)*(c.z-a.z)-(p.z-a.z)*(c.x-a.x))/d,w=((b.x-a.x)*(p.z-a.z)-(b.z-a.z)*(p.x-a.x))/d;if(u>=0&&w>=0&&u+w<=1){ada=true;break;}}if(ada)break;}
        Check(ada&&Mathf.Abs(p.x-3.30034f)<.05f&&Mathf.Abs(p.y+.54467f)<.1f,"Kaki Arthur berada tepat di mesh lantai merah/oranye asli");
    }
    static IEnumerator UjiLantaiMerah()
    {
        yield return DiJalan(6.98f);yield return Pilih("Menuju_GK2",0,"GK2_JalurUtama_BelokOtomatis");float cabang=jalur.JalurAktif.JarakTerdekat(Titik("TitikPersimpangan_GK2").position);
        yield return Jalan(1,29.2f,5);Foto("revisi-01-pilar-tidak-menutupi");
        Check(Mathf.Abs(jalur.kamera.nearClipPlane-1.55f)<.01f,"Bidang dekat kamera mengeluarkan batang depan tanpa memindahkan aset");
        yield return Jalan(1,cabang-1,10);yield return Jalan(1,cabang+1.65f,5);yield return Pilih("GK2_Persimpangan",1,"GK2_JalurBelakang");
        var r=jalur.JalurAktif;PeriksaLantaiMerah();Foto("merah-01-masuk-koridor-asli");
        Check(Mathf.Abs(arthur.transform.position.z+54.9685f)<.1f&&Mathf.Abs(Titik("GK2_Belakang_KembaliKePersimpangan").position.z+54.9685f)<.001f,"Kedatangan, bubble, dan interaksi kembali berada dekat pintu pada gambar kedua");
        var akses=Titik("GK2_Belakang_KembaliKePersimpangan").GetComponent<InteraksiJalurKampus>();
        Check(Mathf.Abs(jalur.kamera.WorldToViewportPoint(akses.ikon.position).x-.5f)<.035f,"Bubble di akses pintu terpusat di atas posisi kedatangan Arthur");
        yield return Tunggu(()=>DialogManager.Instance.idInteraksiAktif=="GK2_Belakang_KembaliKePersimpangan",3,"Akses tengah");pilihan.AksiTombolInteraksi();yield return .4f;yield return FotoMenu("merah-menu-kembali-persimpangan");
        Check(DialogManager.Instance.opsiInteraksiAktif[0].teksOpsi=="Kembali ke persimpangan","Akses tengah mempunyai tujuan persimpangan");pilihan.TekanPilihanLangsung(0);yield return 1.4f;
        Check(jalur.JalurAktif.name=="GK2_JalurUtama_BelokOtomatis"&&Vector3.Distance(arthur.transform.position,Titik("TitikPersimpangan_GK2").position)<.1f,"Akses tengah kembali ke persimpangan GK2");
        yield return Pilih("GK2_Persimpangan",1,"GK2_JalurBelakang");
        yield return Jalan(-1,r.JarakTerdekat(r.titik[12].position),5);Foto("merah-02-tangga-tetap-bisa-dilalui");
        yield return Jalan(-1,0,12);Foto("merah-03-panah-GA-simetris");
        var ikon=Titik("GK2_Belakang_KeGA").GetComponent<InteraksiJalurKampus>().ikon;var teks=ikon.GetComponentInChildren<TMPro.TMP_Text>(true);teks.ForceMeshUpdate();
        float glyph=jalur.kamera.WorldToScreenPoint(teks.transform.TransformPoint(teks.textBounds.center)).x;float latar=jalur.kamera.WorldToScreenPoint(ikon.GetComponent<SpriteRenderer>().bounds.center).x;
        Log("UKUR panah GA selisih horizontal="+Mathf.Abs(glyph-latar)+" px");Check(Mathf.Abs(glyph-latar)<1,"Panah atas terpusat pada bubble walaupun kamera tertahan");Check(Quaternion.Angle(teks.transform.rotation,ikon.rotation)<.01f,"Panah tegak tanpa rotasi tambahan");
        Check(Mathf.Abs(jalur.kamera.transform.position.x-32.10035f)<.01f,"Batas kamera pilihan pengguna dipertahankan");
        yield return Jalan(1,r.JarakTerdekat(Titik("TitikBelakang_GK2").position),12);PeriksaLantaiMerah();Foto("merah-04-tengah-setelah-jalan-balik");
        yield return Jalan(1,r.JarakTerdekat(new Vector3(3.30034f,0,-45.4685f)),5);yield return .5f;
        Check(DialogManager.Instance.idInteraksiAktif!="GK2_Belakang_KembaliKePersimpangan","Lokasi tengah lama tidak lagi menawarkan akses kembali");
        yield return Jalan(1,r.batasJarakIkutKamera+.3f,5);yield return 1f;Foto("revisi-02-batas-kamera-oranye");
        Vector3 posisiBatas=jalur.kamera.transform.position;Quaternion sudutBatas=jalur.kamera.transform.rotation;
        Check(Mathf.Abs(posisiBatas.z+40.4685f)<.1f&&Mathf.Abs(Mathf.DeltaAngle(jalur.kamera.transform.eulerAngles.y,270))<.1f,"Bingkai batas oranye tetap menghadap lurus seperti acuan");
        yield return Jalan(1,r.Panjang,5);PeriksaLantaiMerah();Foto("merah-05-ujung-ke-Jasmine");
        Check(Vector3.Distance(posisiBatas,jalur.kamera.transform.position)<.08f&&Quaternion.Angle(sudutBatas,jalur.kamera.transform.rotation)<.1f,"Kamera berhenti pada bingkai acuan sementara Arthur tetap mencapai ujung");
        Check(Mathf.Abs(arthur.transform.position.z+33.4685f)<.15f,"Arthur mencapai ujung koridor oranye sebelum bangunan berakhir");
        pilihan.AksiTombolInteraksi();yield return .4f;yield return FotoMenu("merah-menu-pulang-Jasmine");Check(DialogManager.Instance.opsiInteraksiAktif[0].teksOpsi=="Ke Depan Asrama Jasmine","Akses ujung tetap berbeda dari akses tengah");pilihan.TekanPilihanLangsung(0);yield return 1.4f;Check(jalur.JalurAktif==null&&Mathf.Abs(arthur.transform.position.x-6.98f)<.1f,"Pulang dari ujung koridor tiba di depan Jasmine");
        yield return UjiLaguPianoHarpa();
        Log("LULUS: masuk di lantai merah, dua akses berbeda, tangga dua arah, panah simetris dan batas kamera tetap.");
    }
    static IEnumerator UjiLaguPianoHarpa()
    {
        var musik=MusikArthur.Instance;var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();
        Check(musik.playlist.lagu.Length==5&&musik.playlist.lagu.Count(l=>l?.audio!=null)==4,"Album memuat empat lagu berisi dan satu slot tersisa");
        var clip=musik.playlist.lagu[2].audio;
        Check(clip.channels==2&&clip.samples==1008000&&Mathf.Abs(clip.length-32*60f/84)<.001f,"Loop stereo memuat tepat delapan bar pada 84 BPM");
        hp.BukaHPDefault();yield return 1.5f;hp.BukaMusik();yield return 1.5f;
        var ui=hp.musik;Check(ui.gameObject.activeInHierarchy&&ui.namaSlot[2].text.Contains("Jeda di Antara Langkah"),"Judul lagu baru terlihat di album HP slot 03");
        ui.PilihSlot(2);ui.Putar();float sampai=Time.realtimeSinceStartup+2.5f,peak=0;var samples=new float[256];int frame=Time.frameCount;
        while(Time.realtimeSinceStartup<sampai){musik.sumber.GetOutputData(samples,0);foreach(float x in samples)peak=Mathf.Max(peak,Mathf.Abs(x));yield return null;}
        Check(Time.frameCount>frame+10&&musik.Memutar&&musik.sumber.time>1&&peak>.005f,"Piano dan harpa benar-benar diputar dari album (peak="+peak+")");
        yield return FotoMenu("album-neon-loop");
        ui.Putar();yield return .2f;Check(musik.Dijeda&&!musik.Memutar,"Lagu baru dapat dijeda lewat HP");
        ui.Putar();yield return .3f;hp.TutupAplikasi();yield return 1.4f;Check(musik.Memutar,"Menutup aplikasi HP tidak memutus lagu");
        if(!musik.ulang)ui.Ulang();yield return .2f;
        Check(musik.sumber.loop,"Tombol ULANG memakai loop sampel native AudioSource");
        // Jalankan dua sambungan alami, bukan hanya melompat ke ujung klip.
        int putaran=0;int sebelumnya=musik.sumber.timeSamples;float batas=Time.realtimeSinceStartup+clip.length*2+4;
        while(putaran<2)
        {
            CheckBerjalan(musik.Memutar,"Audio berhenti saat loop");
            int kini=musik.sumber.timeSamples;
            if(kini<sebelumnya-1000){putaran++;Log("PASS: Sambungan alami ke putaran "+(putaran+1)+"; sumber tetap memutar");}
            sebelumnya=kini;
            if(Time.realtimeSinceStartup>batas)throw new Exception("Loop alami tidak berulang dua kali");
            yield return null;
        }
        yield return .4f;musik.sumber.GetOutputData(samples,0);Check(samples.Any(x=>Mathf.Abs(x)>.003f),"Sinyal audio tetap keluar setelah dua sambungan loop");
        ui.Ulang();yield return .2f;Check(!musik.ulang&&!musik.sumber.loop,"ULANG dapat dimatikan kembali");
        musik.Geser(.9998f);yield return 1f;Check(musik.Pilihan==0&&musik.Memutar,"Dengan ULANG mati, album melanjutkan ke lagu lama");
        musik.HentikanSemua();Log("LULUS: album, output audio, jeda, tutup HP, dua loop alami dan lanjut lagu.");
    }
    static void CheckBerjalan(bool ok,string pesan){if(!ok)throw new Exception(pesan);}
    static IEnumerator UjiLaguEmosional()
    {
        var musik=MusikArthur.Instance;var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();
        var daftar=musik.playlist.lagu;var clip=daftar[3].audio;
        Check(daftar.Length==5&&daftar.Count(l=>l?.audio!=null)==4,"Empat lagu terisi, satu slot album tetap kosong");
        Check(daftar[0].judul=="Jam Pulang - Campus Groove"&&daftar[1].judul=="Jam Pulang - Instrumental"&&daftar[2].judul=="Jeda di Antara Langkah"&&daftar[2].audio.samples==1008000,"Tiga slot lama dan loop cozy dipertahankan");
        Check(clip.channels==2&&clip.samples==2185084&&Mathf.Abs(clip.length-128*60f/155)<.001f,"Lagu emosional stereo: tepat 32 bar pada 155 BPM");
        hp.BukaHPDefault();yield return 1.5f;hp.BukaMusik();yield return 1.5f;var ui=hp.musik;
        Check(ui.gameObject.activeInHierarchy&&ui.namaSlot[3].text.Contains("Blue Neon"),"Slot 04 menampilkan judul Blue Neon dalam album HP");
        ui.PilihSlot(3);if(!musik.ulang)ui.Ulang();ui.Putar();float sampai=Time.realtimeSinceStartup+2f,peak=0;var sampel=new float[256];
        while(Time.realtimeSinceStartup<sampai){musik.sumber.GetOutputData(sampel,0);foreach(float x in sampel)peak=Mathf.Max(peak,Mathf.Abs(x));yield return null;}
        Check(musik.Memutar&&musik.sumber.time>1&&peak>.01f,"Lagu baru benar-benar memutar sinyal (peak="+peak+")");
        ui.judulLagu.ForceMeshUpdate();Check(!ui.judulLagu.isTextOverflowing&&!ui.namaSlot[3].isTextOverflowing,"Judul baru muat pada layar album");
        yield return FotoMenu("album-blue-neon");
        ui.Putar();yield return .3f;Check(musik.Dijeda&&!musik.Memutar,"Tombol jeda lagu baru bekerja");ui.Putar();yield return .3f;
        hp.TutupAplikasi();yield return 1.4f;Check(musik.Memutar&&musik.sumber.loop,"Lagu terus memutar dengan loop native setelah HP ditutup");
        int sebelumnya=musik.sumber.timeSamples;float batas=Time.realtimeSinceStartup+clip.length+3;bool sambung=false;
        while(!sambung){CheckBerjalan(musik.Memutar,"Audio berhenti saat sambungan");int kini=musik.sumber.timeSamples;sambung=kini<sebelumnya-1000;sebelumnya=kini;if(Time.realtimeSinceStartup>batas)throw new Exception("Loop baru tidak menyambung secara alami");yield return null;}
        yield return .4f;musik.sumber.GetOutputData(sampel,0);Check(sampel.Any(x=>Mathf.Abs(x)>.003f),"Sambungan alami ke putaran kedua tetap mengeluarkan audio");
        musik.UbahUlang();yield return .2f;musik.Geser(.9998f);yield return .8f;Check(musik.Pilihan==0&&musik.Memutar,"Matikan ULANG meneruskan album ke lagu lama");
        musik.HentikanSemua();Log("LULUS: lagu emosional terpasang, cozy tetap ada, judul utuh, audio, jeda, loop alami dan urutan album benar.");
    }
    static IEnumerator UjiBatasGA()
    {
        yield return DiJalan(6.98f);yield return Pilih("Menuju_GK2",0,"GK2_JalurUtama_BelokOtomatis");
        float cabang=jalur.JalurAktif.JarakTerdekat(Titik("TitikPersimpangan_GK2").position);yield return Jalan(1,cabang-1,10);yield return Jalan(1,cabang+1.65f,5);
        pilihan.AksiTombolInteraksi();yield return .4f;yield return FotoMenu("gk2-menu-persimpangan");pilihan.TekanPilihanLangsung(0);yield return 1.4f;
        var atas=jalur.JalurAktif;Check(atas.lanjutanAkhir==null,"Batas baru atas menutup jalan ke tangga jauh");
        yield return KeNode(atas,6,1,5);Foto("batas-01-belokan-atas");
        yield return Jalan(1,atas.Panjang,12);yield return TahanArah(1,1);Check(Mathf.Abs(jalur.JarakDiJalur-atas.Panjang)<.02f,"Arthur berhenti di garis batas atas");Foto("batas-02-atas-ke-GA");
        yield return PeriksaGA("GK2_Atas_KeGA","gk2-menu-GA-atas");
        yield return Jalan(-1,0,15);yield return Pilih("GK2_TurunDariTeras",0,"GK2_JalurUtama_BelokOtomatis");yield return Pilih("GK2_Persimpangan",1,"GK2_JalurBelakang");
        var belakang=jalur.JalurAktif;Check(belakang.lanjutanAwal==null,"Batas baru belakang menutup jalan ke tangga penghubung");
        yield return Jalan(-1,belakang.JarakTerdekat(belakang.titik[7].position),12);Foto("batas-03-belokan-belakang");
        yield return Jalan(-1,0,8);yield return TahanArah(-1,1);Check(jalur.JarakDiJalur<.02f,"Arthur berhenti di garis batas belakang");Foto("batas-04-belakang-ke-GA");
        yield return PeriksaGA("GK2_Belakang_KeGA","gk2-menu-GA-belakang");
        yield return Jalan(1,belakang.JarakTerdekat(belakang.titik[13].position),12);Foto("batas-05-sebelum-tangga-pulang");
        yield return Jalan(1,belakang.Panjang,5);yield return TahanArah(1,.5f);Check(Mathf.Abs(jalur.JarakDiJalur-belakang.Panjang)<.02f,"Batas ujung pulang menahan Arthur");Foto("batas-06-kamera-pulang-jelas");
        pilihan.AksiTombolInteraksi();yield return .4f;yield return FotoMenu("gk2-menu-pulang-jasmine");pilihan.TekanPilihanLangsung(0);yield return 1.4f;
        Check(jalur.JalurAktif==null&&Mathf.Abs(arthur.transform.position.x-6.98f)<.1f,"Pulang ke depan Jasmine tetap berfungsi");
        Log("LULUS: batas atas dan belakang, pilihan Ke GA, belokan, tangga, panel teks, serta akses pulang Jasmine.");
    }
    static IEnumerator FotoMenu(string nama)
    {
        UnityEngine.Canvas.ForceUpdateCanvases();
        foreach(var t in pilihan.teksPilihanUtama.Where(t=>t.gameObject.activeInHierarchy))
        {
            t.ForceMeshUpdate();
            Check(!t.isTextOverflowing&&t.textInfo.lineCount==1,"Teks utuh satu baris: "+t.text);
            Check(t.preferredWidth<=t.rectTransform.rect.width+.1f&&t.preferredHeight<=t.rectTransform.rect.height+.1f,"Ruang panel cukup untuk "+t.text);
        }
        string path=Dir+nama+".png";if(File.Exists(path))File.Delete(path);
        ScreenCapture.CaptureScreenshot(path);yield return Tunggu(()=>File.Exists(path),10,"Foto menu "+nama);yield return .3f;
    }
    static IEnumerator LewatiSambungan(JalurKampus tujuan,float arah)
    {
        arthur.kecepatanJalan=8;float deadline=Time.realtimeSinceStartup+12;var awal=arthur.transform.position;
        while(jalur.JalurAktif!=tujuan)
        {
            if(arah>0)arthur.JalanKeKanan();else arthur.JalanKeKiri();
            if(Time.realtimeSinceStartup>deadline)throw new Exception("Sambungan tertahan: "+arthur.transform.position+" route="+jalur.JalurAktif.name);
            yield return null;
        }
        arthur.BerhentiBergerak();yield return .5f;
        Check(Vector3.Distance(arthur.transform.position,awal)<2,"Sambungan dilalui dengan berjalan tanpa loncatan posisi");
        Check(!pilihan.efekTransisi.SedangBerjalan&&!DialogManager.Instance.PercakapanAktif,"Tidak ada teleport atau dialog pada sambungan jalan");
    }
    static IEnumerator UjiSambungan()
    {
        yield return DiJalan(6.98f);yield return Pilih("Menuju_GK2",0,"GK2_JalurUtama_BelokOtomatis");
        float cabang=jalur.JalurAktif.JarakTerdekat(Titik("TitikPersimpangan_GK2").position);yield return Jalan(1,cabang-1,10);yield return Jalan(1,cabang+1.65f,5);
        var opsi=Titik("GK2_Persimpangan").GetComponent<AreaInteraksi>().daftarOpsi;
        Check(opsi.Length==2&&opsi[0].teksOpsi=="Ikut jalan atas"&&opsi[1].teksOpsi=="Ikut jalan belakang","Dua pilihan persimpangan sesuai revisi");
        pilihan.AksiTombolInteraksi();yield return .4f;yield return FotoMenu("gk2-menu-persimpangan");pilihan.TekanPilihanLangsung(0);yield return 1.4f;
        var atas=jalur.JalurAktif;var belakang=atas.lanjutanAkhir;
        Check(atas.name=="GK2_TerasAtas"&&belakang.name=="GK2_JalurBelakang"&&belakang.lanjutanAwal==atas,"Kedua jalur tersambung dua arah");
        Foto("sambung-01-teras-awal");
        yield return KeNode(atas,3,1,5);Foto("sambung-02-atas-mulai-belok");
        yield return KeNode(atas,6,1,5);Foto("sambung-03-atas-selesai-belok");Check(Mathf.Abs(Mathf.DeltaAngle(jalur.kamera.transform.eulerAngles.y,90))<4,"Kamera mengikuti belokan teras");
        yield return KeNode(atas,10);Foto("sambung-04-ujung-sayap-atas");
        yield return KeNode(atas,13,1,5);Foto("sambung-05-belok-kedua-atas");
        yield return Jalan(1,atas.JarakTerdekat(new Vector3(40.3f,0,-104.4685f)),5);Foto("sambung-06-tangga-atas");
        yield return Jalan(1,atas.Panjang-.4f,10);yield return LewatiSambungan(belakang,1);Foto("sambung-07-jalan-tersambung");
        yield return Jalan(1,belakang.JarakTerdekat(new Vector3(12.3f,0,-104.4685f)),5);Foto("sambung-08-tangga-belakang");
        yield return KeNode(belakang,7,1,5);Foto("sambung-09-belakang-mulai-belok");
        yield return KeNode(belakang,11,1,5);Foto("sambung-10-belakang-selesai-belok");Check(Mathf.Abs(Mathf.DeltaAngle(jalur.kamera.transform.eulerAngles.y,270))<4,"Kamera mengikuti belokan belakang");
        yield return KeNode(belakang,13);Foto("sambung-11-sebelum-tangga-hijau");
        yield return KeNode(belakang,20,1,5);Foto("sambung-12-tangga-hijau");
        yield return KeNode(belakang,20,1,5);Foto("sambung-13-akses-pulang-jasmine");
        var pintu=Titik("GK2_KembaliDariBelakang").GetComponent<InteraksiJalurKampus>();Check(pintu.ikon.localScale.x>.1f,"Bubble pulang terlihat di ujung dekat lingkaran hijau");
        pilihan.AksiTombolInteraksi();yield return .4f;yield return FotoMenu("gk2-menu-pulang-jasmine");pilihan.TekanPilihanLangsung(0);yield return 1.4f;
        Check(jalur.JalurAktif==null&&Mathf.Abs(arthur.transform.position.x-6.98f)<.1f,"Akses ujung belakang pulang ke depan Jasmine");
        yield return DiJalan(6.98f);yield return Pilih("Menuju_GK2",0,"GK2_JalurUtama_BelokOtomatis");yield return Jalan(1,cabang-1,10);yield return Jalan(1,cabang+1.65f,5);yield return Pilih("GK2_Persimpangan",1,"GK2_JalurBelakang");
        yield return Jalan(-1,.4f,15);yield return LewatiSambungan(atas,-1);yield return KeNode(atas,0,-1,15);
        Check(Vector3.Distance(arthur.transform.position,atas.titik[0].position)<.18f,"Perjalanan balik melewati kedua belokan dan tangga sampai akses turun");Foto("sambung-14-balik-ke-teras");
        yield return Pilih("GK2_TurunDariTeras",0,"GK2_JalurUtama_BelokOtomatis");
        Log("LULUS: belokan atas dan belakang, tangga, sambungan dua arah, pilihan terbaca, dan pulang Jasmine.");
    }
    static void PeriksaBubbleTeras()
    {
        var ikon=Titik("GK2_TurunDariTeras").GetComponent<InteraksiJalurKampus>().ikon;
        var bounds=ikon.GetComponent<SpriteRenderer>().bounds;var kepala=arthur.GetComponent<KetukKepalaArthur>().kepala.bounds;var c=jalur.kamera;
        Log("UKUR TERAS horizontal ikon="+c.WorldToScreenPoint(ikon.position)+" kepala="+kepala+" camera="+c.transform.position);
        Check(DiAtasLebarKepala(ikon.position,kepala,c),"Bubble turun berada di atas kepala pada titik teleportasi");
        float jarak=bounds.min.y-kepala.max.y;Log("UKUR TERAS gap="+jarak+" headOffset="+(kepala.max.y-arthur.transform.position.y));Check(jarak>.25f&&jarak<.5f,"Bubble turun dekat kepala pada titik teleportasi");
        foreach(float x in new[]{bounds.min.x,bounds.max.x})foreach(float y in new[]{bounds.min.y,bounds.max.y}){var v=c.WorldToViewportPoint(new Vector3(x,y,bounds.center.z));Check(v.x>.02f&&v.x<.98f&&v.y>.02f&&v.y<.98f,"Bubble turun berada utuh dalam bingkai");}
    }
    static void PeriksaPenandaJalurTetap(string nama)
    {
        var b=Titik(nama).GetComponent<InteraksiJalurKampus>();var kepala=arthur.GetComponent<KetukKepalaArthur>().kepala.bounds;var c=jalur.kamera;var bounds=b.ikon.GetComponent<SpriteRenderer>().bounds;
        Check(DiAtasLebarKepala(b.ikon.position,kepala,c),nama+" berada di atas lebar kepala pada titik interaksi, termasuk saat Arthur menghadap kiri");
        var teks=b.ikon.GetComponentInChildren<TMPro.TMP_Text>(true);teks.ForceMeshUpdate();
        Log("UKUR "+nama+" gap="+(bounds.min.y-kepala.max.y)+" headOffset="+(kepala.max.y-arthur.transform.position.y)+" ikon="+b.ikon.position+" labelLocal="+teks.rectTransform.anchoredPosition3D+" labelRot="+teks.transform.localEulerAngles+" active="+teks.gameObject.activeInHierarchy+" enabled="+teks.enabled+" color="+teks.color+" size="+teks.fontSize+" chars="+teks.textInfo.characterCount+" glyphBounds="+teks.textBounds);
        Check(bounds.min.y-kepala.max.y>.2f&&bounds.min.y-kepala.max.y<.5f,nama+" mempunyai jarak dekat di atas kepala");
        var p=c.WorldToViewportPoint(bounds.max);Check(p.y<.8f,nama+" tidak berada di ujung atas atau menabrak HUD");
    }
    static bool DiAtasLebarKepala(Vector3 posisi,Bounds kepala,Camera kamera)
    {
        float kiri=float.PositiveInfinity,kanan=float.NegativeInfinity;
        foreach(float x in new[]{kepala.min.x,kepala.max.x})foreach(float y in new[]{kepala.min.y,kepala.max.y})foreach(float z in new[]{kepala.min.z,kepala.max.z}){float p=kamera.WorldToScreenPoint(new Vector3(x,y,z)).x;kiri=Mathf.Min(kiri,p);kanan=Mathf.Max(kanan,p);}
        float ikon=kamera.WorldToScreenPoint(posisi).x;return ikon>=kiri-6&&ikon<=kanan+6;
    }
    static float PeriksaPenanda(string nama, bool ikuti)
    {
        var b=GameObject.Find("FisikAreaDepan/Npc_Dan_TriggerDepan/"+nama).GetComponent<TempatBubbleTrigger>();var c=jalur.kamera;
        var kepala=arthur.GetComponent<KetukKepalaArthur>().kepala.bounds;
        var h=c.WorldToScreenPoint(kepala.center);var ikon=c.WorldToScreenPoint(b.ikonTransform.position);
        Check(b.ikonTransform.localScale.x>.13f,nama+" terlihat");
        if(ikuti)Check(Mathf.Abs(h.x-ikon.x)<1.5f,nama+" terpusat pada kepala; selisih "+Mathf.Abs(h.x-ikon.x)+" px");
        var bounds=b.ikonTransform.GetComponent<SpriteRenderer>().bounds;
        float gap=(c.WorldToScreenPoint(new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)).y-c.WorldToScreenPoint(new Vector3(kepala.center.x,kepala.max.y,kepala.center.z)).y)/c.pixelHeight;
        Log(nama+" jarak kepala ke bubble = "+gap+" tinggi layar");
        Check(gap>.015f&&gap<.13f,nama+" mempunyai jarak yang wajar di atas kepala");return gap;
    }
    static IEnumerator UjiPenanda()
    {
        yield return DiJalan(-21.03f);float gh=PeriksaPenanda("TriggerTempatKeAsramaGH",true);Foto("penanda-gh-rapi");
        yield return TahanArah(1,.4f);PeriksaPenanda("TriggerTempatKeAsramaGH",true);
        yield return DiJalan(91.80034f);float kantin=PeriksaPenanda("TriggerTempatKeKantin",true);Foto("penanda-kantin-rapi");
        yield return TahanArah(-1,.4f);PeriksaPenanda("TriggerTempatKeKantin",true);
        yield return DiJalan(59.42f);float acuan=PeriksaPenanda("TriggerTempatKeHalamanAsramaJasmine2",false);Foto("penanda-acuan-tetap");
        Check(Mathf.Abs(gh-acuan)<.012f&&Mathf.Abs(kantin-acuan)<.012f,"Jarak GH dan Kantin konsisten dengan penanda gambar ketiga");
        Log("LULUS: penanda terpusat saat diam dan berjalan, dengan jarak konsisten.");
    }
    static IEnumerator UjiAkses()
    {
        yield return DiJalan(91.80034f);
        Check(DialogManager.Instance.idInteraksiAktif=="Menuju_Kantin","Akses Kantin aktif tepat di persimpangan paving");
        Foto("akses-01-kantin-persimpangan");
        yield return Pilih("Menuju_Kantin",0,"Kantin_LorongTengah");
        yield return Jalan(1,jalur.JalurAktif.Panjang,5);
        yield return Pilih("Kantin_KembaliKeJasmine",0,null);
        Check(Mathf.Abs(arthur.transform.position.x-91.80034f)<.1f,"Pulang Kantin tiba di persimpangan yang sama");
        yield return DiJalan(59.42f);
        yield return Tunggu(()=>DialogManager.Instance.idInteraksiAktif=="MasukJasmine2",3,"Pintu pada gambar kedua");
        Check(DialogManager.Instance.opsiInteraksiAktif.Length==2&&DialogManager.Instance.opsiInteraksiAktif[1].teksOpsi=="Ke GA","Pintu gambar kedua menampilkan Masuk dan Ke GA");
        pilihan.AksiTombolInteraksi();yield return .35f;Vector3 awal=arthur.transform.position;
        pilihan.TekanPilihanLangsung(1);yield return .3f;
        Check(DialogManager.Instance.PercakapanAktif&&Vector3.Distance(awal,arthur.transform.position)<.05f&&pilihan.teksCeritaUtama.text.Contains("belum tersedia"),"Ke GA memberi keterangan tanpa memindahkan Arthur");
        pilihan.TekanKeluarLangsung();yield return .6f;
        yield return DiJalan(6.98f);
        Check(DialogManager.Instance.opsiInteraksiAktif.Length==2&&DialogManager.Instance.opsiInteraksiAktif[1].teksOpsi=="Ke Asrama Jasmine","Percabangan gambar ketiga mempunyai Ke GK2 dan Ke Asrama Jasmine");
        yield return Pilih("Menuju_GK2",1,null);
        Check(pilihan.grupFisikBelakang.activeInHierarchy&&!pilihan.grupFisikDepan.activeInHierarchy,"Ke Asrama Jasmine memakai halaman Jasmine yang sudah ada");
        Foto("akses-03-asrama-jasmine");
        yield return DiJalan(-13.4f);
        Check(DialogManager.Instance.idInteraksiAktif=="Menuju_AsramaGH","Akses GH terjangkau pada ujung kiri jalan");
        Foto("akses-04-gh-ujung");
        var marker=GameObject.Find("FisikAreaDepan/Npc_Dan_TriggerDepan/TriggerTempatKeAsramaGH").GetComponent<TempatBubbleTrigger>().ikonTransform.GetComponent<SpriteRenderer>();
        Bounds bounds=marker.bounds;
        foreach(float x in new[]{bounds.min.x,bounds.max.x})foreach(float y in new[]{bounds.min.y,bounds.max.y})
        {var p=jalur.kamera.WorldToViewportPoint(new Vector3(x,y,bounds.center.z));Check(p.x>.02f&&p.x<.98f&&p.y>.02f&&p.y<.98f,"Seluruh tepi bubble GH berada di dalam layar");}
        pilihan.AksiTombolInteraksi();yield return .35f;awal=arthur.transform.position;
        pilihan.TekanPilihanLangsung(0);yield return .3f;
        Check(DialogManager.Instance.PercakapanAktif&&Vector3.Distance(awal,arthur.transform.position)<.05f&&pilihan.teksCeritaUtama.text.Contains("belum tersedia"),"Ke Asrama GH menunggu world tersedia tanpa teleport kosong");
        pilihan.TekanKeluarLangsung();yield return .6f;
        Check(!DialogManager.Instance.PercakapanAktif&&!arthur.kunciPergerakan,"Keluar dari pilihan sementara memulihkan pergerakan");
        Log("LULUS: akses Kantin di persimpangan, pulang ke titik yang sama, GA sementara, tujuan Jasmine, dan bubble GH utuh.");
    }
    static IEnumerator UjiPeta20261025()
    {
        var world=GameObject.Find("World_JasmineKantinGK2");
        Check(UnityEditor.AssetDatabase.GetAssetPath(world.GetComponentInChildren<MeshFilter>().sharedMesh).Contains("Revisi20261025"),"CharacterLab memakai hasil ekspor Mineways terbaru");
        Check(world.GetComponentsInChildren<MeshRenderer>().Length==290,"Seluruh 290 kelompok material world terbaru terpasang");
        yield return DiJalan(6.98f);Foto("peta-00-jalan-Jasmine");yield return Pilih("Menuju_GK2",0,"GK2_JalurUtama_BelokOtomatis");
        float cabang=jalur.JalurAktif.JarakTerdekat(Titik("TitikPersimpangan_GK2").position);
        yield return Jalan(1,cabang-1,10);yield return Jalan(1,cabang+1.65f,5);yield return Pilih("GK2_Persimpangan",1,"GK2_JalurBelakang");
        var r=jalur.JalurAktif;PeriksaLantaiMerah();
        yield return Jalan(-1,r.JarakTerdekat(new Vector3(5.3f,0,-90.4685f)),10);yield return .6f;
        Foto("peta-01-pohon-di-belakang");ScreenCapture.CaptureScreenshot(Dir+"peta-layar-01-pohon.png");yield return .2f;
        yield return Jalan(-1,r.JarakTerdekat(r.titik[8].position),6);yield return .6f;
        Foto("peta-02-tong-air-dan-tikungan");ScreenCapture.CaptureScreenshot(Dir+"peta-layar-02-tong-air.png");yield return .2f;
        yield return Jalan(-1,0,12);yield return PeriksaGA("GK2_Belakang_KeGA","peta-menu-GA");
        Check(Mathf.Abs(jalur.kamera.transform.position.x-r.batasKananKamera)<.03f,"Batas kamera di sisi GA tetap dipertahankan");
        yield return Jalan(1,r.batasJarakIkutKamera+.15f,12);yield return .8f;
        var posisi=jalur.kamera.transform.position;var sudut=jalur.kamera.transform.rotation;
        Check(Vector3.Distance(posisi,new Vector3(11.30034f,-.6f,-41.8f))<.03f,"Kamera sudah mencapai bingkai pulang sebelum Arthur mencapai ujung");
        Check(Quaternion.Angle(sudut,Quaternion.Euler(0,270,0))<.1f,"Bingkai pulang menghadap lurus seperti screenshot");
        Foto("peta-03-bingkai-batas-pulang");
        yield return Jalan(1,r.Panjang,5);yield return TahanArah(1,.4f);yield return .4f;
        Check(Vector3.Distance(posisi,jalur.kamera.transform.position)<.03f&&Quaternion.Angle(sudut,jalur.kamera.transform.rotation)<.1f,"Kamera tetap berhenti ketika Arthur berjalan sampai akses Jasmine");
        Check(Mathf.Abs(arthur.transform.position.z+33.4685f)<.15f,"Batas jalan Arthur tidak ikut dipendekkan");
        Foto("peta-04-Arthur-di-ujung");ScreenCapture.CaptureScreenshot(Dir+"peta-layar-04-batas-pulang.png");yield return .2f;
        var bubble=Titik("GK2_KembaliDariBelakang").GetComponent<InteraksiJalurKampus>().ikon.GetComponent<SpriteRenderer>().bounds;
        foreach(float x in new[]{bubble.min.x,bubble.max.x})foreach(float y in new[]{bubble.min.y,bubble.max.y})foreach(float z in new[]{bubble.min.z,bubble.max.z})
        {var p=jalur.kamera.WorldToViewportPoint(new Vector3(x,y,z));Check(p.z>0&&p.x>.005f&&p.x<.995f&&p.y>.005f&&p.y<.995f,"Bubble pulang tetap utuh setelah kamera berhenti");}
        yield return Pilih("GK2_KembaliDariBelakang",0,null);Check(Mathf.Abs(arthur.transform.position.x-6.98f)<.1f,"Akses pulang tetap tiba di Jasmine");
        yield return UjiKantinMerah();
        Log("LULUS: world baru, jalan belakang, tikungan, dua batas kamera, akses pulang, Jasmine dan Kantin.");
    }
    static IEnumerator UjiKantinMerah()
    {
        yield return DiJalan(91.80034f);yield return Pilih("Menuju_Kantin",0,"Kantin_LorongTengah");
        var r=jalur.JalurAktif;var c=jalur.kamera;
        Check(r.titik.Length==2&&Mathf.Abs(r.Panjang-7)<.001f&&r.titik.All(t=>Mathf.Abs(t.position.x-91.30034f)<.001f),"Jalur lurus tujuh satuan mengikuti garis merah, dengan X tetap");
        Check(r.kameraTetap&&r.arahKamera.All(y=>y==270)&&r.putaranVisualSaatBelok==0,"Kamera kantin menghadap lurus dari sisi kanan world");
        Check(Mathf.Abs(jalur.JarakDiJalur-r.Panjang)<.05f,"Masuk kantin tiba di ujung kanan dekat akses jalan");
        var ga=Titik("Kantin_PergiKeGA").GetComponent<InteraksiJalurKampus>();var pulang=Titik("Kantin_KembaliKeJasmine").GetComponent<InteraksiJalurKampus>();
        Check(!ga.ikutiKepalaArthur&&!pulang.ikutiKepalaArthur&&ga.ikon.GetComponentInChildren<TMPro.TMP_Text>(true).text=="←"&&pulang.ikon.GetComponentInChildren<TMPro.TMP_Text>(true).text=="→","Dua bubble menetap dengan panah kiri dan kanan");
        var g0=ga.ikon.position;var p0=pulang.ikon.position;var camera0=c.transform.position;var rotation0=c.transform.rotation;
        Check(c.WorldToViewportPoint(ga.ikon.position).x<c.WorldToViewportPoint(pulang.ikon.position).x,"Akses GA tampil di kiri dan akses jalan di kanan");
        Foto("kantin-01-tiba-kanan");yield return FotoMenu("kantin-layar-01-tiba-kanan");
        int rendered=0;float maxError=0;
        Action<Camera> amati=cam=>{if(cam==c){rendered++;maxError=Mathf.Max(maxError,Vector3.Distance(cam.transform.position,camera0));}};
        Camera.CameraCallback builtin=cam=>amati(cam);
        Action<UnityEngine.Rendering.ScriptableRenderContext,Camera> srp=(ctx,cam)=>amati(cam);
        Camera.onPreCull+=builtin;UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering+=srp;
        try
        {
            yield return Jalan(-1,r.Panjang*.5f,3);Foto("kantin-02-tengah-garis-merah");yield return FotoMenu("kantin-layar-02-tengah");
            Check(Mathf.Abs(arthur.transform.position.y-ArthurJalurKampus.JarakKaki+1.3f)<.08f,"Kaki Arthur tetap pada tinggi lantai putih asli");
            Check(DialogManager.Instance.idInteraksiAktif!="Kantin_PergiKeGA"&&DialogManager.Instance.idInteraksiAktif!="Kantin_KembaliKeJasmine","Tengah jalur tidak memicu akses ujung");
            yield return Jalan(-1,0,3);yield return TahanArah(-1,.7f);Check(jalur.JarakDiJalur<.03f&&arthur.transform.position.z>=-27.01f,"Batas kiri menahan Arthur di ujung merah");
            Foto("kantin-03-ujung-GA");yield return PeriksaGA("Kantin_PergiKeGA","kantin-menu-pergi-GA","Pergi ke GA");
            yield return Jalan(1,r.Panjang,3);yield return TahanArah(1,.7f);Check(Mathf.Abs(jalur.JarakDiJalur-r.Panjang)<.03f&&arthur.transform.position.z<=-19.93f,"Batas kanan menahan Arthur di ujung merah");
            Check(rendered>10&&maxError<.005f&&Quaternion.Angle(c.transform.rotation,rotation0)<.01f,"Kamera tetap selama gerak, diam, dan interaksi (frame="+rendered+", selisih="+maxError+")");
        }
        finally{Camera.onPreCull-=builtin;UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering-=srp;}
        Check(Vector3.Distance(g0,ga.ikon.position)<.001f&&Vector3.Distance(p0,pulang.ikon.position)<.001f,"Bubble tetap di dua ujung saat Arthur berjalan");
        yield return Tunggu(()=>DialogManager.Instance.idInteraksiAktif=="Kantin_KembaliKeJasmine",3,"Akses jalan di kanan");
        pilihan.AksiTombolInteraksi();yield return .4f;yield return FotoMenu("kantin-menu-kembali-jalan");
        Check(DialogManager.Instance.opsiInteraksiAktif.Length==1&&DialogManager.Instance.opsiInteraksiAktif[0].teksOpsi=="Kembali ke jalan","Akses kanan mempunyai pilihan Kembali ke jalan");
        pilihan.TekanPilihanLangsung(0);yield return 1.5f;
        Check(jalur.JalurAktif==null&&arthur.GetComponent<Rigidbody2D>().simulated&&!jalur.colliderJalur.enabled&&Mathf.Abs(arthur.transform.position.x-91.80034f)<.1f,"Kembali ke jalan tiba di persimpangan Jasmine yang sama, dengan fisika pulih");
        Check(Mathf.Abs(c.fieldOfView-60)<.02f&&Quaternion.Angle(c.transform.localRotation,Quaternion.Euler(1.5f,0,0))<.1f,"Kamera Jasmine pulih setelah keluar dari bingkai tetap kantin");
        Foto("kantin-04-kembali-ke-jalan");Log("LULUS: garis merah, kiri-kanan, kamera tetap, dua bubble, GA sementara dan pulang ke jalan.");
    }
    static void Foto(string nama)
    {
        var camera = jalur.kamera;
        var target = RenderTexture.GetTemporary(1280,720,24); var aktif = RenderTexture.active; var lama = camera.targetTexture;
        var pixels = new Texture2D(1280,720,TextureFormat.RGB24,false);
        try { camera.targetTexture = target; camera.Render(); RenderTexture.active = target; pixels.ReadPixels(new Rect(0,0,1280,720),0,0); pixels.Apply(); File.WriteAllBytes(Dir + nama + ".png", pixels.EncodeToPNG()); }
        finally { camera.targetTexture = lama; RenderTexture.active = aktif; RenderTexture.ReleaseTemporary(target); UnityEngine.Object.DestroyImmediate(pixels); }
        var kepala = arthur.GetComponent<KetukKepalaArthur>().kepala;
        var p = camera.WorldToViewportPoint(kepala.bounds.center);
        Check(p.z > 0 && p.x > .05f && p.x < .95f && p.y > .05f && p.y < .95f, "Arthur berada dalam bingkai " + nama);
        Log("FOTO " + nama + " player=" + arthur.transform.position + " camera=" + camera.transform.position + " yaw=" + camera.transform.eulerAngles.y);
    }
}
