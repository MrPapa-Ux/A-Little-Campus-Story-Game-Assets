// PANDUAN PROYEK - Paket Chapel / ChapelPioneer
// Fungsi: Pengaturan tampilan atap, dinding belakang, balkon, dan alat musik chapel melalui Inspector.
// Panduan: Assets/ChapelPioneer/BACA DULU.md

using UnityEngine;

// Pilihan untuk menyesuaikan interior dengan kamera game 2.5D, tanpa proses per-frame.
public class PioneerChapelView : MonoBehaviour
{
    [Header("Bagian ruangan")]
    public GameObject atap, atapSayap, dindingBelakang, balkon, alatMusik;
    [Header("Tampilan potongan untuk kamera")]
    public bool bukaAtap, bukaBelakang, sembunyikanBalkon, sembunyikanAlatMusik;
    void OnEnable(){Terapkan();}
    void OnValidate(){Terapkan();}
    [ContextMenu("Terapkan Tampilan")]
    public void Terapkan()
    {
        if(atap!=null)atap.SetActive(!bukaAtap);
        if(atapSayap!=null)atapSayap.SetActive(!bukaAtap);
        if(dindingBelakang!=null)dindingBelakang.SetActive(!bukaBelakang);
        if(balkon!=null)balkon.SetActive(!sembunyikanBalkon);
        if(alatMusik!=null)alatMusik.SetActive(!sembunyikanAlatMusik);
    }
}
