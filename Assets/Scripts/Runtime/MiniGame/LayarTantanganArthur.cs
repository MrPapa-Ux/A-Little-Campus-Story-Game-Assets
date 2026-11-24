using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class LayarTantanganArthur : MonoBehaviour
{
    static readonly Color Kertas = new Color32(255, 243, 213, 255);
    static readonly Color Tinta = new Color32(40, 35, 37, 255);
    static readonly Color Aksen = new Color32(232, 160, 76, 255);
    static readonly Color Petak = new Color32(240, 218, 174, 255);
    static readonly string[] Judul = { "Puzzle Foto Kampus", "Tebas Buah", "Joget Bareng Bimo", "Kalkulator Nisay", "Foto Ruang Dekan", "Jigsaw Taman UNKLAB", "Album Kampus", "Arthur Mendapat Item!" };
    static readonly int[][] Ketukan =
    {
        new[] { 0, 1, 0, 0, 1, 1, 0 },
        new[] { 1, 0, 1, 1, 0, 1, 0, 0, 1, 0 },
        new[] { 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1 }
    };
    static readonly float[][] JedaKetukanJoget =
    {
        new[] { .88f, .58f, .20f, 1.16f, .66f, .81f },
        new[] { .16f, 1.15f, .64f, .17f, .89f, .53f, 1.19f, .18f, .73f },
        new[] { .13f, .91f, .16f, 1.16f, .55f, .14f, .72f, .19f, 1.21f, .13f, .69f, .17f }
    };
    static readonly string[] Soal = { "8 + 7", "9 × 4", "24 ÷ 3" };
    static readonly decimal[] Jawaban = { 15, 36, 8 };

    RectTransform bidang;
    TMP_Text status, layarHitung, operasiHitung, soalHitung, langkahHitung;
    Button[] ubin;
    RectTransform[] buah;
    Vector2[] posisiBuah;
    bool[] terpotong;
    int[] papan;
    int kosong, langkah, poin, sesi, nada;
    bool ubinBergerak;
    Image latarPapan;
    Texture2D gambar;
    Texture2D[] gambarBuah;
    Action menang;
    JenisTantanganArthur jenis;
    DialogManager dialog;
    KarakterController pemain;
    NPCJogetGoofy joget, jogetBimo;
    Camera kameraJoget;
    Image[] tombolJoget;
    Color[] warnaTombolJoget;
    TMP_Text abaJoget;
    AudioSource bunyiJoget;
    readonly MusikMinigameArthur musik = new MusikMinigameArthur();
    AudioClip laguMinigame;
    bool siapKetukJoget, sedangUlangJoget;
    Coroutine urutanAbaJoget;
    float mulaiAbaJoget, batasKetukJoget;
    Image sisaWaktuJoget;
    const float JendelaKetukJoget = 1f;
    static readonly Color32[] WarnaArahJoget = {
        new Color32(143, 226, 222, 255), new Color32(255, 197, 117, 255)
    };
    Camera kameraPuzzle;
    RenderTexture latarPuzzle;
    bool kunciAwal, aktif, tuntas;
    decimal angkaKiri, hasil;
    string masukan = "", operasi = "";
    bool hitungValid;
    float waktuBuah;
    int jari = int.MinValue;
    Vector2 jejak;
    TMP_FontAsset font;
    Vector2 ukuranFotoPuzzle;
    bool PapanFoto => jenis == JenisTantanganArthur.Puzzle || jenis == JenisTantanganArthur.Jigsaw;
    int pilihanJigsaw = -1, levelJigsaw, ukuranJigsaw;
    readonly Vector2 pusatPuzzle = new Vector2(-157, 2);

    public void Buka(JenisTantanganArthur permainan, Texture2D foto, Action selesai, Texture2D[] ikonBuah = null, int levelJigsaw = 1, NPCJogetGoofy mitraJoget = null)
    {
        jenis = permainan;
        this.levelJigsaw = Mathf.Clamp(levelJigsaw, 1, TantanganSampingArthur.LevelJigsawTerakhir);
        ukuranJigsaw = TantanganSampingArthur.UkuranJigsaw(this.levelJigsaw);
        gambar = foto;
        gambarBuah = ikonBuah;
        menang = selesai;
        dialog = DialogManager.Instance;
        pemain = dialog != null ? dialog.arthurScript : null;
        aktif = true;
        if (dialog != null) dialog.minigameAktif = true;
        if (pemain != null)
        {
            kunciAwal = pemain.kunciPergerakan;
            pemain.BerhentiBergerak();
            pemain.kunciPergerakan = true;
        }

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 900;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1100, 650);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        gameObject.AddComponent<GraphicRaycaster>();
        var gelap = BuatRect("Latar", transform, Vector2.zero, Vector2.zero);
        gelap.anchorMin = Vector2.zero;
        gelap.anchorMax = Vector2.one;
        gelap.sizeDelta = Vector2.zero;
        if (PapanFoto || jenis == JenisTantanganArthur.Album || jenis == JenisTantanganArthur.Hadiah) BekukanLatarPuzzle(gelap);
        else gelap.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.09f, 0.10f, .88f);
        bidang = BuatRect("Kartu tantangan", gelap, Vector2.zero, new Vector2(920, 580));
        var bingkai = bidang.gameObject.AddComponent<Image>();
        bingkai.color = Kertas;
        // Referensi langsung ke gambar dan font tutorial yang dipakai pada scene ini.
        if (PapanFoto || jenis == JenisTantanganArthur.Album || jenis == JenisTantanganArthur.Hadiah ||
            jenis == JenisTantanganArthur.Buah || jenis == JenisTantanganArthur.Joget || jenis == JenisTantanganArthur.Hitung)
        {
            var tutorial = FindFirstObjectByType<CeritaKamar201Arthur>();
            if (tutorial != null)
            {
                font = tutorial.judul.font;
                var kertas = tutorial.panel.transform.Find("Kertas_Dialog_Asli")?.GetComponent<Image>();
                if (kertas == null)
                    foreach (var calon in tutorial.panel.GetComponentsInChildren<Image>(true))
                        if (calon.name == "Kertas_Dialog_Asli") { kertas = calon; break; }
                if (kertas != null && kertas.sprite != null)
                {
                    bingkai.sprite = kertas.sprite;
                    bingkai.type = kertas.type;
                    bingkai.color = kertas.color;
                }
            }
        }
        Label(bidang, new Vector2(-42, 226), new Vector2(715, 50), Judul[(int)jenis], font != null ? 42 : 36, FontStyles.Bold);
        if (jenis != JenisTantanganArthur.Hadiah)
            Tombol(bidang, new Vector2(365, 224), new Vector2(100, 50), "Tutup", Tutup, Petak);
        status = Label(bidang, new Vector2(0, -231), new Vector2(840, 58), "", font != null ? 31 : 25);

        laguMinigame = jenis switch
        {
            JenisTantanganArthur.Joget => MusikMinigameArthur.LaguPonsel("GigglyMagicPow"),
            JenisTantanganArthur.Puzzle or JenisTantanganArthur.Jigsaw or
                JenisTantanganArthur.Buah or JenisTantanganArthur.Hitung => MusikMinigameArthur.LaguTapTipTup(),
            _ => null
        };
        musik.Putar(gameObject, laguMinigame);
        switch (jenis)
        {
            case JenisTantanganArthur.Puzzle: BukaPuzzle(); break;
            case JenisTantanganArthur.Jigsaw: BukaJigsaw(); break;
            case JenisTantanganArthur.Buah: BukaBuah(); break;
            case JenisTantanganArthur.Joget: BukaJoget(mitraJoget); break;
            case JenisTantanganArthur.Hitung: BukaHitung(); break;
            case JenisTantanganArthur.Foto:
                Foto(bidang, new Vector2(0, 2), Dalam(gambar, 570, 406), new Rect(0, 0, 1, 1));
                status.text = "Dikirim 10 menit yang lalu";
                break;
        }
    }

    public void BukaHadiah(CatatanArthur item)
    {
        Buka(JenisTantanganArthur.Hadiah, null, null);
        var ikon = BuatRect("Sprite item", bidang, new Vector2(0, 55), new Vector2(145, 145)).gameObject.AddComponent<Image>();
        ikon.sprite = item.ikon;
        ikon.enabled = item.ikon != null;
        ikon.preserveAspect = true;
        ikon.raycastTarget = false;
        Label(bidang, new Vector2(0, -52), new Vector2(740, 66), item.judul, 48, FontStyles.Bold);
        Label(bidang, new Vector2(0, -117), new Vector2(740, 65), item.isi, 32);
        Tombol(bidang, new Vector2(0, -213), new Vector2(190, 55), "Lanjut", Tutup, Petak);
        var bintang = BuatRect("Bintang item", bidang, new Vector2(0, 55), new Vector2(410, 225)).gameObject.AddComponent<BintangPuzzleArthur>();
        bintang.Siapkan();
        BunyiTombolUI.MainkanItemDidapat();
    }

    public void BukaAlbum(Texture2D[] foto, string[] keterangan, Action selesai)
    {
        Buka(JenisTantanganArthur.Album, foto[0], null);
        int halaman = 0;
        var rect = BuatRect("Foto album", bidang, new Vector2(0, 26), Vector2.zero);
        var fotoAktif = rect.gameObject.AddComponent<RawImage>();
        fotoAktif.raycastTarget = false;
        TMP_Text teksTombol = null;
        var lanjut = Tombol(bidang, new Vector2(359, -229), new Vector2(125, 44), "Berikut", () =>
        {
            if (halaman >= foto.Length - 1)
            {
                Tutup();
                selesai?.Invoke();
                return;
            }
            halaman++;
            Tampilkan();
        }, Petak);
        teksTombol = lanjut.GetComponentInChildren<TMP_Text>();
        status.rectTransform.anchoredPosition = new Vector2(-68, -215);
        status.rectTransform.sizeDelta = new Vector2(665, 86);
        status.fontSize = 29;
        void Tampilkan()
        {
            fotoAktif.texture = foto[halaman];
            rect.sizeDelta = Dalam(foto[halaman], 695, 351);
            status.text = keterangan != null && halaman < keterangan.Length ? keterangan[halaman] : "";
            teksTombol.text = halaman == foto.Length - 1 ? "Selesai" : "Berikut";
        }
        Tampilkan();
    }

    void BekukanLatarPuzzle(RectTransform latar)
    {
        // The board covers most of the screen. Keep a dim, low-resolution view
        // of campus and skip drawing its 3D geometry until the player closes it.
        var kamera = pemain != null ? pemain.GetComponent<ArthurJalurKampus>()?.kamera : null;
        if (kamera != null && kamera.enabled && kamera.targetTexture == null)
        {
            int lebar = Mathf.Clamp(Mathf.RoundToInt(360 * kamera.aspect), 320, 800);
            latarPuzzle = RenderTexture.GetTemporary(lebar, 360, 24);
            try
            {
                RenderPipelineCapture(kamera, latarPuzzle);
                kameraPuzzle = kamera;
#if !UNITY_EDITOR
                kameraPuzzle.enabled = false;
#endif
                var foto = latar.gameObject.AddComponent<RawImage>();
                foto.texture = latarPuzzle;
                foto.color = new Color(.40f, .40f, .40f, 1);
                return;
            }
            catch (Exception)
            {
                if (kameraPuzzle != null) kameraPuzzle.enabled = true;
                kameraPuzzle = null;
                RenderTexture.ReleaseTemporary(latarPuzzle);
                latarPuzzle = null;
            }
        }
        latar.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.09f, 0.10f, 1);
    }

    static void RenderPipelineCapture(Camera kamera, RenderTexture tujuan)
    {
        var permintaan = new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = tujuan };
        UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(kamera, permintaan);
    }

    void BukaPuzzle()
    {
        if (gambar == null) { status.text = "Foto belum tersedia."; return; }
        Vector2 ukuran = ukuranFotoPuzzle = Dalam(gambar, 510, 366);
        Vector2 bagian = ukuran / 3;
        latarPapan = BuatRect("Alas puzzle", bidang, pusatPuzzle, ukuran).gameObject.AddComponent<Image>();
        latarPapan.color = new Color32(220, 218, 213, 255);
        latarPapan.raycastTarget = false;
        ubin = new Button[9];
        papan = new[] {0, 1, 2, 3, 4, 5, 6, 7, -1};
        kosong = 8;
        // Acak melalui langkah legal. Susunan selalu bisa dikembalikan.
        foreach (int posisi in new[] {7, 4, 3, 6, 7, 8, 5, 4, 7, 6}) Tukar(posisi);
        for (int i = 0; i < 9; i++)
        {
            int indeks = i;
            Vector2 posisi = new Vector2(-157 + (i % 3 - 1) * bagian.x, 2 + (1 - i / 3) * bagian.y);
            ubin[i] = Tombol(bidang, posisi, bagian - Vector2.one * 3, "", () => Geser(indeks), Tinta);
            ubin[i].transition = Selectable.Transition.None;
            Foto(ubin[i].transform, Vector2.zero, bagian - Vector2.one * 3, Rect.zero);
            var gesture = ubin[i].gameObject.AddComponent<GeserUbinArthur>();
            gesture.Pindah = (delta) => Geser(indeks, delta);
        }
        Foto(bidang, new Vector2(265, 60), Dalam(gambar, 262, 202), new Rect(0, 0, 1, 1));
        Label(bidang, new Vector2(264, 181), new Vector2(268, 37), "Foto acuan", 29, FontStyles.Bold);
        Label(bidang, new Vector2(263, -110), new Vector2(280, 120), "Geser atau ketuk potongan di sebelah kotak kosong.", 30);
        GambarUbin();
    }

    void BukaJigsaw()
    {
        if (gambar == null) { status.text = "Foto belum tersedia."; return; }
        ukuranFotoPuzzle = Dalam(gambar, 510, 366);
        Vector2 ukuranPotongan = ukuranFotoPuzzle / ukuranJigsaw;
        float setengah = ukuranJigsaw * .5f;
        float tengah = setengah - .5f;
        latarPapan = BuatRect("Alas jigsaw", bidang, pusatPuzzle, ukuranFotoPuzzle).gameObject.AddComponent<Image>();
        latarPapan.color = Tinta;
        latarPapan.raycastTarget = false;
        papan = new int[ukuranJigsaw * ukuranJigsaw];
        ubin = new Button[papan.Length];
        // Semua potongan tetap tersedia dan bisa ditukar dengan potongan mana pun.
        for (int i = 0; i < papan.Length; i++) papan[i] = i;
        for (int i = papan.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i);
            (papan[i], papan[j]) = (papan[j], papan[i]);
        }
        if (SegarkanSkorJigsaw() == papan.Length) (papan[0], papan[1]) = (papan[1], papan[0]);

        for (int i = 0; i < papan.Length; i++)
        {
            int indeks = i;
            var posisi = pusatPuzzle + Vector2.Scale(new Vector2(i % ukuranJigsaw - tengah, tengah - i / ukuranJigsaw), ukuranPotongan);
            ubin[i] = Tombol(bidang, posisi, ukuranPotongan, "", () => PilihJigsaw(indeks), Color.clear);
            ubin[i].transition = Selectable.Transition.None;
            var foto = BuatRect("Potongan jigsaw", ubin[i].transform, Vector2.zero, ukuranPotongan)
                .gameObject.AddComponent<PotonganJigsawArthur>();
            foto.texture = gambar;
            foto.raycastTarget = false;
            foto.UkuranPapan = ukuranJigsaw;
            foto.Atur(papan[i], false);
            var geser = ubin[i].gameObject.AddComponent<SeretJigsawArthur>();
            geser.Lepas = (layar, kamera) =>
            {
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(bidang, layar, kamera, out var titik)) return;
                var relatif = titik - pusatPuzzle;
                int x = Mathf.FloorToInt(relatif.x / ukuranPotongan.x + setengah);
                int y = Mathf.FloorToInt(setengah - relatif.y / ukuranPotongan.y);
                if (x >= 0 && x < ukuranJigsaw && y >= 0 && y < ukuranJigsaw) TukarJigsaw(indeks, y * ukuranJigsaw + x);
            };
        }
        Foto(bidang, new Vector2(265, 60), Dalam(gambar, 262, 202), new Rect(0, 0, 1, 1));
        Label(bidang, new Vector2(264, 181), new Vector2(268, 37), "Foto acuan", 29, FontStyles.Bold);
        Label(bidang, new Vector2(263, -110), new Vector2(280, 120), "Seret satu potongan ke potongan lain, atau ketuk keduanya untuk bertukar.", 27);
        SegarkanSkorJigsaw();
    }

    void PilihJigsaw(int indeks)
    {
        if (tuntas) return;
        int sebelumnya = pilihanJigsaw;
        if (sebelumnya < 0)
        {
            pilihanJigsaw = indeks;
            ubin[indeks].GetComponentInChildren<PotonganJigsawArthur>().Atur(papan[indeks], true);
        }
        else if (sebelumnya == indeks) BersihkanPilihanJigsaw();
        else TukarJigsaw(sebelumnya, indeks);
    }

    void BersihkanPilihanJigsaw()
    {
        if (pilihanJigsaw < 0) return;
        ubin[pilihanJigsaw].GetComponentInChildren<PotonganJigsawArthur>().Atur(papan[pilihanJigsaw], false);
        pilihanJigsaw = -1;
    }

    void TukarJigsaw(int asal, int tujuan)
    {
        if (tuntas || asal == tujuan) return;
        BersihkanPilihanJigsaw();
        (papan[asal], papan[tujuan]) = (papan[tujuan], papan[asal]);
        ubin[asal].GetComponentInChildren<PotonganJigsawArthur>().Atur(papan[asal], false);
        ubin[tujuan].GetComponentInChildren<PotonganJigsawArthur>().Atur(papan[tujuan], false);
        if (SegarkanSkorJigsaw() != papan.Length) return;
        Menang(levelJigsaw == TantanganSampingArthur.LevelJigsawTerakhir
            ? "Level 3 selesai! Fotonya sudah utuh."
            : $"Level {levelJigsaw} selesai! Bicara dengan Nara untuk level {levelJigsaw + 1}.");
        StartCoroutine(RayakanPuzzle());
    }

    int SegarkanSkorJigsaw()
    {
        int benar = 0;
        for (int i = 0; i < papan.Length; i++) if (papan[i] == i) benar++;
        status.text = $"Level {levelJigsaw} ({ukuranJigsaw}×{ukuranJigsaw})   •   {benar} / {papan.Length} potongan tepat";
        return benar;
    }

    void Tukar(int posisi)
    {
        papan[kosong] = papan[posisi];
        papan[posisi] = -1;
        kosong = posisi;
    }

    void Geser(int indeks, Vector2 arah = default)
    {
        if (tuntas || ubinBergerak || Mathf.Abs(kosong % 3 - indeks % 3) + Mathf.Abs(kosong / 3 - indeks / 3) != 1) return;
        var tujuan = new Vector2(kosong % 3 - indeks % 3, indeks / 3 - kosong / 3);
        if (arah != Vector2.zero && Vector2.Dot(arah.normalized, tujuan) < .45f) return;
        StartCoroutine(AnimasikanGeser(indeks));
    }

    IEnumerator AnimasikanGeser(int indeks)
    {
        ubinBergerak = true;
        var petak = (RectTransform)ubin[indeks].transform;
        Vector2 asal = petak.anchoredPosition;
        Vector2 tujuan = ((RectTransform)ubin[kosong].transform).anchoredPosition;
        petak.SetAsLastSibling();
        float mulai = Time.unscaledTime;
        const float durasi = .115f;
        for (float waktu = 0; waktu < durasi; waktu = Time.unscaledTime - mulai)
        {
            petak.anchoredPosition = Vector2.LerpUnclamped(asal, tujuan, Mathf.SmoothStep(0, 1, waktu / durasi));
            yield return null;
        }
        petak.anchoredPosition = asal;
        Tukar(indeks);
        langkah++;
        if (kosong == 8)
        {
            bool urut = true;
            for (int i = 0; i < 8; i++) urut &= papan[i] == i;
            if (urut)
            {
                papan[8] = 8;
                Menang("Fotonya sudah utuh!");
                    StartCoroutine(RayakanPuzzle());
            }
        }
        GambarUbin();
        ubinBergerak = false;
    }

    void GambarUbin()
    {
        for (int i = 0; i < ubin.Length; i++)
        {
            int isi = papan[i];
            var foto = ubin[i].GetComponentInChildren<RawImage>(true);
            foto.enabled = isi >= 0;
            ubin[i].image.enabled = isi >= 0;
            ubin[i].interactable = !tuntas && isi >= 0;
            if (isi >= 0) foto.uvRect = new Rect((isi % 3) / 3f, (2 - isi / 3) / 3f, 1f / 3, 1f / 3);
            foto.rectTransform.sizeDelta = ((RectTransform)ubin[i].transform).sizeDelta;
        }
        if (!tuntas) status.text = "Langkah " + langkah;
    }

    IEnumerator RayakanPuzzle()
    {
        var petak = new RectTransform[ubin.Length];
        var asal = new Vector2[ubin.Length];
        for (int i = 0; i < petak.Length; i++)
        {
            petak[i] = (RectTransform)ubin[i].transform;
            asal[i] = petak[i].anchoredPosition;
        }
        const float durasi = .48f;
        float mulai = Time.unscaledTime;
        for (float waktu = 0; waktu < durasi; waktu = Time.unscaledTime - mulai)
        {
            float renggang = Mathf.Sin(Mathf.PI * waktu / durasi) * .13f;
            for (int i = 0; i < petak.Length; i++)
                petak[i].anchoredPosition = asal[i] + (asal[i] - pusatPuzzle) * renggang;
            yield return null;
        }
        foreach (var bagian in petak) bagian.gameObject.SetActive(false);
        latarPapan.enabled = false;
        Foto(bidang, pusatPuzzle, ukuranFotoPuzzle, new Rect(0, 0, 1, 1));

        var efek = BuatRect("Bintang puzzle", bidang, pusatPuzzle, ukuranFotoPuzzle).gameObject.AddComponent<BintangPuzzleArthur>();
        efek.Siapkan();
        yield return new WaitForSecondsRealtime(1.2f);
        Tutup();
    }

    void BukaBuah()
    {
        buah = new RectTransform[6];
        posisiBuah = new Vector2[buah.Length];
        terpotong = new bool[buah.Length];
        Label(bidang, new Vector2(0, 160), new Vector2(795, 50), "Usap layar untuk menebas 6 buah. Tidak ada batas waktu.", 30);
        Color[] warna = { new Color32(238, 105, 94, 255), new Color32(245, 168, 68, 255), new Color32(154, 181, 92, 255) };
        string[] rupa = { "Apel", "Stroberi", "Semangka" };
        for (int i = 0; i < buah.Length; i++)
        {
            posisiBuah[i] = new Vector2(-290 + i % 3 * 290, i < 3 ? -92 : 52);
            buah[i] = BuatRect(rupa[i % 3], bidang, posisiBuah[i], new Vector2(126, 88));
            if (gambarBuah != null && gambarBuah.Length > i % 3 && gambarBuah[i % 3] != null)
            {
                var ikon = buah[i].gameObject.AddComponent<RawImage>();
                ikon.texture = gambarBuah[i % 3];
                ikon.rectTransform.sizeDelta = Dalam(gambarBuah[i % 3], 94, 94);
                Label(buah[i], Vector2.zero, new Vector2(70, 44), "•   •\nᴗ", 23, FontStyles.Bold);
            }
            else
            {
                buah[i].gameObject.AddComponent<Image>().color = warna[i % 3];
                Label(buah[i], Vector2.zero, new Vector2(110, 80), "•  •\n" + rupa[i % 3], 24, FontStyles.Bold);
            }
        }
        status.text = "0 / 6 buah";
    }

    void Usap(Vector2 awal, Vector2 akhir)
    {
        if (tuntas || (akhir - awal).sqrMagnitude < 4) return;
        for (int i = 0; i < buah.Length; i++)
        {
            if (terpotong[i]) continue;
            var tengah = RectTransformUtility.WorldToScreenPoint(null, buah[i].position);
            Vector2 jarak = akhir - awal;
            float t = Mathf.Clamp01(Vector2.Dot(tengah - awal, jarak) / jarak.sqrMagnitude);
            float radius = 47 * bidang.lossyScale.x;
            if ((awal + t * jarak - tengah).sqrMagnitude > radius * radius) continue;
            terpotong[i] = true;
            buah[i].gameObject.SetActive(false);
            poin++;
        }
        status.text = poin + " / 6 buah";
        if (poin == buah.Length) Menang("Enam buah berhasil ditebas!");
    }

    void BukaJoget(NPCJogetGoofy mitra)
    {
        joget = pemain != null ? pemain.GetComponent<NPCJogetGoofy>() : null;
        jogetBimo = mitra;
        var jalur = pemain != null ? pemain.GetComponent<ArthurJalurKampus>() : null;
        kameraJoget = jalur != null ? jalur.kamera : Camera.main;
        if (joget != null) joget.MulaiIkutiAba();
        if (jogetBimo != null) jogetBimo.MulaiIkutiAba();
        if(pemain!=null)pemain.TombolArahMinigame+=Ketuk;
        var panelKertas = bidang.GetComponent<Image>();
        panelKertas.enabled = false;
        bidang.parent.GetComponent<Image>().color = new Color(0, 0, 0, .12f);
        bidang.parent.GetComponent<Image>().raycastTarget=false;
        var judul = bidang.GetChild(0).GetComponent<TMP_Text>();
        var pita = BuatRect("Pita joget", bidang, new Vector2(-42, 226), new Vector2(715, 60)).gameObject.AddComponent<Image>();
        pita.color = new Color32(255, 242, 213, 246);
        pita.raycastTarget = false;
        pita.transform.SetSiblingIndex(judul.transform.GetSiblingIndex());

        var kartu = BuatRect("Aba-aba joget", bidang, new Vector2(0, 126), new Vector2(332, 110)).gameObject.AddComponent<Image>();
        kartu.color = Tinta;
        kartu.raycastTarget = false;
        abaJoget = Label(kartu.transform, new Vector2(0, 3), new Vector2(314, 98), "", 63, FontStyles.Bold);
        var indikator = BuatRect("Sisa waktu ketuk", kartu.transform, new Vector2(-154, -44), new Vector2(308, 5));
        indikator.pivot = new Vector2(0, .5f);
        sisaWaktuJoget = indikator.gameObject.AddComponent<Image>();
        sisaWaktuJoget.raycastTarget = false;
        sisaWaktuJoget.enabled = false;
        var alas = BuatRect("Pita sesi", bidang, status.rectTransform.anchoredPosition, new Vector2(480, 54)).gameObject.AddComponent<Image>();
        alas.color = Tinta;
        alas.raycastTarget = false;
        alas.transform.SetSiblingIndex(status.transform.GetSiblingIndex());
        status.color = Kertas;
        status.fontSize = 27;
        // Pakai EventTrigger tombol jalan asli, tanpa membuat pasangan tombol baru.
        tombolJoget=new Image[2];warnaTombolJoget=new Color[2];
        foreach(var trigger in FindObjectsByType<EventTrigger>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            foreach(var entry in trigger.triggers)if(entry.eventID==EventTriggerType.PointerDown)
                for(int i=0;i<entry.callback.GetPersistentEventCount();i++){
                    if(entry.callback.GetPersistentTarget(i)!=pemain)continue;
                    string metode=entry.callback.GetPersistentMethodName(i);
                    int arah=metode=="JalanKeKiri"?0:metode=="JalanKeKanan"?1:-1;
                    if(arah<0)continue;
                    tombolJoget[arah]=trigger.GetComponent<Image>();
                    if(tombolJoget[arah]!=null)warnaTombolJoget[arah]=tombolJoget[arah].color;
                }

        bunyiJoget = gameObject.AddComponent<AudioSource>();
        bunyiJoget.playOnAwake = false;
        bunyiJoget.spatialBlend = 0;
        bunyiJoget.clip = Resources.Load<AudioClip>("ButtonSfx/PianoLipatan");
        urutanAbaJoget = StartCoroutine(AbaAbaJoget(true));
    }

    IEnumerator AbaAbaJoget(bool sesiBaru)
    {
        siapKetukJoget = false;
        sisaWaktuJoget.enabled = false;
        WarnaTombolJoget(-1);
        status.text = $"Tahap {sesi + 1} / 3   •   Ketukan {nada + 1} / {Ketukan[sesi].Length}";
        abaJoget.color = Kertas;
        abaJoget.rectTransform.localScale = Vector3.one;
        if (sesiBaru)
        {
            for (int hitung = 3; hitung > 0; hitung--)
            {
                abaJoget.text = hitung.ToString();
                BunyiJoget(1f);
                yield return new WaitForSecondsRealtime(.8f);
            }
        }
        else
        {
            abaJoget.text = "✓";
            yield return new WaitForSecondsRealtime(JedaKetukanJoget[sesi][nada - 1]);
        }

        int arah = Ketukan[sesi][nada];
        abaJoget.text = arah == 0 ? "← KIRI" : "KANAN →";
        abaJoget.color = WarnaArahJoget[arah];
        WarnaTombolJoget(arah);
        BunyiJoget(arah == 0 ? 1.15f : 1.45f);
        mulaiAbaJoget = Time.unscaledTime;
        if(jogetBimo!=null)jogetBimo.GerakAba(arah,false,mulaiAbaJoget,kameraJoget!=null?kameraJoget.transform.right:Vector3.right);
        batasKetukJoget = mulaiAbaJoget + JendelaKetukJoget;
        sisaWaktuJoget.color = WarnaArahJoget[arah];
        sisaWaktuJoget.rectTransform.localScale = Vector3.one;
        sisaWaktuJoget.enabled = true;
        siapKetukJoget = true;
        urutanAbaJoget = null;
    }

    void WarnaTombolJoget(int arah)
    {
        for (int i = 0; i < tombolJoget.Length; i++)
        {
            if(tombolJoget[i]!=null)tombolJoget[i].color = i == arah ? WarnaArahJoget[i] : warnaTombolJoget[i];
        }
    }

    void BunyiJoget(float pitch)
    {
        if (bunyiJoget.clip == null) return;
        bunyiJoget.pitch = pitch;
        bunyiJoget.PlayOneShot(bunyiJoget.clip, .7f);
    }

    void GerakJoget(int arah, bool salah)
    {
        float waktu = siapKetukJoget ? mulaiAbaJoget : Time.unscaledTime;
        int arahBimo=Ketukan[sesi][nada];
        Vector3 kanan = kameraJoget != null ? kameraJoget.transform.right : Vector3.right;
        if (joget != null) joget.GerakAba(salah?1-arahBimo:arah, salah, waktu, kanan);
        if (jogetBimo != null) jogetBimo.GerakAba(arahBimo, false, waktu, kanan);
    }

    void GagalJoget(int arah = -1)
    {
        if (sedangUlangJoget || tuntas) return;
        sedangUlangJoget = true;
        GerakJoget(arah < 0 ? Ketukan[sesi][nada] : arah, true);
        if (urutanAbaJoget != null) StopCoroutine(urutanAbaJoget);
        urutanAbaJoget = StartCoroutine(UlangJoget());
    }

    IEnumerator UlangJoget()
    {
        siapKetukJoget = false;
        sesi = nada = 0;
        WarnaTombolJoget(-1);
        sisaWaktuJoget.enabled = false;
        abaJoget.rectTransform.localScale = Vector3.one;
        abaJoget.color = new Color32(255, 153, 133, 255);
        abaJoget.text = "Ulang!";
        status.text = "Tahap 1 / 3";
        musik.Hentikan();
        BunyiJoget(.65f);
        yield return new WaitForSecondsRealtime(1.05f);
        if(joget!=null){joget.SelesaiIkutiAba();joget.MulaiIkutiAba();}
        if(jogetBimo!=null){jogetBimo.SelesaiIkutiAba();jogetBimo.MulaiIkutiAba();}
        sedangUlangJoget = false;
        musik.Putar(gameObject, laguMinigame);
        yield return AbaAbaJoget(true);
    }

    void Ketuk(int arah)
    {
        if (tuntas || sedangUlangJoget) return;
        if (!siapKetukJoget || Time.unscaledTime >= batasKetukJoget || arah != Ketukan[sesi][nada])
        {
            GagalJoget(arah);
            return;
        }

        BunyiJoget(1.6f);
        GerakJoget(arah, false);
        bool sesiBaru = ++nada == Ketukan[sesi].Length;
        if (sesiBaru)
        {
            nada = 0;
            if (++sesi == Ketukan.Length)
            {
                siapKetukJoget = false;
                WarnaTombolJoget(-1);
                sisaWaktuJoget.enabled = false;
                abaJoget.text = "Mantap!";
                abaJoget.color = Kertas;
                abaJoget.rectTransform.localScale = Vector3.one;
                Menang("Tahap 3 / 3");
                return;
            }
        }
        urutanAbaJoget = StartCoroutine(AbaAbaJoget(sesiBaru));
    }

    void BukaHitung()
    {
        var bidangKertas = bidang.GetComponent<Image>();
        bidangKertas.enabled = false;
        Sprite bingkai = bidangKertas.sprite;
        Image.Type jenisBingkai = bidangKertas.type;
        var warnaBadan = new Color32(102, 159, 151, 255);
        var warnaLcd = new Color32(199, 220, 172, 255);
        var tintaLcd = new Color32(39, 56, 46, 255);

        Image Panel(string nama, Transform induk, Vector2 posisi, Vector2 ukuran, Color warna, bool pakaiBingkai = true)
        {
            var gambarPanel = BuatRect(nama, induk, posisi, ukuran).gameObject.AddComponent<Image>();
            gambarPanel.color = warna;
            gambarPanel.raycastTarget = false;
            if (pakaiBingkai && bingkai != null)
            {
                gambarPanel.sprite = bingkai;
                gambarPanel.type = jenisBingkai;
            }
            return gambarPanel;
        }

        void BingkaiTombol(Button tombol)
        {
            tombol.image.sprite = bingkai;
            tombol.image.type = jenisBingkai;
        }

        var judul = bidang.GetChild(0).GetComponent<TMP_Text>();
        var badan = Panel("Badan Kalkulator Nisay", bidang, new Vector2(-182, -4), new Vector2(424, 492), warnaBadan);
        badan.transform.SetAsFirstSibling();
        var kartuSoal = Panel("Kertas soal", bidang, new Vector2(233, -6), new Vector2(350, 331), Kertas);
        kartuSoal.transform.SetAsFirstSibling();
        judul.rectTransform.anchoredPosition = new Vector2(-182, 197);
        judul.rectTransform.sizeDelta = new Vector2(358, 51);
        judul.fontSize = 38;
        BingkaiTombol(bidang.GetComponentInChildren<Button>());

        var layar = Panel("Bingkai LCD", badan.transform, new Vector2(0, 111), new Vector2(347, 109), Tinta, false);
        Panel("LCD", layar.transform, Vector2.zero, new Vector2(331, 93), warnaLcd, false);
        operasiHitung = Label(layar.transform, new Vector2(0, 27), new Vector2(298, 30), "", 25);
        operasiHitung.alignment = TextAlignmentOptions.MidlineRight;
        operasiHitung.color = tintaLcd;
        layarHitung = Label(layar.transform, new Vector2(0, -13), new Vector2(298, 52), "0", 48, FontStyles.Bold);
        layarHitung.alignment = TextAlignmentOptions.MidlineRight;
        layarHitung.color = tintaLcd;
        layarHitung.enableAutoSizing = true;
        layarHitung.fontSizeMin = 27;
        layarHitung.fontSizeMax = 48;

        string[] kunci = { "7", "8", "9", "÷", "4", "5", "6", "×", "1", "2", "3", "−", "C", "0", "=", "+" };
        for (int i = 0; i < kunci.Length; i++)
        {
            string tombol = kunci[i];
            Color warna = tombol == "=" ? Aksen : tombol == "C" ? new Color32(233, 158, 143, 255) :
                i % 4 == 3 ? new Color32(166, 215, 203, 255) : Kertas;
            var posisi = new Vector2(-133.5f + 89 * (i % 4), 31 - 66 * (i / 4));
            Panel("Bayangan tombol", badan.transform, posisi + new Vector2(0, -3), new Vector2(82, 58),
                new Color32(57, 102, 97, 255), false);
            var tombolUi = Tombol(badan.transform, posisi, new Vector2(82, 56), tombol, () => Hitung(tombol), warna);
            BingkaiTombol(tombolUi);
            tombolUi.GetComponentInChildren<TMP_Text>().fontSize = 35;
        }

        langkahHitung = Label(kartuSoal.transform, new Vector2(0, 117), new Vector2(265, 44), "", 28, FontStyles.Bold);
        soalHitung = Label(kartuSoal.transform, new Vector2(0, 59), new Vector2(270, 64), "", 46, FontStyles.Bold);
        BingkaiTombol(Tombol(kartuSoal.transform, new Vector2(0, -16), new Vector2(246, 62), "Kirim jawaban", Jawab, Aksen));
        status.rectTransform.SetParent(kartuSoal.transform, false);
        status.rectTransform.anchoredPosition = new Vector2(0, -101);
        status.rectTransform.sizeDelta = new Vector2(280, 76);
        status.fontSize = 23;
        status.text = "Hitung dengan kalkulator, lalu tekan =.";
        SegarkanHitung();
    }

    void Hitung(string tombol)
    {
        if (tuntas) return;
        if (tombol == "C") { masukan = ""; operasi = ""; hitungValid = false; }
        else if (tombol[0] >= '0' && tombol[0] <= '9')
        {
            if (hitungValid) { masukan = ""; operasi = ""; }
            hitungValid = false;
            if (masukan.Length < 5) masukan += tombol;
        }
        else if (tombol == "=" && operasi != "" && decimal.TryParse(masukan, NumberStyles.None, CultureInfo.InvariantCulture, out decimal kanan))
        {
            if (operasi == "÷" && kanan == 0) { status.text = "Tidak bisa membagi dengan nol. Tekan C."; return; }
            hasil = operasi switch { "+" => angkaKiri + kanan, "−" => angkaKiri - kanan, "×" => angkaKiri * kanan, _ => angkaKiri / kanan };
            masukan = hasil.ToString("0.####", CultureInfo.InvariantCulture);
            operasi = "";
            hitungValid = true;
        }
        else if (tombol != "=" && decimal.TryParse(masukan, NumberStyles.Number, CultureInfo.InvariantCulture, out angkaKiri))
        {
            operasi = tombol;
            masukan = "";
            hitungValid = false;
        }
        SegarkanHitung();
    }

    void SegarkanHitung()
    {
        operasiHitung.text = operasi == "" ? "" : $"{angkaKiri.ToString(CultureInfo.InvariantCulture)} {operasi}";
        layarHitung.text = masukan.Length == 0 ? "0" : masukan;
        langkahHitung.text = $"Soal {poin + 1} / {Soal.Length}";
        soalHitung.text = Soal[poin];
    }

    void Jawab()
    {
        if (tuntas) return;
        if (!hitungValid) { status.text = "Hitung dengan kalkulator dan tekan = dulu."; return; }
        if (hasil != Jawaban[poin])
        {
            status.text = "Belum tepat. Tekan C dan coba lagi.";
            musik.Putar(gameObject, laguMinigame);
            return;
        }
        if (++poin == Jawaban.Length) { Menang("Kalkulator lulus uji coba!"); return; }
        Hitung("C");
        status.text = "Betul! Lanjut ke soal berikutnya.";
    }

    void Update()
    {
        if (!aktif) return;
        if (Input.GetKeyDown(KeyCode.Escape)) { Tutup(); return; }
        if (tuntas) return;
        if (jenis == JenisTantanganArthur.Joget)
        {
            if (siapKetukJoget)
            {
                float sisa = batasKetukJoget - Time.unscaledTime;
                if (sisa <= 0)
                {
                    GagalJoget();
                    return;
                }
                sisaWaktuJoget.rectTransform.localScale = new Vector3(sisa / JendelaKetukJoget, 1, 1);
                float t = Mathf.Clamp01((Time.unscaledTime - mulaiAbaJoget) / .38f);
                if (t < 1) abaJoget.rectTransform.localScale = Vector3.one * (1 + .16f * Mathf.Sin(t * Mathf.PI));
                else if (abaJoget.rectTransform.localScale != Vector3.one) abaJoget.rectTransform.localScale = Vector3.one;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A)) Ketuk(0);
            else if (Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D)) Ketuk(1);
            return;
        }
        if (jenis != JenisTantanganArthur.Buah) return;
        waktuBuah += Time.unscaledDeltaTime;
        for (int i = 0; i < buah.Length; i++)
            if (!terpotong[i])
                buah[i].anchoredPosition = posisiBuah[i] + Vector2.up * (Mathf.Sin(waktuBuah * 1.8f + i * 1.2f) * 35);

        if (Input.touchCount > 0)
        {
            if (jari == -1) jari = int.MinValue;
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch sentuh = Input.GetTouch(i);
                if (jari == int.MinValue && sentuh.phase == TouchPhase.Began) { jari = sentuh.fingerId; jejak = sentuh.position; }
                if (jari != sentuh.fingerId) continue;
                if (sentuh.phase == TouchPhase.Moved || sentuh.phase == TouchPhase.Ended) Usap(jejak, sentuh.position);
                jejak = sentuh.position;
                if (sentuh.phase == TouchPhase.Ended || sentuh.phase == TouchPhase.Canceled) jari = int.MinValue;
                break;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) { jari = -1; jejak = Input.mousePosition; }
            if (jari == -1 && (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)))
            {
                Usap(jejak, Input.mousePosition);
                jejak = Input.mousePosition;
            }
            if (!Input.GetMouseButton(0)) jari = int.MinValue;
        }
    }

    void Menang(string pujian)
    {
        if (tuntas) return;
        tuntas = true;
        musik.Hentikan();
        menang?.Invoke();
        status.text = pujian;
        if (!PapanFoto) StartCoroutine(TutupSetelahMenang());
    }

    IEnumerator TutupSetelahMenang()
    {
        yield return new WaitForSecondsRealtime(1f);
        Tutup();
    }

    void Tutup()
    {
        Pulihkan();
        Destroy(gameObject);
    }

    void OnDestroy() => Pulihkan();

    void Pulihkan()
    {
        if (!aktif) return;
        aktif = false;
        musik.Tutup();
        if(jenis==JenisTantanganArthur.Joget){WarnaTombolJoget(-1);if(pemain!=null)pemain.TombolArahMinigame-=Ketuk;}
        if (kameraPuzzle != null) kameraPuzzle.enabled = true;
        if (latarPuzzle != null)
        {
            RenderTexture.ReleaseTemporary(latarPuzzle);
            latarPuzzle = null;
        }
        if (joget != null) joget.SelesaiIkutiAba();
        if (jogetBimo != null) jogetBimo.SelesaiIkutiAba();
        if (dialog != null) dialog.minigameAktif = false;
        if (pemain != null)
        {
            pemain.BerhentiBergerak();
            pemain.kunciPergerakan = kunciAwal;
        }
    }

    static RectTransform BuatRect(string nama, Transform induk, Vector2 posisi, Vector2 ukuran)
    {
        var rt = new GameObject(nama, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(induk, false);
        rt.anchoredPosition = posisi;
        rt.sizeDelta = ukuran;
        return rt;
    }

    TMP_Text Label(Transform induk, Vector2 posisi, Vector2 ukuran, string teks, float besar, FontStyles gaya = FontStyles.Normal)
    {
        var label = BuatRect("Teks", induk, posisi, ukuran).gameObject.AddComponent<TextMeshProUGUI>();
        label.text = teks;
        if (font != null) label.font = font;
        label.color = Tinta;
        label.fontSize = besar;
        label.fontStyle = gaya;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    Button Tombol(Transform induk, Vector2 posisi, Vector2 ukuran, string teks, Action klik, Color warna)
    {
        var rt = BuatRect(teks, induk, posisi, ukuran);
        var image = rt.gameObject.AddComponent<Image>();
        image.color = warna;
        var tombol = rt.gameObject.AddComponent<Button>();
        tombol.targetGraphic = image;
        tombol.navigation = new Navigation { mode = Navigation.Mode.None };
        tombol.onClick.AddListener(() => klik());
        if (teks.Length > 0) Label(rt, Vector2.zero, ukuran - new Vector2(8, 3), teks, 26, FontStyles.Bold);
        return tombol;
    }

    void Foto(Transform induk, Vector2 posisi, Vector2 ukuran, Rect potongan)
    {
        var gambarUI = BuatRect("Foto", induk, posisi, ukuran).gameObject.AddComponent<RawImage>();
        gambarUI.texture = gambar;
        gambarUI.uvRect = potongan;
        gambarUI.raycastTarget = false;
    }

    static Vector2 Dalam(Texture2D foto, float lebar, float tinggi)
    {
        float skala = Mathf.Min(lebar / foto.width, tinggi / foto.height);
        return new Vector2(foto.width, foto.height) * skala;
    }
}

public sealed class BintangPuzzleArthur : MonoBehaviour
{
    const float Durasi = 1.1f;
    readonly RawImage[] bintang = new RawImage[15];
    readonly Vector2[] jalur = new Vector2[15];
    Texture2D tekstur;
    float mulai;

    public void Siapkan()
    {
        tekstur = new Texture2D(48, 48, TextureFormat.RGBA32, false);
        var titik = new Vector2[10];
        for (int i = 0; i < titik.Length; i++)
        {
            float sudut = Mathf.PI * .5f + i * Mathf.PI / 5;
            titik[i] = new Vector2(Mathf.Cos(sudut), Mathf.Sin(sudut)) * (i % 2 == 0 ? 22 : 10) + Vector2.one * 24;
        }
        var piksel = new Color32[48 * 48];
        for (int y = 0; y < 48; y++)
        for (int x = 0; x < 48; x++)
        {
            bool diDalam = false;
            for (int i = 0, j = titik.Length - 1; i < titik.Length; j = i++)
                if ((titik[i].y > y) != (titik[j].y > y) &&
                    x < (titik[j].x - titik[i].x) * (y - titik[i].y) / (titik[j].y - titik[i].y) + titik[i].x)
                    diDalam = !diDalam;
            if (diDalam) piksel[y * 48 + x] = new Color32(255, 186, 36, 255);
        }
        tekstur.SetPixels32(piksel);
        tekstur.Apply(false, true);
        var ukuran = ((RectTransform)transform).rect.size * .5f;
        for (int i = 0; i < bintang.Length; i++)
        {
            float sudut = i * Mathf.PI * 2 / bintang.Length;
            jalur[i] = Vector2.Scale(new Vector2(Mathf.Cos(sudut), Mathf.Sin(sudut)), ukuran + Vector2.one * 14);
            var gambar = new GameObject("Bintang", typeof(RectTransform)).AddComponent<RawImage>();
            gambar.transform.SetParent(transform, false);
            gambar.rectTransform.sizeDelta = Vector2.one * (22 + i % 3 * 7);
            gambar.rectTransform.anchoredPosition = jalur[i] * .53f;
            gambar.texture = tekstur;
            gambar.raycastTarget = false;
            bintang[i] = gambar;
        }
        mulai = Time.unscaledTime;
    }

    void Update()
    {
        float t = (Time.unscaledTime - mulai) / Durasi;
        if (t >= 1) { Destroy(gameObject); return; }
        float melesat = 1 - (1 - t) * (1 - t);
        float alpha = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.2f, 1, t));
        for (int i = 0; i < bintang.Length; i++)
        {
            bintang[i].rectTransform.anchoredPosition = jalur[i] * (.53f + .47f * melesat);
            bintang[i].canvasRenderer.SetAlpha(alpha);
        }
    }

    void OnDestroy()
    {
        if (tekstur != null) Destroy(tekstur);
    }
}

public sealed class GeserUbinArthur : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Action<Vector2> Pindah;
    Vector2 awal;
    public void OnBeginDrag(PointerEventData data) => awal = data.pressPosition;
    public void OnDrag(PointerEventData data) { }
    public void OnEndDrag(PointerEventData data)
    {
        Vector2 delta = data.position - awal;
        if (delta.sqrMagnitude > 144) Pindah?.Invoke(delta);
    }
}
