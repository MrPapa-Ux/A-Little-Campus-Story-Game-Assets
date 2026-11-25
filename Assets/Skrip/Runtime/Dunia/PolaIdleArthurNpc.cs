using UnityEngine;

// Kurva posisi dari animasi Arthur, digunakan sebagai selisih agar bentuk/pose NPC tetap milik pemilik game.
public sealed class PolaIdleArthurNpc : ScriptableObject
{
    public float durasi=9;
    public AnimationCurve[] x=new AnimationCurve[6];
    public AnimationCurve[] y=new AnimationCurve[6];
    public AnimationCurve[] putaran=new AnimationCurve[6];
    public float Sudut(int bagian,float waktu){
        if(putaran==null||bagian>=putaran.Length||putaran[bagian]==null)return 0;
        return Mathf.DeltaAngle(putaran[bagian].Evaluate(0),putaran[bagian].Evaluate(Mathf.Repeat(waktu,Mathf.Max(.1f,durasi))));
    }
    public Vector3 Gerak(int bagian,float waktu){
        if(bagian>=x.Length||bagian>=y.Length)return Vector3.zero;
        float t=Mathf.Repeat(waktu,Mathf.Max(.1f,durasi));
        return new Vector3(x[bagian]!=null?x[bagian].Evaluate(t)-x[bagian].Evaluate(0):0,y[bagian]!=null?y[bagian].Evaluate(t)-y[bagian].Evaluate(0):0,0);
    }
}
