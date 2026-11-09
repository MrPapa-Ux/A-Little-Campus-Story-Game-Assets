// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menerapkan susunan/revisi yang dibuat pada checkpoint 12.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class CampusCheckpoint12
{
    static readonly Color Ink=new Color32(40,35,30,255),Cloth=new Color32(115,76,46,255),Lcd=new Color32(187,207,159,255),LcdInk=new Color32(38,58,29,255);
    static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/AmaticSC-Regular SDF.asset");
    static TMP_FontAsset Body=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit Mode diperlukan.");
        var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include);
        if(hp==null || hp.musik==null)throw new InvalidOperationException("Buka CharacterLab dengan checkpoint 11.");
        if(hp.tas.transform.Find("BingkaiHP/Kertas/BagianDalamTas")!=null)throw new InvalidOperationException("Revisi 12 sudah terpasang.");
        Musik(hp);Tas(hp.tas);Pesan(hp.pesan);
        EditorUtility.SetDirty(hp);EditorSceneManager.MarkSceneDirty(hp.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work/checkpoint12-apply.txt"),"COMPLETE: sprite Music tanpa kotak, mixtape oranye dan wiggly saat play, Tas enam kotak, Pesan LCD dengan avatar kontak; To-do dipertahankan.\n");
    }
    static void Musik(SistemHP hp)
    {
        var app=hp.musik;var sheet=app.transform.Find("BingkaiHP/Kertas");Remove(sheet.Find("Subjudul"));
        app.warnaKaset=hp.warnaDefaultHP;app.pita.GetComponent<UnityEngine.UI.Image>().color=app.warnaKaset;app.coretan=app.GetComponentInChildren<KasetCoretanUI>(true);
        foreach(var t in new[]{app.labelPutar,app.labelAcak,app.labelUlang,sheet.Find("VolumeLabel").GetComponent<TMP_Text>()})Wiggle(t);
        foreach(var slider in new[]{app.volume,app.progres})slider.fillRect.GetComponent<UnityEngine.UI.Image>().color=app.warnaKaset;
        for(int i=0;i<app.latarSlot.Length;i++)app.latarSlot[i].color=i==0?Color.Lerp(app.warnaKaset,new Color32(239,230,211,255),.28f):new Color32(239,230,211,255);
        var icon=hp.daftarIkon.First(g=>g.name=="IkonMusic");
        var back=icon.GetComponent<UnityEngine.UI.Image>();back.color=Color.clear;
        Remove(icon.GetComponent<UnityEngine.UI.Outline>());
        var note=icon.transform.Find("GambarNot");var image=note.GetComponent<UnityEngine.UI.Image>();image.color=Color.white;image.preserveAspect=true;((RectTransform)note).sizeDelta=new Vector2(78,78);
        icon.GetComponent<UnityEngine.UI.Button>().targetGraphic=image;
        var wig=note.GetComponent<WigglyBox>();if(wig==null)wig=note.gameObject.AddComponent<WigglyBox>();wig.kekuatanGoyangPosisi=1.3f;wig.kekuatanGoyangRotasi=1.4f;wig.kecepatanAnimasi=7;
        var header=sheet.Find("NotMusik");if(header!=null){var hw=header.gameObject.AddComponent<WigglyBox>();hw.kekuatanGoyangPosisi=1;hw.kekuatanGoyangRotasi=1.2f;}
        EditorUtility.SetDirty(app);EditorUtility.SetDirty(back);EditorUtility.SetDirty(image);EditorUtility.SetDirty(icon.GetComponent<UnityEngine.UI.Button>());
    }
    static void Tas(AplikasiCatatanHP app)
    {
        var sheet=app.transform.Find("BingkaiHP/Kertas");Remove(sheet.Find("PunggungBuku"));
        sheet.Find("Subjudul").GetComponent<TMP_Text>().text="DI DALAM TAS";
        var bagSub=sheet.Find("Subjudul").GetComponent<TMP_Text>();bagSub.rectTransform.anchoredPosition=new Vector2(-419,270);bagSub.rectTransform.sizeDelta=new Vector2(590,38);bagSub.fontSize=25;bagSub.color=new Color32(245,214,166,255);
        sheet.Find("Pemilik").GetComponent<TMP_Text>().text="MILIK ARTHUR";
        var detail=sheet.Find("KertasDetail");Remove(detail.Find("Selotip"));Remove(detail.Find("JahitanKantong"));detail.localRotation=Quaternion.identity;
        detail.GetComponent<UnityEngine.UI.Image>().color=new Color32(220,190,152,255);
        var cloth=Box(sheet,"BagianDalamTas",new Vector2(-420,16),new Vector2(735,650),Cloth);cloth.SetAsFirstSibling();
        var lining=Box(cloth,"LapisanKain",new Vector2(0,-10),new Vector2(690,545),new Color32(148,107,69,255));
        var zipper=Box(cloth,"Ritsleting",new Vector2(0,291),new Vector2(690,37),Ink);
        for(int i=0;i<36;i++)Rule(zipper,"Gigi_"+i,new Vector2(-327+i*18.6f,i%2==0?4:-4),new Vector2(7,23),new Color32(223,191,133,255));
        var pull=Box(zipper,"TarikanRitsleting",new Vector2(280,-10),new Vector2(33,59),new Color32(213,174,107,255));pull.localRotation=Quaternion.Euler(0,0,-10);
        for(int i=0;i<25;i++){Rule(cloth,"JahitanKiri_"+i,new Vector2(-345,-292+i*24),new Vector2(3,11),new Color32(221,177,123,255));Rule(cloth,"JahitanKanan_"+i,new Vector2(345,-292+i*24),new Vector2(3,11),new Color32(221,177,123,255));}
        Text(cloth,"LabelKompartemen","KOMPARTEMEN UTAMA",new Vector2(0,-306),new Vector2(640,34),24,Font, new Color32(245,214,166,255));
        var buttons=app.tombolBaris.ToList();var labels=app.labelBaris.ToList();var backgrounds=app.latarBaris.ToList();
        var extra=UnityEngine.Object.Instantiate(buttons[4].gameObject,sheet);extra.name="Catatan_5";var b=extra.GetComponent<UnityEngine.UI.Button>();b.onClick=new UnityEngine.UI.Button.ButtonClickedEvent();UnityEventTools.AddIntPersistentListener(b.onClick,app.PilihBaris,5);buttons.Add(b);labels.Add(extra.GetComponentInChildren<TMP_Text>(true));backgrounds.Add(extra.GetComponent<UnityEngine.UI.Image>());
        app.tombolBaris=buttons.ToArray();app.labelBaris=labels.ToArray();app.latarBaris=backgrounds.ToArray();app.ikonBaris=new UnityEngine.UI.Image[6];
        for(int i=0;i<6;i++)
        {
            var r=(RectTransform)buttons[i].transform;r.anchoredPosition=new Vector2(-630+i%3*210,156-i/3*224);r.sizeDelta=new Vector2(184,188);
            labels[i].rectTransform.anchoredPosition=new Vector2(0,-57);labels[i].rectTransform.sizeDelta=new Vector2(170,62);labels[i].fontSize=22;
            var item=R(r,"GambarBarang",new Vector2(0,28),new Vector2(95,100));var im=I(item,Color.white,false);im.preserveAspect=true;app.ikonBaris[i]=im;
            Rule(r,"JahitanSlot",new Vector2(0,-82),new Vector2(140,2),new Color32(104,69,39,255));
            EditorUtility.SetDirty(buttons[i]);EditorUtility.SetDirty(labels[i]);
        }
        app.warnaPilihan=new Color32(246,164,56,255);
        Text(detail,"LabelSaku","ISI SLOT TERPILIH",new Vector2(0,290),new Vector2(650,50),32,Font,Ink);
        var basePocket=Box(detail,"SakuDepan",new Vector2(0,-230),new Vector2(690,118),new Color32(162,111,66,255));
        for(int i=0;i<19;i++)Rule(basePocket,"Jahit_"+i,new Vector2(-315+i*35,-43),new Vector2(18,3),new Color32(238,195,135,255));
        Text(basePocket,"TulisanSaku","SIMPAN BAIK-BAIK.",Vector2.zero,new Vector2(620,65),34,Font,new Color32(255,229,187,255));
        app.detailIsi.rectTransform.anchoredPosition=new Vector2(0,4);app.detailIsi.rectTransform.sizeDelta=new Vector2(650,300);
        app.aksi.GetComponent<UnityEngine.UI.Image>().color=new Color32(179,127,78,255);
        EditorUtility.SetDirty(app);
    }
    static void Pesan(AplikasiCatatanHP app)
    {
        var sheet=app.transform.Find("BingkaiHP/Kertas");Remove(sheet.Find("PunggungBuku"));
        sheet.GetComponent<UnityEngine.UI.Image>().color=new Color32(158,178,131,255);
        sheet.Find("Subjudul").GetComponent<TMP_Text>().text="KOTAK MASUK";
        var smsSub=sheet.Find("Subjudul").GetComponent<TMP_Text>();smsSub.rectTransform.anchoredPosition=new Vector2(-419,279);smsSub.rectTransform.sizeDelta=new Vector2(590,45);smsSub.fontSize=29;
        app.jumlah.rectTransform.anchoredPosition=new Vector2(-419,-357);app.jumlah.rectTransform.sizeDelta=new Vector2(590,48);app.jumlah.fontSize=20;
        sheet.Find("Pemilik").GetComponent<TMP_Text>().text="SMS";
        var detail=sheet.Find("KertasDetail");Remove(detail.Find("Selotip"));foreach(Transform child in detail.Cast<Transform>().Where(x=>x.name.StartsWith("GarisBuku_")).ToArray())Remove(child);
        detail.name="LayarPercakapan";detail.localRotation=Quaternion.identity;detail.GetComponent<UnityEngine.UI.Image>().color=Lcd;
        var inbox=Box(sheet,"LayarKontak",new Vector2(-419,-14),new Vector2(670,650),new Color32(127,149,106,255));inbox.SetAsFirstSibling();
        app.ikonBaris=new UnityEngine.UI.Image[5];app.avatarBaris=new AvatarKontakUI[5];
        for(int i=0;i<5;i++)
        {
            var row=(RectTransform)app.tombolBaris[i].transform;
            app.avatarBaris[i]=Avatar(row,"Avatar",new Vector2(-245,0),new Vector2(75,75),out var photo);app.ikonBaris[i]=photo;
            app.labelBaris[i].rectTransform.anchoredPosition=new Vector2(47,0);app.labelBaris[i].rectTransform.sizeDelta=new Vector2(470,80);app.labelBaris[i].alignment=TextAlignmentOptions.Left;app.labelBaris[i].fontSize=23;app.labelBaris[i].color=LcdInk;
            EditorUtility.SetDirty(app.labelBaris[i]);
        }
        app.warnaPilihan=new Color32(147,182,109,255);
        app.avatarKontak=Avatar(detail,"AvatarKontak",new Vector2(-285,225),new Vector2(85,85),out var selectedPhoto);app.fotoKontak=selectedPhoto;
        app.detailJudul.rectTransform.anchoredPosition=new Vector2(60,226);app.detailJudul.rectTransform.sizeDelta=new Vector2(560,90);app.detailJudul.fontSize=45;app.detailJudul.color=LcdInk;
        app.statusKontak=Text(detail,"StatusKontak","SMS / KOTAK MASUK",new Vector2(60,158),new Vector2(560,44),23,Body,LcdInk);
        Rule(detail,"PemisahKontak",new Vector2(0,115),new Vector2(690,3),new Color32(96,122,73,255));
        var bubble=Box(detail,"BalonPesan",new Vector2(0,-69),new Vector2(690,320),new Color32(218,231,193,255));bubble.SetAsFirstSibling();
        var tail=Rule(bubble,"EkorPesan",new Vector2(-343,130),new Vector2(25,25),new Color32(218,231,193,255));tail.localRotation=Quaternion.Euler(0,0,45);tail.SetAsFirstSibling();
        app.detailIsi.rectTransform.anchoredPosition=new Vector2(0,-67);app.detailIsi.rectTransform.sizeDelta=new Vector2(635,270);app.detailIsi.fontSize=27;app.detailIsi.color=LcdInk;
        Text(detail,"FooterLayar","SMS  /  ARTHUR",new Vector2(0,-277),new Vector2(650,44),25,Body,LcdInk);
        for(int i=0;i<4;i++)Rule(sheet,"Sinyal_"+i,new Vector2(641+i*18,322+i*5),new Vector2(10,14+i*10),LcdInk);
        app.aksi.GetComponent<UnityEngine.UI.Image>().color=new Color32(71,94,49,255);app.labelAksi.color=new Color32(226,238,203,255);
        EditorUtility.SetDirty(app);EditorUtility.SetDirty(app.detailJudul);EditorUtility.SetDirty(app.detailIsi);EditorUtility.SetDirty(app.labelAksi);
    }
    static AvatarKontakUI Avatar(Transform parent,string name,Vector2 pos,Vector2 size,out UnityEngine.UI.Image photo)
    {
        var r=R(parent,name,pos,size);var circle=I(r,new Color32(220,232,197,255),false);circle.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");r.gameObject.AddComponent<Mask>().showMaskGraphic=true;
        var face=R(r,"Anonim",Vector2.zero,size*.9f);var avatar=face.gameObject.AddComponent<AvatarKontakUI>();avatar.color=LcdInk;avatar.raycastTarget=false;
        var pic=R(r,"Foto",Vector2.zero,size);photo=I(pic,Color.white,false);photo.preserveAspect=true;pic.gameObject.SetActive(false);return avatar;
    }
    static void Wiggle(TMP_Text t){var w=t.GetComponent<WigglyText>();if(w==null)w=t.gameObject.AddComponent<WigglyText>();w.kekuatanGoyang=1.4f;EditorUtility.SetDirty(w);}
    static void Remove(UnityEngine.Object obj){if(obj!=null)UnityEngine.Object.DestroyImmediate(obj is Transform t?t.gameObject:obj);}
    static RectTransform R(Transform p,string name,Vector2 pos,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;}
    static UnityEngine.UI.Image I(RectTransform r,Color c,bool ray){var i=r.gameObject.AddComponent<UnityEngine.UI.Image>();i.color=c;i.raycastTarget=ray;return i;}
    static RectTransform Rule(Transform p,string name,Vector2 pos,Vector2 size,Color c){var r=R(p,name,pos,size);I(r,c,false);return r;}
    static RectTransform Box(Transform p,string name,Vector2 pos,Vector2 size,Color c){var r=Rule(p,name,pos,size,c);var o=r.gameObject.AddComponent<UnityEngine.UI.Outline>();o.effectColor=Ink;o.effectDistance=new Vector2(2,-2);return r;}
    static TMP_Text Text(Transform p,string name,string value,Vector2 pos,Vector2 size,float font,TMP_FontAsset face,Color color){var r=R(p,name,pos,size);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.text=value;t.font=face;t.fontSize=font;t.color=color;t.alignment=TextAlignmentOptions.Center;t.overflowMode=TextOverflowModes.Ellipsis;t.raycastTarget=false;return t;}
}
