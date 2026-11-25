using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Menyalin tata dunia dengan memetakan ulang referensi ke scene tujuan.
// Objek cerita, UI, titik awal cerita, dan komponen khusus Chapter1 dipertahankan.
public static class RevisiAksesDanKeselarasan
{
    public const string Work = "work/revisi-turun-dosen-20261111/";
    static T[] All<T>(Scene s) where T : Component => s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
    static Transform Named(Scene s, string name) => All<Transform>(s).Single(t => t.name == name);
    static string Key(Transform t)
    {
        int occurrence = 0;
        if (t.parent != null)
            for (int i = 0; i < t.GetSiblingIndex(); i++) if (t.parent.GetChild(i).name == t.name) occurrence++;
        return (t.parent != null ? Key(t.parent) + "/" : "") + t.name + "#" + occurrence;
    }
    static Dictionary<string, Transform> Tree(Scene s) => All<Transform>(s).ToDictionary(Key);
    static void Backup(Scene s)
    {
        Directory.CreateDirectory(Work + "cadangan");
        string file = Work + "cadangan/" + s.name + ".unity";
        if (!File.Exists(file)) EditorSceneManager.SaveScene(s, file, true);
    }
    static bool Shared(string n) => n.StartsWith("Rute", StringComparison.Ordinal)
        || n.StartsWith("GK1_Ruangan", StringComparison.Ordinal) || n.StartsWith("NPC_Kelas", StringComparison.Ordinal)
        || n.StartsWith("NPC_Edukasi_", StringComparison.Ordinal) || n.StartsWith("NPC DOSEN", StringComparison.Ordinal)
        || n.StartsWith("NPC Pelintas", StringComparison.Ordinal) || n == "NPC_Bo_Keuangan_GH"
        || n == "AksesStudyGarden_CharacterLab" || n == "RevisiCrystalDuaJalur" || n == "Penghuni_Kampus"
        || n == "PertemuanAnjingKucing_Kampus" || n == "Keramaian_Interior_Dining"
        || n == "RUTE KELAS GK2 - aktif" || n == "FisikAreaDepan" || n == "FisikAreaBelakang";

    static Object Remap(Object value, Dictionary<Object, Object> map, Scene source)
    {
        if (value == null) return null;
        if (map.TryGetValue(value, out var replacement)) return replacement;
        Scene owner = value is Component c ? c.gameObject.scene : value is GameObject go ? go.scene : default;
        if (owner == source) throw new InvalidOperationException("Referensi dunia tidak punya pasangan: " + value.name + " (" + value.GetType().Name + ")");
        return value;
    }
    static void References(Component component, Dictionary<Object, Object> map, Scene source)
    {
        var so = new SerializedObject(component); var property = so.GetIterator(); bool changed = false;
        while (property.Next(true))
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference || property.propertyPath == "m_Script") continue;
            var value = property.objectReferenceValue;
            var mapped = Remap(value, map, source);
            if (mapped != value) { property.objectReferenceValue = mapped; changed = true; }
        }
        if (changed) so.ApplyModifiedPropertiesWithoutUndo();
        if (PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
    }
    static void MatchComponents(Transform a, Transform b, Dictionary<Object, Object> map)
    {
        map[a] = b; map[a.gameObject] = b.gameObject;
        foreach (var type in a.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType()).Distinct())
        {
            var src = a.GetComponents(type).Where(c => c.GetType() == type).ToArray();
            var dst = b.GetComponents(type).Where(c => c.GetType() == type).ToList();
            while (dst.Count < src.Length) dst.Add(b.gameObject.AddComponent(type));
            for (int i = 0; i < src.Length; i++) map[src[i]] = dst[i];
        }
    }
    public static void Synchronize(Scene source, Scene target, List<string> report)
    {
        Backup(target);
        var src = Tree(source); var dst = Tree(target);
        var selected = All<Transform>(source).Where(t => Shared(t.root.name)).ToArray();
        var copied = new HashSet<Transform>(); int added = 0;
        foreach (var t in selected)
        {
            string key = Key(t);
            if (!dst.TryGetValue(key, out var other))
            {
                var clone = Object.Instantiate(t.gameObject); clone.name = t.name;
                SceneManager.MoveGameObjectToScene(clone, target);
                if (t.parent != null) clone.transform.SetParent(dst[Key(t.parent)], false);
                foreach (var child in clone.GetComponentsInChildren<Transform>(true)) dst[Key(child)] = child;
                other = clone.transform; added++;
            }
            copied.Add(t);
        }
        var map = new Dictionary<Object, Object>();
        // Map unchanged objects too: controller, UI, audio, quest references remain local.
        foreach (var pair in src)
        {
            if (!dst.TryGetValue(pair.Key, out var other)) continue;
            var a = pair.Value;
            map[a] = other; map[a.gameObject] = other.gameObject;
            if (copied.Contains(a)) MatchComponents(a, other, map);
            else foreach (var c in a.GetComponents<Component>().Where(c => c != null))
            {
                var same = a.GetComponents(c.GetType()).Where(x => x.GetType() == c.GetType()).ToArray();
                var others = other.GetComponents(c.GetType()).Where(x => x.GetType() == c.GetType()).ToArray();
                int i = Array.IndexOf(same, c); if (i < others.Length) map[c] = others[i];
            }
        }
        int components = 0;
        foreach (var t in selected)
        {
            var other = (Transform)map[t];
            other.localPosition = t.localPosition; other.localRotation = t.localRotation; other.localScale = t.localScale;
            other.gameObject.layer = t.gameObject.layer; other.gameObject.tag = t.gameObject.tag;
            // Legacy 2D floor activation is part of the story boot sequence.
            if (!(t.parent == null && t.name.StartsWith("FisikArea", StringComparison.Ordinal))) other.gameObject.SetActive(t.gameObject.activeSelf);
            foreach (var c in t.GetComponents<Component>().Where(c => c != null && !(c is Transform)))
            {
                var dest = (Component)map[c]; EditorUtility.CopySerialized(c, dest); References(dest, map, source); components++;
            }
            if (PrefabUtility.IsPartOfPrefabInstance(other)) PrefabUtility.RecordPrefabInstancePropertyModifications(other);
        }
        EditorSceneManager.MarkSceneDirty(target);
        report.Add(target.name + ": dunia diselaraskan; komponen=" + components + ", subpohon baru=" + added + "; identitas objek cerita lama dipertahankan.");
    }

    struct Vertex
    {
        public Vector3 p, n; public Vector2 uv;
        public static Vertex Lerp(Vertex a, Vertex b, float t) => new Vertex { p = Vector3.Lerp(a.p,b.p,t), n = Vector3.Lerp(a.n,b.n,t).normalized, uv = Vector2.Lerp(a.uv,b.uv,t) };
    }
    static float Axis(Vector3 p, int axis) => axis == 0 ? p.x : axis == 1 ? p.y : p.z;
    static List<Vertex> Clip(List<Vertex> polygon, int axis, float bound, bool positive)
    {
        var result = new List<Vertex>(); if (polygon.Count == 0) return result;
        var prev = polygon[polygon.Count - 1]; float dp = (Axis(prev.p,axis)-bound)*(positive?1:-1);
        foreach (var current in polygon)
        {
            float dc = (Axis(current.p,axis)-bound)*(positive?1:-1);
            if ((dp>=0)!=(dc>=0)) result.Add(Vertex.Lerp(prev,current,dp/(dp-dc)));
            if (dc>=0) result.Add(current);
            prev=current; dp=dc;
        }
        return result;
    }
    // Subtract a rectangular opening, interpolating UV/normals instead of dropping whole faces.
    static Mesh Opening(Mesh original, Bounds hole)
    {
        var p=original.vertices; var n=original.normals; var uv=original.uv;
        var vertices=new List<Vector3>(); var normals=new List<Vector3>(); var coords=new List<Vector2>();
        var triangles=new List<int>[original.subMeshCount];
        for(int sub=0;sub<triangles.Length;sub++)
        {
            var output=triangles[sub]=new List<int>(); var indices=original.GetTriangles(sub);
            for(int i=0;i<indices.Length;i+=3)
            {
                var remainder=new List<Vertex>();
                for(int j=0;j<3;j++){int k=indices[i+j];remainder.Add(new Vertex{p=p[k],n=n[k],uv=uv[k]});}
                var pieces=new List<List<Vertex>>();
                for(int axis=0;axis<3&&remainder.Count>0;axis++)
                {
                    float min=Axis(hole.min,axis), max=Axis(hole.max,axis);
                    pieces.Add(Clip(remainder,axis,min,false)); remainder=Clip(remainder,axis,min,true);
                    pieces.Add(Clip(remainder,axis,max,true)); remainder=Clip(remainder,axis,max,false);
                }
                foreach(var poly in pieces)
                    for(int j=1;j+1<poly.Count;j++)
                    {
                        if(Vector3.Cross(poly[j].p-poly[0].p,poly[j+1].p-poly[0].p).sqrMagnitude<1e-12f)continue;
                        foreach(var v in new[]{poly[0],poly[j],poly[j+1]}){output.Add(vertices.Count);vertices.Add(v.p);normals.Add(v.n);coords.Add(v.uv);}
                    }
            }
        }
        var mesh=new Mesh{name=original.name+" - akses terbuka"};
        if(vertices.Count>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,coords);mesh.subMeshCount=triangles.Length;
        for(int i=0;i<triangles.Length;i++)mesh.SetTriangles(triangles[i],i);
        mesh.RecalculateBounds();return mesh;
    }
    static void ExpandOffice(Scene s,List<string> report)
    {
        const string meshFolder="Assets/Aset/Dunia/Ruang_Dosen/Meshes/AksesBilik";
        Directory.CreateDirectory(meshFolder);
        var route=Named(s,"GK1_RuanganDosen").GetComponent<JalurKampus>();
        var model=route.transform.Find("Ruangan Dosen");
        Vector3 scale=new Vector3(1.45f,1f,1.3f); model.localScale=scale;
        // Raise the room shell, keeping desk/partition height usable for the NPCs.
        foreach(Transform piece in model)
            if(new[]{"02_","03_","04_","05_","09_","10_"}.Any(prefix=>piece.name.StartsWith(prefix,StringComparison.Ordinal)))
            {piece.localScale=new Vector3(1,1.2f,1);PrefabUtility.RecordPrefabInstancePropertyModifications(piece);}
        int openings=0;
        foreach(var filter in model.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.StartsWith("Bilik_",StringComparison.Ordinal)&&!f.name.EndsWith("_Kursi",StringComparison.Ordinal)))
        {
            var original=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Aset/Dunia/Ruang_Dosen/Meshes/"+filter.name+".asset");
            var center=original.bounds.center; Bounds hole;
            if(filter.name.StartsWith("Bilik_Kiri"))hole=new Bounds(new Vector3(-3.13f,1f,center.z+.32f),new Vector3(.6f,2.4f,.76f));
            else if(filter.name.StartsWith("Bilik_Kanan"))hole=new Bounds(new Vector3(3.13f,1f,center.z-.32f),new Vector3(.6f,2.4f,.76f));
            else hole=new Bounds(new Vector3(center.x-.32f,1f,4.67f),new Vector3(.76f,2.4f,.6f));
            string path=meshFolder+"/"+filter.name+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null){mesh=Opening(original,hole);AssetDatabase.CreateAsset(mesh,path);}
            filter.sharedMesh=mesh;
            var collider=filter.GetComponent<MeshCollider>();if(collider!=null)collider.sharedMesh=mesh;
            PrefabUtility.RecordPrefabInstancePropertyModifications(filter);
            if(collider!=null)PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
            openings++;
        }
        // Keep the user's current route edits, scaling positions once from the original plan.
        string flag=Work+"office-expanded.done";
        if(!File.Exists(flag))
        {
            var modelChildren=new HashSet<Transform>(model.GetComponentsInChildren<Transform>(true));
            foreach(var t in route.GetComponentsInChildren<Transform>(true).Where(t=>t!=route.transform&&!modelChildren.Contains(t)))
            {
                // Scale top-level planning objects only; children inherit their parent's translation.
                if(t.parent==route.transform)t.localPosition=Vector3.Scale(t.localPosition,new Vector3(scale.x,1,scale.z));
            }
            foreach(var npc in All<NpcYangBisaDiatur>(s).Where(n=>n.jalurInteraksi==route))
            {
                Vector3 local=route.transform.InverseTransformPoint(npc.transform.position);
                npc.transform.position=route.transform.TransformPoint(Vector3.Scale(local,new Vector3(scale.x,1,scale.z)));
                route.Sampel(route.JarakTerdekat(npc.transform.position),out var sample,out _,out _);
                npc.geserTitikInteraksi=sample-Vector3.up*ArthurJalurKampus.JarakKaki-npc.transform.position;
                npc.jarakInteraksi=1.2f;
                PrefabUtility.RecordPrefabInstancePropertyModifications(npc.transform);PrefabUtility.RecordPrefabInstancePropertyModifications(npc);
            }
            File.WriteAllText(flag,"Ruang dosen 1.45x lebar, 1.2x tinggi, 1.3x panjang. Jangan skala ulang titik dan NPC.");
        }
        foreach(var npc in All<NpcYangBisaDiatur>(s).Where(n=>n.jalurInteraksi==route))
        {
            var local=route.transform.InverseTransformPoint(npc.transform.position);
            var slot=model.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.StartsWith("Bilik_Kiri_",StringComparison.Ordinal)&&!f.name.EndsWith("_Kursi",StringComparison.Ordinal))
                .OrderBy(f=>Mathf.Abs(f.sharedMesh.bounds.center.z*scale.z-local.z)).First();
            local.z=(slot.sharedMesh.bounds.center.z+.32f)*scale.z;npc.transform.position=route.transform.TransformPoint(local);
            route.Sampel(route.JarakTerdekat(npc.transform.position),out var standing,out _,out _);
            npc.geserTitikInteraksi=standing-Vector3.up*ArthurJalurKampus.JarakKaki-npc.transform.position;
            PrefabUtility.RecordPrefabInstancePropertyModifications(npc.transform);PrefabUtility.RecordPrefabInstancePropertyModifications(npc);
        }
        PrefabUtility.RecordPrefabInstancePropertyModifications(model);
        var camera=route.GetComponent<KameraPilihanPemilik>();
        route.Sampel(route.Panjang*.5f,out camera.acuan,out _,out _);
        camera.gunakan=true;camera.ikutiArthur=true;camera.kehalusan=5;camera.sudut=65;
        camera.posisi=route.transform.position+new Vector3(1.9f,2.45f,camera.acuan.z-route.transform.position.z);
        camera.bidikan=route.transform.position+new Vector3(-2.9f,1.15f,camera.acuan.z-route.transform.position.z);
        var exit=Named(s,"GK1_Keluar_RuanganDosen").GetComponent<InteraksiJalurKampus>();
        exit.transform.position=route.titik[0].position;exit.radius=1.3f;exit.tinggiIkonDiLayar=.11f;
        route.radiusColliderKhusus=.22f;JalurKampusInspector.Rebuild(route);
        report.Add("Ruangan dosen diperbesar 45% lebar,20% tinggi,30% panjang; "+openings+" bilik memiliki bukaan hampir1m, mesh collider mengikuti bukaan.");
    }
    [MenuItem("Arthur/Revisi akses/Selaraskan eksplorasi dan Chapter1")]
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Hentikan Play dahulu.");
        Directory.CreateDirectory(Work);var original=SceneManager.GetActiveScene();
        var source=SceneManager.GetSceneByPath("Assets/Scenes/Explorer.unity");
        if(!source.isLoaded)throw new InvalidOperationException("Buka Explorer dahulu.");
        Backup(source);var report=new List<string>();
        var old=SceneManager.GetSceneByPath("Assets/Scenes/CharacterLab.unity");bool wasLoaded=old.isLoaded;
        if(!wasLoaded)old=EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity",OpenSceneMode.Additive);
        if(!All<InteraksiJalurKampus>(source).Any(m=>m.name=="GK2_TurunDariTeras"))
        {
            var template=Named(old,"GK2_TurunDariTeras");var clone=Object.Instantiate(template.gameObject);clone.name=template.name;
            SceneManager.MoveGameObjectToScene(clone,source);clone.transform.SetParent(Named(source,"RuteKampus_CharacterLab"),true);
            var srcTree=Tree(old);var dstTree=Tree(source);var map=new Dictionary<Object,Object>();
            foreach(var pair in srcTree)if(dstTree.TryGetValue(pair.Key,out var dest))
            {map[pair.Value]=dest;map[pair.Value.gameObject]=dest.gameObject;foreach(var c in pair.Value.GetComponents<Component>().Where(c=>c!=null)){var d=dest.GetComponent(c.GetType());if(d!=null)map[c]=d;}}
            foreach(var c in clone.GetComponentsInChildren<Component>(true).Where(c=>c!=null&&!(c is Transform)))References(c,map,old);
        }
        var down=Named(source,"GK2_TurunDariTeras").GetComponent<InteraksiJalurKampus>();
        var terrace=Named(source,"GK2_TerasAtas").GetComponent<JalurKampus>();
        // Ujung garis pemilik masuk ke sisi pilar; gunakan akses lama yang
        // dapat dicapai dengan berjalan di atas pijakan fisik.
        var oldAccess=new Vector3(14.8f,terrace.titik[0].position.y,terrace.titik[0].position.z);
        terrace.Sampel(terrace.JarakTerdekat(oldAccess),out var access,out _,out _);
        down.jalur=terrace;down.transform.position=access;down.radius=1.4f;down.tinggiIkonDiLayar=.11f;down.ikutiKepalaArthur=false;down.gameObject.SetActive(true);down.enabled=true;
        var area=down.GetComponent<AreaInteraksi>();area.daftarOpsi[0].teksOpsi="Turun dari teras GK2";
        area.daftarOpsi[0].titikTujuan=Named(source,"TitikPersimpangan_GK2");
        report.Add("Bubble turun lama dipulihkan pada awal jalur teras; tujuan jalur bawah dan kembali naik dipertahankan.");
        ExpandOffice(source,report);
        foreach(var path in new[]{"Assets/Scenes/CharacterLab.unity","Assets/Scenes/Penataan Kampus - Jalur NPC dan Bubble.unity"})
        {
            var s=SceneManager.GetSceneByPath(path);bool loaded=s.isLoaded;if(!loaded)s=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try{Synchronize(source,s,report);Validate(s);EditorSceneManager.SaveScene(s);}
            finally{if(!loaded)EditorSceneManager.CloseScene(s,true);}
        }
        Validate(source);EditorSceneManager.MarkSceneDirty(source);EditorSceneManager.SaveScene(source);
        if(!wasLoaded)EditorSceneManager.CloseScene(old,true);
        SceneManager.SetActiveScene(original);AssetDatabase.SaveAssets();File.WriteAllLines(Work+"pemasangan.txt",report);
    }
    public static void Validate(Scene s)
    {
        bool Runtime(Transform t) { for(;t!=null;t=t.parent)if(t.CompareTag("EditorOnly"))return false;return true; }
        foreach(var route in All<JalurKampus>(s).Where(r=>Runtime(r.transform)))
            if(route.titik==null||route.titik.Length<2||route.titik.Any(t=>t==null||t.gameObject.scene!=s))throw new Exception("Titik jalur rusak: "+s.name+"/"+route.name);
        foreach(var marker in All<InteraksiJalurKampus>(s))
        {
            if(marker.jalur==null||marker.jalur.gameObject.scene!=s)throw new Exception("Jalur bubble rusak: "+s.name+"/"+marker.name);
            foreach(var option in marker.GetComponent<AreaInteraksi>().daftarOpsi??Array.Empty<OpsiInteraksi>())
                if(option.titikTujuan!=null&&option.titikTujuan.gameObject.scene!=s)throw new Exception("Tujuan lintas scene: "+marker.name);
        }
    }
}
