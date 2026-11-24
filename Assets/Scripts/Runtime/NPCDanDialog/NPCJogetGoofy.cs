// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Menggerakkan bagian tubuh NPC untuk tarian goofy tanpa menggeser bubble.
// Panduan: Assets/Scripts/Runtime/NPCDanDialog/README.md

using UnityEngine;

public class NPCJogetGoofy : MonoBehaviour
{
    [Header("Joget dengan bagian tubuh; bubble tetap di tempat")]
    public bool sedangJoget = true;
    [Range(.25f, 3f)] public float kecepatan = 1.5f;
    [Range(.1f, 2f)] public float kekuatan = 1f;
    public Transform badan, kepala, tanganKiri, tanganKanan, kakiKiri, kakiKanan;
    public Transform rambut, telinga;
    Transform[] parts;
    Vector3[] posisi;
    Quaternion[] rotasi;
    bool ikutiAba, aktifSebelumAba, salahAba;
    float mulaiAba, arahAba;
    Transform wadahTarian;
    Vector3 posisiWadah,skalaWadah,geserTarian;
    float arahLayar;
    Animator animatorTarian;
    NpcKampusHidup hidupTarian;
    bool animatorSebelumTarian,hidupSebelumTarian;

    public void MulaiIkutiAba()
    {
        if (ikutiAba) return;
        aktifSebelumAba = enabled;
        if (enabled) ResetPose();
        SimpanPoseDasar();
        wadahTarian=badan!=null?badan.parent:null;
        if(wadahTarian!=null){posisiWadah=wadahTarian.localPosition;skalaWadah=wadahTarian.localScale;}
        geserTarian=Vector3.zero;
        animatorTarian=GetComponent<KarakterController>()?.animator;
        if(animatorTarian!=null){animatorSebelumTarian=animatorTarian.enabled;animatorTarian.enabled=false;}
        hidupTarian=GetComponent<NpcKampusHidup>();
        if(hidupTarian!=null){hidupSebelumTarian=hidupTarian.enabled;hidupTarian.enabled=false;}
        mulaiAba = float.NegativeInfinity;
        ikutiAba = true;
        enabled = true;
    }

    public void GerakAba(int arah, bool salah, float waktu, Vector3 kananLayar)
    {
        if (!ikutiAba) return;
        var induk = badan != null && badan.parent != null ? badan.parent : transform;
        arahLayar=arah==0?-1:1;
        if(wadahTarian!=null){
            // Balik seluruh visual, termasuk rambut; titik interaksi dan collider tidak bergerak.
            var s=skalaWadah;s.x=Mathf.Abs(s.x)*arahLayar;wadahTarian.localScale=s;
            var dunia=kananLayar;dunia.y=0;dunia.Normalize();
            geserTarian=wadahTarian.parent!=null?wadahTarian.parent.InverseTransformVector(dunia*arahLayar*.32f):dunia*arahLayar*.32f;
        }
        float orientasi = Vector3.Dot(induk.TransformVector(Vector3.right), kananLayar) < 0f ? -1f : 1f;
        arahAba = (arah == 0 ? -1f : 1f) * orientasi;
        salahAba = salah;
        mulaiAba = waktu;
    }

    public void SelesaiIkutiAba()
    {
        if (!ikutiAba) return;
        ikutiAba = false;
        ResetPose();
        if(wadahTarian!=null){wadahTarian.localPosition=posisiWadah;wadahTarian.localScale=skalaWadah;}
        if(animatorTarian!=null)animatorTarian.enabled=animatorSebelumTarian;
        if(hidupTarian!=null)hidupTarian.enabled=hidupSebelumTarian;
        enabled = aktifSebelumAba;
    }

    void PoseAba()
    {
        if(float.IsNegativeInfinity(mulaiAba)){ResetPose();return;}
        float berlalu=Mathf.Max(0,Time.unscaledTime-mulaiAba);
        // Terus shuffle selama menunggu aba-aba berikutnya, bukan satu gerak lalu diam.
        float tekan = .35f+.65f*Mathf.Abs(Mathf.Sin(berlalu*7f));
        if(wadahTarian!=null)wadahTarian.localPosition=Vector3.Lerp(wadahTarian.localPosition,posisiWadah+geserTarian,1-Mathf.Exp(-14*Time.unscaledDeltaTime));
        float geser = arahAba * tekan;
        float naik = tekan * .045f;
        Vector3 pinggul = new Vector3(geser * .17f, naik, 0f);
        Quaternion miringKepala = Quaternion.Euler(0f, 0f, -geser * (salahAba ? 20f : 15f));
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == null) continue;
            Vector3 pindah = pinggul;
            Quaternion putar = Quaternion.Euler(0f, 0f, -geser * 15f);
            if (i == 1 || i >= 6)
            {
                pindah += posisi[1] + miringKepala * (posisi[i] - posisi[1]) - posisi[i];
                putar = miringKepala;
            }
            else if (i == 2 || i == 3)
            {
                float sisi = i == 2 ? -1f : 1f;
                pindah += new Vector3((salahAba ? sisi * tekan : geser) * .09f, tekan * (salahAba ? .17f : .07f), 0f);
                putar = Quaternion.Euler(0f, 0f, salahAba ? sisi * tekan * 46f : -geser * 37f);
            }
            else if (i == 4 || i == 5)
            {
                pindah = new Vector3(geser * .09f, salahAba ? naik : 0f, 0f);
                putar = Quaternion.identity;
            }
            parts[i].localPosition = posisi[i] + pindah;
            parts[i].localRotation = putar * rotasi[i];
        }
    }
    void Awake(){SimpanPoseDasar();}
    public void SimpanPoseDasar()
    {
        parts = new[] { badan, kepala, tanganKiri, tanganKanan, kakiKiri, kakiKanan, rambut, telinga };
        posisi = new Vector3[parts.Length]; rotasi = new Quaternion[parts.Length];
        for (int i = 0; i < parts.Length; i++) if (parts[i] != null) { posisi[i] = parts[i].localPosition; rotasi[i] = parts[i].localRotation; }
    }
    void LateUpdate()
    {
        if (ikutiAba) { PoseAba(); return; }
        if (!sedangJoget) { ResetPose(); return; }
        float phase = Time.time * kecepatan * Mathf.PI * 2f;
        float sway = Mathf.Sin(phase);
        Vector3 hip = new Vector3(sway * .11f, -(1f - Mathf.Cos(phase * 2f)) * .035f, 0f) * kekuatan;
        Quaternion headTilt = Quaternion.Euler(0f, 0f, sway * 7f * kekuatan);
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == null) continue;
            bool foot = i == 4 || i == 5, headPart = i == 1 || i >= 6;
            if (foot)
            {
                // Kedua telapak tetap menapak. Pergeseran kecil memberi shuffle, tanpa mengangkat pose keseluruhan.
                parts[i].localPosition = posisi[i] + new Vector3(sway * .025f * (i == 4 ? 1f : -1f) * kekuatan, 0f, 0f);
                parts[i].localRotation = rotasi[i];
            }
            else if (headPart)
            {
                parts[i].localPosition = posisi[1] + hip + headTilt * (posisi[i] - posisi[1]);
                parts[i].localRotation = headTilt * rotasi[i];
            }
            else if (i == 2 || i == 3)
            {
                float wave = Mathf.Sin(phase + (i == 2 ? 0f : Mathf.PI));
                parts[i].localPosition = posisi[i] + hip + new Vector3(wave * .13f, Mathf.Sin(phase * 2f) * .07f, 0f) * kekuatan;
                parts[i].localRotation = rotasi[i] * Quaternion.Euler(0f, 0f, wave * 32f * kekuatan);
            }
            else { parts[i].localPosition = posisi[i] + hip; parts[i].localRotation = rotasi[i] * Quaternion.Euler(0f, 0f, sway * 9f * kekuatan); }
        }
    }
    void ResetPose() { if (parts == null) return; for (int i = 0; i < parts.Length; i++) if (parts[i] != null) { parts[i].localPosition = posisi[i]; parts[i].localRotation = rotasi[i]; } }
    void OnDisable() { ResetPose(); }
}
