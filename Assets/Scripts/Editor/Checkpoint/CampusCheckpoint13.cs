// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menerapkan susunan/revisi yang dibuat pada checkpoint 13.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class CampusCheckpoint13
{
    static TMP_FontAsset Body=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit Mode diperlukan.");
        var h=UnityEngine.Object.FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include);
        Icons(h);Music(h);Books(h);CloseButtons(h);
        EditorUtility.SetDirty(h);EditorSceneManager.MarkSceneDirty(h.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work/checkpoint13-apply.txt"),"COMPLETE\n");
    }
    static void Icons(SistemHP h)
    {
        int order=2;
        foreach(var icon in h.daftarIkon)
        {
            icon.transform.SetParent(h.hpUtama.transform,false);icon.transform.SetSiblingIndex(order++);
            var r=(RectTransform)icon.transform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.localScale=Vector3.one;r.localRotation=Quaternion.identity;r.sizeDelta=new Vector2(78,78);
        }
        h.layarGelap.transform.SetAsLastSibling();
        var music=h.daftarIkon.First(x=>x.name=="IkonMusic");var note=(RectTransform)music.transform.Find("GambarNot");
        note.anchoredPosition=Vector2.zero;note.localScale=Vector3.one;note.sizeDelta=new Vector2(70,70);
        var wig=note.GetComponent<WigglyBox>();wig.kekuatanGoyangPosisi=1.7f;wig.kekuatanGoyangRotasi=1.8f;
        EditorUtility.SetDirty(wig);
    }
    static void Music(SistemHP h)
    {
        var app=h.musik;var sheet=app.transform.Find("BingkaiHP/Kertas");
        foreach(var t in new[]{app.labelPutar,app.labelAcak,app.labelUlang,app.judulLagu,sheet.Find("VolumeLabel").GetComponent<TMP_Text>(),sheet.GetComponentsInChildren<TMP_Text>(true).First(x=>x.name=="JudulKaset")})Wiggle(t);
        var side=sheet.GetComponentsInChildren<TMP_Text>(true).First(x=>x.name=="Sisi");side.text="SIDE A  /  LAGU TES";
        var playlist=app.pemutar.playlist;
        playlist.lagu[0].audio=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/TestAbsurdSong/Heartbeat, Heartbreak.mp3");
        if(playlist.lagu[0].audio==null)throw new InvalidOperationException("Lagu tes tidak ditemukan.");
        playlist.lagu[0].judul="Heartbeat, Heartbreak";playlist.lagu[0].catatan="Lagu yang sudah dipakai di proyek, untuk tes pemutar. Bukan lagu instrumental final Arthur.";
        EditorUtility.SetDirty(playlist);
        var toast=UnityEngine.Object.FindFirstObjectByType<PemutarMusikUI>(FindObjectsInactive.Include);
        toast.pemutar=app.pemutar;toast.hp=h;toast.judul=toast.GetComponentInChildren<TMP_Text>(true);toast.enabled=true;
        var toastRect=(RectTransform)toast.transform;toastRect.anchorMin=toastRect.anchorMax=toastRect.pivot=new Vector2(0,1);toastRect.anchoredPosition=new Vector2(110,-235);toastRect.sizeDelta=new Vector2(610,110);
        var circle=(RectTransform)toast.transform.Find("LatarBulat");circle.anchorMin=circle.anchorMax=circle.pivot=new Vector2(.5f,.5f);circle.anchoredPosition=new Vector2(-256,0);circle.sizeDelta=new Vector2(92,92);
        toast.ikonMusik=(RectTransform)circle.Find("IkonLaguPutar");
        toast.ikonMusik.anchoredPosition=Vector2.zero;toast.ikonMusik.sizeDelta=new Vector2(68,68);
        var title=toast.judul;title.rectTransform.anchorMin=title.rectTransform.anchorMax=title.rectTransform.pivot=new Vector2(.5f,.5f);title.rectTransform.anchoredPosition=new Vector2(45,0);title.rectTransform.sizeDelta=new Vector2(480,105);title.fontSize=33;title.enableAutoSizing=false;title.alignment=TextAlignmentOptions.Left;
        toast.GetComponent<AudioSource>().playOnAwake=false;toast.GetComponent<CanvasGroup>().alpha=0;
        EditorUtility.SetDirty(toast);EditorUtility.SetDirty(toast.GetComponent<AudioSource>());
        app.Segarkan();
    }
    static void Books(SistemHP h)
    {
        foreach(var app in new[]{h.tugas,h.tas,h.pesan})
        {
            var sheet=app.transform.Find("BingkaiHP/Kertas");
            Wiggle(app.detailJudul);Wiggle(app.labelAksi);Wiggle(sheet.Find("Pemilik").GetComponent<TMP_Text>());
            app.jumlah.font=Body;app.jumlah.fontSize=34;app.jumlah.fontStyle=FontStyles.Bold;app.jumlah.rectTransform.sizeDelta=new Vector2(630,65);
            app.jumlah.rectTransform.anchoredPosition=new Vector2(-419,-353);app.jumlah.enableAutoSizing=false;
            app.labelAksi.fontSize=38;app.labelAksi.rectTransform.sizeDelta=new Vector2(690,70);
            app.Segarkan();EditorUtility.SetDirty(app);EditorUtility.SetDirty(app.jumlah);
        }
        var todo=h.tugas.transform.Find("BingkaiHP/Kertas");Remove(todo.Find("Subjudul"));
        var sms=h.pesan.transform.Find("BingkaiHP/Kertas");
        var inbox=sms.Find("Subjudul").GetComponent<TMP_Text>();inbox.fontSize=44;inbox.rectTransform.sizeDelta=new Vector2(590,76);inbox.rectTransform.anchoredPosition=new Vector2(-419,274);Wiggle(inbox);
        h.pesan.detailIsi.fontSize=35;h.pesan.detailIsi.enableAutoSizing=false;h.pesan.detailIsi.overflowMode=TextOverflowModes.Overflow;
        // Mask and ScrollRect keep long messages readable without shrinking the type.
        var bubble=sms.Find("LayarPercakapan/BalonPesan");var viewport=bubble.Find("AreaPesan") as RectTransform;
        if(viewport==null)
        {
            viewport=R(bubble,"AreaPesan",Vector2.zero,new Vector2(635,280));viewport.gameObject.AddComponent<RectMask2D>();
            var scroll=bubble.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            var content=h.pesan.detailIsi.rectTransform;content.SetParent(viewport,false);content.anchorMin=new Vector2(0,1);content.anchorMax=new Vector2(1,1);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;content.sizeDelta=Vector2.zero;
            var fit=content.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.content=content;
            bubble.GetComponent<Image>().raycastTarget=true;
        }
        h.pesan.gulirPesan=bubble.GetComponent<ScrollRect>();
        var bars=sms.Find("BatangSinyal") as RectTransform;
        if(bars==null)
        {
            bars=R(sms,"BatangSinyal",new Vector2(668,329),new Vector2(85,65));
            for(int i=0;i<4;i++)sms.Find("Sinyal_"+i).SetParent(bars,true);
            var w=bars.gameObject.AddComponent<WigglyBox>();w.kekuatanGoyangPosisi=1.8f;w.kekuatanGoyangRotasi=1;w.kecepatanAnimasi=7;
        }
        Tas(h.tas);h.pesan.Segarkan();
    }
    static void Tas(AplikasiCatatanHP app)
    {
        var sheet=app.transform.Find("BingkaiHP/Kertas");Remove(sheet.Find("Subjudul"));
        var bag=sheet.Find("BagianDalamTas");bag.GetComponent<Image>().color=Color.clear;
        var outline=bag.GetComponent<Outline>();if(outline!=null)UnityEngine.Object.DestroyImmediate(outline);
        if(bag.Find("BentukRansel")==null)
        {
            var shape=R(bag,"BentukRansel",Vector2.zero,new Vector2(735,650));shape.SetAsFirstSibling();var g=shape.gameObject.AddComponent<BentukTasUI>();g.color=new Color32(115,76,46,255);g.raycastTarget=false;
        }
        var lining=bag.Find("LapisanKain");lining.GetComponent<Image>().color=Color.clear;
        foreach(Transform child in bag.Cast<Transform>().Where(x=>x.name.StartsWith("Jahitan")).ToArray())Remove(child);
        var zipper=(RectTransform)bag.Find("Ritsleting");zipper.anchoredPosition=new Vector2(0,255);zipper.sizeDelta=new Vector2(635,52);zipper.GetComponent<Image>().raycastTarget=true;
        for(int i=0;i<36;i++){var tooth=(RectTransform)zipper.Find("Gigi_"+i);tooth.anchoredPosition=new Vector2(-300+i*17.1f,i%2==0?4:-4);}
        var pull=(RectTransform)zipper.Find("TarikanRitsleting");pull.sizeDelta=new Vector2(40,67);pull.GetComponent<Image>().raycastTarget=true;
        var zip=zipper.GetComponent<RitsletingTasUI>();if(zip==null)zip=zipper.gameObject.AddComponent<RitsletingTasUI>();zip.tas=app;zip.tarikan=pull;app.ritsleting=zip;
        var label=bag.Find("LabelKompartemen").GetComponent<TMP_Text>();label.text="TARIK RITSLETING  /  GESER HALAMAN";label.fontSize=27;label.rectTransform.anchoredPosition=new Vector2(0,-274);label.rectTransform.sizeDelta=new Vector2(640,58);
        for(int i=0;i<app.tombolBaris.Length;i++)((RectTransform)app.tombolBaris[i].transform).anchoredPosition=new Vector2(-630+i%3*210,143-i/3*215);
        var detail=sheet.Find("KertasDetail");Remove(detail.Find("LabelSaku"));var pocket=(RectTransform)detail.Find("SakuDepan");Remove(pocket.Find("TulisanSaku"));
        pocket.anchoredPosition=new Vector2(0,-188);pocket.sizeDelta=new Vector2(690,230);
        foreach(Transform child in pocket){var p=(RectTransform)child;p.anchoredPosition=new Vector2(p.anchoredPosition.x,-94);}
        if(app.pratinjauBarang==null){var r=R(pocket,"PratinjauBarang",Vector2.zero,new Vector2(380,200));app.pratinjauBarang=r.gameObject.AddComponent<Image>();app.pratinjauBarang.preserveAspect=true;app.pratinjauBarang.raycastTarget=false;}
        app.detailIsi.rectTransform.anchoredPosition=new Vector2(0,64);app.detailIsi.rectTransform.sizeDelta=new Vector2(650,210);app.detailIsi.fontSize=31;
        app.Segarkan();EditorUtility.SetDirty(app);
    }
    static void CloseButtons(SistemHP h)
    {
        var original=h.panelPengaturan.transform.Find("Box_Pengaturan/Btn_TutupX").GetComponent<Image>();
        foreach(var root in new[]{h.musik.gameObject,h.tugas.gameObject,h.pesan.gameObject,h.tas.gameObject,h.petaKampus.gameObject,h.panelSegeraHadir})
        {
            var target=root.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name=="KembaliKeHP"||b.name=="Btn_KembaliPonsel");if(target==null)throw new InvalidOperationException("X tidak ditemukan: "+root.name);
            var im=target.GetComponent<Image>();im.sprite=original.sprite;im.type=original.type;im.color=original.color;
            var outline=target.GetComponent<Outline>();if(outline==null)outline=target.gameObject.AddComponent<Outline>();var src=original.GetComponent<Outline>();outline.effectColor=src.effectColor;outline.effectDistance=src.effectDistance;
            var text=target.GetComponentInChildren<TMP_Text>(true);text.font=Body;text.fontSize=48;text.fontStyle=FontStyles.Normal;text.text="X";text.rectTransform.sizeDelta=new Vector2(80,80);Wiggle(text);
            EditorUtility.SetDirty(target);EditorUtility.SetDirty(im);EditorUtility.SetDirty(text);
        }
    }
    static void Wiggle(TMP_Text t){var w=t.GetComponent<WigglyText>();if(w==null)w=t.gameObject.AddComponent<WigglyText>();w.kekuatanGoyang=2.2f;w.kecepatanAnimasi=7;EditorUtility.SetDirty(w);EditorUtility.SetDirty(t);}
    static void Remove(Transform t){if(t!=null)UnityEngine.Object.DestroyImmediate(t.gameObject);}
    static RectTransform R(Transform p,string name,Vector2 pos,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;}
}
