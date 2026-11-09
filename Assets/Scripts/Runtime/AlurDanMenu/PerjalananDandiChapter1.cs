using System.Collections;
using UnityEngine;

// Melanjutkan state di scene cerita yang sama; jalur dan kamera eksplorasi tetap dipakai.
public sealed class PerjalananDandiChapter1 : MonoBehaviour
{
    public CeritaKamar201Arthur kamar;
    public NpcYangBisaDiatur dandi;
    public Transform depanKelas;
    public AreaInteraksi monologSetelahDandi;
    public string idQuest="chapter1_temui_dandi";
    public bool QuestAktif {get;private set;}
    public bool MomenTerekam2Aktif {get;private set;}
    public bool Selesai {get;private set;}
    SistemPilihan pilihan;
    void Start(){pilihan=FindFirstObjectByType<SistemPilihan>();if(pilihan!=null)pilihan.PercakapanSelesai+=Tuntas;}
    void Update(){
        if(kamar.Status!=CeritaKamar201Arthur.Tahap.Selesai||kamar.panel.activeSelf)return;
        if(!QuestAktif&&!Selesai){QuestAktif=true;Catat(false);}
        if(!QuestAktif||Selesai)return;
        if(!MomenTerekam2Aktif&&kamar.arthur.JalurAktif==dandi.jalurInteraksi){MomenTerekam2Aktif=true;if(kamar.notifikasiMomen!=null)kamar.notifikasiMomen.Tampilkan(2);}
    }
    void Catat(bool selesai){kamar.buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{id=idQuest,judul="Temui Dandi di depan kelas GK1 lantai 3",dikendalikanQuest=true,selesai=selesai,isi=selesai?"Arthur telah berbicara dengan Dandi di depan kelas.":"Keluar dari kamar 201. Pergi melalui GA menuju GK1, naik ke lantai 3, lalu berbicara dengan Dandi di depan kelas. Tidak perlu masuk ke kelas."});}
    void Tuntas(AreaInteraksi area,bool tuntas){if(!QuestAktif||!tuntas||area!=(monologSetelahDandi!=null?monologSetelahDandi:dandi.GetComponent<AreaInteraksi>()))return;Selesai=true;QuestAktif=false;MomenTerekam2Aktif=true;Catat(true);if(kamar.procrastination!=null)kamar.procrastination.tanpaBatasWaktu=true;}
    public void GagalPerjalanan(){StartCoroutine(Pulih());}
    IEnumerator Pulih(){
        var tujuan=MomenTerekam2Aktif?depanKelas:kamar.titikAwal;
        if(KawasanKampus.Instance!=null)yield return KawasanKampus.Instance.Pindahkan(tujuan,()=>kamar.arthur.PindahKe(tujuan));
        else kamar.arthur.PindahKe(tujuan);
        kamar.procrastination.PulihkanBar();kamar.procrastination.tanpaBatasWaktu=false;kamar.pemain.kunciPergerakan=false;
    }
    void OnDestroy(){if(pilihan!=null)pilihan.PercakapanSelesai-=Tuntas;}
}
