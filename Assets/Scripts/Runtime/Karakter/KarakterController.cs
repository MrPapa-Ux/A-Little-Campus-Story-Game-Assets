// PANDUAN PROYEK - Runtime / Karakter
// Fungsi: Mengatur jalan, lari, pengereman, arah visual, dan penguncian gerak Arthur.
// Panduan: Assets/Scripts/Runtime/Karakter/README.md

using UnityEngine;

public class KarakterController : MonoBehaviour
{
    [Header("Pengaturan Pergerakan")]
    public float kecepatanJalan = 5f;
    [Range(0.1f, 1f)] public float pengaliKecepatanJalan = 0.5f;
    [Range(0.15f, 0.5f)] public float jedaKetukGanda = 0.3f;
    public bool SedangBerlari { get; private set; }
    private float arahJalan = 0f;
    private Rigidbody2D rigidbodyKarakter;
    private ArthurJalurKampus jalurKampus;
    private float posisiXFisikaTerakhir;
    private float kecepatanGerakBebas;
    // Menahan tombol pada tembok tidak dihitung sebagai langkah yang memulihkan bar.
    public float KecepatanAktual => GerakTerkunci || arahJalan == 0f ? 0f :
        jalurKampus != null && jalurKampus.JalurAktif != null ? jalurKampus.KecepatanAktual : kecepatanGerakBebas;
    public float gayaNgerem = 20f;
    
    // INI GEMBOKNYA
    public bool kunciPergerakan = false; 
    CascadeTransition transisi;
    bool GerakTerkunci => kunciPergerakan || (transisi != null && transisi.SedangBerjalan) || (KawasanKampus.Instance != null && KawasanKampus.Instance.SedangMemuat);

    [Header("Wadah Utama & Animasi")]
    public Transform wadahVisual;
    public Animator animator; 

    // Memori Skrip
    private Vector3 skalaAsliWadah;
    private float waktuTekanTerakhir = -10f;
    private float arahTekanTerakhir;
    private bool ketukanPertamaDilepas;
    private bool punyaParameterLari;
    private AudioSource suaraLangkah;
    private AudioClip[] langkahLari;
    private readonly ContactPoint2D[] kontakLantai = new ContactPoint2D[12];
    private int indeksLangkah;
    public int JumlahLangkah { get; private set; }

    void Start()
    {
        rigidbodyKarakter = GetComponent<Rigidbody2D>();
        posisiXFisikaTerakhir = rigidbodyKarakter.position.x;
        jalurKampus = GetComponent<ArthurJalurKampus>();
        var pilihan = FindFirstObjectByType<SistemPilihan>();
        if(pilihan!=null)transisi=pilihan.efekTransisi;
        var objekSuara = new GameObject("Langkah Arthur");
        objekSuara.transform.SetParent(transform, false);
        suaraLangkah = objekSuara.AddComponent<AudioSource>();
        suaraLangkah.playOnAwake = false;
        suaraLangkah.spatialBlend = 0f;
        suaraLangkah.volume = .4f;
        langkahLari = new AudioClip[4];
        for (int i = 0; i < 4; i++)
        {
            langkahLari[i] = Resources.Load<AudioClip>("ArthurSfx/Lari" + (i + 1));
            if(langkahLari[i]!=null)langkahLari[i].LoadAudioData();
        }
        if(animator!=null)
        {
            var penerima=animator.GetComponent<PijakanAnimasiArthur>();
            if(penerima==null)penerima=animator.gameObject.AddComponent<PijakanAnimasiArthur>();
            penerima.pemilik=this;
        }
        if (animator != null)
            foreach (AnimatorControllerParameter parameter in animator.parameters)
                if (parameter.name == "isLari") punyaParameterLari = true;

        if (wadahVisual != null)
        {
            skalaAsliWadah = wadahVisual.localScale;
        }
    }

    // --- TOMBOL KONTROL UI ---
    public void JalanKeKiri() 
    { 
        TekanArah(-1f);
    }
    
    public void JalanKeKanan() 
    { 
        TekanArah(1f);
    }

    private void TekanArah(float arah)
    {
        if (GerakTerkunci || arahJalan == arah) return;
        SedangBerlari = ketukanPertamaDilepas && arahTekanTerakhir == arah && Time.unscaledTime - waktuTekanTerakhir <= jedaKetukGanda;
        arahJalan = arah;
        arahTekanTerakhir = arah;
        waktuTekanTerakhir = Time.unscaledTime;
        ketukanPertamaDilepas = false;
        SinkronkanAnimasi();
    }

    public void LepasKiri() { if (arahJalan < 0f) BerhentiJalan(); }
    public void LepasKanan() { if (arahJalan > 0f) BerhentiJalan(); }

    private void SinkronkanAnimasi()
    {
        if (animator == null) return;
        animator.SetBool("isJalan", arahJalan != 0f);
        if (punyaParameterLari) animator.SetBool("isLari", SedangBerlari);
    }
    
    public void BerhentiJalan() 
    { 
        if(suaraLangkah!=null)suaraLangkah.Stop();
        // Tombol lepas, Arthur berhenti (tetap berfungsi walau terkunci)
        ketukanPertamaDilepas = !SedangBerlari && arahJalan != 0f && Time.unscaledTime - waktuTekanTerakhir <= jedaKetukGanda;
        arahJalan = 0f;
        SedangBerlari = false;
        SinkronkanAnimasi();
    }

    // FUNGSI INI DIPANGGIL OLEH DIALOG MANAGER SAAT BUKA DIALOG
    public void BerhentiBergerak()
    {
        if(suaraLangkah!=null)suaraLangkah.Stop();
        arahJalan = 0f;
        SedangBerlari = false;
        ketukanPertamaDilepas = false;
        waktuTekanTerakhir = -10f;
        SinkronkanAnimasi();
        if(rigidbodyKarakter != null) rigidbodyKarakter.linearVelocity = Vector2.zero; // Rem mendadak total
    }

    void Update()
    {
        if (GerakTerkunci && arahJalan != 0f) BerhentiBergerak();
        // CharacterController 3D bergerak setiap frame, sebelum kamera LateUpdate.
        // Rigidbody2D Jasmine tetap menggunakan tick fisika di FixedUpdate.
        if (jalurKampus != null && jalurKampus.JalurAktif != null)
        {
            float kecepatan = kecepatanJalan * (SedangBerlari ? 1f : pengaliKecepatanJalan);
            jalurKampus.Gerakkan(arahJalan, kecepatan, GerakTerkunci, Time.deltaTime);
        }
        if (wadahVisual != null && arahJalan != 0f && !kunciPergerakan)
        {
            float skalaXFlip = (arahJalan > 0) ? skalaAsliWadah.x : -skalaAsliWadah.x;
            wadahVisual.localScale = new Vector3(skalaXFlip, skalaAsliWadah.y, skalaAsliWadah.z);
        }
    }

    public void PijakanLari()
    {
        bool memakaiJalur = jalurKampus != null && jalurKampus.JalurAktif != null;
        float gerak = memakaiJalur ? jalurKampus.KecepatanAktual : rigidbodyKarakter != null ? Mathf.Abs(rigidbodyKarakter.linearVelocity.x) : 0;
        if(!SedangBerlari||arahJalan==0||kunciPergerakan||Time.timeScale==0||gerak<.1f)return;
        bool diTanah = memakaiJalur && jalurKampus.DiTanah;
        if (!memakaiJalur && rigidbodyKarakter != null)
        {
            int jumlah = rigidbodyKarakter.GetContacts(kontakLantai);
            for (int i = 0; i < jumlah; i++)
                if (kontakLantai[i].normal.y > .5f) { diTanah = true; break; }
        }
        if(!diTanah)return;
        AudioClip[] klip = langkahLari;
        AudioClip langkah = klip[indeksLangkah++ % klip.Length];
        if (langkah != null)
        {
            suaraLangkah.PlayOneShot(langkah, SedangBerlari ? .9f : .8f);
            JumlahLangkah++;
        }
    }

    void FixedUpdate()
    {
        float perpindahan = Mathf.Abs(rigidbodyKarakter.position.x - posisiXFisikaTerakhir);
        posisiXFisikaTerakhir = rigidbodyKarakter.position.x;
        // Abaikan teleportasi, jatuh vertikal, serta pemindahan kawasan.
        kecepatanGerakBebas = perpindahan <= kecepatanJalan * Time.fixedDeltaTime * 2f + .01f
            ? perpindahan / Time.fixedDeltaTime : 0f;
        if (jalurKampus != null && jalurKampus.JalurAktif != null) return;
        if(GerakTerkunci){rigidbodyKarakter.linearVelocity=new Vector2(0,rigidbodyKarakter.linearVelocity.y);return;}
        if (arahJalan == 0f && rigidbodyKarakter.linearVelocity.x != 0)
        {
            Vector2 remMendadak = new Vector2(0f, rigidbodyKarakter.linearVelocity.y);
            rigidbodyKarakter.linearVelocity = Vector2.Lerp(rigidbodyKarakter.linearVelocity, remMendadak, Time.fixedDeltaTime * gayaNgerem);
        }
        else if (!GerakTerkunci) // Hanya bisa jalan jika tidak dikunci
        {
            float kecepatan = kecepatanJalan * (SedangBerlari ? 1f : pengaliKecepatanJalan);
            rigidbodyKarakter.linearVelocity = new Vector2(arahJalan * kecepatan, rigidbodyKarakter.linearVelocity.y);
        }
    }

    void OnApplicationFocus(bool fokus) { if (!fokus) BerhentiBergerak(); }
    void OnDisable() { BerhentiBergerak(); }
}
