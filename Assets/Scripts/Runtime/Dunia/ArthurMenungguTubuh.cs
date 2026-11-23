using UnityEngine;

public sealed class ArthurMenungguTubuh : MonoBehaviour
{
    public EkspresiWajah2D wajah;
    public SpriteRenderer kepalaArthur,rambutArthur;
    public Sprite spriteKepalaArthur,spriteRambutArthur;
    void JagaSpriteArthur(){if(kepalaArthur!=null&&spriteKepalaArthur!=null)kepalaArthur.sprite=spriteKepalaArthur;if(rambutArthur!=null&&spriteRambutArthur!=null)rambutArthur.sprite=spriteRambutArthur;}
    void OnEnable(){JagaSpriteArthur();if(wajah!=null){wajah.animasiKedip=true;wajah.durasiKedip=999999;wajah.PaksaKedip();}}
    void LateUpdate(){JagaSpriteArthur();if(wajah!=null&&!wajah.SedangKedip)wajah.PaksaKedip();}
}
