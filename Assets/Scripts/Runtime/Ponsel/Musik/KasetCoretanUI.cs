// PANDUAN PROYEK - Runtime / Ponsel/Musik
// Fungsi: Menggambar kaset dan menggerakkan coretannya saat musik diputar.
// Panduan: Assets/Scripts/Runtime/Ponsel/Musik/README.md

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class KasetCoretanUI : MaskableGraphic
{
    public bool Bergerak {get;private set;}
    public int JumlahLangkah {get;private set;}
    float berikut,phase;
    public void AturGerak(bool value){Bergerak=value;}
    void Update(){if(!Bergerak || Time.unscaledTime<berikut)return;berikut=Time.unscaledTime+1f/7f;phase+=1.73f;JumlahLangkah++;SetVerticesDirty();}
    Vector2 Wiggle(Vector2 p)=>p+new Vector2(Mathf.Sin(p.x*.43f+p.y*.19f+phase),Mathf.Cos(p.y*.37f-p.x*.17f+phase))*2.6f;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;
        Ring(vh,new Vector2(-r.width*.26f,0),r.height*.22f);
        Ring(vh,new Vector2(r.width*.26f,0),r.height*.22f);
        for(int i=0;i<13;i++){float x=-r.width*.35f+i*r.width*.7f/12;Line(vh,new Vector2(x,-r.height*.34f),new Vector2(x+7,-r.height*.40f),2);}
        Line(vh,new Vector2(-r.width*.32f,0),new Vector2(r.width*.32f,0),3);
    }
    void Ring(VertexHelper vh,Vector2 p,float radius)
    {
        for(int i=0;i<36;i++)
        {
            float a=i*Mathf.PI*2/36,b=(i+1)*Mathf.PI*2/36;
            Line(vh,p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius*(1+.025f*Mathf.Sin(a*5)),p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius*(1+.025f*Mathf.Sin(b*5)),3);
        }
        for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Line(vh,p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius*.25f,p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius*.7f,4);}
    }
    void Line(VertexHelper vh,Vector2 a,Vector2 b,float width)
    {
        a=Wiggle(a);b=Wiggle(b);
        Vector2 d=b-a,n=new Vector2(-d.y,d.x).normalized*width*.5f;int i=vh.currentVertCount;
        vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);
        vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i+1,i+3,i+2);
    }
}
