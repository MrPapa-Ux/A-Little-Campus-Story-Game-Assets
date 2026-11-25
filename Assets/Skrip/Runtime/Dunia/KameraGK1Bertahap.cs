using UnityEngine;

// Zoom dahulu; ganti sisi pandang saat Arthur sudah dekat. Kamera tidak
// diinterpolasi melintasi dinding antara dua posisi yang berjauhan.
public sealed class KameraGK1Bertahap : MonoBehaviour
{
    [Range(2,5)] public int lantai=2;
    public float sumbuKoridor=1745.5f;
    public float tinggiLantai=10.04f;
    public float titikPutarKiri=-37f, titikPutarKanan=-94f;
    float putaranHalus, kecepatanPutar;
    bool putaranSiap;
    float zoomAtasHalus, ujungAtasHalus, kecepatanZoomAtas, kecepatanUjungAtas;
    bool kameraAtasSiap;
    readonly RaycastHit[] halangan = new RaycastHit[24];

    public void Bingkai(Vector3 pemain,out Vector3 posisi,out Vector3 bidikan,out float fov,bool langsung=false)
    {
        if(lantai<=3)
        {
            Koridor(pemain,out posisi,out bidikan,out fov,langsung);
            return;
        }
        float z=pemain.z;
        float arah=z>-65.5f?1:-1;
        float zoom=Mathf.SmoothStep(0,1,arah>0
            ?Mathf.InverseLerp(-52.5f,titikPutarKiri,z)
            :Mathf.InverseLerp(-80.5f,titikPutarKanan,z));
        float putar=Mathf.SmoothStep(0,1,arah>0
            ?Mathf.InverseLerp(titikPutarKiri,titikPutarKiri+6,z)
            :Mathf.InverseLerp(titikPutarKanan,titikPutarKanan-6,z));
        if(langsung||!kameraAtasSiap)
        {
            zoomAtasHalus=zoom;ujungAtasHalus=putar;
            kecepatanZoomAtas=kecepatanUjungAtas=0;kameraAtasSiap=true;
        }
        else
        {
            float dt=Mathf.Min(Time.deltaTime,1f/30);
            zoomAtasHalus=Mathf.SmoothDamp(zoomAtasHalus,zoom,ref kecepatanZoomAtas,.65f,.8f,dt);
            ujungAtasHalus=Mathf.SmoothDamp(ujungAtasHalus,putar,ref kecepatanUjungAtas,.65f,.8f,dt);
        }
        // Blend camera offsets relative to Arthur. Following Z immediately prevents
        // running from leaving the camera behind; the arm and turn ease independently.
        // The arm enters the corridor below the next slab and above the railing.
        float dekat=zoomAtasHalus, putaran=ujungAtasHalus;
        // Hold the approved courtyard frame while upper-floor Arthur takes the last
        // steps toward the solid middle corner; the facade must not cover him.
        float zBingkaiLebar=arah>0?Mathf.Max(z,-52.5f):Mathf.Min(z,-80.5f);
        Vector3 lebar=new Vector3(sumbuKoridor-15.5f,tinggiLantai+6,zBingkaiLebar);
        Vector3 rapat=new Vector3(sumbuKoridor-Mathf.Lerp(1.4f,.7f,putaran),
            tinggiLantai+Mathf.Lerp(2.8f,2.5f,putaran),
            Mathf.Clamp(z+arah*4.5f*putaran,-110.2f,-17.3f));
        float kananUjung=arah<0?Mathf.InverseLerp(-103,-107.5f,z):0;
        if(arah>0)rapat.y+=1.9f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(-28,-21.5f,z));
        posisi=Vector3.Lerp(lebar,rapat,dekat);
        bidikan=Vector3.Lerp(new Vector3(sumbuKoridor,tinggiLantai+1,zBingkaiLebar),
            new Vector3(sumbuKoridor,tinggiLantai+Mathf.Lerp(.95f,.55f,kananUjung),z),dekat);
        fov=Mathf.Lerp(60,Mathf.Lerp(84,Mathf.Lerp(48,54,kananUjung),putaran),dekat);
    }

    // Kamera lantai atas berada di bawah plafon dan di sisi dalam pagar.
    // Hanya sudut pandangnya yang diperhalus; posisi sepanjang koridor selalu
    // mengikuti Arthur, sehingga berlari tidak meninggalkan kamera di luar world.
    void Koridor(Vector3 pemain,out Vector3 posisi,out Vector3 bidikan,out float fov,bool langsung)
    {
        float z=pemain.z;
        float putaran=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-49,-82,z));
        if(langsung||!putaranSiap)
        {putaranHalus=putaran;kecepatanPutar=0;putaranSiap=true;}
        // Frame yang tersendat tidak boleh mengejar seluruh putaran sekaligus.
        else putaranHalus=Mathf.SmoothDamp(putaranHalus,putaran,ref kecepatanPutar,.38f,1,Mathf.Min(Time.deltaTime,1f/30));
        float tengah=Mathf.Sin(putaranHalus*Mathf.PI);
        posisi=new Vector3(sumbuKoridor-1.3f-.3f*tengah,
            tinggiLantai+2.2f+.4f*tengah,
            Mathf.Clamp(z+Mathf.Lerp(4,-4,putaranHalus),-110.2f,-20.8f));
        bidikan=new Vector3(sumbuKoridor+.7f,tinggiLantai+1.25f,z);
        float ujungKiri=Mathf.SmoothStep(0,1,Mathf.InverseLerp(-26,-21.5f,z));
        posisi.y+=.2f*ujungKiri;
        fov=Mathf.Lerp(Mathf.Lerp(50,88,tengah),90,ujungKiri);

        // Batasi lengan kamera pada geometri bangunan, bukan trigger interaksi.
        // Pemeriksaan dari dada dan kaki turut melindungi tubuh sprite yang lebar.
        Vector3 pusat=new Vector3(sumbuKoridor,tinggiLantai+1.15f,z);
        Vector3 lengan=posisi-pusat;
        float aman=1;
        for(int titik=0;titik<3;titik++)
        {
            float tinggi=titik==0?.25f:titik==1?1.15f:1.85f;
            Vector3 asal=new Vector3(sumbuKoridor,tinggiLantai+tinggi,z);
            Vector3 arah=posisi-asal;float jarak=arah.magnitude;
            int jumlah=Physics.SphereCastNonAlloc(asal,.12f,arah.normalized,halangan,jarak,~0,QueryTriggerInteraction.Ignore);
            for(int i=0;i<jumlah;i++)
                if(halangan[i].collider is MeshCollider)
                    aman=Mathf.Min(aman,Mathf.Clamp01((halangan[i].distance-.18f)/jarak));
        }
        if(aman<1)
        {
            posisi=pusat+lengan*Mathf.Max(.15f,aman);
            fov=Mathf.Min(105,2*Mathf.Atan(Mathf.Tan(fov*Mathf.Deg2Rad*.5f)/Mathf.Max(.3f,aman))*Mathf.Rad2Deg);
        }
    }
}
