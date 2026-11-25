using System;
using System.IO;
using System.Reflection;
using UnityEditor;

// Pemeriksaan lokal tanpa kursor; tidak mengubah scene atau menjalankan game.
[InitializeOnLoad]
public static class SambunganExplorerSementara
{
    static SambunganExplorerSementara() { EditorApplication.delayCall += Periksa; }
    static void Periksa()
    {
        if (AssetDatabase.IsAssetImportWorkerProcess()) return;
        const string laporan = "work/explorer/sambungan.txt";
        if (File.Exists(laporan)) return;
        Directory.CreateDirectory("work/explorer");
        try
        {
            Type tipe = Type.GetType("Unity.Pipeline.Editor.PipelineServerStartup, Unity.Pipeline.Editor");
            MethodInfo metode = tipe?.GetMethod("RestartServer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (metode == null) throw new Exception("Fungsi sambungan Unity tidak ditemukan.");
            metode.Invoke(null, null);
            File.WriteAllText(laporan, "Sambungan lokal dimulai ulang. Scene tidak diubah.");
        }
        catch (Exception e) { File.WriteAllText(laporan, e.ToString()); }
    }
}
