using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Satu penghargaan untuk versi demo. Tidak menyimpan progres cerita lintas sesi.
public sealed class PenghargaanDemoArthur : MonoBehaviour
{
 public const string KunciSelesai="GameFinished";
 public const string AlamatCerita="https://www.wattpad.com/story/415653090-a-little-campus-story";
 public Button tombolCerita;
 public TMP_Text status;
 bool pratinjau;
 public static bool Terbuka=>PlayerPrefs.GetInt(KunciSelesai,0)==1;
 public static void TandaiSelesai(){PlayerPrefs.SetInt(KunciSelesai,1);PlayerPrefs.Save();}
 void OnEnable(){transform.SetAsLastSibling();var canvas=GetComponent<Canvas>();if(canvas!=null){canvas.overrideSorting=true;canvas.sortingOrder=200;}Segarkan();}
 void Update(){if(Input.GetKeyDown(KeyCode.Escape))Tutup();}
 public void Segarkan(){bool buka=Terbuka||pratinjau;if(tombolCerita!=null)tombolCerita.interactable=buka;if(status!=null)status.text=buka?"DIPEROLEH — CHAPTER 1 SELESAI":"SELESAIKAN DEMO CHAPTER 1";}
 public void BukaCerita(){if(!Terbuka&&!pratinjau)return;Application.OpenURL(AlamatCerita);}
 public void Tutup(){pratinjau=false;gameObject.SetActive(false);}
#if UNITY_EDITOR
 public void PratinjauEditor(){pratinjau=true;gameObject.SetActive(true);Segarkan();}
#endif
}
