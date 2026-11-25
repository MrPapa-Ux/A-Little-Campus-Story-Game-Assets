using UnityEngine;

// Satu pengatur per pasangan, bukan satu timer untuk setiap anggota kerumunan.
public sealed class EmoteBergantianNpc : MonoBehaviour
{
    public NPCBubbleTrigger pertama,kedua;
    public Sprite[] emote;
    public float jedaAwal=4;
    [Min(1)] public float jedaAntarEmote=3.8f;
    [Min(.2f)] public float durasiTampil=2.4f;
    public bool satuUntukKawasan;
    static float akhirEmoteKawasan;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetJeda(){akhirEmoteKawasan=0;}
    float berikut;int giliran;ArthurJalurKampus arthur;
    void OnEnable(){berikut=Time.time+jedaAwal;}
    void Update()
    {
        if(Time.time<berikut)return;
        berikut=Time.time+jedaAntarEmote;
        if(arthur==null)arthur=FindFirstObjectByType<ArthurJalurKampus>();
        if(arthur==null||(arthur.transform.position-transform.position).sqrMagnitude>14*14)return;
        if(satuUntukKawasan){
            var npc=GetComponent<NpcYangBisaDiatur>();
            if(npc!=null&&npc.jalurInteraksi!=arthur.JalurAktif)return;
            if(Time.time<akhirEmoteKawasan)return;
        }
        if(DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif))return;
        var bubble=(giliran++%2==0)?pertama:kedua;
        if(bubble!=null&&emote!=null&&emote.Length>0){
            bubble.MulaiAnimasiBubbleKhusus(emote[(giliran/2)%emote.Length],durasiTampil);
            if(satuUntukKawasan)akhirEmoteKawasan=Time.time+durasiTampil+2f;
        }
    }
}
