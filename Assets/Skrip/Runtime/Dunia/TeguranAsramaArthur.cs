using System.Collections;
using UnityEngine;

// Repeatable entrance scene. It never teleports the player or completes a quest.
public sealed class TeguranAsramaArthur : MonoBehaviour
{
    public GameObject penjagaPutra, penjagaPutri;
    public bool SedangBerjalan {get;private set;}
    void OnDisable(){
        if(!SedangBerjalan)return;
        StopAllCoroutines();
        if(penjagaPutra!=null)penjagaPutra.SetActive(false);
        if(penjagaPutri!=null)penjagaPutri.SetActive(false);
        if(DialogManager.Instance!=null){DialogManager.Instance.minigameAktif=false;DialogManager.Instance.TutupDialog();}
        SedangBerjalan=false;
    }
    public static bool PerluTeguran(OpsiInteraksi opsi,bool perempuan,JalurKampus jalurSekarang)
    {
        if(opsi==null)return false;
        if(opsi.aksesJasmine!=null&&!opsi.aksesJasmine.keluar)return !perempuan;
        var tujuan=opsi.titikTujuan!=null?opsi.titikTujuan.GetComponent<TujuanJalurKampus>()?.jalur:null;
        if(!perempuan||tujuan==null)return false;
        string next=tujuan.name,now=jalurSekarang!=null?jalurSekarang.name:"";
        if(next=="GH_Kamar201")return now!=next;
        if(next=="GH_BalkonLantaiDua")return now!="GH_Kamar201"&&now!=next;
        if(next.StartsWith("GH_"))return !now.StartsWith("GH_");
        return next.StartsWith("UNK_CrystalInterior_")&&!now.StartsWith("UNK_CrystalInterior_");
    }
    public static bool Cegat(OpsiInteraksi opsi)
    {
        var pemain=FindFirstObjectByType<ArthurJalurKampus>();
        var tubuh=pemain!=null?pemain.GetComponent<PertukaranTubuhArthur>():null;
        bool perempuan=tubuh!=null&&tubuh.DiTubuhPerempuan;
        if(!PerluTeguran(opsi,perempuan,pemain!=null?pemain.JalurAktif:null))return false;
        var scene=FindFirstObjectByType<TeguranAsramaArthur>();
        if(scene==null)return false;
        if(!scene.SedangBerjalan)scene.StartCoroutine(scene.Mainkan(pemain,perempuan));
        return true;
    }
    IEnumerator Mainkan(ArthurJalurKampus pemain,bool perempuan)
    {
        SedangBerjalan=true;
        var dialog=DialogManager.Instance;var pilihan=FindFirstObjectByType<SistemPilihan>();var kontrol=pemain.GetComponent<KarakterController>();
        var penjaga=perempuan?penjagaPutra:penjagaPutri;
        var rig=penjaga!=null?penjaga.GetComponent<NpcKampusHidup>():null;
        try{
            pilihan.TutupPercakapanTerarah();
            while(dialog.PercakapanAktif)yield return null;
            dialog.minigameAktif=true;kontrol.BerhentiBergerak();kontrol.kunciPergerakan=true;
            var kaki=pemain.transform.position-Vector3.up*ArthurJalurKampus.JarakKaki;
            var kanan=pemain.kamera.transform.right;kanan.y=0;kanan.Normalize();
            var awal=kaki+kanan*6;var dekat=kaki+kanan*1.8f;
            penjaga.transform.SetPositionAndRotation(awal,Quaternion.Euler(0,pemain.kamera.transform.eulerAngles.y,0));penjaga.SetActive(true);
            if(rig!=null){rig.langkahMengikutiGerak=true;rig.waktuJalan=0;rig.kegiatan=KegiatanNpcKampus.Berjalan;}
            float t=0;while(t<1.7f){t+=Time.unscaledDeltaTime;var sebelum=penjaga.transform.position;penjaga.transform.position=Vector3.Lerp(awal,dekat,Mathf.SmoothStep(0,1,t/1.7f));if(rig!=null)rig.waktuJalan+=Vector3.Distance(sebelum,penjaga.transform.position)/2.5f;yield return null;}
            if(rig!=null)rig.kegiatan=KegiatanNpcKampus.Berdiri;
            var area=penjaga.GetComponent<AreaInteraksi>();string nama=perempuan?"Bram":"Yuna";
            area.skenarioDialog=new[]{new BarisDialog{namaKarakter=nama,siapaYangBicara=Pembicara.Kanan_NPC,teksDialog="Hei, apa yang kau lakukan!",teksTombolLanjut="Lanjut",ubahEkspresi=true,ekspresi=EmosiKarakter.Netral},new BarisDialog{namaKarakter=perempuan?"Naya":"Arthur",siapaYangBicara=Pembicara.Kiri_Arthur,teksDialog="...",teksTombolLanjut="Lanjut"},new BarisDialog{namaKarakter=nama,siapaYangBicara=Pembicara.Kanan_NPC,teksDialog=perempuan?"Ini asrama putra. Kamu tidak boleh masuk.":"Ini asrama putri. Kamu tidak boleh masuk.",teksTombolLanjut="Baik"}};
            area.daftarOpsi=System.Array.Empty<OpsiInteraksi>();area.tampilkanTombolKeluar=true;
            dialog.minigameAktif=false;pilihan.SegarkanPercakapan(area);
            while(dialog.PercakapanAktif)yield return null;
            kontrol.kunciPergerakan=true;t=0;
            if(rig!=null)rig.kegiatan=KegiatanNpcKampus.Berjalan;
            while(t<1.7f){t+=Time.unscaledDeltaTime;var sebelum=penjaga.transform.position;penjaga.transform.position=Vector3.Lerp(dekat,awal,t/1.7f);if(rig!=null)rig.waktuJalan+=Vector3.Distance(sebelum,penjaga.transform.position)/2.5f;yield return null;}
        }finally{
            if(rig!=null)rig.kegiatan=KegiatanNpcKampus.Berdiri;
            if(penjaga!=null)penjaga.SetActive(false);
            if(dialog!=null)dialog.minigameAktif=false;
            if(kontrol!=null){kontrol.BerhentiBergerak();kontrol.kunciPergerakan=false;}
            SedangBerjalan=false;
        }
    }
}
