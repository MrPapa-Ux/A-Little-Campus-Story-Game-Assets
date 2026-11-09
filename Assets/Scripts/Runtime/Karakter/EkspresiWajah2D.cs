// PANDUAN PROYEK - Runtime / Karakter
// Fungsi: Mengatur kedipan, mulut bicara, serta ekspresi netral, senyum, senang, sedih, dan marah.
// Panduan: Assets/Scripts/Runtime/Karakter/README.md

using System.Collections.Generic;
using UnityEngine;

public enum EmosiKarakter { Netral, Senyum, Senang, Sedih, Marah }

// Lapisan bentuk sederhana mengikuti sprite kepala; tidak mengubah gambar aslinya.
public class EkspresiWajah2D : MonoBehaviour
{
    public SpriteRenderer kepala;
    public Vector2 mataKiri = new Vector2(-0.17f, -0.075f);
    public Vector2 mataKanan = new Vector2(0.30f, -0.015f);
    public Vector2 posisiMulut = new Vector2(0.10f, -0.355f);
    public Vector2 ukuranMaskMulut = new Vector2(0.16f, 0.065f);
    public Vector4 maskMataKiri = new Vector4(-0.35f, -0.235f, 0.01f, 0.005f);
    public Vector4 maskMataKanan = new Vector4(0.17f, -0.155f, 0.42f, 0.055f);
    public Color warnaKulit = new Color32(244, 193, 151, 255);
    [Tooltip("Matikan untuk NPC yang hanya membutuhkan mulut bicara; mata dan rambut aslinya dipertahankan.")]
    public bool animasiKedip = true;
    [Min(0.05f)] public float durasiKedip = 0.14f;
    public Vector2 jedaKedip = new Vector2(2.5f, 5f);
    [Header("Ekspresi bebas dari Inspector")]
    [Tooltip("Centang untuk memakai pilihan ekspresi. Bisa diubah saat Play Mode.")]
    public bool gunakanEkspresi;
    [Tooltip("Opsional untuk adegan sedih: mata menunduk lembut menggantikan mata setengah terbuka.")]
    public bool mataSedihMenunduk;
    [Range(0,1)] public float intensitasSedih=1f;
    public EmosiKarakter ekspresi = EmosiKarakter.Senyum;
    public EmosiKarakter EkspresiAktif { get; private set; }
    private EmosiKarakter? ekspresiDialog;
    private Transform mataEmosi, mulutEmosi;
    private GameObject lapisanEmosi;
    class CacheEmosi { public GameObject visual; public Transform mata, mulut; }
    private readonly Dictionary<EmosiKarakter, CacheEmosi> cacheEmosi = new Dictionary<EmosiKarakter, CacheEmosi>();
    public int JumlahModelEkspresiDibangun { get; private set; }
    private GameObject kelopak;
    private GameObject penutupMulut;
    private Transform mulut;
    private Material kulit;
    private Material tinta;
    private readonly List<Mesh> meshes = new List<Mesh>();
    private float kedipBerikutnya;
    private float akhirKedip;
    private float akhirUcapan;
    private int variasiUcapan;
    public bool SedangKedip => kelopak != null && kelopak.activeSelf;
    public bool MulutTerbuka => penutupMulut != null && penutupMulut.activeSelf;

    void Awake()
    {
        if (kepala == null) kepala = GetComponent<SpriteRenderer>();
        if (kepala == null) { enabled = false; return; }
        Shader shader = Shader.Find("Sprites/Default");
        kulit = new Material(shader) { color = kepala.color, mainTexture = kepala.sprite.texture };
        tinta = new Material(shader) { color = Color.black, mainTexture = Texture2D.whiteTexture };
        if (animasiKedip)
        {
            kelopak = new GameObject("Kelopak_Runtime"); kelopak.transform.SetParent(transform, false);
            Bentuk(kelopak.transform, "MaskMataKiri", Kotak(maskMataKiri), kulit, 1);
            Bentuk(kelopak.transform, "MaskMataKanan", Kotak(maskMataKanan), kulit, 1);
            Bentuk(kelopak.transform, "MataKiriTertutup", GarisMata(mataKiri, 0.155f), tinta, 2);
            Bentuk(kelopak.transform, "MataKananTertutup", GarisMata(mataKanan, 0.115f), tinta, 2);
        }
        penutupMulut = Bentuk(transform, "MaskMulut_Runtime", MaskMulut(posisiMulut, ukuranMaskMulut), kulit, 1);
        mulut = Bentuk(penutupMulut.transform, "MulutBicara_Runtime", Ellipse(Vector2.zero, Vector2.one), tinta, 2).transform;
        mulut.localPosition = new Vector3(posisiMulut.x, posisiMulut.y, -0.001f);
        if (kelopak != null) kelopak.SetActive(false);
        penutupMulut.SetActive(false);
        kedipBerikutnya = Time.time + Random.Range(1f, 3f);
        BuatEmosi(EmosiKarakter.Netral);
    }
    void Update()
    {
        if (kepala == null || !kepala.enabled) { HentikanBicara(); return; }
        if (animasiKedip && Time.time >= kedipBerikutnya) PaksaKedip();
        if (SedangKedip && Time.time >= akhirKedip) kelopak.SetActive(false);
        if (MulutTerbuka && Time.time >= akhirUcapan) HentikanBicara();
        EmosiKarakter target = ekspresiDialog ?? (gunakanEkspresi ? ekspresi : EmosiKarakter.Netral);
        if (target != EkspresiAktif) BuatEmosi(target);
        if (mataEmosi != null) mataEmosi.gameObject.SetActive(EkspresiAktif != EmosiKarakter.Senang || !SedangKedip);
        if (mulutEmosi != null) mulutEmosi.gameObject.SetActive(!MulutTerbuka);
    }
    public void PaksaKedip()
    {
        if (kelopak == null || !isActiveAndEnabled) return;
        kelopak.SetActive(true);
        akhirKedip = Time.time + durasiKedip;
        kedipBerikutnya = akhirKedip + Random.Range(jedaKedip.x, jedaKedip.y);
    }
    public void UcapkanHuruf()
    {
        if (penutupMulut == null || !isActiveAndEnabled) return;
        akhirUcapan = Time.time + 0.12f;
        penutupMulut.SetActive(true);
        variasiUcapan = (variasiUcapan + 1) % 3;
        mulut.localScale = new Vector3(0.065f + variasiUcapan * 0.008f, 0.025f + variasiUcapan * 0.012f, 1f);
    }
    public void HentikanBicara() { akhirUcapan = 0f; if (penutupMulut != null) penutupMulut.SetActive(false); }
    void OnDisable() { HentikanBicara(); ekspresiDialog = null; if (kelopak != null) kelopak.SetActive(false); }
    public void AturEkspresiDialog(EmosiKarakter emosi) { ekspresiDialog = emosi; if (kulit != null) BuatEmosi(emosi); }
    public void KembalikanEkspresi() { ekspresiDialog = null; if (kulit != null) BuatEmosi(gunakanEkspresi ? ekspresi : EmosiKarakter.Netral); }
    void OnDestroy()
    {
        foreach (Mesh mesh in meshes) Destroy(mesh);
        if (kulit != null) Destroy(kulit);
        if (tinta != null) Destroy(tinta);
    }
    // Hanya mengubah instans wajah pemanggil, tanpa mengubah NPC lain.
    public void AturSedihCerita(bool menunduk,float intensitas)
    {
        bool berubah=mataSedihMenunduk!=menunduk || !Mathf.Approximately(intensitasSedih,intensitas);
        mataSedihMenunduk=menunduk;intensitasSedih=Mathf.Clamp01(intensitas);
        gunakanEkspresi=true;ekspresi=EmosiKarakter.Sedih;
        if(berubah && cacheEmosi.TryGetValue(EmosiKarakter.Sedih,out CacheEmosi lama))
        {
            if(lapisanEmosi==lama.visual){lapisanEmosi=null;EkspresiAktif=EmosiKarakter.Netral;}
            if(lama.visual!=null){lama.visual.SetActive(false);Destroy(lama.visual);}
            cacheEmosi.Remove(EmosiKarakter.Sedih);
        }
        if(kulit!=null)BuatEmosi(EmosiKarakter.Sedih);
    }

    void BuatEmosi(EmosiKarakter emosi)
    {
        if (emosi == EkspresiAktif && (emosi == EmosiKarakter.Netral || lapisanEmosi != null)) return;
        EkspresiAktif = emosi;
        if (lapisanEmosi != null) lapisanEmosi.SetActive(false);
        lapisanEmosi = null; mataEmosi = null; mulutEmosi = null;
        if (emosi == EmosiKarakter.Netral) return;
        if (cacheEmosi.TryGetValue(emosi, out CacheEmosi cached)) { lapisanEmosi = cached.visual; mataEmosi = cached.mata; mulutEmosi = cached.mulut; lapisanEmosi.SetActive(true); return; }
        JumlahModelEkspresiDibangun++;
        lapisanEmosi = new GameObject("Ekspresi_" + emosi); lapisanEmosi.transform.SetParent(transform, false);
        mulutEmosi = new GameObject("MulutEkspresi").transform; mulutEmosi.SetParent(lapisanEmosi.transform, false);
        EmosiBentuk(mulutEmosi, "TutupMulut", MaskMulut(posisiMulut, ukuranMaskMulut), kulit, 3);
        float curve = emosi == EmosiKarakter.Sedih ? .035f*intensitasSedih : emosi == EmosiKarakter.Marah ? .035f : -.045f;
        EmosiBentuk(mulutEmosi, "GarisMulut", GarisKurva(posisiMulut, ukuranMaskMulut.x * .53f, curve, 0f, .009f), tinta, 4);
        if (emosi == EmosiKarakter.Senang)
            EmosiBentuk(mulutEmosi, "MulutSenang", Ellipse(posisiMulut + Vector2.down * .015f, new Vector2(.075f, .04f)), tinta, 4);
        if (emosi == EmosiKarakter.Senyum) { SimpanCache(emosi); return; }
        mataEmosi = new GameObject("MataEkspresi").transform; mataEmosi.SetParent(lapisanEmosi.transform, false);
        bool happy = emosi == EmosiKarakter.Senang;
        if (happy)
        {
            EmosiBentuk(mataEmosi, "TutupKiri", Kotak(maskMataKiri), kulit, 3);
            EmosiBentuk(mataEmosi, "TutupKanan", Kotak(maskMataKanan), kulit, 3);
            EmosiBentuk(mataEmosi, "MataKiri", GarisKurva(mataKiri, .115f, .035f, 0f, .009f), tinta, 4);
            EmosiBentuk(mataEmosi, "MataKanan", GarisKurva(mataKanan, .09f, .03f, 0f, .009f), tinta, 4);
        }
        else
        {
            if(emosi==EmosiKarakter.Sedih && mataSedihMenunduk)
            {
                EmosiBentuk(mataEmosi,"TutupMataSedihKiri",Kotak(maskMataKiri),kulit,3);
                EmosiBentuk(mataEmosi,"TutupMataSedihKanan",Kotak(maskMataKanan),kulit,3);
                EmosiBentuk(mataEmosi,"MataMenundukKiri",GarisKurva(mataKiri,.13f,-.018f*intensitasSedih,.012f*intensitasSedih,.008f),tinta,4);
                EmosiBentuk(mataEmosi,"MataMenundukKanan",GarisKurva(mataKanan,.10f,-.018f*intensitasSedih,-.012f*intensitasSedih,.008f),tinta,4);
            }
            // Sedih/marah mempertahankan gambar mata asli; yang berubah hanya alis dan mulut.
            EmosiBentuk(mataEmosi, "TutupAlisKiri", Kotak(new Vector4(maskMataKiri.x, maskMataKiri.w + .015f, maskMataKiri.z, maskMataKiri.w + .10f)), kulit, 3);
            EmosiBentuk(mataEmosi, "TutupAlisKanan", Kotak(new Vector4(maskMataKanan.x, maskMataKanan.w + .015f, maskMataKanan.z, maskMataKanan.w + .10f)), kulit, 3);
            float slope = emosi == EmosiKarakter.Marah ? -.045f : .045f*intensitasSedih;
            EmosiBentuk(mataEmosi, "AlisKiri", GarisKurva(new Vector2(mataKiri.x, maskMataKiri.w + .06f), .09f, .005f, slope, .009f), tinta, 4);
            EmosiBentuk(mataEmosi, "AlisKanan", GarisKurva(new Vector2(mataKanan.x, maskMataKanan.w + .06f), .08f, .005f, -slope, .009f), tinta, 4);
        }
        SimpanCache(emosi);
    }
    void SimpanCache(EmosiKarakter emosi) { cacheEmosi[emosi] = new CacheEmosi { visual = lapisanEmosi, mata = mataEmosi, mulut = mulutEmosi }; }
    void EmosiBentuk(Transform parent, string nama, Mesh mesh, Material material, int order) { Bentuk(parent, nama, mesh, material, order); }
    static Mesh GarisKurva(Vector2 center, float width, float curve, float slope, float thickness)
    {
        const int n = 12; var v = new Vector3[(n + 1) * 2]; var t = new int[n * 6]; var c = new Color[v.Length];
        for (int i = 0; i <= n; i++)
        {
            float u = i / (float)n; Vector2 p = center + new Vector2((u * 2f - 1f) * width, Mathf.Sin(u * Mathf.PI) * curve + (u * 2f - 1f) * slope);
            v[i * 2] = p + Vector2.up * thickness; v[i * 2 + 1] = p - Vector2.up * thickness; c[i * 2] = c[i * 2 + 1] = Color.white;
            if (i == n) continue; int a = i * 2, k = i * 6; t[k] = a; t[k + 1] = a + 1; t[k + 2] = a + 2; t[k + 3] = a + 1; t[k + 4] = a + 3; t[k + 5] = a + 2;
        }
        var mesh = new Mesh { name = "GarisEkspresi", vertices = v, triangles = t, colors = c }; mesh.RecalculateBounds(); return mesh;
    }
    GameObject Bentuk(Transform parent, string nama, Mesh mesh, Material material, int urutan)
    {
        if (material == kulit)
        {
            Rect rect = kepala.sprite.rect;
            Vector2 uv = new Vector2((rect.x + rect.width * 0.5f) / kepala.sprite.texture.width, (rect.y + rect.height * 0.72f) / kepala.sprite.texture.height);
            var coords = new Vector2[mesh.vertexCount]; for (int i = 0; i < coords.Length; i++) coords[i] = uv;
            mesh.uv = coords;
        }
        var go = new GameObject(nama, typeof(MeshFilter), typeof(MeshRenderer));
        go.layer = gameObject.layer;
        go.transform.SetParent(parent, false); go.transform.localPosition = new Vector3(0f, 0f, -0.001f * urutan);
        go.GetComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
        renderer.sortingLayerID = kepala.sortingLayerID; renderer.sortingOrder = kepala.sortingOrder + urutan;
        meshes.Add(mesh);
        return go;
    }
    static Mesh Kotak(Vector4 bounds)
    {
        var mesh = new Mesh { name = "EyeCover", vertices = new[] { new Vector3(bounds.x, bounds.y), new Vector3(bounds.z, bounds.y), new Vector3(bounds.z, bounds.w), new Vector3(bounds.x, bounds.w) }, triangles = new[] { 0, 1, 2, 0, 2, 3 }, colors = new[] { Color.white, Color.white, Color.white, Color.white } };
        mesh.RecalculateBounds(); return mesh;
    }
    static Mesh MaskMulut(Vector2 center, Vector2 size)
    {
        Vector2[] outline = { new Vector2(-0.6875f, 0.923f), new Vector2(1f, 0.923f), new Vector2(0.8125f, -0.154f), new Vector2(0.1875f, -0.769f), new Vector2(-0.6875f, -0.769f) };
        var vertices = new Vector3[outline.Length]; var colors = new Color[outline.Length];
        for (int i = 0; i < outline.Length; i++) { vertices[i] = center + Vector2.Scale(outline[i], size); colors[i] = Color.white; }
        var mesh = new Mesh { name = "MouthCover", vertices = vertices, colors = colors, triangles = new[] { 0, 1, 2, 0, 2, 3, 0, 3, 4 } };
        mesh.RecalculateBounds(); return mesh;
    }
    static Mesh Ellipse(Vector2 center, Vector2 radius)
    {
        const int segments = 32;
        var vertices = new Vector3[segments + 1]; var triangles = new int[segments * 3];
        vertices[0] = center;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            vertices[i + 1] = center + new Vector2(Mathf.Cos(angle) * radius.x, Mathf.Sin(angle) * radius.y);
            triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = (i + 1) % segments + 1;
        }
        var mesh = new Mesh { name = "FaceEllipse", vertices = vertices, triangles = triangles };
        var colors = new Color[vertices.Length]; for (int i = 0; i < colors.Length; i++) colors[i] = Color.white; mesh.colors = colors;
        mesh.RecalculateBounds(); return mesh;
    }
    static Mesh GarisMata(Vector2 center, float width)
    {
        const int segments = 8;
        var vertices = new Vector3[(segments + 1) * 2]; var triangles = new int[segments * 6];
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector2 point = center + new Vector2(Mathf.Lerp(-width, width, t), -0.025f * Mathf.Sin(t * Mathf.PI));
            vertices[i * 2] = point + Vector2.up * 0.008f; vertices[i * 2 + 1] = point - Vector2.up * 0.008f;
            if (i == segments) continue;
            int n = i * 6, v = i * 2;
            triangles[n] = v; triangles[n + 1] = v + 1; triangles[n + 2] = v + 2;
            triangles[n + 3] = v + 1; triangles[n + 4] = v + 3; triangles[n + 5] = v + 2;
        }
        var mesh = new Mesh { name = "ClosedEyeLine", vertices = vertices, triangles = triangles };
        var colors = new Color[vertices.Length]; for (int i = 0; i < colors.Length; i++) colors[i] = Color.white; mesh.colors = colors;
        mesh.RecalculateBounds(); return mesh;
    }
}
