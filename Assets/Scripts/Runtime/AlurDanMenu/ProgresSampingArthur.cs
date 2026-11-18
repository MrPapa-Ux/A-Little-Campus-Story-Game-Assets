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
    }

    static Simpanan Baca()
    {
        var json = PlayerPrefs.GetString(Kunci, "");
        if (string.IsNullOrEmpty(json)) return new Simpanan();
        try
        {
            var simpanan = JsonUtility.FromJson<Simpanan>(json);
            if (simpanan?.titipan != null && simpanan.hadiah != null) return simpanan;
        }
        catch (ArgumentException) { }
        return new Simpanan();
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
    }

    public static void SimpanJikaEksplorasi(Scene scene)
    {
        if (scene.name == "Explorer" && PenghargaanDemoArthur.Terbuka) Simpan(scene);
    }
}
