// PANDUAN PROYEK - Runtime / Ponsel/Tas
// Fungsi: Menggambar siluet tas untuk halaman inventori.
// Panduan: Assets/Scripts/Runtime/Ponsel/Tas/README.md

using UnityEngine;
using UnityEngine.UI;

// Siluet kain dan gagang digambar sekali saat layout berubah, tanpa tekstur tambahan.
[RequireComponent(typeof(CanvasRenderer))]
public class BentukTasUI : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;
        Vector2[] p={new Vector2(-.29f,.44f),new Vector2(.27f,.45f),new Vector2(.40f,.36f),new Vector2(.47f,.19f),new Vector2(.49f,-.35f),new Vector2(.40f,-.48f),new Vector2(-.38f,-.49f),new Vector2(-.49f,-.37f),new Vector2(-.47f,.17f),new Vector2(-.41f,.35f)};
        for(int i=0;i<p.Length;i++)p[i]=Vector2.Scale(p[i],r.size)+r.center;
        vh.AddVert(r.center,color,Vector2.zero);
        foreach(var v in p)vh.AddVert(v,color,Vector2.zero);
        for(int i=0;i<p.Length;i++){vh.AddTriangle(0,i+1,(i+1)%p.Length+1);Line(vh,p[i],p[(i+1)%p.Length],4,new Color32(47,31,22,255));}
        Vector2 a=Vector2.Scale(new Vector2(-.14f,.44f),r.size),b=Vector2.Scale(new Vector2(-.12f,.55f),r.size),c=Vector2.Scale(new Vector2(.11f,.56f),r.size),d=Vector2.Scale(new Vector2(.15f,.44f),r.size);
        Line(vh,a,b,21,new Color32(67,43,27,255));Line(vh,b,c,21,new Color32(67,43,27,255));Line(vh,c,d,21,new Color32(67,43,27,255));
    }
    static void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color color)
    {
        var n=new Vector2(-(b-a).y,(b-a).x).normalized*width/2;int i=vh.currentVertCount;
        vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i+1,i+3,i+2);
    }
}
