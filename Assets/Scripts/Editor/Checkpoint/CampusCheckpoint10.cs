// PANDUAN PROYEK - Editor / Checkpoint
// Fungsi: Menerapkan susunan/revisi yang dibuat pada checkpoint 10.
// Panduan: Assets/Scripts/Editor/Checkpoint/README.md

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class CampusCheckpoint10
{
 public static void Apply(){
  if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Edit Mode diperlukan");
  var hp=Object.FindFirstObjectByType<SistemHP>(FindObjectsInactive.Include);
  var morph=hp.GetComponent<TransformasiLayarHP>();if(morph==null)morph=hp.gameObject.AddComponent<TransformasiLayarHP>();
  morph.ponsel=hp.hpUtama.GetComponent<RectTransform>();morph.layar=(RectTransform)hp.hpUtama.transform.Find("LayarTerang");
  morph.badan=hp.badanHP;
  var group=hp.hpUtama.GetComponent<CanvasGroup>();if(group==null)group=hp.hpUtama.AddComponent<CanvasGroup>();
  group.alpha=1;group.interactable=true;group.blocksRaycasts=true;morph.kelompok=group;morph.durasi=.25f;morph.jedaTV=0f;hp.transformasiLayar=morph;
  hp.transisiAplikasi.durasi=.32f;hp.transisiAplikasi.durasiMati=.16f;hp.transisiAplikasi.durasiFadeKeluar=.18f;EditorUtility.SetDirty(hp.transisiAplikasi);
  var bg=morph.layar.GetComponent<UnityEngine.UI.Image>();bg.color=Color.Lerp(hp.warnaDefaultHP,new Color(.81f,.78f,.69f,1),.85f);bg.raycastTarget=false;
  var pose=hp.poseKarakter;pose.balikTelapak=true;pose.sudutTanganKanan=-5f;
  pose.posisiTanganKanan=new Vector3(.96f,-.83f,-.18f);pose.posisiHP=new Vector3(.94f,-.38f,-.22f);
  pose.hpKecil.transform.localScale=Vector3.one*(1.1f/pose.hpKecil.sprite.bounds.size.y);
  pose.hpKecil.transform.localPosition=pose.posisiHP;
  var screen=pose.hpKecil.transform.Find("LatarLayar");if(screen==null){screen=new GameObject("LatarLayar",typeof(SpriteRenderer)).transform;screen.SetParent(pose.hpKecil.transform,false);}
  var sr=screen.GetComponent<SpriteRenderer>();sr.sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
  screen.localPosition=new Vector3(.24f,-.24f,.015f);screen.localScale=new Vector3(1.65f/sr.sprite.bounds.size.x,2.55f/sr.sprite.bounds.size.y,1);
  sr.sortingLayerID=pose.hpKecil.sortingLayerID;sr.sortingOrder=pose.hpKecil.sortingOrder-1;sr.color=bg.color;pose.layarKecil=sr;
  EditorUtility.SetDirty(pose);EditorUtility.SetDirty(hp);EditorUtility.SetDirty(morph);EditorUtility.SetDirty(group);
  EditorSceneManager.MarkSceneDirty(hp.gameObject.scene);EditorSceneManager.SaveScene(hp.gameObject.scene);AssetDatabase.SaveAssets();
 }
}
