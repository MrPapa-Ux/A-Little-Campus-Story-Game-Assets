using System.Collections;
using UnityEngine;

// Dipasang pada pengelola yang tetap aktif ketika Arthur mengunjungi kawasan lain.
public sealed class NpcKunciMotor : MonoBehaviour
{
    public GameObject npc;
    public AreaInteraksi area;
    public NPCBubbleTrigger bubble;
    public Transform[] lintasanPergi;
    [Range(0,1)] public float peluangPerDetik=.01f;
    public bool SudahBerinteraksi {get;private set;}
    public bool Selesai {get;private set;}
    public bool SedangPergi {get;private set;}
    public int JumlahUndian {get;private set;}
    float detik;bool dialogTerlihat;
    ArthurJalurKampus arthur;
    void Start(){arthur=FindFirstObjectByType<ArthurJalurKampus>();}
    bool Dijeda => Time.timeScale<=0 || (KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat) ||
        (DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif));
    void Update()
    {
        if(npc==null||Selesai)return;
        var d=DialogManager.Instance;
        if(d!=null&&d.PercakapanAktif&&d.PemilikInteraksi==area)dialogTerlihat=true;
        if(dialogTerlihat&&d!=null&&!d.PercakapanAktif)SudahBerinteraksi=true;
        if(!SudahBerinteraksi||Dijeda)return;
        detik+=Time.deltaTime;
        if(detik<1)return;
        detik%=1;JumlahUndian++;CobaTemukan(Random.value);
    }
    public bool CobaTemukan(float undian)
    {
        if(!SudahBerinteraksi||Selesai||Dijeda||undian>=peluangPerDetik)return false;
        Selesai=true;
        if(area!=null)area.enabled=false;
        foreach(var c in npc.GetComponents<Collider2D>())c.enabled=false;
        bool terlihat=npc.activeInHierarchy&&arthur!=null&&arthur.kamera!=null;
        if(terlihat){var v=arthur.kamera.WorldToViewportPoint(npc.transform.position+Vector3.up);terlihat=v.z>0&&v.x>-.1f&&v.x<1.1f&&v.y>0&&v.y<1.1f;}
        if(terlihat)StartCoroutine(Pergi());else npc.SetActive(false);
        return true;
    }
    IEnumerator Pergi()
    {
        SedangPergi=true;
        bubble?.TampilkanTeksKhusus("Ketemu!",9);
        yield return new WaitForSeconds(1.1f);
        var bagian=npc.GetComponentsInChildren<SpriteRenderer>(true);
        Transform kiri=null,kanan=null;Vector3 pk=Vector3.zero,pr=Vector3.zero;
        foreach(var r in bagian){if(r.name=="KakiKiri"){kiri=r.transform;pk=kiri.localPosition;}if(r.name=="KakiKanan"){kanan=r.transform;pr=kanan.localPosition;}}
        if(lintasanPergi!=null)foreach(var titik in lintasanPergi)
        {
            if(titik==null)continue;
            while(npc.activeInHierarchy&&Vector3.Distance(npc.transform.position,titik.position)>.02f)
            {
                if(KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat){npc.SetActive(false);SedangPergi=false;yield break;}
                npc.transform.position=Vector3.MoveTowards(npc.transform.position,titik.position,Time.deltaTime*.8f);
                float langkah=Mathf.Sin(Time.time*10)*.045f;
                if(kiri!=null)kiri.localPosition=pk+Vector3.up*Mathf.Max(0,langkah);
                if(kanan!=null)kanan.localPosition=pr+Vector3.up*Mathf.Max(0,-langkah);
                yield return null;
            }
        }
        var meshes=npc.GetComponentsInChildren<MeshRenderer>(true);var warna=new Color[bagian.Length];for(int i=0;i<bagian.Length;i++)warna[i]=bagian[i].color;
        var blok=new MaterialPropertyBlock();float t=0;
        while(t<1.2f&&npc.activeInHierarchy){t+=Time.deltaTime;float alpha=1-Mathf.Clamp01(t/1.2f);
            for(int i=0;i<bagian.Length;i++){var c=warna[i];c.a*=alpha;bagian[i].color=c;}
            foreach(var r in meshes){if(r==null)continue;if(r.shadowCastingMode==UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly){r.enabled=alpha>.2f;continue;}if(r.sharedMaterial==null||!r.sharedMaterial.HasProperty("_BaseColor"))continue;var c=r.sharedMaterial.GetColor("_BaseColor");c.a*=alpha;blok.SetColor("_BaseColor",c);r.SetPropertyBlock(blok);}
            if(bubble!=null&&bubble.teksSimbolAsli!=null){var c=bubble.teksSimbolAsli.color;c.a=alpha;bubble.teksSimbolAsli.color=c;}
            yield return null;}
        npc.SetActive(false);SedangPergi=false;
    }
}
