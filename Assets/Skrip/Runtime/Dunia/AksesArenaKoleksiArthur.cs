using System.Collections;
using UnityEngine;

// The original preview arena runs as an overlay, keeping the world and save intact.
public sealed class AksesArenaKoleksiArthur : MonoBehaviour
{
    public const string PetunjukKoleksi="Kumpulkan semua item di Chapter ini, untuk membuka tantangan yang masih dirahasiakan.";
    public GameObject arenaPrefab;
    public BukuArthur buku;
    void Start(){if(buku!=null){buku.Berubah+=CatatAkses;CatatAkses();}}
    void OnDestroy(){if(buku!=null)buku.Berubah-=CatatAkses;}
    void CatatAkses()
    {
        if(buku==null||buku.tugas.Exists(t=>t.id=="koleksi_arena_terbuka")||!Terbuka)return;
        buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{id="koleksi_arena_terbuka",judul="Dua tempat yang belum biasa",isi="Koleksi sudah lengkap. Periksa tanda ? di GK1 dan akses depan Edelweiss. Kedua permainan dapat diulang kapan saja.",dikendalikanQuest=true});
    }
    public bool Terbuka
    {
        get {
            if(buku==null)return false;
            foreach(var q in FindObjectsByType<QuestKontakArthur>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(q.buku==buku)foreach(var m in q.misi)if(m.tahap!=3)return false;
            foreach(var t in FindObjectsByType<TantanganSampingArthur>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(t.buku==buku&&t.ikonHadiah!=null&&!t.Selesai)return false;
            return true;
        }
    }
    public void Buka(){if(Terbuka&&arenaPrefab!=null)StartCoroutine(Mulai());}
    IEnumerator Mulai()
    {
        var p=FindFirstObjectByType<SistemPilihan>();p?.TutupPercakapanTerarah();
        while(DialogManager.Instance!=null&&DialogManager.Instance.PercakapanAktif)yield return null;
        var d=DialogManager.Instance;var hp=FindFirstObjectByType<SistemHP>();
        if(hp!=null&&hp.hpSedangBuka)hp.TutupHP();
        if(d!=null){d.minigameAktif=true;if(d.arthurScript!=null){d.arthurScript.BerhentiBergerak();d.arthurScript.kunciPergerakan=true;}}
        var go=Instantiate(arenaPrefab);go.GetComponent<PratinjauMinigameBayangan>().keluarKeKampus=true;
        while(go!=null)yield return null;
        if(d!=null){d.minigameAktif=false;if(d.arthurScript!=null)d.arthurScript.kunciPergerakan=false;}
    }
}
