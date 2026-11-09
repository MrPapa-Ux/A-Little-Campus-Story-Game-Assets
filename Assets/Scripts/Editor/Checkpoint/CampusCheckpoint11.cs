// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menerapkan susunan/revisi yang dibuat pada checkpoint 11.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class CampusCheckpoint11
{
    static readonly Color Paper=new Color32(239,229,207,255),Ink=new Color32(40,35,47,255),Purple=new Color32(205,187,222,255);
    static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/AmaticSC-Regular SDF.asset");
    static TMP_FontAsset Body=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
    static string Work(string f)=>Path.Combine(Directory.GetParent(Application.dataPath).FullName,"work",f);
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit Mode diperlukan.");
        EditorSceneManager.SaveOpenScenes();
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/CharacterLab.unity")EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        var hp=UnityEngine.Object.FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include);
        if(hp.musik!=null)throw new InvalidOperationException("Checkpoint 11 sudah terpasang. Tidak menggandakan panel.");
        Sprite icon=ImportIcon();
        var audio=GameObject.Find("AudioGameArthur");if(audio==null)audio=new GameObject("AudioGameArthur");
        var service=audio.AddComponent<MusikArthur>();service.sumber=audio.AddComponent<AudioSource>();service.tema=audio.AddComponent<AudioSource>();
        foreach(var s in audio.GetComponents<AudioSource>()){s.playOnAwake=false;s.spatialBlend=0;s.volume=.65f;}
        const string playlistPath="Assets/AudioArthur/Playlist_Arthur.asset";
        var playlist=AssetDatabase.LoadAssetAtPath<PlaylistArthur>(playlistPath);
        if(playlist==null){playlist=ScriptableObject.CreateInstance<PlaylistArthur>();playlist.lagu=Enumerable.Range(1,5).Select(i=>new LaguArthur{judul="Arthur - "+i.ToString("00")}).ToArray();AssetDatabase.CreateAsset(playlist,playlistPath);}
        service.playlist=playlist;
        foreach(var old in UnityEngine.Object.FindObjectsByType<PemutarMusikUI>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            old.enabled=false;var source=old.GetComponent<AudioSource>();if(source!=null){source.playOnAwake=false;source.Stop();EditorUtility.SetDirty(source);}
            var group=old.GetComponent<CanvasGroup>();if(group!=null){group.alpha=0;group.blocksRaycasts=false;EditorUtility.SetDirty(group);}EditorUtility.SetDirty(old);
        }
        var buku=hp.GetComponent<BukuArthur>();if(buku==null)buku=hp.gameObject.AddComponent<BukuArthur>();
        BuatMusik(hp,service,icon);
        hp.tugas=BuatBuku(hp,buku,JenisBukuArthur.Tugas,"AplikasiTugas","TO-DO / ARTHUR","HALAMAN RENCANA",new Color32(211,222,195,255));
        hp.pesan=BuatBuku(hp,buku,JenisBukuArthur.Pesan,"AplikasiPesan","PESAN / ARTHUR","KABAR DALAM KERTAS",new Color32(239,212,158,255));
        hp.tas=BuatBuku(hp,buku,JenisBukuArthur.Tas,"AplikasiTas","TAS / ARTHUR","KANTONG PERJALANAN",new Color32(217,177,144,255));
        BuatIkon(hp,icon);PasangSuara();
        foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None))EditorUtility.SetDirty(source);
        EditorUtility.SetDirty(service);EditorUtility.SetDirty(hp);EditorUtility.SetDirty(buku);
        EditorSceneManager.MarkSceneDirty(hp.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
        File.WriteAllText(Work("checkpoint11-apply.txt"),"COMPLETE: playlist Arthur 5 slot kosong, Music/To-do/Pesan/Tas, 4 profil suara kertas. Android; tidak ada konfigurasi iOS atau perubahan model kampus.\n");
    }
    static Sprite ImportIcon()
    {
        const string path="Assets/AudioArthur/IkonMusicAsli.png";
        AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.mipmapEnabled=false;importer.maxTextureSize=1024;importer.alphaIsTransparency=true;importer.SaveAndReimport();
        var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
        var cap=provider.GetDataProvider<ISpriteFrameEditCapability>();
        if(cap==null || !cap.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite))throw new InvalidOperationException("Importer tidak mendukung pemotongan sprite dengan aman.");
        var id=GUID.Generate();var rect=new SpriteRect{name="Music_Coretaan",rect=new Rect(800,400,1023,1420),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f),spriteID=id};
        provider.SetSpriteRects(new[]{rect});var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();if(names==null)throw new InvalidOperationException("SpriteNameFileIdDataProvider tidak tersedia.");
        names.SetNameFileIdPairs(new[]{new SpriteNameFileIdPair(rect.name,id)});provider.Apply();importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().First();
    }
    static void BuatMusik(SistemHP hp,MusikArthur service,Sprite icon)
    {
        var sheet=Card(hp,"AplikasiMusik","MUSIC / ARTHUR",out var root);
        var app=root.AddComponent<AplikasiMusikHP>();hp.musik=app;app.hp=hp;app.pemutar=service;
        Text(sheet,"Subjudul","LIMA LAGU, SATU KASET.",new Vector2(-420,306),new Vector2(620,55),34);
        var tape=Box(sheet,"Kaset",new Vector2(-425,53),new Vector2(625,350),Purple);tape.localRotation=Quaternion.Euler(0,0,-2.3f);app.pita=tape;
        Box(tape,"LabelKaset",new Vector2(0,102),new Vector2(565,80),Paper);
        Text(tape,"JudulKaset","ARTHUR'S MIXTAPE",new Vector2(0,105),new Vector2(520,70),49);
        var wheel=R(tape,"CoretanPita",new Vector2(0,-22),new Vector2(500,220));var art=wheel.gameObject.AddComponent<KasetCoretanUI>();art.color=Ink;art.raycastTarget=false;
        Text(tape,"Sisi","SIDE A  /  INSTRUMENTAL",new Vector2(0,-133),new Vector2(500,42),28);
        var note=R(sheet,"NotMusik",new Vector2(-726,279),new Vector2(40,54));var image=I(note,Ink,false);image.sprite=icon;image.preserveAspect=true;
        Text(sheet,"Daftar","ISI KASET",new Vector2(385,301),new Vector2(690,55),45);
        app.namaSlot=new TMP_Text[5];app.latarSlot=new UnityEngine.UI.Image[5];
        for(int i=0;i<5;i++)
        {
            var b=Button(sheet,"Lagu_"+i,"",new Vector2(385,213-i*91),new Vector2(710,76),null);
            UnityEventTools.AddIntPersistentListener(b.onClick,app.PilihSlot,i);app.namaSlot[i]=b.GetComponentInChildren<TMP_Text>();app.namaSlot[i].font=Body;app.namaSlot[i].fontSize=25;
            app.latarSlot[i]=b.GetComponent<UnityEngine.UI.Image>();
        }
        app.judulLagu=Text(sheet,"LaguAktif","BELUM ADA LAGU",new Vector2(-425,-180),new Vector2(640,80),44);
        app.status=Text(sheet,"Status","Kaset Arthur masih kosong.",new Vector2(-425,-232),new Vector2(640,50),24,true);
        app.waktu=Text(sheet,"Waktu","00:00 / 00:00",new Vector2(-425,-280),new Vector2(640,38),23,true);
        app.progres=Slider(sheet,"PosisiLagu",new Vector2(-425,-324),new Vector2(610,36),app.Geser);
        Button(sheet,"Sebelumnya","<",new Vector2(-670,-396),new Vector2(110,77),app.Kiri);
        var play=Button(sheet,"Putar","PUTAR",new Vector2(-425,-396),new Vector2(325,77),app.Putar);app.labelPutar=play.GetComponentInChildren<TMP_Text>();
        Button(sheet,"Berikutnya",">",new Vector2(-180,-396),new Vector2(110,77),app.Kanan);
        app.labelAcak=Button(sheet,"Acak","ACAK: OFF",new Vector2(205,-286),new Vector2(320,64),app.Acak).GetComponentInChildren<TMP_Text>();
        app.labelUlang=Button(sheet,"Ulang","ULANG: OFF",new Vector2(565,-286),new Vector2(320,64),app.Ulang).GetComponentInChildren<TMP_Text>();
        Text(sheet,"VolumeLabel","VOLUME MUSIK",new Vector2(385,-352),new Vector2(640,40),30);
        app.volume=Slider(sheet,"Volume",new Vector2(385,-404),new Vector2(660,36),app.Volume);
        root.SetActive(false);EditorUtility.SetDirty(app);
    }
    static AplikasiCatatanHP BuatBuku(SistemHP hp,BukuArthur buku,JenisBukuArthur jenis,string name,string title,string subtitle,Color accent)
    {
        var sheet=Card(hp,name,title,out var root);var app=root.AddComponent<AplikasiCatatanHP>();app.hp=hp;app.buku=buku;app.jenis=jenis;
        Text(sheet,"Subjudul",subtitle,new Vector2(-435,304),new Vector2(590,60),33);
        // Binding buku dan lubang kertas sengaja tidak simetris, seperti buku coretan Arthur.
        var strip=Box(sheet,"PunggungBuku",new Vector2(-770,-30),new Vector2(35,715),accent);
        for(int i=0;i<7;i++){var hole=Box(strip,"Jilid_"+i,new Vector2(0,300-i*98),new Vector2(65,12),Ink);hole.localRotation=Quaternion.Euler(0,0,i%2==0?-4:3);}
        app.labelBaris=new TMP_Text[5];app.latarBaris=new UnityEngine.UI.Image[5];app.tombolBaris=new UnityEngine.UI.Button[5];
        for(int i=0;i<5;i++)
        {
            var b=Button(sheet,"Catatan_"+i,"",new Vector2(-419,207-i*114),new Vector2(595,94),null);
            UnityEventTools.AddIntPersistentListener(b.onClick,app.PilihBaris,i);app.tombolBaris[i]=b;app.labelBaris[i]=b.GetComponentInChildren<TMP_Text>();app.labelBaris[i].font=Body;app.labelBaris[i].fontSize=23;app.latarBaris[i]=b.GetComponent<UnityEngine.UI.Image>();
        }
        var detail=Box(sheet,"KertasDetail",new Vector2(348,-16),new Vector2(760,626),new Color32(250,244,225,255));detail.localRotation=Quaternion.Euler(0,0,1f);
        var tape=Box(detail,"Selotip",new Vector2(0,310),new Vector2(250,38),accent);tape.localRotation=Quaternion.Euler(0,0,-4);
        if(jenis==JenisBukuArthur.Tas)
        {
            var pocket=Box(detail,"JahitanKantong",new Vector2(0,-182),new Vector2(680,170),accent);
            for(int i=0;i<18;i++)Box(pocket,"Jahit_"+i,new Vector2(-315+i*37,-63),new Vector2(17,3),Ink);
            Text(pocket,"TulisanKantong","BARANG KECIL, CERITA PANJANG.",Vector2.zero,new Vector2(640,90),34);
        }
        else for(int i=0;i<6;i++){var rule=R(detail,"GarisBuku_"+i,new Vector2(0,135-i*70),new Vector2(690,1));I(rule,new Color(.48f,.53f,.58f,.18f),false);}
        app.detailJudul=Text(detail,"JudulDetail","",new Vector2(0,218),new Vector2(680,100),48);
        app.detailIsi=Text(detail,"IsiDetail","",new Vector2(0,jenis==JenisBukuArthur.Tas?50:-25),new Vector2(650,jenis==JenisBukuArthur.Tas?220:350),27,true);
        app.detailIsi.alignment=TextAlignmentOptions.TopLeft;
        app.jumlah=Text(sheet,"NomorHalaman","00 CATATAN",new Vector2(-419,-340),new Vector2(590,50),29);
        Button(sheet,"Kiri","<",new Vector2(-650,-410),new Vector2(120,72),app.Kiri);Button(sheet,"Kanan",">",new Vector2(-190,-410),new Vector2(120,72),app.Kanan);
        Text(sheet,"Pemilik","PUNYA ARTHUR",new Vector2(-419,-410),new Vector2(300,55),31);
        app.aksi=Button(sheet,"Aksi","BELUM ADA CATATAN",new Vector2(348,-410),new Vector2(720,72),app.Interaksi);app.labelAksi=app.aksi.GetComponentInChildren<TMP_Text>();app.labelAksi.fontSize=33;
        root.SetActive(false);EditorUtility.SetDirty(app);return app;
    }
    static void BuatIkon(SistemHP hp,Sprite icon)
    {
        var r=R(hp.hpUtama.transform,"IkonMusic",Vector2.zero,new Vector2(78,78));var image=I(r,Purple,true);Outline(r,2);
        var button=r.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=image;UnityEventTools.AddPersistentListener(button.onClick,hp.BukaMusik);
        var note=R(r,"GambarNot",Vector2.zero,new Vector2(55,66));var n=I(note,Ink,false);n.sprite=icon;n.preserveAspect=true;
        var selected=R(r,"TelahTerpilih",Vector2.zero,new Vector2(86,86));I(selected,new Color(.5f,1,.3f,.22f),false);Outline(selected,3);selected.gameObject.SetActive(false);
        hp.daftarIkon.Insert(hp.daftarIkon.Count-1,r.gameObject);
        var layar=(RectTransform)hp.hpUtama.transform.Find("LayarTerang");
        for(int i=0;i<hp.daftarIkon.Count;i++)
        {
            var t=(RectTransform)hp.daftarIkon[i].transform;t.sizeDelta=new Vector2(78,78);t.position=layar.TransformPoint(new Vector3((i%3-1)*99,125-i/3*105,0));
            var im=t.GetComponent<UnityEngine.UI.Image>();if(im!=null)im.preserveAspect=true;
        }
    }
    static void PasangSuara()
    {
        string[] actors={"Arthur_Player","NPC_Lyra","NPC_Nara_Ekspresi","NPC_Bimo_Joget"};string[] names={"Arthur","Lyra","Nara","Bimo"};float[] pitches={1f,.88f,1.28f,.72f};
        var clips=Enumerable.Range(1,3).Select(i=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AudioArthur/SuaraKertas/Kertas_"+i+".wav")).ToArray();
        for(int i=0;i<actors.Length;i++)
        {
            var actor=GameObject.Find(actors[i]);if(actor==null)throw new InvalidOperationException("Aktor tidak ditemukan: "+actors[i]);
            string path="Assets/AudioArthur/SuaraKertas/Profil_"+names[i]+".asset";
            var profile=AssetDatabase.LoadAssetAtPath<ProfilSuaraKertas>(path);
            if(profile==null){profile=ScriptableObject.CreateInstance<ProfilSuaraKertas>();profile.namaKarakter=names[i];profile.potongan=clips;profile.nada=pitches[i];profile.jeda=.066f+i*.008f;profile.volume=.4f;AssetDatabase.CreateAsset(profile,path);}
            var voice=actor.GetComponent<SuaraDialogKertas>();if(voice==null)voice=actor.AddComponent<SuaraDialogKertas>();voice.profil=profile;voice.sumber=voice.GetComponent<AudioSource>();if(voice.sumber==null)voice.sumber=actor.AddComponent<AudioSource>();
            voice.sumber.playOnAwake=false;voice.sumber.loop=false;voice.sumber.spatialBlend=0;EditorUtility.SetDirty(voice);
        }
    }
    static RectTransform Card(SistemHP hp,string name,string title,out GameObject root)
    {
        var r=R(hp.petaKampus.transform.parent,name,Vector2.zero,Vector2.zero);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;I(r,new Color(0,0,0,.28f),true);root=r.gameObject;
        var frame=Box(r,"BingkaiHP",Vector2.zero,new Vector2(1720,990),hp.warnaDefaultHP);frame.localScale=Vector3.one*.56f;
        var sync=root.AddComponent<BingkaiAplikasiHP>();sync.sistemHP=hp;sync.bingkai=frame.GetComponent<UnityEngine.UI.Image>();
        var sheet=Box(frame,"Kertas",Vector2.zero,new Vector2(1670,946),Paper);
        Text(sheet,"JudulApp",title,new Vector2(-130,388),new Vector2(1240,110),74,false,true);
        var close=Button(sheet,"KembaliKeHP","X",Vector2.zero,new Vector2(86,86),hp.TutupAplikasi);var cr=(RectTransform)close.transform;cr.anchorMin=cr.anchorMax=cr.pivot=Vector2.one;cr.anchoredPosition=new Vector2(-24,-24);close.GetComponentInChildren<TMP_Text>().gameObject.AddComponent<WigglyText>().kekuatanGoyang=1.5f;
        return sheet;
    }
    static RectTransform R(Transform parent,string name,Vector2 pos,Vector2 size)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;
    }
    static UnityEngine.UI.Image I(RectTransform r,Color c,bool ray){var i=r.gameObject.AddComponent<UnityEngine.UI.Image>();i.color=c;i.raycastTarget=ray;return i;}
    static void Outline(RectTransform r,float width){var o=r.gameObject.AddComponent<UnityEngine.UI.Outline>();o.effectColor=Ink;o.effectDistance=new Vector2(width,-width);}
    static RectTransform Box(Transform parent,string name,Vector2 pos,Vector2 size,Color color){var r=R(parent,name,pos,size);I(r,color,false);Outline(r,1.5f);return r;}
    static TMP_Text Text(Transform parent,string name,string text,Vector2 pos,Vector2 size,float font,bool body=false,bool wiggle=false)
    {
        var r=R(parent,name,pos,size);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=body?Body:Font;t.fontSize=font;t.text=text;t.color=Ink;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.overflowMode=TextOverflowModes.Ellipsis;
        if(wiggle)r.gameObject.AddComponent<WigglyText>().kekuatanGoyang=1.25f;return t;
    }
    static UnityEngine.UI.Button Button(Transform parent,string name,string text,Vector2 pos,Vector2 size,UnityAction action)
    {
        var r=Box(parent,name,pos,size,Paper);var image=r.GetComponent<UnityEngine.UI.Image>();image.raycastTarget=true;var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;var colors=b.colors;colors.disabledColor=new Color(1,1,1,.55f);b.colors=colors;if(action!=null)UnityEventTools.AddPersistentListener(b.onClick,action);
        Text(r,"Tulisan",text,Vector2.zero,size-new Vector2(20,8),37);return b;
    }
    static Slider Slider(Transform parent,string name,Vector2 pos,Vector2 size,UnityAction<float> action)
    {
        var r=R(parent,name,pos,size);var bg=I(r,new Color32(214,203,188,255),true);Outline(r,1.5f);
        var slider=r.gameObject.AddComponent<Slider>();slider.minValue=0;slider.maxValue=1;
        var fill=R(r,"Fill",Vector2.zero,Vector2.zero);fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;I(fill,Purple,false);slider.fillRect=fill;
        var handle=R(r,"Handle",Vector2.zero,new Vector2(25,12));var image=I(handle,Ink,true);slider.handleRect=handle;slider.targetGraphic=image;
        UnityEventTools.AddPersistentListener(slider.onValueChanged,action);return slider;
    }
}
