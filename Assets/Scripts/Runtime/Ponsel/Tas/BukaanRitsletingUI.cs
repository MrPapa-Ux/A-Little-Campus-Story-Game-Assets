// PANDUAN PROYEK - Runtime / Ponsel/Tas
// Fungsi: Menggambar bukaan pita/gigi ritsleting sesuai kemajuan tarikan.
// Panduan: Assets/Scripts/Runtime/Ponsel/Tas/README.md

using UnityEngine;
using UnityEngine.UI;

// Bukaan dibatasi tinggi jalur; tidak mengembang ke gambar barang di bawahnya.
[RequireComponent(typeof(CanvasRenderer))]
public class BukaanRitsletingUI : MaskableGraphic
{
    [Range(0,1)]public float kemajuan;
    public float margin=22;
    public Color warnaPita=new Color32(40,33,24,255),warnaGigi=new Color32(223,191,133,255),warnaDalam=new Color32(61,39,26,255);
    public void AturKemajuan(float t){t=Mathf.Clamp01(t);if(Mathf.Abs(t-kemajuan)<.0001f)return;kemajuan=t;SetVerticesDirty();}
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;float pivot=Mathf.Lerp(r.xMin+margin,r.xMax-margin,kemajuan),gap=Mathf.Min(15,r.height*.5f-9);
        for(float x=r.xMin;x<r.xMax;x+=6)
        {
            float end=Mathf.Min(x+6,r.xMax),g0=Gap(x,pivot,gap),g1=Gap(end,pivot,gap);
            if(g0>0||g1>0){Quad(vh,new Vector2(x,-g0),new Vector2(x,g0),new Vector2(end,g1),new Vector2(end,-g1),warnaDalam);Line(vh,new Vector2(x,g0),new Vector2(end,g1),5,warnaPita);Line(vh,new Vector2(x,-g0),new Vector2(end,-g1),5,warnaPita);}
            else Quad(vh,new Vector2(x,-12),new Vector2(x,12),new Vector2(end,12),new Vector2(end,-12),warnaPita);
        }
        for(float x=r.xMin+9;x<r.xMax-5;x+=17)
        {
            float g=Gap(x,pivot,gap);
            if(g>1){Box(vh,new Vector2(x,g+3),new Vector2(6,8),warnaGigi);Box(vh,new Vector2(x,-g-3),new Vector2(6,8),warnaGigi);}
            else Box(vh,new Vector2(x,0),new Vector2(6,19),warnaGigi);
        }
    }
    float Gap(float x,float pivot,float max)=>kemajuan<=.001f||x>=pivot?0:max*Mathf.SmoothStep(0,1,Mathf.Clamp01((pivot-x)/80));
    static void Box(VertexHelper vh,Vector2 c,Vector2 s,Color col){var h=s/2;Quad(vh,c+new Vector2(-h.x,-h.y),c+new Vector2(-h.x,h.y),c+h,c+new Vector2(h.x,-h.y),col);}
    static void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color col){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width/2;Quad(vh,a-n,a+n,b+n,b-n,col);}
    static void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color col){int i=vh.currentVertCount;vh.AddVert(a,col,Vector2.zero);vh.AddVert(b,col,Vector2.zero);vh.AddVert(c,col,Vector2.zero);vh.AddVert(d,col,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
}
