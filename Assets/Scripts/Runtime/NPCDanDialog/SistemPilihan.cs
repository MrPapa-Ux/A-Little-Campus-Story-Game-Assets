// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Menjalankan baris percakapan, mesin tik, potret, pilihan, ekspresi, emote, dan teleportasi.
// Panduan: Assets/Scripts/Runtime/NPCDanDialog/README.md

using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections; 

public class SistemPilihan : MonoBehaviour
{
    public event System.Action<AreaInteraksi,int> BarisSelesaiDiketik;
    public event System.Action<AreaInteraksi,bool> PercakapanSelesai;
    private bool ceritaTuntas;
    [Header("Referensi Sistem Utama")]
    public DialogManager dialogManager;
    public CascadeTransition efekTransisi; 
    
    private NPCBubbleTrigger npcLawanBicara;
    private NPCMenghadapArthur npcMenghadap;
    private EkspresiWajah2D wajahPembicara;
    private SuaraDialogKertas suaraPembicara;
    private DialogTeriakGoofy efekTeriak;

    [Header("--- SUTRADARA UI ---")]
    public TextMeshProUGUI teksCeritaUtama;
    [Header("Batas aman teks di dalam kertas dialog")]
    [Tooltip("Batas gambar kertas pada aset DialogTextBox, tanpa ruang transparannya.")]
    public Rect bidangKertasDialog = new Rect(.126f,.504f,.784f,.31f);
    [Tooltip("Jarak teks dari tepi kertas: kiri, atas, kanan, bawah.")]
    public Vector4 jarakDalamDialog = new Vector4(32,24,32,24);
    [Min(0)] public float jarakDariPotret = 28f;
    [Min(20)] public float ukuranHurufDialog = 44f;
    [Header("Keterbacaan dialog pada layar HP")]
    [Range(1f,1.2f)] public float pembesaranPanelDialog = 1.14f;
    [Range(1f,1.4f)] public float pembesaranPotretDialog = 1.22f;
    bool ukuranDialogHPDiterapkan;
    readonly Vector3[] sudutPotret = new Vector3[4];
    public GameObject teksNextKhusus; 
    
    [Header("Referensi Potret & Nama")]
    public GameObject potretKiriArthur; 
    public GameObject potretKananNPC;   
    [Tooltip("Potret NPC lelaki bila NPC belum diberi potret tersendiri pada Area Interaksi.")]
    public Sprite potretNpcLelaki;
    [Header("Ekspresi potret Arthur")]
    public Sprite potretArthurSenyum;
    public Sprite potretArthurSenang;
    public Sprite potretArthurSedih;
    private Sprite potretNpcSemula;
    private bool potretNpcDisimpan;
    public TextMeshProUGUI teksNamaKiri; 
    public TextMeshProUGUI teksNamaKanan;

    [Header("Daftar Panel Opsi Utama")]
    public EfekPilihanUI[] daftarPilihanUtama;
    public TextMeshProUGUI[] teksPilihanUtama;

    [Header("Panel Tombol Keluar")]
    public EfekPilihanUI panelKeluarKhusus;
    public TextMeshProUGUI teksKeluarKhusus;

    [Header("--- EFEK MESIN TIK ---")]
    public float kecepatanKetik = 0.03f; 

    [Header("Pelindung Tekanan Berulang")]
    [Min(0f)] public float jedaSetelahTeks = 0.3f;
    [Min(0f)] public float jedaAntarInput = 0.12f;

    [Header("--- GRUP FISIK / CHUNK ---")]
    public GameObject grupFisikDepan;     
    public GameObject grupFisikBelakang;  
    public GameObject grupFisikRuangan;   

    private int indeksPilihan = 0;
    private int jumlahPilihanAktif = 0; 

    private BarisDialog[] ceritaSaatIni;
    private int indeksCerita = 0;
    
    private bool sedangBacaCerita = false;
    private bool sedangMainkanEmote = false; 
    private bool sedangNgetik = false;
    private Coroutine coroutineKetik;
    private float inputBerikutnya;
    private bool sedangMenutup;
    private int indeksHurufBerikutnya;
    private int awalKataTerakhir;
    private string teksBaris = "";

    // --- VARIABEL ANIMASI NEXT ---
    private Coroutine coroutineAnimasiNext;
    private RectTransform rectNext;
    private Vector2 posisiAwalNext;
    private bool posisiAwalNextDisimpan = false;

    // Percakapan dimulai eksplisit setelah canvas dibuka. Membuka kembali canvas
    // sesudah emote tidak boleh memulai ulang skenario.

    private void MulaiPercakapan()
    {
        if (DialogManager.Instance != null) ceritaSaatIni = DialogManager.Instance.skenarioAktif; 

        indeksCerita = 0;
        ceritaTuntas = false;
        sedangMainkanEmote = false;
        sedangNgetik = false;
        sedangMenutup = false;
        inputBerikutnya = Time.unscaledTime + jedaAntarInput;
        
        CariNPCLawanBicara();
        ResetSemuaPanel(); 

        if (ceritaSaatIni != null && ceritaSaatIni.Length > 0)
        {
            sedangBacaCerita = true;
            TampilkanBarisSaatIni();
        }
        else
        {
            sedangBacaCerita = false;
            if (teksCeritaUtama != null)
            {
                teksCeritaUtama.gameObject.SetActive(true);
                teksCeritaUtama.text = "Apa yang ingin kamu lakukan?"; 
            }
            SembunyikanSemuaPotret(); 
            AturBatasAmanTeks();
            SembunyikanNextKhusus();
            SiapkanOpsiDinamis();
        }
        UpdateVisual();
    }
    
    private void CariNPCLawanBicara()
    {
        AkhiriPercakapanVisual();
        npcMenghadap = null;
        npcLawanBicara = null;
        if (DialogManager.Instance == null || string.IsNullOrEmpty(DialogManager.Instance.idInteraksiAktif)) return;
        var pemilik = DialogManager.Instance.PemilikInteraksi;
        GameObject npcObj = pemilik != null ? pemilik.gameObject : GameObject.Find(DialogManager.Instance.idInteraksiAktif);
        if (npcObj != null) npcLawanBicara = npcObj.GetComponent<NPCBubbleTrigger>();
        if (npcObj != null) npcMenghadap = npcObj.GetComponent<NPCMenghadapArthur>();
        if (npcMenghadap != null && dialogManager.arthurScript != null) npcMenghadap.HadapArthur(dialogManager.arthurScript.transform);
    }

    private void TampilkanBarisSaatIni()
    {
        BarisDialog baris = ceritaSaatIni[indeksCerita];
        AturPembicara(baris); 

        ResetSemuaPanel();
        SembunyikanNextKhusus();
        
        jumlahPilihanAktif = 0; 
        UpdateVisual();

        if (teksCeritaUtama != null)
        {
            teksCeritaUtama.gameObject.SetActive(true);
            if (coroutineKetik != null) StopCoroutine(coroutineKetik);
            teksBaris = baris.teksDialog ?? "";
            awalKataTerakhir = CariAwalKataTerakhir(teksBaris);
            indeksHurufBerikutnya = 0;
            teksCeritaUtama.text = "";
            coroutineKetik = StartCoroutine(EfekKetik(baris.teksDialog, baris.teksTombolLanjut));
        }
    }

    // --- FITUR BARU: JEDA TANDA BACA ---
    private IEnumerator EfekKetik(string teksLengkap, string teksNext)
    {
        sedangNgetik = true;
        teksLengkap = teksLengkap ?? "";
        while (indeksHurufBerikutnya < teksLengkap.Length)
        {
            char huruf = teksLengkap[indeksHurufBerikutnya++];
            teksCeritaUtama.text += huruf;
            if (char.IsLetterOrDigit(huruf) && wajahPembicara != null) wajahPembicara.UcapkanHuruf();
            if(suaraPembicara!=null)suaraPembicara.Huruf(huruf);
            if (efekTeriak != null) efekTeriak.Huruf(huruf);
            
            // Logika jeda napas layaknya karakter sungguhan
            if (huruf == '.' || huruf == '?' || huruf == '!')
            {
                yield return new WaitForSeconds(1f); 
            }
            else if (huruf == ',')
            {
                yield return new WaitForSeconds(0.5f); 
            }
            else
            {
                yield return new WaitForSeconds(kecepatanKetik); 
            }
        }

        SelesaiNgetik(teksNext);
    }

    public static int CariAwalKataTerakhir(string teks)
    {
        if (string.IsNullOrWhiteSpace(teks)) return 0;
        int akhir = teks.Length - 1;
        while (akhir >= 0 && !char.IsLetterOrDigit(teks[akhir])) akhir--;
        if (akhir < 0) return 0;
        while (akhir > 0 && !char.IsWhiteSpace(teks[akhir - 1])) akhir--;
        return akhir;
    }

    private void PercepatSampaiKataTerakhir()
    {
        if (indeksHurufBerikutnya >= awalKataTerakhir) return;
        if (coroutineKetik != null) StopCoroutine(coroutineKetik);
        indeksHurufBerikutnya = awalKataTerakhir;
        teksCeritaUtama.text = teksBaris.Substring(0, awalKataTerakhir);
        coroutineKetik = StartCoroutine(EfekKetik(teksBaris, ceritaSaatIni[indeksCerita].teksTombolLanjut));
    }

    private void SelesaiNgetik(string teksNext)
    {
        sedangNgetik = false;
        HentikanMulut();
        coroutineKetik = null;
        inputBerikutnya = Time.unscaledTime + jedaSetelahTeks;
        teksCeritaUtama.text = ceritaSaatIni[indeksCerita].teksDialog; 
        
        bool iniBarisTerakhir = (indeksCerita == ceritaSaatIni.Length - 1);
        bool adaEmoteSetelahIni = ceritaSaatIni[indeksCerita].picuEmoteSetelahIni;
        if(iniBarisTerakhir&&!adaEmoteSetelahIni)ceritaTuntas=true;
        BarisSelesaiDiketik?.Invoke(dialogManager.PemilikInteraksi,indeksCerita);

        if (iniBarisTerakhir && !adaEmoteSetelahIni)
        {
            sedangBacaCerita = false;
            SembunyikanNextKhusus();
            SiapkanOpsiDinamis();
        }
        else
        {
            sedangBacaCerita = true;
            bool adaTeksNext = !string.IsNullOrWhiteSpace(teksNext);

            if (adaTeksNext)
            {
                MunculkanNextKhusus(); // TEKS NEXT MELAYANG MUNCUL
                SiapkanTombolSelanjutnya(teksNext); 
            }
            else
            {
                SembunyikanNextKhusus();
                SiapkanHanyaKeluar();
            }
        }
    }

    // --- PENGATUR ANIMASI NEXT MELAYANG NAIK TURUN ---
    private void MunculkanNextKhusus()
    {
        if (teksNextKhusus != null)
        {
            if (!posisiAwalNextDisimpan)
            {
                rectNext = teksNextKhusus.GetComponent<RectTransform>();
                if (rectNext != null)
                {
                    posisiAwalNext = rectNext.anchoredPosition;
                    posisiAwalNextDisimpan = true;
                }
            }

            teksNextKhusus.SetActive(true);
            
            // JALANKAN ANIMASI BOUNCING
            if (coroutineAnimasiNext != null) StopCoroutine(coroutineAnimasiNext);
            coroutineAnimasiNext = StartCoroutine(AnimasiNaikTurunNext());
        }
    }

    private void SembunyikanNextKhusus()
    {
        if (teksNextKhusus != null)
        {
            teksNextKhusus.SetActive(false);
            if (coroutineAnimasiNext != null) StopCoroutine(coroutineAnimasiNext);
            
            if (rectNext != null && posisiAwalNextDisimpan)
            {
                rectNext.anchoredPosition = posisiAwalNext;
            }
        }
    }

    private IEnumerator AnimasiNaikTurunNext()
    {
        if (rectNext == null) yield break;
        float waktu = 0f;
        float kecepatanNaikTurun = 6f; 
        float jarakNaikTurun = 7f; 

        while (true)
        {
            waktu += Time.deltaTime * kecepatanNaikTurun;
            float yOffset = Mathf.Sin(waktu) * jarakNaikTurun;
            rectNext.anchoredPosition = posisiAwalNext + new Vector2(0, yOffset);
            yield return null;
        }
    }
    // ----------------------------------------------------

    private void AturPembicara(BarisDialog baris)
    {
        HentikanMulut();
        if (wajahPembicara != null) wajahPembicara.KembalikanEkspresi();
        wajahPembicara = null;
        suaraPembicara=null;
        if(baris.siapaYangBicara==Pembicara.Kiri_Arthur && dialogManager.arthurScript!=null)suaraPembicara=dialogManager.arthurScript.GetComponent<SuaraDialogKertas>();
        else if(baris.siapaYangBicara==Pembicara.Kanan_NPC)
        {
            var pemilik = dialogManager.PemilikInteraksi;
            var aktor = pemilik != null ? pemilik.gameObject : npcLawanBicara != null ? npcLawanBicara.gameObject : null;
            if (aktor != null) suaraPembicara = aktor.GetComponentInChildren<SuaraDialogKertas>();
        }
        if (baris.siapaYangBicara == Pembicara.Kiri_Arthur && dialogManager.arthurScript != null)
            wajahPembicara = dialogManager.arthurScript.GetComponentInChildren<EkspresiWajah2D>();
        else if (baris.siapaYangBicara == Pembicara.Kanan_NPC)
        {
            var pemilik = dialogManager.PemilikInteraksi;
            var aktor = pemilik != null ? pemilik.gameObject : npcLawanBicara != null ? npcLawanBicara.gameObject : null;
            if (aktor != null) wajahPembicara = aktor.GetComponentInChildren<EkspresiWajah2D>();
        }
        if (wajahPembicara != null && baris.ubahEkspresi) wajahPembicara.AturEkspresiDialog(baris.ekspresi);
        if (efekTeriak == null && teksCeritaUtama != null) efekTeriak = teksCeritaUtama.GetComponent<DialogTeriakGoofy>();
        if (efekTeriak != null) efekTeriak.Atur(baris.berteriak, baris.kekuatanTeriak);
        SembunyikanSemuaPotret(); 

        if (baris.siapaYangBicara == Pembicara.Kiri_Arthur)
        {
            if (potretKiriArthur != null)
            {
                var gambar = potretKiriArthur.GetComponent<Image>();
                var emosi = baris.ubahEkspresi ? baris.ekspresi : EmosiKarakter.Senyum;
                var potret = emosi == EmosiKarakter.Sedih ? potretArthurSedih
                    : emosi == EmosiKarakter.Senang ? potretArthurSenang : potretArthurSenyum;
                if (gambar != null && potret != null) { gambar.sprite = potret; gambar.preserveAspect = true; }
            }
            if (potretKiriArthur != null) potretKiriArthur.SetActive(true);
            if (teksNamaKiri != null) { teksNamaKiri.gameObject.SetActive(true); teksNamaKiri.text = baris.namaKarakter; }
        }
        else if (baris.siapaYangBicara == Pembicara.Kanan_NPC)
        {
            if (potretKananNPC != null)
            {
                var gambar = potretKananNPC.GetComponent<Image>();
                if (gambar != null)
                {
                    if (!potretNpcDisimpan) { potretNpcSemula = gambar.sprite; potretNpcDisimpan = true; }
                    var pemilik = dialogManager != null ? dialogManager.PemilikInteraksi : null;
                    gambar.sprite = pemilik != null && pemilik.potretDialog != null ? pemilik.potretDialog : potretNpcLelaki != null ? potretNpcLelaki : potretNpcSemula;
                    gambar.preserveAspect = true;
                }
            }
            if (potretKananNPC != null) potretKananNPC.SetActive(true);
            if (teksNamaKanan != null) { teksNamaKanan.gameObject.SetActive(true); teksNamaKanan.text = baris.namaKarakter; }
        }
        AturBatasAmanTeks();
    }

    // Rect teks mengikuti bagian kertas yang benar-benar terlihat, bukan seluruh PNG transparan.
    // Potret kiri/kanan mempunyai kolom sendiri dan tidak boleh tertimpa isi percakapan.
    public void AturBatasAmanTeks()
    {
        if(teksCeritaUtama==null)return;
        var r=teksCeritaUtama.rectTransform;var panel=r.parent as RectTransform;if(panel==null)return;
        PerbesarDialogHP(panel);
        var p=panel.rect;
        // DialogTextBox memakai 9-slice. Koordinat PNG tidak bisa dikalikan
        // langsung dengan ukuran panel karena pinggirnya tidak ikut diregangkan.
        var gambar=panel.GetComponent<UnityEngine.UI.Image>();
        var kertas=bidangKertasDialog;
        if(gambar!=null && gambar.sprite!=null && gambar.type==UnityEngine.UI.Image.Type.Sliced)
        {
            var sprite=gambar.sprite;var b=sprite.border;
            float ppu=gambar.pixelsPerUnit*gambar.pixelsPerUnitMultiplier;
            float x0=PetakanSumbuKertas(kertas.xMin*sprite.rect.width,sprite.rect.width,p.width,b.x,b.z,ppu);
            float x1=PetakanSumbuKertas(kertas.xMax*sprite.rect.width,sprite.rect.width,p.width,b.x,b.z,ppu);
            float y0=PetakanSumbuKertas(kertas.yMin*sprite.rect.height,sprite.rect.height,p.height,b.y,b.w,ppu);
            float y1=PetakanSumbuKertas(kertas.yMax*sprite.rect.height,sprite.rect.height,p.height,b.y,b.w,ppu);
            kertas=Rect.MinMaxRect(x0/p.width,y0/p.height,x1/p.width,y1/p.height);
        }
        float kiri=p.xMin+p.width*kertas.xMin+jarakDalamDialog.x;
        float kanan=p.xMin+p.width*kertas.xMax-jarakDalamDialog.z;
        float bawah=p.yMin+p.height*kertas.yMin+jarakDalamDialog.w;
        float atas=p.yMin+p.height*kertas.yMax-Mathf.Max(60,jarakDalamDialog.y);
        foreach(var potret in new[]{potretKiriArthur,potretKananNPC})
        {
            if(potret==null||!potret.activeSelf||!(potret.transform is RectTransform rp))continue;
            // Bingkai lebih lebar daripada ilustrasi potret.
            var bingkai=rp.Find("BingkaiSketsa") as RectTransform;
            (bingkai!=null?bingkai:rp).GetWorldCorners(sudutPotret);
            float min=float.PositiveInfinity,max=float.NegativeInfinity;
            foreach(var titik in sudutPotret){float x=panel.InverseTransformPoint(titik).x;min=Mathf.Min(min,x);max=Mathf.Max(max,x);}
            if(potret==potretKiriArthur)kiri=Mathf.Max(kiri,max+jarakDariPotret);
            else kanan=Mathf.Min(kanan,min-jarakDariPotret);
        }
        if(kanan<=kiri||atas<=bawah)return;
        r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);
        r.anchoredPosition=new Vector2((kiri+kanan)*.5f-p.center.x,(atas+bawah)*.5f-p.center.y);
        r.sizeDelta=new Vector2(kanan-kiri,atas-bawah);teksCeritaUtama.margin=Vector4.zero;
        teksCeritaUtama.alignment=TextAlignmentOptions.Center;
        teksCeritaUtama.fontSizeMax=ukuranHurufDialog+4;teksCeritaUtama.fontSizeMin=Mathf.Max(20,ukuranHurufDialog);
        teksCeritaUtama.enableAutoSizing=true;teksCeritaUtama.textWrappingMode=TextWrappingModes.Normal;

    }

    // Dilakukan sekali per instance, bukan per baris dialog (mencegah skala bertambah berulang).
    void PerbesarDialogHP(RectTransform panel)
    {
        if(ukuranDialogHPDiterapkan)return;
        ukuranDialogHPDiterapkan=true;
        panel.localScale*=pembesaranPanelDialog;
        foreach(var efek in panel.GetComponentsInParent<EfekMunculDialog>(true))
            efek.PerbesarBidang(panel,pembesaranPanelDialog);
        // Pertahankan ruang di atas nama/petunjuk setelah kertas diperbesar.
        panel.anchoredPosition+=new Vector2(0,-30);
        foreach(var potret in new[]{potretKiriArthur,potretKananNPC})
            if(potret!=null)potret.transform.localScale*=pembesaranPotretDialog;
        // Opsi adalah saudara panel, sehingga perlu ikut turun agar tidak menimpa kertas.
        if(panelKeluarKhusus!=null && panelKeluarKhusus.transform.parent==panel.parent)
            ((RectTransform)panelKeluarKhusus.transform).anchoredPosition+=new Vector2(0,-55);
        if(daftarPilihanUtama!=null)foreach(var opsi in daftarPilihanUtama)
            if(opsi!=null && opsi.transform.parent==panel.parent)
                ((RectTransform)opsi.transform).anchoredPosition+=new Vector2(0,-55);
    }

    static float PetakanSumbuKertas(float titik,float sumber,float tujuan,float awal,float akhir,float ppu)
    {
        float a=awal/ppu,b=akhir/ppu;
        float skala=Mathf.Min(1f,tujuan/Mathf.Max(.001f,a+b));a*=skala;b*=skala;
        if(awal>0 && titik<awal)return titik/awal*a;
        if(akhir>0 && titik>sumber-akhir)return tujuan-(sumber-titik)/akhir*b;
        return a+(titik-awal)/Mathf.Max(.001f,sumber-awal-akhir)*(tujuan-a-b);
    }

    private void HentikanMulut() { if (wajahPembicara != null) wajahPembicara.HentikanBicara();if(suaraPembicara!=null)suaraPembicara.Hentikan(); }
    public void AkhiriPercakapanVisual()
    {
        if (coroutineKetik != null) { StopCoroutine(coroutineKetik); coroutineKetik = null; }
        sedangNgetik = false;
        HentikanMulut();
        if (wajahPembicara != null) wajahPembicara.KembalikanEkspresi();
        if (efekTeriak != null) efekTeriak.Atur(false);
        if (npcMenghadap != null) npcMenghadap.KembaliSemula();
    }
    void OnDisable() { HentikanMulut(); AkhiriPercakapanVisual(); }

    private void SembunyikanSemuaPotret()
    {
        if (potretKiriArthur != null) potretKiriArthur.SetActive(false);
        if (potretKananNPC != null) potretKananNPC.SetActive(false);
        if (teksNamaKiri != null) teksNamaKiri.gameObject.SetActive(false);
        if (teksNamaKanan != null) teksNamaKanan.gameObject.SetActive(false);
    }

    private void SiapkanTombolSelanjutnya(string teksNext)
    {
        ResetSemuaPanel();
        
        if (daftarPilihanUtama.Length > 0 && daftarPilihanUtama[0] != null)
        {
            daftarPilihanUtama[0].gameObject.SetActive(true);
            if (teksPilihanUtama.Length > 0 && teksPilihanUtama[0] != null) 
            {
                teksPilihanUtama[0].text = teksNext; 
            }
        }
        
        bool bisaKeluar = (DialogManager.Instance != null && DialogManager.Instance.statusTombolKeluar);
        if (panelKeluarKhusus != null) panelKeluarKhusus.gameObject.SetActive(bisaKeluar);
        
        jumlahPilihanAktif = bisaKeluar ? 2 : 1; 
        indeksPilihan = 0; 
        UpdateVisual();
    }

    private void SiapkanHanyaKeluar()
    {
        ResetSemuaPanel();
        
        bool bisaKeluar = (DialogManager.Instance != null && DialogManager.Instance.statusTombolKeluar);
        if (panelKeluarKhusus != null) panelKeluarKhusus.gameObject.SetActive(bisaKeluar);
        
        jumlahPilihanAktif = bisaKeluar ? 1 : 0; 
        indeksPilihan = 0; 
        UpdateVisual();
    }

    public void AksiTombolInteraksi()
    {
        if(dialogManager!=null&&dialogManager.minigameAktif)return;
        if (sedangMainkanEmote || sedangMenutup || Time.unscaledTime < inputBerikutnya) return;
        SistemHP hp = FindFirstObjectByType<SistemHP>();
        if (hp != null && hp.hpSedangBuka) return;
        if (dialogManager == null || dialogManager.canvasDialog == null) return;
        inputBerikutnya = Time.unscaledTime + jedaAntarInput;

        if (dialogManager != null && !dialogManager.canvasDialog.activeInHierarchy)
        {
            if (DialogManager.Instance != null && DialogManager.Instance.bisaInteraksi) 
            {
                dialogManager.BukaDialog(); 
                if (dialogManager.canvasDialog.activeInHierarchy) MulaiPercakapan();
            }
            return;
        }

        if (sedangNgetik)
        {
            PercepatSampaiKataTerakhir();
            return;
        }

        if (jumlahPilihanAktif == 0 && sedangBacaCerita) 
        {
            LanjutCeritaAtauEmote();
            return;
        }
        else if (jumlahPilihanAktif == 0) return;

        if (sedangBacaCerita) 
        {
            BarisDialog barisAktif = ceritaSaatIni[indeksCerita];
            bool adaTeksNext = !string.IsNullOrWhiteSpace(barisAktif.teksTombolLanjut);

            if (!adaTeksNext)
            {
                if (indeksPilihan == 0 && panelKeluarKhusus != null && panelKeluarKhusus.gameObject.activeSelf && panelKeluarKhusus.sedangDipilih)
                {
                    EksekusiPilihan();
                }
                else
                {
                    LanjutCeritaAtauEmote();
                }
            }
            else
            {
                if (indeksPilihan == jumlahPilihanAktif - 1 && panelKeluarKhusus != null && panelKeluarKhusus.gameObject.activeSelf) 
                {
                    EksekusiPilihan(); 
                }
                else if (indeksPilihan == 0) 
                {
                    LanjutCeritaAtauEmote(); 
                }
            }
        }
        else 
        {
            EksekusiPilihan(); 
        }
    }

    // Sentuhan memilih panel yang diketuk langsung; tetap memakai pelindung
    // input dan alur yang sama dengan tombol interaksi/navigasi sebelumnya.
    public void TekanPilihanLangsung(int indeksPanel)
    {
        if (!BolehSentuhPilihan()) return;
        if (indeksPanel < 0 || indeksPanel >= daftarPilihanUtama.Length || indeksPanel >= jumlahPilihanAktif) return;
        EfekPilihanUI panel = daftarPilihanUtama[indeksPanel];
        if (panel == null || !panel.gameObject.activeInHierarchy) return;
        indeksPilihan = indeksPanel;
        UpdateVisual();
        AksiTombolInteraksi();
    }

    public void TekanKeluarLangsung()
    {
        if (!BolehSentuhPilihan() || panelKeluarKhusus == null || !panelKeluarKhusus.gameObject.activeInHierarchy) return;
        indeksPilihan = jumlahPilihanAktif - 1;
        UpdateVisual();
        AksiTombolInteraksi();
    }

    private bool BolehSentuhPilihan()
    {
        return dialogManager != null && dialogManager.canvasDialog != null && dialogManager.canvasDialog.activeInHierarchy
            && !sedangNgetik && !sedangMainkanEmote && !sedangMenutup && Time.unscaledTime >= inputBerikutnya;
    }

    private void LanjutCeritaAtauEmote()
    {
        BarisDialog barisSaatIni = ceritaSaatIni[indeksCerita];
        
        if (barisSaatIni.picuEmoteSetelahIni && npcLawanBicara != null && barisSaatIni.spriteEmote != null)
        {
            StartCoroutine(SutradaraMainkanEmote(barisSaatIni));
        }
        else
        {
            ProsesLanjutBarisCerita();
        }
    }

    private IEnumerator SutradaraMainkanEmote(BarisDialog baris)
    {
        sedangMainkanEmote = true; 
        
        EfekMunculDialog efekDialog = dialogManager.canvasDialog.GetComponent<EfekMunculDialog>();
        
        SembunyikanNextKhusus();
        ResetSemuaPanel(); 
        jumlahPilihanAktif = 0; 
        UpdateVisual();

        if (efekDialog != null) 
        {
            efekDialog.TutupDialog(); 
            yield return new WaitForSeconds(efekDialog.durasiTutup); 
        }
        
        if (dialogManager != null && dialogManager.canvasDialog != null) 
        {
            dialogManager.canvasDialog.SetActive(false); 
        }
        
        if (npcLawanBicara != null) npcLawanBicara.MulaiAnimasiBubbleKhusus(baris.spriteEmote, baris.durasiEmote);
        
        yield return new WaitForSeconds(baris.durasiEmote);
        if(indeksCerita==ceritaSaatIni.Length-1)ceritaTuntas=true;

        if(baris.akhiriSetelahEmote)
        {
            sedangMainkanEmote=false;
            yield return ProsesTutupDialogAnimasi();
            yield break;
        }
        
        if (dialogManager != null && dialogManager.canvasDialog != null) 
        {
            dialogManager.canvasDialog.SetActive(true); 
            if (dialogManager.arthurScript != null)
            {
                dialogManager.arthurScript.BerhentiBergerak();
                dialogManager.arthurScript.kunciPergerakan = true;
            }
        }
        
        // SUDAH BERSIH TOTAL DARI BUKADIALOG()!
        
        sedangMainkanEmote = false; 
        inputBerikutnya = Time.unscaledTime + jedaSetelahTeks;

        if (indeksCerita >= ceritaSaatIni.Length - 1)
        {
            sedangBacaCerita = false;
            SembunyikanNextKhusus();
            SiapkanOpsiDinamis();
        }
        else
        {
            ProsesLanjutBarisCerita();
        }
    }

    private void ProsesLanjutBarisCerita()
    {
        indeksCerita++;
        if (indeksCerita < ceritaSaatIni.Length)
        {
            TampilkanBarisSaatIni();
        }
        else
        {
            sedangBacaCerita = false;
            SembunyikanNextKhusus();
            SiapkanOpsiDinamis();
        }
    }

    private void SiapkanOpsiDinamis()
    {
        OpsiInteraksi[] daftarOpsi = null;
        if (DialogManager.Instance != null) daftarOpsi = DialogManager.Instance.opsiInteraksiAktif; 

        ResetSemuaPanel(); 

        bool bisaKeluar = (DialogManager.Instance != null && DialogManager.Instance.statusTombolKeluar);
        if (panelKeluarKhusus != null) panelKeluarKhusus.gameObject.SetActive(bisaKeluar);

        if (daftarOpsi != null && daftarOpsi.Length > 0)
        {
            jumlahPilihanAktif = Mathf.Min(daftarOpsi.Length, daftarPilihanUtama.Length) + (bisaKeluar ? 1 : 0); 
            // Sisakan jarak dari tepi gambar pertanyaan, juga saat opsi disorot.
            // Daftar tiga opsi tetap turun ke bawah, tanpa menimpa kotak cerita.
            for(int i=0;i<daftarPilihanUtama.Length;i++)
                if(daftarPilihanUtama[i]!=null)daftarPilihanUtama[i].AturTambahanTinggi(-16f-(daftarOpsi.Length>3?i*15f:0));
            if (panelKeluarKhusus != null) panelKeluarKhusus.AturTambahanTinggi(-16f);

            for (int i = 0; i < daftarOpsi.Length && i < daftarPilihanUtama.Length; i++)
            {
                if (daftarPilihanUtama[i] != null)
                {
                    daftarPilihanUtama[i].gameObject.SetActive(true); 
                    if (i < teksPilihanUtama.Length && teksPilihanUtama[i] != null) teksPilihanUtama[i].text = daftarOpsi[i].teksOpsi;
                }
            }
        }
        else
        {
            jumlahPilihanAktif = bisaKeluar ? 1 : 0; 
            if (jumlahPilihanAktif == 0) jumlahPilihanAktif = 1; 
        }

        indeksPilihan = 0; 
        UpdateVisual();
    }

    private void ResetSemuaPanel()
    {
        foreach (EfekPilihanUI panel in daftarPilihanUtama) 
        { 
            if (panel != null) { panel.AturTambahanTinggi(0); panel.gameObject.SetActive(false); }
        }

        if (panelKeluarKhusus != null) 
        {
            panelKeluarKhusus.gameObject.SetActive(false);
            if (teksKeluarKhusus != null) teksKeluarKhusus.text = "Keluar";
        }
    }

    public void PindahBawah() 
    {
        if (sedangNgetik || sedangMenutup || sedangMainkanEmote || Time.unscaledTime < inputBerikutnya || jumlahPilihanAktif <= 1) return;
        indeksPilihan++;
        if(indeksPilihan >= jumlahPilihanAktif) indeksPilihan = 0;
        UpdateVisual();
    }

    public void PindahAtas() 
    {
        if (sedangNgetik || sedangMenutup || sedangMainkanEmote || Time.unscaledTime < inputBerikutnya || jumlahPilihanAktif <= 1) return;
        indeksPilihan--;
        if(indeksPilihan < 0) indeksPilihan = jumlahPilihanAktif - 1;
        UpdateVisual();
    }

    public void UpdateVisual() 
    {
        for(int i = 0; i < daftarPilihanUtama.Length; i++) 
        {
            if (daftarPilihanUtama[i] != null) 
            {
                bool aktif = (jumlahPilihanAktif > 0) && (i == indeksPilihan);
                daftarPilihanUtama[i].sedangDipilih = aktif;
            }
        }
        
        if (panelKeluarKhusus != null) 
        {
            bool aktifKeluar = (jumlahPilihanAktif > 0) && (indeksPilihan == jumlahPilihanAktif - 1);
            if (!panelKeluarKhusus.gameObject.activeSelf) aktifKeluar = false;
            panelKeluarKhusus.sedangDipilih = aktifKeluar;
        }
    }

    private void EksekusiPilihan()
    {
        if (indeksPilihan == jumlahPilihanAktif - 1 && panelKeluarKhusus != null && panelKeluarKhusus.gameObject.activeSelf)
        {
            if (dialogManager != null) StartCoroutine(ProsesTutupDialogAnimasi());
            return; 
        }

        OpsiInteraksi[] daftarOpsi = null;
        if (DialogManager.Instance != null) daftarOpsi = DialogManager.Instance.opsiInteraksiAktif;

        if (daftarOpsi != null && indeksPilihan < daftarOpsi.Length)
        {
            OpsiInteraksi opsiDipilih = daftarOpsi[indeksPilihan];
            if(!BatasMalamChapter1.Boleh(opsiDipilih.titikTujuan,opsiDipilih.teksOpsi)){if(teksCeritaUtama!=null)teksCeritaUtama.text="Akses ke tempat ini ditutup pada malam hari. Kembali ke kamar 201 Guest House.";return;}
            if (opsiDipilih.cabangJalur != null)
            {
                if (opsiDipilih.cabangJalur.Pilih(opsiDipilih.pilihJalanDepan))
                    StartCoroutine(ProsesTutupDialogAnimasi());
                return;
            }
            if(opsiDipilih.rapidTap!=null)
            {
                StartCoroutine(BukaLatihanRapidTap(opsiDipilih.rapidTap));return;
            }
            
            if (opsiDipilih.titikTujuan != null)
            {
                JalankanTransisiTeleport(
                    opsiDipilih.titikTujuan, 
                    opsiDipilih.nyalakanGrupDepan, 
                    opsiDipilih.nyalakanGrupBelakang, 
                    opsiDipilih.nyalakanGrupRuangan
                );
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(opsiDipilih.pesanBelumTersedia))
                {
                    if (teksCeritaUtama != null) teksCeritaUtama.text = opsiDipilih.pesanBelumTersedia;
                    inputBerikutnya = Time.unscaledTime + jedaAntarInput;
                    return;
                }
                if (dialogManager != null) StartCoroutine(ProsesTutupDialogAnimasi());
            }
        }
        else
        {
            if (dialogManager != null) StartCoroutine(ProsesTutupDialogAnimasi());
        }
    }

    private void JalankanTransisiTeleport(Transform tujuan, bool areaDepan, bool areaBelakang, bool areaRuangan)
    {
        if (efekTransisi != null)
        {
            System.Action pindah = () =>
            {
                PindahPosisiArthur(tujuan); 
                if (grupFisikDepan != null) grupFisikDepan.SetActive(areaDepan);
                if (grupFisikBelakang != null) grupFisikBelakang.SetActive(areaBelakang);
                if (grupFisikRuangan != null) grupFisikRuangan.SetActive(areaRuangan);
            };
            if(KawasanKampus.Instance!=null)
                efekTransisi.MulaiTransisiTeleportRutin(() => KawasanKampus.Instance.Pindahkan(tujuan,pindah));
            else efekTransisi.MulaiTransisiTeleport(pindah);

            if (dialogManager != null) StartCoroutine(ProsesTutupDialogAnimasi());
        }
        else
        {
            Debug.LogError("CanvasLoading belum dimasukkan ke SistemPilihan!");
        }
    }

    private void PindahPosisiArthur(Transform titikTujuan)
    {
        if (titikTujuan != null && dialogManager != null && dialogManager.arthurScript != null)
        {
            Rigidbody2D rbArthur = dialogManager.arthurScript.GetComponent<Rigidbody2D>();
            if (rbArthur != null)
            {
                rbArthur.linearVelocity = Vector2.zero;
                rbArthur.angularVelocity = 0f;
            }
            var jalur = dialogManager.arthurScript.GetComponent<ArthurJalurKampus>();
            if (jalur != null) jalur.PindahKe(titikTujuan);
            else dialogManager.arthurScript.transform.position = titikTujuan.position;
            if (rbArthur != null) rbArthur.WakeUp();
        }
    }

    private IEnumerator BukaLatihanRapidTap(RapidTapArthur latihan)
    {
        yield return ProsesTutupDialogAnimasi();
        latihan.Buka();
    }
    private IEnumerator ProsesTutupDialogAnimasi()
    {
        HentikanMulut();
        if (sedangMenutup) yield break;
        sedangMenutup = true;
        SembunyikanNextKhusus();
        EfekMunculDialog efek = dialogManager.canvasDialog.GetComponent<EfekMunculDialog>();
        if (efek != null)
        {
            efek.TutupDialog();
            yield return new WaitForSeconds(efek.durasiTutup);
        }
        var pemilik = dialogManager.PemilikInteraksi;
        dialogManager.TutupDialog();
        PercakapanSelesai?.Invoke(pemilik,ceritaTuntas);
        sedangMenutup = false;
        inputBerikutnya = Time.unscaledTime + jedaSetelahTeks;
    }
}
