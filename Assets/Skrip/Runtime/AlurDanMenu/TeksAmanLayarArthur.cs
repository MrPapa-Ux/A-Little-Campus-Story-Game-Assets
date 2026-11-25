using UnityEngine;
using TMPro;

// Keeps the authored text panel inside the visible display on wide phones and notches.
[RequireComponent(typeof(TMP_Text))]
public sealed class TeksAmanLayarArthur : MonoBehaviour
{
    readonly Vector3[] corners = new Vector3[4];
    void LateUpdate()
    {
        var text = GetComponent<TMP_Text>();
        var canvas = text.canvas;
        var rect = text.rectTransform;
        if (canvas == null || !(rect.parent is RectTransform parent)) return;
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Rect safe = new Rect(0, 0, Screen.width, Screen.height);
#if UNITY_ANDROID && !UNITY_EDITOR
        safe = Screen.safeArea;
#endif
        float margin = Mathf.Min(Screen.width, Screen.height) * .025f;
        safe.xMin += margin; safe.xMax -= margin;
        safe.yMin += margin; safe.yMax -= margin;
        rect.GetWorldCorners(corners);
        var min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        var max = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
        Vector2 delta = Vector2.zero;
        if (min.x < safe.xMin) delta.x = safe.xMin - min.x;
        else if (max.x > safe.xMax) delta.x = safe.xMax - max.x;
        if (min.y < safe.yMin) delta.y = safe.yMin - min.y;
        else if (max.y > safe.yMax) delta.y = safe.yMax - max.y;
        if (delta.sqrMagnitude < .25f) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,min,camera,out var before);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,min+delta,camera,out var after);
        rect.anchoredPosition += after-before;
    }
}
