using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

// Pemeriksaan perjalanan nyata GA: opsi, perpindahan, lantai, batas dan kamera render.
[InitializeOnLoad]
public static class CampusRevisi13Checks
{
 const string Dir="work/revisi-13-20261026/",Key="Campus.Revisi13.Check";
 static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();static double wait;
 static KarakterController actor;static ArthurJalurKampus mover;static SistemPilihan choices;
 static CampusRevisi13Checks(){EditorApplication.update+=Tick;}
 static Transform GA(string n)=>GameObject.Find("RuteGA_CharacterLab/"+n).transform;
 static Transform Old(string n)=>GameObject.Find("RuteKampus_CharacterLab/"+n).transform;
 static void Log(string s)=>File.AppendAllText(Dir+"pengujian.txt",s+"\n");
 static void Check(bool ok,string s){if(!ok)throw new Exception(s);Log("PASS: "+s);}
 static void Tick(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating||AssetDatabase.IsAssetImportWorkerProcess())return;
  try{
   if(File.Exists(Dir+"uji.request")){
    if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().isDirty||SceneManager.GetActiveScene().path!="Assets/Scenes/CharacterLab.unity")throw new Exception("Perlu CharacterLab tersimpan");
    var request=File.ReadAllText(Dir+"uji.request");SessionState.SetBool(Key+".phone",request.Contains("ponsel"));SessionState.SetBool(Key+".quick",request.Contains("cepat"));SessionState.SetBool(Key+".camera",request.Contains("kamera"));File.Delete(Dir+"uji.request");SessionState.SetBool(Key+".mute",EditorUtility.audioMasterMute);SessionState.SetBool(Key+".background",Application.runInBackground);SessionState.SetBool(Key+".done",false);
    EditorUtility.audioMasterMute=true;File.WriteAllText(Dir+"pengujian.txt","Pengujian perjalanan GA\n");File.WriteAllText(Dir+"running.flag","running");EditorApplication.isPlaying=true;return;
   }
   if(!File.Exists(Dir+"running.flag"))return;
   if(!EditorApplication.isPlaying){
    if(EditorApplication.isPlayingOrWillChangePlaymode)return;
    EditorUtility.audioMasterMute=SessionState.GetBool(Key+".mute",false);Application.runInBackground=SessionState.GetBool(Key+".background",false);JedaSaatTidakAktif.PengujianLatarBelakang=false;
    File.Delete(Dir+"running.flag");stack.Clear();Log("SELESAI: Play mati, pengaturan latar belakang dan mute dipulihkan.");return;
   }
   JedaSaatTidakAktif.PengujianLatarBelakang=true;Application.runInBackground=true;EditorApplication.isPaused=false;
   var pause=UnityEngine.Object.FindFirstObjectByType<JedaSaatTidakAktif>();if(pause!=null)pause.AturJeda(false);Time.timeScale=1;
   if(SessionState.GetBool(Key+".done",false)||Time.timeSinceLevelLoad<.8f)return;
   if(stack.Count==0){stack.Push(Run());wait=0;}
   if(EditorApplication.timeSinceStartup<wait)return;
   var e=stack.Peek();if(!e.MoveNext()){stack.Pop();if(stack.Count==0){SessionState.SetBool(Key+".done",true);EditorApplication.isPlaying=false;}return;}
   if(e.Current is IEnumerator nested)stack.Push(nested);else if(e.Current is float delay)wait=EditorApplication.timeSinceStartup+delay;
  }catch(Exception e){Log("GAGAL: "+e);SessionState.SetBool(Key+".done",true);stack.Clear();if(EditorApplication.isPlaying)EditorApplication.isPlaying=false;}
 }
 static IEnumerator Until(Func<bool> yes,string message,float seconds=4){float end=Time.realtimeSinceStartup+seconds;while(!yes()){if(Time.realtimeSinceStartup>end)throw new Exception("Batas tunggu: "+message);yield return null;}}
 static void Place(Vector3 p,JalurKampus r,bool jasmine=false){
  actor.BerhentiBergerak();var t=new GameObject("TujuanUjiGA");t.transform.position=p;t.AddComponent<TujuanJalurKampus>().jalur=r;
  if(choices.grupFisikDepan!=null)choices.grupFisikDepan.SetActive(jasmine);if(choices.grupFisikBelakang!=null)choices.grupFisikBelakang.SetActive(false);if(choices.grupFisikRuangan!=null)choices.grupFisikRuangan.SetActive(false);
  mover.PindahKe(t.transform);UnityEngine.Object.Destroy(t);
 }
 static void Place(JalurKampus r,float d){r.Sampel(d,out var p,out _,out _);Place(p,r);}
 static IEnumerator Open(string id){yield return Until(()=>DialogManager.Instance.idInteraksiAktif==id,"Bubble "+id);choices.AksiTombolInteraksi();yield return .4f;Check(DialogManager.Instance.PercakapanAktif,"Dialog "+id+" terbuka lewat tombol interaksi");}
 static IEnumerator Choose(int index,string target){choices.TekanPilihanLangsung(index);yield return 1.4f;Check(!DialogManager.Instance.PercakapanAktif&&!choices.efekTransisi.SedangBerjalan,"Transisi selesai");Check(target==null?mover.JalurAktif==null:mover.JalurAktif!=null&&mover.JalurAktif.name==target,"Tujuan perpindahan "+(target??"Jasmine"));}
 static IEnumerator Walk(float input,float distance,float speed=8,bool run=false){
  actor.kecepatanJalan=speed;actor.BerhentiBergerak();
  if(run){if(input>0)actor.JalanKeKanan();else actor.JalanKeKiri();yield return .06f;if(input>0)actor.LepasKanan();else actor.LepasKiri();yield return .06f;}
  float end=Time.realtimeSinceStartup+Mathf.Abs(mover.JarakDiJalur-distance)/(speed*(run?1:.5f))+8;
  while(input>0?mover.JarakDiJalur<distance-.07f:mover.JarakDiJalur>distance+.07f){if(Time.realtimeSinceStartup>end)throw new Exception("Gerak tertahan: "+mover.JalurAktif.name+" d="+mover.JarakDiJalur+" p="+actor.transform.position+" lock="+actor.kunciPergerakan+" actual="+mover.KecepatanAktual+" running="+actor.SedangBerlari+" yaw="+mover.kamera.transform.eulerAngles+" trans="+choices.efekTransisi.SedangBerjalan);if(input>0)actor.JalanKeKanan();else actor.JalanKeKiri();yield return null;}
  actor.BerhentiBergerak();yield return .4f;mover.JalurAktif.Sampel(mover.JarakDiJalur,out var p,out _,out _);Check(Vector3.Distance(p,actor.transform.position)<.18f,"Arthur tetap pada pijakan "+mover.JalurAktif.name);
 }
 static IEnumerator Photo(string name){
  var cam=mover.kamera;var rt=RenderTexture.GetTemporary(1280,720,24);var old=RenderTexture.active;var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);var prev=cam.targetTexture;float aspect=cam.aspect;
  try{cam.aspect=1280f/720;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(Dir+name+".png",texture.EncodeToPNG());}
  finally{cam.targetTexture=prev;cam.aspect=aspect;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(texture);}
  var head=actor.GetComponent<KetukKepalaArthur>().kepala;var screen=cam.WorldToViewportPoint(head.bounds.center);Check(screen.z>0&&screen.x>.04f&&screen.x<.96f&&screen.y>.04f&&screen.y<.96f,"Arthur terlihat dalam bingkai "+name);
  var sprites=mover.wadahArahVisual.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.enabled&&s.sprite!=null).ToArray();
  var bounds=sprites[0].bounds;foreach(var sprite in sprites.Skip(1))bounds.Encapsulate(sprite.bounds);
  var low=cam.WorldToViewportPoint(new Vector3(bounds.center.x,bounds.min.y,bounds.center.z));var high=cam.WorldToViewportPoint(new Vector3(bounds.center.x,bounds.max.y,bounds.center.z));
  float bodyHeight=Mathf.Abs(high.y-low.y)*720;Log("UKUR "+name+" tinggiArthur720="+bodyHeight);
  Check(bodyHeight>=55&&bodyHeight<=(name.Contains("sudut-tangga")?250:160),"Ukuran Arthur nyaman dan konsisten: "+name);
  foreach(float x in new[]{bounds.min.x,bounds.max.x})foreach(float y in new[]{bounds.min.y,bounds.max.y}){
   var v=cam.WorldToViewportPoint(new Vector3(x,y,bounds.center.z));Check(v.z>0&&v.x>.02f&&v.x<.98f&&v.y>.02f&&v.y<.98f,"Seluruh badan/kaki berada dalam bingkai "+name);
  }
  var marker=GA("GA_LantaiSatu").transform.parent.GetComponentsInChildren<InteraksiJalurKampus>().FirstOrDefault(m=>m.jalur==mover.JalurAktif&&Vector3.Distance(m.transform.position,actor.transform.position)<.25f);
  if(marker!=null&&marker.ikon.localScale.sqrMagnitude>.001f){
   var bubble=marker.ikon.GetComponent<SpriteRenderer>().bounds;float gap=bubble.min.y-head.bounds.max.y;float offX=Mathf.Abs(cam.WorldToViewportPoint(bubble.center).x-cam.WorldToViewportPoint(head.bounds.center).x);
   Log("UKUR bubble "+marker.name+" jarakKepala="+gap+" selisihTengahLayar="+offX);
   Check(gap>=.06f&&gap<=.42f,"Bubble dekat di atas kepala: "+marker.name);
   Check(offX<.035f,"Bubble berada di tengah atas kepala: "+marker.name);
  }
  ScreenCapture.CaptureScreenshot(Dir+name+"-HUD.png");yield return .2f;
 }
 static JalurKampus Route(string n)=>GA(n).GetComponent<JalurKampus>();
 // DialogTextBox.png mempunyai padding transparan; uji batas tinta setelah nine-slicing.
 static Rect VisibleRect(UnityEngine.UI.Image im){var r=im.rectTransform.rect;var b=im.sprite.border/im.pixelsPerUnit;float py=266,ph=500;float y0=254/im.pixelsPerUnit;float y1=b.y+(399-py)/(ph-py)*(r.height-b.y-b.w);float x0=67/im.pixelsPerUnit,x1=r.width-47/im.pixelsPerUnit;var c=im.GetComponentInParent<Canvas>();Camera cam=c.renderMode==RenderMode.ScreenSpaceOverlay?null:c.worldCamera;var lo=RectTransformUtility.WorldToScreenPoint(cam,im.rectTransform.TransformPoint(new Vector3(r.xMin+x0,r.yMin+y0)));var hi=RectTransformUtility.WorldToScreenPoint(cam,im.rectTransform.TransformPoint(new Vector3(r.xMin+x1,r.yMin+y1)));return Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y);}
 static void CheckMenu(int count){
  Canvas.ForceUpdateCanvases();var prompt=choices.teksCeritaUtama.GetComponentInParent<UnityEngine.UI.Image>();Rect? prior=null;var question=VisibleRect(prompt);Log("UI pertanyaan="+question);
  for(int i=0;i<count;i++){var panel=(RectTransform)choices.daftarPilihanUtama[i].transform;var rect=VisibleRect(panel.GetComponent<UnityEngine.UI.Image>());Log("UI pilihan "+i+"="+rect);choices.teksPilihanUtama[i].ForceMeshUpdate();Check(!choices.teksPilihanUtama[i].isTextOverflowing,"Teks pilihan utuh "+i);Check(rect.yMax<question.yMin-2,"Pilihan berada di bawah panel pertanyaan "+i);if(prior.HasValue)Check(rect.yMax<prior.Value.yMin-2,"Jarak antarpilihan tidak bertumpuk "+i);prior=rect;}
  Check(choices.daftarPilihanUtama.Count(p=>p.gameObject.activeInHierarchy)==count,"Jumlah panel sesuai opsi "+count);
 }
 static IEnumerator Run(){
  actor=UnityEngine.Object.FindFirstObjectByType<KarakterController>();mover=actor.GetComponent<ArthurJalurKampus>();choices=UnityEngine.Object.FindFirstObjectByType<SistemPilihan>();actor.GetComponent<LazyBarArthur>().tanpaBatasWaktu=true;
  int f=Time.frameCount;float tm=Time.time;yield return .6f;Check(Time.frameCount>f&&Time.time>tm+.25f,"Simulasi maju nyata");
  if(SessionState.GetBool(Key+".phone",false)){var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();hp.BukaHPDefault();yield return 1f;hp.BukaMusik();yield return 1.5f;var app=hp.musik;Check(app.gameObject.activeInHierarchy,"Aplikasi musik terbuka dari HP");app.Segarkan();foreach(var name in new[]{"Cloudny","Pozy Mozy","Blue Neon","Velvet Farewell","Afterglow"})Check(app.namaSlot.Any(t=>t.text.Contains(name)),"Judul tampak di HP "+name);Check(!app.namaSlot.Any(t=>t.text.Contains("Instrumental")),"Slot instrumental dihapus dari HP");ScreenCapture.CaptureScreenshot(Dir+"musik-hp-HUD.png");yield return .3f;hp.TutupAplikasi();yield return .5f;hp.TutupHP();yield return .6f;Log("LULUS: lima lagu terlihat pada menu musik HP.");yield break;}
  foreach(var pair in new[]{new[]{"GK2_Persimpangan","↑ ↓"},new[]{"GK2_Ujung_KeGA","→"}}){var m=Old(pair[0]).GetComponent<InteraksiJalurKampus>();Check(m.ikon.GetComponentInChildren<TMPro.TMP_Text>(true).text==pair[1],"Arah panah "+pair[0]);Place(m.transform.position,m.jalur);yield return .5f;yield return Photo(pair[0]);}
  Check(GA("GA_KeLobby").GetComponent<InteraksiJalurKampus>().ikon.GetComponentInChildren<TMPro.TMP_Text>(true).text=="→","Panah lobby ke kanan");
  foreach(var pair in new[]{new[]{"GA_KeLobby","06-lobby-kanan"},new[]{"GA_Barat_KeLobby","13-sudut-tangga-lobby-kiri"}}){var m=GA(pair[0]).GetComponent<InteraksiJalurKampus>();Place(m.transform.position,m.jalur);yield return .5f;yield return Photo(pair[1]);yield return Open(pair[0]);yield return Choose(0,"GA_Lobby");}
  var top=GA("TitikTengahLobby_GA_LantaiTiga");foreach(int side in new[]{0,1}){Place(top.position,Route("GA_TerasLobby_LantaiTiga"));yield return .5f;yield return Open("GA_LobbyLantaiTiga_Turun");yield return Choose(side,side==0?"GA_LorongBarat_LantaiDua":"GA_LorongTengah_LantaiDua");Check(Mathf.Abs(actor.transform.position.x-(side==0?569:587))<.05f,"Turun tiga tiba di tengah lorong sisi "+side);yield return Photo("02-turun-tiga-sisi-"+side);yield return Open(side==0?"GA_TanggaBarat_NaikLantaiTiga":"GA_TanggaTengah_NaikLantaiTiga");yield return Choose(0,"GA_TerasLobby_LantaiTiga");}
  foreach(var n in new[]{"GA_LorongTengah_LantaiDua","GA_LorongBarat_LantaiDua","GA_LorongTengah_LantaiTiga","GA_LorongBarat_LantaiTiga"}){var r=Route(n);Place(r,2);yield return .4f;yield return Walk(1,8,5);Check(Mathf.Abs(actor.transform.position.x-(n.Contains("Tengah")?587:569))<.05f,"Lintasan tengah ubin "+n);yield return Walk(-1,2,5);}
  var front=Route("GA_JalanDepan");Place(front,24);yield return .5f;float low=actor.transform.position.y;yield return Walk(1,34,5);Check(actor.transform.position.y-low>.44f&&actor.transform.position.y-low<.60f,"Arthur naik setengah blok mengikuti anak tangga slab");Check(Mathf.Abs(actor.transform.position.y-(61-58.5f+ArthurJalurKampus.JarakKaki+.04f))<.12f,"Kaki di atas paving tinggi, bukan di dalam beton");yield return Photo("09-10-pijakan-tangga");yield return Walk(-1,24,5);Check(Mathf.Abs(actor.transform.position.y-low)<.12f,"Turun anak tangga kembali ke paving rendah");
  var fm=GA("GA_Teleport_JalanBelakang");Place(fm.position,front);yield return .5f;yield return Photo("11-bubble-jalan-belakang");yield return Open(fm.name);yield return Choose(0,"GA_JalanBelakang_Lorong");Check(Mathf.Abs(actor.transform.position.x-545)<.05f,"Teleport tiba di tengah bay x-34.5");yield return Photo("08-bubble-tengah-bay");yield return Open("GA_Teleport_JalanDepan");yield return Choose(0,front.name);
  var stair=GA("GA_UjungBarat_NaikLantaiDua");Place(stair.position,Route("GA_JalanBelakang_Lorong"));yield return .5f;yield return Photo("07-tangga-ujung-barat");yield return Open(stair.name);yield return Choose(0,"GA_TerasBarat_LantaiDua");yield return Open("GA_UjungBarat_TurunLantaiSatu");yield return Choose(0,"GA_JalanBelakang_Lorong");Check(Vector3.Distance(actor.transform.position,stair.position)<.06f,"Tangga ujung barat pergi-pulang");
  Check(Mathf.Abs(Route("GA_JalanBelakang_Lorong").jarakKamera-16)<.01f,"Kamera jalan belakang diperjauh menjadi 16");
  var music=UnityEngine.Object.FindFirstObjectByType<MusikArthur>();Check(music.playlist.lagu.Length==5,"Playlist tetap lima slot");Check(string.Join("|",music.playlist.lagu.Select(l=>l.judul))=="Cloudny|Pozy Mozy|Blue Neon|Velvet Farewell|Afterglow","Judul dan urutan lagu sesuai revisi");foreach(int index in new[]{3,4}){music.Pilih(index);music.PutarJeda();yield return Until(()=>music.sumber.isPlaying&&music.sumber.time>.15f,"Lagu baru terdengar",8);Check(music.sumber.clip.length>90,"Durasi lagu baru lebih dari 90 detik "+music.Lagu.judul);Check(music.Lagu.audio.loadType==AudioClipLoadType.Streaming,"Lagu baru memakai streaming "+music.Lagu.judul);music.HentikanSemua();}
  Log("LULUS: panah, akses lobby dua sisi, turun tiga ke lorong dua, pusat ubin, anak tangga slab, teleport tengah bay, tangga ujung barat, judul dan pemutaran dua lagu baru.");
 }
}
