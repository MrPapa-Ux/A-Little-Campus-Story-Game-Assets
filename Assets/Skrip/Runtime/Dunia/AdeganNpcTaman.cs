using System.Collections;
using UnityEngine;

// Adegan hanya selesai setelah baris terakhir dibaca; menekan Keluar lebih awal tidak menghilangkan NPC.
public sealed class AdeganNpcTaman : MonoBehaviour
{
    public bool menghilangSetelahDialog=true;
    [Tooltip("Opsional: kelompok dosen dan mahasiswa yang ikut menghilang setelah percakapan selesai.")]
    public GameObject kelompokIkutPergi;
    [Tooltip("Opsional: identitas adegan sekali selesai yang diteruskan dari Chapter 1 ke Eksplorasi.")]
    public string idProgres;
    public NPCBubbleTrigger[] jawabanBersama;
    [Header("Teguran Developer (opsional)")]
    public bool tegurJikaDilewatiCepat;
    public bool pudarOtomatisDiAkhir;
    [TextArea] public string dialogTeguran="Nah, muncul lagi. Tadi kau baca, atau cuma cari tombol lanjut? Kali ini dengarkan dulu. Tenang, ini bukan bimbingan skripsi.";
    public bool SudahSelesai {get;private set;}
    public bool PernahMenegur {get;private set;}
    public bool KunciLewati => tegurJikaDilewatiCepat&&PernahMenegur&&!SudahSelesai;
    AreaInteraksi area;SistemPilihan pilihan;bool menjawab;
    bool sedangPudar;int jumlahLewati;BarisDialog[] dialogAwal;
    void OnEnable()
    {
        if(SudahSelesai){(kelompokIkutPergi!=null?kelompokIkutPergi:gameObject).SetActive(false);return;}
        area=GetComponent<AreaInteraksi>();pilihan=FindFirstObjectByType<SistemPilihan>();
        if(dialogAwal==null&&area!=null)dialogAwal=area.skenarioDialog;
        if(pilihan!=null){pilihan.BarisSelesaiDiketik+=Baris;pilihan.PercakapanSelesai+=Tutup;}
    }
    void OnDisable(){if(pilihan!=null){pilihan.BarisSelesaiDiketik-=Baris;pilihan.PercakapanSelesai-=Tutup;}StopAllCoroutines();menjawab=false;sedangPudar=false;}
    public void MulaiMengobrol(){if(tegurJikaDilewatiCepat){jumlahLewati=0;menjawab=false;}}
    public void CatatLewati(){if(tegurJikaDilewatiCepat&&!KunciLewati)jumlahLewati++;}
    void Baris(AreaInteraksi pemilik,int indeks)
    {
        if(pemilik!=area||indeks!=area.skenarioDialog.Length-1||menjawab)return;
        if(pudarOtomatisDiAkhir){menjawab=true;StartCoroutine(AkhirDeveloper());return;}
        if(jawabanBersama==null||jawabanBersama.Length==0)return;
        menjawab=true;StartCoroutine(Jawaban());
    }
    IEnumerator AkhirDeveloper()
    {
        yield return new WaitForSeconds(1.1f);
        if(DialogManager.Instance!=null&&DialogManager.Instance.PercakapanAktif&&DialogManager.Instance.PemilikInteraksi==area)
            pilihan.TutupPercakapanTerarah();
    }
    IEnumerator Jawaban()
    {
        yield return new WaitForSeconds(.6f);
        foreach(var b in jawabanBersama)if(b!=null)b.TampilkanTeksKhusus("Siap, kapiten!",4f);
        yield return new WaitForSeconds(.45f);
        if(DialogManager.Instance!=null&&DialogManager.Instance.PemilikInteraksi==area)pilihan.TekanKeluarLangsung();
    }
    void Tutup(AreaInteraksi pemilik,bool tuntas)
    {
        if(pemilik!=area||!tuntas||!menghilangSetelahDialog||SudahSelesai||sedangPudar)return;
        bool kembali=tegurJikaDilewatiCepat&&!PernahMenegur&&jumlahLewati>0;
        if(kembali)PernahMenegur=true;
        else {SudahSelesai=true;ProgresSampingArthur.SimpanJikaEksplorasi(gameObject.scene);}
        StartCoroutine(Pudar(kembali));
    }
    public void PulihkanSelesai(){SudahSelesai=true;(kelompokIkutPergi!=null?kelompokIkutPergi:gameObject).SetActive(false);}
    IEnumerator Pudar(bool kembali=false)
    {
        sedangPudar=true;
        var kelompok=kelompokIkutPergi!=null?kelompokIkutPergi:gameObject;
        var interaksi=kelompok.GetComponentsInChildren<NpcYangBisaDiatur>(true);var aktifInteraksi=new bool[interaksi.Length];
        for(int i=0;i<interaksi.Length;i++){aktifInteraksi[i]=interaksi[i].enabled;interaksi[i].enabled=false;}
        area.enabled=false;
        var bubble=GetComponent<NPCBubbleTrigger>();if(bubble!=null)bubble.AturPenawaranJalur(false);
        yield return new WaitForSeconds(.3f);
        var gambar=kelompok.GetComponentsInChildren<SpriteRenderer>(true);var warna=new Color[gambar.Length];
        for(int i=0;i<gambar.Length;i++)warna[i]=gambar[i].color;
        var bayangan=kelompok.GetComponentsInChildren<MeshRenderer>(true);
        var aktifBayangan=new bool[bayangan.Length];for(int i=0;i<bayangan.Length;i++)aktifBayangan[i]=bayangan[i].enabled;
        float t=0;while(t<1.8f){t+=Time.deltaTime;float alpha=1-Mathf.SmoothStep(0,1,t/1.8f);
            for(int i=0;i<gambar.Length;i++){var c=warna[i];c.a*=alpha;gambar[i].color=c;}
            if(alpha<.3f)foreach(var b in bayangan)b.enabled=false;
            yield return null;}
        if(!kembali){kelompok.SetActive(false);yield break;}
        // Keep the host active while invisible, so its return and dialogue subscriptions survive.
        yield return new WaitForSeconds(.55f);
        // Repeat the advice and its original ending after the one-time reprimand.
        var awal=dialogAwal??System.Array.Empty<BarisDialog>();var ulang=new BarisDialog[awal.Length+1];
        ulang[0]=new BarisDialog{siapaYangBicara=Pembicara.Kanan_NPC,namaKarakter="Developer",teksDialog=dialogTeguran,teksTombolLanjut="Lanjut"};
        System.Array.Copy(awal,0,ulang,1,awal.Length);area.skenarioDialog=ulang;area.tampilkanTombolKeluar=false;menjawab=false;
        float masuk=0;
        while(masuk<.35f){masuk+=Time.deltaTime;float alpha=Mathf.SmoothStep(0,1,masuk/.35f);
            for(int i=0;i<gambar.Length;i++){var c=warna[i];c.a*=alpha;gambar[i].color=c;}yield return null;}
        for(int i=0;i<gambar.Length;i++)gambar[i].color=warna[i];
        for(int i=0;i<bayangan.Length;i++)bayangan[i].enabled=aktifBayangan[i];
        area.enabled=true;
        for(int i=0;i<interaksi.Length;i++)interaksi[i].enabled=aktifInteraksi[i];
        if(bubble!=null)bubble.TampilkanTeksKhusus("Rajin skip, ya.",4f);
        sedangPudar=false;
    }
}
