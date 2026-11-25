using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PratinjauMinigameBayangan : MonoBehaviour
{
    public enum Arena { Istana, Edelweiss }
    enum Fase { Siap, Main, Menang, Kalah }

    public Arena arena;
    public GayaMinigameBayangan gaya;
    public AudioClip laguArena;
    [HideInInspector] public bool keluarKeKampus;
    readonly MusikMinigameArthur musik = new MusikMinigameArthur();
    public Texture2D latar, tokoh, stress, burnout, pedang;

    const int TotalGelombang = 3;
    const float Lantai = -174f;
    const float BatasX = 522f;
    const float DurasiTebas = .34f;
    const float LepasTebas = .12f;
    static readonly Color32 Gelap = new Color32(17, 26, 39, 250);
    static readonly Color32 Terang = new Color32(244, 241, 219, 255);
    static readonly Color32 Tenang = new Color32(142, 226, 216, 255);
    static readonly Color32 Pulih = new Color32(255, 195, 142, 255);
    static readonly Color32 Redup = new Color32(76, 65, 57, 255);
    static readonly Color32 Kertas = new Color32(250, 234, 204, 255);
    static readonly Color32 AksenKertas = new Color32(211, 142, 63, 255);
    static readonly string[] Bayangan = { "STRESS", "BURNOUT" };
    static readonly string[] Motivasi = { "TENANG", "PULIH" };

    sealed class Lawan
    {
        public RectTransform akar;
        public RawImage gambar;
        public TextMeshProUGUI label, nyawa;
        public Vector2 posisi;
        public int tipe, hp;
        public float jedaSerang, kedip;
    }

    sealed class Proyektil
    {
        public RectTransform akar;
        public Image garis;
        public RawImage sabit;
        public TextMeshProUGUI label;
        public Vector2 posisi, kecepatan;
        public int tipe;
    }

    readonly Lawan[] musuh = new Lawan[6];
    readonly Proyektil[] peluru = new Proyektil[22];
    readonly Button[] kartuKata = new Button[2];
    RectTransform panel, pemain, bilah, arenaUI, tombolMulai;
    Button keluarArena, keluarHasil;
    RawImage gambarArthur;
    Image[] potonganArthur;
    int frameArthur = -2;
    float waktuLari;
    TextMeshProUGUI judulPanel, isiPanel, judulTombol, info, hati, umpanBalik;
    Fase fase;
    Vector2 posisiPemain;
    int pilihan, arah = 1, arahTebas = 1, kataTebas, ronde, nyawa, dikalahkan;
    bool kataDilepas;
    bool kiri, kanan;
    float umur, tembak, kebal, ayun, antarRonde, pesan, ulang;
    bool Pesawat => arena == Arena.Edelweiss;

    void Awake()
    {
        Application.targetFrameRate = 60;
        var kanvas = new GameObject("Arena 2D", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        kanvas.transform.SetParent(transform, false);
        kanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var skala = kanvas.GetComponent<CanvasScaler>();
        skala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        skala.referenceResolution = new Vector2(1280, 720);
        skala.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        var layar = (RectTransform)kanvas.transform;
        var dasar = Kotak(layar, "Latar layar", Vector2.zero, Vector2.zero, Gelap).rectTransform;
        dasar.anchorMin = Vector2.zero;
        dasar.anchorMax = Vector2.one;
        dasar.offsetMin = dasar.offsetMax = Vector2.zero;

        arenaUI = Rect(layar, "Panggung", Vector2.zero, new Vector2(1280, 720));
        Foto(arenaUI, "Latar arena", Vector2.zero, arenaUI.sizeDelta, latar);
        Kotak(arenaUI, "Pita status", new Vector2(0, 314), new Vector2(1280, 92), Kertas);
        Kotak(arenaUI, "Batas status", new Vector2(0, 268), new Vector2(1280, 3), AksenKertas);
        Teks(arenaUI, "Lokasi", new Vector2(-412, 334), new Vector2(380, 22), Pesawat ? "ASRAMA EDELWEISS  /  SHOOTER" : "GK1  /  HACK AND SLASH", 16,
            Redup, TextAlignmentOptions.MidlineLeft);
        Teks(arenaUI, "Judul", new Vector2(-412, 300), new Vector2(380, 44), Pesawat ? "Langit Edel" : "Pedang Harapan", 33,
            Gelap, TextAlignmentOptions.MidlineLeft).fontStyle = FontStyles.Bold;
        info = Teks(arenaUI, "Gelombang", new Vector2(30, 315), new Vector2(380, 45), "", 25, Gelap);
        hati = Teks(arenaUI, "Nyawa", new Vector2(332, 315), new Vector2(190, 45), "", 25, Gelap);
        keluarArena = Tombol(arenaUI, "Keluar arena", new Vector2(534, 314), new Vector2(146, 50),
            "Keluar", 26, Kertas, KembaliKeAwal);
        umpanBalik = Teks(arenaUI, "Umpan balik", new Vector2(0, 228), new Vector2(650, 44), "", 22, Tenang);

        pemain = Rect(arenaUI, "Arthur", Vector2.zero, new Vector2(128, 128));
        if (Pesawat)
            gambarArthur = Foto(pemain, "Pesawat Arthur", new Vector2(0, -12), new Vector2(104, 88), tokoh);
        var pose = Pesawat ? gaya.kepala : gaya.diam;
        potonganArthur = new Image[pose.bagian.Length];
        for (int i = 0; i < potonganArthur.Length; i++)
            potonganArthur[i] = Kotak(pemain, "Sprite Arthur", Vector2.zero, Vector2.zero, Color.white);
        TampilkanArthur(pose);

        if (!Pesawat)
        {
            bilah = Foto(pemain, "Pedang", Vector2.zero, new Vector2(86, 49), pedang).rectTransform;
            bilah.pivot = new Vector2(.13f, .23f);
            bilah.SetSiblingIndex(potonganArthur[gaya.tanganKanan].transform.GetSiblingIndex());
            AturGenggaman(pose);
        }
        pemain.anchoredPosition = new Vector2(Pesawat ? 0 : -228, Lantai);

        for (int i = 0; i < musuh.Length; i++)
        {
            var akar = Rect(arenaUI, "Shadow", Vector2.zero, new Vector2(156, 100));
            musuh[i] = new Lawan
            {
                akar = akar,
                gambar = Foto(akar, "Wujud", Vector2.zero, new Vector2(86, 84), stress),
                label = Teks(akar, "Nama", new Vector2(0, 56), new Vector2(140, 27), "", 19, Terang),
                nyawa = Teks(akar, "Ketahanan", new Vector2(0, -41), new Vector2(126, 18), "", 13, Terang)
            };
            akar.gameObject.SetActive(false);
        }

        for (int i = 0; i < peluru.Length; i++)
        {
            var garis = Kotak(arenaUI, "Tembakan", Vector2.zero, new Vector2(102, 30), Tenang);
            var sabit = Foto(garis.rectTransform, "Lengkung sabit", Vector2.zero, new Vector2(161, 84), gaya.sabit);
            var label = Teks(garis.rectTransform, "Kata", Vector2.zero, new Vector2(100, 31), "", 25, Gelap);
            label.fontStyle = FontStyles.Bold;
            peluru[i] = new Proyektil { akar = garis.rectTransform, garis = garis, label = label, sabit = sabit };
            garis.gameObject.SetActive(false);
        }

        var kontrol = Rect(arenaUI, "Kontrol", Vector2.zero, Vector2.zero);
        Kotak(kontrol, "Pita kontrol", new Vector2(0, -305), new Vector2(1280, 110), Kertas);
        Kotak(kontrol, "Batas kontrol", new Vector2(0, -250), new Vector2(1280, 3), AksenKertas);
        var kiriUI = Tombol(kontrol, "Kiri", new Vector2(-544, -305), new Vector2(102, 78), "", 35, Color.white, null, gaya.kiri);
        var kananUI = Tombol(kontrol, "Kanan", new Vector2(-427, -305), new Vector2(102, 78), "", 35, Color.white, null, gaya.kanan);
        Tahan(kiriUI, ditekan => kiri = ditekan);
        Tahan(kananUI, ditekan => kanan = ditekan);
        kartuKata[0] = Tombol(kontrol, "Pilih Tenang", new Vector2(-125, -300), new Vector2(214, 62), "1  TENANG", 22, Tenang, () => PilihKata(0));
        kartuKata[1] = Tombol(kontrol, "Pilih Pulih", new Vector2(113, -300), new Vector2(214, 62), "2  PULIH", 22, Pulih, () => PilihKata(1));
        Teks(kontrol, "Lawan Stress", new Vector2(-125, -341), new Vector2(214, 22), "Melawan Stress", 19, Redup);
        Teks(kontrol, "Lawan Burnout", new Vector2(113, -341), new Vector2(214, 22), "Melawan Burnout", 19, Redup);
        if (Pesawat)
            Teks(kontrol, "Mode tembakan", new Vector2(474, -305), new Vector2(270, 57), "Tembak otomatis", 26, Gelap);
        else
        {
            Tombol(kontrol, "Tebas", new Vector2(442, -305), new Vector2(87, 84), "", 23, Color.white, Serang, gaya.aksi);
            Teks(kontrol, "Petunjuk tebas", new Vector2(549, -305), new Vector2(108, 68), "Tebas\nSpasi", 24, Gelap);
        }

        var tirai = Kotak(arenaUI, "Layar awal dan hasil", Vector2.zero, arenaUI.sizeDelta, new Color(0.02f, 0.05f, 0.08f, .55f));
        tirai.raycastTarget = true;
        panel = tirai.rectTransform;
        var kertas = Kotak(panel, "Kertas Tutorial Arthur", new Vector2(0, 17), new Vector2(688, 402), Color.white);
        kertas.sprite = gaya.kertas;
        var kartu = kertas.rectTransform;
        judulPanel = Teks(kartu, "Judul panel", new Vector2(0, 102), new Vector2(540, 65), "", 55, Gelap);
        judulPanel.fontStyle = FontStyles.Bold;
        isiPanel = Teks(kartu, "Panduan", new Vector2(0, 10), new Vector2(480, 102), "", 28, Gelap);
        var mulai = Tombol(kartu, "Mulai atau ulangi", new Vector2(0, -102), new Vector2(228, 65), "Mulai", 30, Pulih, Mulai);
        judulTombol = mulai.GetComponentInChildren<TextMeshProUGUI>();
        tombolMulai = (RectTransform)mulai.transform;
        keluarHasil = Tombol(kartu, "Keluar hasil", new Vector2(126, -102), new Vector2(200, 65),
            "Keluar", 28, Kertas, KembaliKeAwal);
        if (EventSystem.current == null)
        {
            var masukan = new GameObject("Input pratinjau", typeof(EventSystem), typeof(StandaloneInputModule));
            masukan.transform.SetParent(transform, false);
        }
        PilihKata(0);
        KembaliKeAwal();
        musik.Putar(gameObject, laguArena);
    }

    void Mulai()
    {
        ResetArena();
        fase = Fase.Main;
        musik.Putar(gameObject, laguArena);
        panel.gameObject.SetActive(false);
        keluarArena.gameObject.SetActive(true);
        GelombangBerikut();
    }

    void OnDisable() => musik.Tutup();

    void KembaliKeAwal()
    {
        if(keluarKeKampus){Destroy(gameObject);return;}
        musik.Tutup();
        ResetArena();
        Status();
        TampilkanPanel(Fase.Siap);
    }

    void ResetArena()
    {
        kiri = kanan = false;
        arah = 1;
        nyawa = 4;
        ronde = dikalahkan = 0;
        umur = kebal = ayun = antarRonde = pesan = 0;
        tembak = Pesawat ? .35f : 0;
        waktuLari = 0;
        frameArthur = -2;
        posisiPemain = new Vector2(Pesawat ? 0 : -228, Lantai);
        pemain.anchoredPosition = posisiPemain;
        pemain.localScale = Vector3.one;
        if (gambarArthur != null) gambarArthur.color = Color.white;
        TampilkanArthur(Pesawat ? gaya.kepala : gaya.diam);
        umpanBalik.text = "";
        foreach (var l in musuh) l.akar.gameObject.SetActive(false);
        foreach (var p in peluru) p.akar.gameObject.SetActive(false);
        PilihKata(0);
        if (!Pesawat) AturGenggaman(gaya.diam);
    }

    void OnApplicationFocus(bool fokus)
    {
        if (!fokus) kiri = kanan = false;
    }

    void Update()
    {
        float dt = Mathf.Min(.05f, Time.unscaledDeltaTime);
        if (fase != Fase.Siap && Input.GetKeyDown(KeyCode.Escape)) { KembaliKeAwal(); return; }
        if (fase != Fase.Main)
        {
            ulang -= dt;
            if (ulang <= 0 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))) Mulai();
            return;
        }

        umur += dt;
        kebal -= dt;
        tembak -= dt;
        if (pesan > 0 && (pesan -= dt) <= 0) umpanBalik.text = "";
        if (Input.GetKeyDown(KeyCode.Alpha1)) PilihKata(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) PilihKata(1);

        bool keKiri = kiri || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool keKanan = kanan || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        int gerak = (keKanan ? 1 : 0) - (keKiri ? 1 : 0);
        posisiPemain.x = Mathf.Clamp(posisiPemain.x + gerak * (Pesawat ? 415 : 335) * dt, -BatasX, BatasX);
        if (!Pesawat && gerak != 0 && ayun <= 0) arah = gerak;
        if (Pesawat || Input.GetKeyDown(KeyCode.Space)) Serang();
        if (!Pesawat) PerbaruiTebas(dt);
        pemain.anchoredPosition = posisiPemain + Vector2.up * (Pesawat && gerak != 0 ? Mathf.Sin(umur * 22) * 2.8f : 0);
        pemain.localScale = new Vector3(Pesawat ? 1 : ayun > 0 ? arahTebas : arah, 1, 1);
        bool berkedip = kebal > 0 && Mathf.FloorToInt(kebal * 14) % 2 == 0;
        if (Pesawat)
        {
            gambarArthur.color = new Color(1, 1, 1, berkedip ? .38f : 1);
            foreach (var bagian in potonganArthur) bagian.canvasRenderer.SetAlpha(berkedip ? .38f : 1);
        }
        else
        {
            waktuLari = gerak != 0 ? Mathf.Repeat(waktuLari + dt, gaya.durasiLari) : 0;
            int frame = gerak != 0 ? Mathf.Min(gaya.lari.Length - 1, (int)(waktuLari / gaya.durasiLari * gaya.lari.Length)) : -1;
            var pose = frame >= 0 ? gaya.lari[frame] : gaya.diam;
            if (frame != frameArthur)
            {
                frameArthur = frame;
                TampilkanArthur(pose);
            }
            AturGenggaman(pose);
            foreach (var bagian in potonganArthur) bagian.canvasRenderer.SetAlpha(berkedip ? .38f : 1);
        }
        PerbaruiMusuh(dt);
        if (fase != Fase.Main) return;
        PerbaruiPeluru(dt);

        foreach (var l in musuh) if (l.akar.gameObject.activeSelf) return;
        if (antarRonde == 0)
        {
            if (ronde == TotalGelombang) { TampilkanPanel(Fase.Menang); return; }
            antarRonde = .85f;
            Kabar("Gelombang berikutnya…", Terang, .9f);
            foreach (var p in peluru) p.akar.gameObject.SetActive(false);
        }
        else if ((antarRonde -= dt) <= 0)
        {
            antarRonde = 0;
            GelombangBerikut();
        }
    }

    void TampilkanArthur(GayaMinigameBayangan.Pose pose)
    {
        for (int i = 0; i < pose.bagian.Length; i++)
        {
            var bagian = pose.bagian[i];
            var gambar = potonganArthur[i];
            gambar.sprite = bagian.sprite;
            gambar.color = bagian.warna;
            gambar.rectTransform.anchoredPosition = bagian.posisi + (Pesawat ? new Vector2(0, 26) : Vector2.zero);
            gambar.rectTransform.sizeDelta = bagian.ukuran;
            gambar.rectTransform.localRotation = Quaternion.Euler(0, 0, bagian.sudut);
        }
    }

    void AturGenggaman(GayaMinigameBayangan.Pose pose)
    {
        int indeks = gaya.tanganKanan;
        var posisiTangan = pose.bagian[indeks].posisi;
        float sudutTangan = pose.bagian[indeks].sudut;
        float sudutPedang = -18 + Mathf.Clamp(Mathf.DeltaAngle(gaya.diam.bagian[indeks].sudut, sudutTangan), -24, 24) * .55f;
        Vector2 geserTangan = Vector2.zero;
        float ayunTangan = 0;
        float miringBadan = 0;
        if (ayun > 0)
        {
            float t = DurasiTebas - ayun;
            if (t < LepasTebas)
            {
                float tarik = Mathf.SmoothStep(0, 1, t / LepasTebas);
                geserTangan = new Vector2(-21, 34) * tarik;
                ayunTangan = 42 * tarik;
                miringBadan = 13 * tarik;
            }
            else
            {
                float hentak = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(LepasTebas, .19f, t));
                float kembali = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.19f, DurasiTebas, t));
                geserTangan = Vector2.Lerp(new Vector2(-21, 34), new Vector2(33, -4), hentak) * kembali;
                ayunTangan = Mathf.Lerp(42, -55, hentak) * kembali;
                miringBadan = Mathf.Lerp(13, -15, hentak) * kembali;
            }
        }

        var tangan = potonganArthur[indeks].rectTransform;
        tangan.anchoredPosition = posisiTangan + geserTangan;
        tangan.localRotation = Quaternion.Euler(0, 0, sudutTangan + ayunTangan);
        bilah.anchoredPosition = tangan.anchoredPosition;
        bilah.localRotation = Quaternion.Euler(0, 0, sudutPedang + ayunTangan);

        var badan = potonganArthur[gaya.badan].rectTransform;
        badan.localRotation = Quaternion.Euler(0, 0, pose.bagian[gaya.badan].sudut + miringBadan);
    }

    void PerbaruiTebas(float dt)
    {
        if (ayun <= 0) return;
        ayun = Mathf.Max(0, ayun - dt);
        if (kataDilepas || DurasiTebas - ayun < LepasTebas) return;
        kataDilepas = true;
        Vector2 sasaran = Vector2.right * arahTebas;
        Lontar(posisiPemain + sasaran * 61, sasaran * 536, kataTebas);
    }

    void GelombangBerikut()
    {
        ronde++;
        int jumlah = ronde + 1;
        for (int i = 0; i < jumlah; i++)
        {
            var l = musuh[i];
            l.tipe = (i + ronde - 1) % 2;
            l.hp = ronde == 3 ? 2 : 1;
            l.jedaSerang = 1.9f + i * .46f;
            l.kedip = 0;
            l.posisi = Pesawat
                ? new Vector2((i - (jumlah - 1) * .5f) * 252, 140 + (i % 2) * 45)
                : new Vector2((i % 2 == 0 ? 1 : -1) * (457 + i / 2 * 122), Lantai);
            l.gambar.texture = l.tipe == 0 ? stress : burnout;
            l.gambar.color = Color.white;
            l.label.text = Bayangan[l.tipe];
            l.label.color = Warna(l.tipe);
            l.nyawa.text = l.hp == 2 ? "● ●" : "●";
            l.akar.anchoredPosition = l.posisi;
            l.akar.gameObject.SetActive(true);
        }
        Status();
    }

    void PerbaruiMusuh(float dt)
    {
        for (int i = 0; i < musuh.Length; i++)
        {
            var l = musuh[i];
            if (!l.akar.gameObject.activeSelf) continue;
            l.kedip -= dt;
            l.gambar.color = l.kedip > 0 ? new Color(1, .64f, .6f) : Color.white;
            if (Pesawat)
            {
                l.posisi.x = Mathf.Clamp(l.posisi.x + Mathf.Sin(umur * 1.7f + i) * 74 * dt, -BatasX, BatasX);
                l.posisi.y = Mathf.Max(-80, l.posisi.y - (11 + 5 * ronde) * dt);
                if ((l.jedaSerang -= dt) <= 0)
                {
                    Lontar(l.posisi - Vector2.up * 33, Vector2.down * (183 + 26 * ronde), -1);
                    l.jedaSerang = 2.45f + i * .18f;
                }
            }
            else
            {
                l.posisi.x = Mathf.MoveTowards(l.posisi.x, posisiPemain.x, (74 + 16 * ronde) * dt);
                if (Mathf.Abs(l.posisi.x - posisiPemain.x) < 50) KenaPemain();
            }
            l.akar.anchoredPosition = l.posisi + Vector2.up * (Mathf.Sin(umur * 6.2f + i) * 6f);
        }
    }

    void PilihKata(int tipe)
    {
        pilihan = tipe;
        for (int i = 0; i < kartuKata.Length; i++)
        {
            var teks = kartuKata[i].GetComponentInChildren<TextMeshProUGUI>();
            kartuKata[i].image.color = i == pilihan ? Warna(i) : new Color32(233, 213, 176, 255);
            teks.color = Gelap;
        }
    }

    void Serang()
    {
        if (fase != Fase.Main || tembak > 0) return;
        tembak = Pesawat ? .52f : .41f;
        if (Pesawat)
        {
            Lontar(posisiPemain + Vector2.up * 53, Vector2.up * 410, pilihan);
            return;
        }
        ayun = DurasiTebas;
        kataDilepas = false;
        kataTebas = pilihan;
        arahTebas = arah;
    }

    void Lontar(Vector2 titik, Vector2 laju, int tipe)
    {
        foreach (var p in peluru)
        {
            if (p.akar.gameObject.activeSelf) continue;
            p.tipe = tipe;
            p.posisi = titik;
            p.kecepatan = laju;
            p.akar.sizeDelta = tipe < 0 ? new Vector2(16, 24) : new Vector2(102, 30);
            p.label.text = tipe < 0 ? "" : Motivasi[tipe];
            p.sabit.enabled = tipe >= 0;
            p.garis.color = tipe < 0 ? new Color32(199, 113, 160, 255) : new Color(0, 0, 0, 0);
            if (tipe >= 0)
            {
                p.sabit.color = Warna(tipe);
                p.sabit.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(laju.y, laju.x) * Mathf.Rad2Deg);
                p.label.rectTransform.anchoredPosition = laju.y != 0 ? new Vector2(0, 24) : new Vector2(Mathf.Sign(laju.x) * 26, 5);
                p.label.color = Terang;
            }
            p.akar.anchoredPosition = titik;
            p.akar.gameObject.SetActive(true);
            return;
        }
    }

    void PerbaruiPeluru(float dt)
    {
        foreach (var p in peluru)
        {
            if (!p.akar.gameObject.activeSelf) continue;
            p.posisi += p.kecepatan * dt;
            p.akar.anchoredPosition = p.posisi;
            if (Mathf.Abs(p.posisi.x) > 620 || p.posisi.y < -238 || p.posisi.y > 253)
            {
                p.akar.gameObject.SetActive(false);
                continue;
            }
            if (p.tipe < 0)
            {
                if ((p.posisi - posisiPemain).sqrMagnitude < 36 * 36)
                {
                    p.akar.gameObject.SetActive(false);
                    KenaPemain();
                }
                continue;
            }

            foreach (var l in musuh)
            {
                if (!l.akar.gameObject.activeSelf) continue;
                Vector2 selisih = p.posisi - l.posisi;
                if (Mathf.Abs(selisih.x) > 57 || Mathf.Abs(selisih.y) > 42) continue;
                p.akar.gameObject.SetActive(false);
                if (p.tipe == l.tipe)
                {
                    l.hp--;
                    l.kedip = .13f;
                    if (l.hp == 0)
                    {
                        l.akar.gameObject.SetActive(false);
                        dikalahkan++;
                        Status();
                    }
                    else l.nyawa.text = "●";
                    Kabar(Motivasi[p.tipe] + (l.hp == 0 ? " mengalahkan " : " melemahkan ") + Bayangan[l.tipe], Warna(p.tipe));
                }
                else Kabar("Coba " + Motivasi[l.tipe] + " untuk melawan " + Bayangan[l.tipe], Warna(l.tipe));
                break;
            }
        }
    }

    void KenaPemain()
    {
        if (kebal > 0 || fase != Fase.Main) return;
        kebal = 1.15f;
        nyawa--;
        Status();
        if (nyawa == 0) TampilkanPanel(Fase.Kalah);
        else Kabar("Hati-hati, Arthur!", Pulih);
    }

    void Status()
    {
        info.text = ronde == 0 ? TotalGelombang + " gelombang" : "Gelombang " + ronde + " / " + TotalGelombang;
        hati.text = "Nyawa  " + nyawa + " / 4";
    }

    void Kabar(string teks, Color warna, float durasi = .95f)
    {
        umpanBalik.text = teks;
        umpanBalik.color = warna;
        pesan = durasi;
    }

    void TampilkanPanel(Fase berikut)
    {
        fase = berikut;
        if (berikut == Fase.Kalah || berikut == Fase.Menang) musik.Hentikan();
        ulang = .25f;
        panel.gameObject.SetActive(true);
        bool siap = fase == Fase.Siap;
        keluarArena.gameObject.SetActive(false);
        keluarHasil.gameObject.SetActive(!siap);
        tombolMulai.anchoredPosition = new Vector2(siap ? 0 : -126, -102);
        if (siap)
        {
            judulPanel.text = Pesawat ? "Langit Edel" : "Pedang Harapan";
            isiPanel.text = Pesawat
                ? "Arahkan pesawat ke kiri atau kanan. Tembakan berjalan otomatis. Pilih kata sesuai Shadow di depanmu."
                : "Bergerak ke kiri atau kanan, lalu tebas sambil menghadap Shadow. Pilih kata yang tepat untuk menembus bayangannya.";
            judulTombol.text = "Mulai";
        }
        else
        {
            judulPanel.text = fase == Fase.Menang ? "Bayangan menghilang" : "Coba lagi, Arthur";
            isiPanel.text = fase == Fase.Menang ? "Ketiga gelombang selesai. Shadow dikalahkan: " + dikalahkan + "."
                : "Shadow dikalahkan: " + dikalahkan + ". Pilih kata yang cocok dan jaga jarak.";
            judulTombol.text = "Main lagi";
        }
    }

    static Color Warna(int tipe) => tipe == 0 ? Tenang : Pulih;

    static RectTransform Rect(Transform induk, string nama, Vector2 posisi, Vector2 ukuran)
    {
        var rect = new GameObject(nama, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(induk, false);
        rect.anchoredPosition = posisi;
        rect.sizeDelta = ukuran;
        return rect;
    }

    static RawImage Foto(Transform induk, string nama, Vector2 posisi, Vector2 ukuran, Texture2D tekstur)
    {
        var gambar = Rect(induk, nama, posisi, ukuran).gameObject.AddComponent<RawImage>();
        gambar.texture = tekstur;
        gambar.raycastTarget = false;
        return gambar;
    }

    static Image Kotak(Transform induk, string nama, Vector2 posisi, Vector2 ukuran, Color warna)
    {
        var gambar = Rect(induk, nama, posisi, ukuran).gameObject.AddComponent<Image>();
        gambar.color = warna;
        gambar.raycastTarget = false;
        return gambar;
    }

    TextMeshProUGUI Teks(Transform induk, string nama, Vector2 posisi, Vector2 ukuran, string teks, int ukuranFont,
        Color warna, TextAlignmentOptions rata = TextAlignmentOptions.Center)
    {
        var label = Rect(induk, nama, posisi, ukuran).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = gaya.font;
        label.text = teks;
        label.fontSize = ukuranFont;
        label.color = warna;
        label.alignment = rata;
        label.raycastTarget = false;
        return label;
    }

    Button Tombol(Transform induk, string nama, Vector2 posisi, Vector2 ukuran, string teks, int ukuranFont, Color warna, Action aksi, Sprite sprite = null)
    {
        var gambar = Kotak(induk, nama, posisi, ukuran, warna);
        gambar.raycastTarget = true;
        gambar.sprite = sprite != null ? sprite : gaya.kertas;
        gambar.preserveAspect = sprite != null;
        var tombol = gambar.gameObject.AddComponent<Button>();
        tombol.targetGraphic = gambar;
        var navigasi = tombol.navigation;
        navigasi.mode = Navigation.Mode.None;
        tombol.navigation = navigasi;
        Teks(gambar.transform, "Label", Vector2.zero, ukuran, teks, ukuranFont, Gelap).fontStyle = FontStyles.Bold;
        if (aksi != null) tombol.onClick.AddListener(() => aksi());
        return tombol;
    }

    static void Tahan(Button tombol, Action<bool> ubah)
    {
        var pemicu = tombol.gameObject.AddComponent<EventTrigger>();
        foreach (var kejadian in new[] { EventTriggerType.PointerDown, EventTriggerType.PointerUp, EventTriggerType.PointerExit })
        {
            var entri = new EventTrigger.Entry { eventID = kejadian };
            entri.callback.AddListener(_ => ubah(kejadian == EventTriggerType.PointerDown));
            pemicu.triggers.Add(entri);
        }
    }
}
