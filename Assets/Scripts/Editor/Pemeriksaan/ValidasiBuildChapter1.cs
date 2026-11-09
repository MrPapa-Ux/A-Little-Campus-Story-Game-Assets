using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

// Editor previews may be saved for arranging artwork, but must never ship as gameplay.
public sealed class ValidasiBuildChapter1 : IProcessSceneWithReport
{
    public int callbackOrder => 1000;
    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (report == null || scene.name != "Chapter1_Kamar201") return;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var story in root.GetComponentsInChildren<CeritaKamar201Arthur>(true))
                if (!story.isActiveAndEnabled) throw new BuildFailedException("Tutorial Chapter 1 nonaktif: pulihkan scene dari pratinjau sebelum build.");
            foreach (var end in root.GetComponentsInChildren<PenutupChapter1Arthur>(true))
                if (!end.isActiveAndEnabled || end.ilustrasiPenutup.activeSelf || !end.chapel.enabled)
                    throw new BuildFailedException("Scene Chapter 1 masih dalam pratinjau ending. Build dibatalkan agar gameplay tidak terlewati.");
        }
    }
}
