using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Cahaya sederhana per vertex untuk bangunan Android; tekstur dan lubang jendela tetap dipakai.
// Material sumber tidak disunting. Salinan dibuang bersama kawasan yang ditinggalkan.
public sealed class BahanDuniaAndroidArthur : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    readonly Dictionary<Material, Material> salinan = new Dictionary<Material, Material>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Pasang() { SceneManager.sceneLoaded -= SaatMuat; SceneManager.sceneLoaded += SaatMuat; }

    static void SaatMuat(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.StartsWith("Kawasan_", System.StringComparison.Ordinal)) return;
        var shader = Resources.Load<Shader>("ArthurAndroid/DuniaRingan");
        if (shader == null || !shader.isSupported) return;
        var go = new GameObject("Bahan dunia Android ringan");
        SceneManager.MoveGameObjectToScene(go, scene);
        var pemilik = go.AddComponent<BahanDuniaAndroidArthur>();
        foreach (var root in scene.GetRootGameObjects())
        foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            var materials = renderer.sharedMaterials;
            bool berubah = false;
            for (int i = 0; i < materials.Length; i++)
            {
                var asli = materials[i];
                if (asli == null || asli.shader.name != "Universal Render Pipeline/Lit"
                    || (asli.HasProperty("_Surface") && asli.GetFloat("_Surface") > .5f)) continue;
                if (!pemilik.salinan.TryGetValue(asli, out var ringan))
                {
                    ringan = new Material(shader) { name = asli.name + " (Android)", renderQueue = asli.renderQueue };
                    ringan.SetTexture("_BaseMap", asli.GetTexture("_BaseMap"));
                    ringan.SetTextureScale("_BaseMap", asli.GetTextureScale("_BaseMap"));
                    ringan.SetTextureOffset("_BaseMap", asli.GetTextureOffset("_BaseMap"));
                    ringan.SetColor("_BaseColor", asli.GetColor("_BaseColor"));
                    if (asli.HasProperty("_Cull")) ringan.SetFloat("_Cull", asli.GetFloat("_Cull"));
                    if (asli.HasProperty("_Cutoff")) ringan.SetFloat("_Cutoff", asli.GetFloat("_Cutoff"));
                    if (asli.IsKeywordEnabled("_EMISSION")) ringan.SetColor("_EmissionColor", asli.GetColor("_EmissionColor"));
                    if (asli.IsKeywordEnabled("_ALPHATEST_ON") || (asli.HasProperty("_AlphaClip") && asli.GetFloat("_AlphaClip") > .5f))
                        ringan.EnableKeyword("_ALPHATEST_ON");
                    pemilik.salinan.Add(asli, ringan);
                }
                materials[i] = ringan; berubah = true;
            }
            if (berubah) renderer.sharedMaterials = materials;
        }
        Debug.Log("ARTHUR_GRAPHICS bahanRingan=" + pemilik.salinan.Count + " kawasan=" + scene.name
            + " api=" + SystemInfo.graphicsDeviceType);
    }

    void OnDestroy()
    {
        foreach (var material in salinan.Values) if (material != null) Destroy(material);
        salinan.Clear();
    }
#endif
}
