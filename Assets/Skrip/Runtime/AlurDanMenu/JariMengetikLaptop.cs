using UnityEngine;

// Tangan khusus adegan laptop, tidak mengganti tangan Arthur saat eksplorasi.
public sealed class JariMengetikLaptop : MonoBehaviour
{
    public Transform[] jari;
    public float selisihFase;
    Quaternion[] rotasiAwal;
    void Awake(){rotasiAwal=new Quaternion[jari.Length];for(int i=0;i<jari.Length;i++)rotasiAwal[i]=jari[i].localRotation;}
    public void AturFrame(float t){
        if(rotasiAwal==null)Awake();
        bool mengetik=Mathf.FloorToInt(t*1.2f)%7!=6;
        for(int i=0;i<jari.Length;i++){
            float ketuk=mengetik?Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*17f+selisihFase+i*1.7f)),5):0;
            jari[i].localRotation=rotasiAwal[i]*Quaternion.Euler(ketuk*10f,0,0);
        }
    }
}
