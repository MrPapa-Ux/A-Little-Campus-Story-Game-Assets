using UnityEngine;

// Satu pengatur per pasangan, bukan satu timer untuk setiap anggota kerumunan.
public sealed class EmoteBergantianNpc : MonoBehaviour
{
    public NPCBubbleTrigger pertama,kedua;
    public Sprite[] emote;
    public float jedaAwal=4;
    float berikut;int giliran;ArthurJalurKampus arthur;
    void OnEnable(){berikut=Time.time+jedaAwal;}
    void Update()
    {
        if(Time.time<berikut)return;
        berikut=Time.time+3.8f;
        if(arthur==null)arthur=FindFirstObjectByType<ArthurJalurKampus>();
        if(arthur==null||(arthur.transform.position-transform.position).sqrMagnitude>14*14)return;
        if(DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif))return;
        var bubble=(giliran++%2==0)?pertama:kedua;
        if(bubble!=null&&emote!=null&&emote.Length>0)bubble.MulaiAnimasiBubbleKhusus(emote[(giliran/2)%emote.Length],2.4f);
    }
}
