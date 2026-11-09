// PANDUAN PROYEK - Runtime / MiniGame/Rhythm
// Fungsi: Mengirim sentuhan pada tombol jalur ke RhythmArthur.
// Panduan: Assets/Scripts/Runtime/MiniGame/Rhythm/README.md

using UnityEngine;
using UnityEngine.EventSystems;

public class RhythmJalurInput : MonoBehaviour,IPointerDownHandler
{
    public RhythmArthur game;
    [Range(0,3)] public int jalur;
    public void OnPointerDown(PointerEventData e)
    {
        if(e.button==PointerEventData.InputButton.Left&&game!=null)game.PukulJalur(jalur);
    }
}
