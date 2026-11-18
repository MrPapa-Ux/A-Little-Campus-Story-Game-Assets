using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CeritaKamar201Arthur : MonoBehaviour
{
    [Serializable] public class Halaman {public string judul;[TextArea(3,8)] public string isi;public Vector2 titikPetunjuk;public float sudutPanah;public Sprite gambar;}
    public enum Tahap {Memuat,Tutorial,Mencari,WaktuHabis,Selesai}
    public Halaman[] tutorial;
    public GameObject panel;
    public TMP_Text judul,isi,nomor,teksLanjut,timer;
    public Button kembali,lanjut;
    public RectTransform penandaTutorial;
    public UnityEngine.UI.Image gambarPetunjuk;
    public KarakterController pemain;
    public ArthurJalurKampus arthur;
    public Transform titikAwal;
    public BukuArthur buku;
    public AreaInteraksi areaHardisk;
    public GameObject titikHardisk,visualHardisk;
    public InteraksiJalurKampus pintuKeluar;
    public Sprite ikonHardisk;
    public LazyBarArthur procrastination;
    public NotifikasiMomenTerekam notifikasiMomen;
    [Min(1)] public float durasiPencarian=240;
    public Tahap Status {get;private set;}=Tahap.Memuat;
    public float SisaWaktu {get;private set;}
    public int HalamanAktif {get;private set;}
    public bool MomenTerekam1Aktif {get;private set;}
    const string IdItem="chapter1_hardisk_arthur",IdQuest="chapter1_cari_hardisk";
    SistemPilihan pilihan;
    SistemHP hp;
    static readonly int[] Awal = { 0, 14 };
    static readonly int[] Interaksi = { 1, 2 };
    static readonly int[] Bar = { 3 };
    static readonly int[] Ponsel = { 4, 5, 13 };
    static readonly System.Collections.Generic.Dictionary<string, int[]> Aplikasi =
        new System.Collections.Generic.Dictionary<string, int[]>
        {
            { "IkonMap", new[] { 6 } }, { "IkonToDoLIst", new[] { 7 } },
            { "IkonTas", new[] { 8 } }, { "IkonPesan", new[] { 9 } },
            { "IkonMusic", new[] { 10 } }, { "IkonSetting", new[] { 11 } },
            { "IkonGame", new[] { 12 } }
        };
    readonly System.Collections.Generic.HashSet<int> tutorialDilihat = new System.Collections.Generic.HashSet<int>();
    int[] kelompokTutorial;
    int indeksKelompok;
    bool kunciSebelumTutorial;
    RectTransform kartuTutorial;
    Vector2 ukuranPanelLama;
    Rect amanPanelLama;

    IEnumerator Start(){
        pemain.kunciPergerakan=true;panel.SetActive(false);timer.gameObject.SetActive(false);BlokirInteraksi(true);
        if(procrastination!=null)procrastination.tanpaBatasWaktu=true;
        titikHardisk.SetActive(false);visualHardisk.SetActive(false);
        if(pintuKeluar!=null)pintuKeluar.enabled=false;
        kembali.onClick.AddListener(Sebelumnya);lanjut.onClick.AddListener(Berikutnya);
        pilihan=FindFirstObjectByType<SistemPilihan>();if(pilihan!=null)pilihan.PercakapanSelesai+=SetelahDialog;
        hp=FindFirstObjectByType<SistemHP>();
        while(KawasanKampus.Instance!=null&&!KawasanKampus.Instance.Siap){
            if(!string.IsNullOrEmpty(KawasanKampus.Instance.KesalahanTerakhir)){judul.text="Kamar belum berhasil dimuat";isi.text="Hentikan dan coba Play kembali.";panel.SetActive(true);yield break;}
            yield return null;
        }
        while(KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat)yield return null;
        // Transisi kawasan memiliki kunci kendalinya sendiri; tutorial tetap mengunci pemain.
        yield return new WaitForSeconds(.3f);
        MomenTerekam1Aktif=true;Status=Tahap.Tutorial;
        kartuTutorial = judul.transform.parent as RectTransform;
        var kanvasTutorial = panel.GetComponentInParent<CanvasScaler>();
        if (kanvasTutorial != null) kanvasTutorial.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        BukaTutorial(Awal);
    }
    void Update(){
        if (kelompokTutorial != null)
        {
            SesuaikanUkuranTutorial();
            return;
        }
        if(Status==Tahap.WaktuHabis)pemain.kunciPergerakan=true;
        if ((Status == Tahap.Mencari || Status == Tahap.Selesai) && CobaTutorialKonteks()) return;
        if(Status!=Tahap.Mencari)return;
        if(procrastination!=null&&procrastination.waktuHabis)return;
        if(DialogManager.Instance!=null&&DialogManager.Instance.PercakapanAktif)return;
        if(hp!=null&&hp.MenjedaParameter)return;
        SisaWaktu=Mathf.Max(0,SisaWaktu-Time.deltaTime);SegarkanTimer();
        if(SisaWaktu<=0){
            if(procrastination!=null){procrastination.GagalKarenaBatasWaktu();return;}
            TampilkanGagal();
        }
    }

    bool CobaTutorialKonteks()
    {
        var dialog = DialogManager.Instance;
        if (dialog == null || dialog.minigameAktif || dialog.PercakapanAktif ||
            (KawasanKampus.Instance != null && KawasanKampus.Instance.SedangMemuat)) return false;
        if (hp != null && hp.MenjedaParameter)
        {
            if (hp.SiapNavigasiUtama) return BukaTutorial(Ponsel);
            return hp.IkonAplikasiAktif != null && Aplikasi.TryGetValue(hp.IkonAplikasiAktif, out var halaman)
                && BukaTutorial(halaman);
        }
        if (pemain.kunciPergerakan) return false;
        if (dialog.PemilikInteraksi != null && BukaTutorial(Interaksi)) return true;
        return procrastination != null && procrastination.RasioTersisa < .98f &&
            !procrastination.waktuHabis && BukaTutorial(Bar);
    }

    bool BukaTutorial(int[] kelompok)
    {
        if (tutorial == null || kelompok[kelompok.Length - 1] >= tutorial.Length ||
            !tutorialDilihat.Add(kelompok[0])) return false;
        kelompokTutorial = kelompok;
        indeksKelompok = 0;
        kunciSebelumTutorial = pemain.kunciPergerakan;
        pemain.BerhentiBergerak();
        TampilkanTutorial();
        SesuaikanUkuranTutorial();
        return true;
    }

    void SesuaikanUkuranTutorial()
    {
        if (kartuTutorial == null || !(panel.transform is RectTransform latar)) return;
        Vector2 ruang = latar.rect.size;
        if ((ruang == ukuranPanelLama && amanPanelLama == Screen.safeArea) || ruang.x <= 0 || ruang.y <= 0) return;
        ukuranPanelLama = ruang;
        amanPanelLama = Screen.safeArea;
        PonselAplikasiBersama.PasTengah(kartuTutorial, new Vector2(.91f, .90f));
    }
    public void TampilkanGagal(){
        if(Status==Tahap.Selesai){var chapel=FindFirstObjectByType<PerjalananChapelChapter1>();if(chapel!=null&&chapel.PulihkanSetelahGlitch())return;GetComponent<PerjalananDandiChapter1>()?.GagalPerjalanan();return;}
            Status=Tahap.WaktuHabis;pemain.kunciPergerakan=true;titikHardisk.SetActive(false);visualHardisk.SetActive(false);
            panel.SetActive(true);BlokirInteraksi(true);judul.text="Coba Lagi";isi.text="Pencarian belum selesai.";
            nomor.text="";kembali.gameObject.SetActive(false);teksLanjut.text="Coba Lagi";
    }
    public void Sebelumnya()
    {
        if (kelompokTutorial == null || indeksKelompok == 0) return;
        indeksKelompok--;
        TampilkanTutorial();
    }
    public void Berikutnya(){
        if (kelompokTutorial != null)
        {
            if (indeksKelompok + 1 < kelompokTutorial.Length)
            {
                indeksKelompok++;
                TampilkanTutorial();
            }
            else if (Status == Tahap.Tutorial) MulaiPencarian();
            else
            {
                kelompokTutorial = null;
                panel.SetActive(false);
                BlokirInteraksi(false);
                pemain.kunciPergerakan = kunciSebelumTutorial;
            }
        }
        else if(Status==Tahap.WaktuHabis){arthur.PindahKe(titikAwal);MulaiPencarian();}
        else if(Status==Tahap.Selesai){panel.SetActive(false);BlokirInteraksi(false);pemain.BerhentiBergerak();pemain.kunciPergerakan=false;}
    }
    void TampilkanTutorial(){
        panel.SetActive(true);BlokirInteraksi(true);pemain.kunciPergerakan=true;
        HalamanAktif = kelompokTutorial[indeksKelompok];
        judul.text=tutorial[HalamanAktif].judul;isi.text=tutorial[HalamanAktif].isi;
        nomor.text=kelompokTutorial.Length>1 ? (indeksKelompok+1)+" / "+kelompokTutorial.Length : "";
        kembali.gameObject.SetActive(indeksKelompok>0);
        teksLanjut.text=indeksKelompok+1==kelompokTutorial.Length && Status==Tahap.Tutorial?"Mulai Bermain":"Lanjut";
        if(penandaTutorial!=null){var halaman=tutorial[HalamanAktif];penandaTutorial.anchorMin=penandaTutorial.anchorMax=halaman.titikPetunjuk;penandaTutorial.anchoredPosition=Vector2.zero;penandaTutorial.localRotation=Quaternion.Euler(0,0,halaman.sudutPanah);}
        if(gambarPetunjuk!=null&&tutorial[HalamanAktif].gambar!=null)gambarPetunjuk.sprite=tutorial[HalamanAktif].gambar;
    }
    public void MulaiPencarian(){
        if(Status!=Tahap.Tutorial&&Status!=Tahap.WaktuHabis)return;
        kelompokTutorial = null;
        panel.SetActive(false);BlokirInteraksi(false);Status=Tahap.Mencari;SisaWaktu=durasiPencarian;pemain.BerhentiBergerak();pemain.kunciPergerakan=false;
        titikHardisk.SetActive(true);visualHardisk.SetActive(true);timer.gameObject.SetActive(true);SegarkanTimer();
        if(procrastination!=null){procrastination.JedaKhususCerita=false;procrastination.PulihkanBar();procrastination.tanpaBatasWaktu=false;}
        if(notifikasiMomen!=null)notifikasiMomen.Tampilkan(1);
        buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{id=IdQuest,judul="Cari Hardisk di kamar",dikendalikanQuest=true,isi="Cari Hardisk Arthur di Kamar 201. Periksa lantai di bawah lemari kiri. Waktu pencarian empat menit."});
    }
    void SetelahDialog(AreaInteraksi area,bool tuntas){if(tuntas&&area==areaHardisk)AmbilHardisk();}
    public bool AmbilHardisk(){
        if(Status!=Tahap.Mencari||Vector3.Distance(arthur.transform.position,areaHardisk.transform.position)>2.3f)return false;
        Status=Tahap.Selesai;titikHardisk.SetActive(false);visualHardisk.SetActive(false);timer.gameObject.SetActive(false);
        buku.Tambah(JenisBukuArthur.Tas,new CatatanArthur{id=IdItem,judul="Hardisk Arthur",ikon=ikonHardisk,jumlah=1,isi="Hard disk milik Arthur yang ditemukan di bawah lemari."});
        buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{id=IdQuest,judul="Cari Hardisk di kamar",dikendalikanQuest=true,selesai=true,isi="Hardisk Arthur ditemukan dan sudah masuk ke Tas."});
        if(pintuKeluar!=null)pintuKeluar.enabled=true;return true;
    }
    void SegarkanTimer(){int detik=Mathf.CeilToInt(SisaWaktu);timer.text="Cari Hardisk di kamar  "+(detik/60).ToString("00")+":"+(detik%60).ToString("00");}
    void BlokirInteraksi(bool blokir){if(DialogManager.Instance!=null)DialogManager.Instance.minigameAktif=blokir;}
    void OnDestroy(){if(pilihan!=null)pilihan.PercakapanSelesai-=SetelahDialog;}
}
