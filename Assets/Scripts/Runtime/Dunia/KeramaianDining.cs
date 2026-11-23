using UnityEngine;
// Figuran hanya aktif di interior Dining; alur cerita dan NPC kawasan lain tetap terpisah.
public sealed class KeramaianDining : MonoBehaviour
{
 public JalurKampus jalur;
 public GameObject isi;
 [Header("Reaksi suasana Dining")]
 public bool reaksiOtomatis=true;
 public Sprite[] emoteReaksi;
 [Min(1)]public float jarakReaksi=24;
 [Min(.5f)]public float jedaReaksiMinimum=1.4f,jedaReaksiMaksimum=2.8f;
 [Min(.5f)]public float durasiReaksi=2.4f;
 NPCBubbleTrigger[] peserta;
 float[] bolehLagi,tampilSampai;
 float berikutReaksi;
 int terakhir=-1;
 public int JumlahReaksi {get;private set;}
 public NPCBubbleTrigger ReaksiTerakhir {get;private set;}
 ArthurJalurKampus arthur;
 void Start(){arthur=FindFirstObjectByType<ArthurJalurKampus>();SiapkanReaksi();}
 void SiapkanReaksi(){
  peserta=isi!=null?isi.GetComponentsInChildren<NPCBubbleTrigger>(true):System.Array.Empty<NPCBubbleTrigger>();
  bolehLagi=new float[peserta.Length];tampilSampai=new float[peserta.Length];
  for(int i=0;i<peserta.Length;i++)bolehLagi[i]=Time.time+Random.Range(1f,5f);
  berikutReaksi=Time.time+1.2f;
 }
 void Update(){
  if(arthur==null||isi==null)return;
  bool aktif=arthur.JalurAktif==jalur&&!(PerjalananChapelChapter1.Instance!=null&&PerjalananChapelChapter1.Instance.MalamAktif);
  if(isi.activeSelf!=aktif)isi.SetActive(aktif);
  if(!aktif||!reaksiOtomatis||Time.time<berikutReaksi)return;
  berikutReaksi=Time.time+Random.Range(Mathf.Max(.5f,jedaReaksiMinimum),Mathf.Max(jedaReaksiMinimum,jedaReaksiMaksimum));
  if(JedaSaatTidakAktif.Dijeda||(KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat))return;
  var dialog=DialogManager.Instance;
  if(dialog!=null&&(dialog.PercakapanAktif||dialog.minigameAktif))return;
  var kamera=arthur.kamera;
  if(kamera==null||!kamera.isActiveAndEnabled||emoteReaksi==null||emoteReaksi.Length==0||peserta.Length==0)return;
  int ramai=0;for(int i=0;i<tampilSampai.Length;i++)if(tampilSampai[i]>Time.time&&peserta[i]!=null&&peserta[i].isActiveAndEnabled)ramai++;
  if(ramai>=2)return;
  int awal=Random.Range(0,peserta.Length);
  for(int j=0;j<peserta.Length;j++){
   int i=(awal+j)%peserta.Length;var npc=peserta[i];
   if(i==terakhir||npc==null||!npc.isActiveAndEnabled||bolehLagi[i]>Time.time||npc.rendererEmote==null)continue;
   if((npc.transform.position-arthur.transform.position).sqrMagnitude>jarakReaksi*jarakReaksi)continue;
   var posisi=npc.kepalaNpc!=null?npc.kepalaNpc.bounds.center:npc.transform.position+Vector3.up;
   var layar=kamera.WorldToViewportPoint(posisi);
   if(layar.z<=0||layar.x<.03f||layar.x>.97f||layar.y<.03f||layar.y>.92f)continue;
   var emote=emoteReaksi[Random.Range(0,emoteReaksi.Length)];if(emote==null)continue;
   npc.MulaiAnimasiBubbleKhusus(emote,durasiReaksi);
   bolehLagi[i]=Time.time+Random.Range(7f,14f);tampilSampai[i]=Time.time+durasiReaksi+npc.durasiAnimasi;
   terakhir=i;ReaksiTerakhir=npc;JumlahReaksi++;break;
  }
 }
}
