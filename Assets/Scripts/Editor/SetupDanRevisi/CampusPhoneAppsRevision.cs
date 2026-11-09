// PANDUAN PROYEK - Editor / SetupDanRevisi
// Fungsi: Menerapkan/revisi bingkai aplikasi HP dan bentuk ponsel.
// Panduan: Assets/Scripts/Editor/SetupDanRevisi/README.md

using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CampusPhoneAppsRevision
{
    public static void Apply()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Gunakan Edit Mode.");
        var h=UnityEngine.Object.FindFirstObjectByType<SistemHP>();var g=UnityEngine.Object.FindFirstObjectByType<RhythmArthur>();
        if(h.ponselBersama!=null)throw new InvalidOperationException("Bingkai bersama sudah terpasang.");
        var prefabRoot=PrefabUtility.GetOutermostPrefabInstanceRoot(h.panelPengaturan);
        if(prefabRoot!=null)PrefabUtility.UnpackPrefabInstance(prefabRoot,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        var canvas=h.panelPengaturan.GetComponentInParent<Canvas>();
        var frame=Rect("HP_Landscape_Bersama",canvas.transform,Vector2.zero,new Vector2(1830,1010));
        var common=frame.gameObject.AddComponent<PonselAplikasiBersama>();common.kelompok=frame.gameObject.AddComponent<CanvasGroup>();
        var body=Rect("BingkaiHPAsli",frame,Vector2.zero,new Vector2(1010,1830));body.localRotation=Quaternion.Euler(0,0,-90);common.gambar=body.gameObject.AddComponent<BingkaiPonselUI>();common.gambar.teksturHP=h.badanHP.sprite.texture;common.gambar.color=h.warnaDefaultHP;common.gambar.raycastTarget=false;
        common.layar=Rect("LayarAplikasi",frame,Vector2.zero,new Vector2(1660,900));var screenBG=common.layar.gameObject.AddComponent<UnityEngine.UI.Image>();screenBG.color=new Color32(239,231,211,255);screenBG.raycastTarget=false;
        var sourceX=h.panelPengaturan.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b=>b.name=="Btn_TutupX");
        var exit=UnityEngine.Object.Instantiate(sourceX.gameObject,common.layar,false);exit.name="X_KembaliKeHP";exit.SetActive(true);var exitRect=(RectTransform)exit.transform;Center(exitRect,new Vector2(777,400),new Vector2(76,76));
        common.keluar=exit.GetComponent<UnityEngine.UI.Button>();while(common.keluar.onClick.GetPersistentEventCount()>0)UnityEventTools.RemovePersistentListener(common.keluar.onClick,0);UnityEventTools.AddPersistentListener(common.keluar.onClick,h.TutupAplikasi);
        var exitText=exit.GetComponentInChildren<TMP_Text>(true);exitText.text="X";exitText.fontSize=50;
        var apps=new[]{h.petaKampus.gameObject,h.tas.gameObject,h.tugas.gameObject,h.pesan.gameObject,h.musik.gameObject};
        foreach(var app in apps)
        {
            Stretch((RectTransform)app.transform,common.layar);
            var dim=app.GetComponent<UnityEngine.UI.Image>();if(dim!=null){dim.color=Color.clear;dim.raycastTarget=false;}
            var old=(RectTransform)app.transform.Find("BingkaiHP");var image=old.GetComponent<UnityEngine.UI.Image>();if(image!=null)image.color=Color.clear;
            var outline=old.GetComponent<UnityEngine.UI.Outline>();if(outline!=null)outline.enabled=false;
            old.localScale=new Vector3(1660/old.rect.width,900/old.rect.height,1);
            foreach(var b in app.GetComponentsInChildren<UnityEngine.UI.Button>(true))if(b.name=="KembaliKeHP")b.gameObject.SetActive(false);
        }
        var settings=h.panelPengaturan;Stretch((RectTransform)settings.transform,common.layar);
        var settingsImage=settings.GetComponent<UnityEngine.UI.Image>();settingsImage.color=Color.clear;settingsImage.raycastTarget=false;
        var floating=settings.transform.Find("BingkaiMengambang");var boxSettings=settings.transform.Find("Box_Pengaturan");var confirmation=settings.transform.Find("KonfirmasiKeluar");
        var settingsGroup=Rect("IsiPengaturan",settings.transform,Vector2.zero,new Vector2(1308,748));
        floating.SetParent(settingsGroup,false);boxSettings.SetParent(settingsGroup,false);confirmation.SetParent(settingsGroup,false);
        settingsGroup.localScale=new Vector3(1660/1308f,900/748f,1);
        if(floating.parent!=settingsGroup)throw new InvalidOperationException("Parent pengaturan: "+floating.parent.name+" / target "+settingsGroup.name);
        var oldSettings=floating.GetComponent<UnityEngine.UI.Image>();oldSettings.color=Color.clear;var oldOutline=oldSettings.GetComponent<UnityEngine.UI.Outline>();if(oldOutline!=null)oldOutline.enabled=false;sourceX.gameObject.SetActive(false);
        // Ikon baru satu grid dengan ikon lainnya. Semua callback salinan dibersihkan.
        var template=h.daftarIkon.First(x=>x.name=="IkonKeluar");var icon=UnityEngine.Object.Instantiate(template,template.transform.parent,false);icon.name="IkonGame";
        var iconRect=(RectTransform)icon.transform;iconRect.anchoredPosition=new Vector2(667.12f,162.58f);iconRect.sizeDelta=new Vector2(78,78);
        var iconImage=icon.GetComponent<UnityEngine.UI.Image>();iconImage.sprite=null;iconImage.color=new Color32(74,137,142,255);iconImage.raycastTarget=true;
        var gamepad=Rect("GamepadCoretan",icon.transform,Vector2.zero,new Vector2(76,76));var art=gamepad.gameObject.AddComponent<IkonGameCoretan>();art.raycastTarget=false;
        var button=icon.GetComponent<UnityEngine.UI.Button>();while(button.onClick.GetPersistentEventCount()>0)UnityEventTools.RemovePersistentListener(button.onClick,0);UnityEventTools.AddStringPersistentListener(button.onClick,h.BukaAplikasi,"IkonGame");
        var selected=icon.transform.Find("TelahTerpilih");if(selected!=null){selected.gameObject.SetActive(false);selected.SetAsLastSibling();}
        ((RectTransform)template.transform).anchoredPosition=new Vector2(766.12f,162.58f);h.daftarIkon.Insert(h.daftarIkon.IndexOf(template),icon);
        // Rhythm mengisi layar bersama; bidang sentuh menjadi jauh lebih lebar.
        // Runtime rhythm mewarisi Canvas HP; Canvas lama akan mereset skala saat dipindah.
        UnityEngine.Object.DestroyImmediate(g.GetComponent<UnityEngine.UI.CanvasScaler>());
        UnityEngine.Object.DestroyImmediate(g.GetComponent<UnityEngine.UI.GraphicRaycaster>());
        UnityEngine.Object.DestroyImmediate(g.GetComponent<Canvas>());
        Stretch((RectTransform)g.transform,common.layar);
        var bridge=g.GetComponent<RapidTapArthur>();if(bridge!=null)bridge.enabled=false;
        var oldShade=g.panel.GetComponent<UnityEngine.UI.Image>();oldShade.color=Color.clear;oldShade.raycastTarget=false;
        var oldPhone=g.panel.transform.Find("BingkaiHPArthur");if(oldPhone!=null)oldPhone.gameObject.SetActive(false);
        var gameScreen=(RectTransform)g.panel.transform.Find("LayarGame");gameScreen.anchoredPosition=Vector2.zero;gameScreen.localScale=new Vector3(1660/1080f,900/680f,1);
        g.tombolKeluar.gameObject.SetActive(false);g.tombolKeluar=common.keluar;
        var pause=(RectTransform)g.tombolJeda.transform;pause.SetParent(gameScreen,false);Center(pause,new Vector2(-477,297),new Vector2(56,56));
        g.hud.fontSize=38;g.hud.fontStyle=FontStyles.Bold;g.hpInduk=h;
        common.keluar.transform.SetAsLastSibling();h.ponselBersama=common;h.gameRhythm=g;h.transformasiLayar.bingkaiBersama=common;
        // Canvas HP mengikuti resolusi perangkat; posisi dan ukuran desain tetap 1920x1080.
        var hpScaler=h.GetComponent<UnityEngine.UI.CanvasScaler>();hpScaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;hpScaler.referenceResolution=new Vector2(1920,1080);hpScaler.matchWidthOrHeight=.5f;
        var npc=GameObject.Find("NPC_Raka_RapidTap");
        if(npc!=null)
        {
            var area=npc.GetComponent<AreaInteraksi>();if(area==null||!area.daftarOpsi.Any(o=>o.rapidTap==bridge))throw new InvalidOperationException("Identitas NPC tes tidak sesuai; penghapusan dibatalkan.");
            Undo.DestroyObjectImmediate(npc);
        }
        RapikanBentukAsli();common.Sesuaikan();frame.gameObject.SetActive(false);EditorUtility.SetDirty(h);EditorUtility.SetDirty(g);EditorUtility.SetDirty(common);EditorUtility.SetDirty(h.transformasiLayar);EditorSceneManager.MarkSceneDirty(h.gameObject.scene);EditorSceneManager.SaveOpenScenes();
    }
    public static void RapikanBentukAsli()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Gunakan Edit Mode.");
        var h=UnityEngine.Object.FindFirstObjectByType<SistemHP>();var common=h.ponselBersama;var g=h.gameRhythm;
        // Bingkai landscape tipis dan simetris, tanpa tombol home pada sisi kiri.
        ((RectTransform)common.transform).sizeDelta=new Vector2(1860,1040);
        common.gambar.rectTransform.sizeDelta=new Vector2(1040,1860);
        var latar=common.transform.Find("LatarLayarSprite") as RectTransform;
        if(latar==null){latar=Rect("LatarLayarSprite",common.transform,new Vector2(22,4),new Vector2(1080,752));var bg=latar.gameObject.AddComponent<UnityEngine.UI.Image>();bg.color=new Color32(8,17,24,255);bg.raycastTarget=false;}
        latar.sizeDelta=new Vector2(1736,988);latar.anchoredPosition=Vector2.zero;latar.SetAsFirstSibling();
        common.layar.sizeDelta=new Vector2(1712,952);common.layar.anchoredPosition=Vector2.zero;
        Center((RectTransform)common.keluar.transform,new Vector2(803,422),new Vector2(76,76));
        common.keluar.GetComponentInChildren<TMP_Text>(true).fontSize=44;
        foreach(var app in new[]{h.petaKampus.gameObject,h.tas.gameObject,h.tugas.gameObject,h.pesan.gameObject,h.musik.gameObject})
        {
            var old=(RectTransform)app.transform.Find("BingkaiHP");old.localScale=new Vector3(1712/old.rect.width,952/old.rect.height,1);
        }
        var settings=(RectTransform)h.panelPengaturan.transform.Find("IsiPengaturan");settings.localScale=new Vector3(1712/1308f,952/748f,1);
        var gameScreen=(RectTransform)g.panel.transform.Find("LayarGame");gameScreen.localScale=new Vector3(1712/1080f,952/680f,1);
        foreach(var lane in g.panel.GetComponentsInChildren<RhythmJalurInput>(true))
        {
            var label=lane.GetComponentInChildren<TMP_Text>(true);label.text="KETUK";label.fontSize=36;
        }
        var guide=gameScreen.Find("Petunjuk").GetComponent<TMP_Text>();guide.text="KETUK TOMBOL SAAT NOT MENCAPAI GARIS";
        h.daftarIkon.First(i=>i.name=="IkonGame").GetComponent<UnityEngine.UI.Image>().color=Color.clear;
        var gambarGame=h.daftarIkon.First(i=>i.name=="IkonGame").transform.Find("GamepadCoretan");if(gambarGame!=null)gambarGame.gameObject.SetActive(false);
        common.Sesuaikan();common.gambar.SetVerticesDirty();EditorSceneManager.MarkSceneDirty(h.gameObject.scene);EditorSceneManager.SaveOpenScenes();
    }
    static RectTransform Rect(string name,Transform parent,Vector2 pos,Vector2 size)
    {
        var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);Center(r,pos,size);return r;
    }
    static void Center(RectTransform r,Vector2 pos,Vector2 size){r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;r.localScale=Vector3.one;}
    static void Stretch(RectTransform r,Transform parent){r.SetParent(parent,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;r.localScale=Vector3.one;r.localRotation=Quaternion.identity;}
}
