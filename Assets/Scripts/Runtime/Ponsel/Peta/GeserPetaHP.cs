// PANDUAN PROYEK - Runtime / Ponsel/Peta
// Fungsi: Menerjemahkan drag dan scroll menjadi pergeseran atau zoom peta.
// Panduan: Assets/Scripts/Runtime/Ponsel/Peta/README.md

using UnityEngine;
using UnityEngine.EventSystems;

public class GeserPetaHP : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
{
    public PetaKampusHP peta;
    Vector2 previous;
    public void OnBeginDrag(PointerEventData e) { RectTransformUtility.ScreenPointToLocalPointInRectangle(peta.viewport, e.position, e.pressEventCamera, out previous); }
    public void OnDrag(PointerEventData e)
    {
        if (Input.touchCount > 1) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(peta.viewport, e.position, e.pressEventCamera, out Vector2 next);
        peta.Geser(next - previous); previous = next;
    }
    public void OnScroll(PointerEventData e) { if (e.scrollDelta.y > 0) peta.Perbesar(); else if (e.scrollDelta.y < 0) peta.Perkecil(); }
}
