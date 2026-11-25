using UnityEngine;

// Pad lembut orisinal, bukan rekaman suara orang atau lagu ibadah.
[RequireComponent(typeof(AudioSource))]
public sealed class SuasanaIbadahChapel : MonoBehaviour
{
 AudioClip clip;
 void Awake(){const int rate=22050,detik=8;var samples=new float[rate*detik];float[] nada={130.75f,164.75f,196f};
  for(int i=0;i<samples.Length;i++){float t=i/(float)rate;float napas=.76f+.12f*Mathf.Sin(t*Mathf.PI*.25f);float a=0;
   foreach(float f in nada)a+=Mathf.Sin(2*Mathf.PI*f*t)*.055f+Mathf.Sin(4*Mathf.PI*f*t)*.012f;
   samples[i]=a*napas;
  }
  clip=AudioClip.Create("Pad tenang Chapel — orisinal",samples.Length,1,rate,false);clip.SetData(samples,0);var source=GetComponent<AudioSource>();source.clip=clip;source.loop=true;source.playOnAwake=false;source.spatialBlend=0;source.volume=.35f;
 }
 void OnDestroy(){if(clip!=null)Destroy(clip);}
}
