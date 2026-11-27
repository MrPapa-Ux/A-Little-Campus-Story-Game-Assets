using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class AuditChunkDanQuiz {
 public static void Run(){var s=EditorSceneManager.OpenScene("Assets/Scenes/Penataan Kampus - Jalur NPC dan Bubble.unity");var all=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();Directory.CreateDirectory("../work/chunk-20261126");var h=all.Select(t=>t.GetComponent<SistemHP>()).First(x=>x!=null);var p=h.panelPengaturan.GetComponent<PengaturanManager>();File.WriteAllText("../work/chunk-20261126/audit.txt",string.Join("\n",p.GetComponentsInChildren<RectTransform>(true).Select(t=>t.name+" parent="+t.parent?.name+" pos="+t.anchoredPosition+" size="+t.sizeDelta+" anchor="+t.anchorMin+"/"+t.anchorMax))+"\nROUTES\n"+string.Join("\n",all.Where(t=>t.name.Contains("Jasmine")||t.name.StartsWith("GH_")).Where(t=>t.GetComponent<TujuanJalurKampus>()!=null||t.GetComponent<JalurKampus>()!=null).Select(t=>t.name+" "+t.position+" route="+t.GetComponent<TujuanJalurKampus>()?.jalur?.name)));EditorApplication.Exit(0);}
}
