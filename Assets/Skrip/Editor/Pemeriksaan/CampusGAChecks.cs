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
public static class CampusGAChecks
{
 const string Dir="work/ga-gameplay-20261025/",Key="Campus.GA.Check";
 static readonly Stack<IEnumerator> stack=new Stack<IEnumerator>();static double wait;
 static KarakterController actor;static ArthurJalurKampus mover;static SistemPilihan choices;
 static CampusGAChecks(){EditorApplication.update+=Tick;}
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
  Check(bodyHeight>=108&&bodyHeight<=155,"Ukuran Arthur nyaman dan konsisten: "+name);
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
 static IEnumerator Run(){
  actor=UnityEngine.Object.FindFirstObjectByType<KarakterController>();mover=actor.GetComponent<ArthurJalurKampus>();choices=UnityEngine.Object.FindFirstObjectByType<SistemPilihan>();actor.GetComponent<LazyBarArthur>().tanpaBatasWaktu=true;
  int frame=Time.frameCount;float time=Time.time;yield return .6f;Check(Time.frameCount>frame&&Time.time>time+.25f,"Frame dan simulasi maju nyata");
  var lower=GA("GA_LantaiSatu").GetComponent<JalurKampus>();var second=GA("GA_LantaiDua").GetComponent<JalurKampus>();var third=GA("GA_LantaiTiga").GetComponent<JalurKampus>();
  if(SessionState.GetBool(Key+".camera",false)){
   foreach(var r in new[]{lower,second,third})foreach(float input in new[]{1f,-1f}){
    float end=r==lower?26:18;
    Place(r,input>0?0:end);yield return .5f;actor.kecepatanJalan=5;
    var trace=new JejakGerakKamera(mover,input>0?Vector3.left:Vector3.right,Dir+r.name+(input>0?"-transisi-maju.csv":"-transisi-balik.csv"));
    try{yield return Walk(input,input>0?end:0,5,true);}finally{trace.Dispose();}
    Check(trace.Frames>100,"Transisi tangga diamati pada frame render: "+r.name+" "+input);
    Check(trace.CameraReverse<.004f&&trace.ActorReverse<.004f,"Kamera tangga tidak tertarik balik: "+r.name+" "+input+" reverse="+trace.CameraReverse);
   }
   Place(GA("TitikMasuk_GA").position,lower);yield return .5f;yield return Photo("07-GA-siap-dicoba");yield break;
  }
  if(SessionState.GetBool(Key+".quick",false)){Place(lower,lower.Panjang);yield return .5f;yield return Walk(-1,0,10,true);yield return Photo("cepat-batas-masuk");Place(GA("TitikTangga_GA_LantaiSatu").position,lower);yield return .5f;yield return Photo("cepat-tangga");yield break;}
  Check(choices.daftarPilihanUtama.Length==3,"Tiga panel tujuan pulang tersedia");
  Place(new Vector3(59.42f,.443f,2.17f),null,true);yield return .6f;yield return Open("MasukJasmine2");yield return Choose(1,"GA_LantaiSatu");CheckFloors(lower);yield return Photo("01-GA-tiba-dari-Jasmine");
  yield return Open("GA_KembaliKeAreaLama");
  Check(choices.daftarPilihanUtama.All(p=>p.gameObject.activeInHierarchy),"Ketiga pilihan pulang tampil");
  for(int i=0;i<3;i++){var t=choices.teksPilihanUtama[i];t.ForceMeshUpdate();Check(!t.isTextOverflowing,"Teks tujuan utuh: "+t.text);Check(Mathf.Abs(((RectTransform)choices.daftarPilihanUtama[i].transform).anchoredPosition.y-(-257-81*i))<5,"Panel tujuan tersusun: "+i);}
  yield return Photo("02-GA-menu-pulang");yield return Choose(2,null);Check(actor.GetComponent<Rigidbody2D>().simulated&&!mover.colliderJalur.enabled,"Fisika Jasmine pulih");
  yield return Open("MasukJasmine2");Check(!choices.daftarPilihanUtama[2].gameObject.activeSelf,"Panel ketiga tersembunyi pada dua opsi lama");Check(Mathf.Abs(((RectTransform)choices.daftarPilihanUtama[0].transform).anchoredPosition.y+257)<5,"Posisi dua pilihan lama dipulihkan");yield return Choose(1,"GA_LantaiSatu");
  foreach(string src in new[]{"GK2_Ujung_KeGA","GK2_Atas_KeGA","GK2_Belakang_KeGA","Kantin_PergiKeGA"}){
   var point=Old(src);var route=point.GetComponent<InteraksiJalurKampus>().jalur;Place(point.position,route);yield return .5f;yield return Open(src);if(src=="GK2_Ujung_KeGA")yield return Photo("08-GK2-akses-GA");yield return Choose(0,"GA_LantaiSatu");yield return Open("GA_KembaliKeAreaLama");yield return Choose(src.StartsWith("Kantin")?1:0,src.StartsWith("Kantin")?"Kantin_LorongTengah":"GK2_TerasAtas");
  }
  var source=Old("Kantin_PergiKeGA");Place(source.position,source.GetComponent<InteraksiJalurKampus>().jalur);yield return .5f;yield return Open(source.name);yield return Choose(0,"GA_LantaiSatu");
  yield return Walk(1,lower.JarakTerdekat(GA("GA_AksesLanjutan").position),5);yield return Open("GA_AksesLanjutan");var before=actor.transform.position;choices.TekanPilihanLangsung(0);yield return .3f;Check(Vector3.Distance(before,actor.transform.position)<.05f&&DialogManager.Instance.PercakapanAktif,"Akses lanjutan disiapkan tanpa teleport kosong");choices.TekanKeluarLangsung();yield return .5f;
  yield return Walk(1,lower.JarakTerdekat(GA("TitikTangga_GA_LantaiSatu").position),5);yield return Photo("03-GA-tangga-lantai-satu");yield return Open("GA_NaikLantaiDua");yield return Choose(0,"GA_LantaiDua");CheckFloors(second);yield return Photo("04-GA-lantai-dua");
  Check(Vector3.Distance(actor.transform.position,GA("TitikTangga_GA_LantaiDua").position)<.05f,"Tiba di kiri lantai dua sesuai screenshot");
  yield return Open("GA_TurunLantaiSatu");Check(DialogManager.Instance.opsiInteraksiAktif.Length==1&&DialogManager.Instance.opsiInteraksiAktif[0].teksOpsi=="Turun ke lantai satu","Bubble kiri hanya turun ke lantai satu");yield return Photo("05-GA-menu-turun-kiri");yield return Choose(0,"GA_LantaiSatu");
  Check(Vector3.Distance(actor.transform.position,GA("TitikTangga_GA_LantaiSatu").position)<.05f,"Turun kiri lantai dua kembali ke tangga lantai satu");
  yield return Open("GA_NaikLantaiDua");yield return Choose(0,"GA_LantaiDua");
  yield return Walk(1,second.JarakTerdekat(GA("TitikNaik_GA_LantaiDua").position),5);yield return Photo("09-GA-naik-kanan-lantai-dua");
  yield return Open("GA_NaikLantaiTiga");Check(DialogManager.Instance.opsiInteraksiAktif.Length==1&&DialogManager.Instance.opsiInteraksiAktif[0].teksOpsi=="Naik ke lantai tiga","Bubble kanan hanya naik ke lantai tiga");yield return Choose(0,"GA_LantaiTiga");CheckFloors(third);
  Check(Vector3.Distance(actor.transform.position,GA("TitikTangga_GA_LantaiTiga").position)<.05f,"Tiba di kiri lantai tiga sesuai screenshot");yield return Photo("06-GA-lantai-tiga");
  yield return Open("GA_TurunLantaiDua");Check(DialogManager.Instance.opsiInteraksiAktif.Length==1&&DialogManager.Instance.opsiInteraksiAktif[0].teksOpsi=="Turun ke lantai dua","Lantai tiga hanya satu opsi turun");yield return Choose(0,"GA_LantaiDua");Check(Vector3.Distance(actor.transform.position,GA("TitikNaik_GA_LantaiDua").position)<.05f,"Turun lantai tiga kembali ke tangga kanan lantai dua");
  foreach(var r in new[]{second,third}){yield return Trace(r,false);yield return Trace(r,true);}
  yield return Trace(lower,false);yield return Trace(lower,true);
  Place(GA("TitikTangga_GA_LantaiDua").position,second);yield return .5f;yield return Open("GA_TurunLantaiSatu");yield return Choose(0,"GA_LantaiSatu");
  actor.kecepatanJalan=5;Place(GA("TitikMasuk_GA").position,lower);yield return .5f;yield return Photo("07-GA-siap-dicoba");
  Log("LULUS: lima akses masuk termasuk ujung GK2, tiga tujuan pulang, tangga kiri-kanan 1-2-3 pulang-pergi, akses lanjutan, batas dan kamera jalan/lari. Pengaturan gameplay uji tidak disimpan.");
 }
}
