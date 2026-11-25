using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class RevisiNarasiDanKelas
{
    static readonly string Work=KelasReferensiVideo.Work;
    public static void Run()
    {
        Directory.CreateDirectory(Work+"cadangan");var results=new List<string>();
        string scriptId=AssetDatabase.AssetPathToGUID("Assets/Skrip/Runtime/AlurDanMenu/PratinjauArthurMengetik.cs");
        foreach(var path in AssetDatabase.FindAssets("t:Scene",new[]{"Assets"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>File.ReadAllText(p).Contains(scriptId)))
        {
            var scene=EditorSceneManager.OpenScene(path);string copy=Work+"cadangan/"+Path.GetFileName(path);if(!File.Exists(copy))EditorSceneManager.SaveScene(scene,copy,true);
            foreach(var typing in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<PratinjauArthurMengetik>(true))){typing.jedaSetelahKoma=.5f;EditorUtility.SetDirty(typing);}
            EditorSceneManager.SaveScene(scene);results.Add("PASS jeda titik/koma0.5: "+path);
        }
        var transition=EditorSceneManager.OpenScene("Assets/Scenes/Transisi_Arthur.unity");var memory=Object.FindFirstObjectByType<KenanganPembukaArthur>();
        if(memory==null||memory.adegan.Length!=8)throw new Exception("Urutan kenangan pembuka hilang");
        if(!File.Exists(Work+"cadangan/Transisi_Arthur.unity"))EditorSceneManager.SaveScene(transition,Work+"cadangan/Transisi_Arthur.unity",true);
        var imageImporter=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(memory.gambar.texture));
        imageImporter.npotScale=TextureImporterNPOTScale.None;imageImporter.maxTextureSize=2048;imageImporter.textureCompression=TextureImporterCompression.Uncompressed;
        imageImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=2048,format=TextureImporterFormat.RGBA32});imageImporter.SaveAndReimport();
        memory.gambar.rectTransform.localScale=Vector3.one;
        var holder=memory.gambar.transform.parent.GetComponent<RectTransform>();holder.localScale=Vector3.one;holder.anchorMin=new Vector2(.08f,.38f);holder.anchorMax=new Vector2(.92f,.96f);holder.offsetMin=holder.offsetMax=Vector2.zero;
        var caption=memory.teks.rectTransform;caption.localScale=Vector3.one;caption.anchorMin=new Vector2(.08f,.04f);caption.anchorMax=new Vector2(.92f,.33f);caption.offsetMin=caption.offsetMax=Vector2.zero;
        memory.teks.enableAutoSizing=true;memory.teks.fontSize=48;memory.teks.fontSizeMin=30;memory.teks.fontSizeMax=48;memory.teks.characterSpacing=memory.teks.wordSpacing=0;memory.teks.lineSpacing=6;memory.teks.paragraphSpacing=4;memory.teks.alignment=TMPro.TextAlignmentOptions.MidlineLeft;
        memory.TerapkanPanel(memory.adegan[0]);
        results.Add("PASS rasio gambar mengikuti piksel sumber, NPOT tidak dipaksa persegi; teks skala1:1 dan autosize30–48.");
        float totalBefore=0;
        foreach(var entry in memory.adegan){string text=entry.narasi??"";float old=Mathf.Max(entry.lamaTampil,text.Length/Mathf.Max(1,memory.hurufPerDetik)+text.Count(c=>c==','||c=='?')*memory.jedaTandaBaca+2);if(Mathf.Abs(memory.DurasiTahan(entry)-old)>.0001f)throw new Exception("Durasi berubah "+entry.narasi);totalBefore+=old+memory.fadeMasuk+memory.fadeKeluar;results.Add("PASS durasi gambar="+(old+memory.fadeMasuk+memory.fadeKeluar).ToString("F4",System.Globalization.CultureInfo.InvariantCulture));}
        if(Mathf.Abs(totalBefore-memory.DurasiTotal)>.001f)throw new Exception("Total durasi berubah");
        results.Add("PASS total kenangan tetap="+totalBefore.ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+" detik; musik="+memory.laguKenangan.name);
        memory.durasiSudahDisetujui=true;EditorUtility.SetDirty(memory);EditorSceneManager.SaveScene(transition);
        // Pengujian batas jeda: 10huruf/detik, A terbit0.1; titik0.2; B baru0.8.
        foreach(string sample in new[]{"A.B","A,B"}){
            if(PratinjauArthurMengetik.HitungHurufTerlihat(sample,.699f,10,99,1.5f)!=2||PratinjauArthurMengetik.HitungHurufTerlihat(sample,.801f,10,99,1.5f)!=3)throw new Exception("Jeda singkat bukan0.5: "+sample);
        }
        results.Add("PASS jeda titik/koma0.5 pada batas waktu, termasuk nilai serialized lama yang berbeda.");
        KelasReferensiVideo.Build();
        File.WriteAllLines(Work+"checks.txt",results);AssetDatabase.SaveAssets();
        Debug.Log("NARASI_KELAS_REVISI_OK");
    }
    public static void Render(){KelasReferensiVideo.Render();}
    public static void RenderFlashback()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Transisi_Arthur.unity");var memory=Object.FindFirstObjectByType<KenanganPembukaArthur>();var canvas=memory.GetComponent<Canvas>();
        foreach(var other in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(other!=canvas)other.enabled=false;
        foreach(var oldCamera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))oldCamera.enabled=false;
        var camera=new GameObject("Kamera uji rasio flashback").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.nearClipPlane=.01f;camera.farClipPlane=20;
        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
        memory.areaAman.anchorMin=Vector2.zero;memory.areaAman.anchorMax=Vector2.one;memory.areaAman.offsetMin=memory.areaAman.offsetMax=Vector2.zero;
        memory.kelompok.gameObject.SetActive(true);memory.kelompok.alpha=1;memory.teks.alpha=1;
        var log=new List<string>();
        foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(2412,1080)}){
            var rt=RenderTexture.GetTemporary(size.x,size.y,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;
            foreach(var entry in memory.adegan){
                memory.TerapkanPanel(entry);memory.teks.text=entry.narasi;Canvas.ForceUpdateCanvases();memory.teks.ForceMeshUpdate(true,true);
                var rect=memory.gambar.rectTransform.rect;float expected=memory.gambar.texture.width*entry.bagianGambar.width/(memory.gambar.texture.height*entry.bagianGambar.height);
                if(Mathf.Abs(rect.width/rect.height-expected)>.005f)throw new Exception("Gambar tertarik pada rasio "+size);
                if(memory.teks.isTextOverflowing)throw new Exception("Narasi terpotong "+size+" "+entry.narasi);
            }
            memory.TerapkanPanel(memory.adegan[6]);memory.teks.text=memory.adegan[6].narasi;Canvas.ForceUpdateCanvases();memory.teks.ForceMeshUpdate(true,true);
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=rt});
            var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();File.WriteAllBytes(Work+"flashback-"+size.x+"x"+size.y+".png",image.EncodeToPNG());Object.DestroyImmediate(image);RenderTexture.active=old;camera.targetTexture=null;RenderTexture.ReleaseTemporary(rt);
            log.Add("PASS "+size+":8panel rasio asli,8narasi utuh; font="+memory.teks.fontSize);
        }
        File.WriteAllLines(Work+"layout-checks.txt",log);
        EditorSceneManager.OpenScene("Assets/Scenes/Explorer.unity");
    }
}
