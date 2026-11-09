// PANDUAN PROYEK - Runtime / Karakter
// Fungsi: Mengatur pose kepala/tangan dan HP kecil saat karakter membuka atau menutup ponsel.
// Panduan: Assets/Scripts/Runtime/Karakter/README.md

using UnityEngine;

[DefaultExecutionOrder(200)]
public class PoseMemegangHP : MonoBehaviour
{
    [Header("Sambungan karakter")]
    public Transform wadahVisual, kepala, rambut, telinga, tanganKiri, tanganKanan;
    public AnimationClip poseBerdiri;
    public SpriteRenderer hpKecil;
    public SpriteRenderer layarKecil;
    [Header("Gaya memegang HP")]
    [Range(.1f, .8f)] public float durasiPose = .3f;
    [Range(-18f, 0f)] public float tunduk = -8f;
    public Vector3 posisiHP = new Vector3(.94f, -.57f, -.15f);
    public Vector3 posisiTanganKanan = new Vector3(1.12f, -.81f, -.18f);
    public Vector3 posisiTanganKiri = new Vector3(.50f, -.87f, -.18f);
    public bool satuTangan = true;
    [Tooltip("Balik telapak tangan ke atas saat memegang HP")]
    public bool balikTelapak = true;
    public float sudutTanganKanan = -20f;
    public bool SedangMemegang => diminta || blend > .001f;
    public float KekuatanPose => blend;
    bool diminta, punyaPose;
    float blend;
    Transform[] bagian;
    Vector3[] posisi;
    Quaternion[] rotasi;
    Vector3 skalaTangan;
    void Awake() { bagian = new[] { kepala, rambut, telinga, tanganKiri, tanganKanan }; posisi = new Vector3[bagian.Length]; rotasi = new Quaternion[bagian.Length]; if (hpKecil != null) hpKecil.gameObject.SetActive(false); }
    public void Buka(Color warna)
    {
        if (wadahVisual == null || kepala == null) return;
        if (poseBerdiri != null) poseBerdiri.SampleAnimation(wadahVisual.gameObject, 0f);
        for (int i = 0; i < bagian.Length; i++) if (bagian[i] != null) { posisi[i] = bagian[i].localPosition; rotasi[i] = bagian[i].localRotation; }
        punyaPose = true; diminta = true;
        skalaTangan=tanganKanan.localScale;
        if (hpKecil != null) { hpKecil.color = warna; hpKecil.gameObject.SetActive(true); }
        if(layarKecil!=null)layarKecil.color=Color.Lerp(warna,new Color(.81f,.78f,.69f,1f),.85f);
    }
    public void Tutup() { diminta = false; }
    void LateUpdate()
    {
        if (!punyaPose) return;
        blend = Mathf.MoveTowards(blend, diminta ? 1f : 0f, Time.unscaledDeltaTime / Mathf.Max(.05f, durasiPose));
        float t = blend * blend * (3f - 2f * blend);
        Quaternion tilt = Quaternion.Euler(0f, 0f, tunduk * t);
        Vector3 head = posisi[0] + new Vector3(.04f, -.065f, 0f) * t;
        for (int i = 0; i < bagian.Length; i++)
        {
            if (bagian[i] == null) continue;
            if (i < 3) { bagian[i].localPosition = head + tilt * (posisi[i] - posisi[0]); bagian[i].localRotation = tilt * rotasi[i]; }
            else if (i == 3 && satuTangan) { bagian[i].localPosition = posisi[i]; bagian[i].localRotation = rotasi[i]; }
            else
            {
                bagian[i].localPosition = Vector3.Lerp(posisi[i], i == 3 ? posisiTanganKiri : posisiTanganKanan, t);
                bagian[i].localRotation = Quaternion.Slerp(rotasi[i], Quaternion.Euler(0f,0f,i == 3 ? 18f : sudutTanganKanan), t);
                if(i==4) bagian[i].localScale=new Vector3(skalaTangan.x,skalaTangan.y*(balikTelapak ? 1f-2f*t : 1f),skalaTangan.z);
            }
        }
        if (hpKecil != null)
        {
            hpKecil.transform.localPosition = posisiHP + Vector3.down * (.18f * (1f - t));
            hpKecil.transform.localRotation = Quaternion.Euler(0f,0f,-8f);
            Color c = hpKecil.color; c.a = t; hpKecil.color = c;
            if(layarKecil!=null){Color bg=layarKecil.color;bg.a=t;layarKecil.color=bg;}
        }
        if (!diminta && blend <= 0f) { punyaPose = false; if (hpKecil != null) hpKecil.gameObject.SetActive(false); }
    }
    void OnDisable()
    {
        diminta = false; blend = 0f;
        if (punyaPose && bagian != null) for (int i = 0; i < bagian.Length; i++) if (bagian[i] != null) { bagian[i].localPosition = posisi[i]; bagian[i].localRotation = rotasi[i]; }
        punyaPose = false; if (hpKecil != null) hpKecil.gameObject.SetActive(false);
        if(tanganKanan!=null && skalaTangan!=Vector3.zero)tanganKanan.localScale=skalaTangan;
    }
}
