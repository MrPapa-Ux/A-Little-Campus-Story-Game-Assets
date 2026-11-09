using UnityEngine;using UnityEngine.UI;using UnityEngine.EventSystems;using TMPro;

// Memakai bentuk lingkaran dan cincin hitung mundur Tap-Tap yang sudah ada.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class BulatanKataMekar : MaskableGraphic,IPointerDownHandler
{
 public TapTapMekarArthur game;
 public TMP_Text[] kata;
 public float radius=108,durasiBulatan=3.5f;
 public Vector2[] tempat={new Vector2(.11f,.46f),new Vector2(.49f,.80f),new Vector2(.79f,.32f),new Vector2(.27f,.64f),new Vector2(.71f,.63f),new Vector2(.27f,.12f),new Vector2(.44f,.43f),new Vector2(.82f,.80f)};
 readonly bool[] aktif=new bool[20],selesai=new bool[20];readonly float[] umur=new float[20];readonly int[] lokasi=new int[20];int berikut,nomorTempat;float jeda;bool berjalan;
 public int JumlahTerlihat{get;private set;}
 public Vector2 Posisi(int i){var r=rectTransform.rect;var p=tempat[lokasi[i]%tempat.Length];return new Vector2(Mathf.Lerp(r.xMin,r.xMax,p.x),Mathf.Lerp(r.yMin,r.yMax,p.y));}
 public bool Terlihat(int i)=>i>=0&&i<20&&aktif[i]&&!selesai[i];
 public void Mulai(){berjalan=true;berikut=nomorTempat=0;jeda=0;JumlahTerlihat=0;for(int i=0;i<20;i++){aktif[i]=selesai[i]=false;umur[i]=0;kata[i].gameObject.SetActive(false);kata[i].text=game.kataBuruk[i];}for(int i=0;i<3;i++)Munculkan();SetVerticesDirty();}
 void Munculkan(){
  for(int n=0;n<20;n++){int i=berikut;berikut=(berikut+1)%20;if(selesai[i]||aktif[i])continue;
   int spot=nomorTempat++%tempat.Length;for(int tries=0;tries<tempat.Length;tries++){bool used=false;for(int j=0;j<20;j++)if(aktif[j]&&lokasi[j]==spot)used=true;if(!used)break;spot=nomorTempat++%tempat.Length;}
   aktif[i]=true;lokasi[i]=spot;umur[i]=0;JumlahTerlihat++;
   var label=kata[i];var rect=label.rectTransform;
   rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
   rect.sizeDelta=new Vector2(radius*1.50f,radius*1.20f);
   rect.anchoredPosition=Posisi(i);label.margin=Vector4.zero;
   label.alignment=TextAlignmentOptions.Center;label.enableAutoSizing=true;
   label.fontSizeMin=22;label.fontSizeMax=36;label.overflowMode=TextOverflowModes.Truncate;
   label.raycastTarget=false;label.gameObject.SetActive(true);label.alpha=1;label.transform.localScale=Vector3.one;return;
  }
 }
 public void Perbarui(float dt){if(!berjalan)return;
  for(int i=0;i<20;i++)if(aktif[i]){umur[i]+=dt;if(umur[i]>=durasiBulatan){aktif[i]=false;kata[i].gameObject.SetActive(false);JumlahTerlihat--;}}
  jeda-=dt;if(JumlahTerlihat<3&&jeda<=0){Munculkan();jeda=.25f;}SetVerticesDirty();
 }
 public void Sisihkan(int i){if(!Terlihat(i))return;aktif[i]=false;selesai[i]=true;kata[i].gameObject.SetActive(false);JumlahTerlihat--;jeda=.12f;SetVerticesDirty();}
 public void Hentikan(){berjalan=false;}
 public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left||game.Status!=TapTapMekarArthur.Fase.Bermain)return;if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var p))return;
  for(int i=0;i<20;i++)if(Terlihat(i)&&(Posisi(i)-p).sqrMagnitude<=radius*radius){game.KetukKata(i);return;}
 }
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();for(int i=0;i<20;i++){if(!Terlihat(i))continue;var p=Posisi(i);float muncul=Mathf.Lerp(.88f,1,Mathf.Clamp01(umur[i]/.12f));
   PapanRhythmArthur.Circle(vh,p,radius*muncul,new Color32(244,235,213,250));PapanRhythmArthur.Arc(vh,p,radius*muncul,4,new Color32(20,25,28,255),1);
   PapanRhythmArthur.Arc(vh,p,(radius+10)*muncul,5,new Color32(239,154,57,255),Mathf.Clamp01(1-umur[i]/durasiBulatan));
  }}
}
