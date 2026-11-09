using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CampusCheckpoint18
{
    [MenuItem("Tools/A Little Campus Story/Revisi 18/Terapkan tap dan audio")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit Mode diperlukan.");
        var game=UnityEngine.Object.FindFirstObjectByType<RhythmArthur>(FindObjectsInactive.Include);
        if(game==null)throw new InvalidOperationException("Buka scene CharacterLab terlebih dahulu.");
        foreach(string guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Resources/ArthurSfx","Assets/Resources/ButtonSfx"}))
        {
            var importer=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as AudioImporter;
            if(importer==null)continue;var settings=importer.defaultSampleSettings;
            settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.preloadAudioData=true;
            importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
        }
        var voices=Enumerable.Range(1,3).Select(i=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/ArthurSfx/Dialog"+i+".wav")).ToArray();
        foreach(string guid in AssetDatabase.FindAssets("t:ProfilSuaraKertas",new[]{"Assets/AudioArthur"}))
        {
            var profile=AssetDatabase.LoadAssetAtPath<ProfilSuaraKertas>(AssetDatabase.GUIDToAssetPath(guid));
            profile.potongan=voices;profile.volume=.28f;profile.variasiNada=.06f;profile.jeda=.095f;EditorUtility.SetDirty(profile);
        }
        game.TerapkanTampilanPemindai();game.PilihLevel(1);
        var song=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AudioArthur/DemoOrisinal/Jam_Pulang_Campus_Groove.wav");
        if(song==null)throw new InvalidOperationException("Lagu baru belum diimpor.");
        var playlist=AssetDatabase.LoadAssetAtPath<PlaylistArthur>("Assets/AudioArthur/Playlist_Arthur.asset");
        playlist.lagu[0]=new LaguArthur{judul="Jam Pulang - Campus Groove",audio=song,catatan="Komposisi baru: soul-pop instrumental, 112 BPM. Melodi dan aransemen baru; tanpa sampel lagu referensi."};
        playlist.lagu[1]=new LaguArthur{judul="Jam Pulang - Instrumental",audio=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/ArthurSfx/CampusLoop.wav"),catatan="Bagian instrumental dari komposisi Jam Pulang."};
        for(int i=2;i<playlist.lagu.Length;i++)
            if(playlist.lagu[i]?.judul=="Campus Loop"&&playlist.lagu[i].audio==playlist.lagu[1].audio)
                playlist.lagu[i]=new LaguArthur{judul="Arthur - "+(i+1).ToString("00")};
        foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            var serialized=new SerializedObject(source);var resource=serialized.FindProperty("m_Resource");
            var old=source.clip??resource?.objectReferenceValue as AudioClip;
            if(old==null||!old.name.Contains("Heartbeat"))continue;
            source.clip=song;EditorUtility.SetDirty(source);
        }
        foreach(var label in UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(label.text.Contains("Heartbeat, Heartbreak")){label.text=label.text.Replace("Heartbeat, Heartbreak","Jam Pulang - Campus Groove");EditorUtility.SetDirty(label);}
        EditorUtility.SetDirty(playlist);EditorUtility.SetDirty(game);EditorUtility.SetDirty(game.papan);
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();
        Directory.CreateDirectory("work/audio18");File.WriteAllText("work/audio18/applied.txt","Tap visible circles; no scan line; ordered difficulty; walking silent; concrete running; original album music and UI audio.\n"+DateTime.UtcNow.ToString("o"));
    }
}
