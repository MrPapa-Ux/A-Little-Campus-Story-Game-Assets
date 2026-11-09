// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menerapkan susunan/revisi yang dibuat pada checkpoint 14.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class CampusCheckpoint14
{
    static TMP_FontAsset Body=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Kembali ke Edit Mode.");
        var h=UnityEngine.Object.FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include);
        foreach(var app in new[]{h.pesan,h.tas,h.tugas})Footer(h,app);
        Messages(h.pesan);Bag(h.tas);Map(h);
        EditorUtility.SetDirty(h);EditorSceneManager.MarkSceneDirty(h.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();
    }
    static void Footer(SistemHP h,AplikasiCatatanHP app)
    {
        var sheet=app.transform.Find("BingkaiHP/Kertas");
        var count=app.jumlah;count.rectTransform.anchoredPosition=new Vector2(-419,-337);count.rectTransform.sizeDelta=new Vector2(590,48);Readable(count,36);count.fontStyle=FontStyles.Bold;
        foreach(string name in new[]{"Kiri","Kanan"})
        {
            var r=(RectTransform)sheet.Find(name);r.anchoredPosition=new Vector2(name=="Kiri"?-650:-188,-418);r.sizeDelta=new Vector2(120,88);
            Readable(r.GetComponentInChildren<TMP_Text>(true),46);
        }
        var owner=sheet.Find("Pemilik");if(owner!=null)owner.gameObject.SetActive(false);
        string source=app.jenis==JenisBukuArthur.Tas?"IkonTas":app.jenis==JenisBukuArthur.Pesan?"IkonPesan":"IkonToDoLIst";
        Icon(sheet,"IkonNavigasi",h.daftarIkon.First(x=>x.name==source).GetComponent<Image>().sprite,new Vector2(-419,-418),new Vector2(92,92));
        var action=(RectTransform)app.aksi.transform;action.anchoredPosition=new Vector2(348,-418);action.sizeDelta=new Vector2(720,88);
        Readable(app.labelAksi,33);app.labelAksi.fontStyle=FontStyles.Bold;app.labelAksi.rectTransform.sizeDelta=new Vector2(690,78);
        app.labelAksi.color=app.jenis==JenisBukuArthur.Pesan?new Color32(246,250,223,255):new Color32(35,26,20,255);
        var w=app.labelAksi.GetComponent<WigglyText>();if(w!=null)w.kekuatanGoyang=2;
        app.Segarkan();EditorUtility.SetDirty(app);
    }
    static void Messages(AplikasiCatatanHP app)
    {
        var sheet=app.transform.Find("BingkaiHP/Kertas");
        var inbox=(RectTransform)sheet.Find("LayarKontak");inbox.anchoredPosition=new Vector2(-419,20);inbox.sizeDelta=new Vector2(670,592);
        var title=sheet.Find("Subjudul").GetComponent<TMP_Text>();title.rectTransform.anchoredPosition=new Vector2(-419,271);title.rectTransform.sizeDelta=new Vector2(590,70);Readable(title,38);title.fontStyle=FontStyles.Bold;
        for(int i=0;i<app.tombolBaris.Length;i++){var r=(RectTransform)app.tombolBaris[i].transform;r.anchoredPosition=new Vector2(-419,178-i*98.5f);r.sizeDelta=new Vector2(595,90);}
        var footer=sheet.Find("LayarPercakapan/FooterLayar");if(footer!=null)footer.gameObject.SetActive(false);
        Readable(app.statusKontak,30);app.statusKontak.rectTransform.sizeDelta=new Vector2(560,55);
        app.Segarkan();
    }
    static void Bag(AplikasiCatatanHP app)
    {
        var root=app.transform.Find("BingkaiHP/Kertas/BagianDalamTas");
        var hint=root.Find("LabelKompartemen").GetComponent<TMP_Text>();hint.text="Tarik ritsleting\nuntuk pindah halaman";hint.rectTransform.anchoredPosition=new Vector2(0,-237);hint.rectTransform.sizeDelta=new Vector2(630,100);Readable(hint,36);hint.fontStyle=FontStyles.Bold;hint.color=new Color32(255,241,215,255);
        app.pratinjauBarang.rectTransform.anchoredPosition=Vector2.zero;app.pratinjauBarang.rectTransform.sizeDelta=new Vector2(380,190);
        foreach(var label in app.labelBaris){label.fontSize=28;label.rectTransform.sizeDelta=new Vector2(174,65);label.enableAutoSizing=false;}
        var demo=app.buku.GetComponent<ContohIsiTasHP>();if(demo==null)demo=app.buku.gameObject.AddComponent<ContohIsiTasHP>();demo.buku=app.buku;demo.aktifkanContoh=true;demo.hanyaDiEditor=true;
        string[] files={"banana","bandage_box","ball_pen","cookies","potatochip_yellow","water","glue_stick","receipt"};
        string[] names={"Pisang","Plester","Pena","Biskuit","Keripik","Air minum","Lem stik","Struk"};
        string[] descriptions={"Pisang untuk mengisi tenaga di sela kelas.","Sekotak plester untuk luka kecil.","Pena untuk mencatat sebelum lupa.","Biskuit untuk teman istirahat.","Sebungkus keripik renyah.","Bekal air minum untuk hari ini.","Lem stik untuk merapikan tugas.","Struk belanja yang terselip di tas."};
        demo.contoh=new CatatanArthur[files.Length];
        for(int i=0;i<files.Length;i++)
        {
            string path="Assets/ContohTasArthur/"+files[i]+".png";
            if(!AssetDatabase.IsValidFolder("Assets/ContohTasArthur"))AssetDatabase.CreateFolder("Assets","ContohTasArthur");
            if(AssetDatabase.LoadAssetAtPath<Texture2D>(path)==null)AssetDatabase.CopyAsset("Assets/All Asset/Sprites/Icon/Pixel_Mart/"+files[i]+".png",path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.isReadable=false;importer.filterMode=FilterMode.Point;importer.maxTextureSize=128;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite==null)throw new InvalidOperationException("Sprite contoh belum tersedia: "+path);
            demo.contoh[i]=new CatatanArthur{id="contoh_tas_"+files[i],judul=names[i],isi=descriptions[i],ikon=sprite,jumlah=1};
        }
        EditorUtility.SetDirty(demo);app.Segarkan();
    }
    static void Map(SistemHP h)
    {
        var button=h.petaKampus.GetComponentsInChildren<Button>(true).First(x=>x.name=="Ikhtisar");
        var label=button.GetComponentInChildren<TMP_Text>(true);if(label!=null)label.gameObject.SetActive(false);
        Icon(button.transform,"IkonMapIkhtisar",h.daftarIkon.First(x=>x.name=="IkonMap").GetComponent<Image>().sprite,Vector2.zero,new Vector2(86,86));
    }
    static void Readable(TMP_Text t,float size){t.font=Body;t.fontSize=size;t.enableAutoSizing=false;t.overflowMode=TextOverflowModes.Ellipsis;EditorUtility.SetDirty(t);}
    static void Icon(Transform parent,string name,Sprite sprite,Vector2 pos,Vector2 size)
    {
        var r=parent.Find(name) as RectTransform;if(r==null){r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.gameObject.AddComponent<Image>();}
        r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;
        var im=r.GetComponent<Image>();im.sprite=sprite;im.color=Color.white;im.preserveAspect=true;im.raycastTarget=false;
        var w=r.GetComponent<WigglyBox>();if(w==null)w=r.gameObject.AddComponent<WigglyBox>();w.kekuatanGoyangPosisi=1.2f;w.kekuatanGoyangRotasi=1.2f;w.kecepatanAnimasi=7;
    }
}
