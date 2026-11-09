using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Penutup hanya untuk Chapter 1, tanpa mengubah mode eksplorasi.
public sealed class PenutupChapter1Arthur : MonoBehaviour
{
 public enum Fase {MenungguPulang,MengulangRutinitas,Petunjuk,Diam,Pagi,Selesai,Kredit,Glitch,MonologSebelumGame,MonologIstirahat}
 public PerjalananChapelChapter1 chapel;
 public GameObject prefabMengetik;
 public Transform titikDuduk,titikPagi;
 public CanvasGroup layarGelap;
 public GameObject panel;
 public TMP_Text judul,isi,aksi,teksKredit,petunjukDiam,teksPenutup;
 public Button tombol;
 public GameObject ilustrasiPenutup;
 public CanvasGroup gambarArthurPertama,gambarArthurKedua;
 public GameObject grupTeksPenutup;
 [Min(.5f)] public float lamaGambarPertama=3;
 [Min(.1f)] public float durasiPergantianGambar=.6f;
 public AreaInteraksi monologPagi;
 [Min(1)] public float durasiDiam=30;
 [Min(0)] public float jedaMasukKamar=1.5f;
 [Header("Monolog kamar sebelum permainan penutup")]
 [TextArea(1,3)] public string[] monologKamar={
  "Udah berapa lama aku ngerjain?",
  "Entahlah, yang penting selesai.",
  "Tapi apakah ini akan selesai?",
  "Aku gak tahu, yang penting usaha.",
  "Aku mau main ah,",
  "Jangan, tugasnya belum selesai.",
  "Hmnm…."
 };
 [TextArea(1,3)] public string monologIstirahat="Aku harus istirahat, jangan memaksakan diri, ya,";
 [Min(1)] public float tahanMonolog=2f;
 public string sceneMenu="MainMenu";
 [TextArea(8,20)] public string isiKredit="A LITTLE CAMPUS STORY\n\nCHAPTER 1\nArthur Saputra\nKeraguan dan Penerimaan\n\nDibuat dengan Unity\n\nTerima kasih telah bermain.";
 [Min(5)] public float durasiKredit=20;
 public Fase Status {get;private set;}
 public float SisaDiam {get;private set;}
 public int JumlahUlang {get;private set;}
 public bool TahanPratinjauPenutup {get;set;}
 public bool Selesai=>Status==Fase.Selesai||Status==Fase.Kredit;
 GameObject panggung;AdeganRhythmPembukaArthur otomatis;
 bool dimulai,kameraLama,pendengarLama,visualLama,waktuEnabledLama;
 AudioListener pendengar;float abaikanInputSampai;
 Color ambientLama;UnityEngine.Rendering.AmbientMode modeAmbientLama;float refleksiLama;
 readonly List<GameObject> canvasDisembunyikan=new List<GameObject>();
 void Start(){panel.SetActive(false);teksKredit.gameObject.SetActive(false);petunjukDiam.gameObject.SetActive(false);ilustrasiPenutup.SetActive(false);tombol.gameObject.SetActive(false);layarGelap.alpha=0;layarGelap.blocksRaycasts=false;tombol.onClick.AddListener(Aksi);}
 void Update(){
  if(Status==Fase.MenungguPulang&&!dimulai&&chapel.Selesai&&KawasanKampus.Instance!=null&&!KawasanKampus.Instance.SedangMemuat){dimulai=true;StartCoroutine(TungguMasukKamar());}
  if(Status!=Fase.Diam||Time.timeScale<=0)return;
  if(otomatis!=null&&otomatis.Status==AdeganRhythmPembukaArthur.Fase.Selesai)otomatis.MainkanRondePenutup();
  if(Time.unscaledTime<abaikanInputSampai)return;
  ProsesDiam(Time.unscaledDeltaTime,Input.anyKeyDown||Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(1)||Input.touchCount>0);
 }
 public void ProsesDiam(float selang,bool menekan){
  if(Status!=Fase.Diam)return;
  if(menekan){Status=Fase.Glitch;StartCoroutine(Gagal());return;}
  SisaDiam=Mathf.Max(0,SisaDiam-Mathf.Max(0,selang));petunjukDiam.text="Jangan menekan apa pun · "+Mathf.CeilToInt(SisaDiam).ToString("00")+" detik";
  if(SisaDiam<=0){Status=Fase.Pagi;StartCoroutine(Pagi());}
 }
 IEnumerator Redup(bool gelap){layarGelap.blocksRaycasts=true;float awal=layarGelap.alpha;for(float t=0;t<.7f;t+=Time.unscaledDeltaTime){layarGelap.alpha=Mathf.Lerp(awal,gelap?1:0,Mathf.SmoothStep(0,1,t/.7f));yield return null;}layarGelap.alpha=gelap?1:0;layarGelap.blocksRaycasts=gelap;}
 IEnumerator TungguMasukKamar(){
  var k=chapel.kamar;k.pemain.BerhentiBergerak();k.pemain.kunciPergerakan=true;k.procrastination.JedaKhususCerita=true;
  yield return new WaitForSecondsRealtime(jedaMasukKamar);yield return Pembukaan();
 }
 IEnumerator Pembukaan(){
  Status=Fase.MengulangRutinitas;var k=chapel.kamar;k.pemain.BerhentiBergerak();k.pemain.kunciPergerakan=true;k.procrastination.JedaKhususCerita=true;
  yield return Redup(true);yield return KawasanKampus.Instance.Pindahkan(titikDuduk,()=>k.arthur.PindahKe(titikDuduk));
  chapel.petunjukWaktu.gameObject.SetActive(false);k.timer.gameObject.SetActive(false);k.pemain.kunciPergerakan=true;
  foreach(var n in FindObjectsByType<NpcYangBisaDiatur>(FindObjectsInactive.Include,FindObjectsSortMode.None))n.gameObject.SetActive(false);foreach(var n in FindObjectsByType<NpcKampusHidup>(FindObjectsInactive.Include,FindObjectsSortMode.None))n.gameObject.SetActive(false);
  yield return Redup(false);DialogManager.Instance.minigameAktif=true;yield return Redup(true);
  kameraLama=k.arthur.kamera.enabled;pendengar=k.arthur.kamera.GetComponent<AudioListener>();pendengarLama=pendengar!=null&&pendengar.enabled;if(pendengar!=null)pendengar.enabled=false;visualLama=k.arthur.wadahArahVisual.gameObject.activeSelf;k.arthur.wadahArahVisual.gameObject.SetActive(false);k.arthur.kamera.enabled=false;
  foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))if(canvas.transform.root!=transform&&canvas.gameObject.activeSelf){canvasDisembunyikan.Add(canvas.gameObject);canvas.gameObject.SetActive(false);}
  modeAmbientLama=RenderSettings.ambientMode;ambientLama=RenderSettings.ambientLight;refleksiLama=RenderSettings.reflectionIntensity;waktuEnabledLama=chapel.waktu.enabled;chapel.waktu.enabled=false;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.001f,.002f,.003f);RenderSettings.reflectionIntensity=0;
  panggung=Instantiate(prefabMengetik);panggung.transform.position+=Vector3.right*5000;otomatis=panggung.GetComponentInChildren<AdeganRhythmPembukaArthur>(true);otomatis.permainanDikendalikanPemain=false;otomatis.worship=null;if(otomatis.bubbleIstirahat!=null){otomatis.bubbleIstirahat.alpha=0;otomatis.bubbleIstirahat.gameObject.SetActive(false);}otomatis.bubbleIstirahat=null;otomatis.lamaMengetik=float.MaxValue;otomatis.pembukaan.tampilkanTeks=false;otomatis.enabled=true;otomatis.Ulangi();var cam=panggung.GetComponentInChildren<Camera>(true);cam.backgroundColor=Color.black;cam.clearFlags=CameraClearFlags.SolidColor;
  yield return Redup(false);
  Status=Fase.MonologSebelumGame;
  foreach(var kalimat in monologKamar)yield return TampilkanPikiran(kalimat);
  otomatis.pembukaan.tampilkanTeks=false;otomatis.lamaMengetik=0;otomatis.Ulangi();
  yield return TungguGame();MulaiDiam();
 }
 // Gunakan bidang teks dan animasi mengetik lama; posisi/ukuran tetap milik pemilik scene.
 IEnumerator TampilkanPikiran(string kalimat){
  var p=otomatis.pembukaan;p.enabled=true;p.kalimat=kalimat;p.tampilkanTeks=true;
  p.jedaTeks=.35f;p.tahanTeksSelesai=tahanMonolog;p.Ulangi();
  while(p.WaktuAdegan<p.DurasiTeks)yield return null;
  p.tampilkanTeks=false;if(p.teksPikiran!=null)p.teksPikiran.gameObject.SetActive(false);
 }
 IEnumerator TungguGame(){while(otomatis!=null&&otomatis.Status!=AdeganRhythmPembukaArthur.Fase.RondePertama&&otomatis.Status!=AdeganRhythmPembukaArthur.Fase.RondeKedua)yield return null;}
 void MulaiDiam(){Status=Fase.Diam;SisaDiam=durasiDiam;abaikanInputSampai=Time.unscaledTime+.4f;petunjukDiam.gameObject.SetActive(true);petunjukDiam.text="Jangan menekan apa pun · 30 detik";}
 IEnumerator Gagal(){
  JumlahUlang++;petunjukDiam.gameObject.SetActive(false);otomatis.rhythm.TutupDariHP();
  var go=Instantiate(chapel.kamar.procrastination.panelGlitch,layarGelap.transform.parent);go.name="Glitch Anti-Game";go.SetActive(true);var r=go.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;r.SetAsLastSibling();
  var image=go.GetComponent<Image>();if(image!=null){image.sprite=null;image.color=new Color(.01f,.01f,.01f,.95f);}
  var effect=go.GetComponent<EfekGlitchLayar>();if(effect==null)effect=go.AddComponent<EfekGlitchLayar>();var text=go.GetComponentInChildren<TextMeshProUGUI>(true);effect.Mulai(text,chapel.kamar.procrastination.kataGlitch);for(float t=0;t<1.3f;t+=Time.unscaledDeltaTime){effect.Tampilkan(t/1.3f);yield return null;}yield return new WaitForSecondsRealtime(.3f);go.SetActive(false);Destroy(go);
  otomatis.Ulangi();yield return TungguGame();MulaiDiam();
 }
 IEnumerator Pagi(){
  petunjukDiam.gameObject.SetActive(false);
  Status=Fase.MonologIstirahat;
  otomatis.StopAllCoroutines();otomatis.enabled=false;otomatis.rhythm.TutupDariHP();
  otomatis.layarHP.alpha=0;otomatis.layarHP.gameObject.SetActive(false);otomatis.hpPortrait.gameObject.SetActive(false);
  otomatis.pembukaan.enabled=true;
  yield return TampilkanPikiran(monologIstirahat);
  yield return Redup(true);Status=Fase.Pagi;if(panggung!=null){panggung.SetActive(false);Destroy(panggung);panggung=null;}RenderSettings.ambientMode=modeAmbientLama;RenderSettings.ambientLight=ambientLama;RenderSettings.reflectionIntensity=refleksiLama;chapel.waktu.enabled=waktuEnabledLama;chapel.waktu.berjalanOtomatis=false;chapel.waktu.AturWaktu(WaktuKampus.Pagi);
  ilustrasiPenutup.SetActive(true);teksPenutup.text=monologPagi.skenarioDialog[0].teksDialog;if(grupTeksPenutup!=null)grupTeksPenutup.SetActive(true);if(gambarArthurPertama!=null)gambarArthurPertama.alpha=1;if(gambarArthurKedua!=null)gambarArthurKedua.alpha=0;yield return Redup(false);
  yield return new WaitForSecondsRealtime(lamaGambarPertama);
  var rectPertama=gambarArthurPertama!=null?gambarArthurPertama.transform as RectTransform:null;
  Vector2 posisiPertama=rectPertama!=null?rectPertama.anchoredPosition:Vector2.zero;
  for(float t=0;t<durasiPergantianGambar;t+=Time.unscaledDeltaTime){float x=Mathf.SmoothStep(0,1,t/durasiPergantianGambar);if(gambarArthurPertama!=null)gambarArthurPertama.alpha=1-x;if(rectPertama!=null)rectPertama.anchoredPosition=posisiPertama+Vector2.right*(220*x);yield return null;}
  if(gambarArthurPertama!=null)gambarArthurPertama.alpha=0;if(rectPertama!=null)rectPertama.anchoredPosition=posisiPertama;
  if(grupTeksPenutup!=null)grupTeksPenutup.SetActive(false);teksPenutup.text="";
  for(float t=0;t<durasiPergantianGambar;t+=Time.unscaledDeltaTime){if(gambarArthurKedua!=null)gambarArthurKedua.alpha=Mathf.SmoothStep(0,1,t/durasiPergantianGambar);yield return null;}
  if(gambarArthurKedua!=null)gambarArthurKedua.alpha=1;if(grupTeksPenutup!=null)grupTeksPenutup.SetActive(false);
  yield return new WaitForSecondsRealtime(3);while(TahanPratinjauPenutup)yield return null;yield return Redup(true);ilustrasiPenutup.SetActive(false);
  chapel.kamar.buku.Tambah(JenisBukuArthur.Tugas,new CatatanArthur{id="chapter1_penutup",judul="Chapter 1 selesai",isi="Arthur telah menyelesaikan tantangan diam selama 30 detik.",selesai=true,dikendalikanQuest=true});PenghargaanDemoArthur.TandaiSelesai();Status=Fase.Selesai;yield return Kredit();
 }
 IEnumerator Kredit(){Status=Fase.Kredit;panel.SetActive(false);teksKredit.text=isiKredit;teksKredit.gameObject.SetActive(true);tombol.gameObject.SetActive(true);aksi.text="Lewati";layarGelap.blocksRaycasts=false;var r=teksKredit.rectTransform;teksKredit.ForceMeshUpdate();r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,Mathf.Max(r.rect.height,teksKredit.preferredHeight+80));float batas=(r.rect.height+((RectTransform)r.parent).rect.height)*.5f;for(float t=0;t<durasiKredit;t+=Time.unscaledDeltaTime){r.anchoredPosition=new Vector2(0,Mathf.Lerp(-batas,batas,t/durasiKredit));yield return null;}KembaliMenu();}
 public void SiapkanPratinjau(){StopAllCoroutines();enabled=false;TahanPratinjauPenutup=false;Status=Fase.Petunjuk;panel.SetActive(false);petunjukDiam.gameObject.SetActive(false);teksKredit.gameObject.SetActive(false);ilustrasiPenutup.SetActive(true);gambarArthurPertama.alpha=1;gambarArthurKedua.alpha=0;if(grupTeksPenutup!=null)grupTeksPenutup.SetActive(false);layarGelap.alpha=0;layarGelap.blocksRaycasts=false;tombol.gameObject.SetActive(true);aksi.text="Putar penutup";}
 public void Aksi(){if(Status==Fase.Petunjuk){tombol.gameObject.SetActive(false);Status=Fase.Pagi;StartCoroutine(Pagi());}else if(Status==Fase.Kredit)KembaliMenu();}
 void KembaliMenu(){LoadingManager.sceneTujuan=sceneMenu;SceneManager.LoadScene("LoadingScene");}
 void OnDestroy(){if(panggung!=null)Destroy(panggung);}
}
