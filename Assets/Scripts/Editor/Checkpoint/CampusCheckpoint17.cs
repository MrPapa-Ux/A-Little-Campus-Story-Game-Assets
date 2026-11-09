// Checkpoint 17: circular scan rhythm and audio derived from the user's local sound assets.
using System;
using System.Linq;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CampusCheckpoint17
{
    [MenuItem("Tools/A Little Campus Story/Checkpoint 17/Terapkan revisi")]
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Gunakan Edit Mode.");
        var game=UnityEngine.Object.FindFirstObjectByType<RhythmArthur>(FindObjectsInactive.Include);
        if(game==null)throw new InvalidOperationException("Buka scene CharacterLab terlebih dahulu.");
        foreach(string asset in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Resources/ArthurSfx","Assets/Resources/ButtonSfx"}))
        {
            var importer=AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(asset)) as AudioImporter;
            if(importer==null)continue;
            var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat=AudioCompressionFormat.PCM;settings.preloadAudioData=true;importer.defaultSampleSettings=settings;
            importer.forceToMono=true;importer.SaveAndReimport();
        }
        Undo.RegisterFullObjectHierarchyUndo(game.gameObject,"Checkpoint 17 - Rhythm dan audio");
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        foreach(var label in game.panel.GetComponentsInChildren<TMP_Text>(true))
        {
            label.font=font;label.fontStyle=FontStyles.Normal;label.textWrappingMode=TextWrappingModes.Normal;
            foreach(var wiggle in label.GetComponents<WigglyText>())wiggle.enabled=false;
            if(label.name=="JudulGame") {label.text="RHYTHM";label.fontSize=34;}
            else if(label.name=="Petunjuk") {label.text="Ketuk not saat garis melintasinya.";label.fontSize=24;}
            else label.fontSize=Mathf.Min(label.fontSize,32);
            EditorUtility.SetDirty(label);
        }
        game.judulMenu.text="RHYTHM";game.labelUtama.text="MULAI";
        game.isiMenu.text="Ketuk not bulat saat garis pemindai melintasinya.\nGaris bergerak naik dan turun mengikuti tempo.\n\nPenilaian: Perfect, Good, dan Miss.";
        game.hud.text="SKOR 0     COMBO 0     AKURASI 0%";
        game.penilaian.fontSize=34;game.judulMenu.fontSize=40;game.isiMenu.fontSize=27;
        game.papan.raycastTarget=true;game.papan.rectTransform.anchoredPosition=new Vector2(0,-20);
        game.papan.rectTransform.sizeDelta=new Vector2(1000,450);
        foreach(var lane in game.panel.GetComponentsInChildren<RhythmJalurInput>(true))lane.gameObject.SetActive(false);
        // Judgement is outside the tap area, so it cannot hide the scan line or cover a note.
        game.penilaian.rectTransform.anchoredPosition=new Vector2(0,205);
        game.audioBeat.volume=.6f;
        game.TerapkanTampilanPemindai();
        EditorUtility.SetDirty(game);EditorUtility.SetDirty(game.papan);
        var clips=Enumerable.Range(1,3).Select(i=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/ArthurSfx/Dialog"+i+".wav")).ToArray();
        if(clips.Any(c=>c==null))throw new InvalidOperationException("Audio checkpoint belum diimpor.");
        foreach(string guid in AssetDatabase.FindAssets("t:ProfilSuaraKertas",new[]{"Assets/AudioArthur"}))
        {
            var profile=AssetDatabase.LoadAssetAtPath<ProfilSuaraKertas>(AssetDatabase.GUIDToAssetPath(guid));
            profile.potongan=clips;profile.volume=.28f;profile.variasiNada=.06f;profile.jeda=.095f;
            EditorUtility.SetDirty(profile);
        }
        var playlist=AssetDatabase.LoadAssetAtPath<PlaylistArthur>("Assets/AudioArthur/Playlist_Arthur.asset");
        var loop=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/ArthurSfx/CampusLoop.wav");
        if(playlist!=null&&loop!=null)
        {
            int slot=Array.FindIndex(playlist.lagu,l=>l!=null&&l.judul=="Campus Loop");
            if(slot<0)slot=Array.FindIndex(playlist.lagu,l=>l==null||l.audio==null);
            if(slot>=0){playlist.lagu[slot]=new LaguArthur{judul="Campus Loop",audio=loop,catatan="Adaptasi ringan dari aset ChillLooping pengguna."};EditorUtility.SetDirty(playlist);}
        }
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
        EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();
        Directory.CreateDirectory("work/audio17");File.WriteAllText("work/audio17/applied.txt","Checkpoint 17 applied: circular notes, scan line, formal labels, adapted sound assets.\n"+DateTime.UtcNow.ToString("o"));
        Debug.Log("Checkpoint 17: revisi rhythm dan audio diterapkan.");
    }
}
