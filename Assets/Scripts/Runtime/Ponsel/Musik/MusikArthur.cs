// PANDUAN PROYEK - Runtime / Ponsel/Musik
// Fungsi: Memutar playlist Arthur, lagu tema karakter, acak/ulang, jeda, serta menahan musik saat minigame.
// Panduan: Assets/Scripts/Runtime/Ponsel/Musik/README.md

using System;
using UnityEngine;

// Audio terpisah dari Canvas: menutup HP tidak mematikan lagu.
public class MusikArthur : MonoBehaviour
{
    public static MusikArthur Instance { get; private set; }
    public PlaylistArthur playlist;
    public AudioSource sumber, tema;
    public bool acak, ulang;
    public int Pilihan { get; private set; }
    public bool Dijeda { get; private set; }
    public bool TemaAktif => tema != null && tema.clip != null;
    public bool Memutar => sumber != null && sumber.isPlaying;
    public event Action Berubah;
    bool berjalan, appDijeda, lanjutSetelahTema;
    bool ditahanMinigame,lanjutMusikMinigame,lanjutTemaMinigame;
    public string Status { get; private set; } = "Kaset Arthur masih kosong.";
    public LaguArthur Lagu => playlist != null && playlist.lagu != null && Pilihan >= 0 && Pilihan < playlist.lagu.Length ? playlist.lagu[Pilihan] : null;
    public float Volume => sumber == null ? 0f : sumber.volume;
    public float Progres => sumber != null && sumber.clip != null && sumber.clip.length > 0 ? sumber.time/sumber.clip.length : 0f;

    void Awake()
    {
        if(Instance!=null && Instance!=this){Destroy(gameObject);return;}
        Instance=this;DontDestroyOnLoad(gameObject);
        sumber.playOnAwake=false;sumber.loop=false;sumber.spatialBlend=0;
        tema.playOnAwake=false;tema.spatialBlend=0;
        sumber.volume=tema.volume=Mathf.Clamp01(PlayerPrefs.GetFloat("ArthurMusicVolume",.65f));
        sumber.clip=Lagu?.audio;Status=sumber.clip==null?"Kaset Arthur masih kosong.":"Siap diputar.";
    }
    void Update()
    {
        // AudioSource menyambung sampel loop langsung, tanpa menunggu frame
        // berikutnya untuk memanggil Play lagi di ujung lagu.
        if(sumber!=null && sumber.loop!=ulang)sumber.loop=ulang;
        if(JedaSaatTidakAktif.Dijeda || ditahanMinigame || appDijeda || TemaAktif || !berjalan || Dijeda || sumber.isPlaying) return;
        if(sumber.clip!=null && sumber.clip.loadState==AudioDataLoadState.Loading)return;
        if(ulang){sumber.Play();return;}
        Berikutnya();
    }
    public void Pilih(int index)
    {
        if(playlist==null || playlist.lagu==null || index<0 || index>=playlist.lagu.Length)return;
        sumber.Stop();berjalan=false;Dijeda=false;lanjutSetelahTema=false;Pilihan=index;
        sumber.clip=Lagu?.audio;
        Status=sumber.clip==null ? "Lagu ini belum ada di kaset." : "Siap diputar.";Berubah?.Invoke();
    }
    public void PutarJeda()
    {
        if(TemaAktif){Status="Tema karakter sedang diputar.";Berubah?.Invoke();return;}
        if(Lagu?.audio==null){Status="Kaset ini menunggu lagu pertamanya.";Berubah?.Invoke();return;}
        if(sumber.isPlaying){sumber.Pause();Dijeda=true;Status="Dijeda.";}
        else {if(Dijeda)sumber.UnPause();else {sumber.clip=Lagu.audio;sumber.Play();}Dijeda=false;berjalan=true;Status="Sedang diputar.";}
        Berubah?.Invoke();
    }
    public void Berikutnya(){Langkah(1);}
    public void Sebelumnya(){Langkah(-1);}
    void Langkah(int arah)
    {
        int count=playlist?.lagu?.Length??0;
        if(count==0)return;
        int start=Pilihan;
        if(acak && arah>0)
        {
            int valid=0;for(int i=0;i<count;i++)if(playlist.lagu[i]?.audio!=null && i!=start)valid++;
            if(valid>0){int pick=UnityEngine.Random.Range(0,valid);for(int i=0;i<count;i++)if(playlist.lagu[i]?.audio!=null && i!=start && pick--==0){Pilih(i);if(!TemaAktif)PutarJeda();return;}}
        }
        for(int i=1;i<=count;i++){int next=(start+arah*i%count+count)%count;if(playlist.lagu[next]?.audio!=null){Pilih(next);if(!TemaAktif)PutarJeda();return;}}
        berjalan=false;Status="Belum ada lagu yang bisa diputar.";Berubah?.Invoke();
    }
    public void UbahAcak(){acak=!acak;Berubah?.Invoke();}
    public void UbahUlang(){ulang=!ulang;if(sumber!=null)sumber.loop=ulang;Berubah?.Invoke();}
    public void UbahVolume(float value){sumber.volume=tema.volume=Mathf.Clamp01(value);PlayerPrefs.SetFloat("ArthurMusicVolume",sumber.volume);}
    public void Geser(float value){if(sumber.clip!=null)sumber.time=Mathf.Clamp01(value)*Mathf.Max(0,sumber.clip.length-.03f);}
    public void MulaiTema(AudioClip clip)
    {
        if(clip==null)return;
        if(!TemaAktif){lanjutSetelahTema=sumber.isPlaying;if(lanjutSetelahTema)sumber.Pause();}
        tema.Stop();tema.clip=clip;tema.loop=true;tema.Play();Status="Tema karakter.";Berubah?.Invoke();
    }
    public void SelesaiTema()
    {
        tema.Stop();tema.clip=null;
        if(lanjutSetelahTema && sumber.clip!=null){sumber.UnPause();berjalan=true;Dijeda=false;Status="Sedang diputar.";}
        else Status=Dijeda?"Dijeda.":"Siap diputar.";
        lanjutSetelahTema=false;Berubah?.Invoke();
    }
    public void TahanMinigame(bool tahan)
    {
        if(ditahanMinigame==tahan)return;ditahanMinigame=tahan;
        if(tahan){lanjutMusikMinigame=sumber.isPlaying;lanjutTemaMinigame=tema.isPlaying;if(lanjutMusikMinigame)sumber.Pause();if(lanjutTemaMinigame)tema.Pause();}
        else {if(lanjutMusikMinigame)sumber.UnPause();if(lanjutTemaMinigame)tema.UnPause();lanjutMusikMinigame=lanjutTemaMinigame=false;}
    }
    public void HentikanSemua()
    {
        berjalan=Dijeda=lanjutSetelahTema=ditahanMinigame=lanjutMusikMinigame=lanjutTemaMinigame=false;
        sumber.Stop();tema.Stop();tema.clip=null;Status="Dihentikan.";Berubah?.Invoke();
    }
    void OnApplicationPause(bool pause){appDijeda=pause;if(pause)PlayerPrefs.Save();}
    void OnApplicationQuit(){PlayerPrefs.Save();}
    void OnDestroy(){if(Instance==this)Instance=null;}
}
