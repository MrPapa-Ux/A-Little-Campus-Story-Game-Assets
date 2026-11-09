// PANDUAN PROYEK - Runtime / ParameterDanGlitch
// Fungsi: Menggambar potongan kotak gangguan untuk efek glitch melalui mesh UI.
// Panduan: Assets/Scripts/Runtime/ParameterDanGlitch/README.md

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class KotakGlitchUI : MaskableGraphic
{
    struct Potongan {public Rect rect;public Color warna;}
    readonly Potongan[] potongan=new Potongan[80];
    public Rect[] Jendela {get;}=new Rect[6];
    int jumlah,jendela;
    readonly Vector2[] pusat={new Vector2(.53f,.53f),new Vector2(.24f,.73f),new Vector2(.75f,.29f),new Vector2(.69f,.76f),new Vector2(.32f,.27f),new Vector2(.49f,.43f)};
    public void Acak(System.Random rng,int banyak,float progress,int windows)
    {
        var r=rectTransform.rect;jumlah=Mathf.Clamp(banyak,0,potongan.Length);jendela=windows;
        for(int i=0;i<jumlah;i++)
        {
            float w=r.width*(i%3==0?.16f+(float)rng.NextDouble()*.6f:.015f+(float)rng.NextDouble()*.19f);
            float h=r.height*(i%3==0?.002f+(float)rng.NextDouble()*.014f:.012f+(float)rng.NextDouble()*.065f);
            var col=i%4==0?new Color(.05f,.85f,.8f,.25f):i%4==1?new Color(.9f,.09f,.25f,.36f):new Color(.01f,.015f,.025f,.72f);
            potongan[i]=new Potongan{rect=new Rect(r.xMin+(float)rng.NextDouble()*(r.width-w),r.yMin+(float)rng.NextDouble()*(r.height-h),w,h),warna=col};
        }
        for(int i=0;i<jendela;i++)
        {
            float w=r.width*(i==0?.43f:.32f),h=r.height*(i==0?.25f:.21f);
            var c=new Vector2(r.xMin+pusat[i].x*r.width,r.yMin+pusat[i].y*r.height)+new Vector2((float)rng.NextDouble()-.5f,(float)rng.NextDouble()-.5f)*18*progress;
            Jendela[i]=new Rect(c-new Vector2(w,h)/2,new Vector2(w,h));
        }
        SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        for(int i=0;i<jumlah;i++)Box(vh,potongan[i].rect,potongan[i].warna);
        for(int i=0;i<jendela;i++)
        {
            var r=Jendela[i];var offset=r;offset.position+=new Vector2(8,-7);Box(vh,offset,new Color(.03f,.85f,.78f,.65f));
            Box(vh,r,new Color(.72f,.09f,.17f,.98f));r.xMin+=3;r.yMin+=3;r.xMax-=3;r.yMax-=3;Box(vh,r,new Color(.035f,.015f,.027f,.98f));
        }
        // Fine horizontal tears; no full-screen white flash.
        var screen=rectTransform.rect;
        for(float y=screen.yMin;y<screen.yMax;y+=13)Box(vh,new Rect(screen.xMin,y,screen.width,1),new Color(0,0,0,.2f));
    }
    static void Box(VertexHelper vh,Rect r,Color col)
    {
        int i=vh.currentVertCount;vh.AddVert(new Vector3(r.xMin,r.yMin),col,Vector2.zero);vh.AddVert(new Vector3(r.xMin,r.yMax),col,Vector2.zero);vh.AddVert(new Vector3(r.xMax,r.yMax),col,Vector2.zero);vh.AddVert(new Vector3(r.xMax,r.yMin),col,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
    }
}
