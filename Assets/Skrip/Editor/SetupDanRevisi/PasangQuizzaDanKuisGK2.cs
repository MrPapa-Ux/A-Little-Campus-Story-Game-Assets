using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Penambahan terpisah: tidak menjalankan ulang pemasangan NPC lama atau mengubah susunan manual.
public static class PasangQuizzaDanKuisGK2
{
    const string Panduan = "https://www.unklab.ac.id/wp-content/uploads/2024/08/BUKU-PANDUAN-UNKLAB-2024.pdf";
    static MrQuizoChapter1.Soal S(string t, string j, string[] p, string e, string sumber) =>
        new MrQuizoChapter1.Soal { pertanyaan=t, jawaban=j, pengecoh=p, penjelasan=e,
            komentarSalah="SKS bukan skor yang naik karena asal tekan, Arthur.", sumber=sumber };
    public static MrQuizoChapter1.Soal[] BankQuizza() => new[] {
        S("Menurut Buku Panduan UNKLAB 2024, berapa batas SKS semester pertama dan kedua untuk Diploma/S1?", "20 SKS", new[]{"24 SKS","28 SKS","30 SKS"}, "Batas semester 1–2 adalah 20 SKS.", Panduan),
        S("Menurut panduan UNKLAB 2024, berapa batas atas beban semester reguler mulai semester ketiga?", "24 SKS", new[]{"20 SKS","26 SKS","30 SKS"}, "Batas atasnya 24 SKS; izin beban individu tetap mengikuti syarat akademik.", Panduan),
        S("Menurut panduan UNKLAB 2024, berapa maksimum normal pada semester padat?", "6 SKS", new[]{"9 SKS","12 SKS","20 SKS"}, "Beban normal semester padat maksimal 6 SKS.", Panduan),
        S("Dalam panduan UNKLAB 2024, mahasiswa ber-IPK minimal 3,25 boleh mengambil paling banyak berapa SKS semester padat?", "7 SKS", new[]{"6 SKS","10 SKS","24 SKS"}, "Untuk IPK minimal 3,25, batas semester padat adalah 7 SKS.", Panduan),
        S("Menurut panduan UNKLAB 2024, satu tahun akademik terdiri dari apa?", "2 semester reguler dan 1 padat", new[]{"3 semester reguler","1 reguler dan 2 padat","2 semester padat saja"}, "Terdapat dua semester reguler dan satu semester padat.", Panduan),
        S("Menurut panduan UNKLAB 2024, berapa IPK minimum untuk izin tambahan satu mata kuliah di atas beban normal 20 SKS?", "3,25", new[]{"2,00","2,50","3,00"}, "IPK minimal 3,25; tambahan tetap memerlukan izin sesuai ketentuan.", Panduan)
    };
    public static MrQuizoChapter1.Soal[] BankGK2() => new[] {
        S("Di HP Arthur, fitur mana yang dipakai untuk membaca pesan permintaan bantuan?", "Kotak Masuk",new[]{"Pemutar musik","Peta","Galeri"},"Permintaan bantuan dapat dibaca dan dibalas dari Kotak Masuk.","Desain game: SistemHP dan QuestKontakArthur"),
        S("Apa yang perlu dilakukan setelah menerima permintaan bantuan lewat kontak?", "Baca tujuan lalu bantu NPC terkait",new[]{"Tutup HP dan anggap selesai","Ganti lagu sampai selesai","Hapus semua kontak"},"Menerima pesan belum menyelesaikan tugasnya. Kerjakan tujuan yang diberikan.","Desain game: QuestKontakArthur"),
        S("Bagaimana cara memeriksa hadiah item yang sudah dikumpulkan Arthur?", "Buka tas di HP",new[]{"Tekan jam berkali-kali","Lihat atap gedung","Keluar dari permainan"},"Tas menyimpan item yang telah diperoleh.","Desain game: BukuArthur dan SistemHP"),
        S("Jika arah tujuan terasa membingungkan, fitur mana yang sebaiknya diperiksa?", "Peta kampus",new[]{"Volume musik","Menu keluar","Kecerahan layar"},"Periksa peta dan penunjuk tujuan sebelum berputar lagi.","Desain game: SistemHP"),
        S("Saat ada tanda misterius yang meminta semua item Chapter, apa langkah yang masuk akal?", "Selesaikan tugas dan kumpulkan item",new[]{"Tekan tandanya seratus kali","Tunggu tanpa melakukan apa pun","Ganti nama Arthur"},"Hadiah dari tugas dan tantangan membantu melengkapi koleksi.","Desain game: AksesArenaKoleksiArthur")
    };

    // Pemasangan historis; menu dinonaktifkan agar titik GK2 lama tidak terpasang kembali.
    public static void Run()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new Exception("Hentikan Play dahulu.");
        var awal=SceneManager.GetActiveScene();
        var laporan=new System.Collections.Generic.List<string>();
        foreach(var nama in new[]{"Penataan Kampus - Jalur NPC dan Bubble","Explorer","CharacterLab"})
        {
            string path="Assets/Scenes/"+nama+".unity";
            var scene=SceneManager.GetSceneByPath(path); bool loaded=scene.isLoaded;
            if(!loaded)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
            var lama=all.Select(t=>t.GetComponent<MrQuizoChapter1>()).FirstOrDefault(q=>q!=null&&q.kuisSebelumnya==null);
            if(lama==null){laporan.Add(nama+": pratinjau tanpa kuis Chapter 1, tidak diubah");if(!loaded)EditorSceneManager.CloseScene(scene,true);continue;}
            string backup="../work/quizo-quizza-20261126/"+nama+"-sebelum.unity";
            if(!File.Exists(backup))EditorSceneManager.SaveScene(scene,backup,true);
            Transform Cari(string n)=>all.Single(t=>t.name==n);
            var sari=Cari("Sari - Keperawatan");
            MrQuizoChapter1 Buat(string rootName,GameObject template,string label,string kawasan)
            {
                var root=scene.GetRootGameObjects().FirstOrDefault(g=>g.name==rootName);
                if(root==null){root=new GameObject(rootName);root.AddComponent<KelompokNpcManual>().kawasan=kawasan;}
                var q=root.GetComponentInChildren<MrQuizoChapter1>(true);
                if(q==null){var go=UnityEngine.Object.Instantiate(template,root.transform);go.name=label;q=go.GetComponent<MrQuizoChapter1>()??go.AddComponent<MrQuizoChapter1>();}
                var materi=q.GetComponent<MateriKuliahNpc>();if(materi!=null)UnityEngine.Object.DestroyImmediate(materi);
                var n=q.GetComponent<NpcYangBisaDiatur>();n.namaNpc=label;n.dialogSederhana=false;n.jarakInteraksi=2.8f;
                q.namaPenjaga=label;q.jalurKantin=null;q.jumlahSoal=5;
                var rig=q.GetComponent<NpcKampusHidup>();if(rig!=null)rig.kegiatan=KegiatanNpcKampus.Berdiri;
                return q;
            }
            TujuanJalurKampus Titik(MrQuizoChapter1 q,string n,string route,Vector3 posisi)
            {
                var t=q.transform.parent.Find(n);if(t==null){t=new GameObject(n).transform;t.SetParent(q.transform.parent,false);}
                t.position=posisi;var tujuan=t.GetComponent<TujuanJalurKampus>()??t.gameObject.AddComponent<TujuanJalurKampus>();tujuan.jalur=Cari(route).GetComponent<JalurKampus>();return tujuan;
            }
            void SelesaiAtur(MrQuizoChapter1 q)
            {
                q.transform.position=q.posisiJaga[0].position;
                q.GetComponent<NpcYangBisaDiatur>().jalurInteraksi=q.batasJalan[0].jalur;
                var a=q.GetComponent<AreaInteraksi>();a.idTempat=q.name;a.tipeInteraksi=TipeInteraksi.NPC_Karakter;
                a.skenarioDialog=new[]{new BarisDialog{siapaYangBicara=Pembicara.Kanan_NPC,namaKarakter=q.namaPenjaga,teksDialog=q.sapaan}};
                a.daftarOpsi=new[]{new OpsiInteraksi{teksOpsi="Siap, tes aku!",mrQuizo=q,aksiQuizo=-1},new OpsiInteraksi{teksOpsi="Nanti dulu.",mrQuizo=q,aksiQuizo=-3}};
                foreach(var s in q.bankSoal)if(s.pengecoh.Length!=3||s.pengecoh.Contains(s.jawaban)||s.pengecoh.Distinct().Count()!=3)throw new Exception("Opsi ganda: "+s.pertanyaan);
            }
            var gk=Buat("NPC Kuis Lanjutan - Jalur Bawah GK2",lama.gameObject,"Mr. Quizzo","JasmineGK2");
            gk.kuisSebelumnya=lama;gk.bankSoal=BankGK2();gk.tujuanGA=Cari("TitikMasuk_GA");
            gk.sapaan="Sudah lewat kuis pertama? Sekarang kita cek apakah HP-mu dipakai untuk hal selain ganti lagu.";
            gk.pintuKeluar=new[]{"GK2_Atas_KeGA","GK2_Belakang_KeGA","GK2_Ujung_KeGA"}.Select(n=>Cari(n).GetComponent<AreaInteraksi>()).ToArray();
            var p=Titik(gk,"Posisi kuis jalur bawah GK2","GK2_JalurUtama_BelokOtomatis",new Vector3(7.3f,-.54f-ArthurJalurKampus.JarakKaki,-36f));
            gk.batasJalan=new[]{p};gk.posisiJaga=new[]{p.transform};SelesaiAtur(gk);
            var ga=Buat("NPC Mrs. Quizza - Dua Akses GA",sari.gameObject,"Mrs. Quizza","GA");
            ga.kuisSebelumnya=gk;ga.bankSoal=BankQuizza();ga.tujuanGA=null;
            ga.sapaan="Sebelum ke GK1, kita cek bekal akademikmu. Acuan kuis ini Buku Panduan UNKLAB 2024, bukan pengganti arahan dosen wali.";
            ga.pintuKeluar=new[]{"GA_KeGedungKuliahSatu","GA_Taman_MenujuPioneer_Bubble"}.Select(n=>Cari(n).GetComponent<AreaInteraksi>()).ToArray();
            var p1=Titik(ga,"Posisi Mrs. Quizza - Lorong GA","GA_LantaiSatu",new Vector3(611.86f,2.04f,80));
            var p2=Titik(ga,"Posisi Mrs. Quizza - Jalan Depan GA","GA_JalanDepan",new Vector3(546.5f,3.76f-ArthurJalurKampus.JarakKaki,90.856f));
            ga.batasJalan=new[]{p1,p2};ga.posisiJaga=new[]{p1.transform,p2.transform};SelesaiAtur(ga);
            int arena=0;
            foreach(var a in all.Select(t=>t.GetComponent<AksesArenaKoleksiArthur>()).Where(a=>a!=null))
            {
                var area=a.GetComponent<AreaInteraksi>();
                foreach(var text in a.GetComponentsInChildren<TMPro.TMP_Text>(true))text.text="?";
                if(area!=null){area.skenarioDialog=new[]{new BarisDialog{siapaYangBicara=Pembicara.Tidak_Ada,teksDialog=AksesArenaKoleksiArthur.PetunjukKoleksi}};foreach(var o in area.daftarOpsi??Array.Empty<OpsiInteraksi>())if(o.arenaKoleksi!=null)o.teksOpsi="Periksa tanda ?";}
                arena++;
            }
            EditorSceneManager.MarkSceneDirty(scene);if(!EditorSceneManager.SaveScene(scene))throw new IOException("Gagal menyimpan "+nama);
            laporan.Add(nama+": 3 penjaga, 2 posisi Quizza, "+arena+" arena bertanda ?");
            if(!loaded)EditorSceneManager.CloseScene(scene,true);
        }
        SceneManager.SetActiveScene(awal);AssetDatabase.SaveAssets();
        File.WriteAllText("../work/quizo-quizza-20261126/hasil-pemasangan.txt",string.Join("\n",laporan));
        File.WriteAllText("../work/quizo-quizza-20261126/soal-dan-sumber.txt",string.Join("\n\n",BankGK2().Concat(BankQuizza()).Select(s=>s.pertanyaan+"\nJawaban: "+s.jawaban+"\nPenjelasan: "+s.penjelasan+"\nSumber: "+s.sumber)));
        Debug.Log(string.Join("\n",laporan));
    }
}
