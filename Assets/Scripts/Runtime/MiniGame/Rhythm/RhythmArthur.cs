// PANDUAN PROYEK - Runtime / MiniGame/Rhythm
// Fungsi: Mengatur menu/ronde rhythm, jadwal audio DSP, input jalur, jeda, hasil, dan glitch kegagalan.
// Panduan: Assets/Scripts/Runtime/MiniGame/Rhythm/README.md

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RhythmArthur : MonoBehaviour
{
    public const string NamaGame = "TapTIpTup";
    public enum Fase { Tutup, Siap, Bermain, Jeda, Menang, Gagal, Glitch }
    [Header("Rhythm Arthur — not ketuk, tanpa hold")]
    [Range(70,160)] public float bpm=110;
    [Range(.5f,3)] public float waktuJatuh=1.6f;
    [Range(.04f,.15f)] public float jendelaPerfect=.09f;
    [Range(.15f,.3f)] public float jendelaGood=.22f;
    [Range(.4f,1)] public float rasioLulus=.7f;
    [Range(-150,150)] public float kalibrasiInputMs;
    public bool glitchSaatGagal=true;
    public bool jedaSaatKehilanganFokus=true;
    [Tooltip("Opsional. Waktu dalam detik sejak awal audio (termasuk intro 2 detik). Kosong = pola latihan bawaan.")]
    public NotArthur[] chartKhusus;
    public UnityEvent saatMenang=new UnityEvent(),saatGagal=new UnityEvent();
    [Header("Referensi")]
    public DialogManager dialog;
    public SistemHP hpInduk;
    public LazyBarArthur lazyBar;
    public PoseMemegangHP pose;
    public AudioSource audioBeat;
    public GameObject panel,menu;
    public PapanRhythmArthur papan;
    public TextMeshProUGUI hud,waktu,penilaian,judulMenu,isiMenu,labelUtama;
    public UnityEngine.UI.Button tombolUtama,tombolJeda,tombolKeluar;
    public Fase Status {get;private set;}
    public ArthurRhythmRound Ronde {get;private set;}
    public double WaktuLagu=>Status==Fase.Jeda?waktuSaatJeda:AudioSettings.dspTime-dspMulai;
    public float PanjangRonde {get;private set;}
    public float TempoRonde {get;private set;}
    public int LevelPilihan {get;private set;}=1;
    public int BeatRonde {get;private set;}
    public bool AdeganOtomatis {get;private set;}
    public bool NotCepatPembuka {get;set;}
    public float DurasiUntukNot(int index)=>NotCepatPembuka&&(index==4||index==6||index==8||index==10||index==11)?.045f:DurasiNot;
    public float DurasiNot=>LevelPilihan==1?2.4f:LevelPilihan==2?1.45f:.85f;
    public float JedaNot=>LevelPilihan==1?1.4f:LevelPilihan==2?.85f:.5f;
    public int JumlahNot=>LevelPilihan==1?12:LevelPilihan==2?20:32;
    UnityEngine.UI.Button[] pilihanLevel;
    double dspMulai,waktuSaatJeda;
    float feedbackHingga,bpmClip;
    int beatClip;
    readonly int[] frameInput={-1,-1,-1,-1};
    AudioClip clipBeat;
    bool punyaKunci,kunciLama,audioBelumMulai;
    Vector3 posisiAman;
    Coroutine coroutineGagal;
    GameObject panelGlitch;
    int tickHUD=-1;
    void Awake()
    {
        TerapkanTampilanPemindai();
        tombolUtama.onClick.AddListener(TekanUtama);tombolJeda.onClick.AddListener(Jeda);tombolKeluar.onClick.AddListener(Tutup);
        audioBeat.playOnAwake=false;audioBeat.loop=false;audioBeat.spatialBlend=0;Status=Fase.Tutup;panel.SetActive(false);
    }
    // Also runs at runtime so old saved scene references cannot bring back the four-lane layout.
    public void TerapkanTampilanPemindai()
    {
        var font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        foreach(var label in panel.GetComponentsInChildren<TMP_Text>(true))
        {
            if(font!=null)label.font=font;
            label.fontStyle=FontStyles.Normal;label.textWrappingMode=TextWrappingModes.Normal;
            foreach(var wiggle in label.GetComponents<WigglyText>())wiggle.enabled=false;
            if(label.name=="JudulGame"){label.text=NamaGame;label.fontSize=44;}
            else if(label.name=="Petunjuk"){label.text="Ketuk lingkaran sebelum menghilang.";label.fontSize=24;}
            else label.fontSize=Mathf.Min(label.fontSize,32);
        }
        judulMenu.text=NamaGame;labelUtama.text="MULAI";
        var fontJudul=Resources.Load<TMP_FontAsset>("Fonts & Materials/AmaticSC-Regular SDF");
        foreach(var label in panel.GetComponentsInChildren<TMP_Text>(true))
            if((label.name=="JudulGame"||label==judulMenu)&&fontJudul!=null)label.font=fontJudul;
        isiMenu.text="Ketuk lingkaran segera setelah muncul.\nTidak perlu menunggu irama atau garis.\n\nLingkaran yang terlewat dihitung Miss.";
        hud.text="SKOR 0     COMBO 0     AKURASI 0%";
        penilaian.fontSize=34;judulMenu.fontSize=52;isiMenu.fontSize=27;
        papan.raycastTarget=true;papan.rectTransform.anchoredPosition=new Vector2(0,-20);
        papan.rectTransform.sizeDelta=new Vector2(1000,450);
        foreach(var lane in panel.GetComponentsInChildren<RhythmJalurInput>(true))lane.gameObject.SetActive(false);
        penilaian.rectTransform.anchoredPosition=new Vector2(0,205);
        audioBeat.volume=.6f;
        SiapkanPilihanLevel();
    }
    void SiapkanPilihanLevel()
    {
        pilihanLevel=new UnityEngine.UI.Button[3];
        for(int i=0;i<3;i++)
        {
            string name="PilihLevel"+(i+1);var existing=menu.transform.Find(name);
            var button=existing!=null?existing.GetComponent<UnityEngine.UI.Button>():Instantiate(tombolUtama,menu.transform);
            button.name=name;button.onClick.RemoveAllListeners();int level=i+1;
            button.onClick.AddListener(()=>PilihLevel(level));
            var rect=(RectTransform)button.transform;rect.anchoredPosition=new Vector2((i-1)*275,-100);rect.sizeDelta=new Vector2(250,62);
            var label=button.GetComponentInChildren<TMP_Text>(true);label.text="LEVEL "+level+"\n"+new[]{"MUDAH","NORMAL","SULIT"}[i];label.fontSize=21;
            label.rectTransform.sizeDelta=new Vector2(240,58);
            pilihanLevel[i]=button;
        }
        ((RectTransform)tombolUtama.transform).anchoredPosition=new Vector2(0,-185);
        isiMenu.rectTransform.anchoredPosition=new Vector2(0,40);isiMenu.rectTransform.sizeDelta=new Vector2(890,155);
        SegarkanLevel();
    }
    public void PilihLevel(int level)
    {
        if(level<1||level>3||Status==Fase.Bermain||Status==Fase.Jeda||Status==Fase.Glitch)return;
        LevelPilihan=level;Ronde=null;if(Status!=Fase.Tutup)Status=Fase.Siap;
        SegarkanLevel();papan.Segarkan();
    }
    void SegarkanLevel()
    {
        if(pilihanLevel!=null)for(int i=0;i<3;i++)
        {
            pilihanLevel[i].gameObject.SetActive(true);
            bool selected=LevelPilihan==i+1;
            pilihanLevel[i].GetComponent<UnityEngine.UI.Image>().color=selected?new Color32(238,155,53,255):new Color32(49,66,78,255);
            pilihanLevel[i].GetComponentInChildren<TMP_Text>(true).color=selected?new Color32(20,28,36,255):new Color32(245,234,210,255);
        }
        judulMenu.text=NamaGame;
        isiMenu.text="Ketuk lingkaran sebelum menghilang.\n"+new[]{"Level 1: 12 lingkaran, jeda panjang, waktu tekan 2,4 detik.","Level 2: 20 lingkaran, waktu tekan 1,45 detik.","Level 3: 32 lingkaran, waktu tekan 0,85 detik."}[LevelPilihan-1];
        labelUtama.text="MULAI";waktu.text="LEVEL "+LevelPilihan;
    }
    public void Buka()
    {
        if(Status!=Fase.Tutup||dialog==null||dialog.minigameAktif||lazyBar!=null&&lazyBar.waktuHabis)return;
        var hp=FindFirstObjectByType<SistemHP>();if(hp!=null&&hp.hpSedangBuka&&hpInduk!=hp)return;
        punyaKunci=true;dialog.minigameAktif=true;
        var p=dialog.arthurScript;
        if(p!=null){kunciLama=p.kunciPergerakan;posisiAman=p.transform.position;p.BerhentiBergerak();p.kunciPergerakan=true;}
        if(pose!=null&&!pose.SedangMemegang)pose.Buka(hp!=null?hp.warnaDefaultHP:new Color(1,.5f,0));
        if(MusikArthur.Instance!=null)MusikArthur.Instance.TahanMinigame(true);
        panel.SetActive(true);Status=Fase.Siap;Ronde=null;papan.Segarkan();menu.SetActive(true);
        judulMenu.text=NamaGame;
        isiMenu.text="Ketuk lingkaran segera setelah muncul.\nTidak perlu menunggu irama atau garis.\n\nLingkaran yang terlewat dihitung Miss.";
        labelUtama.text="MULAI";hud.text="SKOR 0     COMBO 0";waktu.text=Mathf.Clamp(bpm,70,160).ToString("0")+" BPM";penilaian.text="";
        tombolJeda.interactable=false;tombolKeluar.interactable=true;
        SegarkanLevel();
    }
    // Pintu masuk khusus adegan cerita. Permainan eksplorasi tetap memakai Buka().
    public void MulaiAdeganOtomatis()
    {
        if(Status==Fase.Bermain||Status==Fase.Jeda||Status==Fase.Glitch)return;
        AdeganOtomatis=true;LevelPilihan=1;Status=Fase.Siap;panel.SetActive(true);
        papan.raycastTarget=false;MulaiRonde();
        tombolJeda.gameObject.SetActive(false);tombolKeluar.gameObject.SetActive(false);
    }
    public void TekanUtama()
    {
        if(Status==Fase.Jeda){Lanjutkan();return;}
        if(Status==Fase.Siap||Status==Fase.Menang||Status==Fase.Gagal)MulaiRonde();
    }
    public static NotArthur[] BuatChart(float tempo)
    {
        var notes=new List<NotArthur>();int[] lanes={0,1,2,3,2,1,0,2};float step=60/Mathf.Clamp(tempo,70,160);
        for(int b=0;b<32;b++)
        {
            notes.Add(new NotArthur(lanes[b%8],2+b*step));
            if((b>=8&&b%4==2)||(b>=16&&b%4==1))notes.Add(new NotArthur((lanes[b%8]+1)%4,2+(b+.5f)*step));
        }
        return notes.ToArray();
    }
    public static NotArthur[] BuatChartLevel(int level,float tempo)
    {
        level=Mathf.Clamp(level,1,3);
        int count=level==1?12:level==2?20:32;
        float interval=level==1?1.4f:level==2?.85f:.5f;
        var notes=new NotArthur[count];
        for(int i=0;i<count;i++)notes[i]=new NotArthur(i%4,1+i*interval);
        return notes;
    }
    public void MulaiRonde()
    {
        if(Status!=Fase.Siap&&Status!=Fase.Menang&&Status!=Fase.Gagal)return;
        TempoRonde=112;BeatRonde=40;
        var chart=BuatChartLevel(LevelPilihan,TempoRonde);
        Ronde=new ArthurRhythmRound(chart,.09f,.22f,DurasiNot);
        if(NotCepatPembuka){var durations=new float[chart.Length];for(int i=0;i<durations.Length;i++)durations[i]=DurasiUntukNot(i);Ronde.durasiPerNot=durations;}
        PanjangRonde=chart[chart.Length-1].waktu+DurasiNot+.25f;
        if(clipBeat==null)
        {
            clipBeat=ArthurRhythmAudio.Buat(112,40,0);bpmClip=112;beatClip=40;
        }
        audioBeat.Stop();audioBeat.volume=.6f;audioBeat.clip=clipBeat;dspMulai=AudioSettings.dspTime+.10;audioBeat.PlayScheduled(dspMulai);
        Status=Fase.Bermain;menu.SetActive(false);tombolJeda.interactable=true;tickHUD=-1;penilaian.text="";
        for(int i=0;i<4;i++){frameInput[i]=-1;papan.kilat[i]=0;}SegarkanHUD();
    }
    void Update()
    {
        if(Status==Fase.Tutup)return;
        if(!AdeganOtomatis&&Input.GetKeyDown(KeyCode.Escape)){if(Status==Fase.Bermain)Jeda();else if(Status==Fase.Jeda)Lanjutkan();else Tutup();return;}
        if(Status!=Fase.Bermain)return;
        double t=WaktuLagu;
        audioBeat.volume=.6f*Mathf.Clamp01((PanjangRonde-(float)t)/.3f);
        if(Ronde.Lewatkan(t))Feedback("MISS",new Color(1,.37f,.38f));
        if(Ronde.Energi<=0){Selesai(false);return;}
        if(t>=PanjangRonde){Selesai(Ronde.Lulus(LevelPilihan==1?.6f:LevelPilihan==2?.65f:.7f));return;}
        for(int i=0;i<4;i++)papan.kilat[i]=Mathf.MoveTowards(papan.kilat[i],0,Time.unscaledDeltaTime*4);
        if(Time.unscaledTime>feedbackHingga)penilaian.text="";
        int tick=Mathf.FloorToInt((float)t*10);if(tick!=tickHUD){tickHUD=tick;SegarkanHUD();}
        papan.Segarkan();
    }
    public void PukulJalur(int lane)
    {
        if(Status!=Fase.Bermain||lane<0||lane>3||frameInput[lane]==Time.frameCount)return;
        double t=WaktuLagu;if(t<.3||t>=PanjangRonde)return;
        frameInput[lane]=Time.frameCount;Ronde.Lewatkan(t);if(Ronde.Energi<=0){Selesai(false);return;}var nilai=Ronde.Tekan(lane,t+kalibrasiInputMs/1000.0);
        papan.kilat[lane]=1;
        Feedback(nilai==ArthurRhythmRound.Nilai.Kosong?"MISS":nilai==ArthurRhythmRound.Nilai.Perfect?"PERFECT":"GOOD",nilai==ArthurRhythmRound.Nilai.Kosong?new Color(1,.4f,.4f):new Color(1,.84f,.45f));
        SegarkanHUD();papan.Segarkan();if(Ronde.Energi<=0)Selesai(false);
    }
    public void PukulNot(int index)
    {
        if(Status!=Fase.Bermain||index<0||index>=Ronde.not.Length)return;
        int lane=Ronde.not[index].jalur;
        if(Ronde.hasil[index]!=0)return;
        double t=WaktuLagu;if(t<.3||t>=PanjangRonde)return;
        frameInput[lane]=Time.frameCount;
        Ronde.Lewatkan(t);
        if(Ronde.Energi<=0){Selesai(false);return;}
        var nilai=Ronde.TekanNot(index,t);
        Feedback(nilai==ArthurRhythmRound.Nilai.Kosong?"MISS":nilai==ArthurRhythmRound.Nilai.Perfect?"PERFECT":"GOOD",nilai==ArthurRhythmRound.Nilai.Kosong?new Color(1,.4f,.4f):new Color(1,.84f,.45f));
        SegarkanHUD();papan.Segarkan();if(Ronde.Energi<=0)Selesai(false);
    }
    void Feedback(string text,Color color){penilaian.text=text;penilaian.color=color;feedbackHingga=Time.unscaledTime+.45f;}
    void SegarkanHUD()
    {
        hud.SetText("LV "+LevelPilihan+"     SKOR {0}     COMBO {1}     AKURASI {2:1}%",Ronde.Skor,Ronde.Combo,Ronde.AkurasiSaatIni);
        int s=Mathf.CeilToInt(Mathf.Max(0,PanjangRonde-(float)WaktuLagu));waktu.text=(s/60).ToString("00")+":"+(s%60).ToString("00");
    }
    public void Jeda()
    {
        if(Status!=Fase.Bermain)return;
        waktuSaatJeda=WaktuLagu;audioBelumMulai=waktuSaatJeda<0;
        if(audioBelumMulai)audioBeat.Stop();else audioBeat.Pause();Status=Fase.Jeda;
        menu.SetActive(true);judulMenu.text="JEDA";isiMenu.text="Permainan dijeda.\nTekan Lanjut untuk melanjutkan permainan.";labelUtama.text="LANJUT";tombolJeda.interactable=false;
        if(pilihanLevel!=null)foreach(var b in pilihanLevel)b.gameObject.SetActive(false);
    }
    public void Lanjutkan()
    {
        if(Status!=Fase.Jeda)return;
        dspMulai=AudioSettings.dspTime-waktuSaatJeda;if(audioBelumMulai)audioBeat.PlayScheduled(dspMulai);else audioBeat.UnPause();
        Status=Fase.Bermain;menu.SetActive(false);tombolJeda.interactable=true;
        if(pilihanLevel!=null)foreach(var b in pilihanLevel)b.gameObject.SetActive(true);
    }
    void Selesai(bool win)
    {
        audioBeat.Stop();Status=win?Fase.Menang:Fase.Gagal;tombolJeda.interactable=false;menu.SetActive(true);
        judulMenu.text=win?"SELESAI":"GAGAL";
        isiMenu.text="SKOR "+Ronde.Skor+"   /   AKURASI "+Ronde.Akurasi.ToString("0.0")+"%\nPERFECT "+Ronde.Perfect+"   GOOD "+Ronde.Good+"   MISS "+Ronde.Miss+"\nMAX COMBO "+Ronde.MaxCombo;
        labelUtama.text="ULANGI";penilaian.text="";
        if(win)saatMenang.Invoke();else
        {
            if(dialog!=null&&dialog.arthurScript!=null){dialog.arthurScript.BerhentiBergerak();dialog.arthurScript.transform.position=posisiAman;}
            saatGagal.Invoke();if(Status==Fase.Gagal&&glitchSaatGagal&&lazyBar!=null&&lazyBar.panelGlitch!=null)coroutineGagal=StartCoroutine(Glitch());
        }
    }
    IEnumerator Glitch()
    {
        Status=Fase.Glitch;panelGlitch=lazyBar.panelGlitch;panelGlitch.SetActive(true);var fx=lazyBar.efekGlitchLayar;if(fx!=null)fx.Mulai(lazyBar.teksGlitch,lazyBar.kataGlitch);
        var image=panelGlitch.GetComponent<UnityEngine.UI.Image>();if(image!=null){image.sprite=null;image.type=UnityEngine.UI.Image.Type.Simple;image.color=new Color(0,0,0,.85f);}
        float end=Time.unscaledTime+.65f;while(Time.unscaledTime<end){if(fx!=null)fx.Tampilkan(1-(end-Time.unscaledTime)/.65f);yield return null;}
        BersihkanGlitch();Status=Fase.Gagal;coroutineGagal=null;
    }
    void BersihkanGlitch(){if(panelGlitch!=null){var fx=panelGlitch.GetComponent<EfekGlitchLayar>();if(fx!=null)fx.Gelapkan();panelGlitch.SetActive(false);panelGlitch=null;}}
    public void Tutup()
    {
        if(Status!=Fase.Tutup&&hpInduk!=null&&hpInduk.isActiveAndEnabled&&hpInduk.GameSedangBuka){hpInduk.TutupAplikasi();return;}
        TutupDariHP();
    }
    public void TutupDariHP()
    {
        if(coroutineGagal!=null){StopCoroutine(coroutineGagal);coroutineGagal=null;}BersihkanGlitch();
        if(audioBeat!=null)audioBeat.Stop();Status=Fase.Tutup;if(panel!=null)panel.SetActive(false);AdeganOtomatis=false;
        if(punyaKunci)
        {
            bool hpMasihBuka=hpInduk!=null&&hpInduk.hpSedangBuka;
            if(dialog!=null){dialog.minigameAktif=false;if(dialog.arthurScript!=null)dialog.arthurScript.kunciPergerakan=hpMasihBuka||kunciLama;}
            if(pose!=null&&!hpMasihBuka)pose.Tutup();if(MusikArthur.Instance!=null)MusikArthur.Instance.TahanMinigame(false);
        }
        punyaKunci=false;
    }
    void OnApplicationPause(bool pause){if(pause)Jeda();}
    void OnApplicationFocus(bool focus){if(!focus&&jedaSaatKehilanganFokus)Jeda();}
    void OnDisable(){TutupDariHP();}
    void OnDestroy(){if(clipBeat!=null)Destroy(clipBeat);}
}
