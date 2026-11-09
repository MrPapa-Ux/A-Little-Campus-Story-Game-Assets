// PANDUAN PROYEK - Runtime / Ponsel/Tampilan
// Fungsi: Menggambar bingkai ponsel dari tekstur melalui mesh UI.
// Panduan: Assets/Scripts/Runtime/Ponsel/Tampilan/README.md

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class BingkaiPonselUI : MaskableGraphic
{
    public Texture teksturHP;
    public override Texture mainTexture=>teksturHP!=null?teksturHP:Texture2D.whiteTexture;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;
        // Pertahankan potongan sudut asli; hanya garis lurus memanjang mengikuti layar.
        float[] x={r.xMin,r.xMin+36,r.xMax-36,r.xMax};
        float[] y={r.yMin,r.yMin+66,r.yMax-66,r.yMax};
        float[] u={.29f,.38f,.72f,.81f},v={.03f,.216f,.696f,.86f};
        for(int row=0;row<3;row++)for(int col=0;col<3;col++)
        {
            // Layar ditutup latar sendiri; bagian tombol home memakai bidang polos.
            if(row==1&&col==1)continue;
            float a=u[col],b=u[col+1];if(row==0&&col==1){a=.39f;b=.41f;}
            Quad(vh,x[col],y[row],x[col+1],y[row+1],a,v[row],b,v[row+1]);
        }
    }
    void Quad(VertexHelper vh,float x0,float y0,float x1,float y1,float u0,float v0,float u1,float v1)
    {
        int n=vh.currentVertCount;vh.AddVert(new Vector3(x0,y0),color,new Vector2(u0,v0));vh.AddVert(new Vector3(x0,y1),color,new Vector2(u0,v1));vh.AddVert(new Vector3(x1,y1),color,new Vector2(u1,v1));vh.AddVert(new Vector3(x1,y0),color,new Vector2(u1,v0));vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
    }
}
