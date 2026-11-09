using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

// Hanya dipasang pada scene cerita. Scenery Dining tetap memakai kawasan yang sedang dimuat.
public sealed class PerjalananDiningChapter1 : MonoBehaviour
{
 public PerjalananDandiChapter1 perjalananDandi;
 public CeritaKamar201Arthur kamar;
 public DialogManager dialog;
 public GameObject prefabAdegan;
 public string idQuest="chapter1_dining";
 public bool QuestAktif{get;private set;}
 public bool AdeganAktif{get;private set;}
 public bool Selesai{get;private set;}
 GameObject instans;TapTapMekarArthur mekar;
 bool kunciLama,visualLama,kameraLama,barLama,dialogLama,pendengarLama;
 AudioListener pendengar;
 readonly List<GameObject> canvasAktif=new List<GameObject>();
 readonly List<Light> cahayaAktif=new List<Light>();
 readonly List<Renderer> npcAktif=new List<Renderer>();
 void Update(){
  if(Selesai||AdeganAktif||perjalananDandi==null||!perjalananDandi.Selesai)return;
  if(!QuestAktif){QuestAktif=true;Catat(false);if(kamar.procrastination!=null){kamar.procrastination.PulihkanBar();kamar.procrastination.tanpaBatasWaktu=false;}}
  if(kamar.arthur.JalurAktif!=null&&kamar.arthur.JalurAktif.name=="UNK_Dining_Dalam"&&KawasanKampus.Instance!=null&&KawasanKampus.Instance.Siap&&!KawasanKampus.Instance.SedangMemuat&&dialog!=null&&!dialog.minigameAktif&&!dialog.PercakapanAktif)MulaiAdegan();
 }
 void Catat(bool selesai){kamar.buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{id=idQuest,judul="Pergi ke Dining",dikendalikanQuest=true,selesai=selesai,isi=selesai?"Arthur telah menyelesaikan peristiwa di Dining.":"Setelah berbicara dengan Dandi, pergi ke Dining melalui taman UNKLAB. Masuk melalui pintu Dining."});}
 public void MulaiAdegan(){
  if(AdeganAktif||Selesai||prefabAdegan==null||kamar==null)return;
  AdeganAktif=true;var pemain=kamar.pemain;var arthur=kamar.arthur;
  if(MusikArthur.Instance!=null)MusikArthur.Instance.HentikanSemua();
  kunciLama=pemain.kunciPergerakan;visualLama=arthur.wadahArahVisual.gameObject.activeSelf;kameraLama=arthur.kamera.enabled;barLama=kamar.procrastination!=null&&kamar.procrastination.tanpaBatasWaktu;dialogLama=dialog!=null&&dialog.minigameAktif;
  pendengar=arthur.kamera.GetComponent<AudioListener>();pendengarLama=pendengar!=null&&pendengar.enabled;if(pendengar!=null)pendengar.enabled=false;
  pemain.BerhentiBergerak();pemain.kunciPergerakan=true;arthur.wadahArahVisual.gameObject.SetActive(false);arthur.kamera.enabled=false;if(kamar.procrastination!=null)kamar.procrastination.tanpaBatasWaktu=true;if(dialog!=null)dialog.minigameAktif=true;
  var scene=gameObject.scene;
  // Snapshot visibility before disabling parents. activeSelf also includes hidden loading canvases.
  var terlihat=scene.GetRootGameObjects().SelectMany(root=>root.GetComponentsInChildren<Canvas>(true)).Where(c=>c.gameObject.activeInHierarchy).Select(c=>c.gameObject).Distinct().ToArray();
  foreach(var c in terlihat){canvasAktif.Add(c);c.SetActive(false);}
  foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l.enabled){cahayaAktif.Add(l);l.enabled=false;}
  foreach(var root in scene.GetRootGameObjects())foreach(var n in root.GetComponentsInChildren<NpcKampusHidup>(true))foreach(var r in n.GetComponentsInChildren<Renderer>(true))if(r.enabled){npcAktif.Add(r);r.enabled=false;}
  var input=EventSystem.current;
  instans=Instantiate(prefabAdegan);SceneManager.MoveGameObjectToScene(instans,scene);
  if(input!=null){foreach(var es in instans.GetComponentsInChildren<EventSystem>(true))es.gameObject.SetActive(false);EventSystem.current=input;}
  mekar=instans.GetComponent<TapTapMekarArthur>();
  if(mekar==null){Pulihkan();Debug.LogError("Adegan Dining tidak mempunyai Tap-Tap Mekar.",this);return;}
  mekar.saatSelesai.AddListener(Tuntas);
 }
 void Tuntas(){Selesai=true;QuestAktif=false;Catat(true);Pulihkan();if(kamar!=null&&kamar.procrastination!=null)kamar.procrastination.tanpaBatasWaktu=true;}
 void Pulihkan(){
  if(mekar!=null)mekar.saatSelesai.RemoveListener(Tuntas);
  if(instans!=null){instans.SetActive(false);Destroy(instans);instans=null;}
  foreach(var c in canvasAktif)if(c!=null)c.SetActive(true);canvasAktif.Clear();foreach(var l in cahayaAktif)if(l!=null)l.enabled=true;cahayaAktif.Clear();foreach(var r in npcAktif)if(r!=null)r.enabled=true;npcAktif.Clear();
  if(kamar!=null){if(kamar.pemain!=null)kamar.pemain.kunciPergerakan=kunciLama;if(kamar.arthur!=null){kamar.arthur.wadahArahVisual.gameObject.SetActive(visualLama);kamar.arthur.kamera.enabled=kameraLama;}if(kamar.procrastination!=null)kamar.procrastination.tanpaBatasWaktu=barLama;}
  if(pendengar!=null)pendengar.enabled=pendengarLama;if(dialog!=null)dialog.minigameAktif=dialogLama;AdeganAktif=false;
 }
 void OnDisable(){if(AdeganAktif)Pulihkan();}
}
