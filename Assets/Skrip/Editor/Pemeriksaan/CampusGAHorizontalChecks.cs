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
public static class CampusGAHorizontalChecks
{
 const string Dir="work/ga-horizontal-20261026/",Key="Campus.GAHorizontal.Check";
 static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();static double wait;
 static KarakterController actor;static ArthurJalurKampus mover;static SistemPilihan choices;
 static CampusGAHorizontalChecks(){EditorApplication.update+=Tick;}
 static Transform GA(string n)=>GameObject.Find("RuteGA_CharacterLab/"+n).transform;
 static Transform Old(string n)=>GameObject.Find("RuteKampus_CharacterLab/"+n).transform;
 static void Log(string s)=>File.AppendAllText(Dir+"pengujian.txt",s+"\n");
 static void Check(bool ok,string s){if(!ok)throw new Exception(s);Log("PASS: "+s);}
 static void Tick(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating||AssetDatabase.IsAssetImportWorkerProcess())return;
  try{
   if(File.Exists(Dir+"uji.request")){
    if(EditorApplication.isPlayingOrWillChangePlaymode||SceneManager.GetActiveScene().isDirty||SceneManager.GetActiveScene().path!="Assets/Scenes/CharacterLab.unity")throw new Exception("Perlu CharacterLab tersimpan");
    var request=File.ReadAllText(Dir+"uji.request");SessionState.SetBool(Key+".quick",request.Contains("cepat"));SessionState.SetBool(Key+".camera",request.Contains("kamera"));File.Delete(Dir+"uji.request");SessionState.SetBool(Key+".mute",EditorUtility.audioMasterMute);SessionState.SetBool(Key+".background",Application.runInBackground);SessionState.SetBool(Key+".done",false);
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
  Check(bodyHeight>=55&&bodyHeight<=(name=="07-sudut-tangga-disukai"?250:160),"Ukuran Arthur nyaman dan konsisten: "+name);
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
 static void CheckFloors(JalurKampus active){
  foreach(string n in new[]{"GA_LantaiSatu","GA_LantaiDua","GA_LantaiTiga"}){var r=GA(n).GetComponent<JalurKampus>();Check(r.pijakanKhususJalur.activeSelf==(r==active),"Pijakan lantai terpisah: "+n);}
 }
 static IEnumerator Trace(JalurKampus r,bool running){
  Place(r,13);yield return .5f;actor.kecepatanJalan=5;actor.BerhentiBergerak();
  if(running){actor.JalanKeKanan();yield return .06f;actor.LepasKanan();yield return .06f;}
  var trace=new JejakGerakKamera(mover,Vector3.left,Dir+r.name+(running?"-lari.csv":"-jalan.csv"));
  float end=Time.realtimeSinceStartup+1.6f;
  try{while(Time.realtimeSinceStartup<end){actor.JalanKeKanan();yield return null;}}finally{actor.BerhentiBergerak();trace.Dispose();}
  Check(trace.Frames>10,"Kamera dicatat pada frame render "+r.name+" "+running);
  Check(trace.CameraReverse<.004f&&trace.ActorReverse<.004f,"Gerak tidak ditarik balik "+r.name+" "+running);
  Check(running?trace.VerticalRange>.02f&&trace.VerticalRange<.12f:trace.VerticalRange<.003f,"Ayunan hanya ketika berlari "+r.name+" rentang="+trace.VerticalRange);
  yield return .3f;
 }
 static JalurKampus Route(string n)=>GA(n).GetComponent<JalurKampus>();
 static IEnumerator Travel(float input,string name,float distance,bool run=true){
  actor.kecepatanJalan=8;actor.BerhentiBergerak();
  if(run){if(input>0)actor.JalanKeKanan();else actor.JalanKeKiri();yield return .06f;if(input>0)actor.LepasKanan();else actor.LepasKiri();yield return .06f;}
  float end=Time.realtimeSinceStartup+35;string previous=mover.JalurAktif.name;
  while(!(mover.JalurAktif.name==name&&(input>0?mover.JarakDiJalur>=distance-.05f:mover.JarakDiJalur<=distance+.05f))){
   if(Time.realtimeSinceStartup>end)throw new Exception("Tertahan "+mover.JalurAktif.name+" "+mover.JarakDiJalur+" tujuan="+name+" "+distance);
   actor.kecepatanJalan=mover.JalurAktif.name==name?Mathf.Clamp(Mathf.Abs(mover.JarakDiJalur-distance)*5,.8f,8):8;
   if(input>0)actor.JalanKeKanan();else actor.JalanKeKiri();
   if(previous!=mover.JalurAktif.name){Log("SAMBUNG "+previous+" -> "+mover.JalurAktif.name);previous=mover.JalurAktif.name;}
   mover.JalurAktif.Sampel(mover.JarakDiJalur,out var p,out _,out _);CheckNear(p);yield return null;
  }
  actor.BerhentiBergerak();yield return .5f;Check(mover.JalurAktif.name==name,"Berjalan sampai "+name+" "+distance);
 }
 static void CheckNear(Vector3 p){if(Vector3.Distance(p,actor.transform.position)>.2f)throw new Exception("Arthur keluar pijakan "+mover.JalurAktif.name);}
 static IEnumerator TraceCorridor(bool forward){
  var r=Route("GA_LorongTanggaLobby");Place(r,forward?.8f:r.Panjang-.8f);yield return .5f;
  var trace=new JejakGerakKamera(mover,forward?Vector3.forward:Vector3.back,Dir+(forward?"lorong-maju.csv":"lorong-balik.csv"));
  try{yield return Travel(forward?1:-1,r.name,forward?r.Panjang-.8f:.8f);}finally{trace.Dispose();}
  Check(trace.Frames>30,"Frame render lorong direkam "+forward);Check(trace.CameraReverse<.004f,"Kamera lorong tidak tertarik balik "+forward+" reverse="+trace.CameraReverse);
 }
 static IEnumerator TraceUpper(JalurKampus r,bool forward){
  Place(r,forward?.8f:r.Panjang-.8f);yield return .5f;var trace=new JejakGerakKamera(mover,forward?Vector3.forward:Vector3.back,Dir+r.name+(forward?"-maju.csv":"-balik.csv"));
  try{yield return Travel(forward?1:-1,r.name,forward?r.Panjang-.8f:.8f);}finally{trace.Dispose();}
  Check(trace.Frames>30,"Frame render teras direkam "+r.name+" "+forward);Check(trace.CameraReverse<.004f,"Kamera teras tidak tertarik balik "+r.name+" "+forward+" reverse="+trace.CameraReverse);
 }
 static Rect ScreenRect(RectTransform rect){var corners=new Vector3[4];rect.GetWorldCorners(corners);var c=rect.GetComponentInParent<Canvas>();Camera cam=c.renderMode==RenderMode.ScreenSpaceOverlay?null:c.worldCamera;var lo=RectTransformUtility.WorldToScreenPoint(cam,corners[0]);var hi=RectTransformUtility.WorldToScreenPoint(cam,corners[2]);return Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y);}
 // DialogTextBox.png mempunyai padding transparan; uji batas tinta setelah nine-slicing.
 static Rect VisibleRect(UnityEngine.UI.Image im){var r=im.rectTransform.rect;var b=im.sprite.border/im.pixelsPerUnit;float py=266,ph=500;float y0=254/im.pixelsPerUnit;float y1=b.y+(399-py)/(ph-py)*(r.height-b.y-b.w);float x0=67/im.pixelsPerUnit,x1=r.width-47/im.pixelsPerUnit;var c=im.GetComponentInParent<Canvas>();Camera cam=c.renderMode==RenderMode.ScreenSpaceOverlay?null:c.worldCamera;var lo=RectTransformUtility.WorldToScreenPoint(cam,im.rectTransform.TransformPoint(new Vector3(r.xMin+x0,r.yMin+y0)));var hi=RectTransformUtility.WorldToScreenPoint(cam,im.rectTransform.TransformPoint(new Vector3(r.xMin+x1,r.yMin+y1)));return Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y);}
 static void CheckMenu(int count){
  Canvas.ForceUpdateCanvases();var prompt=choices.teksCeritaUtama.GetComponentInParent<UnityEngine.UI.Image>();Rect? prior=null;var question=VisibleRect(prompt);Log("UI pertanyaan="+question);
  for(int i=0;i<count;i++){var panel=(RectTransform)choices.daftarPilihanUtama[i].transform;var rect=VisibleRect(panel.GetComponent<UnityEngine.UI.Image>());Log("UI pilihan "+i+"="+rect);choices.teksPilihanUtama[i].ForceMeshUpdate();Check(!choices.teksPilihanUtama[i].isTextOverflowing,"Teks pilihan utuh "+i);Check(rect.yMax<question.yMin-2,"Pilihan berada di bawah panel pertanyaan "+i);if(prior.HasValue)Check(rect.yMax<prior.Value.yMin-2,"Jarak antarpilihan tidak bertumpuk "+i);prior=rect;}
  Check(choices.daftarPilihanUtama.Count(p=>p.gameObject.activeInHierarchy)==count,"Jumlah panel sesuai opsi "+count);
 }
 static IEnumerator Run(){
  actor=UnityEngine.Object.FindFirstObjectByType<KarakterController>();mover=actor.GetComponent<ArthurJalurKampus>();choices=UnityEngine.Object.FindFirstObjectByType<SistemPilihan>();actor.GetComponent<LazyBarArthur>().tanpaBatasWaktu=true;
  int f=Time.frameCount;float t=Time.time;yield return .6f;Check(Time.frameCount>f&&Time.time>t+.25f,"Frame dan simulasi maju nyata");
  var lobby=Route("GA_Lobby");Place(lobby,lobby.Panjang);yield return .5f;yield return Open("GA_Lobby_PilihJalan");yield return Photo("01-pilihan-rapi");CheckMenu(3);choices.TekanKeluarLangsung();yield return .6f;
  var front=Route("GA_JalanDepan");var lane=Route("GA_JalanBelakang_Lorong");var marker=GA("GA_Teleport_JalanBelakang");Place(marker.position,front);yield return .5f;yield return Photo("02-akses-jalan-belakang");yield return Open(marker.name);CheckMenu(1);Check(DialogManager.Instance.opsiInteraksiAktif[0].teksOpsi=="Pergi ke jalan belakang","Label teleport sesuai permintaan");yield return Photo("03-menu-teleport");yield return Choose(0,lane.name);Check(Vector3.Distance(actor.transform.position,GA("TitikTeleport_JalanBelakangGA").position)<.05f,"Arthur teleport ke jalan belakang");yield return Photo("04-tiba-jalan-belakang");yield return Open("GA_Teleport_JalanDepan");yield return Choose(0,front.name);Check(Vector3.Distance(actor.transform.position,marker.position)<.05f,"Teleport balik tiba pada akses yang sama");
  foreach(string side in new[]{"Tengah","Barat"}){var m=GA("GA_Tangga"+side+"_NaikLantaiTiga");Place(m.position,m.GetComponent<InteraksiJalurKampus>().jalur);yield return .5f;yield return Photo("05-naik-lantai-tiga-"+side);yield return Open(m.name);CheckMenu(1);yield return Choose(0,"GA_TerasLobby_LantaiTiga");Check(Vector3.Distance(actor.transform.position,GA("TitikTengahLobby_GA_LantaiTiga").position)<.05f,"Tangga "+side+" tiba di lantai tiga");yield return Open("GA_LobbyLantaiTiga_Turun");CheckMenu(2);yield return Choose(side=="Tengah"?1:0,side=="Tengah"?"GA_LorongTengah_LantaiDua":"GA_LorongBarat_LantaiDua");Check(Vector3.Distance(actor.transform.position,GA(side=="Tengah"?"TitikTurunTiga_SisiTimurGA":"TitikTurunTiga_SisiBaratGA").position)<.05f,"Turun kembali pada sisi yang dipilih");}
  foreach(var r in new[]{lane,Route("GA_TerasBarat_LantaiDua"),Route("GA_TerasBarat_LantaiTiga")}){
   Place(r,12);yield return .5f;Check(Mathf.Abs(Mathf.DeltaAngle(mover.kamera.transform.eulerAngles.y,180))<.2f,"Kamera horizontal "+r.name);var start=actor.transform.position;var startScreen=mover.kamera.WorldToViewportPoint(start);actor.kecepatanJalan=4;
   var trace=new JejakGerakKamera(mover,Vector3.left,Dir+r.name+"-horizontal.csv");try{yield return Walk(1,17,4);}finally{trace.Dispose();}var delta=actor.transform.position-start;Check(delta.x < -4&&Mathf.Abs(delta.z)<.02f,"Arthur berjalan horizontal, tidak masuk kedalaman "+r.name);Check(trace.CameraReverse<.004f,"Kamera horizontal tidak tertarik balik "+r.name);yield return Photo("06-horizontal-"+r.name);yield return Walk(-1,12,4);Check(Mathf.Abs(mover.JarakDiJalur-12)<.2f,"Kendali kiri kembali pada jalur "+r.name);
  }
  var corner=Route("GA_JalanBelakang");Place(corner,8);yield return .5f;var liked=GA("Kamera_SudutDisukaiGA");Check(Vector3.Distance(mover.kamera.transform.position,liked.position)<.025f,"Sudut kamera tangga yang disukai dipertahankan");yield return Photo("07-sudut-tangga-disukai");
  Log("LULUS: susunan 1/2/3 opsi, teleport pergi-pulang, dua akses lantai tiga, gerak horizontal tiga koridor, kamera tangga disukai.");
 }
}
