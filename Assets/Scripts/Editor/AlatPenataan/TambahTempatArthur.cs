using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class TambahTempatArthur
{
    public static JalurKampus JalurDipilih(out int index)
    {
        index=-1;var t=Selection.activeTransform;if(t==null)return null;
        var route=t.GetComponent<JalurKampus>()??t.GetComponentInParent<JalurKampus>();
        if(route!=null&&route.titik!=null)index=Array.IndexOf(route.titik,t);
        return route;
    }
    public static Transform Sisipkan(JalurKampus route,int index)
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||route==null||route.titik==null||route.titik.Length<2)return null;
        int count=route.titik.Length;index=Mathf.Clamp(index,0,count);
        // Keep existing connected endpoints intact; insert inside the adjoining segment.
        if(index==0&&route.lanjutanAwal!=null)index=1;
        if(index==count&&route.lanjutanAkhir!=null)index=count-1;
        Vector3 position=index==0?route.titik[0].position+(route.titik[0].position-route.titik[1].position).normalized*2:
            index==count?route.titik[count-1].position+(route.titik[count-1].position-route.titik[count-2].position).normalized*2:
            Vector3.Lerp(route.titik[index-1].position,route.titik[index].position,.5f);
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Tambah titik lintasan Arthur");Undo.RegisterCompleteObjectUndo(route,"Tambah titik lintasan Arthur");
        var point=new GameObject(GameObjectUtility.GetUniqueNameForSibling(route.transform,"TitikBaru")).transform;Undo.RegisterCreatedObjectUndo(point.gameObject,"Tambah titik lintasan Arthur");point.SetParent(route.transform);point.position=position;
        var points=route.titik.ToList();points.Insert(index,point);route.titik=points.ToArray();route.arahKamera=SisipNilai(route.arahKamera,index,count);route.putaranBadan=SisipNilai(route.putaranBadan,index,count);
        JalurKampusInspector.Rebuild(route,true);EditorUtility.SetDirty(route);EditorSceneManager.MarkSceneDirty(route.gameObject.scene);Undo.CollapseUndoOperations(group);Selection.activeTransform=point;SceneView.RepaintAll();return point;
    }
    static float[] SisipNilai(float[] values,int index,int count)
    {
        if(values==null||values.Length==0)return values;
        var result=Enumerable.Range(0,count).Select(i=>values[Mathf.Min(i,values.Length-1)]).ToList();
        float a=result[Mathf.Clamp(index-1,0,count-1)],b=result[Mathf.Clamp(index,0,count-1)];result.Insert(index,Mathf.LerpAngle(a,b,.5f));return result.ToArray();
    }
    public static InteraksiJalurKampus Bubble(JalurKampus route,Vector3 position,string label,Transform destination,string arrow="↑")
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||route==null)return null;
        var template=UnityEngine.Object.FindObjectsByType<InteraksiJalurKampus>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(m=>m.jalur!=null&&m.ikon!=null&&m.GetComponent<WigglyBubbleInteraksi>()!=null&&m.GetComponent<PengaturCabangKampus>()==null);
        if(template==null)throw new InvalidOperationException("Template bubble tempat belum tersedia pada scene ini.");
        route.Sampel(route.JarakTerdekat(position),out position,out _,out _);
        Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();Undo.SetCurrentGroupName("Tambah bubble interaksi Arthur");
        var go=UnityEngine.Object.Instantiate(template.gameObject,route.transform.parent);go.name=GameObjectUtility.GetUniqueNameForSibling(route.transform.parent,route.name+"_Bubble");Undo.RegisterCreatedObjectUndo(go,"Tambah bubble interaksi Arthur");go.transform.position=position;
        foreach(var c in go.GetComponents<Collider2D>())UnityEngine.Object.DestroyImmediate(c);
        var m=go.GetComponent<InteraksiJalurKampus>();m.jalur=route;m.jalurTambahan=Array.Empty<JalurKampus>();m.ikutiKepalaArthur=false;m.radius=1.2f;m.tinggiIkonDiLayar=.11f;m.durasiMenciut=.18f;m.ikon.position=position+Vector3.up*.9f;
        var area=go.GetComponent<AreaInteraksi>();area.idTempat=go.name;area.tipeInteraksi=TipeInteraksi.Tempat_Teleportasi;area.skenarioDialog=Array.Empty<BarisDialog>();area.tampilkanTombolKeluar=true;
        area.daftarOpsi=new[]{new OpsiInteraksi{teksOpsi=string.IsNullOrWhiteSpace(label)?"Pergi ke tempat tujuan":label,titikTujuan=destination,nyalakanGrupBelakang=false,pesanBelumTersedia="Tujuan tempat ini belum disambungkan."}};
        var text=m.ikon.GetComponentInChildren<TMPro.TMP_Text>(true);if(text!=null)text.text=arrow;
        for(int i=0;i<10&&Array.IndexOf(go.GetComponents<Component>(),m)>1;i++)if(!UnityEditorInternal.ComponentUtility.MoveComponentUp(m))break;
        EditorSceneManager.MarkSceneDirty(route.gameObject.scene);Undo.CollapseUndoOperations(group);BubbleArthurScene.Pilih(m);return m;
    }
    public static void Panel()
    {
        var route=JalurDipilih(out int index);EditorGUILayout.LabelField("TAMBAH TITIK DAN BUBBLE",EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Klik bulatan hijau di Scene terlebih dahulu. Titik baru muncul di tengah ruas sebelahnya; di ujung bebas, lintasan diperpanjang 2 meter. Geser titik baru sesuai lantai lalu Ctrl+S.",MessageType.Info);
        EditorGUILayout.LabelField("Jalur",route!=null?route.name:"Pilih bulatan hijau atau jalur");
        using(new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode||route==null)){
            using(new EditorGUILayout.HorizontalScope()){
                if(GUILayout.Button("+ Titik sebelum"))Sisipkan(route,index<0?0:index);
                if(GUILayout.Button("+ Titik sesudah"))Sisipkan(route,index<0?route.titik.Length:index+1);
            }
            if(GUILayout.Button("+ Bubble pada titik terpilih")){
                var p=index>=0?route.titik[index].position:route.titik[route.titik.Length-1].position;
                Bubble(route,p,"Pergi ke tempat tujuan",null);
            }
        }
    }
    public static void PanelScene(Transform point)
    {
        if(point==null)return;var route=point.GetComponentInParent<JalurKampus>();if(route==null)return;int index=Array.IndexOf(route.titik,point);if(index<0)return;
        Handles.BeginGUI();GUILayout.BeginArea(new Rect(64,12,290,115),EditorStyles.helpBox);
        GUILayout.Label(route.name+" / "+point.name,EditorStyles.boldLabel);
        using(new GUILayout.HorizontalScope()){
            if(GUILayout.Button("+ Sebelum"))Sisipkan(route,index);
            if(GUILayout.Button("+ Sesudah"))Sisipkan(route,index+1);
        }
        if(GUILayout.Button("+ Bubble di titik ini"))Bubble(route,point.position,"Pergi ke tempat tujuan",null);
        GUILayout.Label("Ctrl+Z: batalkan · Ctrl+S: simpan");GUILayout.EndArea();Handles.EndGUI();
    }
}
