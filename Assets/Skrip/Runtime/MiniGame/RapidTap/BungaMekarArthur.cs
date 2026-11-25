using UnityEngine;
using UnityEngine.UI;

// Bunga berupa geometri UI ringan; tidak menambah tekstur besar atau kamera baru.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class BungaMekarArthur : UnityEngine.UI.Graphic
{
    [Range(0,1)] public float mekar;
    public void Atur(float nilai){mekar=Mathf.Clamp01(nilai);SetVerticesDirty();}
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();float ukuran=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.48f;
        for(int i=0;i<8;i++){
            float sudut=i*Mathf.PI/4;var arah=new Vector2(Mathf.Cos(sudut),Mathf.Sin(sudut));
            float r=ukuran*Mathf.Lerp(.30f,.48f,mekar);Vector2 pusat=arah*ukuran*Mathf.Lerp(.20f,.44f,mekar);
            Elips(vh,pusat,arah,r,r*.58f,new Color32(32,29,27,255));
            Elips(vh,pusat,arah,r*.91f,r*.50f,Color.Lerp(new Color(.56f,.63f,.46f),new Color(.94f,.65f,.25f),mekar));
        }
        Elips(vh,Vector2.zero,Vector2.right,ukuran*.31f,ukuran*.31f,new Color32(32,29,27,255));
        Elips(vh,Vector2.zero,Vector2.right,ukuran*.27f,ukuran*.27f,new Color(.97f,.81f,.48f));
    }
    static void Elips(VertexHelper vh,Vector2 pusat,Vector2 arah,float panjang,float lebar,Color warna){
        int start=vh.currentVertCount;var silang=new Vector2(-arah.y,arah.x);vh.AddVert(pusat,warna,Vector2.zero);
        const int n=28;for(int i=0;i<=n;i++){float t=i*Mathf.PI*2/n;float kasar=1+Mathf.Sin(i*5.7f)*.018f;vh.AddVert(pusat+(arah*Mathf.Cos(t)*panjang+silang*Mathf.Sin(t)*lebar)*kasar,warna,Vector2.zero);if(i>0)vh.AddTriangle(start,start+i,start+i+1);}
    }
}
