using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

// Berlaku pada salinan scene saat build Android. Scene kerja dan tampilan PC tidak disunting.
public sealed class AndroidSceneRenderSettings : IProcessSceneWithReport
{
    public int callbackOrder => 1000;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (report == null || report.summary.platform != UnityEditor.BuildTarget.Android) return;
        var roots = scene.GetRootGameObjects();
        var canvases = roots.SelectMany(root => root.GetComponentsInChildren<Canvas>(true)).ToArray();
        foreach (var arthur in roots.SelectMany(root => root.GetComponentsInChildren<ArthurJalurKampus>(true)))
        {
            var camera = arthur.kamera;
            if (camera == null) continue;
            float distance = 80f;
            foreach (var canvas in canvases)
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == camera)
                    distance = Mathf.Max(distance, canvas.planeDistance + 1f);
            // Pertahankan jarak pandang kamera asli; jangan hilangkan world jauh
            // khusus pada build Android. Canvas tetap harus masuk jangkauan.
            camera.farClipPlane = Mathf.Max(camera.farClipPlane, distance);
            Debug.Log("ARTHUR_BUILD_CAMERA scene=" + scene.name + " far=" + camera.farClipPlane);
        }
    }
}
