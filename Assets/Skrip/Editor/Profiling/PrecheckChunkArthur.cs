using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class PrecheckChunkArthur {
 public static void Run(){var hasil=new List<string>();bool had=PlayerPrefs.HasKey(JangkauanChunkArthur.Kunci);int saved=PlayerPrefs.GetInt(JangkauanChunkArthur.Kunci,1);int code=1;
  void Cek(bool ok,string text){if(!ok)throw new Exception(text);hasil.Add("LULUS: "+text);}
  try{foreach(var sceneName in new[]{"Penataan Kampus - Jalur NPC dan Bubble","Explorer"}){
   var s=EditorSceneManager.OpenScene("Assets/Scenes/"+sceneName+".unity");var objects=s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
   var q=objects.Select(t=>t.GetComponent<MrQuizoChapter1>()).Single(x=>x!=null&&x.namaPenjaga=="Mr. Quizzo");
   Cek(q.posisiJaga.Length==1,sceneName+": hanya satu posisi Quizzo");
   Cek(q.posisiJaga.Any(t=>t.GetComponent<TujuanJalurKampus>().jalur==null),sceneName+": posisi di depan Jasmine");
   Cek(q.batasJalan.Length==0&&!q.posisiJaga.Any(t=>t.GetComponent<TujuanJalurKampus>().jalur!=null),sceneName+": tanpa posisi atau penghalang Quizzo di GK2");
   Cek(q.pintuKeluar.Length>0&&q.bolehTanpaKuisSebelumnya,sceneName+": gerbang masuk mengarah ke kuis tanpa ketergantungan GA");
   var hp=objects.Select(t=>t.GetComponent<SistemHP>()).First(x=>x!=null);var p=hp.panelPengaturan.GetComponent<PengaturanManager>();p.SinkronkanPengaturan();
   Cek(p.sliderChunk!=null&&p.sliderChunk.transform.parent==p.sliderVolume.transform.parent,sceneName+": chunk satu panel dengan volume");
   for(int i=0;i<4;i++){p.sliderChunk.value=i;Cek(JangkauanChunkArthur.Pilihan==i,sceneName+": slider pilihan "+i);}
   p.UbahChunk(0);Cek(JangkauanChunkArthur.Jarak==32,sceneName+": jarak dekat 32m");p.UbahChunk(3);Cek(float.IsPositiveInfinity(JangkauanChunkArthur.Jarak),sceneName+": mode utuh memulihkan seluruh jangkauan");
   Cek(p.sliderCahaya!=null&&p.sliderVolume!=null,sceneName+": kontrol lama tetap terhubung");
  }code=0;}catch(Exception e){hasil.Add("GAGAL: "+e);Debug.LogException(e);}finally{if(had)PlayerPrefs.SetInt(JangkauanChunkArthur.Kunci,saved);else PlayerPrefs.DeleteKey(JangkauanChunkArthur.Kunci);PlayerPrefs.Save();File.WriteAllLines("../work/chunk-20261126/precheck-editor.txt",hasil);if(code!=0||Environment.GetEnvironmentVariable("ALCS_CHAIN")!="1")EditorApplication.Exit(code);}
 }
}
