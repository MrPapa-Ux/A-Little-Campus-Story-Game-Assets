using System.Collections;
using UnityEngine;

// Adegan hanya selesai setelah baris terakhir dibaca; menekan Keluar lebih awal tidak menghilangkan NPC.
public sealed class AdeganNpcTaman : MonoBehaviour
{
    public bool menghilangSetelahDialog=true;
    public NPCBubbleTrigger[] jawabanBersama;
    public bool SudahSelesai {get;private set;}
    AreaInteraksi area;SistemPilihan pilihan;bool menjawab;
    void OnEnable()
    {
        if(SudahSelesai){gameObject.SetActive(false);return;}
        area=GetComponent<AreaInteraksi>();pilihan=FindFirstObjectByType<SistemPilihan>();
        if(pilihan!=null){pilihan.BarisSelesaiDiketik+=Baris;pilihan.PercakapanSelesai+=Tutup;}
    }
    void OnDisable(){if(pilihan!=null){pilihan.BarisSelesaiDiketik-=Baris;pilihan.PercakapanSelesai-=Tutup;}StopAllCoroutines();menjawab=false;}
    void Baris(AreaInteraksi pemilik,int indeks)
    {
        if(pemilik!=area||jawabanBersama==null||jawabanBersama.Length==0||indeks!=area.skenarioDialog.Length-1||menjawab)return;
        menjawab=true;StartCoroutine(Jawaban());
    }
    IEnumerator Jawaban()
    {
        yield return new WaitForSeconds(.6f);
        foreach(var b in jawabanBersama)if(b!=null)b.TampilkanTeksKhusus("Siap, kapiten!",4f);
        yield return new WaitForSeconds(.45f);
        if(DialogManager.Instance!=null&&DialogManager.Instance.PemilikInteraksi==area)pilihan.TekanKeluarLangsung();
    }
    void Tutup(AreaInteraksi pemilik,bool tuntas){if(pemilik==area&&tuntas&&menghilangSetelahDialog&&!SudahSelesai){SudahSelesai=true;StartCoroutine(Pudar());}}
    IEnumerator Pudar()
    {
        var interaksi=GetComponent<NpcYangBisaDiatur>();if(interaksi!=null)interaksi.enabled=false;
        area.enabled=false;
        var bubble=GetComponent<NPCBubbleTrigger>();if(bubble!=null)bubble.AturPenawaranJalur(false);
        yield return new WaitForSeconds(.3f);
        var gambar=GetComponentsInChildren<SpriteRenderer>(true);var warna=new Color[gambar.Length];
        for(int i=0;i<gambar.Length;i++)warna[i]=gambar[i].color;
        var bayangan=GetComponentsInChildren<MeshRenderer>(true);
        float t=0;while(t<1.8f){t+=Time.deltaTime;float alpha=1-Mathf.SmoothStep(0,1,t/1.8f);
            for(int i=0;i<gambar.Length;i++){var c=warna[i];c.a*=alpha;gambar[i].color=c;}
            if(alpha<.3f)foreach(var b in bayangan)b.enabled=false;
            yield return null;}
        gameObject.SetActive(false);
    }
}
