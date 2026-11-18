using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Rekonstruksi proporsional dari video pengguna; satu unit = satu meter.
// Model merupakan aset baru, tidak mengganti kelas GK2 yang sudah dimainkan.
public static class KelasReferensiVideo
{
    public const string Root = "Assets/Ruangan Kelas Referensi";
    public const string Work = "work/narasi-kelas-20261111/";
    const float HalfWidth = 8.4f;
    static readonly CultureInfo CI = CultureInfo.InvariantCulture;
    static Material wall, frame, tile, wood, worn, dark, board, glass, stone, partition;
    static List<string> checks = new List<string>();
    sealed class Geometry
    {
        public readonly List<Vector3> vertices = new List<Vector3>(), normals = new List<Vector3>();
        readonly List<Vector2> uv = new List<Vector2>();
        public readonly List<Material> materials = new List<Material>();
        readonly List<List<int>> indices = new List<List<int>>();
        public void Box(Vector3 center, Vector3 size, Material material, Quaternion rotation = default)
        {
            if (rotation.x==0 && rotation.y==0 && rotation.z==0 && rotation.w==0) rotation = Quaternion.identity;
            Vector3[] corners = {new Vector3(-.5f,-.5f,-.5f),new Vector3(.5f,-.5f,-.5f),new Vector3(.5f,.5f,-.5f),new Vector3(-.5f,.5f,-.5f),new Vector3(-.5f,-.5f,.5f),new Vector3(.5f,-.5f,.5f),new Vector3(.5f,.5f,.5f),new Vector3(-.5f,.5f,.5f)};
            int[][] faces = {new[]{1,0,3,2},new[]{4,5,6,7},new[]{0,4,7,3},new[]{5,1,2,6},new[]{3,7,6,2},new[]{0,1,5,4}};
            Vector3[] directions = {Vector3.back,Vector3.forward,Vector3.left,Vector3.right,Vector3.up,Vector3.down};
            int slot=materials.IndexOf(material);if(slot<0){slot=materials.Count;materials.Add(material);indices.Add(new List<int>());}
            for(int f=0;f<6;f++){
                int offset=vertices.Count;float w=f<2?size.x:f<4?size.z:size.x,h=f<4?size.y:size.z;
                Vector2[] coords={new Vector2(0,0),new Vector2(w,0),new Vector2(w,h),new Vector2(0,h)};
                for(int i=0;i<4;i++){vertices.Add(center+rotation*Vector3.Scale(corners[faces[f][i]],size));normals.Add(rotation*directions[f]);uv.Add(coords[i]);}
                indices[slot].AddRange(new[]{offset,offset+1,offset+2,offset,offset+2,offset+3});
            }
        }
        public void Rod(Vector3 a,Vector3 b,float width,Material material){var delta=b-a;Box((a+b)*.5f,new Vector3(width,width,delta.magnitude),material,Quaternion.LookRotation(delta));}
        public void BevelBox(Vector3 center,Vector3 size,Material material,Quaternion rotation=default)
        {
            if(rotation.x==0&&rotation.y==0&&rotation.z==0&&rotation.w==0)rotation=Quaternion.identity;
            Vector3 h=size*.5f;float bevel=Mathf.Min(.008f,Mathf.Min(h.x,Mathf.Min(h.y,h.z))*.30f);
            int slot=materials.IndexOf(material);if(slot<0){slot=materials.Count;materials.Add(material);indices.Add(new List<int>());}
            void Face(Vector3[] points,Vector3 normal){
                if(Vector3.Dot(Vector3.Cross(points[1]-points[0],points[2]-points[0]),normal)<0)Array.Reverse(points);
                int first=vertices.Count;foreach(var p in points){vertices.Add(center+rotation*p);normals.Add(rotation*normal);uv.Add(Mathf.Abs(normal.y)>.7f?new Vector2(p.x+h.x,p.z+h.z):Mathf.Abs(normal.x)>.7f?new Vector2(p.z+h.z,p.y+h.y):new Vector2(p.x+h.x,p.y+h.y));}
                for(int i=1;i+1<points.Length;i++)indices[slot].AddRange(new[]{first,first+i,first+i+1});
            }
            for(int axis=0;axis<3;axis++)foreach(float sign in new[]{-1f,1f}){
                int u=(axis+1)%3,v=(axis+2)%3;var points=new Vector3[4];int index=0;
                foreach(var corner in new[]{new Vector2(-1,-1),new Vector2(1,-1),new Vector2(1,1),new Vector2(-1,1)}){var p=Vector3.zero;p[axis]=h[axis]*sign;p[u]=(h[u]-bevel)*corner.x;p[v]=(h[v]-bevel)*corner.y;points[index++]=p;}
                var normal=Vector3.zero;normal[axis]=sign;Face(points,normal);
            }
            for(int axis=0;axis<3;axis++)foreach(float su in new[]{-1f,1f})foreach(float sv in new[]{-1f,1f}){
                int u=(axis+1)%3,v=(axis+2)%3;var points=new Vector3[4];
                for(int i=0;i<4;i++){bool firstSide=i<2;var p=Vector3.zero;p[axis]=(i==0||i==3?-1:1)*(h[axis]-bevel);p[u]=su*(h[u]-(firstSide?0:bevel));p[v]=sv*(h[v]-(firstSide?bevel:0));points[i]=p;}
                var normal=Vector3.zero;normal[u]=su;normal[v]=sv;Face(points,normal.normalized);
            }
            foreach(float x in new[]{-1f,1f})foreach(float y in new[]{-1f,1f})foreach(float z in new[]{-1f,1f}){
                var signs=new Vector3(x,y,z);var points=new Vector3[3];for(int i=0;i<3;i++){var p=Vector3.Scale(signs,h-Vector3.one*bevel);p[i]=signs[i]*h[i];points[i]=p;}Face(points,signs.normalized);
            }
        }
        public Mesh Mesh(string name){var m=new Mesh{name=name};if(vertices.Count>65535)m.indexFormat=IndexFormat.UInt32;m.SetVertices(vertices);m.SetNormals(normals);m.SetUVs(0,uv);m.subMeshCount=indices.Count;for(int i=0;i<indices.Count;i++)m.SetTriangles(indices[i],i);m.RecalculateBounds();return m;}
    }
    static float Noise(int x,int y,int seed){uint n=(uint)(x*374761393+y*668265263+seed*1471);n=(n^(n>>13))*1274126177;return (n^(n>>16))/(float)uint.MaxValue;}
    static Texture2D Texture(string name,Func<int,int,Color> color,bool transparent=false)
    {
        string path=Root+"/Textures/"+name+".png";
        if(!File.Exists(path)){
            var t=new Texture2D(512,512,TextureFormat.RGBA32,false);var pixels=new Color[512*512];
            for(int y=0;y<512;y++)for(int x=0;x<512;x++)pixels[y*512+x]=color(x,y);
            t.SetPixels(pixels);t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);
        }
        AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=512;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=4;
        importer.textureCompression=TextureImporterCompression.Compressed;importer.alphaSource=transparent?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;
        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name="Android",overridden=true,maxTextureSize=512,format=transparent?TextureImporterFormat.ETC2_RGBA8:TextureImporterFormat.ETC2_RGB4});importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static Material Mat(string name,Color color,Texture2D texture=null,float smooth=.15f)
    {
        string path=Root+"/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
        mat.SetColor("_BaseColor",color);mat.SetTexture("_BaseMap",texture);mat.SetFloat("_Smoothness",smooth);mat.SetFloat("_Metallic",0);mat.enableInstancing=true;EditorUtility.SetDirty(mat);return mat;
    }
    static Transform Group(string name,Transform parent){var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
    static GameObject Part(string name,Geometry geometry,Transform parent,bool collider=false)
    {
        string file=Root+"/Meshes/"+name+".asset";var mesh=geometry.Mesh(name);var existing=AssetDatabase.LoadAssetAtPath<Mesh>(file);
        if(existing==null)AssetDatabase.CreateAsset(mesh,file);else{EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}
        EditorUtility.SetDirty(mesh);
        var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterials=geometry.materials.ToArray();
        GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic);
        if(collider)go.AddComponent<MeshCollider>().sharedMesh=mesh;return go;
    }
    static Geometry Chair()
    {
        var g=new Geometry();
        // Kursi kayu dengan dudukan bilah, sandaran terbuka, dan meja lengan kanan.
        for(int i=0;i<5;i++)g.BevelBox(new Vector3(0,.445f,-.22f+i*.105f),new Vector3(.49f,.035f,.078f),wood);
        foreach(float x in new[]{-.215f,.215f}){
            g.Box(new Vector3(x,.215f,.18f),new Vector3(.055f,.43f,.055f),wood,Quaternion.Euler(-5,0,x>0?-3:3));
            g.Box(new Vector3(x,.475f,-.24f),new Vector3(.055f,.95f,.055f),wood,Quaternion.Euler(-6,0,0));
            g.Box(new Vector3(x,.37f,-.015f),new Vector3(.045f,.07f,.52f),wood);
            g.Box(new Vector3(x,.18f,-.015f),new Vector3(.035f,.035f,.48f),wood);
        }
        for(int i=0;i<3;i++)g.BevelBox(new Vector3(0,.65f+i*.10f,-.27f-i*.012f),new Vector3(.49f,.072f,.035f),wood,Quaternion.Euler(-6,0,0));
        g.Box(new Vector3(0,.28f,-.23f),new Vector3(.44f,.035f,.035f),wood);
        g.BevelBox(new Vector3(.31f,.72f,.035f),new Vector3(.36f,.035f,.53f),worn);
        g.Box(new Vector3(.25f,.58f,.245f),new Vector3(.045f,.27f,.045f),wood);
        g.Rod(new Vector3(.23f,.43f,-.20f),new Vector3(.43f,.70f,-.17f),.035f,wood);
        g.Box(new Vector3(-.235f,.65f,-.035f),new Vector3(.06f,.045f,.43f),wood);
        g.Box(new Vector3(-.235f,.54f,.15f),new Vector3(.04f,.22f,.04f),wood);
        foreach(float x in new[]{-.215f,.215f})foreach(float y in new[]{.66f,.86f})g.Box(new Vector3(x,y,-.302f),new Vector3(.012f,.012f,.006f),dark);
        return g;
    }
    static void Window(Geometry frames,Geometry panes,Geometry grilles,float side,float z,float width)
    {
        float x=side*HalfWidth, bottom=1.05f,top=3.10f;
        foreach(float y in new[]{bottom,top})frames.Box(new Vector3(x,y,z),new Vector3(.16f,.07f,width+.10f),frame);
        foreach(float edge in new[]{-width*.5f,width*.5f})frames.Box(new Vector3(x,(bottom+top)*.5f,z+edge),new Vector3(.16f,top-bottom,.065f),frame);
        int columns=4;
        for(int c=0;c<columns;c++){
            float zz=z-width*.5f+(c+.5f)*width/columns;
            frames.Box(new Vector3(x,2.07f,zz+width/columns*.5f),new Vector3(.12f,2.08f,.035f),frame);
            for(int slat=0;slat<15;slat++)panes.Box(new Vector3(x,1.13f+slat*.132f,zz),new Vector3(.018f,.127f,width/columns-.045f),glass,Quaternion.Euler(0,0,side*23));
            var center=new Vector3(x+side*.105f,2.04f,zz);
            Vector2[] pattern={new Vector2(0,.70f),new Vector2(.13f,.40f),new Vector2(.13f,-.37f),new Vector2(0,-.67f),new Vector2(-.13f,-.37f),new Vector2(-.13f,.40f)};
            for(int n=0;n<pattern.Length;n++){var a=pattern[n];var b=pattern[(n+1)%pattern.Length];grilles.Rod(center+new Vector3(0,a.y,a.x),center+new Vector3(0,b.y,b.x),.014f,frame);}
            grilles.Rod(center+new Vector3(0,.99f,0),center+new Vector3(0,.70f,0),.014f,frame);grilles.Rod(center+new Vector3(0,-.67f,0),center+new Vector3(0,-.99f,0),.014f,frame);
        }
    }
    static void Door(Transform parent,float z,string key)
    {
        var hinge=Group("Engsel pintu "+key+" - dapat diputar",parent);hinge.localPosition=new Vector3(HalfWidth,0,z-.53f);hinge.localRotation=Quaternion.Euler(0,58,0);
        var leaf=new Geometry();leaf.Box(new Vector3(0,1.13f,.52f),new Vector3(.055f,2.26f,1.04f),wood);
        for(int row=0;row<3;row++)foreach(float column in new[]{.27f,.77f})leaf.Box(new Vector3(-.036f,.40f+row*.67f,column),new Vector3(.018f,.53f,.40f),worn);
        leaf.Box(new Vector3(-.046f,1.55f,.52f),new Vector3(.006f,.25f,.32f),board);
        leaf.Box(new Vector3(-.09f,1.02f,.94f),new Vector3(.10f,.045f,.035f),dark);
        var go=Part("Daun pintu "+key,leaf,hinge);var c=go.AddComponent<BoxCollider>();c.center=new Vector3(0,1.13f,.52f);c.size=new Vector3(.065f,2.26f,1.04f);
    }
    static void Export(GameObject root)
    {
        using(var file=new StreamWriter(Root+"/Ruangan Kelas Referensi.obj")){
            file.WriteLine("# Ruang kelas dari video referensi pengguna. Satuan meter; Y ke atas.");file.WriteLine("mtllib Ruangan Kelas Referensi.mtl");int offset=1;
            foreach(var mf in root.GetComponentsInChildren<MeshFilter>(true)){
                var mesh=mf.sharedMesh;var matrix=root.transform.worldToLocalMatrix*mf.transform.localToWorldMatrix;file.WriteLine("o "+mf.name.Replace(' ','_'));
                foreach(var v in mesh.vertices){var p=matrix.MultiplyPoint3x4(v);file.WriteLine(FormattableString.Invariant($"v {p.x} {p.y} {-p.z}"));}
                foreach(var uv in mesh.uv)file.WriteLine(FormattableString.Invariant($"vt {uv.x} {uv.y}"));
                foreach(var n in mesh.normals){var normal=matrix.inverse.transpose.MultiplyVector(n).normalized;file.WriteLine(FormattableString.Invariant($"vn {normal.x} {normal.y} {-normal.z}"));}
                var mats=mf.GetComponent<MeshRenderer>().sharedMaterials;
                for(int sub=0;sub<mesh.subMeshCount;sub++){file.WriteLine("usemtl "+mats[sub].name.Replace(' ','_'));var tri=mesh.GetTriangles(sub);for(int i=0;i<tri.Length;i+=3){int a=offset+tri[i],b=offset+tri[i+2],c=offset+tri[i+1];file.WriteLine($"f {a}/{a}/{a} {b}/{b}/{b} {c}/{c}/{c}");}}
                offset+=mesh.vertexCount;
            }
        }
        using(var file=new StreamWriter(Root+"/Ruangan Kelas Referensi.mtl"))foreach(var mat in root.GetComponentsInChildren<MeshRenderer>(true).SelectMany(r=>r.sharedMaterials).Distinct()){
            var c=mat.GetColor("_BaseColor");file.WriteLine("newmtl "+mat.name.Replace(' ','_'));file.WriteLine(FormattableString.Invariant($"Kd {c.r} {c.g} {c.b}"));file.WriteLine("d "+c.a.ToString(CI));file.WriteLine("illum 2");if(mat.mainTexture!=null)file.WriteLine("map_Kd Textures/"+Path.GetFileName(AssetDatabase.GetAssetPath(mat.mainTexture)));file.WriteLine();
        }
    }
    public static void Build()
    {
        checks.Clear();
        Directory.CreateDirectory(Work);foreach(var directory in new[]{"Textures","Materials","Meshes","Prefabs","Scenes"})Directory.CreateDirectory(Root+"/"+directory);AssetDatabase.Refresh();
        var grain=Texture("Kayu kursi halus",(x,y)=>{float grain=Mathf.PerlinNoise(x*.065f,y*.003f)*.045f+Mathf.Sin(x*.39f+Mathf.PerlinNoise(x*.007f,y*.005f)*7)*.012f;return new Color(.36f+grain,.18f+grain*.72f,.085f+grain*.40f);});
        var scratched=Texture("Kayu meja aus halus",(x,y)=>{float grain=(Mathf.PerlinNoise(x*.055f,y*.002f)-.5f)*.09f+Mathf.Sin(x*.35f+Mathf.PerlinNoise(x*.01f,y*.003f)*4)*.015f;float nick=Noise(x,y/60,41)>.997f?.10f:0;return new Color(.48f+grain+nick,.29f+grain*.7f+nick,.14f+grain*.4f+nick*.7f);});
        var tiles=Texture("Ubin abu 60cm",(x,y)=>{float noise=(Noise(x,y,15)-.5f)*.018f;bool joint=x<3||y<3;return joint?new Color(.36f,.37f,.36f):new Color(.70f+noise,.71f+noise,.695f+noise);});
        var plaster=Texture("Cat putih hangat",(x,y)=>{float n=(Noise(x,y,5)-.5f)*.012f;return new Color(.88f+n,.875f+n,.84f+n);});
        var stones=Texture("Batu alam eksterior",(x,y)=>{int xx=x/83,yy=y/109;float n=Noise(xx,yy,61);bool joint=x%83<4||y%109<4;return joint?new Color(.73f,.72f,.66f):new Color(.32f+n*.18f,.29f+n*.16f,.22f+n*.14f);});
        var folds=Texture("Panel lipat krem",(x,y)=>{float dirt=(Noise(x/7,y/20,17)-.5f)*.075f;float seam=x<8?-.18f:0;return new Color(.77f+dirt+seam,.75f+dirt+seam,.65f+dirt+seam);});
        wall=Mat("Cat dinding",Color.white,plaster);frame=Mat("Bingkai putih",new Color(.88f,.90f,.88f));tile=Mat("Ubin lantai",Color.white,tiles,.27f);tile.mainTextureScale=Vector2.one/ .6f;
        wood=Mat("Kayu cokelat",Color.white,grain,.2f);worn=Mat("Kayu permukaan aus",Color.white,scratched,.16f);dark=Mat("Besi gelap",new Color(.055f,.065f,.07f));board=Mat("Papan tulis putih",new Color(.94f,.95f,.92f),null,.38f);stone=Mat("Batu alam",Color.white,stones);partition=Mat("Sekat lipat",Color.white,folds);partition.mainTextureScale=new Vector2(2.5f,.8f);
        glass=Mat("Kaca jalusi",new Color(.68f,.82f,.86f,.20f),null,.68f);glass.SetFloat("_Surface",1);glass.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);glass.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);glass.SetFloat("_ZWrite",0);glass.SetFloat("_Cull",0);glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.SetOverrideTag("RenderType","Transparent");glass.renderQueue=3000;
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var root=new GameObject("Ruangan Kelas Referensi - 100 kursi dan proyektor");
        var shell=Group("Bangunan",root.transform);
        var floor=new Geometry();floor.Box(new Vector3(0,-.075f,0),new Vector3(HalfWidth*2+.2f,.15f,11),tile);Part("Lantai ubin",floor,shell,true);
        var front=new Geometry();front.Box(new Vector3(0,1.8f,5.45f),new Vector3(HalfWidth*2+.2f,3.6f,.16f),wall);Part("Dinding papan tulis",front,shell,true);
        var back=new Geometry();back.Box(new Vector3(0,1.8f,-5.45f),new Vector3(HalfWidth*2+.2f,3.6f,.16f),partition);for(int i=0;i<43;i++)back.Box(new Vector3(-HalfWidth+i*.40f,1.8f,-5.35f),new Vector3(.018f,3.42f,.02f),frame);Part("Sekat lipat belakang",back,shell,true);
        var windows=new Geometry();var panes=new Geometry();var grilles=new Geometry();
        foreach(float side in new[]{-1f,1f}){
            var sides=new Geometry();
            // Kusen dan tembok menyisakan bukaan jendela; dua pintu di sisi kanan.
            float lowMin=side>0?-3.85f:-5.45f,lowMax=side>0?3.85f:5.45f;
            sides.Box(new Vector3(side*HalfWidth,.50f,(lowMin+lowMax)*.5f),new Vector3(.16f,1f,lowMax-lowMin),wall);
            sides.Box(new Vector3(side*HalfWidth,3.37f,0),new Vector3(.16f,.46f,10.9f),wall);
            var centers=side>0?new[]{-2.55f,0,2.55f}:new[]{-3.65f,-1.22f,1.22f,3.65f};float width=side>0?2.30f:2.18f;
            foreach(float z in centers){Window(windows,panes,grilles,side,z,width);foreach(float sign in new[]{-1f,1f})sides.Box(new Vector3(side*HalfWidth,2.08f,z+sign*(width*.5f+.07f)),new Vector3(.18f,2.14f,.14f),wall);}
            if(side>0){foreach(float z in new[]{-4.65f,4.65f}){
                sides.Box(new Vector3(HalfWidth,2.31f,z),new Vector3(.14f,.07f,1.23f),frame);
                foreach(float edge in new[]{-.61f,.61f})sides.Box(new Vector3(HalfWidth,2.72f,z+edge),new Vector3(.14f,.83f,.05f),frame);
                for(int i=0;i<5;i++)sides.Box(new Vector3(HalfWidth-.1f,2.42f+i*.13f,z),new Vector3(.035f,.055f,1.05f),frame);
                Door(shell,z,z<0?"belakang":"depan");
            }foreach(float z in new[]{-5.31f,-3.99f,3.99f,5.31f})sides.Box(new Vector3(HalfWidth,1.57f,z),new Vector3(.18f,3.14f,.28f),wall);
            }else foreach(float z in new[]{-5.12f,5.12f})sides.Box(new Vector3(-HalfWidth,2.07f,z),new Vector3(.18f,2.14f,.65f),wall);
            var lower=new Geometry();lower.Box(new Vector3(side*(HalfWidth+.1f),.49f,(lowMin+lowMax)*.5f),new Vector3(.035f,.98f,lowMax-lowMin),stone);Part(side<0?"Batu alam kiri":"Batu alam kanan",lower,shell);
            Part(side<0?"Dinding berjendela kiri":"Dinding berjendela kanan",sides,shell,true);
        }
        Part("Kusen jalusi",windows,shell);Part("Kaca jalusi - opsional",panes,shell);Part("Terali putih geometris",grilles,shell);
        var ceiling=new Geometry();ceiling.Box(new Vector3(0,3.66f,0),new Vector3(HalfWidth*2+.25f,.12f,11),wall);foreach(float z in new[]{-3.8f,0,3.8f})ceiling.Box(new Vector3(0,3.48f,z),new Vector3(HalfWidth*2+.1f,.30f,.18f),frame);ceiling.Box(new Vector3(0,3.48f,0),new Vector3(.20f,.30f,10.8f),frame);Part("Plafon dan balok",ceiling,shell);
        var whiteboard=new Geometry();whiteboard.Box(new Vector3(0,1.86f,5.31f),new Vector3(3.25f,1.45f,.045f),board);foreach(float y in new[]{1.12f,2.60f})whiteboard.Box(new Vector3(0,y,5.29f),new Vector3(3.33f,.036f,.055f),dark);foreach(float x in new[]{-1.65f,1.65f})whiteboard.Box(new Vector3(x,1.86f,5.29f),new Vector3(.035f,1.51f,.055f),dark);whiteboard.Box(new Vector3(0,1.10f,5.25f),new Vector3(3.30f,.025f,.13f),frame);Part("Papan tulis dan bingkai",whiteboard,shell);
        var teacher=new Geometry();teacher.Box(new Vector3(-2.65f,.85f,4.72f),new Vector3(1.25f,.045f,.53f),worn);foreach(float x in new[]{-3.17f,-2.13f})foreach(float z in new[]{4.53f,4.91f})teacher.Box(new Vector3(x,.42f,z),new Vector3(.055f,.84f,.055f),wood);Part("Meja pengajar",teacher,shell,true);
        var projector=new Geometry();projector.Box(new Vector3(0,3.38f,.7f),new Vector3(.14f,.05f,.14f),dark);projector.Box(new Vector3(0,3.10f,.7f),new Vector3(.035f,.55f,.035f),dark);projector.Box(new Vector3(0,2.83f,.7f),new Vector3(.20f,.035f,.16f),dark);projector.Box(new Vector3(0,2.75f,.7f),new Vector3(.36f,.13f,.28f),frame);projector.Box(new Vector3(.095f,2.75f,.848f),new Vector3(.09f,.08f,.018f),dark);projector.Box(new Vector3(.095f,2.75f,.861f),new Vector3(.060f,.050f,.009f),glass);for(int i=0;i<9;i++)projector.Box(new Vector3(-.182f,2.75f,.60f+i*.022f),new Vector3(.005f,.055f,.010f),dark);projector.Rod(new Vector3(.08f,3.50f,.7f),new Vector3(.08f,3.50f,5.3f),.018f,frame);Part("Proyektor gantung dan kabel",projector,shell);
        var seats=Group("100 kursi kayu berlengan",root.transform);var shared=Part("Kursi kayu berlengan",Chair(),seats);PrefabUtility.SaveAsPrefabAsset(shared,Root+"/Prefabs/Kursi Kayu Berlengan.prefab");
        // Denah pengguna: dua blok kiri/kanan dan satu deret melintang di belakang.
        int seatIndex=0;
        void Seat(string name,float x,float z){
            var chair=seatIndex++==0?shared:Object.Instantiate(shared,seats);chair.name=name;
            chair.transform.localPosition=new Vector3(x,0,z);chair.transform.localRotation=Quaternion.identity;
            var collider=chair.GetComponent<BoxCollider>();if(collider==null)collider=chair.AddComponent<BoxCollider>();collider.center=new Vector3(.11f,.48f,0);collider.size=new Vector3(.78f,.96f,.62f);
        }
        foreach(bool left in new[]{true,false})for(int row=0;row<7;row++)for(int col=0;col<6;col++)
            Seat($"Kursi {(left?"kiri":"kanan")} {row+1:00}-{col+1:00}",(left?-6.85f:1.65f)+col*.9f,-2.95f+row*.95f);
        for(int col=0;col<16;col++)Seat($"Kursi belakang {col+1:00}",-6.8f+col*.85f,-4.35f);
        if(seatIndex!=100)throw new Exception("Susunan denah harus memuat 100 kursi.");
        if(root.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3)>90000)throw new Exception("Model terlalu berat untuk target mobile.");
        PrefabUtility.SaveAsPrefabAsset(root,Root+"/Prefabs/Ruangan Kelas Referensi.prefab");Export(root);
        RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.78f,.82f,.86f);RenderSettings.ambientEquatorColor=new Color(.66f,.68f,.65f);RenderSettings.ambientGroundColor=new Color(.44f,.42f,.38f);RenderSettings.fog=false;
        var sun=new GameObject("Cahaya jendela").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.4f;sun.transform.rotation=Quaternion.Euler(48,-64,0);sun.shadows=LightShadows.Soft;
        var camera=new GameObject("Kamera pratinjau kelas").AddComponent<Camera>();camera.transform.position=new Vector3(-.8f,1.70f,-5.12f);camera.transform.LookAt(new Vector3(.2f,1.55f,4.2f));camera.fieldOfView=54;camera.nearClipPlane=.05f;camera.farClipPlane=45;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.72f,.81f,.86f);
        EditorSceneManager.SaveScene(scene,Root+"/Scenes/Pratinjau Ruangan Kelas.unity");AssetDatabase.SaveAssets();
        checks.Add("PASS denah:42 kursi kiri,42 kanan,16 belakang; jarak pusat kursi blok0.90m dan baris0.95m; papan tulis di depan tengah,2 pintu di kanan; skala kursi tetap1.");checks.Add("Triangles="+root.GetComponentsInChildren<MeshFilter>().Sum(m=>m.sharedMesh.triangles.Length/3));checks.Add("Ukuran rancangan:16.8x10.8x3.6m; lebar ruangan dua kali versi sebelumnya. Aset terpisah; tidak mengganti GK2.");
        File.WriteAllLines(Work+"model-checks.txt",checks);File.WriteAllText(Root+"/BACA DULU.md","# Ruangan kelas referensi video dan denah\n\nPrefab lengkap: Prefabs/Ruangan Kelas Referensi.prefab. Kursi terpisah: Prefabs/Kursi Kayu Berlengan.prefab. Pratinjau: Scenes/Pratinjau Ruangan Kelas.unity. Model portabel: Ruangan Kelas Referensi.obj + .mtl + folder Textures.\n\nDimensi rancangan16.8x10.8x3.6m; belum pengukuran lapangan. Lebar dua kali versi sebelumnya. Denah:100 kursi,42 kiri,42 kanan,16 di belakang. Jarak pusat kursi pada blok0.90m; jarak baris0.95m; deret belakang0.85m. Semua menghadap papan tulis di tengah dinding depan. Dua pintu di dinding kanan. Skala kursi tetap, bukan diregangkan. Proyektor gantung tetap tersedia. Engsel pintu dapat diputar. Semua bukaan jendela/pintu memakai geometri terpisah. Kaca dapat dinonaktifkan bila perlu. Tekstur512 dengan mipmap dan kompresi ETC2 Android. Collider sederhana untuk kursi, mesh collider pada bangunan. Aset terpisah; belum mengganti ruang GK2.\n");
        AssetDatabase.Refresh();
    }
    public static void Render()
    {
        var scene=EditorSceneManager.OpenScene(Root+"/Scenes/Pratinjau Ruangan Kelas.unity");var camera=Object.FindFirstObjectByType<Camera>();
        void Photo(string name,Vector3 from,Vector3 to){camera.transform.position=from;camera.transform.LookAt(to);var rt=RenderTexture.GetTemporary(1600,1000,24,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;camera.targetTexture=rt;var request=new UniversalRenderPipeline.SingleCameraRequest{destination=rt};RenderPipeline.SubmitRenderRequest(camera,request);RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();File.WriteAllBytes(Work+name+".png",tex.EncodeToPNG());Object.DestroyImmediate(tex);camera.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);}
        camera.fieldOfView=54;
        Photo("kelas-dari-belakang",new Vector3(-.8f,1.70f,-5.12f),new Vector3(.2f,1.55f,4.2f));
        Photo("kelas-dari-depan",new Vector3(.7f,1.74f,5.05f),new Vector3(-.1f,1.55f,-4.5f));
        foreach(var name in new[]{"Plafon dan balok","Sekat lipat belakang","Dinding berjendela kanan","Batu alam kanan"})Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Single(t=>t.name==name).gameObject.SetActive(false);
        camera.orthographic=true;camera.orthographicSize=9.5f;
        Photo("kelas-potongan",new Vector3(14,14,-17),new Vector3(0,.8f,0));
        foreach(var name in new[]{"Sekat lipat belakang","Dinding berjendela kanan","Batu alam kanan"})Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(t=>t.name==name).gameObject.SetActive(true);
        // Depan di atas dan pintu di kanan, sama seperti orientasi sketsa pengguna.
        camera.orthographicSize=6.2f;
        Photo("kelas-denah",new Vector3(0,20,0),new Vector3(0,0,.001f));
        EditorSceneManager.OpenScene("Assets/Scenes/Explorer.unity");
    }
}
