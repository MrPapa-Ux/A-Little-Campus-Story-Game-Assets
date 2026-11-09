using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Editor-only controls for place anchors. Route geometry is never edited here.
[InitializeOnLoad]
public static class BubbleArthurScene
{
    static InteraksiJalurKampus selected;
    static Tool previousTool;
    static bool hidden;
    static int dragGroup = -1;
    public static bool IkutiJalur
    {
        get => EditorPrefs.GetBool("Arthur.Bubble.IkutiJalur", true);
        set => EditorPrefs.SetBool("Arthur.Bubble.IkutiJalur", value);
    }
    static BubbleArthurScene()
    {
        SceneView.duringSceneGui += Draw;
        Selection.selectionChanged += SelectionChanged;
        AssemblyReloadEvents.beforeAssemblyReload += Restore;
        EditorApplication.playModeStateChanged += _ => Restore();
    }
    static void SelectionChanged()
    {
        if (selected != null && Selection.activeGameObject != selected.gameObject) Restore();
    }
    static void Restore()
    {
        FinishDrag();
        if (hidden && Tools.current == Tool.None) Tools.current = previousTool;
        hidden = false;
        selected = null;
    }
    static void FinishDrag()
    {
        if (dragGroup >= 0) Undo.CollapseUndoOperations(dragGroup);
        dragGroup = -1;
    }
    public static void Pilih(InteraksiJalurKampus marker)
    {
        if (marker == null) return;
        Restore();
        // Selection changes also restore the existing green path tool first.
        Selection.activeGameObject = marker.gameObject;
        selected = marker;
        previousTool = Tools.current;
        hidden = true;
        Tools.current = Tool.None;
        SceneView.RepaintAll();
    }
    public static string Ringkasan(InteraksiJalurKampus marker)
    {
        var area = marker.GetComponent<AreaInteraksi>();
        return area != null && area.daftarOpsi != null && area.daftarOpsi.Length > 0
            ? string.Join(" / ", area.daftarOpsi.Where(o => o != null).Select(o => o.teksOpsi))
            : marker.name;
    }
    public static void Geser(InteraksiJalurKampus marker, Vector3 next, bool snap)
    {
        if (marker == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (snap && marker.jalur != null)
            marker.jalur.Sampel(marker.jalur.JarakTerdekat(next), out next, out _, out _);
        var old = marker.transform.position;
        if ((old - next).sqrMagnitude < .00000001f) return;
        // Arrival points sharing this place stay paired with its interaction anchor.
        var arrivals = Object.FindObjectsByType<TujuanJalurKampus>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(t => t.jalur == marker.jalur && marker.jalur != null &&
                t.transform != marker.transform && !t.transform.IsChildOf(marker.transform) &&
                (t.transform.position - old).sqrMagnitude < .0001f).ToArray();
        var transforms = arrivals.Select(t => t.transform).Prepend(marker.transform).ToArray();
        Undo.RecordObjects(transforms, "Geser tempat bubble Arthur");
        var delta = next - old;
        foreach (var t in transforms)
        {
            t.position += delta;
            PrefabUtility.RecordPrefabInstancePropertyModifications(t);
            EditorUtility.SetDirty(t);
            EditorSceneManager.MarkSceneDirty(t.gameObject.scene);
        }
        SceneView.RepaintAll();
    }
    static void Draw(SceneView view)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || !view.drawGizmos) return;
        var previousColor = Handles.color;
        foreach (var marker in Object.FindObjectsByType<InteraksiJalurKampus>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!marker.isActiveAndEnabled || !marker.gameObject.scene.IsValid()) continue;
            // Use the existing text-icon location, so clicking the blue T area selects its parent place.
            var p = marker.ikon != null ? marker.ikon.position : marker.transform.position + Vector3.up;
            if ((p - marker.transform.position).sqrMagnitude > 25) p = marker.transform.position + Vector3.up;
            var screen = view.camera.WorldToViewportPoint(p);
            if (screen.z <= 0 || screen.z > 100 || screen.x < -.05f || screen.x > 1.05f || screen.y < -.05f || screen.y > 1.05f) continue;
            float size = HandleUtility.GetHandleSize(p) * .13f;
            Handles.color = new Color(1, .62f, .12f);
            if (Handles.Button(p, view.camera.transform.rotation, size, size * 1.4f, Handles.CircleHandleCap)) Pilih(marker);
            Handles.Label(p + view.camera.transform.up * size * 1.5f, new GUIContent("BUBBLE", "Klik untuk membaca fungsi dan menggeser tempat interaksi."));
        }
        var active = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<InteraksiJalurKampus>() : null;
        if (active != null && selected == null) Pilih(active);
        if (selected != null)
        {
            var foot = selected.transform.position - Vector3.up * ArthurJalurKampus.JarakKaki;
            Handles.color = new Color(1, .62f, .12f);
            Handles.DrawWireDisc(foot, Vector3.up, selected.radius);
            Handles.Label(foot + Vector3.up * .35f, Ringkasan(selected));
            EditorGUI.BeginChangeCheck();
            var next = Handles.PositionHandle(foot, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                if (dragGroup < 0) { Undo.IncrementCurrentGroup(); dragGroup = Undo.GetCurrentGroup(); }
                Geser(selected, next + Vector3.up * ArthurJalurKampus.JarakKaki, IkutiJalur);
            }
        }
        if (GUIUtility.hotControl == 0) FinishDrag();
        Handles.color = previousColor;
    }
}
