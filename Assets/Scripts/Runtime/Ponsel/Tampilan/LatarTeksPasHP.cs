// PANDUAN PROYEK - Runtime / Ponsel/Tampilan
// Fungsi: Menyesuaikan ukuran latar teks dengan isi TMP setelah perubahan teks, dengan batas ukuran.
// Panduan: Assets/Scripts/Runtime/Ponsel/Tampilan/README.md

using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Ukur ulang hanya saat teks berubah, bukan setiap frame animasi wiggly.
public class LatarTeksPasHP : MonoBehaviour
{
    public TMP_Text teks;
    public Vector2 ruangTepi=new Vector2(36,18);
    public Vector2 ukuranMinimum=new Vector2(130,76),ukuranMaksimum=new Vector2(500,150);
    bool perluDisesuaikan;
    RectTransform bidang;
    void Awake(){bidang=(RectTransform)transform;}
    void OnEnable(){TMPro_EventManager.TEXT_CHANGED_EVENT.Add(TeksBerubah);Sesuaikan();}
    void OnDisable(){TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(TeksBerubah);perluDisesuaikan=false;}
    // TMP memanggil ini dari dalam rebuild Canvas. Jangan mengubah UI di sini.
    void TeksBerubah(Object changed){if(changed==teks)perluDisesuaikan=true;}
    public void Sesuaikan(){perluDisesuaikan=true;}
    void LateUpdate()
    {
        if(!perluDisesuaikan||teks==null||!teks.isActiveAndEnabled)return;
        if(CanvasUpdateRegistry.IsRebuildingGraphics()||CanvasUpdateRegistry.IsRebuildingLayout())return;
        perluDisesuaikan=false;
        var preferred=teks.GetPreferredValues(teks.text,1000,float.PositiveInfinity);
        var size=new Vector2(Mathf.Clamp(preferred.x+ruangTepi.x,ukuranMinimum.x,ukuranMaksimum.x),Mathf.Clamp(preferred.y+ruangTepi.y,ukuranMinimum.y,ukuranMaksimum.y));
        if((bidang.sizeDelta-size).sqrMagnitude>.01f)bidang.sizeDelta=size;
    }
}
