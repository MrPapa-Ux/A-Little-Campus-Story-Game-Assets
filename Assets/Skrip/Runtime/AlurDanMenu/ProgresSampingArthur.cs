using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Simpan hasil Chapter 1 yang diperlukan untuk meneruskan aktivitas di Eksplorasi.
public static class ProgresSampingArthur
{
    public const string Kunci = "Explorer.ProgresSamping.v1";

    [Serializable] sealed class Titipan
    {
        public string id;
        public TahapTitipanArthur tahap;
    }

    [Serializable] sealed class Simpanan
    {
        public List<Titipan> titipan = new List<Titipan>();
        public List<string> hadiah = new List<string>();
        public List<string> adegan = new List<string>();
        public List<Titipan> kontak = new List<Titipan>();
    }

    static Simpanan Baca()
    {
        var json = PlayerPrefs.GetString(Kunci, "");
        if (string.IsNullOrEmpty(json)) return new Simpanan();
        try
        {
            var simpanan = JsonUtility.FromJson<Simpanan>(json);
            if (simpanan?.titipan != null && simpanan.hadiah != null) {
                if(simpanan.adegan==null)simpanan.adegan=new List<string>();
                return simpanan;
            }
        }
        catch (ArgumentException) { }
        return new Simpanan();
    }
    public static int TahapKontak(Scene scene,string id)
    {
        if(scene.name!="Explorer"||!PenghargaanDemoArthur.Terbuka)return 0;
        var data=Baca().kontak?.Find(x=>x.id==id);return data==null?0:(int)data.tahap;
    }
    public static void SimpanKontak(string id,int tahap)
    {
        var s=Baca();if(s.kontak==null)s.kontak=new List<Titipan>();
        var item=s.kontak.Find(x=>x.id==id);
        if(item==null)s.kontak.Add(new Titipan{id=id,tahap=(TahapTitipanArthur)tahap});else item.tahap=(TahapTitipanArthur)Math.Max((int)item.tahap,tahap);
        PlayerPrefs.SetString(Kunci,JsonUtility.ToJson(s));PlayerPrefs.Save();
    }

    public static void Simpan(Scene scene)
    {
        var simpanan = Baca();
        foreach (var quest in UnityEngine.Object.FindObjectsByType<QuestTitipanArthur>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (quest.gameObject.scene != scene || string.IsNullOrEmpty(quest.idQuest) ||
                quest.Tahap == TahapTitipanArthur.BelumDiterima) continue;
            var lama = simpanan.titipan.Find(x => x != null && x.id == quest.idQuest);
            if (lama == null) simpanan.titipan.Add(new Titipan { id = quest.idQuest, tahap = quest.Tahap });
            else if (quest.Tahap > lama.tahap) lama.tahap = quest.Tahap;
        }
        foreach (var tantangan in UnityEngine.Object.FindObjectsByType<TantanganSampingArthur>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (tantangan.gameObject.scene == scene && tantangan.Selesai && !simpanan.hadiah.Contains(tantangan.IdHadiah))
                simpanan.hadiah.Add(tantangan.IdHadiah);
        }
        foreach(var adegan in UnityEngine.Object.FindObjectsByType<AdeganNpcTaman>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(adegan.gameObject.scene==scene&&adegan.SudahSelesai&&!string.IsNullOrEmpty(adegan.idProgres)&&!simpanan.adegan.Contains(adegan.idProgres))
                simpanan.adegan.Add(adegan.idProgres);
        PlayerPrefs.SetString(Kunci, JsonUtility.ToJson(simpanan));
        PlayerPrefs.Save();
    }

    public static void Pulihkan(Scene scene)
    {
        if (scene.name != "Explorer" || !PenghargaanDemoArthur.Terbuka) return;
        var simpanan = Baca();
        foreach (var quest in UnityEngine.Object.FindObjectsByType<QuestTitipanArthur>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (quest.gameObject.scene != scene) continue;
            var data = simpanan.titipan.Find(x => x != null && x.id == quest.idQuest);
            if (data != null) quest.Pulihkan(data.tahap);
        }
        foreach (var tantangan in UnityEngine.Object.FindObjectsByType<TantanganSampingArthur>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (tantangan.gameObject.scene == scene && simpanan.hadiah.Contains(tantangan.IdHadiah))
                tantangan.PulihkanHadiah();
        foreach(var adegan in UnityEngine.Object.FindObjectsByType<AdeganNpcTaman>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(adegan.gameObject.scene==scene&&!string.IsNullOrEmpty(adegan.idProgres)&&simpanan.adegan.Contains(adegan.idProgres))
                adegan.PulihkanSelesai();
    }

    public static void SimpanJikaEksplorasi(Scene scene)
    {
        if (scene.name == "Explorer" && PenghargaanDemoArthur.Terbuka) Simpan(scene);
    }
}
