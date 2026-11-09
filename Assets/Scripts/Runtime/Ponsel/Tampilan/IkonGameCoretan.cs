// PANDUAN PROYEK - Runtime / Ponsel/Tampilan
// Fungsi: Menggambar ikon controller bergaya coretan untuk aplikasi game.
// Panduan: Assets/Scripts/Runtime/Ponsel/Tampilan/README.md

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class IkonGameCoretan : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var r=rectTransform.rect;
        Vector2[] p={new Vector2(-.43f,-.22f),new Vector2(-.33f,.25f),new Vector2(-.18f,.32f),new Vector2(.18f,.29f),new Vector2(.36f,.19f),new Vector2(.44f,-.24f),new Vector2(.28f,-.33f),new Vector2(.12f,-.1f),new Vector2(-.15f,-.12f),new Vector2(-.29f,-.34f)};
        var body=new Color32(238,164,68,255);
        var ink=new Color32(35,31,28,255);
        for(int i=0;i<p.Length;i++)
        {
            Vector2 a=Scale(p[i],r),b=Scale(p[(i+1)%p.Length],r);
            Tri(vh,r.center,a,b,body);
            // Garis utama dan garis kedua yang sedikit meleset memberi kesan tinta/crayon.
            Line(vh,a,b,3.2f,ink);
            Vector2 wobble=new Vector2(Mathf.Sin(i*2.31f)*1.35f,Mathf.Cos(i*1.73f)*1.15f);
            Line(vh,a+wobble,b+wobble,1.15f,new Color32(40,31,25,145));
        }

        // Kilau kertas pendek pada badan controller, tetap terbaca pada ukuran ikon HP.
        Line(vh,Scale(new Vector2(-.16f,.19f),r),Scale(new Vector2(.13f,.18f),r),1.4f,new Color32(255,224,159,220));
        Vector2 left=Scale(new Vector2(-.22f,.04f),r);
        Line(vh,left+Vector2.left*9,left+Vector2.right*9,4.6f,ink);
        Line(vh,left+Vector2.down*9,left+Vector2.up*9,4.6f,ink);
        Dot(vh,Scale(new Vector2(.22f,.11f),r),5,new Color32(205,68,46,255));
        Dot(vh,Scale(new Vector2(.32f,-.03f),r),5,new Color32(59,119,121,255));
    }
    static Vector2 Scale(Vector2 p,Rect r)=>r.center+new Vector2(p.x*r.width,p.y*r.height);
    static void Tri(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color col){int n=vh.currentVertCount;vh.AddVert(a,col,Vector2.zero);vh.AddVert(b,col,Vector2.zero);vh.AddVert(c,col,Vector2.zero);vh.AddTriangle(n,n+1,n+2);}
    static void Line(VertexHelper vh,Vector2 a,Vector2 b,float w,Color c){var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*w/2;Tri(vh,a-n,a+n,b+n,c);Tri(vh,a-n,b+n,b-n,c);}
    static void Dot(VertexHelper vh,Vector2 center,float radius,Color color){for(int i=0;i<8;i++){float a=i*Mathf.PI/4,b=(i+1)*Mathf.PI/4;Tri(vh,center,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,color);}}
}
