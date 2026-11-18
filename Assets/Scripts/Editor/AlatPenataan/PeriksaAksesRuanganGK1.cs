using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PeriksaAksesRuanganGK1
{
    const string Folder="work/akses-kamera-universal/";
    static PeriksaAksesRuanganGK1(){EditorApplication.update+=Tick;}
    static void Tick()
    {
        if(!File.Exists(Folder+"periksa.request")||EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
        File.Delete(Folder+"periksa.request");
        var log=new List<string>();var original=SceneManager.GetActiveScene();
        try
        {
            foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Chapter1Preview/Chapter1_Kamar201.unity"})
            {
                var s=SceneManager.GetSceneByPath(path);bool loaded=s.isLoaded;
                if(!loaded)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
                try
                {
                    bool changed=false;
                    var markers=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<InteraksiJalurKampus>(true)).Where(m=>m.name.StartsWith("GK1_Masuk_Ruangan")).ToArray();
                    foreach(var m in markers)
                    {
                        var a=m.GetComponent<AreaInteraksi>();var dest=a.daftarOpsi?.FirstOrDefault()?.titikTujuan;
                        if(m.jalur==null||dest==null||dest.GetComponent<TujuanJalurKampus>()?.jalur==null)throw new Exception(path+": pintu tidak tersambung: "+m.name);
                        m.jalur.Sampel(m.jalur.JarakTerdekat(m.transform.position),out var p,out _,out _);
                        float gap=Vector3.Distance(p,m.transform.position);
                        if(gap>m.radius)
                        {
                            Directory.CreateDirectory(Folder+"cadangan");
                            string backup=Folder+"cadangan/"+Path.GetFileName(path);
                            if(!File.Exists(backup))EditorSceneManager.SaveScene(s,backup,true);
                            Undo.RecordObject(m.transform,"Dekatkan titik pintu ke jalur Arthur");
                            m.transform.position=p;
                            EditorUtility.SetDirty(m.transform);EditorSceneManager.MarkSceneDirty(s);changed=true;
                            log.Add("DIPERBAIKI: "+m.name+" dipusatkan pada jalur di depan pintu; selisih sebelumnya "+gap.ToString("F3")+" m.");
                            gap=0;
                        }
                        log.Add(path+" | "+m.name+" | aktif="+m.isActiveAndEnabled+" | jarak dari jalur="+gap.ToString("F3")+" | radius="+m.radius+" | tujuan="+dest.GetComponent<TujuanJalurKampus>().jalur.name);
                        if(gap>m.radius)throw new Exception("Pintu di luar jangkauan jalur: "+m.name);
                    }
                    if(markers.Length!=2)throw new Exception(path+": jumlah pintu "+markers.Length);
                    if(changed)EditorSceneManager.SaveScene(s);
                }
                finally{if(!loaded)EditorSceneManager.CloseScene(s,true);}
            }
            SceneManager.SetActiveScene(original);
            PanelWorldKampus.Buka();KameraSemuaJalurArthur.Pintu(false);
            log.Add("Panel World dan Prefab dibuka; Scene diarahkan ke pintu dekan. Kamera universal tersedia untuk semua jalur, kamera lama tidak ditimpa.");
        }
        catch(Exception e){log.Add(e.ToString());Debug.LogException(e);}
        File.WriteAllLines(Folder+"hasil.txt",log);
    }
}
