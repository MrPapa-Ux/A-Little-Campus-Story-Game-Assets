// PANDUAN PROYEK - Runtime / Ponsel/Peta
// Fungsi: Menggambar penanda lokasi peta dengan garis coretan.
// Panduan: Assets/Scripts/Runtime/Ponsel/Peta/README.md

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class LingkaranPeta : MaskableGraphic
{
    [Range(1f,8f)] public float tebal = 4f;
    public Color warnaGarisLuar = new Color32(25,20,35,255);
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear(); Vector2 center=rectTransform.rect.center;
        float radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.34f;
        Loop(vh,center,radius,tebal+3f,warnaGarisLuar); Tail(vh,center,radius,tebal+3f,warnaGarisLuar);
        Loop(vh,center,radius,tebal,color); Tail(vh,center,radius,tebal,color);
    }
    static Vector2 Point(Vector2 center,float radius,float a)
    {
        float r=radius*(1f+.07f*Mathf.Sin(a*3f)+.04f*Mathf.Cos(a*5f)+.03f*Mathf.Sin(a*7f));
        return center+new Vector2(Mathf.Cos(a)*r*1.03f+Mathf.Sin(a*2f)*radius*.08f,Mathf.Sin(a)*r*.91f+Mathf.Cos(a*3f)*radius*.04f);
    }
    static void Loop(VertexHelper vh,Vector2 center,float radius,float width,Color ink)
    {
        const int n=40;
        for(int i=0;i<n;i++)Stroke(vh,Point(center,radius,i*Mathf.PI*2f/n),Point(center,radius,(i+1)*Mathf.PI*2f/n),width,ink);
    }
    static void Tail(VertexHelper vh,Vector2 center,float radius,float width,Color ink)
    {
        Vector2 a=center+new Vector2(.46f,-.46f)*radius,b=center+new Vector2(.76f,-.71f)*radius,c=center+new Vector2(1.16f,-1.08f)*radius;
        Stroke(vh,a,b,width,ink);Stroke(vh,b,c,width,ink);
    }
    static void Stroke(VertexHelper vh,Vector2 a,Vector2 b,float width,Color ink)
    {
        Vector2 d=(b-a).normalized,normal=new Vector2(-d.y,d.x)*width*.5f;
        a-=d*.65f;b+=d*.65f;int v=vh.currentVertCount;
        vh.AddVert(a+normal,ink,Vector2.zero);vh.AddVert(a-normal,ink,Vector2.zero);vh.AddVert(b+normal,ink,Vector2.zero);vh.AddVert(b-normal,ink,Vector2.zero);
        vh.AddTriangle(v,v+1,v+2);vh.AddTriangle(v+1,v+3,v+2);
    }
}
