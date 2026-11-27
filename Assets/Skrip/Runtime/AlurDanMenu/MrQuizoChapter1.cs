using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Setiap penjaga menyimpan progres sendiri; titik jaga miliknya berbagi progres tersebut.
[RequireComponent(typeof(AreaInteraksi))]
public sealed class MrQuizoChapter1 : MonoBehaviour
{
 [Serializable] public sealed class Soal {
  [TextArea]public string pertanyaan;
  public string jawaban;
  public string[] pengecoh;
  [TextArea]public string penjelasan,komentarSalah;
  public string sumber;
 }
 public AreaInteraksi[] pintuKeluar;
 public Transform[] posisiJaga;
 public Transform tujuanGA;
 public JalurKampus jalurKantin;
 public Soal[] bankSoal;
 public string namaPenjaga="Mr.Quizo";
 [TextArea] public string sapaan="Stop dulu, Arthur. Ada kuis kampus sebelum kamu lanjut.";
 public MrQuizoChapter1 kuisSebelumnya;
 public bool bolehTanpaKuisSebelumnya;
 public TujuanJalurKampus[] batasJalan;
 [Range(2,6)]public int jumlahSoal=5;
 public bool Selesai{get;private set;}
 public int Terjawab=>langkah;
 public Soal SoalAktif=>urutan!=null&&langkah<urutan.Count?bankSoal[urutan[langkah]]:null;
 public string[] PilihanTampil{get;private set;}
 public static MrQuizoChapter1 Instance{get;private set;}
 static readonly List<MrQuizoChapter1> penjagaAktif=new List<MrQuizoChapter1>();
 AreaInteraksi area;SistemPilihan pilihan;ArthurJalurKampus arthur;
 List<int> urutan;int langkah;bool sedangSoal;float cekBerikutnya;Transform posisiAktif;
 bool ulangLagu;
 readonly MusikMinigameArthur musik = new MusikMinigameArthur();
 void OnEnable(){if(Application.isPlaying){if(!penjagaAktif.Contains(this))penjagaAktif.Add(this);if(Instance==null)Instance=this;}}
 void OnDisable(){musik.Tutup();penjagaAktif.Remove(this);if(Instance==this)Instance=penjagaAktif.Count>0?penjagaAktif[0]:null;}
 void Start(){area=GetComponent<AreaInteraksi>();pilihan=FindFirstObjectByType<SistemPilihan>();arthur=FindFirstObjectByType<ArthurJalurKampus>();SiapkanUrutan();Intro();Posisikan();}
 void SiapkanUrutan(){
  urutan=new List<int>();if(bankSoal==null||bankSoal.Length<2)throw new InvalidOperationException("Bank Mr.Quizo minimal dua soal.");
  // Dua soal pembuka selalu ditanyakan; soal tambahan berganti antar sesi cerita.
  urutan.Add(0);urutan.Add(1);var sisa=new List<int>();for(int i=2;i<bankSoal.Length;i++)sisa.Add(i);
  for(int i=sisa.Count-1;i>0;i--){int j=UnityEngine.Random.Range(0,i+1);int n=sisa[i];sisa[i]=sisa[j];sisa[j]=n;}
  for(int i=0;i<sisa.Count&&urutan.Count<Mathf.Min(jumlahSoal,bankSoal.Length);i++)urutan.Add(sisa[i]);
 }
 void Update(){
  var dialog = DialogManager.Instance;
  if (dialog == null || !dialog.PercakapanAktif || dialog.PemilikInteraksi != area) musik.Tutup();
  if(Time.unscaledTime<cekBerikutnya||dialog==null||dialog.PercakapanAktif)return;
  cekBerikutnya=Time.unscaledTime+.25f;Posisikan();
 }
 void Posisikan(){
  if(arthur==null||posisiJaga==null||posisiJaga.Length==0)return;
  Transform titik;
  if (jalurKantin != null && posisiJaga.Length == 2)
    titik = posisiJaga[arthur.JalurAktif == jalurKantin ? 1 : 0];
  else
  {
    titik = null;
    float terbaik = float.PositiveInfinity;
    foreach (var calon in posisiJaga)
    {
      if (calon == null) continue;
      var r=calon.GetComponent<TujuanJalurKampus>();
      float jarak = (calon.position - arthur.transform.position).sqrMagnitude;
      if(r!=null && r.jalur==arthur.JalurAktif)jarak-=1000000;
      if (jarak < terbaik) { terbaik = jarak; titik = calon; }
    }
  }
  if (titik == null || titik == posisiAktif) return;
  posisiAktif = titik;
  transform.position = titik.position + (Selesai ? Vector3.right * 1.15f : Vector3.zero);
  if (transform is RectTransform rect) rect.anchoredPosition3D = rect.localPosition;
  var npc=GetComponent<NpcYangBisaDiatur>();if(npc!=null)npc.jalurInteraksi=titik.GetComponent<TujuanJalurKampus>()?.jalur;
 }
 public static bool Cegat(AreaInteraksi pemilik,OpsiInteraksi opsi){
  if(ModeUjiArthur.KuisDilewati)return false;
  if(pemilik==null||opsi==null)return false;
  foreach(var q in penjagaAktif){
  if(q==null||!q.isActiveAndEnabled||q.Selesai)continue;
  if (q.tujuanGA != null && opsi.titikTujuan != q.tujuanGA) continue;
  bool exit=false;foreach(var gate in q.pintuKeluar??Array.Empty<AreaInteraksi>())if(gate!=null&&gate==pemilik){foreach(var original in gate.daftarOpsi??Array.Empty<OpsiInteraksi>())if(original.titikTujuan!=null&&original.titikTujuan==opsi.titikTujuan)exit=true;}
  if(!exit)continue;q.Posisikan();if(q.langkah==0)q.Intro();else q.Pertanyaan();q.pilihan.SegarkanPercakapan(q.area);
  if (q.langkah > 0) q.musik.Putar(q.gameObject, MusikMinigameArthur.LaguTapTipTup());
  return true;
  }return false;
 }
 public static float BatasiGerak(JalurKampus jalur,float dari,float menuju){
  if(ModeUjiArthur.KuisDilewati)return menuju;
  foreach(var q in penjagaAktif)if(q!=null&&!q.Selesai)foreach(var batas in q.batasJalan??Array.Empty<TujuanJalurKampus>()){
   if(batas==null||batas.jalur!=jalur)continue;float titik=jalur.JarakTerdekat(batas.transform.position);
   if(dari<titik && menuju>dari)menuju=Mathf.Min(menuju,Mathf.Max(dari,titik-.85f));
   else if(dari>=titik && menuju<dari)menuju=Mathf.Max(menuju,Mathf.Min(dari,titik+.85f));
  }return menuju;
 }
 OpsiInteraksi Opsi(string label,int aksi)=>new OpsiInteraksi{teksOpsi=label,mrQuizo=this,aksiQuizo=aksi,nyalakanGrupBelakang=false};
 void Dialog(string text,OpsiInteraksi[] opsi){area.idTempat="NPC_Kuis_"+gameObject.GetInstanceID();area.tipeInteraksi=TipeInteraksi.NPC_Karakter;area.skenarioDialog=new[]{new BarisDialog{siapaYangBicara=Pembicara.Kanan_NPC,namaKarakter=namaPenjaga,teksDialog=text,teksTombolLanjut="Lanjut"}};area.daftarOpsi=opsi;area.tampilkanTombolKeluar=true;}
 void Intro(){sedangSoal=false;if(!bolehTanpaKuisSebelumnya&&kuisSebelumnya!=null&&!kuisSebelumnya.Selesai){Dialog("Selesaikan dulu kuis sebelumnya. Jalannya boleh bercabang, urutannya jangan.",new[]{Opsi("Baik, aku kembali dulu.",-3)});return;}Dialog(sapaan+" "+urutan.Count+" soal, tanpa batas waktu. Kalau salah, kita bahas lalu coba lagi.",new[]{Opsi("Siap, tes aku!",-1),Opsi("Sebentar, aku mau mundur dulu.",-3)});}
 void Pertanyaan(){
  if(Selesai)return;var s=SoalAktif;var values=new List<string>{s.jawaban};values.AddRange(s.pengecoh);for(int i=values.Count-1;i>0;i--){int j=UnityEngine.Random.Range(0,i+1);string temp=values[i];values[i]=values[j];values[j]=temp;}PilihanTampil=values.ToArray();var opsi=new OpsiInteraksi[values.Count];for(int i=0;i<values.Count;i++)opsi[i]=Opsi(values[i],i);
  sedangSoal=true;Dialog("Soal "+(langkah+1)+"/"+urutan.Count+". "+s.pertanyaan,opsi);
 }
 public void Pilih(int aksi){
  if(aksi==-3){musik.Tutup();pilihan.TutupPercakapanTerarah();return;}
  if(!bolehTanpaKuisSebelumnya&&kuisSebelumnya!=null&&!kuisSebelumnya.Selesai){Intro();pilihan.SegarkanPercakapan(area);return;}
  if(aksi==-1||aksi==-2){if(Selesai){pilihan.TutupPercakapanTerarah();return;}
   Pertanyaan();pilihan.SegarkanPercakapan(area);
   if (aksi == -1 || ulangLagu) musik.Putar(gameObject, MusikMinigameArthur.LaguTapTipTup());
   ulangLagu = false;
   return;
  }
  if(!sedangSoal||PilihanTampil==null||aksi<0||aksi>=PilihanTampil.Length)return;sedangSoal=false;var soal=SoalAktif;bool benar=PilihanTampil[aksi]==soal.jawaban;
  if(benar){langkah++;Selesai=langkah>=urutan.Count;if(Selesai)StartCoroutine(Minggir());}
  ulangLagu = !benar;
  if (Selesai) musik.Tutup();
  else if (ulangLagu) musik.Hentikan();
  string feedback=benar?"Tepat! "+soal.penjelasan:"Belum tepat. "+soal.komentarSalah+" "+soal.penjelasan;
  if(Selesai)feedback+=" Kuis tuntas! Jalan sudah terbuka. Silakan lanjut, Arthur. Hati-hati di jalan!";
  Dialog(feedback,new[]{Opsi(Selesai?"Terima kasih, aku lanjut!":benar?"Pertanyaan berikutnya!":"Oke, coba lagi.",Selesai?-3:-2)});pilihan.SegarkanPercakapan(area);
 }
 IEnumerator Minggir(){var start=transform.position;var finish=start+Vector3.right*1.15f;var rig=GetComponent<NpcKampusHidup>();if(rig!=null){rig.langkahMengikutiGerak=true;rig.waktuJalan=0;rig.kegiatan=KegiatanNpcKampus.Berjalan;}for(float time=0;time<.5f;time+=Time.deltaTime){var before=transform.position;transform.position=Vector3.Lerp(start,finish,Mathf.SmoothStep(0,1,time/.5f));if(rig!=null)rig.waktuJalan+=Vector3.Distance(before,transform.position)/2.5f;if(transform is RectTransform rect)rect.anchoredPosition3D=rect.localPosition;yield return null;}transform.position=finish;if(transform is RectTransform r)r.anchoredPosition3D=r.localPosition;if(rig!=null)rig.kegiatan=KegiatanNpcKampus.Berdiri;}
}

