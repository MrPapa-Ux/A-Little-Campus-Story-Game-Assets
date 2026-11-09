using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BoFinanceRevision
{
    public static void Run()
    {
        Directory.CreateDirectory("work/bo-keuangan");
        try
        {
            if(EditorApplication.isPlaying || EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer) throw new Exception("Editor belum siap");
            for(int s=0;s<UnityEngine.SceneManagement.SceneManager.sceneCount;s++)
            {
                var opened=UnityEngine.SceneManagement.SceneManager.GetSceneAt(s);
                if(opened.isDirty) throw new Exception("Ada scene belum disimpan: "+opened.path);
            }
            foreach (var path in new[]{"Assets/Scenes/Explorer.unity", "Assets/Scenes/CharacterLab.unity", "Assets/Chapter1Preview/Chapter1_Kamar201.unity"})
            {
                var scene=EditorSceneManager.OpenScene(path);
                var route=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<JalurKampus>(true)).Single(j=>j.name=="GH_LantaiSatu");
                if(route.titik==null||route.titik.Length<2)throw new Exception("Jalur GH tidak lengkap");
                int best=0;float length=0;
                for(int i=0;i<route.titik.Length-1;i++)
                {
                    var a=route.titik[i].position;var b=route.titik[i+1].position;
                    float len=Vector3.Distance(a,b);
                    if(Mathf.Abs(a.y-b.y)<.15f&&len>length){best=i;length=len;}
                }
                if(length<2)throw new Exception("Tidak ditemukan segmen datar untuk Bo");
                string backup="work/bo-keuangan/"+scene.name+"-sebelum.unity";
                if(!File.Exists(backup))File.Copy(path,backup);
                var group=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="NPC_Bo_Keuangan_GH");
                if(group==null){group=new GameObject("NPC_Bo_Keuangan_GH");group.AddComponent<KelompokNpcManual>().kawasan="GH";}
                var n=group.GetComponentInChildren<NpcYangBisaDiatur>(true);
                if(n==null){var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(AlatNpcArthur.Prefab),scene);go.transform.SetParent(group.transform,true);n=go.GetComponent<NpcYangBisaDiatur>();}
                n.name="Bo — Keuangan UNKLAB — Jalur Luar Guest House";
                n.namaNpc="Bo";n.dialogSederhana=false;n.jalurInteraksi=route;n.jarakInteraksi=2.2f;
                n.transform.position=Vector3.Lerp(route.titik[best].position,route.titik[best+1].position,.5f);
                if(n.transform is RectTransform rect)rect.anchoredPosition3D=rect.localPosition;
                n.geserTitikInteraksi=Vector3.zero;
                var area=n.GetComponent<AreaInteraksi>();area.tipeInteraksi=TipeInteraksi.NPC_Karakter;
                string[] lines={
                    "Arthur, biaya kuliah tidak selalu sama setiap semester. Di UNKLAB ada biaya pendaftaran, SKS, dan biaya laboratorium untuk mata kuliah tertentu.",
                    "Kalau SKS yang diambil lebih sedikit, komponen biaya SKS bisa lebih kecil. Jadi, bukan otomatis makin tinggi semester, makin murah.",
                    "Sebaliknya, mengambil lebih banyak SKS atau mata kuliah dengan laboratorium dapat menambah biaya. Periksa rincian tagihan, bukan hanya totalnya.",
                    "Mahasiswa baru juga memiliki beberapa komponen awal yang hanya dibebankan saat pertama mendaftar. Itu dapat membuat perbandingan antarsemester berbeda.",
                    "Untuk nominal dan ketentuan terbaru, cek informasi resmi UNKLAB, akun SIU, atau bagian keuangan. Riwayat pembayaran satu mahasiswa bukan tarif untuk semua orang."
                };
                area.skenarioDialog=lines.Select(t=>new BarisDialog{namaKarakter="Bo",teksDialog=t,siapaYangBicara=Pembicara.Kanan_NPC,teksTombolLanjut="Lanjut"}).ToArray();
                area.daftarOpsi=Array.Empty<OpsiInteraksi>();area.tampilkanTombolKeluar=true;
                foreach(var c in n.GetComponentsInChildren<Collider>(true))if(!c.isTrigger)c.enabled=false;
                n.gameObject.SetActive(true);group.SetActive(true);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                File.AppendAllText("work/bo-keuangan/penempatan.txt",path+" | "+n.transform.position+" | segmen="+best+" panjang="+length+"\n");
            }
            AssetDatabase.SaveAssets();
            File.WriteAllText("work/bo-keuangan/build-status.txt","SETUP_SAVED");
        }
        catch(Exception e){File.WriteAllText("work/bo-keuangan/error.txt",e.ToString());Debug.LogException(e);if(Application.isBatchMode)EditorApplication.Exit(1);}
    }
    public static void Build()
    {
        try
        {
            System.Environment.SetEnvironmentVariable("PATH",@"C:\Windows\System32;C:\Windows;"+System.Environment.GetEnvironmentVariable("PATH"));
            System.Environment.SetEnvironmentVariable("JAVA_HOME",UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath);
            File.WriteAllText("work/bo-keuangan/build-status.txt","BUILDING");
            Directory.CreateDirectory("Builds/Android");
            PlayerSettings.productName="A Little Campus Story";
            EditorUserBuildSettings.buildAppBundle=false;
            string output=Path.GetFullPath("Builds/Android/A Little Campus Story - Bo.apk");
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=output,target=BuildTarget.Android,options=BuildOptions.None});
            File.WriteAllText("work/bo-keuangan/build-status.txt",result.summary.result+"\nBytes="+result.summary.totalSize+"\nErrors="+result.summary.totalErrors+"\n"+output+"\n"+string.Join("\n",result.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Exception).Select(m=>m.content)));
            if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Build gagal, baca log");
            if(Application.isBatchMode)EditorApplication.Exit(0);
        }
        catch(Exception e){File.WriteAllText("work/bo-keuangan/error.txt",e.ToString());Debug.LogException(e);if(Application.isBatchMode)EditorApplication.Exit(1);}
    }
}
