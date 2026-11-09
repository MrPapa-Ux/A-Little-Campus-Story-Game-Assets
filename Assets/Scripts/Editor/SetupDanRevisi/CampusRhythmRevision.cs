// PANDUAN PROYEK - Editor / SetupDanRevisi
// Fungsi: Membentuk percobaan rhythm dan sambungannya.
// Panduan: Assets/Scripts/Editor/SetupDanRevisi/README.md

using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CampusRhythmRevision
{
    static TMP_FontAsset font;
    static readonly Color Orange=new Color32(238,128,27,255),Paper=new Color32(240,224,182,255),Dark=new Color32(16,25,33,255);
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit Mode diperlukan.");
        var bridge=UnityEngine.Object.FindFirstObjectByType<RapidTapArthur>();if(bridge==null)throw new InvalidOperationException("Pintu masuk NPC belum ada.");
        if(bridge.GetComponent<RhythmArthur>()!=null)throw new InvalidOperationException("Rhythm sudah terpasang. Jangan menggandakan UI.");
        var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>();
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/AmaticSC-Regular SDF.asset");
        var g=bridge.gameObject.AddComponent<RhythmArthur>();g.dialog=bridge.dialog;g.lazyBar=bridge.lazyBar;g.pose=bridge.dialog.arthurScript.GetComponent<PoseMemegangHP>();
        g.audioBeat=bridge.gameObject.AddComponent<AudioSource>();g.audioBeat.playOnAwake=false;g.audioBeat.volume=.7f;
        var panel=Rect("RhythmDiHP",bridge.transform,Vector2.zero,Vector2.zero);panel.anchorMin=Vector2.zero;panel.anchorMax=Vector2.one;
        var shade=panel.gameObject.AddComponent<UnityEngine.UI.Image>();shade.color=new Color(0,0,0,.76f);shade.raycastTarget=true;g.panel=panel.gameObject;
        // Pakai tekstur HP yang sama, tampilkan area bingkai saja, lalu putar landscape.
        var frame=Rect("BingkaiHPArthur",panel,Vector2.zero,new Vector2(990,1800));frame.localRotation=Quaternion.Euler(0,0,90);
        var raw=frame.gameObject.AddComponent<UnityEngine.UI.RawImage>();raw.texture=hp.badanHP.sprite.texture;raw.uvRect=new Rect(.29f,.04f,.52f,.82f);raw.color=hp.warnaDefaultHP;raw.raycastTarget=false;
        var screen=Box("LayarGame",panel,new Vector2(-10,0),new Vector2(1080,680),Dark);
        Text("JudulGame",screen,"MULAI DULU",new Vector2(-145,298),new Vector2(710,65),49,Paper,true);
        g.waktu=Text("Timer",screen,"110 BPM",new Vector2(410,298),new Vector2(220,55),34,Orange);
        g.hud=Text("SkorComboFokus",screen,"RHYTHM / ARTHUR",new Vector2(0,244),new Vector2(1010,50),32,Paper);
        var board=Rect("EmpatJalurNot",screen,new Vector2(0,10),new Vector2(960,380));g.papan=board.gameObject.AddComponent<PapanRhythmArthur>();g.papan.game=g;g.papan.raycastTarget=false;
        for(int i=0;i<4;i++)
        {
            float x=(i-1.5f)*240;
            var lane=Box("KetukJalur_"+i,screen,new Vector2(x,-245),new Vector2(226,96),i%2==0?Orange:new Color32(225,210,170,255));
            lane.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;var input=lane.gameObject.AddComponent<RhythmJalurInput>();input.game=g;input.jalur=i;
            Text("Label",lane,new[]{"D","F","J","K"}[i],Vector2.zero,new Vector2(200,84),54,Dark);
        }
        g.penilaian=Text("Timing",screen,"",new Vector2(0,-150),new Vector2(700,65),46,Orange,true);
        Text("Petunjuk",screen,"KETUK SAAT NOT MENCAPAI GARIS   /   D F J K",new Vector2(0,-317),new Vector2(990,40),27,Paper);
        var menu=Box("MenuRonde",screen,new Vector2(0,-40),new Vector2(980,520),new Color32(30,43,53,250));menu.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;g.menu=menu.gameObject;
        g.judulMenu=Text("JudulMenu",menu,"MULAI DULU",new Vector2(0,152),new Vector2(900,70),62,Orange,true);
        g.isiMenu=Text("InstruksiAtauHasil",menu,"",new Vector2(0,15),new Vector2(890,230),34,Paper);
        g.tombolUtama=Button("MainUlangLanjut",menu,"MAIN",new Vector2(0,-155),new Vector2(440,80),out g.labelUtama);
        TextMeshProUGUI temp;g.tombolKeluar=Button("TutupRhythm",panel,"X",new Vector2(780,310),new Vector2(70,70),out temp);
        g.tombolJeda=Button("JedaRhythm",panel,"II",new Vector2(-780,310),new Vector2(70,70),out temp);
        bridge.rhythm=g;bridge.panel.SetActive(false);panel.gameObject.SetActive(false);
        var area=GameObject.Find("NPC_Raka_RapidTap").GetComponent<AreaInteraksi>();area.skenarioDialog=new[]{new BarisDialog{siapaYangBicara=Pembicara.Kanan_NPC,namaKarakter="Raka",teksDialog="Ada game irama di HP-mu, Arthur. Mau coba?"}};area.daftarOpsi[0].teksOpsi="Main rhythm di HP";
        var marker=area.transform.Find("NamaLatihan").GetComponent<TextMeshPro>();marker.text="RAKA\nGAME RHYTHM";
        EditorUtility.SetDirty(bridge);EditorUtility.SetDirty(g);EditorUtility.SetDirty(area);EditorSceneManager.MarkSceneDirty(g.gameObject.scene);EditorSceneManager.SaveOpenScenes();
    }
    static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;return r;
    }
    static RectTransform Box(string name,Transform parent,Vector2 p,Vector2 size,Color color)
    {
        var r=Rect(name,parent,p,size);var img=r.gameObject.AddComponent<UnityEngine.UI.Image>();img.color=color;img.raycastTarget=false;return r;
    }
    static TextMeshProUGUI Text(string name,Transform parent,string content,Vector2 pos,Vector2 size,float fs,Color color,bool wiggly=false)
    {
        var r=Rect(name,parent,pos,size);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=fs;t.enableAutoSizing=false;t.text=content;t.color=color;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;
        if(wiggly){var w=r.gameObject.AddComponent<WigglyText>();w.kekuatanGoyang=.7f;w.kecepatanAnimasi=7;}
        return t;
    }
    static UnityEngine.UI.Button Button(string name,Transform parent,string label,Vector2 pos,Vector2 size,out TextMeshProUGUI text)
    {
        var r=Box(name,parent,pos,size,Orange);var image=r.GetComponent<UnityEngine.UI.Image>();image.raycastTarget=true;
        var border=r.gameObject.AddComponent<UnityEngine.UI.Outline>();border.effectColor=Dark;border.effectDistance=new Vector2(2,-2);
        var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;b.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};
        text=Text("Teks",r,label,Vector2.zero,size-new Vector2(10,8),43,Dark,true);return b;
    }
}
