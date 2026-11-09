// PANDUAN PROYEK - Runtime / Ponsel/Sistem
// Fungsi: Pusat buka/tutup HP, navigasi ikon, pembukaan aplikasi, dan pembagian input saat aplikasi aktif.
// Panduan: Assets/Scripts/Runtime/Ponsel/Sistem/README.md

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SistemHP : MonoBehaviour
{
    [Header("Referensi Komponen Utama")]
    public GameObject hpUtama; 
    public Image badanHP; 
    public Image layarGelap;
    public Button tombolIkonHP;
    public Button tombolBackgroundLuar; 

    [Header("Aplikasi Pengaturan")]
    public GameObject panelPengaturan;
    public TransisiAplikasiTV transisiAplikasi;
    public GameObject panelSegeraHadir;
    public TMP_Text judulSegeraHadir;
    [Header("Aplikasi Peta")]
    public PetaKampusHP petaKampus;
    public PoseMemegangHP poseKarakter;
    public TransformasiLayarHP transformasiLayar;
    [Header("Aplikasi Arthur")]
    public AplikasiMusikHP musik;
    public AplikasiCatatanHP tugas, pesan, tas;
    public RhythmArthur gameRhythm;
    public PonselAplikasiBersama ponselBersama;
    public bool GameSedangBuka=>gameRhythm!=null&&aplikasiAktif==gameRhythm.panel;
    private GameObject aplikasiAktif;
    private bool sedangMembukaAplikasi;
    public bool AplikasiSedangBuka => sedangMembukaAplikasi || (transisiAplikasi != null && transisiAplikasi.SedangBerjalan) || (aplikasiAktif != null && aplikasiAktif.activeInHierarchy);
    // Tetap jeda hingga animasi penutupan HP benar-benar selesai.
    public bool MenjedaParameter => hpSedangBuka || (hpUtama != null && hpUtama.activeInHierarchy) || AplikasiSedangBuka;

    [Header("Pengaturan Tampilan & Animasi")]
    public Color warnaDefaultHP = new Color(1f, 0.5f, 0f, 1f); 
    public float durasiMuncul = 0.3f;
    
    [Tooltip("Waktu tunggu (jeda) sebelum layar hitam mulai memudar")]
    public float jedaSebelumLayarNyala = 0.1f; // <-- ATUR JADI 0 ATAU 0.1 BIAR NGEBUT!
    
    [Tooltip("Kecepatan layar hitam memudar menjadi terang")]
    public float durasiLayarNyala = 0.2f; 
    public float durasiTutup = 0.2f; 

    [Header("Sistem Navigasi Ikon")]
    public List<GameObject> daftarIkon = new List<GameObject>();
    private int indeksIkonAktif = 0;
    
    public bool hpSedangBuka = false;
    private bool bisaNavigasi = false; 

    void Start()
    {
        if (layarGelap != null) layarGelap.raycastTarget = false;
        if (hpUtama != null)
        {
            hpUtama.transform.localScale = Vector3.zero;
            hpUtama.SetActive(false);
        }

        if (tombolBackgroundLuar != null)
        {
            tombolBackgroundLuar.gameObject.SetActive(false);
            tombolBackgroundLuar.onClick.AddListener(KlikDiLuarLayar);
        }

        MatikanSemuaHighlightIkon();
        AturTampilanIkonGame(false);
    }

    void Update()
    {
        if (AplikasiSedangBuka) return;
        if (hpSedangBuka && bisaNavigasi)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) TekanTombolUIKanan();
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) TekanTombolUIKiri();
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) TekanTombolUIInteract();
        }
    }

    public void BukaHPDenganWarna(Color warnaKarakter)
    {
        if(DialogManager.Instance!=null&&DialogManager.Instance.minigameAktif)return;
        if (DialogManager.Instance != null && DialogManager.Instance.canvasDialog != null && DialogManager.Instance.canvasDialog.activeInHierarchy)
        {
            return; 
        }

        if (hpSedangBuka) return;

        hpSedangBuka = true;
        BunyiTombolUI.MainkanBukaHP();
        bisaNavigasi = false; 
        indeksIkonAktif = 0; 

        if (badanHP != null) badanHP.color = warnaKarakter;
        var latar = hpUtama.transform.Find("LayarTerang")?.GetComponent<UnityEngine.UI.Image>();
        if (latar != null) latar.color = Color.Lerp(warnaKarakter,new Color(.81f,.78f,.69f,1f),.85f);
        if (tombolIkonHP != null) tombolIkonHP.gameObject.SetActive(false); 

        // Kunci Arthur
        if (DialogManager.Instance != null && DialogManager.Instance.arthurScript != null)
        {
            DialogManager.Instance.arthurScript.BerhentiBergerak();
            DialogManager.Instance.arthurScript.kunciPergerakan = true;
            PoseMemegangHP poseAktif = DialogManager.Instance.arthurScript.GetComponent<PoseMemegangHP>();
            if (poseAktif != null) poseKarakter = poseAktif;
        }
        if (poseKarakter != null) poseKarakter.Buka(warnaKarakter);

        MatikanSemuaHighlightIkon();
        
        foreach(GameObject ikon in daftarIkon)
        {
             if(ikon != null) ikon.SetActive(false);
        }

        StartCoroutine(AnimasiMekarDanNyala());
    }

    public void BukaHPDefault()
    {
        BukaHPDenganWarna(warnaDefaultHP); 
    }

    public void TutupHP()
    {
        if (!hpSedangBuka || AplikasiSedangBuka || !bisaNavigasi) return;
        bisaNavigasi = false;
        SelesaikanTutupHP();
    }

    private void SelesaikanTutupHP()
    {
        
        hpSedangBuka = false;
        if (poseKarakter != null) poseKarakter.Tutup();
        bisaNavigasi = false;
        if (tombolBackgroundLuar != null) tombolBackgroundLuar.gameObject.SetActive(false);
        
        MatikanSemuaHighlightIkon(); 

        // Bebaskan Arthur
        if (DialogManager.Instance != null && DialogManager.Instance.arthurScript != null)
        {
            if (!DialogManager.Instance.canvasDialog.activeInHierarchy)
            {
                DialogManager.Instance.arthurScript.kunciPergerakan = false;
            }
        }

        StartCoroutine(AnimasiKecilDanTutup()); 
    }

    public void KlikDiLuarLayar()
    {
        if (hpSedangBuka && !AplikasiSedangBuka) TutupHP();
    }

    private void PindahKursor(int arah)
    {
        SetHighlightIkon(indeksIkonAktif, false);
        indeksIkonAktif += arah;

        if (indeksIkonAktif >= daftarIkon.Count) indeksIkonAktif = 0;
        else if (indeksIkonAktif < 0) indeksIkonAktif = daftarIkon.Count - 1;

        SetHighlightIkon(indeksIkonAktif, true);
    }

    private void SetHighlightIkon(int indeks, bool statusNyala)
    {
        if (indeks >= 0 && indeks < daftarIkon.Count && daftarIkon[indeks] != null)
        {
            Transform kotakHijau = daftarIkon[indeks].transform.Find("TelahTerpilih");
            if (kotakHijau != null) kotakHijau.gameObject.SetActive(statusNyala);
        }
    }

    private void MatikanSemuaHighlightIkon()
    {
        for (int i = 0; i < daftarIkon.Count; i++) SetHighlightIkon(i, false);
    }

    private void AturTampilanIkonGame(bool terlihat)
    {
        GameObject ikonGame = daftarIkon.Find(ikon => ikon != null && ikon.name == "IkonGame");
        Transform gambarController = ikonGame != null ? ikonGame.transform.Find("GamepadCoretan") : null;
        if (gambarController != null) gambarController.gameObject.SetActive(terlihat);
    }

    private void BukaAplikasiTerpilih()
    {
        if (indeksIkonAktif < 0 || indeksIkonAktif >= daftarIkon.Count || daftarIkon[indeksIkonAktif] == null) return;
        BukaAplikasi(daftarIkon[indeksIkonAktif].name);
    }

    public void BukaPengaturan()
    {
        BukaAplikasi("IkonSetting");
    }
    public void BukaMusik(){BukaAplikasi("IkonMusic");}

    public void BukaAplikasi(string namaIkon)
    {
        if (!hpSedangBuka || !bisaNavigasi || AplikasiSedangBuka) return;
        if (namaIkon == "IkonKeluar") { TutupHP(); return; }
        GameObject tujuan = namaIkon == "IkonMap" && petaKampus != null ? petaKampus.gameObject : namaIkon == "IkonSetting" ? panelPengaturan : panelSegeraHadir;
        if(namaIkon=="IkonMusic" && musik!=null)tujuan=musik.gameObject;
        else if(namaIkon=="IkonToDoLIst" && tugas!=null)tujuan=tugas.gameObject;
        else if(namaIkon=="IkonPesan" && pesan!=null)tujuan=pesan.gameObject;
        else if(namaIkon=="IkonTas" && tas!=null)tujuan=tas.gameObject;
        else if(namaIkon=="IkonGame" && gameRhythm!=null)tujuan=gameRhythm.panel;
        if (tujuan == null) { Debug.LogWarning("Panel aplikasi belum terhubung: " + namaIkon, this); return; }
        int indeks = daftarIkon.FindIndex(ikon => ikon != null && ikon.name == namaIkon);
        if (indeks >= 0)
        {
            SetHighlightIkon(indeksIkonAktif, false);
            indeksIkonAktif = indeks;
            SetHighlightIkon(indeksIkonAktif, true);
        }
        if (judulSegeraHadir != null && tujuan == panelSegeraHadir)
            judulSegeraHadir.text = namaIkon.Replace("Ikon", "").Replace("ToDoLIst", "TO-DO").ToUpperInvariant();
        sedangMembukaAplikasi = true;
        System.Action tampilkan = () =>
        {
            aplikasiAktif = tujuan;
            tujuan.SetActive(true);
            if(gameRhythm!=null&&tujuan==gameRhythm.panel)gameRhythm.Buka();
            sedangMembukaAplikasi = false;
        };
        StartCoroutine(BukaLandscape(tujuan,tampilkan));
    }

    private IEnumerator BukaLandscape(GameObject tujuan,System.Action tampilkan)
    {
        var frame=BidangAplikasi(tujuan);
        if(ponselBersama!=null){ponselBersama.Warna(badanHP.color);ponselBersama.Sesuaikan();}
        else {var bingkai=frame.GetComponent<UnityEngine.UI.Image>();if(bingkai!=null&&badanHP!=null)bingkai.color=badanHP.color;}
        if(transformasiLayar!=null)yield return transformasiLayar.KeLandscape(ponselBersama!=null?(RectTransform)ponselBersama.transform:frame);
        if (transisiAplikasi != null) { transisiAplikasi.AturBidang(frame); transisiAplikasi.Buka(tampilkan); }
        else tampilkan();
    }

    public void TutupAplikasi()
    {
        if (sedangMembukaAplikasi || (transisiAplikasi != null && transisiAplikasi.SedangBerjalan)) return;
        if (aplikasiAktif == null) return;
        if(GameSedangBuka)gameRhythm.Jeda();
        sedangMembukaAplikasi = true;
        System.Action tutup = () =>
        {
            if(GameSedangBuka)gameRhythm.TutupDariHP();
            if (aplikasiAktif != null) aplikasiAktif.SetActive(false);
            aplikasiAktif = null;
        };
        StartCoroutine(KembaliPortrait(tutup));
    }

    private IEnumerator KembaliPortrait(System.Action tutup)
    {
        if(transisiAplikasi!=null)
        {
            transisiAplikasi.AturBidang(BidangAplikasi(aplikasiAktif));transisiAplikasi.Tutup(tutup);
            while(transisiAplikasi.SedangBerjalan)yield return null;
        }
        else tutup();
        if(transformasiLayar!=null)yield return transformasiLayar.KePortrait();
        sedangMembukaAplikasi=false;
    }

    public void TekanTombolUIKanan() { if(!hpSedangBuka || !bisaNavigasi || TransisiAktif() || GameSedangBuka)return;BunyiTombolUI.MainkanKetukHP();if (PetaAktif()) petaKampus.PilihKanan();else if(aplikasiAktif==musik?.gameObject && aplikasiAktif!=null)musik.Kanan();else if(CatatanAktif()!=null)CatatanAktif().Kanan();else if (hpSedangBuka && bisaNavigasi && !AplikasiSedangBuka) PindahKursor(1); }
    public void TekanTombolUIKiri() { if(!hpSedangBuka || !bisaNavigasi || TransisiAktif() || GameSedangBuka)return;BunyiTombolUI.MainkanKetukHP();if (PetaAktif()) petaKampus.PilihKiri();else if(aplikasiAktif==musik?.gameObject && aplikasiAktif!=null)musik.Kiri();else if(CatatanAktif()!=null)CatatanAktif().Kiri();else if (hpSedangBuka && bisaNavigasi && !AplikasiSedangBuka) PindahKursor(-1); }
    bool TransisiAktif()=>sedangMembukaAplikasi || (transisiAplikasi!=null && transisiAplikasi.SedangBerjalan);
    AplikasiCatatanHP CatatanAktif()=>aplikasiAktif==null?null:aplikasiAktif==tugas?.gameObject?tugas:aplikasiAktif==pesan?.gameObject?pesan:aplikasiAktif==tas?.gameObject?tas:null;
    private bool PetaAktif() => petaKampus != null && petaKampus.gameObject.activeInHierarchy;
    private RectTransform BidangAplikasi(GameObject app)
    {
        if (app == null) return null;
        if(ponselBersama!=null)return ponselBersama.layar;
        Transform frame = app.transform.Find("BingkaiHP") ?? app.transform.Find("BingkaiMengambang") ?? app.transform.Find("Box_Pengaturan") ?? app.transform.Find("KertasAplikasi");
        return (frame != null ? frame : app.transform) as RectTransform;
    }
    public void TekanTombolUIInteract() 
    { 
        if(!hpSedangBuka || !bisaNavigasi || TransisiAktif() || GameSedangBuka)return;
        if (DialogManager.Instance != null && DialogManager.Instance.canvasDialog != null && DialogManager.Instance.canvasDialog.activeInHierarchy) return;
        BunyiTombolUI.MainkanKetukHP();
        if (PetaAktif()) petaKampus.Interaksi();
        else if(aplikasiAktif==musik?.gameObject && aplikasiAktif!=null)musik.Putar();
        else if(CatatanAktif()!=null)CatatanAktif().Interaksi();
        else if (hpSedangBuka && bisaNavigasi && !AplikasiSedangBuka) BukaAplikasiTerpilih();
    }

    private IEnumerator AnimasiMekarDanNyala()
    {
        hpUtama.SetActive(true);
        if (tombolBackgroundLuar != null) tombolBackgroundLuar.gameObject.SetActive(true);
        
        if (layarGelap != null)
        {
            Color warnaGelap = layarGelap.color;
            warnaGelap.a = 1f; 
            layarGelap.color = warnaGelap;
        }

        float waktu = 0f;
        while (waktu < durasiMuncul)
        {
            waktu += Time.deltaTime; 
            float t = waktu / durasiMuncul;
            float ease = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
            hpUtama.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, ease);
            yield return null;
        }
        hpUtama.transform.localScale = Vector3.one;

        // DI SINI BEBASNYA! Jeda layar hitam sebelum terang
        if (jedaSebelumLayarNyala > 0f)
        {
            yield return new WaitForSeconds(jedaSebelumLayarNyala);
        }

        waktu = 0f;
        if (layarGelap != null)
        {
            Color warna = layarGelap.color;
            while (waktu < durasiLayarNyala)
            {
                waktu += Time.deltaTime;
                warna.a = Mathf.Lerp(1f, 0f, waktu / durasiLayarNyala);
                layarGelap.color = warna;
                yield return null;
            }
            warna.a = 0f;
            layarGelap.color = warna;
        }

        foreach(GameObject ikon in daftarIkon)
        {
             if(ikon != null) ikon.SetActive(true);
        }

        AturTampilanIkonGame(true);

        bisaNavigasi = true;
        SetHighlightIkon(indeksIkonAktif, true); 
    }

    private IEnumerator AnimasiKecilDanTutup()
    {
        AturTampilanIkonGame(false);
        float waktu = 0f;
        Vector3 skalaAwal = hpUtama.transform.localScale;

        while (waktu < durasiTutup)
        {
            waktu += Time.deltaTime;
            float t = waktu / durasiTutup;
            
            hpUtama.transform.localScale = Vector3.Lerp(skalaAwal, Vector3.zero, t);
            
            if (layarGelap != null)
            {
                Color warna = layarGelap.color;
                warna.a = Mathf.Lerp(0f, 1f, t);
                layarGelap.color = warna;
            }

            yield return null;
        }
        
        hpUtama.transform.localScale = Vector3.zero;
        hpUtama.SetActive(false); 
        
        if (tombolIkonHP != null) tombolIkonHP.gameObject.SetActive(true); 
    }
}
