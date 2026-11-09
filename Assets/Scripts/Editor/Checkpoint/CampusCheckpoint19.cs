using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class CampusCheckpoint19
{
    [MenuItem("Tools/A Little Campus Story/Revisi 19/Perbaiki dialog dan konsistensi")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Hentikan Play untuk menerapkan perbaikan.");
        var dialog=UnityEngine.Object.FindFirstObjectByType<DialogManager>();
        if(dialog==null)throw new InvalidOperationException("Buka CharacterLab.");
        PlayerSettings.runInBackground=false;Application.runInBackground=false;
        var canvas=dialog.canvasDialog.GetComponent<Canvas>();var scaler=canvas.GetComponent<CanvasScaler>();
        Scale(scaler,new Vector2(1920,1080));
        var panel=(RectTransform)dialog.canvasDialog.transform.Find("PanelKotakDialog");
        panel.anchorMin=panel.anchorMax=new Vector2(.5f,.5f);panel.pivot=new Vector2(.5f,.5f);
        panel.anchoredPosition=new Vector2(0,180);panel.sizeDelta=new Vector2(1522,695);panel.localScale=Vector3.one;
        dialog.canvasDialog.GetComponent<EfekMunculDialog>().bidangAnimasi=panel;
        foreach(string name in new[]{"ObjPotretKIRIContainer","ObjPotretKANANContainer"})
        {
            var placeholder=panel.Find(name).GetComponent<UnityEngine.UI.Image>();
            if(placeholder!=null&&placeholder.sprite==null)placeholder.enabled=false;
        }
        foreach(var s in UnityEngine.Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(s.name=="UI_Gameplay"||s.name=="CanvasSistemHP")Scale(s,new Vector2(1920,1080));
            else if(s.name=="CanvasPengaturan")Scale(s,new Vector2(1280,720));
        var ui=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(c=>c.name=="UI_Gameplay").transform;
        Anchor((RectTransform)ui.Find("Btn_Interact"),new Vector2(1,0),new Vector2(-70,92));
        Anchor((RectTransform)ui.Find("Btn_HandPhone"),Vector2.one,new Vector2(-74,-65));
        var lazy=(RectTransform)ui.Find("LazyBar_Utama");lazy.anchorMin=lazy.anchorMax=new Vector2(0,1);lazy.anchoredPosition=new Vector2(960,-540);
        var music=UnityEngine.Object.FindFirstObjectByType<AplikasiMusikHP>(FindObjectsInactive.Include);
        foreach(var outline in music.GetComponentsInChildren<UnityEngine.UI.Outline>(true))
        {outline.effectDistance=new Vector2(2.5f,-2.5f);EditorUtility.SetDirty(outline);}
        foreach(string guid in AssetDatabase.FindAssets("t:ProfilSuaraKertas",new[]{"Assets/AudioArthur/SuaraKertas"}))
        {
            var profile=AssetDatabase.LoadAssetAtPath<ProfilSuaraKertas>(AssetDatabase.GUIDToAssetPath(guid));
            bool female=profile.namaKarakter=="Lyra"||profile.namaKarakter=="Nara";
            profile.potongan=Enumerable.Range(1,3).Select(i=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/ArthurSfx/Dialog"+(female?"Cewek":"Cowok")+i+".wav")).ToArray();
            if(profile.potongan.Any(c=>c==null))throw new InvalidOperationException("Suara teks belum diimpor.");
            profile.nada=female?(profile.namaKarakter=="Lyra"?1.06f:1f):(profile.namaKarakter=="Bimo"?.9f:1f);
            profile.variasiNada=.035f;profile.volume=.32f;profile.jeda=.045f;profile.hurufPerBunyi=1;EditorUtility.SetDirty(profile);
        }
        foreach(string guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Resources/ArthurSfx"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);if(!path.Contains("DialogCewek")&&!path.Contains("DialogCowok"))continue;
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;
            settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.preloadAudioData=true;
            importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
        }
        var run=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Referensi/Rayman_Reference/ArthurWalk.anim");
        var events=new System.Collections.Generic.List<AnimationEvent>();
        foreach(string foot in new[]{"KakiKiri","KakiKanan"})
        {
            var curve=AnimationUtility.GetEditorCurve(run,EditorCurveBinding.FloatCurve(foot,typeof(Transform),"m_LocalPosition.y"));
            float low=curve.keys.Min(k=>k.value),high=curve.keys.Max(k=>k.value),threshold=low+(high-low)*.15f;
            bool contact=false;float bestTime=0,best=float.MaxValue;
            for(int i=0;i<=Mathf.CeilToInt(run.length*240);i++)
            {
                float t=i/240f,y=curve.Evaluate(Mathf.Min(t,run.length));bool below=t<run.length&&y<threshold;
                if(below){if(!contact){contact=true;best=float.MaxValue;}if(y<best){best=y;bestTime=t;}}
                else if(contact){contact=false;events.Add(new AnimationEvent{functionName="PijakanLari",time=bestTime,messageOptions=SendMessageOptions.DontRequireReceiver});}
            }
        }
        AnimationUtility.SetAnimationEvents(run,events.OrderBy(e=>e.time).ToArray());EditorUtility.SetDirty(run);
        File.WriteAllText("work/bug19/run-contacts.txt",string.Join("\n",events.OrderBy(e=>e.time).Select(e=>e.time.ToString("F4"))));
        EditorSceneManager.MarkSceneDirty(dialog.gameObject.scene);EditorSceneManager.SaveScene(dialog.gameObject.scene);AssetDatabase.SaveAssets();
        File.WriteAllText("work/bug19/applied.txt",DateTime.UtcNow.ToString("o"));
    }
    static void Scale(CanvasScaler scaler,Vector2 size){scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=size;scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;EditorUtility.SetDirty(scaler);}
    static void Anchor(RectTransform rect,Vector2 anchor,Vector2 pos){rect.anchorMin=rect.anchorMax=rect.pivot=anchor;rect.anchoredPosition=pos;}
}
