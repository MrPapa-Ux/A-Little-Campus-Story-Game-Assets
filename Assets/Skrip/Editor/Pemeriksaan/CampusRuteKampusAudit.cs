using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CampusRuteKampusAudit
{
    const string Folder = "work/rute-kampus/";
    static CampusRuteKampusAudit() { EditorApplication.update += Tick; }
    static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
    static string V(Vector3 v) => v.ToString("F3");
    static void Tick()
    {
        if (AssetDatabase.IsAssetImportWorkerProcess() || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        if (!File.Exists(Folder + "audit.request")) return;
        File.Delete(Folder + "audit.request");
        try { Audit(); } catch (Exception e) { File.WriteAllText(Folder + "audit-error.txt", e.ToString()); }
    }
    static void Audit()
    {
        Directory.CreateDirectory(Folder);
        var b = new StringBuilder();
        b.AppendLine("Time=" + DateTime.Now.ToString("O") + " Play=" + EditorApplication.isPlaying + " Paused=" + EditorApplication.isPaused);
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            b.AppendLine("SCENE " + s.path + " loaded=" + s.isLoaded + " dirty=" + s.isDirty);
            if (!s.isLoaded) continue;
            foreach (var root in s.GetRootGameObjects())
            {
                b.AppendLine("ROOT " + root.name + " active=" + root.activeSelf + " pos=" + V(root.transform.position) + " rot=" + V(root.transform.eulerAngles) + " scale=" + V(root.transform.lossyScale) + " hidden=" + SceneVisibilityManager.instance.IsHidden(root));
                if (root.name.StartsWith("World_") || root.name == "AsramaJasmineDanSekitarnya")
                {
                    var rs = root.GetComponentsInChildren<Renderer>(true);
                    var bounds = new Bounds(); bool first = true;
                    foreach (var r in rs) { if (first) { bounds = r.bounds; first = false; } else bounds.Encapsulate(r.bounds); }
                    b.AppendLine("WORLD renderers=" + rs.Length + " enabled=" + rs.Count(r => r.enabled && r.gameObject.activeInHierarchy) + " hidden=" + rs.Count(r => SceneVisibilityManager.instance.IsHidden(r.gameObject)) + " bounds=" + bounds);
                    continue;
                }
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    var components = t.GetComponents<Component>();
                    bool useful = t == root.transform || t.GetComponent<Collider2D>() || t.GetComponent<Camera>() || t.GetComponent<AreaInteraksi>() || t.GetComponent<KarakterController>() || t.GetComponent<NPCBubbleTrigger>() || t.GetComponent<Animator>() || t.name.IndexOf("Titik", StringComparison.OrdinalIgnoreCase) >= 0 || t.name.IndexOf("Tititk", StringComparison.OrdinalIgnoreCase) >= 0 || t.name.IndexOf("GK", StringComparison.OrdinalIgnoreCase) >= 0 || t.name.IndexOf("Kantin", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!useful) continue;
                    b.AppendLine("OBJ " + PathOf(t) + " active=" + t.gameObject.activeInHierarchy + " self=" + t.gameObject.activeSelf + " pos=" + V(t.position) + " local=" + V(t.localPosition) + " rot=" + V(t.eulerAngles) + " scale=" + V(t.lossyScale) + " components=" + string.Join(",", components.Select(c => c == null ? "MISSING" : c.GetType().Name)));
                    foreach (var c in t.GetComponents<Collider2D>()) b.AppendLine(" COLLIDER " + EditorJsonUtility.ToJson(c));
                    foreach (var c in components.OfType<MonoBehaviour>())
                        if (c is AreaInteraksi || c is KameraNgekor || c is KameraGoyang || c is KarakterController || c is SistemPilihan || c is BingkaiKameraTetap) b.AppendLine(" DATA " + c.GetType().Name + " " + EditorJsonUtility.ToJson(c));
                    var cam = t.GetComponent<Camera>(); if (cam) b.AppendLine(" CAMERA fov=" + cam.fieldOfView + " ortho=" + cam.orthographic + " near=" + cam.nearClipPlane + " far=" + cam.farClipPlane + " enabled=" + cam.enabled + " aspect=" + cam.aspect);
                }
            }
        }
        foreach (SceneView sv in SceneView.sceneViews) b.AppendLine("SCENEVIEW pivot=" + V(sv.pivot) + " rot=" + V(sv.rotation.eulerAngles) + " size=" + sv.size + " ortho=" + sv.orthographic + " camera=" + V(sv.camera.transform.position));
        File.WriteAllText(Folder + "audit.txt", b.ToString());
    }
}
