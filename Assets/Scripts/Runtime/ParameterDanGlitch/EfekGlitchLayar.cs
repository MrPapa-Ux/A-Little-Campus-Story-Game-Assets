// PANDUAN PROYEK - Runtime / ParameterDanGlitch
// Fungsi: Mengatur lapisan gangguan layar, teks acak, dan kotak glitch.
// Panduan: Assets/Scripts/Runtime/ParameterDanGlitch/README.md

using TMPro;
using UnityEngine;

// Pool dibuat sekali. Teks, kotak, dan error diperbarui pada tick terbatas.
public class EfekGlitchLayar : MonoBehaviour
{
    [Header("Banjir gangguan layar")]
    public TMP_FontAsset fontError;
    [Range(5,15)] public int pembaruanPerDetik = 10;
    [Range(24,42)] public float ukuranTeks = 30;
    [Range(1,6)] public int jumlahJendela = 5;
    [Range(12,70)] public int jumlahKotak = 48;
    public Color warnaTeks = new Color32(95,224,120,255);
    public Color warnaError = new Color32(255,95,106,255);

    static readonly string[] Cadangan = {"ERROR_404", "BANGUN", "ARTHUR.EXE", "MEMORY_LOST", "NULL", "JANGAN_DIAM", "011001", "SIGNAL_LOST", "SIAPA_DI_SANA", "FATAL_ERR"};
    static readonly string[] Pesan = {
        "FATAL ERROR\nARTHUR.EXE TIDAK MERESPONS\nMEMORY ADDRESS: 0x00000000\n[ COBA LAGI ]  [ GAGAL ]",
        "SIGNAL LOST\nINPUT TERPUTUS.\nJANGAN DIAM.\nJANGAN DIAM.",
        "SYSTEM FAILURE\nDATA TIDAK DAPAT DIPULIHKAN\nERROR 404 / IDENTITAS HILANG\n[ AKSES DITOLAK ]",
        "PERINGATAN\nADA PROSES YANG MASIH BERJALAN.\nSIAPA YANG MENGENDALIKAN?\nREBOOT GAGAL...",
        "NULL REFERENCE\nPLAYER / MEMORY / REALITY\nTIDAK DITEMUKAN.\n[ TUTUP ]  [ TUTUP ]"
    };
    readonly char[] buffer = new char[8192];
    readonly System.Random acak = new System.Random();
    TextMeshProUGUI dasar;
    TextMeshProUGUI[] error;
    KotakGlitchUI kotak;
    RectTransform bidang;
    string[] kata;
    float tickBerikutnya;
    bool disiapkan;
    public int JumlahPembaruan { get; private set; }

    public void Mulai(TextMeshProUGUI teks, string[] pilihanKata)
    {
        dasar=teks; kata=pilihanKata;
        if(!disiapkan) Siapkan();
        kotak.gameObject.SetActive(true);
        if(dasar!=null)
        {
            if(fontError!=null)dasar.font=fontError;
            dasar.enableAutoSizing=false;dasar.fontSize=ukuranTeks;dasar.fontStyle=FontStyles.Normal;
            dasar.textWrappingMode=TextWrappingModes.NoWrap;dasar.overflowMode=TextOverflowModes.Overflow;
            dasar.alignment=TextAlignmentOptions.TopLeft;dasar.margin=Vector4.zero;dasar.raycastTarget=false;
            var r=dasar.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;r.localRotation=Quaternion.identity;r.localScale=Vector3.one;
        }
        tickBerikutnya=-1;JumlahPembaruan=0;Tampilkan(0);
    }
    void Siapkan()
    {
        var root=new GameObject("GangguanLayar",typeof(RectTransform),typeof(Canvas));
        bidang=(RectTransform)root.transform;bidang.SetParent(transform,false);bidang.anchorMin=Vector2.zero;bidang.anchorMax=Vector2.one;bidang.sizeDelta=Vector2.zero;
        var canvas=root.GetComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=30000;
        var visual=new GameObject("KotakDanGarisRusak",typeof(RectTransform),typeof(CanvasRenderer),typeof(KotakGlitchUI));
        var r=(RectTransform)visual.transform;r.SetParent(bidang,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.sizeDelta=Vector2.zero;
        kotak=visual.GetComponent<KotakGlitchUI>();kotak.raycastTarget=false;
        error=new TextMeshProUGUI[6];
        for(int i=0;i<error.Length;i++)
        {
            var go=new GameObject("ErrorTumpuk_"+i,typeof(RectTransform),typeof(CanvasRenderer));go.transform.SetParent(bidang,false);
            var t=go.AddComponent<TextMeshProUGUI>();t.font=fontError!=null?fontError:dasar!=null?dasar.font:TMP_Settings.defaultFontAsset;
            t.enableAutoSizing=false;t.fontSize=27;t.fontStyle=FontStyles.Bold;t.textWrappingMode=TextWrappingModes.Normal;t.overflowMode=TextOverflowModes.Truncate;
            t.alignment=TextAlignmentOptions.TopLeft;t.margin=new Vector4(20,14,20,10);t.raycastTarget=false;error[i]=t;
        }
        disiapkan=true;
    }
    public void Tampilkan(float kemajuan)
    {
        if(!disiapkan||Time.unscaledTime<tickBerikutnya)return;
        tickBerikutnya=Time.unscaledTime+1f/Mathf.Clamp(pembaruanPerDetik,5,15);JumlahPembaruan++;
        float p=Mathf.Clamp01(kemajuan);var size=((RectTransform)transform).rect.size;
        if(dasar!=null)
        {
            int cols=Mathf.Clamp(Mathf.CeilToInt(size.x/(ukuranTeks*.49f))+3,35,150),rows=Mathf.Clamp(Mathf.CeilToInt(size.y/(ukuranTeks*1.1f))+1,12,48),n=0;
            for(int y=0;y<rows;y++)
            {
                int col=0;
                while(col<cols&&n<buffer.Length-cols-2)
                {
                    string word=kata!=null&&kata.Length>0&&acak.Next(3)!=0?kata[acak.Next(kata.Length)]:Cadangan[acak.Next(Cadangan.Length)];
                    if(string.IsNullOrWhiteSpace(word))word="NULL";
                    // Inspector text is rendered literally, not parsed as rich text.
                    int len=Mathf.Min(word.Length,cols-col);
                    for(int j=0;j<len;j++){buffer[n++]=word[j];col++;}buffer[n++]=' ';col++;
                }
                buffer[n++]='\n';
            }
            dasar.richText=false;dasar.color=Color.Lerp(warnaTeks,new Color(.56f,.95f,.83f),p*.4f);dasar.SetCharArray(buffer,0,n);
        }
        int aktif=Mathf.Clamp(1+Mathf.FloorToInt(p*jumlahJendela),1,Mathf.Clamp(jumlahJendela,1,6));
        kotak.Acak(acak,Mathf.Clamp(jumlahKotak,12,70),p,aktif);
        for(int i=0;i<error.Length;i++)
        {
            error[i].gameObject.SetActive(i<aktif);if(i>=aktif)continue;
            var box=kotak.Jendela[i];var r=error[i].rectTransform;r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=box.center;r.sizeDelta=box.size;
            error[i].fontSize=i==0?32:27;error[i].color=i%2==0?warnaError:new Color32(166,255,211,255);
            error[i].text=Pesan[i%Pesan.Length];
        }
    }
    public void Gelapkan()
    {
        if(dasar!=null)dasar.text="";
        if(disiapkan){kotak.gameObject.SetActive(false);foreach(var t in error)t.gameObject.SetActive(false);}
    }
}
