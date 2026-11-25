using UnityEngine;using UnityEngine.UI;
[RequireComponent(typeof(CanvasRenderer))]
public sealed class EfekFearMekar : MaskableGraphic
{
 [Range(0,1)]public float target;
 public Camera kamera;
 [Range(0,.15f)]public float getaranPosisi=.055f;
 [Range(0,1)]public float getaranSudut=.42f;
 float kuat;AudioSource detak;AudioClip clip;
 Vector3 posisiDasar;Quaternion rotasiDasar;float fovDasar;bool kameraDisimpan;
 protected override void Awake(){base.Awake();raycastTarget=false;if(!Application.isPlaying)return;
  detak=GetComponent<AudioSource>()??gameObject.AddComponent<AudioSource>();detak.loop=true;detak.spatialBlend=0;detak.playOnAwake=false;detak.volume=0;
  const int rate=22050;var data=new float[rate*4];uint acak=12345;
  for(int i=0;i<data.Length;i++){float t=i/(float)rate;float fase=Mathf.Repeat(t,1f);float a=Mathf.Exp(-fase*33)*Mathf.Sin(2*Mathf.PI*51*t);float d=fase-.24f;float b=d>=0?Mathf.Exp(-d*43)*Mathf.Sin(2*Mathf.PI*43*t)*.7f:0;acak=1664525*acak+1013904223;float noise=((acak>>8)/(float)0xFFFFFF-.5f)*.015f;data[i]=(a+b)*.5f+Mathf.Sin(2*Mathf.PI*31*t)*.035f+noise;}
  clip=AudioClip.Create("Detak takut Dining",data.Length,1,rate,false);clip.SetData(data,0);detak.clip=clip;
 }
 void SimpanKamera(){if(kameraDisimpan||kamera==null)return;posisiDasar=kamera.transform.position;rotasiDasar=kamera.transform.rotation;fovDasar=kamera.fieldOfView;kameraDisimpan=true;}
 void PulihkanKamera(){if(kameraDisimpan&&kamera!=null){kamera.transform.SetPositionAndRotation(posisiDasar,rotasiDasar);kamera.fieldOfView=fovDasar;}kameraDisimpan=false;}
 protected override void OnDisable(){if(detak!=null)detak.Stop();kuat=0;PulihkanKamera();base.OnDisable();}
 protected override void OnDestroy(){if(clip!=null){if(Application.isPlaying)Destroy(clip);else DestroyImmediate(clip);}base.OnDestroy();}
 public void Atur(float nilai)=>target=Mathf.Clamp01(nilai);
 public void MulaiOvt(float nilai){kuat=target=Mathf.Clamp01(nilai);SetVerticesDirty();}
 void Update(){if(!Application.isPlaying)return;kuat=Mathf.MoveTowards(kuat,target,Time.unscaledDeltaTime*.55f);
  if(detak!=null){detak.volume=kuat*.34f;detak.pitch=Mathf.Lerp(.78f,1.14f,kuat);if(kuat>.01f&&!detak.isPlaying)detak.Play();else if(kuat<=.01f&&detak.isPlaying)detak.Stop();}SetVerticesDirty();}
 void LateUpdate(){if(!Application.isPlaying||kamera==null)return;if(kuat<.001f){PulihkanKamera();return;}SimpanKamera();float t=Time.unscaledTime,amp=kuat*kuat;
  float x=(Mathf.PerlinNoise(t*14,2.3f)-.5f)*2,y=(Mathf.PerlinNoise(8.7f,t*17)-.5f)*2,z=(Mathf.PerlinNoise(t*11,13.4f)-.5f)*2;
  kamera.transform.position=posisiDasar+rotasiDasar*new Vector3(x,y,0)*getaranPosisi*amp;
  kamera.transform.rotation=rotasiDasar*Quaternion.Euler(y*getaranSudut*amp,x*getaranSudut*amp,z*getaranSudut*.38f*amp);
  kamera.fieldOfView=fovDasar-kuat*1.4f;
 }
 float Inner(float angle)=>.45f-kuat*.19f+Mathf.Sin(angle*3+Time.unscaledTime*.65f)*kuat*.015f;
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(kuat<.001f)return;var r=rectTransform.rect;float denyut=1+Mathf.Sin(Time.unscaledTime*1.7f)*.14f;float alpha=Mathf.Clamp01(kuat*denyut);
  Quad(vh,new Vector2(r.xMin,r.yMin),new Vector2(r.xMin,r.yMax),new Vector2(r.xMax,r.yMax),new Vector2(r.xMax,r.yMin),new Color(.008f,.012f,.022f,alpha*.50f));
  // 80 segmen memasukkan keempat sudut persegi panjang tepat pada kelipatan 45 derajat.
  // Tepi diperluas dua piksel; tidak ada celah segitiga atau sambungan yang terbuka.
  const int n=80;var half=new Vector2(r.width*.5f+2,r.height*.5f+2);
  for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n,b=(i+1)*Mathf.PI*2/n;var u=new Vector2(Mathf.Cos(a),Mathf.Sin(a));var v=new Vector2(Mathf.Cos(b),Mathf.Sin(b));
   var p=r.center+Vector2.Scale(u,new Vector2(r.width*Inner(a),r.height*Inner(a)));var q=r.center+Vector2.Scale(v,new Vector2(r.width*Inner(b),r.height*Inner(b)));
   var outP=r.center+Vector2.Scale(u,half)/Mathf.Max(Mathf.Abs(u.x),Mathf.Abs(u.y));var outQ=r.center+Vector2.Scale(v,half)/Mathf.Max(Mathf.Abs(v.x),Mathf.Abs(v.y));
   int k=vh.currentVertCount;var clear=new Color(.005f,.008f,.015f,0);var dark=new Color(.005f,.008f,.015f,alpha);vh.AddVert(p,clear,Vector2.zero);vh.AddVert(outP,dark,Vector2.zero);vh.AddVert(outQ,dark,Vector2.zero);vh.AddVert(q,clear,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
  }}
 static void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color col){int k=vh.currentVertCount;vh.AddVert(a,col,Vector2.zero);vh.AddVert(b,col,Vector2.zero);vh.AddVert(c,col,Vector2.zero);vh.AddVert(d,col,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);}
}
