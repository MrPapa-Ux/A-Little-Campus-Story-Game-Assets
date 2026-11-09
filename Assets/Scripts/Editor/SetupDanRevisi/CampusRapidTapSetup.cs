// PANDUAN PROYEK - Editor / SetupDanRevisi
// Fungsi: Membentuk percobaan rapid tap dan menyelesaikan visualnya.
// Panduan: Assets/Scripts/Editor/SetupDanRevisi/README.md

using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CampusRapidTapSetup
{
    static readonly Color Ink=new Color32(40,31,24,255),Paper=new Color32(219,209,181,255),Orange=new Color32(212,110,15,255);
    static TMP_FontAsset font;
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Gunakan Edit Mode.");
        if(UnityEngine.Object.FindFirstObjectByType<RapidTapArthur>(FindObjectsInactive.Include)!=null)throw new InvalidOperationException("Percobaan sudah ada; jangan menggandakan NPC/UI.");
        var dialog=UnityEngine.Object.FindFirstObjectByType<DialogManager>();
        font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/AmaticSC-Regular SDF.asset");
        if(font==null||dialog==null)throw new InvalidOperationException("Font atau dialog tidak tersedia.");
        var root=new GameObject("CanvasRapidTapArthur",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=2500;
        var scaler=root.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        var game=root.AddComponent<RapidTapArthur>();game.dialog=dialog;game.lazyBar=UnityEngine.Object.FindFirstObjectByType<LazyBarArthur>();
        var overlay=Rect("PanelLatihan",root.transform,Vector2.zero,Vector2.zero);overlay.anchorMin=Vector2.zero;overlay.anchorMax=Vector2.one;
        var shade=overlay.gameObject.AddComponent<UnityEngine.UI.Image>();shade.color=new Color(0,0,0,.68f);shade.raycastTarget=true;game.panel=overlay.gameObject;
        var sheet=Box("KertasLatihan",overlay,Vector2.zero,new Vector2(1580,860),Paper,true);var wobble=sheet.gameObject.AddComponent<WigglyBox>();wobble.kekuatanGoyangPosisi=1;wobble.kekuatanGoyangRotasi=.2f;
        Box("GarisAtas",sheet,new Vector2(0,295),new Vector2(1430,5),Orange);
        var tape1=Box("TapeKiri",sheet,new Vector2(-575,427),new Vector2(190,35),new Color32(170,173,144,255));tape1.localRotation=Quaternion.Euler(0,0,-6);
        var tape2=Box("TapeKanan",sheet,new Vector2(575,427),new Vector2(190,35),new Color32(170,173,144,255));tape2.localRotation=Quaternion.Euler(0,0,5);
        Text("LabelLatihan",sheet,"LATIHAN / ARTHUR",new Vector2(-560,345),new Vector2(350,75),45,true);
        game.judul=Text("Judul",sheet,"AYO, MULAI DULU!",new Vector2(210,345),new Vector2(920,85),66,true);
        var card=Box("KartuArthur",sheet,new Vector2(-550,35),new Vector2(340,430),new Color32(199,187,158,255),true);
        var head=dialog.arthurScript.GetComponentsInChildren<SpriteRenderer>(true).First(x=>x.name=="KepalaArthur");
        var portrait=Rect("PotretArthur",card,new Vector2(0,30),new Vector2(230,230));var image=portrait.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=head.sprite;image.preserveAspect=true;image.raycastTarget=false;
        Text("NamaArthur",card,"ARTHUR",new Vector2(0,-145),new Vector2(310,75),60,true);
        game.godaan=Text("Godaan",sheet,"NANTI AJA...",new Vector2(-550,-255),new Vector2(360,130),42,true);
        game.hitungan=Text("JumlahKetukan",sheet,"20 KETUKAN",new Vector2(-25,225),new Vector2(400,70),52);
        game.waktu=Text("Waktu",sheet,"6 DETIK",new Vector2(550,225),new Vector2(300,70),52);
        var bar=Box("BingkaiIndikator",sheet,new Vector2(210,135),new Vector2(930,55),Ink);
        var fill=Box("IsiIndikator",bar,Vector2.zero,new Vector2(916,41),Orange);game.isiBar=fill.GetComponent<UnityEngine.UI.Image>();game.isiBar.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");game.isiBar.type=UnityEngine.UI.Image.Type.Filled;game.isiBar.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;game.isiBar.fillAmount=0;
        game.tombolKetuk=Button("TombolKetuk",sheet,"MULAI DULU!",new Vector2(210,-40),new Vector2(930,230),Orange,out game.labelKetuk);game.labelKetuk.fontSize=88;
        game.tombolKetuk.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};
        Text("Petunjuk",sheet,"KETUK TOMBOL BERULANG  /  SPASI ATAU ENTER",new Vector2(210,-200),new Vector2(930,60),38);
        game.status=Text("StatusRaka",sheet,"Raka: Satu langkah kecil dulu, Arthur.",new Vector2(210,-270),new Vector2(930,75),42);
        game.tombolMulai=Button("MulaiAtauUlang",sheet,"MULAI",new Vector2(60,-365),new Vector2(440,80),Orange,out game.labelMulai);
        TextMeshProUGUI unused;game.tombolKeluar=Button("KeluarLatihan",sheet,"KEMBALI",new Vector2(565,-365),new Vector2(260,80),Paper,out unused);
        // Salin NPC yang rig dan ekspresinya sudah tersedia; tidak mengubah NPC sumber.
        var source=GameObject.Find("NPC_Nara_Ekspresi");if(source==null)throw new InvalidOperationException("NPC sumber tidak ditemukan.");
        var npc=UnityEngine.Object.Instantiate(source,source.transform.parent);npc.name="NPC_Raka_RapidTap";npc.transform.position=new Vector3(-4,.5f,source.transform.position.z);
        foreach(var sprite in npc.GetComponentsInChildren<SpriteRenderer>(true))if(sprite.name=="Badan")sprite.color=new Color32(255,185,95,255);
        var area=npc.GetComponent<AreaInteraksi>();area.idTempat=npc.name;area.skenarioDialog=new[]{new BarisDialog{siapaYangBicara=Pembicara.Kanan_NPC,namaKarakter="Raka",teksDialog="Arthur, jangan tunggu nanti. Coba latihan ketuk cepat bareng aku!"}};
        area.daftarOpsi=new[]{new OpsiInteraksi{teksOpsi="Coba rapid tap",rapidTap=game}};area.tampilkanTombolKeluar=true;
        var mark=new GameObject("NamaLatihan",typeof(TextMeshPro));mark.transform.SetParent(npc.transform,false);mark.transform.localPosition=new Vector3(0,2.25f,-.15f);
        var label=mark.GetComponent<TextMeshPro>();label.font=font;label.text="RAKA\nLATIHAN TAP";label.fontSize=3.4f;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=new Vector2(5,1.3f);label.color=new Color32(255,218,135,255);label.GetComponent<MeshRenderer>().sortingOrder=100;
        overlay.gameObject.SetActive(false);FinishVisuals();EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(dialog.gameObject.scene);EditorSceneManager.SaveOpenScenes();
    }
    public static void FinishVisuals()
    {
        var game=UnityEngine.Object.FindFirstObjectByType<RapidTapArthur>();var player=game.dialog.arthurScript;
        var source=player.GetComponentsInChildren<SpriteRenderer>(true);var head=source.First(x=>x.name=="KepalaArthur");
        var portrait=(RectTransform)game.panel.transform.Find("KertasLatihan/KartuArthur/PotretArthur");
        var headCenter=head.transform.TransformPoint(head.sprite.bounds.center);
        float factor=210/Mathf.Max(head.sprite.bounds.size.x*Mathf.Abs(head.transform.lossyScale.x),head.sprite.bounds.size.y*Mathf.Abs(head.transform.lossyScale.y));
        portrait.sizeDelta=new Vector2(head.sprite.bounds.size.x*Mathf.Abs(head.transform.lossyScale.x),head.sprite.bounds.size.y*Mathf.Abs(head.transform.lossyScale.y))*factor;
        portrait.localRotation=Quaternion.Euler(0,0,head.transform.eulerAngles.z);
        foreach(string name in new[]{"Telinga","Rambut 1"})
        {
            var sr=source.First(x=>x.name==name);var r=portrait.parent.Find("Potret_"+name) as RectTransform;
            if(r==null)r=Rect("Potret_"+name,portrait.parent,Vector2.zero,Vector2.one);
            var img=r.GetComponent<UnityEngine.UI.Image>();if(img==null)img=r.gameObject.AddComponent<UnityEngine.UI.Image>();img.sprite=sr.sprite;img.preserveAspect=true;img.raycastTarget=false;
            var delta=sr.transform.TransformPoint(sr.sprite.bounds.center)-headCenter;
            r.anchoredPosition=portrait.anchoredPosition+new Vector2(delta.x,delta.y)*factor;
            r.sizeDelta=new Vector2(sr.sprite.bounds.size.x*Mathf.Abs(sr.transform.lossyScale.x),sr.sprite.bounds.size.y*Mathf.Abs(sr.transform.lossyScale.y))*factor;
            r.localRotation=Quaternion.Euler(0,0,sr.transform.eulerAngles.z);EditorUtility.SetDirty(img);
        }
        var npc=GameObject.Find("NPC_Raka_RapidTap");var label=npc.transform.Find("NamaLatihan").GetComponent<TextMeshPro>();label.fontSize=4.5f;label.fontStyle=FontStyles.Bold;label.color=Ink;
        var plate=npc.transform.Find("LatarNamaLatihan");
        if(plate==null){plate=new GameObject("LatarNamaLatihan",typeof(SpriteRenderer)).transform;plate.SetParent(npc.transform,false);}
        plate.localPosition=label.transform.localPosition+new Vector3(0,0,.01f);var renderer=plate.GetComponent<SpriteRenderer>();renderer.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");renderer.drawMode=SpriteDrawMode.Sliced;renderer.size=new Vector2(4.2f,1.3f);renderer.color=Paper;renderer.sortingOrder=99;
        EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveOpenScenes();
    }
    static RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;
    }
    static RectTransform Box(string name,Transform parent,Vector2 pos,Vector2 size,Color color,bool outline=false)
    {
        var r=Rect(name,parent,pos,size);var i=r.gameObject.AddComponent<UnityEngine.UI.Image>();i.color=color;i.raycastTarget=false;
        if(outline){var o=r.gameObject.AddComponent<UnityEngine.UI.Outline>();o.effectColor=Ink;o.effectDistance=new Vector2(3,-3);}
        return r;
    }
    static TextMeshProUGUI Text(string name,Transform parent,string value,Vector2 pos,Vector2 size,float fontSize,bool wiggle=false)
    {
        var r=Rect(name,parent,pos,size);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=fontSize;t.color=Ink;t.enableAutoSizing=false;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;
        if(wiggle){var w=r.gameObject.AddComponent<WigglyText>();w.kekuatanGoyang=.9f;w.kecepatanAnimasi=7;}
        return t;
    }
    static UnityEngine.UI.Button Button(string name,Transform parent,string value,Vector2 pos,Vector2 size,Color color,out TextMeshProUGUI label)
    {
        var r=Box(name,parent,pos,size,color,true);var image=r.GetComponent<UnityEngine.UI.Image>();image.raycastTarget=true;var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;
        b.navigation=new UnityEngine.UI.Navigation{mode=UnityEngine.UI.Navigation.Mode.None};label=Text("Teks",r,value,Vector2.zero,size-new Vector2(25,10),48,true);return b;
    }
}
