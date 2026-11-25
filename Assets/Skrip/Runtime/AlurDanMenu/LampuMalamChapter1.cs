using UnityEngine;

// Lampu hanya untuk malam cerita; setiap titik dapat digeser dari Scene.
public sealed class LampuMalamChapter1 : MonoBehaviour
{
 public PerjalananChapelChapter1 cerita;
 public GameObject perlengkapanJasmine;
 public Light[] lampu;
 [Min(10)]public float jarakAktif=36;
 void LateUpdate(){bool aktif=cerita!=null&&cerita.MalamAktif&&KawasanKampus.Instance!=null&&KawasanKampus.Instance.KawasanAktif=="JasmineGK2";
  if(perlengkapanJasmine.activeSelf!=aktif)perlengkapanJasmine.SetActive(aktif);
  var pemain=cerita!=null?cerita.kamar.arthur:null;
  foreach(var l in lampu)if(l!=null){bool dekat=aktif&&pemain!=null&&(l.transform.position-pemain.transform.position).sqrMagnitude<jarakAktif*jarakAktif;if(l.enabled!=dekat)l.enabled=dekat;}
 }
 void OnDisable(){if(perlengkapanJasmine!=null)perlengkapanJasmine.SetActive(false);}
}
