using UnityEngine;

// Dua pemeran dekat kamera; kerumunan lain tetap mesh gabungan.
public sealed class FigurChapelCerita : MonoBehaviour
{
 public Transform tampakDepan,tampakBelakang,kepala,tanganKiri,tanganKanan;
 public EkspresiWajah2D wajah;
 public bool sedangBerkhotbah;
 public Camera kamera;
 Vector3 kepalaAwal,kiriAwal,kananAwal;float ucapanBerikut;
 void Awake(){if(kepala!=null)kepalaAwal=kepala.localPosition;if(tanganKiri!=null)kiriAwal=tanganKiri.localPosition;if(tanganKanan!=null)kananAwal=tanganKanan.localPosition;}
 void LateUpdate(){
  if(kamera==null)return;float yaw=kamera.transform.eulerAngles.y;
  bool belakang=Mathf.Cos(yaw*Mathf.Deg2Rad)>0;
  if(tampakBelakang!=null){tampakBelakang.gameObject.SetActive(belakang);tampakDepan.gameObject.SetActive(!belakang);}
  tampakDepan.rotation=Quaternion.Euler(0,yaw,0);
  float napas=Mathf.Sin(Time.time*1.35f);
  if(kepala!=null)kepala.localPosition=kepalaAwal+Vector3.up*napas*.008f;
  if(tanganKiri!=null)tanganKiri.localPosition=kiriAwal+Vector3.up*napas*.004f;
  if(tanganKanan!=null)tanganKanan.localPosition=kananAwal+Vector3.up*Mathf.Sin(Time.time*1.8f)*(sedangBerkhotbah?.025f:.004f);
  if(sedangBerkhotbah&&wajah!=null&&DialogManager.Instance!=null&&DialogManager.Instance.PercakapanAktif&&Time.time>=ucapanBerikut){wajah.UcapkanHuruf();ucapanBerikut=Time.time+.13f;}
 }
}
