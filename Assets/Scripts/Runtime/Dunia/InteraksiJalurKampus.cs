using UnityEngine;
using System.Collections.Generic;

// Pendeteksian memakai posisi 3D dan jalur aktif; tidak memicu tempat di lantai/sisi lain.
[RequireComponent(typeof(AreaInteraksi))]
[DefaultExecutionOrder(200)]
public sealed class InteraksiJalurKampus : MonoBehaviour
{
    public JalurKampus jalur;
    public JalurKampus[] jalurTambahan;
    [Min(.1f)] public float radius = 2.4f;
    public Transform ikon;
    public Vector3 ukuranIkon = new Vector3(.1f, .1f, .1f);
    [Min(.05f)] public float durasiMuncul = .2f;
    [Min(.05f)] public float durasiMenciut = .18f;
    [Tooltip("Khusus penanda pencarian item. Interaksi tempat harus memakai false agar bubble menetap pada lokasi.")]
    public bool ikutiKepalaArthur;
    [Tooltip("Tinggi kepala Arthur saat berdiri pada titik interaksi; tidak dibaca dari posisi Arthur yang sedang berjalan.")]
    public float tinggiKepalaDiTitik = -.11f;
    [Min(0)] public float jarakDariKepala = .35f;
    [Tooltip("Tinggi bubble sebagai bagian tinggi layar. Nol mempertahankan ukuran dunia; berguna untuk kamera tetap yang jauh.")]
    [Range(0, .35f)] public float tinggiIkonDiLayar;
    [Tooltip("Haluskan posisi layar saat kamera sinematik berputar. Nol memakai posisi langsung.")]
    [Min(0)] public float waktuHalusDiLayar;
    Vector2 posisiLayarHalus, kecepatanLayar;
    bool layarSiap;
    SpriteRenderer kepalaArthur;
    SpriteRenderer gambarIkon;
    ArthurJalurKampus arthur;
    AreaInteraksi area;
    bool dekat;
    float kemunculan;
    static readonly List<InteraksiJalurKampus> daftar = new List<InteraksiJalurKampus>();
    static int bingkai = -1;
    static InteraksiJalurKampus terpilih;
    static float jarakTempat;
    public bool SedangDitampilkan => dekat;
    public float SkalaKemunculan => kemunculan;
    void OnEnable() { if (!daftar.Contains(this)) daftar.Add(this); bingkai = -1; }
    void OnDisable() { daftar.Remove(this); bingkai = -1; dekat = false; kemunculan = 0; layarSiap=false; if (ikon != null) ikon.localScale = Vector3.zero; if(area!=null&&DialogManager.Instance!=null)DialogManager.Instance.HapusInteraksi(area); }
    bool Cocok(JalurKampus aktif)
    {
        if (aktif == null) return false;
        if (jalur == aktif) return true;
        if (jalurTambahan != null) foreach (var tambahan in jalurTambahan) if (tambahan == aktif) return true;
        return false;
    }
    static void Pilih(ArthurJalurKampus pemain)
    {
        if (bingkai == Time.frameCount) return;
        bingkai = Time.frameCount; terpilih = null;
        float terbaik = float.PositiveInfinity;
        foreach (var calon in daftar)
        {
            if (calon == null || !calon.isActiveAndEnabled || !calon.Cocok(pemain.JalurAktif) || !MalamGAChapter1.BubbleBoleh(calon.name)) continue;
            float d = (pemain.transform.position - calon.transform.position).sqrMagnitude;
            if (d > calon.radius * calon.radius) continue;
            if (d < terbaik) { terbaik = d; terpilih = calon; }
        }
        jarakTempat=terbaik;
        NpcYangBisaDiatur.Terdekat(pemain,out float jarakNpc);
        if(jarakNpc<terbaik)terpilih=null;
    }
    public static float JarakTempatTerdekat(ArthurJalurKampus pemain)
    {
        Pilih(pemain);return jarakTempat;
    }
    void Start() { arthur = FindFirstObjectByType<ArthurJalurKampus>(); area = GetComponent<AreaInteraksi>(); if (ikon != null) ikon.localScale = Vector3.zero; }
    void Update()
    {
        if (arthur == null || area == null || DialogManager.Instance == null) return;
        Pilih(arthur);
        bool baru = terpilih == this;
        // Penanda tersembunyi tidak perlu menulis ulang Transform tiap frame.
        if (!baru && !dekat && kemunculan <= 0f) return;
        var dialog = DialogManager.Instance;
        if (!dialog.PercakapanAktif)
        {
            if (baru) area.TawarkanInteraksi();
            else if (dekat) dialog.HapusInteraksi(area);
        }
        dekat = baru;
        if (ikon != null)
        {
            bool tampil = dekat && !dialog.PercakapanAktif && !dialog.minigameAktif;
            // Finish the previous bubble's exit before revealing a different offer.
            if(tampil && kemunculan <= .001f)
                foreach(var lain in daftar)if(lain!=this&&lain!=null&&lain.kemunculan>.001f){tampil=false;break;}
            float durasi=tampil?durasiMuncul:durasiMenciut;
            kemunculan=Mathf.MoveTowards(kemunculan,tampil?1:0,Time.deltaTime/Mathf.Max(.05f,durasi));
            ikon.localScale = ukuranIkon * kemunculan;
            if (arthur.kamera != null) ikon.rotation = arthur.kamera.transform.rotation;
        }
    }
    void LateUpdate()
    {
        if (kemunculan <= .001f || ikon == null || arthur == null || arthur.kamera == null || ikon.localScale.sqrMagnitude < .00001f) { layarSiap=false; return; }
        if (ikutiKepalaArthur && kepalaArthur == null) kepalaArthur = arthur.GetComponent<KetukKepalaArthur>().kepala;
        if (gambarIkon == null) gambarIkon = ikon.GetComponent<SpriteRenderer>();
        if (gambarIkon == null || (ikutiKepalaArthur && kepalaArthur == null)) return;
        var kamera = arthur.kamera;
        // Place markers use their own standing point, never the moving player.
        // Head-following remains an explicit opt-in for future item search markers.
        Vector3 acuan = ikutiKepalaArthur ? kepalaArthur.bounds.center : transform.position + Vector3.up * tinggiKepalaDiTitik;
        float atasKepala = ikutiKepalaArthur ? kepalaArthur.bounds.max.y : acuan.y;
        ikon.rotation = kamera.transform.rotation;
        ikon.position = acuan;
        for(int ukur=0;ukur<(tinggiIkonDiLayar>0?3:1);ukur++)
        {
          if(tinggiIkonDiLayar>0){
            var b = gambarIkon.localBounds;
            var atas = kamera.WorldToScreenPoint(gambarIkon.transform.TransformPoint(b.center + Vector3.up * b.extents.y));
            var bawah = kamera.WorldToScreenPoint(gambarIkon.transform.TransformPoint(b.center - Vector3.up * b.extents.y));
            float tinggi = Mathf.Abs(atas.y - bawah.y);
            if (tinggi > .01f && atas.z > 0)
                ikon.localScale *= kamera.pixelHeight * tinggiIkonDiLayar * kemunculan / tinggi;
          }
          ikon.position=acuan;
          ikon.position += Vector3.up * (atasKepala + jarakDariKepala - gambarIkon.bounds.min.y);
        }
        var layar = kamera.WorldToScreenPoint(ikon.position);
        var kepala = kamera.WorldToScreenPoint(acuan);
        if (layar.z <= 0 || kepala.z <= 0) return;
        layar.x = kepala.x;
        if(waktuHalusDiLayar>0)
        {
            Vector2 tujuan=new Vector2(layar.x,layar.y);
            if(!layarSiap){posisiLayarHalus=tujuan;kecepatanLayar=Vector2.zero;layarSiap=true;}
            posisiLayarHalus=Vector2.SmoothDamp(posisiLayarHalus,tujuan,ref kecepatanLayar,waktuHalusDiLayar,Mathf.Infinity,Time.deltaTime);
            layar.x=posisiLayarHalus.x;layar.y=posisiLayarHalus.y;
        }
        ikon.position = kamera.ScreenToWorldPoint(layar);
    }
    void OnDrawGizmosSelected() { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, radius); }
}
