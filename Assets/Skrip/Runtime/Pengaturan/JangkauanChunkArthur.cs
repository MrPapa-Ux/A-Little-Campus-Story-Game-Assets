using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Hanya gambar lingkungan. Collider, jalur, NPC, cahaya dan tekstur tidak diubah.
public sealed class JangkauanChunkArthur : MonoBehaviour
{
 public const string Kunci="Arthur.WorldChunkRange.v1";
 public static int? PaksaUji;
 public static int Pilihan=>PaksaUji??Mathf.Clamp(PlayerPrefs.GetInt(Kunci,1),0,3);
 public static float Jarak=>Pilihan==3?float.PositiveInfinity:new[]{32f,64f,96f}[Pilihan];
 public static string Nama=>new[]{"Dekat · 32 m","Sedang · 64 m","Jauh · 96 m","Utuh"}[Pilihan];
 static readonly List<Renderer> daftar=new List<Renderer>();
 public static int Terlihat {get {int n=0;foreach(var r in daftar)if(r!=null&&r.enabled&&!r.forceRenderingOff)n++;return n;}}
 public static int Total=>daftar.Count;
 static JangkauanChunkArthur instance;
 Camera kamera;ArthurJalurKampus arthur;float berikut;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){daftar.Clear();instance=null;PaksaUji=null;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Pasang(){SceneManager.sceneLoaded-=Muat;SceneManager.sceneLoaded+=Muat;}
 static void Muat(Scene scene,LoadSceneMode mode){
  if(instance==null){var go=new GameObject("Jangkauan chunk lingkungan");DontDestroyOnLoad(go);instance=go.AddComponent<JangkauanChunkArthur>();}
  if(!scene.name.StartsWith("Kawasan_"))return;
  foreach(var root in scene.GetRootGameObjects())foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))if(r.enabled&&r.gameObject.activeInHierarchy)daftar.Add(r);
  instance.berikut=0;
 }
 public static void Ubah(int pilihan){PaksaUji=null;PlayerPrefs.SetInt(Kunci,Mathf.Clamp(pilihan,0,3));PlayerPrefs.Save();if(instance!=null)instance.berikut=0;}
 void Update(){if(Time.unscaledTime<berikut)return;berikut=Time.unscaledTime+.15f;
  if(arthur==null)arthur=FindFirstObjectByType<ArthurJalurKampus>();
  kamera=arthur!=null?arthur.kamera:Camera.main;if(kamera==null)return;
  float batas=Jarak;var p=kamera.transform.position;
  for(int i=daftar.Count-1;i>=0;i--){var r=daftar[i];if(r==null){daftar.RemoveAt(i);continue;}bool sembunyi=!float.IsPositiveInfinity(batas)&&r.bounds.SqrDistance(p)>batas*batas;
   if(r.forceRenderingOff!=sembunyi)r.forceRenderingOff=sembunyi;
  }
 }
}
