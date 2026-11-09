using System;
using UnityEngine;

// Bingkai buatan perancang untuk sudut sempit; tidak mengubah lintasan Arthur.
public sealed class LintasanKameraKampus : MonoBehaviour
{
    [Serializable] public struct Bingkai
    {
        public float z;
        public Vector3 posisi;
        public Vector3 bidikan;
        [Range(0,1)] public float dekat;
    }
    public Bingkai[] bingkai;
    [Tooltip("Haluskan kemajuan di lintasan, sehingga posisi kamera tidak memotong sudut antar bingkai.")]
    public bool halusDiLintasan;
    [Tooltip("Jaga bingkai kamera tetap dekat dengan Arthur saat berlari; nol mempertahankan penghalusan lama.")]
    [Min(0)] public float batasKetertinggalan;
    [Min(.05f)] public float waktuHalusLintasan=.45f;
    [Range(.15f,.6f)] public float tinggiPemainSaatDekat=.42f;
    public float Kedekatan { get; private set; }
    [HideInInspector] public bool parameterDalamAcuanWorld;
    [HideInInspector] public Vector3 pusatAcuan;
    [HideInInspector] public Quaternion rotasiAcuan = Quaternion.identity;
    [Min(.1f)] public float kecepatanMaksimum = 7.5f;
    [Min(1f)] public float putaranMaksimum = 30f;
    [Min(0f)] public float antisipasiLangkah = .65f;
    [Tooltip("Antisipasi arah pulang (Z bertambah), agar kamera keluar dari sisi tiang sebelum Arthur melewatinya.")]
    [Min(0f)] public float antisipasiPulang = 1.25f;
    float zSebelumnya;
    bool siap;
    float zHalus,kecepatanHalus;bool halusSiap;
    float arahTerakhir,waktuArah;
    public void Sampel(Vector3 acuan, out Vector3 posisi, out Vector3 bidikan, bool langsung = false)
    {
        float z = parameterDalamAcuanWorld ? (Quaternion.Inverse(rotasiAcuan)*(acuan-pusatAcuan)).z : acuan.z;
        float pergeseran = z - zSebelumnya;
        zSebelumnya = z;
        bool bergerak=Mathf.Abs(pergeseran)>.0001f&&Mathf.Abs(pergeseran)<3f;
        if(halusDiLintasan)
        {
            if(langsung){arahTerakhir=0;waktuArah=0;}
            else if(bergerak){arahTerakhir=Mathf.Sign(pergeseran);waktuArah=Time.time;}
            if(!langsung&&Time.time-waktuArah<.12f)
                z+=arahTerakhir*(arahTerakhir>0?antisipasiPulang:antisipasiLangkah);
        }
        else if (siap && !langsung && bergerak)
            z += Mathf.Sign(pergeseran) * (pergeseran > 0 ? antisipasiPulang : antisipasiLangkah);
        siap = true;
        if(bingkai==null||bingkai.Length==0){posisi=acuan;bidikan=acuan+Vector3.forward;Kedekatan=0;return;}
        if(halusDiLintasan)
        {
            if(langsung||!halusSiap){zHalus=z;kecepatanHalus=0;halusSiap=true;}
            else
            {
                float berikut=Mathf.SmoothDamp(zHalus,z,ref kecepatanHalus,waktuHalusLintasan,Mathf.Infinity,Time.deltaTime);
                Nilai(zHalus,out var lama,out _,out _);Nilai(berikut,out var baru,out _,out _);
                float jarak=Vector3.Distance(lama,baru),batas=kecepatanMaksimum*Time.deltaTime;
                if(jarak>batas&&jarak>.00001f)
                {
                    float rendah=0,tinggi=1;
                    for(int i=0;i<9;i++){float tengah=(rendah+tinggi)*.5f;Nilai(Mathf.Lerp(zHalus,berikut,tengah),out var uji,out _,out _);if(Vector3.Distance(lama,uji)>batas)tinggi=tengah;else rendah=tengah;}
                    berikut=Mathf.Lerp(zHalus,berikut,rendah);
                }
                if(batasKetertinggalan>0)
                {
                    berikut=Mathf.Clamp(berikut,z-batasKetertinggalan,z+batasKetertinggalan);
                    if(Mathf.Abs(berikut-z)>=batasKetertinggalan-.001f)kecepatanHalus=0;
                }
                zHalus=berikut;
            }
            z=zHalus;
        }
        Nilai(z,out posisi,out bidikan,out float dekat);Kedekatan=dekat;
    }
    void Nilai(float z,out Vector3 posisi,out Vector3 bidikan,out float dekat)
    {
        dekat=0;
        posisi = transform.position; bidikan = posisi + Vector3.forward;
        if (bingkai == null || bingkai.Length == 0) return;
        var awal = bingkai[0];
        if (z <= awal.z) { posisi = awal.posisi; bidikan = awal.bidikan; dekat=awal.dekat; return; }
        for (int i = 1; i < bingkai.Length; i++)
        {
            var akhir = bingkai[i];
            if (z <= akhir.z)
            {
                float t = Mathf.InverseLerp(awal.z, akhir.z, z);
                posisi = Vector3.Lerp(awal.posisi, akhir.posisi, t);
                bidikan = Vector3.Lerp(awal.bidikan, akhir.bidikan, t);
                dekat = Mathf.Lerp(awal.dekat,akhir.dekat,t);
                return;
            }
            awal = akhir;
        }
        posisi = awal.posisi; bidikan = awal.bidikan; dekat=awal.dekat;
    }
}
