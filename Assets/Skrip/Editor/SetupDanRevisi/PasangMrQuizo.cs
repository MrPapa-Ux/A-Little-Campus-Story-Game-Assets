using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class PasangMrQuizo
{
 const string Work="work/mr-quizo-20261112/";
 static MrQuizoChapter1.Soal Soal(string q,string benar,string[] salah,string ingat,string lucu,string sumber)=>new MrQuizoChapter1.Soal{pertanyaan=q,jawaban=benar,pengecoh=salah,penjelasan=ingat,komentarSalah=lucu,sumber=sumber};
 public static MrQuizoChapter1.Soal[] Bank(){
  const string pimpinan="https://www.unklab.ac.id/acara-serah-terima-jabatan-pimpinan-fakultas-prodi-dan-lembaga-universitas-klabat-periode-2025-2030/";
  const string filkom="https://www.unklab.ac.id/fakultas-ilmu-komputer/",profil="https://www.unklab.ac.id/tentang-unklab/";
  return new[]{
   Soal("Siapa rektor Universitas Klabat?","Prof. Danny Ivan Rantung",new[]{"Stenly Richard Pungus","Prof. Andrew Tanny Liem","Pak Router, ahli Wi-Fi"},"Rektor UNKLAB adalah Prof. Danny Ivan Rantung. Rektor memimpin universitas.","Pak Router sibuk mengurus sinyal, Arthur.","https://www.unklab.ac.id/professorial-inauguration-unklab-2026-guru-besar/"),
   Soal("Siapa dekan Fakultas Ilmu Komputer (FILKOM) UNKLAB?","Stenly Richard Pungus",new[]{"Semmy W. Taju","Jimmy H. Moedjahedy","Dr. Ctrl+Alt+Del"},"Dekan FILKOM adalah Stenly Richard Pungus. Ingat pasangan nama dan fakultasnya.","Restart laptop belum tentu me-restart ingatan.",pimpinan),
   Soal("Siapa dekan Fakultas Ekonomi dan Bisnis (FEB) UNKLAB?","Dr. Elvis Ronald Sumanti",new[]{"Stenly Richard Pungus","Semmy W. Taju","Pak Cashback, M.Diskon"},"Dekan FEB adalah Dr. Elvis Ronald Sumanti. FEB: Ekonomi dan Bisnis.","Cashback boleh dicari. Nama dekan juga perlu diingat.",pimpinan),
   Soal("Siapa Kaprodi Sistem Informasi di FILKOM UNKLAB?","Jimmy H. Moedjahedy",new[]{"Ir. Marchel T. Tombeng","Semmy W. Taju","Bang Login, lupa sandi"},"Program Studi Sistem Informasi dipimpin Jimmy Moedjahedy.","Lupa sandi masih bisa direset. Nama kaprodi kita pelajari lagi.",filkom),
   Soal("Kampus UNKLAB berada di daerah mana?","Airmadidi, Sulawesi Utara",new[]{"Tomohon, Sulawesi Utara","Bitung, Sulawesi Utara","Di folder Downloads"},"Kampus UNKLAB berada di Airmadidi, Minahasa Utara, Sulawesi Utara.","Folder Downloads boleh penuh, tetapi kampus tidak tinggal di sana.",profil),
   Soal("Tahun berapa Universitas Klabat berdiri?","1965",new[]{"1999","2005","Tahun pertama Wi-Fi nge-lag"},"Universitas Klabat berdiri pada tahun 1965. Angka itu juga ada pada lambangnya.","Wi-Fi bisa putus. Ingatan tentang 1965 kita sambungkan lagi.",profil)
  };
 }
 [MenuItem("Arthur/Chapter 1/Pasang Mr.Quizo di akses Jasmine")]
 public static void Run(){
  if(EditorApplication.isPlaying)throw new Exception("Hentikan Play dahulu.");Directory.CreateDirectory(Work);var original=SceneManager.GetActiveScene();const string path="Assets/Scenes/Penataan Kampus - Jalur NPC dan Bubble.unity";var scene=SceneManager.GetSceneByPath(path);bool loaded=scene.isLoaded;if(!loaded)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);SceneManager.SetActiveScene(scene);
  if(!File.Exists(Work+"Chapter1-sebelum-MrQuizo.unity"))EditorSceneManager.SaveScene(scene,Work+"Chapter1-sebelum-MrQuizo.unity",true);
  var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();var root=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="NPC Mr.Quizo - Chapter 1");if(root==null)root=new GameObject("NPC Mr.Quizo - Chapter 1");var group=root.GetComponent<KelompokNpcManual>();if(group==null)group=root.AddComponent<KelompokNpcManual>();group.kawasan="JasmineGK2";
  var q=root.GetComponentInChildren<MrQuizoChapter1>(true);GameObject npc;if(q==null){npc=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Aset/Karakter/NPC_Kampus/Template/NPC_Mahasiswa.prefab"),root.transform);npc.name="Mr.Quizo";q=npc.AddComponent<MrQuizoChapter1>();}else npc=q.gameObject;
  q.bankSoal=Bank();q.jumlahSoal=5;
  foreach(var s in q.bankSoal)if(s.pengecoh.Length!=3||s.pengecoh.Contains(s.jawaban)||s.pengecoh.Distinct().Count()!=3)throw new Exception("Opsi soal tidak valid: "+s.pertanyaan);
  AturTitikJaga(q,all);
  npc.transform.localScale=Vector3.one;
  var n=npc.GetComponent<NpcYangBisaDiatur>();n.namaNpc="Mr.Quizo";n.dialogSederhana=false;n.jalurInteraksi=null;n.jarakInteraksi=2.3f;var life=npc.GetComponent<NpcKampusHidup>();life.kegiatan=KegiatanNpcKampus.Mengobrol;life.fase=2.35f;
  var dandi=all.FirstOrDefault(t=>t.GetComponent<KepergianDandi>()!=null);if(dandi!=null){var male=dandi.GetComponent<NpcKampusHidup>();if(male!=null&&male.kepala!=null&&life.kepala!=null){var from=male.kepala.GetComponent<SpriteRenderer>();var to=life.kepala.GetComponent<SpriteRenderer>();if(from!=null&&to!=null&&from.sprite!=null&&to.sprite!=null){float width=to.sprite.bounds.size.x*Mathf.Abs(to.transform.localScale.x);to.sprite=from.sprite;float size=width/Mathf.Max(.001f,from.sprite.bounds.size.x);to.transform.localScale=new Vector3(size,size,to.transform.localScale.z);PrefabUtility.RecordPrefabInstancePropertyModifications(to);PrefabUtility.RecordPrefabInstancePropertyModifications(to.transform);}}}
  if(dandi!=null&&life.kepala!=null){var male=dandi.GetComponent<NpcKampusHidup>();if(male!=null&&male.kepala!=null){var source=male.kepala.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(r=>r.name=="Rambut 1");var target=life.kepala.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(r=>r.name=="Rambut 1");if(source!=null&&target!=null){target.sprite=source.sprite;target.transform.localPosition=source.transform.localPosition;target.transform.localScale=source.transform.localScale;PrefabUtility.RecordPrefabInstancePropertyModifications(target);PrefabUtility.RecordPrefabInstancePropertyModifications(target.transform);}}}
  if(life.badan!=null){var shirt=life.badan.GetComponent<SpriteRenderer>();if(shirt!=null){shirt.color=new Color(.8f,.65f,1);PrefabUtility.RecordPrefabInstancePropertyModifications(shirt);}}
  foreach(var collider in npc.GetComponents<Collider2D>())collider.enabled=false;
  var area=npc.GetComponent<AreaInteraksi>();area.idTempat="NPC_MrQuizo_Chapter1";area.tipeInteraksi=TipeInteraksi.NPC_Karakter;area.tampilkanTombolKeluar=true;area.skenarioDialog=new[]{new BarisDialog{siapaYangBicara=Pembicara.Kanan_NPC,namaKarakter="Mr.Quizo",teksDialog="Mau ke GA? Mr.Quizo punya kuis kampus untukmu!"}};area.daftarOpsi=new[]{new OpsiInteraksi{teksOpsi="Siap, tes aku!",mrQuizo=q,aksiQuizo=-1},new OpsiInteraksi{teksOpsi="Nanti dulu.",mrQuizo=q,aksiQuizo=-3}};
  foreach(var component in npc.GetComponents<Component>()){PrefabUtility.RecordPrefabInstancePropertyModifications(component);EditorUtility.SetDirty(component);}EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);if(!loaded)EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(original);AssetDatabase.SaveAssets();
  File.WriteAllText(Work+"BANK-SOAL.md","# Mr.Quizo — bank soal\n\nRujukan diverifikasi12 November2026. Dua soal awal (rektor dan dekan FILKOM) selalu masuk; tiga soal tambahan dipilih acak dari empat lainnya. Empat opsi per soal, urutan jawaban diacak. Salah mendapat penjelasan dan boleh mencoba lagi; boleh menunda tetapi akses jalan tetap tertutup. Lulus berlaku selama sesi Chapter1, tidak disimpan global ke permainan baru.\n\n"+string.Join("\n\n",Bank().Select(s=>s.pertanyaan+"\nJawaban: "+s.jawaban+"\nPengecoh: "+string.Join(" | ",s.pengecoh)+"\nPenjelasan: "+s.penjelasan+"\nSumber: "+s.sumber)));
 }

 [MenuItem("Arthur/Chapter 1/Pindahkan Mr.Quizo ke Jalan Jasmine dan Kantin")]
 public static void PindahkanKeJalurGA()
 {
  if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Hentikan Play dahulu.");
  var scene = SceneManager.GetActiveScene();
  if (scene.name != "Penataan Kampus - Jalur NPC dan Bubble" || scene.isDirty)
   throw new InvalidOperationException("Buka dan simpan scene Penataan Kampus - Jalur NPC dan Bubble terlebih dahulu.");
  var objek = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true)).ToArray();
  var quizo = objek.Select(x => x.GetComponent<MrQuizoChapter1>()).Single(x => x != null && x.kuisSebelumnya == null);
  Directory.CreateDirectory(Work);
  const string cadangan = Work + "Chapter1-sebelum-jaga-GA.unity";
  if (!File.Exists(cadangan)) File.Copy(scene.path, cadangan);
  AturTitikJaga(quizo, objek);
  EditorSceneManager.MarkSceneDirty(scene);
  if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Scene Chapter 1 gagal disimpan.");
 }

 static void AturTitikJaga(MrQuizoChapter1 quizo, Transform[] objek)
 {
  Transform Cari(string nama) => objek.Single(x => x.name == nama);
  var jalan = Cari("TriggerTempatKeHalamanAsramaJasmine2");
  var kantin = Cari("Kantin_PergiKeGA");
  var jalur = kantin.GetComponent<InteraksiJalurKampus>().jalur;
  var masukGA = Cari("TitikMasuk_GA");
  quizo.pintuKeluar = new[] { jalan.GetComponent<AreaInteraksi>(), kantin.GetComponent<AreaInteraksi>() };
  if (quizo.pintuKeluar.Any(gate => !gate.daftarOpsi.Any(opsi => opsi.titikTujuan == masukGA)))
   throw new InvalidOperationException("Akses ke GA harus tersedia dari Jalan Jasmine dan Kantin.");

  var titikJalan = Cari("TitikJalanRayaJasmine2").position;
  var posisiJalan = titikJalan + new Vector3(-1.7f, -ArthurJalurKampus.JarakKaki + .03f, 0);
  float jarakAkses = jalur.JarakTerdekat(kantin.position);
  float jarakJaga = Mathf.Clamp(jarakAkses + (jarakAkses > jalur.Panjang * .5f ? -2f : 2f), 0, jalur.Panjang);
  jalur.Sampel(jarakJaga, out var posisiKantin, out _, out float yaw);
  posisiKantin += Vector3.down * (ArthurJalurKampus.JarakKaki - .04f)
   + Quaternion.Euler(0, yaw, 0) * Vector3.forward * .55f;

  Transform Tandai(string namaLama, string nama, Vector3 posisi)
  {
   var induk = quizo.transform.parent;
   var titik = induk.Find(nama) ?? induk.Find(namaLama);
   if (titik == null)
   {
    titik = new GameObject(nama).transform;
    titik.SetParent(induk, false);
   }
   titik.name = nama;
   titik.position = posisi;
   return titik;
  }

  quizo.tujuanGA = masukGA;
  quizo.jalurKantin = jalur;
  quizo.posisiJaga = new[]
  {
   Tandai("Posisi jaga pintu Jasmine 1", "Posisi jaga Jalan Jasmine ke GA", posisiJalan),
   Tandai("Posisi jaga pintu Jasmine 2", "Posisi jaga Kantin ke GA", posisiKantin)
  };
  quizo.transform.position = posisiJalan;
  if (quizo.transform is RectTransform rect) rect.anchoredPosition3D = rect.localPosition;
  PrefabUtility.RecordPrefabInstancePropertyModifications(quizo.transform);
  EditorUtility.SetDirty(quizo);
 }
}
