using UnityEngine;

// Panah kecil pada penanda pemain; tidak bergantung pada glyph font.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class PanahYouPeta : UnityEngine.UI.MaskableGraphic
{
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
    {
        vh.Clear();
        Gambar(vh,Color.white,2.5f);
        Gambar(vh,color,1.25f);
    }
    void Gambar(UnityEngine.UI.VertexHelper vh,Color warna,float tebal)
    {
        Garis(vh,new Vector2(0,8),new Vector2(0,-7),warna,tebal);
        Garis(vh,new Vector2(-6,0),new Vector2(0,-7),warna,tebal);
        Garis(vh,new Vector2(6,0),new Vector2(0,-7),warna,tebal);
    }
    void Garis(UnityEngine.UI.VertexHelper vh,Vector2 a,Vector2 b,Color warna,float tebal)
    {
        Vector2 normal=new Vector2(-(b-a).y,(b-a).x).normalized*tebal;
        int i=vh.currentVertCount;
        vh.AddVert(a+normal,warna,Vector2.zero);vh.AddVert(a-normal,warna,Vector2.zero);
        vh.AddVert(b+normal,warna,Vector2.zero);vh.AddVert(b-normal,warna,Vector2.zero);
        vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i+1,i+3,i+2);
    }
}
