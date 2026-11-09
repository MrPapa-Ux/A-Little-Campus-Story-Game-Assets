using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class CampusCheckpoint22
{
    public static void Apply()
    {
        if(EditorApplication.isPlaying||EditorApplication.isPlayingOrWillChangePlaymode)throw new System.InvalidOperationException("Tunggu Unity selesai keluar dari Play Mode.");
        var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/Scenes/CharacterLab.unity");
        bool opened=!scene.isLoaded;if(opened)scene=EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity",OpenSceneMode.Additive);
        var d=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<DialogManager>(true)).First();
        foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            var area=canvas.transform.Find("AreaTampilan16x9") as RectTransform;if(area==null)continue;
            area.anchorMin=Vector2.zero;area.anchorMax=Vector2.one;area.offsetMin=area.offsetMax=Vector2.zero;
        }
        var p=d.arthurScript;var visual=p.wadahVisual;
        var dancer=p.GetComponent<NPCJogetGoofy>();if(dancer==null)dancer=p.gameObject.AddComponent<NPCJogetGoofy>();
        dancer.badan=visual.Find("Badan");dancer.kepala=visual.Find("KepalaArthur");dancer.rambut=visual.Find("Rambut 1");dancer.telinga=visual.Find("Telinga");
        dancer.tanganKiri=visual.Find("TanganKiri");dancer.tanganKanan=visual.Find("TanganKanan");dancer.kakiKiri=visual.Find("KakiKiri");dancer.kakiKanan=visual.Find("KakiKanan");
        var npc=GameObject.Find("NPC_Bimo_Joget").GetComponent<NPCJogetGoofy>();dancer.kecepatan=npc.kecepatan;dancer.kekuatan=npc.kekuatan;dancer.enabled=false;
        var tap=p.GetComponent<KetukKepalaArthur>();if(tap==null)tap=p.gameObject.AddComponent<KetukKepalaArthur>();tap.kepala=dancer.kepala.GetComponent<SpriteRenderer>();
        EditorSceneManager.MarkSceneDirty(d.gameObject.scene);EditorSceneManager.SaveScene(d.gameObject.scene);AssetDatabase.SaveAssets();if(opened)EditorSceneManager.CloseScene(scene,true);
    }
}
