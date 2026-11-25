using System;
using UnityEngine;

// Audio ronde terpisah dari playlist dan efek suara permainan.
internal sealed class MusikMinigameArthur
{
    AudioSource sumber;
    MusikArthur pemutar;
    bool aktif;

    internal static AudioClip LaguTapTipTup() => Resources.Load<AudioClip>("ArthurSfx/RhythmCampus");

    internal static AudioClip LaguPonsel(string judul)
    {
        var lagu = MusikArthur.Instance?.playlist?.lagu;
        return lagu == null ? null : Array.Find(lagu, entri => entri!=null&&entri.judul.Replace(" ", "")==judul.Replace(" ", ""))?.audio;
    }

    internal void Putar(GameObject pemilik, AudioClip klip)
    {
        if (klip == null) return;
        if (!aktif)
        {
            aktif = true;
            pemutar = MusikArthur.Instance;
            if (pemutar != null) pemutar.TahanMinigame(true);
        }
        if (sumber == null)
        {
            sumber = pemilik.AddComponent<AudioSource>();
            sumber.playOnAwake = false;
            sumber.spatialBlend = 0;
            sumber.loop = true;
        }
        sumber.Stop();
        sumber.clip = klip;
        sumber.volume = (pemutar != null ? pemutar.Volume : PlayerPrefs.GetFloat("ArthurMusicVolume", .65f)) * .7f;
        sumber.Play();
    }

    internal void Hentikan()
    {
        if (sumber != null) sumber.Stop();
    }

    internal void Tutup()
    {
        if (!aktif) return;
        Hentikan();
        aktif = false;
        if (pemutar != null) pemutar.TahanMinigame(false);
        pemutar = null;
    }
}
