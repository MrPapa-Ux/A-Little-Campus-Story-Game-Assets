using TMPro;
using UnityEngine;

// Jam dunia permainan, bukan jam laptop/HP; satu hari permainan tetap 12 menit.
public sealed class JamKampusHP : MonoBehaviour
{
    public TMP_Text teks;
    int menitTerakhir=-1;
    void OnEnable(){menitTerakhir=-1;Perbarui();}
    void Update(){Perbarui();}
    public static string FormatJam(float jam)
    {
        int menit=Mathf.FloorToInt(Mathf.Repeat(jam,24)*60);
        return (menit/60).ToString("00")+":"+(menit%60).ToString("00");
    }
    void Perbarui()
    {
        if(teks==null)return;
        float jam=SiklusWaktuKampus.Instance!=null?SiklusWaktuKampus.Instance.Jam:SiklusWaktuKampus.JamMulaiPermainan;
        int menit=Mathf.FloorToInt(Mathf.Repeat(jam,24)*60);
        if(menit==menitTerakhir)return;
        menitTerakhir=menit;teks.text=FormatJam(jam);
    }
}
