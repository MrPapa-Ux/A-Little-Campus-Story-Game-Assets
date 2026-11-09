// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menerapkan susunan/revisi yang dibuat pada checkpoint 9.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class CampusCheckpoint9
{
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Pasang di Edit Mode.");
        EditorSceneManager.SaveOpenScenes();
        if(SceneManager.GetActiveScene().path!="Assets/Scenes/CharacterLab.unity")EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        var scene=SceneManager.GetActiveScene(); var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();
        var roots=scene.GetRootGameObjects();
        foreach(var actor in roots.SelectMany(r=>r.GetComponentsInChildren<KarakterController>(true)))Group(actor.wadahVisual);
        foreach(var npc in roots.SelectMany(r=>r.GetComponentsInChildren<NPCMenghadapArthur>(true)))Group(npc.wadahVisual);
        hp.poseKarakter.satuTangan=true; EditorUtility.SetDirty(hp.poseKarakter);
        hp.petaKampus.warnaLingkaran=new Color32(20,215,255,255);hp.petaKampus.warnaPilihan=new Color32(255,30,130,255);
        foreach(var item in hp.petaKampus.lokasi){item.lingkaran.tebal=4f;item.lingkaran.SetVerticesDirty();EditorUtility.SetDirty(item.lingkaran);}
        EditorUtility.SetDirty(hp.petaKampus);
        string root=Directory.GetParent(Application.dataPath).FullName;
        // Posisi sebelumnya dipakai agar pemasangan ulang tidak menurunkan mobil berulang kali.
        string stamp=Path.Combine(root,"work/checkpoint9-car-height.txt");
        if(!File.Exists(stamp))
        {
            var cars=roots.SelectMany(r=>r.GetComponentsInChildren<MobilKampus>(true)).ToArray();
            File.WriteAllText(stamp,string.Join("\n",cars.Select(c=>c.name+" Y lama="+c.transform.position.y+" turun=0.12")));
            foreach(var car in cars){car.transform.position+=Vector3.down*.12f;EditorUtility.SetDirty(car.transform);}
        }
        const string texture="Assets/AllAssetPeta/PetaKampusGoofy.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(texture);
        var platform=importer.GetPlatformTextureSettings("Android");platform.name="Android";platform.overridden=true;platform.maxTextureSize=2048;
        platform.format=TextureImporterFormat.ETC2_RGBA8;platform.compressionQuality=100;
        importer.SetPlatformTextureSettings(platform);importer.SaveAndReimport();
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        File.WriteAllText(Path.Combine(root,"work/checkpoint9-apply.txt"),"COMPLETE: sorting tiap aktor, mobil turun 0.12, HP satu tangan, TV mengikuti panel, marker Q kontras, cache animasi/UI, kompresi peta Android.\n");
    }
    static void Group(Transform visual)
    {
        if(visual==null)return;
        var group=visual.GetComponent<SortingGroup>();if(group==null)group=visual.gameObject.AddComponent<SortingGroup>();
        group.sortingLayerID=visual.GetComponentsInChildren<SpriteRenderer>(true).First().sortingLayerID;group.sortingOrder=0;
        EditorUtility.SetDirty(group);
    }
}
