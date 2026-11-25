using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class PasangCutsceneLanjutan20261123
{
    const string Folder="Assets/Chapter1Preview/CutsceneLanjutan";
    static GameObject Buat(string nama,string[] file,string[] narasi){
        var root=PrefabUtility.LoadPrefabContents("Assets/Chapter1Preview/RefleksiDining/RefleksiDining.prefab");
        try{
            root.name=nama;
            var p=root.GetComponent<KenanganPembukaArthur>();
            p.laguKenangan=null;
            p.GetComponent<Canvas>().sortingOrder=30000;
            p.adegan=file.Select((f,i)=>new KenanganPembukaArthur.Adegan{narasi=narasi[i],lamaTampil=4,bagianGambar=new Rect(0,0,1,1),gambarTerpisah=AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/"+f)}).ToArray();
            if(p.adegan.Any(x=>x.gambarTerpisah==null))throw new Exception("Gambar cutscene belum ditemukan");
            p.TerapkanPanel(p.adegan[0]);
            return PrefabUtility.SaveAsPrefabAsset(root,Folder+"/"+nama+".prefab");
        }finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    public static string Run(){
        if(EditorApplication.isPlaying)throw new Exception("Hanya Edit Mode");
        foreach(var f in Directory.GetFiles(Folder,"*.png")){
            var i=(TextureImporter)AssetImporter.GetAtPath(f.Replace('\\','/'));
            i.textureType=TextureImporterType.Default;i.mipmapEnabled=false;i.npotScale=TextureImporterNPOTScale.None;i.maxTextureSize=2048;i.wrapMode=TextureWrapMode.Clamp;i.SaveAndReimport();
        }
        var dandi=Buat("RefleksiDandi",new[]{"05-Aku-Salah-Lagi.png","06-Closeup-Sedih.png"},new[]{"Aku salah lagi.",""});
        var pagi=Buat("PagiSebelumPenutup",new[]{"01-Sudah-Pagi.png","02-Laptop.png","03-Senyum.png","04-Saatnya-Keluar.png"},new[]{"Sudah pagi...","","","Saatnya keluar."});
        var setup=EditorSceneManager.GetSceneManagerSetup();
        var laporan="";
        try{foreach(var path in new[]{"Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity","Assets/Scenes/Penataan Kampus - Jalur NPC dan Bubble.unity"}){
            var backup="work/cutscene-revisi-20261123/"+Path.GetFileNameWithoutExtension(path)+"-sebelum-integrasi.unity";
            if(!File.Exists(backup))File.Copy(path,backup);
            var s=EditorSceneManager.OpenScene(path);
            var semua=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MonoBehaviour>(true)).Where(x=>x!=null).ToArray();
            foreach(var d in semua.OfType<PerjalananDandiChapter1>()){d.prefabRefleksi=dandi;EditorUtility.SetDirty(d);}
            foreach(var p in semua.OfType<PenutupChapter1Arthur>()){p.prefabPagi=pagi;EditorUtility.SetDirty(p);}
            foreach(var door in semua.OfType<InteraksiJalurKampus>().Where(x=>x.name=="GK1_Lantai3_Kiri_Bubble")){
                door.jalur.Sampel(door.jalur.JarakTerdekat(door.transform.position),out var pt,out _,out _);door.transform.position=pt;
            }
            var cerita=semua.OfType<CeritaKamar201Arthur>().FirstOrDefault();
            if(cerita!=null){
                var hp=semua.OfType<SistemHP>().FirstOrDefault();
                var ikon=hp!=null&&hp.tombolIkonHP!=null?hp.tombolIkonHP.GetComponent<Image>():null;
                var dm=semua.OfType<DialogManager>().FirstOrDefault();
                Transform parent=ikon!=null?ikon.transform.parent:dm!=null&&dm.hudSaatPercakapan!=null?dm.hudSaatPercakapan.FirstOrDefault(x=>x!=null)?.transform:null;
                if(parent!=null){
                    var old=parent.Find("MainQuestArthur");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
                    var go=new GameObject("MainQuestArthur",typeof(RectTransform),typeof(TextMeshProUGUI),typeof(PetunjukQuestUtamaArthur));go.transform.SetParent(parent,false);
                    var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(1,1);r.pivot=new Vector2(1,1);r.anchoredPosition=new Vector2(-145,-105);r.sizeDelta=new Vector2(370,120);
                    var t=go.GetComponent<TextMeshProUGUI>();t.font=semua.OfType<SistemPilihan>().FirstOrDefault()?.teksCeritaUtama.font;t.color=new Color(.8f,.08f,.05f);t.fontSize=27;t.enableAutoSizing=true;t.fontSizeMin=22;t.fontSizeMax=27;t.alignment=TextAlignmentOptions.TopRight;t.raycastTarget=false;
                    var q=go.GetComponent<PetunjukQuestUtamaArthur>();q.buku=cerita.buku;q.teks=t;
                }
            }
            EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);
            laporan+=path+": Dandi="+semua.OfType<PerjalananDandiChapter1>().Count()+", Penutup="+semua.OfType<PenutupChapter1Arthur>().Count()+"\n";
        }}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        AssetDatabase.SaveAssets();File.WriteAllText("work/cutscene-revisi-20261123/hasil-integrasi.txt",laporan);return laporan;
    }
}
