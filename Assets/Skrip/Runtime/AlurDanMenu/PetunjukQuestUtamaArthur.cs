using TMPro;
using UnityEngine;

public sealed class PetunjukQuestUtamaArthur : MonoBehaviour
{
    public BukuArthur buku;
    public TMP_Text teks;
    SistemHP hp;
    DialogManager dialog;
    void Start(){hp=FindFirstObjectByType<SistemHP>();dialog=DialogManager.Instance;}
    void LateUpdate(){if(teks!=null)teks.alpha=(hp!=null&&hp.hpSedangBuka)||(dialog!=null&&(dialog.PercakapanAktif||dialog.minigameAktif||(dialog.arthurScript!=null&&dialog.arthurScript.kunciPergerakan)))?0:1;}
    void OnEnable(){if(buku!=null)buku.Berubah+=Segarkan;Segarkan();}
    void OnDisable(){if(buku!=null)buku.Berubah-=Segarkan;}
    void Segarkan(){
        if(teks==null)return;
        CatatanArthur tugas=buku!=null?buku.tugas.Find(x=>x!=null&&!x.selesai&&!string.IsNullOrEmpty(x.id)&&x.id.StartsWith("chapter1_")):null;
        teks.text=tugas!=null?"MISI UTAMA\n"+tugas.judul:"";
    }
}
