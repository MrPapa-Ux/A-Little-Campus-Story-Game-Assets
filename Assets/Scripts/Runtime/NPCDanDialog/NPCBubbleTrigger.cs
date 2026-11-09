// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Memunculkan dan menggoyangkan bubble/emote NPC ketika pemain mendekat.
// Panduan: Assets/Scripts/Runtime/NPCDanDialog/README.md

using System.Collections;
using UnityEngine;
using TMPro; // WAJIB ADA untuk mengenali TextMeshPro!

[DefaultExecutionOrder(400)]
public class NPCBubbleTrigger : MonoBehaviour
{
    [Header("Referensi Bubble")]
    public Transform bubbleChatTransform; 
    
    [Header("Referensi Visual (Teks vs Gambar)")]
    [Tooltip("Tarik objek Simbol (yang pakai TextMeshPro) ke sini!")]
    public TextMeshPro teksSimbolAsli; 
    [Tooltip("Tarik objek baru (yang pakai Sprite Renderer) ke sini!")]
    public SpriteRenderer rendererEmote; 

    [Header("Pengaturan Animasi Skala")]
    public float durasiAnimasi = 1.0f; 
    public Vector3 ukuranMaksimal = new Vector3(0.2f, 0.2f, 0.2f); 
    [Range(0,.22f)] public float tinggiBubbleDiLayar=.13f;
    private Vector3 skalaAnimasi;
    private SpriteRenderer gambarBubble;

    [Header("Arah ekor bubble NPC")]
    [Tooltip("Letakkan ekor dekat wajah dan arahkan tepat ke mulut. Posisi yang sudah pas dipertahankan.")]
    public bool arahkanEkorKeKepala = true;
    [Tooltip("Opsional. Kosong: mencari sprite kepala NPC, bukan kepala Arthur pemain.")]
    public SpriteRenderer kepalaNpc;
    [Tooltip("Opsional: penanda mulut yang dapat digeser sendiri. Kosong memakai posisi mulut animasi bicara NPC.")]
    public Transform titikMulutNpc;
    [Range(1f, 2f), Tooltip("Perbesaran khusus bubble NPC; bubble item tidak ikut berubah.")]
    public float pengaliUkuranNpc = 1.3f;
    [Range(.05f, .4f), Tooltip("Jarak ujung ekor dari mulut, dibanding tinggi wajah. Berlaku saat NPC menghadap kiri maupun kanan.")]
    public float jarakEkorDariMulut = .16f;
    private EkspresiWajah2D wajahNpc;
    private bool kepalaSudahDicari;
    private Vector3 offsetGoyang;

    [Header("Pengaturan Goyang (Wiggly)")]
    public float jedaWiggly = 0.5f; 
    public float kekuatanGoyang = 0.05f; 
    public float kecepatanGoyang = 15f; 

    private Coroutine animasiCoroutine;
    private Coroutine wigglyCoroutine;
    
    private Vector3 posisiAsliBubble;
    private bool bolehGoyang = false;
    private float timerGoyang;
    private bool sedangDinonaktifkan;
    private bool pemainDekat, dialogMenyembunyikan;
    private Camera kameraBubble;
    private bool sapaanKhusus;
    private Coroutine sapaanCoroutine;
    private bool posisiTersimpan;
    private bool BolehAnimasi => !sedangDinonaktifkan && gameObject.activeInHierarchy && isActiveAndEnabled;
    
    void SimpanPosisiAwal()
    {
        if(posisiTersimpan||bubbleChatTransform==null)return;
        posisiAsliBubble=bubbleChatTransform.localPosition;
        posisiTersimpan=true;
    }
    
    void Awake(){SimpanPosisiAwal();}
    void OnEnable() { sedangDinonaktifkan = false; SimpanPosisiAwal(); }
    void OnDisable()
    {
        sedangDinonaktifkan = true; StopAllCoroutines(); animasiCoroutine = null; wigglyCoroutine = null; bolehGoyang = false; skalaAnimasi=Vector3.zero; offsetGoyang=Vector3.zero;
        pemainDekat=false;dialogMenyembunyikan=false;sapaanKhusus=false;
        sapaanCoroutine=null;
        if (bubbleChatTransform != null) { bubbleChatTransform.localScale = Vector3.zero; if(posisiTersimpan)bubbleChatTransform.localPosition = posisiAsliBubble; }
    }

    void Start()
    {
        SimpanPosisiAwal();
        if (bubbleChatTransform != null)
        {
            bubbleChatTransform.localScale = Vector3.zero;
        }
        
        // Pastikan saat mulai, teks T nyala dan gambar Emote mati
        SetModeNormal();
    }

    // --- FUNGSI PENGATUR VISUAL ---
    private void SetModeNormal()
    {
        sapaanKhusus = false;
        if (teksSimbolAsli != null) { teksSimbolAsli.text = "..."; teksSimbolAsli.gameObject.SetActive(true); }
        if (rendererEmote != null) rendererEmote.gameObject.SetActive(false);
    }

    private void SetModeEmote(Sprite spriteBaru)
    {
        // Sapaan biasa tetap elipsis; emote cerita boleh menggantinya sementara.
        SetModeNormal();
        sapaanKhusus = true;
        if(spriteBaru!=null&&rendererEmote!=null){teksSimbolAsli?.gameObject.SetActive(false);rendererEmote.sprite=spriteBaru;rendererEmote.gameObject.SetActive(true);}
    }

    void LateUpdate()
    {
        if (bubbleChatTransform == null) return;
        bubbleChatTransform.localScale=skalaAnimasi;
        if(skalaAnimasi.sqrMagnitude < .000001f)return;
        if (kameraBubble == null) kameraBubble = Camera.main;
        if (kameraBubble == null) return;
        // Mulai dari penempatan manual setiap bingkai, bukan hasil koreksi sebelumnya.
        // Dengan begitu perubahan kamera tidak menumpuk perpindahan bubble.
        bubbleChatTransform.localPosition = posisiAsliBubble;
        bubbleChatTransform.rotation = kameraBubble.transform.rotation;
        if (teksSimbolAsli != null) teksSimbolAsli.transform.rotation = kameraBubble.transform.rotation;
        if (rendererEmote != null) rendererEmote.transform.rotation = kameraBubble.transform.rotation;
        if(gambarBubble==null)gambarBubble=bubbleChatTransform.GetComponentInChildren<SpriteRenderer>();
        CariKepalaNpc();
        float perbesaran = kepalaNpc != null ? Mathf.Max(1, pengaliUkuranNpc) : 1;
        if(tinggiBubbleDiLayar>0&&gambarBubble!=null)
        {
            var b=gambarBubble.localBounds;
            float muncul=Mathf.Clamp01(Mathf.Abs(skalaAnimasi.x/ukuranMaksimal.x));
            // Pivot gambar tidak selalu di tengah. Ulangi pengukuran setelah skala
            // berubah agar kamera menukik tidak menghasilkan bubble berbeda besar.
            for(int i=0;i<3;i++){
                var atas=kameraBubble.WorldToScreenPoint(gambarBubble.transform.TransformPoint(b.center+Vector3.up*b.extents.y));
                var bawah=kameraBubble.WorldToScreenPoint(gambarBubble.transform.TransformPoint(b.center-Vector3.up*b.extents.y));
                float tinggi=Mathf.Abs(atas.y-bawah.y);
                if(tinggi>.01f&&atas.z>0)bubbleChatTransform.localScale*=kameraBubble.pixelHeight*tinggiBubbleDiLayar*perbesaran*muncul/tinggi;
            }
        }
        else bubbleChatTransform.localScale *= perbesaran;
        KoreksiArahEkor(kameraBubble);
        bubbleChatTransform.localPosition += offsetGoyang;
    }

    void CariKepalaNpc()
    {
        if (kepalaSudahDicari) return;
        kepalaSudahDicari = true;
        if (kepalaNpc == null)
            foreach (var s in GetComponentsInChildren<SpriteRenderer>(true))
                if (!s.transform.IsChildOf(bubbleChatTransform) &&
                    (s.name == "KepalaArthur" || s.name == "Kepala" || s.name == "Head"))
                { kepalaNpc = s; break; }
        if (kepalaNpc != null) wajahNpc = kepalaNpc.GetComponent<EkspresiWajah2D>();
    }

    public bool KoreksiArahEkor(Camera kamera)
    {
        if (!arahkanEkorKeKepala || kamera == null || bubbleChatTransform == null) return false;
        CariKepalaNpc();
        // Komponen ini juga dipakai item. Item tidak boleh diperlakukan sebagai NPC.
        if (kepalaNpc == null) return false;
        if (gambarBubble == null) gambarBubble = bubbleChatTransform.GetComponentInChildren<SpriteRenderer>();
        if (gambarBubble == null || gambarBubble.sprite == null || gambarBubble.sprite.name != "BubbleAnjay") return false;

        // Ukur arah wajah dalam sudut pandang kamera, bukan sumbu dunia.
        // Hanya gambar bubble serta posisi isinya yang dicerminkan; huruf tetap normal.
        float arahWajah = Vector3.Dot(kepalaNpc.transform.TransformVector(Vector3.right), kamera.transform.right);
        if (kepalaNpc.flipX) arahWajah = -arahWajah;
        if (Mathf.Abs(arahWajah) > .05f)
        {
            bool kiri = arahWajah < 0;
            if (gambarBubble.flipX != kiri)
            {
                gambarBubble.flipX = kiri;
                if (teksSimbolAsli != null) CerminkanPosisiIsi(teksSimbolAsli.transform);
                if (rendererEmote != null) CerminkanPosisiIsi(rendererEmote.transform);
            }
        }

        var bounds = gambarBubble.sprite.bounds;
        // Ujung serta pangkal ekor pada sprite asli. Teks tidak ikut dicerminkan.
        Vector3 ujung = TitikGambar(bounds, .065f, .128f);
        Vector3 pangkal = TitikGambar(bounds, .285f, .255f);
        Vector3 tip = kamera.WorldToScreenPoint(gambarBubble.transform.TransformPoint(ujung));
        Vector3 basePoint = kamera.WorldToScreenPoint(gambarBubble.transform.TransformPoint(pangkal));
        Vector3 head = kamera.WorldToScreenPoint(kepalaNpc.transform.TransformPoint(kepalaNpc.localBounds.center));
        if (tip.z <= 0 || head.z <= 0) return false;

        var hb = kepalaNpc.localBounds;
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            {
                Vector3 p = hb.center + new Vector3(x * hb.extents.x, y * hb.extents.y, 0);
                Vector3 screen = kamera.WorldToScreenPoint(kepalaNpc.transform.TransformPoint(p));
                min = Vector2.Min(min, screen); max = Vector2.Max(max, screen);
            }
        Vector2 direction = (Vector2)(tip - basePoint);
        if (direction.sqrMagnitude < .01f) return false;
        direction.Normalize();
        Vector3 mulutLokal = hb.center + new Vector3(hb.size.x * .1f, -hb.size.y * .35f, 0);
        if (wajahNpc != null) mulutLokal = new Vector3(wajahNpc.posisiMulut.x, wajahNpc.posisiMulut.y, 0);
        Vector3 mulutDunia = titikMulutNpc != null ? titikMulutNpc.position : kepalaNpc.transform.TransformPoint(mulutLokal);
        Vector3 mouth = kamera.WorldToScreenPoint(mulutDunia);
        if (mouth.z <= 0) return false;

        // Sisakan jarak yang cukup agar ekor tidak menempel pada mulut.
        // Ukuran wajah di layar menjaga jarak tetap proporsional saat kamera berubah.
        Vector2 target = mouth;
        float distance = Mathf.Clamp((max.y - min.y) * jarakEkorDariMulut, 4f, 16f);
        Vector2 correctedTip = target - direction * distance;
        if (((Vector2)tip - correctedTip).sqrMagnitude < .25f) return false;

        Vector3 before = kamera.ScreenToWorldPoint(tip);
        Vector3 after = kamera.ScreenToWorldPoint(new Vector3(correctedTip.x, correctedTip.y, tip.z));
        bubbleChatTransform.position += after - before;
        return true;
    }

    void CerminkanPosisiIsi(Transform isi)
    {
        // Isi langsung memakai koordinat lokal agar pembalikan berulang tidak
        // kehilangan presisi pada world kampus yang jauh dari titik nol.
        if (isi.parent == gambarBubble.transform)
        {
            Vector3 lokal = isi.localPosition;
            lokal.x = -lokal.x;
            isi.localPosition = lokal;
            return;
        }
        Vector3 p = gambarBubble.transform.InverseTransformPoint(isi.position);
        p.x = -p.x;
        isi.position = gambarBubble.transform.TransformPoint(p);
    }

    Vector3 TitikGambar(Bounds b, float x, float y)
    {
        Vector3 p = b.min + new Vector3(b.size.x * x, b.size.y * y, b.extents.z);
        if (gambarBubble.flipX) p.x = -p.x;
        if (gambarBubble.flipY) p.y = -p.y;
        return p;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (BolehAnimasi && collision.CompareTag("Player"))
        {
            pemainDekat=true;
            if(sapaanKhusus)return;
            SetModeNormal();
            bool sembunyi=DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif);
            MulaiAnimasiBubble(sembunyi?Vector3.zero:ukuranMaksimal);
            
            if (wigglyCoroutine != null) StopCoroutine(wigglyCoroutine);
            if(!sembunyi)wigglyCoroutine = StartCoroutine(JedaDanGoyang());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!BolehAnimasi) return;
        if (collision.CompareTag("Player"))
        {
            pemainDekat=false;
            if(sapaanKhusus)return;
            MulaiAnimasiBubble(Vector3.zero);
            
            if (wigglyCoroutine != null) StopCoroutine(wigglyCoroutine);
            bolehGoyang = false;
            offsetGoyang = Vector3.zero;
            if (bubbleChatTransform != null) bubbleChatTransform.localPosition = posisiAsliBubble;
        }
    }

    public void MulaiAnimasiBubble(Vector3 skalaTarget)
    {
        if (animasiCoroutine != null) StopCoroutine(animasiCoroutine);
        if (bubbleChatTransform == null) return;
        if (!BolehAnimasi) { skalaAnimasi=Vector3.zero; bubbleChatTransform.localScale = Vector3.zero; return; }
        animasiCoroutine = StartCoroutine(AnimasiSkala(skalaTarget));
    }

    private IEnumerator AnimasiSkala(Vector3 targetSkala)
    {
        Vector3 skalaAwal = skalaAnimasi;
        float waktu = 0f;

        while (waktu < durasiAnimasi)
        {
            waktu += Time.deltaTime;
            skalaAnimasi = Vector3.Lerp(skalaAwal, targetSkala, waktu / durasiAnimasi);
            yield return null;
        }

        skalaAnimasi = targetSkala;
    }

    private IEnumerator JedaDanGoyang()
    {
        bolehGoyang = false; 
        yield return new WaitForSeconds(jedaWiggly);
        bolehGoyang = true; 
    }

    void Update()
    {
        bool dialog=DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif);
        if(dialog!=dialogMenyembunyikan)
        {
            dialogMenyembunyikan=dialog;
            // Scripted emotes remain available while the normal offer shrinks.
            bool emote=sapaanKhusus;
            if(pemainDekat&&!emote)
            {
                MulaiAnimasiBubble(dialog?Vector3.zero:ukuranMaksimal);
                if(wigglyCoroutine!=null)StopCoroutine(wigglyCoroutine);
                bolehGoyang=false;
                if(!dialog)wigglyCoroutine=StartCoroutine(JedaDanGoyang());
            }
        }
        if (bolehGoyang && bubbleChatTransform != null)
        {
            timerGoyang += Time.deltaTime;
            if (timerGoyang >= (1f / kecepatanGoyang))
            {
                timerGoyang = 0f;
                float xAcak = Random.Range(-kekuatanGoyang, kekuatanGoyang);
                float yAcak = Random.Range(-kekuatanGoyang, kekuatanGoyang);
                offsetGoyang = new Vector3(xAcak, yAcak, 0);
            }
        }
    }

    // --- FUNGSI SUTRADARA EMOTE ---
    public void AturPenawaranJalur(bool dekat)
    {
        if(!BolehAnimasi||pemainDekat==dekat)return;
        pemainDekat=dekat;if(sapaanKhusus)return;
        bool dialog=DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif);
        if(dekat)SetModeNormal();
        MulaiAnimasiBubble(dekat&&!dialog?ukuranMaksimal:Vector3.zero);
        if(wigglyCoroutine!=null)StopCoroutine(wigglyCoroutine);
        bolehGoyang=false;
        offsetGoyang=Vector3.zero;
        if(dekat&&!dialog)wigglyCoroutine=StartCoroutine(JedaDanGoyang());
        else if(bubbleChatTransform!=null&&posisiTersimpan)bubbleChatTransform.localPosition=posisiAsliBubble;
    }

    public void MulaiAnimasiBubbleKhusus(Sprite spriteBaru, float durasiPamer)
    {
        if(!BolehAnimasi)return;
        if(sapaanCoroutine!=null)StopCoroutine(sapaanCoroutine);
        SetModeEmote(spriteBaru); // Sulap jadi gambar!
        sapaanCoroutine=StartCoroutine(JalankanAnimasiKhusus(durasiPamer));
    }

    public void TampilkanTeksKhusus(string teks,float durasi)
    {
        if(!BolehAnimasi)return;
        if(sapaanCoroutine!=null)StopCoroutine(sapaanCoroutine);
        SetModeNormal();sapaanKhusus=true;
        if(teksSimbolAsli!=null)teksSimbolAsli.text=teks;
        sapaanCoroutine=StartCoroutine(JalankanAnimasiKhusus(durasi));
    }

    private IEnumerator JalankanAnimasiKhusus(float durasi)
    {
        if (bubbleChatTransform != null) bubbleChatTransform.gameObject.SetActive(true);
        MulaiAnimasiBubble(ukuranMaksimal);
        
        yield return new WaitForSeconds(durasi);
        
        MulaiAnimasiBubble(Vector3.zero);
        yield return new WaitForSeconds(durasiAnimasi); 
        
        SetModeNormal(); // Kembalikan ke teks "T"
        
        // Restore the normal offer only after the interaction is finished.
        MulaiAnimasiBubble(dialogMenyembunyikan||!pemainDekat?Vector3.zero:ukuranMaksimal);
        sapaanCoroutine=null;
    }
}