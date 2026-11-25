using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PasangJasmineDanMod20261120
{
    const string Work="work/mod-jasmine-20261120/";
    const string Stream="Assets/WorldStreaming/Kawasan_JasmineInterior.unity";
    static readonly string[] Scenes={"Assets/Scenes/Penataan Kampus - Jalur NPC dan Bubble.unity","Assets/Scenes/Explorer.unity","Assets/Scenes/CharacterLab.unity"};
    public static void Jadwalkan(){EditorApplication.delayCall+=Pasang;}
    public static void Pasang(){
        var setup=EditorSceneManager.GetSceneManagerSetup();
        try{
            if(EditorApplication.isPlaying)throw new Exception("Edit mode required");
            foreach(var s in setup)if(s.isLoaded&&SceneManager.GetSceneByPath(s.path).isDirty)throw new Exception("Ada adegan belum disimpan: "+s.path);
            var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Aset/Dunia/Model_Kampus/Jasmine20261120/LorongJasmine.obj");
            var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Aset/Dunia/Model_Kampus/Pratinjau20261025/GK1/KertasGK1.mat");
            if(model==null||material==null)throw new Exception("Model atau material belum diimpor");
            File.WriteAllText(Work+"setup-status.txt","Membuat kawasan lorong");
            var stream=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            var world=(GameObject)PrefabUtility.InstantiatePrefab(model,stream);world.name="Lorong Asrama Jasmine";
            world.transform.SetPositionAndRotation(new Vector3(1030,61,1000),Quaternion.Euler(0,180,0));
            foreach(var r in world.GetComponentsInChildren<MeshRenderer>(true)){
                r.sharedMaterials=Enumerable.Repeat(material,r.GetComponent<MeshFilter>().sharedMesh.subMeshCount).ToArray();
                r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
                if(r.GetComponent<MeshCollider>()==null)r.gameObject.AddComponent<MeshCollider>().sharedMesh=r.GetComponent<MeshFilter>().sharedMesh;
            }
            PrefabUtility.SaveAsPrefabAsset(world,"Assets/Aset/Dunia/Model_Kampus/Jasmine20261120/LorongJasmine.prefab");
            EditorSceneManager.SaveScene(stream,Stream);EditorSceneManager.CloseScene(stream,true);
            var reports=new List<string>();
            foreach(var path in Scenes){
                File.WriteAllText(Work+"setup-status.txt","Memasang ke "+path);
                var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                var oldRoot=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="Akses Lorong Jasmine dan Naya");
                if(oldRoot!=null){
                    if(UnityEngine.Object.FindFirstObjectByType<PertukaranTubuhArthur>(FindObjectsInactive.Include)?.npcPerempuan!=null){reports.Add(path+": sudah ada");continue;}
                    UnityEngine.Object.DestroyImmediate(oldRoot);
                }
                var player=UnityEngine.Object.FindFirstObjectByType<ArthurJalurKampus>(FindObjectsInactive.Include);
                var kawasan=UnityEngine.Object.FindFirstObjectByType<KawasanKampus>(FindObjectsInactive.Include);
                if(player==null||kawasan==null)throw new Exception("Player/world tidak ada: "+path);
                var defs=kawasan.kawasan.ToList();defs.RemoveAll(k=>k.id=="JasmineInterior");defs.Add(new KawasanKampus.Kawasan{id="JasmineInterior",scene=Stream});kawasan.kawasan=defs.ToArray();
                var root=new GameObject("Akses Lorong Jasmine dan Naya");
                var route=Route(root.transform,"Jasmine_Interior_Lorong",new[]{new Vector3(1060.5f,3.71533f,996),new Vector3(1060.5f,3.71533f,1016),new Vector3(1060.5f,3.71533f,1039)},-90);
                route.diDalamRuangan=true;route.jarakKamera=6;route.tinggiKamera=1.05f;route.tinggiBidikan=-.25f;route.bidangDekatKamera=.08f;route.ikutiPosisiKameraTanpaJeda=true;
                var entry=Goal(root.transform,"Masuk Lorong Jasmine",route,route.titik[0].position+Vector3.forward*3);
                var back=Goal(root.transform,"Kembali ke Halaman Jasmine",null,new Vector3(31.29f,-.06f,20.87f));
                var allAreas=UnityEngine.Object.FindObjectsByType<AreaInteraksi>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                foreach(var gate in allAreas.Where(a=>a.name=="TriggerMasukKedalamAsramaJasmine1"||a.name=="TriggerMasukKedalamAsramaJasmine2")){
                    var access=gate.GetComponent<AksesAsramaJasmine>()??gate.gameObject.AddComponent<AksesAsramaJasmine>();access.tujuan=entry;
                    gate.tipeInteraksi=TipeInteraksi.Tempat_Teleportasi;gate.skenarioDialog=new[]{Baris("Asrama Jasmine","Lorong asrama putri.",Pembicara.Tidak_Ada)};
                    gate.daftarOpsi=new[]{new OpsiInteraksi{teksOpsi="Masuk ke lorong",titikTujuan=entry,aksesJasmine=access,nyalakanGrupBelakang=false}};
                    gate.tampilkanTombolKeluar=true;
                }
                var exitTemplate=allAreas.First(a=>a.name=="GK2_KembaliKeJasmine");
                var exit=UnityEngine.Object.Instantiate(exitTemplate.gameObject,root.transform);exit.name="Jasmine_KeluarDariLorong";exit.transform.position=route.titik[0].position;
                var areaExit=exit.GetComponent<AreaInteraksi>();areaExit.idTempat="Jasmine_KeluarDariLorong";areaExit.skenarioDialog=new[]{Baris("Asrama Jasmine","Kembali ke halaman?",Pembicara.Tidak_Ada)};
                areaExit.daftarOpsi=new[]{new OpsiInteraksi{teksOpsi="Keluar ke halaman",titikTujuan=back,nyalakanGrupBelakang=true}};
                var inter=exit.GetComponent<InteraksiJalurKampus>();inter.jalur=route;inter.jalurTambahan=Array.Empty<JalurKampus>();inter.radius=2.5f;
                var classRoute=UnityEngine.Object.FindObjectsByType<JalurKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(r=>r.name=="GK2_Kelas2");
                var group=new GameObject("Naya dan tubuh Arthur");group.transform.SetParent(root.transform);group.AddComponent<KelompokNpcManual>().kawasan="JasmineGK2";
                var sumberNpc=default(Scene);
                var template=UnityEngine.Object.FindObjectsByType<NpcYangBisaDiatur>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(n=>n.namaNpc=="Tari");
                if(template==null){sumberNpc=EditorSceneManager.OpenScene(Scenes[0],OpenSceneMode.Additive);template=UnityEngine.Object.FindObjectsByType<NpcYangBisaDiatur>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(n=>n.namaNpc=="Tari");}
                var naya=SalinNpc(template,group.transform,"Naya",new Vector3(42,.7f,-55.8f),classRoute);
                var body=SalinNpc(template,group.transform,"Arthur",new Vector3(40,.7f,-55.8f),classRoute);body.name="Arthur - tubuh menunggu";
                var pp=player.GetComponentsInChildren<SpriteRenderer>(true);
                var dictionary=pp.GroupBy(p=>p.name).ToDictionary(g=>g.Key,g=>g.First());
                var woman=naya.GetComponentsInChildren<SpriteRenderer>(true).GroupBy(p=>p.name).ToDictionary(g=>g.Key,g=>g.First());
                var top=naya.GetComponent<TampilanNpcPerempuan>();if(top!=null){top.warnaPakaian=new Color(.76f,.66f,.91f);top.Terapkan();}
                var topBody=body.GetComponent<TampilanNpcPerempuan>();if(topBody!=null)UnityEngine.Object.DestroyImmediate(topBody);
                foreach(var r in body.GetComponentsInChildren<SpriteRenderer>(true))if(dictionary.TryGetValue(r.name,out var original)){r.sprite=original.sprite;r.color=original.color;r.enabled=original.enabled;}
                var head=body.GetComponent<NpcKampusHidup>().kepala.GetComponent<SpriteRenderer>();
                var face=head.GetComponent<EkspresiWajah2D>()??head.gameObject.AddComponent<EkspresiWajah2D>();face.kepala=head;
                var originalFace=player.GetComponentsInChildren<EkspresiWajah2D>(true).FirstOrDefault(f=>f.kepala!=null&&f.kepala.name=="KepalaArthur");
                if(originalFace!=null){EditorUtility.CopySerialized(originalFace,face);face.kepala=head;}
                body.AddComponent<ArthurMenungguTubuh>().wajah=face;
                var bubbles=body.GetComponent<NPCBubbleTrigger>();if(bubbles!=null&&bubbles.teksSimbolAsli!=null)bubbles.teksSimbolAsli.text="...";
                var swap=player.GetComponent<PertukaranTubuhArthur>()??player.gameObject.AddComponent<PertukaranTubuhArthur>();
                swap.namaPerempuan="Naya";swap.npcPerempuan=naya;swap.tubuhArthur=body;swap.jalurKelas=classRoute;swap.wajahArthur=player.GetComponentsInChildren<EkspresiWajah2D>(true);
                swap.potretPerempuan=naya.GetComponent<AreaInteraksi>().potretDialog;
                var list=new List<PertukaranTubuhArthur.Bagian>();
                foreach(var r in pp){
                    string key=r.name=="Kepala"?"KepalaArthur":r.name=="Rambut"?"Rambut 1":r.name;
                    if(woman.TryGetValue(key,out var wr)&&key!="Bubble Chat(Clone)"&&key!="Emote")list.Add(new PertukaranTubuhArthur.Bagian{pemain=r,perempuan=wr.sprite,warnaPerempuan=wr.color,sembunyikan=!wr.enabled});
                    else if(r.name.StartsWith("Telinga")||r.name.StartsWith("BanArthur"))list.Add(new PertukaranTubuhArthur.Bagian{pemain=r,sembunyikan=true});
                }
                swap.bagian=list.ToArray();
                var na=naya.GetComponent<AreaInteraksi>();na.skenarioDialog=new[]{Baris("Naya","Mau meminjam tubuhku untuk masuk ke lorong Jasmine? Tubuhmu akan menunggu di sini. Kalau mau kembali, temui Arthur di kelas ini.",Pembicara.Kanan_NPC)};
                na.daftarOpsi=new[]{new OpsiInteraksi{teksOpsi="Bertukar tubuh dengan Naya",pertukaranTubuh=swap,nyalakanGrupBelakang=false}};
                var ba=body.GetComponent<AreaInteraksi>();ba.potretDialog=originalFace!=null?originalFace.kepala.sprite:null;
                ba.skenarioDialog=new[]{Baris("Arthur","...",Pembicara.Kanan_NPC)};ba.daftarOpsi=new[]{new OpsiInteraksi{teksOpsi="Kembali ke tubuh Arthur",pertukaranTubuh=swap,kembaliKeArthur=true,nyalakanGrupBelakang=false}};
                body.SetActive(false);
                if(sumberNpc.IsValid())EditorSceneManager.CloseScene(sumberNpc,true);
                kawasan.titikMulaiPengujian=null;
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                reports.Add(path+": jalur="+route.Panjang+", bagian avatar="+swap.bagian.Length+", NPC Naya dan Arthur terpasang");
            }
            var build=EditorBuildSettings.scenes.ToList();if(!build.Any(s=>s.path==Stream))build.Add(new EditorBuildSettingsScene(Stream,true));EditorBuildSettings.scenes=build.ToArray();
            Directory.CreateDirectory("Assets/Resources");
            var config=AssetDatabase.LoadAssetAtPath<PengaturanModeUjiArthur>("Assets/Resources/PengaturanModeUjiArthur.asset");
            if(config==null){config=ScriptableObject.CreateInstance<PengaturanModeUjiArthur>();AssetDatabase.CreateAsset(config,"Assets/Resources/PengaturanModeUjiArthur.asset");}
            config.font=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/Aset/Pihak_Ketiga/TextMesh_Pro/Resources/Fonts & Materials/Caveat-VariableFont_wght SDF.asset");
            config.bingkai=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Aset/Karakter/Potret_Dialog/Bingkai_Potret_Sketsa.png");EditorUtility.SetDirty(config);AssetDatabase.SaveAssets();
            File.WriteAllLines(Work+"setup-results.txt",reports);File.WriteAllText(Work+"setup-status.txt","PASS");
        }catch(Exception e){File.WriteAllText(Work+"setup-status.txt","FAIL "+e);Debug.LogException(e);}
        finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
    }
    static BarisDialog Baris(string name,string text,Pembicara siapa)=>new BarisDialog{namaKarakter=name,teksDialog=text,siapaYangBicara=siapa,teksTombolLanjut="Lanjut"};
    static GameObject SalinNpc(NpcYangBisaDiatur template,Transform parent,string name,Vector3 pos,JalurKampus route){
        var go=UnityEngine.Object.Instantiate(template.gameObject,parent);go.name=name;go.SetActive(true);go.transform.position=pos;
        if(go.transform is RectTransform rt)rt.anchoredPosition3D=rt.localPosition;
        foreach(var c in go.GetComponents<MateriKuliahNpc>())UnityEngine.Object.DestroyImmediate(c);
        var npc=go.GetComponent<NpcYangBisaDiatur>();npc.dialogSederhana=false;npc.namaNpc=name;npc.jalurInteraksi=route;npc.jarakInteraksi=1.8f;npc.geserTitikInteraksi=new Vector3(0,0,2);
        var area=go.GetComponent<AreaInteraksi>();area.idTempat="NPC_"+name+"_Jasmine";area.tipeInteraksi=TipeInteraksi.NPC_Karakter;area.tampilkanTombolKeluar=true;
        var hidup=go.GetComponent<NpcKampusHidup>();hidup.kegiatan=KegiatanNpcKampus.Berdiri;return go;
    }
    static JalurKampus Route(Transform parent,string name,Vector3[] points,float yaw){var go=new GameObject(name);go.transform.SetParent(parent);var r=go.AddComponent<JalurKampus>();r.titik=points.Select((p,i)=>{var t=new GameObject("Titik "+i).transform;t.SetParent(go.transform);t.position=p;return t;}).ToArray();r.arahKamera=points.Select(p=>yaw).ToArray();return r;}
    static Transform Goal(Transform parent,string name,JalurKampus r,Vector3 p){var t=new GameObject(name).transform;t.SetParent(parent);t.position=p;t.gameObject.AddComponent<TujuanJalurKampus>().jalur=r;return t;}
}
