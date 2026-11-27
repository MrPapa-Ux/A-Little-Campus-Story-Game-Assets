using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Hanya mengatur salinan scene untuk APK. Model sumber, rute, collider dan
// koordinat tidak disunting. Setiap segitiga utuh disalin tepat satu kali.
// Batas renderer yang lebih lokal memungkinkan frustum culling bawaan Unity.
public sealed class RenderWorldGK2Android : IProcessSceneWithReport
{
    public int callbackOrder => 1200;
    sealed class Bagian
    {
        public readonly Dictionary<int,int> indeks=new Dictionary<int,int>();
        public readonly List<int>[] sisi;
        public Bagian(int count){sisi=Enumerable.Range(0,count).Select(_=>new List<int>()).ToArray();}
        public int Tambah(int asli){if(!indeks.TryGetValue(asli,out var n)){n=indeks.Count;indeks.Add(asli,n);}return n;}
    }
    public void OnProcessScene(Scene scene,BuildReport report)
    {
        if(report==null||report.summary.platform!=UnityEditor.BuildTarget.Android||scene.name!="Kawasan_JasmineGK2")return;
        var daftar=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true)).ToArray();
        int diatur=0,hasil=0;long sebelum=0,sesudah=0;
        foreach(var r in daftar)
        {
            bool world=true;
            if(!world||!r.enabled||!r.gameObject.activeInHierarchy)continue;
            var filter=r.GetComponent<MeshFilter>();var asal=filter!=null?filter.sharedMesh:null;
            if(asal==null||asal.subMeshCount==0||r.sharedMaterials.Length<asal.subMeshCount)continue;
            if(r.sharedMaterials.Any(m=>m==null||m.renderQueue>=2501||m.HasProperty("_Surface")&&m.GetFloat("_Surface")>.5f))continue;
            long indexCount=0;bool triangles=true;
            for(int s=0;s<asal.subMeshCount;s++){indexCount+=(long)asal.GetIndexCount(s);triangles&=asal.GetTopology(s)==MeshTopology.Triangles;}
            if(!triangles||indexCount<12288||Mathf.Max(asal.bounds.size.x,asal.bounds.size.z)<48)continue;
            var vertices=asal.vertices;var normals=asal.normals;var uv=asal.uv;var uv2=asal.uv2;
            var colors=asal.colors32;var tangents=asal.tangents;
            var bagian=new Dictionary<Vector3Int,Bagian>();
            for(int s=0;s<asal.subMeshCount;s++)
            {
                var indices=asal.GetIndices(s);
                for(int i=0;i<indices.Length;i+=3)
                {
                    var tengah=(vertices[indices[i]]+vertices[indices[i+1]]+vertices[indices[i+2]])/3f;
                    var key=new Vector3Int(Mathf.FloorToInt(tengah.x/32),Mathf.FloorToInt(tengah.y/16),Mathf.FloorToInt(tengah.z/32));
                    if(!bagian.TryGetValue(key,out var b)){b=new Bagian(asal.subMeshCount);bagian.Add(key,b);}
                    for(int j=0;j<3;j++)b.sisi[s].Add(b.Tambah(indices[i+j]));
                }
            }
            if(bagian.Count<2)continue;
            long copied=0;
            foreach(var pair in bagian)
            {
                var b=pair.Value;var ids=b.indeks.OrderBy(v=>v.Value).Select(v=>v.Key).ToArray();
                var mesh=new Mesh{name=asal.name+" - render lokal "+pair.Key,
                    indexFormat=ids.Length>65535?IndexFormat.UInt32:IndexFormat.UInt16};
                mesh.vertices=ids.Select(i=>vertices[i]).ToArray();
                if(normals.Length==vertices.Length)mesh.normals=ids.Select(i=>normals[i]).ToArray();
                if(uv.Length==vertices.Length)mesh.uv=ids.Select(i=>uv[i]).ToArray();
                if(uv2.Length==vertices.Length)mesh.uv2=ids.Select(i=>uv2[i]).ToArray();
                if(colors.Length==vertices.Length)mesh.colors32=ids.Select(i=>colors[i]).ToArray();
                if(tangents.Length==vertices.Length)mesh.tangents=ids.Select(i=>tangents[i]).ToArray();
                // Kanal tambahan dipertahankan juga, meskipun shader kampus memakai UV0.
                for(int channel=2;channel<8;channel++)
                {
                    var data=new List<Vector4>();asal.GetUVs(channel,data);
                    if(data.Count==vertices.Length)mesh.SetUVs(channel,ids.Select(i=>data[i]).ToList());
                }
                mesh.subMeshCount=asal.subMeshCount;
                for(int s=0;s<asal.subMeshCount;s++){mesh.SetTriangles(b.sisi[s],s,false);copied+=b.sisi[s].Count;}
                mesh.RecalculateBounds();
                var go=new GameObject("Render lokal "+pair.Key){layer=r.gameObject.layer};
                go.transform.SetParent(r.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var dst=go.AddComponent<MeshRenderer>();dst.sharedMaterials=r.sharedMaterials;
                dst.shadowCastingMode=r.shadowCastingMode;dst.receiveShadows=r.receiveShadows;
                dst.lightProbeUsage=r.lightProbeUsage;dst.reflectionProbeUsage=r.reflectionProbeUsage;
                dst.lightmapIndex=r.lightmapIndex;dst.lightmapScaleOffset=r.lightmapScaleOffset;
                dst.realtimeLightmapIndex=r.realtimeLightmapIndex;dst.realtimeLightmapScaleOffset=r.realtimeLightmapScaleOffset;
                dst.renderingLayerMask=r.renderingLayerMask;dst.sortingLayerID=r.sortingLayerID;dst.sortingOrder=r.sortingOrder;
                dst.motionVectorGenerationMode=r.motionVectorGenerationMode;
                hasil++;
            }
            if(copied!=indexCount)throw new BuildFailedException("Segitiga world tidak cocok: "+r.name);
            sebelum+=indexCount/3;sesudah+=copied/3;diatur++;
            r.enabled=false;filter.sharedMesh=null;
        }
        string log="ARTHUR_GK2_RENDER model="+diatur+" kelompokLokal="+hasil+" segitigaSebelum="+sebelum+" segitigaSesudah="+sesudah;
        Debug.Log(log);Directory.CreateDirectory("../work/gk2-fps-20261126");File.WriteAllText("../work/gk2-fps-20261126/geometri.txt",log);
    }
}
