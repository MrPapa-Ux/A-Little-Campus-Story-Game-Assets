// Tap visible circles before their countdown expires. There is no scan line or beat gate.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasRenderer))]
public class PapanRhythmArthur : MaskableGraphic, IPointerDownHandler
{
    public RhythmArthur game;
    public Color tinta=new Color32(17,28,39,255),oranye=new Color32(245,171,73,255);
    public float[] kilat=new float[4];
    public float Garis=>0; // Compatibility for the archived checkpoint-17 test helper.
    public float RadiusNot=>game==null?62:game.LevelPilihan==1?62:game.LevelPilihan==2?54:48;
    static readonly int[] positions={0,6,3,9,5,2,11,4,8,1,7,10};
    public void Segarkan(){SetVerticesDirty();}
    public Vector2 PosisiNot(int index)
    {
        var r=rectTransform.rect;int cell=positions[index%positions.Length];
        return new Vector2(Mathf.Lerp(r.xMin+96,r.xMax-96,(cell%4)/3f),Mathf.Lerp(r.yMin+82,r.yMax-82,(cell/4)/2f));
    }
    public bool Terlihat(int index)
    {
        if(game==null||game.Ronde==null||index<0||index>=game.Ronde.not.Length||game.Ronde.hasil[index]!=0)return false;
        double age=game.WaktuLagu-game.Ronde.not[index].waktu;
        return age>=0&&age<=game.DurasiUntukNot(index);
    }
    public void OnPointerDown(PointerEventData e)
    {
        if(e.button!=PointerEventData.InputButton.Left||game==null||game.Status!=RhythmArthur.Fase.Bermain)return;
        if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,e.position,e.pressEventCamera,out var point))return;
        int closest=-1;float distance=(RadiusNot+12)*(RadiusNot+12);
        for(int i=0;i<game.Ronde.not.Length;i++)
        {
            if(!Terlihat(i))continue;
            float d=(PosisiNot(i)-point).sqrMagnitude;if(d<distance){closest=i;distance=d;}
        }
        if(closest>=0)game.PukulNot(closest);
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;Box(vh,r,tinta);
        if(game==null||game.Ronde==null)return;
        for(int i=0;i<game.Ronde.not.Length;i++)
        {
            if(!Terlihat(i))continue;
            float age=(float)(game.WaktuLagu-game.Ronde.not[i].waktu);
            float left=Mathf.Clamp01(1-age/game.DurasiUntukNot(i));var p=PosisiNot(i);
            float appear=Mathf.Lerp(.85f,1,Mathf.Clamp01(age/.09f));
            Circle(vh,p,RadiusNot*appear,new Color32(53,68,78,255));
            Arc(vh,p,RadiusNot,4,new Color(1,.86f,.59f,.95f),1);
            Arc(vh,p,RadiusNot+9,5,oranye,left);
            Circle(vh,p,9,new Color32(250,226,176,255));
        }
    }
    public static void Circle(VertexHelper vh,Vector2 p,float radius,Color c)
    {
        const int segments=40;int start=vh.currentVertCount;vh.AddVert(p,c,Vector2.zero);
        for(int i=0;i<=segments;i++){float a=i*Mathf.PI*2/segments;vh.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,c,Vector2.zero);if(i>0)vh.AddTriangle(start,start+i,start+i+1);}
    }
    public static void Arc(VertexHelper vh,Vector2 p,float radius,float width,Color c,float fraction)
    {
        const int segments=48;int start=vh.currentVertCount;
        for(int i=0;i<=segments;i++){float a=Mathf.PI*.5f-i*Mathf.PI*2*fraction/segments;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));vh.AddVert(p+d*radius,c,Vector2.zero);vh.AddVert(p+d*(radius-width),c,Vector2.zero);if(i>0){int k=start+(i-1)*2;vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k+1,k+3,k+2);}}
    }
    static void Box(VertexHelper vh,Rect r,Color c)
    {
        int n=vh.currentVertCount;vh.AddVert(new Vector3(r.xMin,r.yMin),c,Vector2.zero);vh.AddVert(new Vector3(r.xMin,r.yMax),c,Vector2.zero);vh.AddVert(new Vector3(r.xMax,r.yMax),c,Vector2.zero);vh.AddVert(new Vector3(r.xMax,r.yMin),c,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
    }
}
