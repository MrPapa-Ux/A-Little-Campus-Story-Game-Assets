using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

// Denah lantai satu pengguna adalah sumber posisi. Video adalah sumber tampilan.
// Aset terpisah: tidak mengganti scene kampus dan tidak membuat lantai kedua.
public static class AsramaCrystalEdelLantaiSatu
{
    public const string Root="Assets/Asrama Crystal Edel Referensi";
    public const string Work="work/asrama-crystal-edel-20261111/";
    const float H=3.3f, DoorH=2.18f, C=1.60f, RoomDepth=4.8f, LobbyFront=-9.8f;
    [Serializable] public class Room { public string key; public float x,z,w,d,doorX,doorZ,dx,dz; public bool special,terminal; }
    [Serializable] public class Layout { public Room[] rooms; }
    struct Gap { public float at,width; public Gap(float a,float w){at=a;width=w;} }
    static Material white,mint,lime,cream,wood,tile,dark,blue,red,glass,cloth,leaf,metal,mirror,curtain,crystalSign,edelSign;
    static readonly List<string> checks=new List<string>();
    static int meshNumber;
    sealed class Geo
    {
        public readonly List<Vector3> vertices=new List<Vector3>(),normals=new List<Vector3>();
        public readonly List<Vector2> uv=new List<Vector2>();
        public readonly List<Material> mats=new List<Material>();
        readonly List<List<int>> triangles=new List<List<int>>();
        public void Face(Vector3[] points,Vector3 normal,Material mat)
        {
            if(Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),normal)<0)points=points.Reverse().ToArray();
            int slot=mats.IndexOf(mat);if(slot<0){slot=mats.Count;mats.Add(mat);triangles.Add(new List<int>());}
            int start=vertices.Count;
            foreach(var p in points){vertices.Add(p);normals.Add(normal);uv.Add(Mathf.Abs(normal.y)>.7f?new Vector2(p.x,p.z):Mathf.Abs(normal.x)>.7f?new Vector2(p.z,p.y):new Vector2(p.x,p.y));}
            for(int i=1;i<points.Length-1;i++)triangles[slot].AddRange(new[]{start,start+i,start+i+1});
        }
        public void Box(Vector3 p,Vector3 size,Material mat,Quaternion q=default)
        {
            if(q==default)q=Quaternion.identity;Vector3 h=size*.5f;
            for(int axis=0;axis<3;axis++)foreach(float sign in new[]{-1f,1f}){
                int u=(axis+1)%3,v=(axis+2)%3;var normal=Vector3.zero;normal[axis]=sign;normal=q*normal;
                var points=new Vector3[4];int n=0;
                foreach(var xy in new[]{new Vector2(-1,-1),new Vector2(1,-1),new Vector2(1,1),new Vector2(-1,1)}){var a=Vector3.zero;a[axis]=h[axis]*sign;a[u]=h[u]*xy.x;a[v]=h[v]*xy.y;points[n++]=p+q*a;}
                Face(points,normal,mat);
            }
        }
        public void Bevel(Vector3 p,Vector3 size,Material mat)
        {
            var h=size*.5f;float b=Mathf.Min(.018f,Mathf.Min(h.x,Mathf.Min(h.y,h.z))*.3f);
            for(int axis=0;axis<3;axis++)foreach(float s in new[]{-1f,1f}){
                int u=(axis+1)%3,v=(axis+2)%3;var n=Vector3.zero;n[axis]=s;var points=new Vector3[4];int k=0;
                foreach(var xy in new[]{new Vector2(-1,-1),new Vector2(1,-1),new Vector2(1,1),new Vector2(-1,1)}){var a=Vector3.zero;a[axis]=h[axis]*s;a[u]=(h[u]-b)*xy.x;a[v]=(h[v]-b)*xy.y;points[k++]=p+a;}Face(points,n,mat);
            }
            for(int axis=0;axis<3;axis++)foreach(float su in new[]{-1f,1f})foreach(float sv in new[]{-1f,1f}){
                int u=(axis+1)%3,v=(axis+2)%3;var points=new Vector3[4];
                for(int i=0;i<4;i++){bool first=i<2;var a=Vector3.zero;a[axis]=(i==0||i==3?-1:1)*(h[axis]-b);a[u]=su*(h[u]-(first?0:b));a[v]=sv*(h[v]-(first?b:0));points[i]=p+a;}
                var normal=Vector3.zero;normal[u]=su;normal[v]=sv;Face(points,normal.normalized,mat);
            }
            foreach(float x in new[]{-1f,1f})foreach(float y in new[]{-1f,1f})foreach(float z in new[]{-1f,1f}){var s=new Vector3(x,y,z);var points=new Vector3[3];for(int i=0;i<3;i++){var a=Vector3.Scale(s,h-Vector3.one*b);a[i]=s[i]*h[i];points[i]=p+a;}Face(points,s.normalized,mat);}
        }
        public void Rod(Vector3 a,Vector3 b,float width,Material mat){Box((a+b)*.5f,new Vector3(width,width,(b-a).magnitude),mat,Quaternion.LookRotation(b-a));}
        public void Poster(Vector3 center,float width,float height,Material mat)
        {
            int slot=mats.IndexOf(mat);if(slot<0){slot=mats.Count;mats.Add(mat);triangles.Add(new List<int>());}
            int start=vertices.Count;
            foreach(var corner in new[]{new Vector2(-.5f,-.5f),new Vector2(.5f,-.5f),new Vector2(.5f,.5f),new Vector2(-.5f,.5f)}){
                vertices.Add(center+new Vector3(corner.x*width,corner.y*height,0));normals.Add(Vector3.forward);uv.Add(new Vector2(.5f-corner.x,corner.y+.5f));
            }
            triangles[slot].AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
        }
        public void RoundedPanel(Vector3 p,Vector3 size,float radius,Material mat,bool upright=false)
        {
            float hx=size.x*.5f,hy=(upright?size.y:size.z)*.5f,thickness=(upright?size.z:size.y)*.5f;
            radius=Mathf.Min(radius,Mathf.Min(hx,hy));var a=new List<Vector3>();var b=new List<Vector3>();
            for(int corner=0;corner<4;corner++)for(int step=0;step<5;step++){
                float angle=(corner*90+step*22.5f)*Mathf.Deg2Rad;
                float cx=(corner==0||corner==3?1:-1)*(hx-radius),cy=(corner<2?1:-1)*(hy-radius);
                float x=cx+Mathf.Cos(angle)*radius,y=cy+Mathf.Sin(angle)*radius;
                a.Add(p+(upright?new Vector3(x,y,thickness):new Vector3(x,thickness,y)));
                b.Add(p+(upright?new Vector3(x,y,-thickness):new Vector3(x,-thickness,y)));
            }
            var normal=upright?Vector3.forward:Vector3.up;Face(a.ToArray(),normal,mat);Face(b.ToArray(),-normal,mat);
            for(int i=0;i<a.Count;i++){int j=(i+1)%a.Count;var n=(a[i]+a[j]+b[i]+b[j])*.25f-p;Face(new[]{a[i],a[j],b[j],b[i]},n.normalized,mat);}
        }
        public void Cylinder(Vector3 p,float radius,float height,Material mat,int sides=12,float topRatio=1)
        {
            var top=new Vector3[sides];var bottom=new Vector3[sides];
            for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;var dir=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));top[i]=p+Vector3.up*height*.5f+dir*radius*topRatio;bottom[i]=p-Vector3.up*height*.5f+dir*radius;}
            Face(top,Vector3.up,mat);Face(bottom,Vector3.down,mat);
            for(int i=0;i<sides;i++){int j=(i+1)%sides;var normal=(bottom[i]+bottom[j])*.5f-p;normal.y=0;Face(new[]{bottom[i],bottom[j],top[j],top[i]},normal.normalized,mat);}
        }
        public Mesh Mesh(string name){var m=new Mesh{name=name};if(vertices.Count>65535)m.indexFormat=IndexFormat.UInt32;m.SetVertices(vertices);m.SetNormals(normals);m.SetUVs(0,uv);m.subMeshCount=mats.Count;for(int i=0;i<mats.Count;i++)m.SetTriangles(triangles[i],i);m.RecalculateBounds();return m;}
    }
    static Transform Group(string n,Transform p){var t=new GameObject(n).transform;t.SetParent(p,false);return t;}
    static Material Mat(string name,Color color,Texture2D texture=null,float smooth=.18f)
    {
        string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
        m.SetColor("_BaseColor",color);m.SetTexture("_BaseMap",texture);m.SetFloat("_Smoothness",smooth);m.enableInstancing=true;EditorUtility.SetDirty(m);return m;
    }
    static Texture2D Texture(string name,Func<int,int,Color> paint)
    {
        string path=Root+"/Textures/"+name+".png";
        if(!File.Exists(path)){var t=new Texture2D(512,512,TextureFormat.RGB24,false);var pixels=new Color[512*512];for(int y=0;y<512;y++)for(int x=0;x<512;x++)pixels[y*512+x]=paint(x,y);t.SetPixels(pixels);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);}
        AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=2;importer.maxTextureSize=512;importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=512,format=TextureImporterFormat.ETC2_RGB4});importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static GameObject Part(string name,Geo g,Transform parent,bool collider=false,bool numbered=true)
    {
        string revision=name=="Tulisan nama asrama"?" - UV R3":name=="Cabang T - dua PK di kiri"||name=="Lekukan bertingkat - sudut pohon dan bidang TV"?" - Tanpa Sekat R4":"";
        string path=Root+"/Meshes/"+(numbered?(++meshNumber).ToString("000")+" - ":"Perabot R2 - ")+name+revision+".asset";
        var generated=g.Mesh(name);var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(mesh==null){mesh=generated;AssetDatabase.CreateAsset(mesh,path);}else{
            // Pakai API mesh agar data GPU ikut berubah saat topologi/ukuran diperbarui.
            mesh.Clear();mesh.indexFormat=generated.indexFormat;mesh.vertices=generated.vertices;mesh.normals=generated.normals;mesh.uv=generated.uv;mesh.subMeshCount=generated.subMeshCount;
            for(int sub=0;sub<generated.subMeshCount;sub++)mesh.SetTriangles(generated.GetTriangles(sub),sub);
            mesh.RecalculateBounds();Object.DestroyImmediate(generated);
        }
        mesh.UploadMeshData(false);EditorUtility.SetDirty(mesh);AssetDatabase.SaveAssetIfDirty(mesh);
        var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterials=g.mats.ToArray();
        if(collider)go.AddComponent<MeshCollider>().sharedMesh=mesh;
        return go;
    }
    static GameObject BoxPart(string name,Vector3 pos,Vector3 size,Material mat,Transform parent,bool collider=false){var g=new Geo();g.Box(pos,size,mat);return Part(name,g,parent,collider);}
    static void Wall(Geo g,float from,float to,float fixedAxis,bool horizontal,Material lower,float bottom=0,float top=H)
    {
        if(to-from<.005f)return;
        void Band(float lo,float hi,Material m){if(hi<=lo)return;g.Box(horizontal?new Vector3((from+to)*.5f,(lo+hi)*.5f,fixedAxis):new Vector3(fixedAxis,(lo+hi)*.5f,(from+to)*.5f),horizontal?new Vector3(to-from,hi-lo,.12f):new Vector3(.12f,hi-lo,to-from),m);}
        Band(bottom,Mathf.Min(top,1.35f),lower);Band(Mathf.Max(bottom,1.35f),top,white);
    }
    static void WallGaps(Geo g,float from,float to,float at,bool horizontal,Material lower,IEnumerable<Gap> gaps,float openingHeight=DoorH)
    {
        float p=from;
        foreach(var gap in gaps.OrderBy(a=>a.at)){
            float a=Mathf.Clamp(gap.at-gap.width*.5f,from,to),b=Mathf.Clamp(gap.at+gap.width*.5f,from,to);
            if(b<=p)continue;Wall(g,p,a,at,horizontal,lower);Wall(g,Mathf.Max(p,a),b,at,horizontal,lower,openingHeight,H);p=b;
        }
        Wall(g,p,to,at,horizontal,lower);
    }
    static void Window(Geo shell,Geo panes,float center,float at,float width,bool horizontal,Material lower,float low=1.45f,float high=2.57f)
    {
        Wall(shell,center-width*.5f,center+width*.5f,at,horizontal,lower,0,low);Wall(shell,center-width*.5f,center+width*.5f,at,horizontal,lower,high,H);
        for(int i=0;i<5;i++){float v=center-width*.5f+i*width/4;shell.Box(horizontal?new Vector3(v,(low+high)*.5f,at):new Vector3(at,(low+high)*.5f,v),horizontal?new Vector3(.035f,high-low+.03f,.11f):new Vector3(.11f,high-low+.03f,.035f),cream);}
        foreach(float y in new[]{low,(low+high)*.5f,high})shell.Box(horizontal?new Vector3(center,y,at):new Vector3(at,y,center),horizontal?new Vector3(width,.04f,.13f):new Vector3(.13f,.04f,width),cream);
        float slat=(high-low)/9;for(int i=0;i<9;i++)panes.Box(horizontal?new Vector3(center,low+(i+.5f)*slat,at):new Vector3(at,low+(i+.5f)*slat,center),horizontal?new Vector3(width-.05f,slat-.005f,.016f):new Vector3(.016f,slat-.005f,width-.05f),glass);
    }
    static void Door(Transform parent,Vector3 center,Vector3 normal,string name,bool locked=false,float width=.78f,float opening=85)
    {
        var root=Group(name,parent);root.localPosition=center;root.localRotation=Quaternion.LookRotation(normal);
        var frameGeo=new Geo();foreach(float x in new[]{-width*.5f-.035f,width*.5f+.035f})frameGeo.Box(new Vector3(x,DoorH*.5f,0),new Vector3(.065f,DoorH,.15f),cream);frameGeo.Box(new Vector3(0,DoorH+.025f,0),new Vector3(width+.13f,.07f,.15f),cream);Part("Kusen",frameGeo,root);
        var hinge=Group(locked?"Engsel terkunci":"Engsel pintu - dapat diputar",root);hinge.localPosition=new Vector3(-width*.5f,0,0);hinge.localRotation=Quaternion.Euler(0,locked?0:opening,0);
        var leafGeo=new Geo();leafGeo.Bevel(new Vector3(width*.5f,DoorH*.5f,0),new Vector3(width,DoorH-.035f,.045f),wood);
        foreach(float y in new[]{.45f,1.1f,1.77f})leafGeo.Bevel(new Vector3(width*.5f,y,-.028f),new Vector3(width-.15f,.48f,.016f),wood);
        leafGeo.Rod(new Vector3(width-.12f,1.02f,-.08f),new Vector3(width-.23f,1.02f,-.08f),.025f,metal);
        var leafObj=Part("Daun pintu",leafGeo,hinge);var collider=leafObj.AddComponent<BoxCollider>();collider.center=new Vector3(width*.5f,DoorH*.5f,0);collider.size=new Vector3(width,DoorH,.05f);
    }
    static GameObject bed,desk,wardrobe,bench,plant,waitingChair,woodChair;
    static GameObject Template(string name,Geo g,Vector3 colliderCenter,Vector3 colliderSize)
    {
        var go=Part(name,g,null,false,false);var c=go.AddComponent<BoxCollider>();c.center=colliderCenter;c.size=colliderSize;
        string path=Root+"/Prefabs/Perabot/"+name+".prefab";PrefabUtility.SaveAsPrefabAsset(go,path);Object.DestroyImmediate(go);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
    static GameObject Place(GameObject prefab,Transform parent,Vector3 position,float yaw=0,string name=null)
    {
        var obj=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);obj.transform.localPosition=position;obj.transform.localRotation=Quaternion.Euler(0,yaw,0);if(name!=null)obj.name=name;return obj;
    }
    static void Furniture()
    {
        var g=new Geo();g.Bevel(new Vector3(0,.38f,0),new Vector3(.98f,.13f,2.05f),wood);g.Bevel(new Vector3(0,.51f,0),new Vector3(.91f,.19f,1.96f),cloth);g.Bevel(new Vector3(0,.64f,.70f),new Vector3(.63f,.12f,.40f),white);g.Bevel(new Vector3(0,.63f,-.40f),new Vector3(.93f,.045f,.85f),blue);
        foreach(float x in new[]{-.42f,.42f})foreach(float z in new[]{-.91f,.91f})g.Box(new Vector3(x,.17f,z),new Vector3(.07f,.34f,.07f),wood);g.Bevel(new Vector3(0,.68f,1f),new Vector3(.98f,.62f,.06f),wood);bed=Template("Tempat tidur tunggal",g,new Vector3(0,.43f,0),new Vector3(1,.86f,2.1f));
        g=new Geo();g.Bevel(new Vector3(0,.76f,0),new Vector3(1.05f,.05f,.55f),wood);foreach(float x in new[]{-.46f,.46f})foreach(float z in new[]{-.22f,.22f})g.Box(new Vector3(x,.37f,z),new Vector3(.05f,.74f,.05f),dark);g.Bevel(new Vector3(-.22f,.80f,0),new Vector3(.30f,.025f,.23f),cream);desk=Template("Meja belajar",g,new Vector3(0,.4f,0),new Vector3(1.05f,.8f,.55f));
        g=new Geo();g.Bevel(new Vector3(0,1f,0),new Vector3(1.15f,2f,.56f),wood);foreach(float x in new[]{-.285f,.285f}){g.Bevel(new Vector3(x,1f,-.295f),new Vector3(.55f,1.9f,.03f),cream);g.Rod(new Vector3(x+(x>0?-.18f:.18f),.95f,-.33f),new Vector3(x+(x>0?-.18f:.18f),1.13f,-.33f),.02f,dark);}wardrobe=Template("Lemari kamar",g,new Vector3(0,1,0),new Vector3(1.15f,2,.63f));
        var upholstery=Mat("Bantalan cokelat bangku video",new Color(.20f,.105f,.075f),null,.28f);
        g=new Geo();g.Bevel(new Vector3(0,.38f,0),new Vector3(1.82f,.10f,.65f),wood);g.RoundedPanel(new Vector3(0,.48f,0),new Vector3(1.64f,.13f,.56f),.075f,upholstery);g.Bevel(new Vector3(0,.79f,.30f),new Vector3(1.82f,.63f,.07f),wood);g.RoundedPanel(new Vector3(0,.80f,.245f),new Vector3(1.62f,.45f,.08f),.06f,upholstery,true);foreach(float x in new[]{-.86f,.86f}){g.Bevel(new Vector3(x,.68f,0),new Vector3(.11f,.07f,.68f),wood);foreach(float z in new[]{-.25f,.25f})g.Box(new Vector3(x,.33f,z),new Vector3(.07f,.66f,.07f),wood);}bench=Template("Bangku kayu lobi",g,new Vector3(0,.55f,0),new Vector3(1.9f,1.1f,.72f));
        g=new Geo();g.Bevel(new Vector3(0,.40f,0),new Vector3(.69f,.08f,.62f),wood);g.RoundedPanel(new Vector3(0,.48f,0),new Vector3(.57f,.12f,.52f),.08f,upholstery);g.Bevel(new Vector3(0,.79f,.28f),new Vector3(.67f,.61f,.055f),wood);g.RoundedPanel(new Vector3(0,.80f,.235f),new Vector3(.55f,.43f,.08f),.07f,upholstery,true);foreach(float x in new[]{-.32f,.32f}){g.Bevel(new Vector3(x,.68f,0),new Vector3(.08f,.055f,.65f),wood);foreach(float z in new[]{-.24f,.24f})g.Box(new Vector3(x,.33f,z),new Vector3(.055f,.66f,.055f),wood);}woodChair=Template("Kursi kayu belakang meja",g,new Vector3(0,.55f,0),new Vector3(.75f,1.1f,.72f));
        g=new Geo();g.Cylinder(new Vector3(0,.33f,0),.23f,.66f,cream,12,.83f);for(int i=0;i<9;i++){float a=i*2.399f;var tip=new Vector3(Mathf.Cos(a)*.38f,.85f+(i%3)*.12f,Mathf.Sin(a)*.38f);g.Rod(new Vector3(0,.63f,0),tip,.018f,leaf);var tangent=new Vector3(-Mathf.Sin(a),0,Mathf.Cos(a))*.10f;g.Face(new[]{tip-tangent,tip+Vector3.up*.20f,tip+tangent,tip-Vector3.up*.13f},new Vector3(Mathf.Cos(a),.4f,Mathf.Sin(a)).normalized,leaf);}plant=Template("Pot tanaman lobi",g,new Vector3(0,.33f,0),new Vector3(.5f,.66f,.5f));
        g=new Geo();g.RoundedPanel(new Vector3(0,.48f,0),new Vector3(.55f,.14f,.51f),.13f,blue);g.RoundedPanel(new Vector3(0,.82f,.23f),new Vector3(.55f,.58f,.12f),.14f,blue,true);foreach(float x in new[]{-.28f,.28f}){g.Rod(new Vector3(x,.14f,-.23f),new Vector3(x,.44f,0),.035f,dark);g.Box(new Vector3(x,.045f,0),new Vector3(.045f,.05f,.52f),dark);g.Bevel(new Vector3(x,.69f,0),new Vector3(.06f,.045f,.43f),dark);g.Rod(new Vector3(x,.45f,-.12f),new Vector3(x,.67f,-.12f),.028f,dark);}g.Box(new Vector3(0,.34f,0),new Vector3(.62f,.045f,.045f),dark);waitingChair=Template("Kursi tunggu biru",g,new Vector3(0,.55f,0),new Vector3(.64f,1.1f,.64f));
    }
    static void FillRoom(Transform parent,float width,float depth,float yaw=0)
    {
        var items=Group("Perabot kamar sementara - dapat diubah",parent);items.localRotation=Quaternion.Euler(0,yaw,0);
        foreach(float side in new[]{-1f,1f})Place(bed,items,new Vector3(side*(width*.5f-.65f),0,.35f));
        Place(desk,items,new Vector3(0,0,.94f),180);
        Place(wardrobe,items,new Vector3(0,0,depth*.5f-.40f));
    }
    static void RoomModel(Room r,Transform parent,bool mirrored,Geo shell,Geo panes,Transform ceiling)
    {
        float sign=mirrored?-1:1;float x=r.x*sign,dx=r.dx*sign;Material lower=mirrored?lime:mint;
        var root=Group((mirrored?"R":"L")+r.key.Substring(1)+(r.special?" - kamar melalui LK":r.terminal?" - kamar ujung":" - kamar"),parent);root.localPosition=new Vector3(x,0,r.z);
        if(r.terminal){
            float wallX=x-sign*r.w*.5f;Wall(shell,r.z-r.d*.5f,r.z+r.d*.5f,wallX,false,lower);
            foreach(float z in new[]{r.z-r.d*.5f,r.z+r.d*.5f})Wall(shell,x-r.w*.5f,x+r.w*.5f,z,true,lower);
            WallGaps(shell,r.z-r.d*.5f,r.z+r.d*.5f,x+sign*r.w*.5f,false,lower,new[]{new Gap(r.doorZ,.70f)});
            Door(root,new Vector3(sign*r.w*.5f,0,r.doorZ-r.z),new Vector3(dx,0,0),"PK ujung",false,.70f);
        }else{
            foreach(float side in new[]{-1f,1f})Wall(shell,r.z-r.d*.5f,r.z+r.d*.5f,x+side*r.w*.5f,false,lower);
            float back=r.z-r.dz*r.d*.5f,ww=1.4f;
            Wall(shell,x-r.w*.5f,x-ww*.5f,back,true,lower);Wall(shell,x+ww*.5f,x+r.w*.5f,back,true,lower);Window(shell,panes,x,back,ww,true,lower);
            if(r.special){
                float partitionX=x+sign*(-r.w*.5f+1.12f),doorZ=r.doorZ;
                WallGaps(shell,C,C+r.d,partitionX,false,lower,new[]{new Gap(doorZ,.78f)});
                Wall(shell,Mathf.Min(partitionX,x+sign*r.w*.5f),Mathf.Max(partitionX,x+sign*r.w*.5f),C,true,lower);
                Door(root,new Vector3(partitionX-x,0,doorZ-r.z),new Vector3(sign,0,0),"PK menyamping melalui LK");
                var furniture=Group("Perabot kamar sementara - dapat diubah",root);Place(bed,furniture,new Vector3(sign*.65f,0,0));Place(wardrobe,furniture,new Vector3(sign*.50f,0,r.d*.5f-.4f));
                Group("LK - akses datar tanpa pintu di mulut lorong",root).localPosition=new Vector3(sign*(-r.w*.5f+.75f),0,-r.d*.5f);
            }else{
                Door(root,new Vector3(r.doorX*sign-x,0,r.dz*r.d*.5f),new Vector3(0,0,r.dz),"PK menghadap lorong");FillRoom(root,r.w,r.d,r.dz>0?180:0);
            }
        }
        if(r.terminal)FillRoom(root,r.d,r.w,dx>0?-90:90);
        BoxPart(root.name+" - plafon",new Vector3(x,H+.055f,r.z),new Vector3(r.w,.11f,r.d),white,ceiling);
    }
    static void Stairs(Transform parent,float minX,float maxX,string name,bool barber)
    {
        var root=Group(name,parent);var g=new Geo();
        float left=minX+1.155f,right=maxX-1.155f,width=1.35f,start=-2f,tread=.30f,rise=.20f;
        const int count=9;float turn=start-count*tread,mid=count*rise,top=mid*2;
        for(int i=0;i<count;i++){
            float y=(i+1)*rise,z=start-(i+.5f)*tread;
            g.Box(new Vector3(right,y*.5f,z),new Vector3(width,y,tread),tile);
            g.Box(new Vector3(right,y+.006f,z+tread*.5f-.035f),new Vector3(width,.018f,.07f),red);
            float yy=mid+(i+1)*rise,zz=turn+(i+.5f)*tread;
            g.Box(new Vector3(left,yy-.08f,zz),new Vector3(width,.16f,tread),tile);
            g.Box(new Vector3(left,yy+.006f,zz-tread*.5f+.035f),new Vector3(width,.018f,.07f),red);
        }
        g.Box(new Vector3((left+right)*.5f,mid-.08f,turn-.60f),new Vector3(right-left+width,.16f,1.2f),tile);
        g.Box(new Vector3(left,top-.08f,-1.65f),new Vector3(width,.16f,.70f),tile);
        Vector3 a=new Vector3(left,mid-.17f,turn),b=new Vector3(left,top-.17f,start);
        g.Box((a+b)*.5f,new Vector3(width,.16f,(b-a).magnitude),tile,Quaternion.LookRotation(b-a));
        Part("N1 menjauhi lorong - bordes - N2 kembali ke lorong",g,root,true);
        Group("Arah N1 dari lorong ke depan bangunan",root).localPosition=new Vector3(right,0,start);
        var rails=new Geo();
        foreach(float edge in new[]{-width*.5f,width*.5f}){
            for(int i=0;i<=count;i++){
                float z=start-i*tread,y=i*rise;
                rails.Rod(new Vector3(right+edge,y,z),new Vector3(right+edge,y+.92f,z),.035f,cream);
                float zz=turn+i*tread,yy=mid+i*rise;
                rails.Rod(new Vector3(left+edge,yy,zz),new Vector3(left+edge,yy+.92f,zz),.035f,cream);
            }
            rails.Rod(new Vector3(right+edge,.92f,start),new Vector3(right+edge,mid+.92f,turn),.055f,cream);
            rails.Rod(new Vector3(left+edge,mid+.92f,turn),new Vector3(left+edge,top+.92f,start),.055f,cream);
        }
        for(int i=0;i<=8;i++){float x=Mathf.Lerp(left-width*.5f,right+width*.5f,i/8f);rails.Rod(new Vector3(x,mid,turn-1.18f),new Vector3(x,mid+.92f,turn-1.18f),.035f,cream);}
        rails.Rod(new Vector3(left-width*.5f,mid+.92f,turn-1.18f),new Vector3(right+width*.5f,mid+.92f,turn-1.18f),.055f,cream);
        Part("Pagar dan pegangan tangga krem",rails,root);
        BoxPart("Jalan lantai biasa di depan tangga",new Vector3((minX+maxX)*.5f,-.07f,(LobbyFront-C)*.5f),new Vector3(maxX-minX,.14f,-C-LobbyFront),tile,root,true);
        var wall=new Geo();
        Wall(wall,LobbyFront,-C,barber?maxX:minX,false,cream);
        Wall(wall,minX,maxX,LobbyFront,true,cream);
        Part("Dinding luar tangga",wall,root,true);
        if(barber){
            var salon=Group("Tempat cukur rambut di bawah tangga",root);var c=new Geo();
            c.Cylinder(new Vector3(0,.10f,0),.32f,.12f,dark);c.Cylinder(new Vector3(0,.32f,0),.065f,.40f,metal);
            c.RoundedPanel(new Vector3(0,.59f,0),new Vector3(.60f,.17f,.60f),.12f,dark);
            c.RoundedPanel(new Vector3(0,.93f,.26f),new Vector3(.60f,.60f,.13f),.10f,dark,true);
            foreach(float side in new[]{-1f,1f})c.Bevel(new Vector3(side*.35f,.79f,0),new Vector3(.07f,.06f,.54f),dark);
            var chair=Part("Kursi cukur putar",c,salon);chair.transform.localPosition=new Vector3(left,0,-2.85f);chair.transform.localRotation=Quaternion.Euler(0,90,0);
            var station=new Geo();station.Box(new Vector3(minX+.10f,1.15f,-2.85f),new Vector3(.07f,2.3f,1.15f),cream);
            station.Bevel(new Vector3(minX+.32f,.80f,-2.85f),new Vector3(.40f,.045f,1.02f),wood);
            station.Box(new Vector3(minX+.15f,1.51f,-2.85f),new Vector3(.035f,1.10f,.76f),mirror);
            Part("Sekat meja dan cermin cukur",station,salon);
            var store=Group("Gudang kecil di bawah tangga",root);var s=new Geo();
            for(int level=0;level<3;level++)s.Box(new Vector3(left,.30f+level*.35f,-4.05f),new Vector3(.85f,.035f,.48f),wood);
            for(int i=0;i<3;i++)s.Bevel(new Vector3(left,.45f+i*.35f,-4.05f),new Vector3(.55f,.24f,.35f),cream);
            s.Box(new Vector3(left,.68f,-3.55f),new Vector3(1.02f,1.36f,.065f),cream);
            s.Bevel(new Vector3(left,.66f,-3.51f),new Vector3(.65f,1.24f,.035f),wood);
            Part("Rak kotak dan pintu gudang rendah",s,store);
        }
        checks.Add("Tangga "+name+":N1 naik menjauhi lorong,N2 kembali;9+9 anak tangga;bordes1.8m;jalur dasar datar.");
    }
    static void Branch(Transform root,Transform ceilings)
    {
        var g=new Geo();var glassGeo=new Geo();float min=-1.1f,max=1.1f,end=11.15f;
        g.Box(new Vector3(0,-.07f,(C+end)*.5f),new Vector3(2.2f,.14f,end-C),tile);
        // Sekat di kanan TV dihapus sampai batas kamar pertama; dinding kamar tetap ada.
        Wall(g,C,end,max,false,mint);WallGaps(g,3.3f,end,min,false,mint,new[]{new Gap(5.2f,.78f),new Gap(9.2f,.78f)});WallGaps(g,min,max,end,true,mint,new[]{new Gap(0,.85f)});Part("Cabang T - dua PK di kiri",g,root,true);
        Door(root,new Vector3(0,0,end),Vector3.back,"Pintu ujung cabang T - tanda cokelat",false,.85f,0);
        for(int i=0;i<2;i++){
            float z=5.2f+i*4;var room=Group("Kamar cabang T "+(i+1),root);room.localPosition=new Vector3(-3.1f,0,z);
            var shell=new Geo();shell.Box(new Vector3(-3.1f,-.07f,z),new Vector3(4,.14f,3.8f),tile);foreach(float edge in new[]{-1f,1f})Wall(shell,-5.1f,-1.1f,z+edge*1.9f,true,mint);
            Wall(shell,z-1.9f,z-.7f,-5.1f,false,mint);Wall(shell,z+.7f,z+1.9f,-5.1f,false,mint);Window(shell,glassGeo,z,-5.1f,1.4f,false,mint);
            Part("Dinding kamar cabang "+i,shell,root,true);Door(room,new Vector3(2,0,0),Vector3.right,"PK cabang");FillRoom(room,3.8f,4,-90);BoxPart("Plafon kamar cabang "+i,new Vector3(-3.1f,H+.055f,z),new Vector3(4,.11f,3.8f),white,ceilings);
        }
        Part("Kaca cabang T - opsional",glassGeo,root);BoxPart("Plafon cabang",new Vector3(0,H+.055f,(C+end)*.5f),new Vector3(2.2f,.11f,end-C),white,ceilings);
    }
    static Material NameSign(string text)
    {
        string path=Root+"/Textures/Papan "+text+".png";AssetDatabase.ImportAsset(path);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=1024;importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Clamp;importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=1024,format=TextureImporterFormat.ETC2_RGB4});importer.SaveAndReimport();
        return Mat("Papan nama "+text,Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(path));
    }
    static void LobbyRecessAndStairWalls(Transform root,Transform ceilings)
    {
        var group=Group("Dinding penutup tangga dan lekukan lobi",root);
        foreach(float x in new[]{-5.2f,6f}){
            var wall=new Geo();
            // Bukaan kiri berada di lantai datar sebelum anak tangga pertama.
            // Bidang tembok utuh menutup anak tangga dan pagar dari arah lobi.
            WallGaps(wall,LobbyFront,C,x,false,mint,new[]{x<0?new Gap(-.7f,1.8f):new Gap(-.75f,4.7f)},2.5f);
            Wall(wall,LobbyFront,-C,x,false,mint,H,4.7f);
            Part(x<0?"Tembok tangga kiri - bukaan akses tetap tersedia":"Tembok tangga kanan - bukaan akses tetap tersedia",wall,group,true);
        }
        // Ceruk berada di luar pita sirkulasi utama; seluruh lantai tetap pada Y=0.
        var recess=new Geo();
        recess.Box(new Vector3(-3.15f,-.07f,(C+3.3f)*.5f),new Vector3(4.1f,.14f,3.3f-C),tile);
        Wall(recess,C,3.3f,-5.2f,false,mint);
        // Dinding depan kamar cabang sudah menjadi bidang belakang TV.
        // Sambungkan hanya celah 10cm di ujungnya; jangan tumpuk sekat kedua di depan.
        Wall(recess,-5.2f,-5.1f,3.3f,true,mint);
        Part("Lekukan bertingkat - sudut pohon dan bidang TV",recess,group,true);
        BoxPart("Plafon ceruk TV rata",new Vector3(-3.15f,H+.055f,(C+3.3f)*.5f),new Vector3(4.1f,.11f,3.3f-C),white,ceilings);
    }
    static void ChristmasTree(Transform parent)
    {
        var root=Group("Pohon Natal di sudut lekukan sesuai sketsa",parent);root.localPosition=new Vector3(-4.45f,0,2.45f);
        var gold=Mat("Hiasan Natal emas",new Color(.86f,.62f,.16f),null,.5f);var green=Mat("Daun pohon Natal",new Color(.075f,.23f,.10f));var ornament=Mat("Hiasan Natal merah",new Color(.64f,.035f,.045f),null,.4f);
        var g=new Geo();g.Cylinder(new Vector3(0,.11f,0),.59f,.22f,red,20);g.Cylinder(new Vector3(0,.35f,0),.065f,.45f,wood);
        for(int i=0;i<5;i++)g.Cylinder(new Vector3(0,.70f+i*.31f,0),.54f-i*.095f,.78f,green,16,.035f);
        for(int i=0;i<36;i++){
            float y=.50f+i*.044f,r=.57f*(1-y/2.45f),a=i*.75f;var p=new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r);
            g.Cylinder(p,.037f,.07f,i%3==0?ornament:gold,8,.8f);
            if(i>0){float py=y-.044f,pr=.57f*(1-py/2.45f),pa=(i-1)*.75f;g.Rod(new Vector3(Mathf.Cos(pa)*pr,py,Mathf.Sin(pa)*pr),p,.012f,gold);}
        }
        var star=new Vector3[10];for(int i=0;i<10;i++){float a=(90+i*36)*Mathf.Deg2Rad,r=i%2==0?.17f:.075f;star[i]=new Vector3(Mathf.Cos(a)*r,2.37f+Mathf.Sin(a)*r,-.02f);}
        // Triangulasi kipas dengan pusat agar bintang cekung tidak terisi salah.
        for(int i=0;i<10;i++){g.Face(new[]{new Vector3(0,2.37f,-.02f),star[i],star[(i+1)%10]},Vector3.back,gold);g.Face(new[]{new Vector3(0,2.37f,-.02f),star[i],star[(i+1)%10]},Vector3.forward,gold);}
        var tree=Part("Pohon hiasan dan alas merah",g,root);var c=tree.AddComponent<BoxCollider>();c.center=new Vector3(0,.9f,0);c.size=new Vector3(1.10f,1.8f,1.10f);
        var curtainGeo=new Geo();
        for(int i=0;i<16;i++)curtainGeo.Box(new Vector3(-.57f+i*.075f,1.50f,.74f+Mathf.Sin(i*1.2f)*.025f),new Vector3(.075f,2.95f,.025f),curtain);
        curtainGeo.Box(new Vector3(0,3.03f,.72f),new Vector3(1.28f,.20f,.09f),cream);Part("Tirai latar sudut pohon Natal",curtainGeo,root);
    }
    static void Lobby(Transform root,Transform ceilings)
    {
        float middle=(LobbyFront-C)*.5f,depth=-C-LobbyFront;
        var lobby=Group("Lobi - denah pengguna",root);var shell=new Geo();
        BoxPart("Lantai lobi",new Vector3(.4f,-.07f,middle),new Vector3(11.2f,.14f,depth),tile,lobby,true);
        WallGaps(shell,-5.2f,-2.4f,LobbyFront,true,mint,new[]{new Gap(-3.45f,1.20f)});
        WallGaps(shell,2.4f,6,LobbyFront,true,mint,new[]{new Gap(4.19f,1.20f)});
        var panes=new Geo();Window(shell,panes,0,LobbyFront,4.8f,true,mint,.85f,2.8f);
        Part("Fasad muka lobi",shell,lobby,true);Part("Kaca jendela lobi - opsional",panes,lobby);
        Door(lobby,new Vector3(-3.45f,0,LobbyFront),Vector3.back,"Pintu masuk tidak dapat diakses",true,1.2f);
        Door(lobby,new Vector3(4.19f,0,LobbyFront),Vector3.back,"Pintu masuk utama",false,1.2f);
        var decor=Group("Perabot lobi dari video",lobby);
        var receptionSign=Group("Papan nama di belakang meja dan kursi resepsionis",decor);receptionSign.localPosition=new Vector3(2.52f,0,C-.065f);receptionSign.localRotation=Quaternion.Euler(0,180,0);
        var nameBoard=new Geo();nameBoard.Bevel(new Vector3(0,2.05f,0),new Vector3(2.12f,.92f,.07f),wood);Part("Bingkai papan nama",nameBoard,receptionSign);
        var title=new Geo();title.Poster(new Vector3(0,2.05f,.04f),1.92f,.72f,crystalSign);Part("Tulisan nama asrama",title,receptionSign);
        // Sandaran dekat jendela; dudukan menghadap meja, bukan menghadap dinding.
        for(int i=0;i<3;i++)Place(bench,decor,new Vector3(-2.2f+i*2.2f,0,-9.0f),180,"Bangku bantalan menghadap meja "+(i+1));
        Place(bench,decor,new Vector3(0,0,-5.25f),0,"Bangku di belakang meja panjang");
        Place(woodChair,decor,new Vector3(3.3f,0,-7.15f),90,"Kursi ujung area duduk");
        var table=new Geo();
        foreach(float x in new[]{-1.1f,1.1f}){
            table.Bevel(new Vector3(x,.57f,-7.2f),new Vector3(2.1f,.065f,.72f),wood);
            foreach(float dx in new[]{-.88f,.88f})foreach(float z in new[]{-7.45f,-6.95f})table.Box(new Vector3(x+dx,.27f,z),new Vector3(.07f,.54f,.07f),wood);
        }
        table.Bevel(new Vector3(2.52f,.80f,.05f),new Vector3(1.80f,.07f,.74f),wood);
        foreach(float x in new[]{1.75f,3.29f})foreach(float z in new[]{-.25f,.35f})table.Box(new Vector3(x,.39f,z),new Vector3(.07f,.78f,.07f),wood);
        table.Bevel(new Vector3(2.52f,.43f,-.27f),new Vector3(1.65f,.62f,.045f),wood);
        Part("Meja panjang dan meja dekat simpang",table,decor,true);
        Place(woodChair,decor,new Vector3(2.52f,0,.89f),0,"Kursi di belakang meja simpang");
        var reception=new Geo();
        reception.Box(new Vector3(-4.25f,.52f,-3.1f),new Vector3(.65f,1.04f,1.6f),cream);
        reception.Box(new Vector3(-3.91f,.46f,-3.1f),new Vector3(.02f,.72f,1.46f),dark);
        reception.Bevel(new Vector3(-4.25f,1.08f,-3.1f),new Vector3(.80f,.07f,1.7f),white);
        Part("Meja layanan panel krem gelap",reception,decor,true);
        Place(plant,decor,new Vector3(-4.65f,0,-9.05f),0,"Vas sudut jauh dari jalur lorong");Place(plant,decor,new Vector3(5.25f,0,-8.45f));
        var curtains=new Geo();
        foreach(float side in new[]{-1f,1f})for(int i=0;i<10;i++){float x=side*(2.12f+i*.055f);curtains.Box(new Vector3(x,1.73f,LobbyFront+.15f+Mathf.Sin(i*1.4f)*.045f),new Vector3(.055f,2.55f,.025f),curtain);}
        curtains.Box(new Vector3(0,3.03f,LobbyFront+.14f),new Vector3(5.35f,.24f,.08f),cream);
        Part("Tirai lobi sesuai nuansa video",curtains,decor);
        var tv=Group("Televisi LED besar di samping pohon Natal",decor);tv.localPosition=new Vector3(-2.4f,1.95f,3.18f);
        var body=new Geo();body.Bevel(new Vector3(0,0,0),new Vector3(1.75f,.99f,.055f),dark);
        body.RoundedPanel(new Vector3(0,0,-.036f),new Vector3(1.65f,.91f,.014f),.015f,Mat("Layar LED besar",new Color(.035f,.055f,.075f),null,.48f),true);
        body.Box(new Vector3(0,0,.065f),new Vector3(.43f,.25f,.12f),metal);
        Part("Panel LED besar dan dudukan dinding",body,tv);
        var blueRow=Group("Deretan kursi biru sisi kanan lantai 1",decor);
        for(int i=0;i<6;i++)Place(waitingChair,blueRow,new Vector3(11.50f+i*.66f,0,-C+.39f),180,"Kursi biru "+(i+1));
        Stairs(root,-9.16f,-5.2f,"Tangga kiri N1 N2",false);
        Stairs(root,6.06f,10.02f,"Tangga kanan dan ruang bawah tangga",true);
        LobbyRecessAndStairWalls(root,ceilings);ChristmasTree(decor);
        BoxPart("Plafon lobi",new Vector3(.4f,H+.055f,middle),new Vector3(11.2f,.11f,depth),white,ceilings);
        checks.Add("Lobi11.2x8.2m;lorong utama3.2m;cabangT2.2m;perabot tetap skala asli.");
        checks.Add("TV tinggi,kursi belakang meja,bangku bantalan menghadap meja,6 kursi biru kanan.");
    }
    static void CorridorFrames(Transform parent,Room[] rooms)
    {
        var gallery=Group("Bingkai koridor - menempel bidang utuh di antara pintu",parent);
        for(int wing=0;wing<2;wing++)foreach(float row in new[]{-1f,1f}){
            float sign=wing==0?1:-1;
            var doors=rooms.Where(r=>!r.terminal&&Mathf.Sign(r.z)==row).Select(r=>(r.special?r.x-r.w*.5f+.76f:r.doorX)*sign).OrderBy(x=>Mathf.Abs(x)).ToArray();
            for(int i=0;i<3;i++){
                float x=(doors[i]+doors[i+1])*.5f;
                if(doors.Any(d=>Mathf.Abs(d-x)<.85f))throw new Exception("Bingkai terlalu dekat pintu");
                var frame=Group($"Bingkai {(wing==0?"kiri":"kanan")} {(row>0?"utara":"selatan")} {i+1}",gallery);frame.localPosition=new Vector3(x,1.95f,row*(C-.09f));frame.localRotation=Quaternion.Euler(0,row>0?180:0,0);
                var g=new Geo();g.Bevel(Vector3.zero,new Vector3(.68f,.50f,.045f),wood);g.Box(new Vector3(0,0,.03f),new Vector3(.60f,.42f,.015f),cream);Part("Bingkai rata pada dinding",g,frame);
            }
        }
    }
    public static void Build()
    {
        Directory.CreateDirectory(Work);foreach(var dir in new[]{"Meshes","Materials","Textures","Prefabs","Prefabs/Perabot","Scenes","Referensi"})Directory.CreateDirectory(Root+"/"+dir);
        AssetDatabase.Refresh();checks.Clear();meshNumber=0;
        var tileTex=Texture("Ubin terang 60cm",(x,y)=>x<3||y<3?new Color(.49f,.49f,.44f):new Color(.79f,.79f,.72f)*(1+(Mathf.PerlinNoise(x*.2f,y*.2f)-.5f)*.025f));
        var woodTex=Texture("Serat kayu pintu",(x,y)=>{float n=Mathf.PerlinNoise(x*.065f,y*.003f)*.10f+Mathf.Sin(x*.3f+Mathf.PerlinNoise(x*.01f,y*.002f)*8)*.015f;return new Color(.43f+n,.27f+n*.75f,.13f+n*.4f);});
        white=Mat("Putih dinding",new Color(.90f,.90f,.85f));mint=Mat("Hijau mint video",new Color(.43f,.67f,.56f));lime=Mat("Hijau terang sayap video",new Color(.56f,.71f,.25f));cream=Mat("Krem pagar dan kusen",new Color(.79f,.73f,.51f));wood=Mat("Kayu pintu dan bangku",Color.white,woodTex);tile=Mat("Ubin 60cm",Color.white,tileTex,.28f);tile.mainTextureScale=Vector2.one/.6f;
        dark=Mat("Besi dan panel gelap",new Color(.075f,.085f,.08f));blue=Mat("Kain kursi biru",new Color(.05f,.26f,.46f));red=Mat("Tepi anak tangga merah tua",new Color(.30f,.075f,.07f));cloth=Mat("Kasur kain krem",new Color(.79f,.77f,.65f));leaf=Mat("Daun tanaman",new Color(.19f,.37f,.16f));metal=Mat("Logam gagang",new Color(.50f,.53f,.52f),null,.65f);mirror=Mat("Cermin cukur",new Color(.64f,.76f,.78f),null,.9f);curtain=Mat("Tirai peach lobi",new Color(.80f,.64f,.56f));
        glass=Mat("Kaca ventilasi opsional",new Color(.60f,.77f,.77f,.24f),null,.65f);glass.SetFloat("_Surface",1);glass.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);glass.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);glass.SetFloat("_ZWrite",0);glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.renderQueue=3000;
        crystalSign=NameSign("CRYSTAL");edelSign=NameSign("EDEL");
        var old=EditorSceneManager.GetActiveScene();if(old.isDirty)throw new Exception("Scene aktif memiliki perubahan belum tersimpan; tidak menggantinya.");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);Furniture();
        var building=new GameObject("Crystal - Lantai 1 - denah pengguna");var root=building.transform;var ceilings=Group("Plafon - nonaktifkan untuk melihat denah",root);
        var plan=JsonUtility.FromJson<Layout>(File.ReadAllText(Root+"/Referensi/DenahLantai1Pengguna.json"));if(plan.rooms.Length!=23)throw new Exception("Data denah kiri harus23 ruang.");
        // Lebar ruang bertambah lewat ukuran geometri, bukan meregangkan pintu/perabot.
        foreach(var r in plan.rooms){float side=Mathf.Sign(r.z);r.d=RoomDepth;r.z=side*(r.terminal?RoomDepth*.5f:C+RoomDepth*.5f);if(r.special)r.doorZ+=C-.85f;}
        for(int side=0;side<2;side++){
            bool mirrored=side==1;var wing=Group(mirrored?"Sayap kanan - urutan cermin kiri":"Sayap kiri - urutan denah asli",root);var shell=new Geo();var panes=new Geo();float min=mirrored?5.2f:-60.76f,max=mirrored?60.76f:-5.2f;Material lower=mirrored?lime:mint;
            shell.Box(new Vector3((min+max)*.5f,-.07f,0),new Vector3(max-min,.14f,(C+RoomDepth)*2+.05f),tile);
            var regular=plan.rooms.Where(r=>!r.terminal).ToArray();
            foreach(var r in plan.rooms)RoomModel(r,wing,mirrored,shell,panes,ceilings);
            foreach(float row in new[]{-1f,1f}){
                var gaps=regular.Where(r=>Mathf.Sign(r.z)==row).Select(r=>new Gap((r.special?r.x-r.w*.5f+.76f:r.doorX)*(mirrored?-1:1),r.special?.72f:.78f)).ToArray();
                float start=mirrored?5.2f:-56.80f,end=mirrored?56.80f:-5.2f;
                if(row<0){if(mirrored)start=10.02f;else end=-9.16f;}
                WallGaps(shell,start,end,row*C,true,lower,gaps);
            }
            Part("Bangunan sayap "+side,shell,wing,true);Part("Kaca sayap "+side+" - opsional",panes,wing);
            BoxPart("Plafon lorong "+side,new Vector3((min+max)*.5f,H+.055f,0),new Vector3(max-min,.11f,C*2),white,ceilings);
        }
        var center=new Geo();center.Box(new Vector3(0,-.07f,0),new Vector3(10.4f,.14f,C*2),tile);Wall(center,1.1f,5.2f,C,true,mint);Part("Simpang T - lantai dan dinding",center,root,true);
        Branch(root,ceilings);Lobby(root,ceilings);CorridorFrames(root,plan.rooms);
        var fixtures=new Geo();for(int side=0;side<2;side++)for(int i=0;i<11;i++){float x=(side==0?-1:1)*(7.7f+i*4.7f);fixtures.Box(new Vector3(x,H-.12f,0),new Vector3(.66f,.045f,.12f),cream);fixtures.Box(new Vector3(x,H-.15f,0),new Vector3(.57f,.018f,.06f),white);}
        Part("Lampu lorong - geometri tanpa puluhan lampu realtime",fixtures,root);
        BoxPart("Plafon simpang",new Vector3(0,H+.055f,0),new Vector3(10.4f,.11f,C*2),white,ceilings);
        AssetDatabase.SaveAssets();PrefabUtility.SaveAsPrefabAsset(building,Root+"/Prefabs/Asrama Crystal - Lantai 1.prefab");
        var signRenderer=root.Find("Lobi - denah pengguna/Perabot lobi dari video/Papan nama di belakang meja dan kursi resepsionis/Tulisan nama asrama").GetComponent<MeshRenderer>();
        building.name="Edel - Lantai 1 - denah sama";signRenderer.sharedMaterial=edelSign;PrefabUtility.SaveAsPrefabAsset(building,Root+"/Prefabs/Asrama Edel - Lantai 1.prefab");building.name="Crystal - Lantai 1 - denah pengguna";signRenderer.sharedMaterial=crystalSign;
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.87f,.88f,.85f);RenderSettings.ambientEquatorColor=new Color(.76f,.77f,.70f);RenderSettings.ambientGroundColor=new Color(.54f,.52f,.44f);RenderSettings.fog=false;
        var sun=new GameObject("Cahaya pratinjau").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.15f;sun.transform.rotation=Quaternion.Euler(60,-32,0);sun.shadows=LightShadows.Soft;
        var camera=new GameObject("Kamera pratinjau asrama").AddComponent<Camera>();camera.transform.position=new Vector3(2,1.68f,-4.45f);camera.transform.LookAt(new Vector3(-1,1.40f,.5f));camera.fieldOfView=62;camera.nearClipPlane=.04f;camera.farClipPlane=180;camera.backgroundColor=new Color(.75f,.82f,.84f);camera.clearFlags=CameraClearFlags.SolidColor;
        camera.transform.position=new Vector3(3.9f,1.70f,-9.15f);camera.transform.LookAt(new Vector3(-1,1.4f,-2.5f));
        EditorSceneManager.SaveScene(scene,Root+"/Scenes/Pratinjau Lantai 1.unity");AssetDatabase.SaveAssets();
        checks.Add("48 ruang:23 sayap kiri,23 sayap kanan,2 kamar tambahan pada cabang T. Tidak membuat lantai2.");
        checks.Add("PK mengikuti posisi denah asli, LK memakai akses menyamping,2 kamar ujung per sayap.");checks.Add("Lobi:meja dekat simpang,tempat duduk,pintu utama terbuka,pintu kiri terkunci,tangga serta tempat cukur dan gudang di bawah tangga kanan.");
        checks.Add("Triangles="+building.GetComponentsInChildren<MeshFilter>(true).Sum(m=>m.sharedMesh.triangles.Length/3));
        File.WriteAllLines(Work+"asset-lantai1-checks.txt",checks);
        File.WriteAllText(Root+"/BACA DULU.md","# Asrama Crystal dan Edel — Lantai 1\n\nDua prefab terpisah memakai susunan dasar sama. Sumber urutan ruang adalah denah pengguna (Referensi/DenahLantai1Pengguna.drawio); tampilan mengacu video AsramaCrystal,Edel.mp4. Bangunan ini belum dipasang ke scene game. Lantai2 tidak dibuat; tangga berhenti pada bordes atas untuk kelanjutan nanti.\n\n48 ruang kamar:23 di kiri,23 di kanan,2 pada cabang T. Sayap kanan mencerminkan urutan kiri, bukan menambah kamar pada sisi kanan cabang yang belum ditunjukkan. PK=pintu kamar; LK=lorong kecil dengan pintu menyamping. Dua pintu kamar ujung per sayap.\n\nUkuran meter merupakan perkiraan dari skala denah, belum pengukuran lapangan. Perabot kamar sementara memakai dua tempat tidur tunggal, meja dan lemari, terkelompok terpisah agar mudah diubah. Ruang LK memakai satu tempat tidur agar jalurnya bebas. Detail tempat cukur dan gudang merupakan penyusunan awal sesuai fungsi yang dikonfirmasi pengguna.\n\nPlafon terpisah agar denah dapat dilihat dari atas. Kaca opsional. Perabot memakai prefab dan mesh bersama. Tidak menambahkan puluhan sumber cahaya realtime. Belum diuji performanya di HP karena belum diintegrasikan ke permainan.\n");AssetDatabase.Refresh();
    }
    public static void Render()
    {
        EditorSceneManager.OpenScene(Root+"/Scenes/Pratinjau Lantai 1.unity");var camera=Object.FindFirstObjectByType<Camera>();var building=GameObject.Find("Crystal - Lantai 1 - denah pengguna");
        void Photo(string name,Vector3 from,Vector3 to,int w=1600,int h=1000){camera.transform.position=from;camera.transform.LookAt(to);var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(w,h,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(Work+name+".png",tex.EncodeToPNG());Object.DestroyImmediate(tex);camera.targetTexture=null;RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);}
        camera.fieldOfView=62;Photo("asrama-lobi",new Vector3(3.9f,1.70f,-9.15f),new Vector3(-1,1.40f,-2.5f));
        Photo("asrama-kursi-meja",new Vector3(4.8f,1.68f,-3.65f),new Vector3(-1.5f,1.0f,-7.4f));
        Photo("asrama-tv",new Vector3(-2.0f,1.65f,-1.7f),new Vector3(-2.4f,1.95f,3.18f));
        Photo("asrama-sudut-pohon",new Vector3(-1.6f,1.65f,-1.7f),new Vector3(-3.4f,1.3f,2.1f));
        Photo("asrama-papan-crystal",new Vector3(4.7f,1.7f,-2.5f),new Vector3(2.52f,1.65f,.8f));
        camera.fieldOfView=58;Photo("asrama-lorong-kiri",new Vector3(-6,1.65f,0),new Vector3(-55,1.5f,0));Photo("asrama-lorong-kanan",new Vector3(6,1.65f,0),new Vector3(55,1.5f,0));
        camera.fieldOfView=62;Photo("asrama-cukur-tangga",new Vector3(8.0f,1.5f,-3.4f),new Vector3(6.9f,1.2f,-2.7f));
        Photo("asrama-tangga-kiri",new Vector3(-6.355f,1.65f,-.6f),new Vector3(-6.355f,1.0f,-4.8f));
        Photo("asrama-kursi-biru",new Vector3(10.75f,1.60f,.48f),new Vector3(14.0f,.90f,-1.15f));
        Photo("asrama-pintu-ujung-cabang",new Vector3(.2f,1.6f,7.3f),new Vector3(0,1.15f,11.15f));
        building.transform.Find("Plafon - nonaktifkan untuk melihat denah").gameObject.SetActive(false);camera.orthographic=true;camera.orthographicSize=10;
        var lobby=building.transform.Find("Lobi - denah pengguna");
        foreach(var n in new[]{"Fasad muka lobi","Kaca jendela lobi - opsional","Pintu masuk tidak dapat diakses","Pintu masuk utama","Perabot lobi dari video/Tirai lobi sesuai nuansa video"})lobby.Find(n).gameObject.SetActive(false);
        camera.orthographicSize=23;Photo("asrama-keseluruhan-lantai1",new Vector3(35,70,-90),new Vector3(0,.7f,1),3200,1100);
        camera.orthographicSize=10;
        Photo("asrama-lobi-potongan",new Vector3(18,28,-28),new Vector3(0,.7f,-1.8f));
        foreach(var n in new[]{"Fasad muka lobi","Kaca jendela lobi - opsional","Pintu masuk tidak dapat diakses","Pintu masuk utama","Perabot lobi dari video/Tirai lobi sesuai nuansa video"})lobby.Find(n).gameObject.SetActive(true);
        camera.orthographicSize=6.7f;Photo("asrama-denah-lobi-berlekuk",new Vector3(0,40,-2.7f),new Vector3(0,0,-2.699f));
        camera.orthographicSize=5.5f;Photo("asrama-lorong-kanan-atas",new Vector3(13,30,-.5f),new Vector3(13,0,0));
        camera.orthographicSize=17.8f;Photo("asrama-denah-lantai1",new Vector3(0,100,0),new Vector3(0,0,.001f),2800,800);
        EditorSceneManager.OpenScene("Assets/Scenes/Explorer.unity");
    }
}
