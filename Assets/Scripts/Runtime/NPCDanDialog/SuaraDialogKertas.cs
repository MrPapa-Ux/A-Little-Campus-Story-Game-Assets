// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Memutar potongan bunyi kertas mengikuti huruf dialog menggunakan profil karakter.
// Panduan: Assets/Scripts/Runtime/NPCDanDialog/README.md

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SuaraDialogKertas : MonoBehaviour
{
    public ProfilSuaraKertas profil;
    public AudioSource sumber;
    public int JumlahBunyi {get;private set;}
    int huruf;
    float berikut;
    void Awake(){if(sumber==null)sumber=GetComponent<AudioSource>();sumber.playOnAwake=false;sumber.spatialBlend=0;sumber.loop=false;}
    public void Huruf(char c)
    {
        if(!char.IsLetterOrDigit(c)){if(char.IsPunctuation(c))sumber.Stop();return;}
        if(profil==null || profil.potongan==null || profil.potongan.Length==0)return;
        if(huruf++%Mathf.Max(1,profil.hurufPerBunyi)!=0 || Time.unscaledTime<berikut)return;
        var clip=profil.potongan[JumlahBunyi%profil.potongan.Length];if(clip==null)return;
        berikut=Time.unscaledTime+profil.jeda;
        sumber.pitch=Mathf.Clamp(profil.nada+(((c*17+JumlahBunyi*7)%11)/5f-1f)*profil.variasiNada,.5f,2f);
        sumber.Stop();sumber.PlayOneShot(clip,profil.volume);JumlahBunyi++;
    }
    public void Hentikan(){if(sumber!=null)sumber.Stop();huruf=0;berikut=0;}
    [ContextMenu("Dengarkan contoh suara (Play Mode)")]
    public void CobaSuara(){if(Application.isPlaying){berikut=0;huruf=0;Huruf('A');}}
    void OnDisable(){Hentikan();}
}
