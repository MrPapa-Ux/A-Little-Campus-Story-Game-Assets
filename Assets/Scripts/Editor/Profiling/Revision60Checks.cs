using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Pemeriksaan konfigurasi dan alat penyunting. Bukan benchmark HP fisik.
public static class Revision60Checks
{
    static readonly List<string> hasil = new List<string>();
    static void Cek(bool ok, string nama) { hasil.Add((ok ? "PASS " : "FAIL ") + nama); if (!ok) throw new Exception(nama); }
    public static void Jalankan()
    {
        string dir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "work/optimasi-60fps-20261108");
        Directory.CreateDirectory(dir);
        try
        {
            Cek(PerformaAndroidArthur.TargetFPS == 60, "Target Android 60 FPS");
            float min = PerformaAndroidArthur.SkalaUntuk(2400,1080,540), max = PerformaAndroidArthur.HitungSkala(2400,1080);
            Cek(Mathf.Approximately(min,.5f) && Mathf.Approximately(max,2f/3f), "Batas resolusi 540-720p pada layar 1080p");
            Cek(Mathf.Approximately(PerformaAndroidArthur.HitungSkala(1080,2400),max), "Rotasi tidak mengubah ketajaman");
            int stabil = 0; float skala = .6f;
            for(int i=0;i<10;i++) skala = PerformaAndroidArthur.SesuaikanSkala(skala,min,max,26,ref stabil);
            Cek(Mathf.Approximately(skala,min), "Beban berat menurunkan skala sampai batas aman");
            for(int i=0;i<40;i++) skala = PerformaAndroidArthur.SesuaikanSkala(skala,min,max,16.67f,ref stabil);
            Cek(Mathf.Approximately(skala,max), "Skala pulih bertahap saat stabil");
            stabil=0;skala=.6f;
            for(int i=0;i<3;i++) skala=PerformaAndroidArthur.SesuaikanSkala(skala,min,max,16.67f,ref stabil);
            Cek(Mathf.Approximately(skala,.6f), "Histeresis mencegah perubahan kualitas setiap frame");
            var rp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
            Cek(rp!=null&&!rp.supportsHDR&&rp.shadowDistance==32&&rp.useSRPBatcher,"Profil mobile HDR off, bayangan 32m, SRP batcher aktif");

            // Uji scene tambahan tanpa menimpa data pemilik.
            var main=SceneManager.GetActiveScene();
            var ga=PanelWorldKampus.Tampilkan(1,false);
            Cek(ga.isLoaded && main.isLoaded && SceneManager.GetActiveScene()==main,"Tampilkan GA tanpa mengganti scene utama");
            Cek(PanelWorldKampus.Tampilkan(1,false)==ga,"Tampilkan ulang tidak menggandakan kawasan");
            var p=new GameObject("UjiIndukTidakAktif");var c=new GameObject("UjiPrefabTerpilih");c.transform.SetParent(p.transform);p.SetActive(false);c.SetActive(false);
            Selection.activeGameObject=c;PanelWorldKampus.AktifkanPilihan();
            Cek(c.activeInHierarchy,"Tombol aktivasi juga mengaktifkan induk");
            UnityEngine.Object.DestroyImmediate(p);

            var daftar=new List<string>();
            foreach(var path in new[]{"Assets/WorldStreaming/Kawasan_JasmineGK2.unity","Assets/WorldStreaming/Kawasan_GA.unity","Assets/WorldStreaming/Kawasan_TamanUNKLAB.unity","Assets/WorldStreaming/Kawasan_GK1.unity"})
            {
                var s=SceneManager.GetSceneByPath(path); bool sudahTerbuka=s.isLoaded;
                if(!sudahTerbuka)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
                var rs=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true)).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
                long triangles=0;foreach(var r in rs){var f=r.GetComponent<MeshFilter>();var mesh=f!=null?f.sharedMesh:null;if(mesh!=null)for(int j=0;j<mesh.subMeshCount;j++)triangles+=(long)mesh.GetIndexCount(j)/3;}
                daftar.Add(s.name+": renderer="+rs.Length+", triangles total kawasan="+triangles+", material unik="+rs.SelectMany(r=>r.sharedMaterials).Where(m=>m!=null).Distinct().Count());
                if(!sudahTerbuka)EditorSceneManager.CloseScene(s,true);
            }
            File.WriteAllLines(Path.Combine(dir,"audit-geometri.txt"),daftar);
            SceneManager.SetActiveScene(main);
            PanelWorldKampus.Buka();
            File.WriteAllLines(Path.Combine(dir,"checks.txt"),hasil);
            Debug.Log("Revision60Checks selesai: "+hasil.Count+" pemeriksaan lulus.");
            if(Application.isBatchMode)EditorApplication.Exit(0);
        }
        catch(Exception e)
        {
            File.WriteAllLines(Path.Combine(dir,"checks.txt"),hasil.Concat(new[]{e.ToString()}));Debug.LogException(e);
            if(Application.isBatchMode)EditorApplication.Exit(1);
        }
    }
    public static void BukaPenataanGA()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity",OpenSceneMode.Single);
        PanelWorldKampus.Tampilkan(1,false);PanelWorldKampus.Buka();
        var n=UnityEngine.Object.FindObjectsByType<NpcYangBisaDiatur>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(x=>x.namaNpc=="Glen");
        if(n!=null){Selection.activeGameObject=n.gameObject;PanelWorldKampus.TampilkanPilihan();}
    }
}

