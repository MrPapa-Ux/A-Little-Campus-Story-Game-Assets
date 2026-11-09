using System.Collections;
using UnityEngine;

public sealed class KepergianDandi : MonoBehaviour
{
    public Transform tujuanBerjalan;
    public AreaInteraksi monologArthur;
    public RuntimeAnimatorController animasiArthur;
    [Min(2)]public float lamaBerjalan=6f;
    [Min(.3f)]public float lamaPudar=1.5f;
    public bool SedangPergi {get;private set;}
    public bool SudahPergi {get;private set;}
    SistemPilihan pilihan;AreaInteraksi area;
    void Start(){area=GetComponent<AreaInteraksi>();pilihan=FindFirstObjectByType<SistemPilihan>();if(pilihan!=null)pilihan.PercakapanSelesai+=SelesaiBicara;}
    void SelesaiBicara(AreaInteraksi pemilik,bool tuntas){if(pemilik==area&&tuntas&&!SedangPergi&&!SudahPergi)StartCoroutine(Pergi());}
    IEnumerator Pergi(){
        SedangPergi=true;var dm=DialogManager.Instance;var arthur=dm!=null?dm.arthurScript:null;
        if(arthur!=null){arthur.BerhentiBergerak();arthur.kunciPergerakan=true;}
        if(dm!=null)dm.minigameAktif=true;
        var npc=GetComponent<NpcYangBisaDiatur>();npc.enabled=false;area.enabled=false;
        var bubble=GetComponent<NPCBubbleTrigger>();if(bubble!=null)bubble.AturPenawaranJalur(false);
        var menghadap=GetComponent<NPCMenghadapArthur>();if(menghadap!=null)menghadap.enabled=false;
        var life=GetComponent<NpcKampusHidup>();if(life!=null)life.enabled=false;
        var visual=life!=null?life.visual:null;var cam=Camera.main;
        Vector3 awal=transform.position,akhir=tujuanBerjalan!=null?tujuanBerjalan.position:awal+Vector3.back*6;
        if(visual!=null&&cam!=null){visual.rotation=Quaternion.Euler(0,cam.transform.eulerAngles.y,0);var s=visual.localScale;s.x=Mathf.Abs(s.x)*(Vector3.Dot(akhir-awal,cam.transform.right)>=0?1:-1);visual.localScale=s;}
        Animator animator=null;
        // Kurva Arthur mengarah langsung ke KepalaArthur, Badan, Tangan dan Kaki.
        // Pada NPC, semuanya berada di WadahKosong di dalam Visual.
        var wadahAnimasi=life!=null&&life.kepala!=null?life.kepala.parent:visual;
        if(wadahAnimasi!=null&&animasiArthur!=null){animator=wadahAnimasi.GetComponent<Animator>();if(animator==null)animator=wadahAnimasi.gameObject.AddComponent<Animator>();animator.runtimeAnimatorController=animasiArthur;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.fireEvents=false;animator.enabled=true;animator.Rebind();animator.SetBool("isJalan",true);animator.Update(0);}
        float mulai=Time.time;
        while(Time.time-mulai<lamaBerjalan){var p=Vector3.Lerp(awal,akhir,Mathf.Clamp01((Time.time-mulai)/lamaBerjalan));if(transform is RectTransform r)r.anchoredPosition3D=p;else transform.position=p;yield return null;}
        if(animator!=null)animator.SetBool("isJalan",false);
        var gambar=GetComponentsInChildren<SpriteRenderer>(true);var warna=new Color[gambar.Length];for(int i=0;i<gambar.Length;i++)warna[i]=gambar[i].color;
        mulai=Time.time;while(Time.time-mulai<lamaPudar){float alpha=1-Mathf.SmoothStep(0,1,(Time.time-mulai)/lamaPudar);for(int i=0;i<gambar.Length;i++){var c=warna[i];c.a*=alpha;gambar[i].color=c;}yield return null;}
        foreach(var renderer in GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
        if(animator!=null)animator.enabled=false;SedangPergi=false;SudahPergi=true;
        // Tetap aktif sampai monolog dipanggil, sehingga coroutine tidak terputus saat model disembunyikan.
        if(dm!=null){dm.minigameAktif=false;dm.TawarkanInteraksi(monologArthur);}
        if(pilihan!=null&&monologArthur!=null)pilihan.AksiTombolInteraksi();
        else if(arthur!=null)arthur.kunciPergerakan=false;
    }
    void OnDestroy(){if(pilihan!=null)pilihan.PercakapanSelesai-=SelesaiBicara;}
}
