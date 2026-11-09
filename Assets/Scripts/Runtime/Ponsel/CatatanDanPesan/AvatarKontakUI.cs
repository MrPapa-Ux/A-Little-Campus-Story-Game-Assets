// PANDUAN PROYEK - Runtime / Ponsel/CatatanDanPesan
// Fungsi: Menggambar avatar anonim berbentuk kepala dan tubuh pada daftar pesan.
// Panduan: Assets/Scripts/Runtime/Ponsel/CatatanDanPesan/README.md

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class AvatarKontakUI : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();float s=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height);Vector2 center=rectTransform.rect.center;
        for(int i=0;i<32;i++){float a=i*Mathf.PI/16,b=(i+1)*Mathf.PI/16;Line(vh,center+new Vector2(Mathf.Cos(a)*.15f,Mathf.Sin(a)*.15f+.19f)*s,center+new Vector2(Mathf.Cos(b)*.15f,Mathf.Sin(b)*.15f+.19f)*s,s*.036f);}
        for(int i=0;i<24;i++){float x=-.34f+i*.68f/24,y=x+.68f/24;Line(vh,center+new Vector2(x,.015f-.34f*(x/.34f)*(x/.34f))*s,center+new Vector2(y,.015f-.34f*(y/.34f)*(y/.34f))*s,s*.036f);}
    }
    void Line(VertexHelper vh,Vector2 a,Vector2 b,float w){Vector2 d=b-a,n=new Vector2(-d.y,d.x).normalized*w*.5f;int i=vh.currentVertCount;vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i+1,i+3,i+2);}
}
