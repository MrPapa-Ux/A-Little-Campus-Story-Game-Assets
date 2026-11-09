using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Dipasang hanya pada cerita Chapter 1; tidak memodifikasi CharacterLab.
public sealed class PerjalananChapelChapter1 : MonoBehaviour
{
 public static PerjalananChapelChapter1 Instance {get;private set;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
 static void ResetInstance(){Instance=null;}
 void OnEnable(){Instance=this;}
 SistemHP referensiHP;
 public enum Tahap { MenungguDining, MenujuChapel, Ibadah, KeluarChapel, Pulang, Selesai, RefleksiSepi }
 public PerjalananDiningChapter1 dining;
 public CeritaKamar201Arthur kamar;
 public SistemPilihan pilihan;
 public SiklusWaktuKampus waktu;
 public GameObject jemaat;
 public Camera kameraIbadah;
 public AudioSource suasanaIbadah;
 public TMP_Text petunjukWaktu;
 public CanvasGroup teriakan;
 public AreaInteraksi monologMasuk,refleksi;
 public AreaInteraksi khotbah;
 public GameObject arthurDuduk,pengkhotbah,lampuAula;
 public Transform kameraArthur,kameraPengkhotbah,titikSelesaiIbadah;
 public CanvasGroup pergantianAdegan;
 [Min(.5f)] public float lamaSorotArthur=3;
 public Transform titikUlangDining;
 [Min(30)] public float batasPerjalanan=600;
 [Min(30)] public float batasPulang=240;
 [Min(1)] public float lamaTeriakan=4;
 public Tahap Status {get;private set;}
 public float SisaWaktu {get;private set;}
 public bool MalamAktif {get;private set;}
 public bool Selesai=>Status==Tahap.Selesai;
 readonly Dictionary<GameObject,bool> keadaanNpc=new Dictionary<GameObject,bool>();
 struct Hud {public CanvasGroup grup;public float alpha;public bool interaksi,raycast;}
 readonly List<Hud> hud=new List<Hud>();
 WaktuKampus waktuLama;bool otomatisLama;float peralihanLama;bool kameraLama;
 bool coroutineAktif;AudioListener pendengar;bool pendengarLama,visualArthurLama;
 float periksaAktivitasBerikut;
 bool momenPulangDicatat;
 bool tugasPulangDisiapkan;
 bool barTerlihatSebelumMalam;
 [Header("Monolog pulang di depan Jasmine")]
 public Vector3 titikMonologJasmine=new Vector3(49.5f,.44f,2.17f);
 [Min(.5f)] public float radiusMonologJasmine=3f;
 bool monologSendirianDimulai;
 void Start(){referensiHP=FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include);pilihan.PercakapanSelesai+=SetelahDialog;jemaat.SetActive(false);if(arthurDuduk!=null)arthurDuduk.SetActive(false);if(pengkhotbah!=null)pengkhotbah.SetActive(false);if(lampuAula!=null)lampuAula.SetActive(false);if(pergantianAdegan!=null)pergantianAdegan.alpha=0;kameraIbadah.gameObject.SetActive(false);teriakan.alpha=0;petunjukWaktu.gameObject.SetActive(false);}
 void Update(){
  if(Status==Tahap.MenungguDining){if(dining!=null&&dining.Selesai)MulaiPerjalanan();return;}
  string r=kamar.arthur.JalurAktif!=null?kamar.arthur.JalurAktif.name:"";
  if(MalamAktif){if(Time.unscaledTime>=periksaAktivitasBerikut){SembunyikanAktivitasLuar();periksaAktivitasBerikut=Time.unscaledTime+.75f;}foreach(var p in keadaanNpc)if(p.Key!=null&&p.Key.activeSelf)p.Key.SetActive(false);}
  bool diChapel=r.StartsWith("Pioneer_");
  bool aulaRamai=diChapel&&MalamAktif&&Status!=Tahap.RefleksiSepi&&Status!=Tahap.KeluarChapel&&Status!=Tahap.Pulang;
  if(jemaat!=null)jemaat.SetActive(aulaRamai);
  if(lampuAula!=null)lampuAula.SetActive(diChapel&&MalamAktif);
  if(pengkhotbah!=null)pengkhotbah.SetActive(aulaRamai);
  if(suasanaIbadah!=null){if(aulaRamai){if(!suasanaIbadah.isPlaying)suasanaIbadah.Play();}else if(suasanaIbadah.isPlaying)suasanaIbadah.Stop();}
  if(Status==Tahap.MenujuChapel){
   var d=DialogManager.Instance;var hp=referensiHP;
   bool jeda=(d!=null&&(d.PercakapanAktif||d.minigameAktif))||(hp!=null&&hp.hpSedangBuka)||(KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat);
   if(!jeda)SisaWaktu=Mathf.Max(0,SisaWaktu-Time.deltaTime);
   petunjukWaktu.gameObject.SetActive(d==null||!d.PercakapanAktif);petunjukWaktu.text="Menuju Chapel  "+Mathf.FloorToInt(SisaWaktu/60).ToString("00")+":"+Mathf.FloorToInt(SisaWaktu%60).ToString("00");
   if(r=="Pioneer_LorongTengah"&&!jeda){MulaiIbadah();return;}
   if(SisaWaktu<=0&&!coroutineAktif)StartCoroutine(UlangPerjalanan());
  }
  if(Status==Tahap.KeluarChapel||Status==Tahap.Pulang){
   if(!tugasPulangDisiapkan)SiapkanTugasPulang();
   var d=DialogManager.Instance;var hp=referensiHP;
   bool jeda=(d!=null&&(d.PercakapanAktif||d.minigameAktif))||(hp!=null&&hp.hpSedangBuka)||(KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat)||r=="GH_Kamar201";
   if(!jeda&&!coroutineAktif)SisaWaktu=Mathf.Max(0,SisaWaktu-Time.deltaTime);
   petunjukWaktu.gameObject.SetActive(!jeda);
   int detik=Mathf.CeilToInt(SisaWaktu);petunjukWaktu.text="Waktu pulang  "+(detik/60).ToString("00")+":"+(detik%60).ToString("00");
   if(SisaWaktu<=0&&!jeda&&!coroutineAktif){coroutineAktif=true;kamar.procrastination.GagalKarenaBatasWaktuQuest();}
  }
  if(Status==Tahap.KeluarChapel&&!diChapel&&KawasanKampus.Instance!=null&&!KawasanKampus.Instance.SedangMemuat)Status=Tahap.Pulang;
  if(Status==Tahap.Pulang&&MalamAktif&&!monologSendirianDimulai&&kamar.arthur.JalurAktif==null&&KawasanKampus.Instance!=null&&KawasanKampus.Instance.KawasanAktif=="JasmineGK2"&&!KawasanKampus.Instance.SedangMemuat){var p=kamar.arthur.transform.position;p.y=titikMonologJasmine.y;var hp=referensiHP;if(Vector3.Distance(p,titikMonologJasmine)<=radiusMonologJasmine&&!DialogManager.Instance.PercakapanAktif&&!DialogManager.Instance.minigameAktif&&(hp==null||!hp.hpSedangBuka)){monologSendirianDimulai=true;StartCoroutine(MonologSendirian());}}
  if(Status==Tahap.Pulang&&r=="GH_Kamar201"){Status=Tahap.Selesai;petunjukWaktu.gameObject.SetActive(false);Catat("chapter1_pulang","Kembali ke Asrama Guest House",true,"Arthur sudah kembali ke kamar 201.");PulihkanMalam();}
 }
 IEnumerator MonologSendirian(){
  var d=DialogManager.Instance;bool jeda=kamar.procrastination.JedaKhususCerita;kamar.procrastination.JedaKhususCerita=true;
  kamar.pemain.BerhentiBergerak();kamar.pemain.kunciPergerakan=true;
  var go=new GameObject("Monolog Arthur — Aku sendirian");go.transform.SetParent(transform);var area=go.AddComponent<AreaInteraksi>();area.idTempat="chapter1_aku_sendirian";area.tampilkanTombolKeluar=true;area.daftarOpsi=System.Array.Empty<OpsiInteraksi>();area.skenarioDialog=new[]{new BarisDialog{siapaYangBicara=Pembicara.Kiri_Arthur,namaKarakter="Arthur",teksDialog="Aku sendirian",teksTombolLanjut="",ubahEkspresi=true,ekspresi=EmosiKarakter.Sedih}};
  d.TawarkanInteraksi(area);pilihan.AksiTombolInteraksi();yield return null;
  while(d.PercakapanAktif)yield return null;
  foreach(var wajah in kamar.arthur.GetComponentsInChildren<EkspresiWajah2D>(true)){wajah.gunakanEkspresi=true;wajah.ekspresi=EmosiKarakter.Sedih;}
  d.HapusInteraksi(area);Destroy(go);kamar.procrastination.JedaKhususCerita=jeda;kamar.pemain.kunciPergerakan=false;
 }
 public void MulaiPerjalanan(){
  if(Status!=Tahap.MenungguDining)return;
  Status=Tahap.MenujuChapel;SisaWaktu=batasPerjalanan;AktifkanMalam();
  if(kamar.procrastination!=null){kamar.procrastination.PulihkanBar();kamar.procrastination.tanpaBatasWaktu=false;}
  Catat("chapter1_chapel","Pergi ke Pioneer Chapel",false,"Ikuti ibadah Rabu malam di Pioneer Chapel. Masuk ke lorong tengah sebelum batas waktu 10 menit berakhir.");
 }
 void AktifkanMalam(){
  if(MalamAktif)return;MalamAktif=true;
  if(kamar.procrastination!=null&&kamar.procrastination.lazyBarUtama!=null){var bar=kamar.procrastination.lazyBarUtama.gameObject;barTerlihatSebelumMalam=bar.activeSelf;bar.SetActive(false);}
  waktuLama=waktu.waktu;otomatisLama=waktu.berjalanOtomatis;peralihanLama=waktu.durasiPeralihan;
  waktu.berjalanOtomatis=false;waktu.durasiPeralihan=3;waktu.AturWaktu(WaktuKampus.Malam);
  SembunyikanAktivitasLuar();
 }
 void SembunyikanAktivitasLuar(){
  foreach(var n in FindObjectsByType<NpcKampusHidup>(FindObjectsInactive.Include,FindObjectsSortMode.None))SimpanNpc(n.gameObject);
  foreach(var n in FindObjectsByType<NpcYangBisaDiatur>(FindObjectsInactive.Include,FindObjectsSortMode.None))SimpanNpc(n.gameObject);
  foreach(var n in FindObjectsByType<PelariSekilasNpc>(FindObjectsInactive.Include,FindObjectsSortMode.None))SimpanNpc(n.gameObject);
  foreach(var n in FindObjectsByType<NPCBubbleTrigger>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(n.GetComponentInParent<ArthurJalurKampus>()==null)SimpanNpc(n.gameObject);
  foreach(var n in FindObjectsByType<MobilKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None))SimpanNpc(n.gameObject);
 }
 void SimpanNpc(GameObject n){if(!keadaanNpc.ContainsKey(n))keadaanNpc.Add(n,n.activeSelf);n.SetActive(false);}
 void PulihkanMalam(){
  if(!MalamAktif)return;MalamAktif=false;
  if(kamar.procrastination!=null&&kamar.procrastination.lazyBarUtama!=null)kamar.procrastination.lazyBarUtama.gameObject.SetActive(barTerlihatSebelumMalam);
  foreach(var p in keadaanNpc)if(p.Key!=null)p.Key.SetActive(p.Value);keadaanNpc.Clear();
  jemaat.SetActive(false);if(suasanaIbadah!=null)suasanaIbadah.Stop();
  if(lampuAula!=null)lampuAula.SetActive(false);if(arthurDuduk!=null)arthurDuduk.SetActive(false);if(pengkhotbah!=null)pengkhotbah.SetActive(false);
  waktu.durasiPeralihan=peralihanLama;waktu.berjalanOtomatis=otomatisLama;waktu.AturWaktu(waktuLama);
 }
 public void SiapkanTugasPulang(){
  if(!tugasPulangDisiapkan)SisaWaktu=batasPulang;
  tugasPulangDisiapkan=true;
  Catat("chapter1_chapel","Pergi ke Pioneer Chapel",true,"Arthur telah mengikuti ibadah Rabu malam.");
  if(!kamar.buku.tugas.Exists(t=>t!=null&&t.id=="chapter1_pulang"))
   Catat("chapter1_pulang","Kembali ke Asrama Guest House",false,"Ibadah Rabu malam telah selesai. Kembali ke kamar 201 Asrama Guest House dalam waktu 4 menit. Keluar dari Pioneer Chapel, ikuti jalan menuju GA, lalu kembali melalui depan Asrama Jasmine.");
 }
 void Catat(string id,string judul,bool selesai,string isi){kamar.buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{id=id,judul=judul,selesai=selesai,isi=isi,dikendalikanQuest=true});}
 public void MulaiIbadah(){
  if(Status!=Tahap.MenujuChapel)return;Status=Tahap.Ibadah;if(kamar.procrastination!=null)kamar.procrastination.JedaKhususCerita=true;petunjukWaktu.gameObject.SetActive(false);
  kamar.pemain.BerhentiBergerak();kamar.pemain.kunciPergerakan=true;
  var d=DialogManager.Instance;if(d.hudSaatPercakapan!=null)foreach(var g in d.hudSaatPercakapan)if(g!=null){hud.Add(new Hud{grup=g,alpha=g.alpha,interaksi=g.interactable,raycast=g.blocksRaycasts});g.alpha=0;g.interactable=false;g.blocksRaycasts=false;}
  kameraLama=kamar.arthur.kamera.enabled;pendengar=kamar.arthur.kamera.GetComponent<AudioListener>();pendengarLama=pendengar!=null&&pendengar.enabled;if(pendengar!=null)pendengar.enabled=false;
  visualArthurLama=kamar.arthur.wadahArahVisual.gameObject.activeSelf;kamar.arthur.wadahArahVisual.gameObject.SetActive(false);
  if(arthurDuduk!=null)arthurDuduk.SetActive(true);
  kamar.arthur.kamera.enabled=false;kameraIbadah.gameObject.SetActive(true);StartCoroutine(PembukaanKhotbah());
 }
 void Bidik(Transform titik){if(titik==null)return;kameraIbadah.transform.SetPositionAndRotation(titik.position,titik.rotation);kameraIbadah.fieldOfView=50;}
 IEnumerator Redup(bool gelap){if(pergantianAdegan==null)yield break;float t=0;while(t<.55f){t+=Time.unscaledDeltaTime;float x=Mathf.SmoothStep(0,1,t/.55f);pergantianAdegan.alpha=gelap?x:1-x;yield return null;}pergantianAdegan.alpha=gelap?1:0;}
 IEnumerator PembukaanKhotbah(){DialogManager.Instance.minigameAktif=true;Bidik(kameraPengkhotbah);yield return BukaSetelahJeda(khotbah,.5f);}
 IEnumerator BukaSetelahJeda(AreaInteraksi area,float jeda){yield return new WaitForSecondsRealtime(jeda);var d=DialogManager.Instance;d.minigameAktif=false;d.TawarkanInteraksi(area);pilihan.AksiTombolInteraksi();}
 void SetelahDialog(AreaInteraksi area,bool tuntas){
  if(!tuntas)return;
  if(Status==Tahap.Ibadah&&area==khotbah&&!coroutineAktif)StartCoroutine(TeriakanDanRefleksi());
  else if(Status==Tahap.RefleksiSepi&&area==monologMasuk)StartCoroutine(BukaSetelahJeda(refleksi,.4f));
  else if(Status==Tahap.RefleksiSepi&&area==refleksi){PulihkanKamera();Status=Tahap.KeluarChapel;if(kamar.procrastination!=null)kamar.procrastination.JedaKhususCerita=false;SiapkanTugasPulang();
   if(!momenPulangDicatat){momenPulangDicatat=true;kamar.notifikasiMomen?.Tampilkan(4);}}
 }
 IEnumerator TeriakanDanRefleksi(){
  coroutineAktif=true;DialogManager.Instance.minigameAktif=true;kamar.pemain.kunciPergerakan=true;
  var skala=teriakan.transform.localScale;float t=0;
  while(t<lamaTeriakan){t+=Time.unscaledDeltaTime;teriakan.alpha=Mathf.SmoothStep(0,1,Mathf.Min(t/.25f,(lamaTeriakan-t)/.35f));teriakan.transform.localScale=skala*(1+Mathf.Sin(t*2.2f)*.006f);yield return null;}
  teriakan.alpha=0;teriakan.transform.localScale=skala;
  yield return Redup(true);Status=Tahap.RefleksiSepi;jemaat.SetActive(false);pengkhotbah.SetActive(false);suasanaIbadah.Stop();Bidik(kameraArthur);yield return new WaitForSecondsRealtime(.5f);yield return Redup(false);coroutineAktif=false;
  yield return BukaSetelahJeda(monologMasuk,.4f);
 }
 IEnumerator UlangPerjalanan(){
  coroutineAktif=true;kamar.pemain.kunciPergerakan=true;
  if(pilihan.efekTransisi!=null){bool tiba=false;pilihan.efekTransisi.MulaiTransisiTeleportRutin(()=>KawasanKampus.Instance.Pindahkan(titikUlangDining,()=>{kamar.arthur.PindahKe(titikUlangDining);tiba=true;}));while(!tiba){if(!string.IsNullOrEmpty(KawasanKampus.Instance.KesalahanTerakhir)){coroutineAktif=false;yield break;}yield return null;}}
  else yield return KawasanKampus.Instance.Pindahkan(titikUlangDining,()=>kamar.arthur.PindahKe(titikUlangDining));
  SisaWaktu=batasPerjalanan;kamar.pemain.kunciPergerakan=false;coroutineAktif=false;
 }
 public bool PulihkanSetelahGlitch(){
  if(Status!=Tahap.MenujuChapel&&Status!=Tahap.Pulang&&Status!=Tahap.KeluarChapel)return false;
  StartCoroutine(PulihGlitch());return true;
 }
 IEnumerator PulihGlitch(){
  bool pulang=Status!=Tahap.MenujuChapel;var tujuan=pulang?titikSelesaiIbadah:titikUlangDining;
  yield return KawasanKampus.Instance.Pindahkan(tujuan,()=>kamar.arthur.PindahKe(tujuan));
  Status=pulang?Tahap.KeluarChapel:Tahap.MenujuChapel;SisaWaktu=pulang?batasPulang:batasPerjalanan;coroutineAktif=false;
  kamar.procrastination.PulihkanBar();kamar.procrastination.tanpaBatasWaktu=false;kamar.pemain.kunciPergerakan=false;
 }
 void PulihkanKamera(){kameraIbadah.gameObject.SetActive(false);if(titikSelesaiIbadah!=null)kamar.arthur.PindahKe(titikSelesaiIbadah);kamar.arthur.wadahArahVisual.gameObject.SetActive(visualArthurLama);if(arthurDuduk!=null)arthurDuduk.SetActive(false);kamar.arthur.kamera.enabled=kameraLama;if(pendengar!=null)pendengar.enabled=pendengarLama;kamar.pemain.kunciPergerakan=false;foreach(var h in hud)if(h.grup!=null){h.grup.alpha=h.alpha;h.grup.interactable=h.interaksi;h.grup.blocksRaycasts=h.raycast;}hud.Clear();}
 void OnDisable(){StopAllCoroutines();if(Status==Tahap.Ibadah||Status==Tahap.RefleksiSepi){teriakan.alpha=0;if(pergantianAdegan!=null)pergantianAdegan.alpha=0;PulihkanKamera();if(DialogManager.Instance!=null)DialogManager.Instance.minigameAktif=false;}if(MalamAktif)PulihkanMalam();}
 void OnDestroy(){if(Instance==this)Instance=null;if(pilihan!=null)pilihan.PercakapanSelesai-=SetelahDialog;}
}
