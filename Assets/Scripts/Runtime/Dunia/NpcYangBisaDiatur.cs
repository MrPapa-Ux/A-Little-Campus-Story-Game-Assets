using System;
using UnityEngine;

[RequireComponent(typeof(AreaInteraksi))]
[DefaultExecutionOrder(300)]
[AddComponentMenu("Arthur/NPC yang bisa diatur")]
public sealed class NpcYangBisaDiatur : MonoBehaviour
{
    [Tooltip("Nama yang ditampilkan pada panel percakapan.")]
    public string namaNpc="Mahasiswa";
    [TextArea(2,6)] public string dialog="...";
    [Tooltip("Matikan jika ingin menyusun banyak baris dan emote langsung di komponen Area Interaksi.")]
    public bool dialogSederhana=true;
    [Min(.5f)] public float jarakInteraksi=1.8f;
    [Tooltip("Geser titik Arthur berdiri, dalam arah dunia. Nol berarti dekat posisi NPC. Berguna supaya NPC di rumput terlihat di samping Arthur, bukan saling menutupi.")]
    public Vector3 geserTitikInteraksi;
    [Tooltip("Opsional: NPC hanya menawarkan dialog ketika Arthur berada di jalur ini. Kosong mempertahankan interaksi lama.")]
    public JalurKampus jalurInteraksi;
    static readonly System.Collections.Generic.List<NpcYangBisaDiatur> aktif=new System.Collections.Generic.List<NpcYangBisaDiatur>();
    static int bingkai=-1;static float jarakTerpilih;static NpcYangBisaDiatur terpilih;
    AreaInteraksi area;NPCBubbleTrigger bubble;ArthurJalurKampus arthur;
    void OnEnable(){if(!aktif.Contains(this))aktif.Add(this);bingkai=-1;}
    public static NpcYangBisaDiatur Terdekat(ArthurJalurKampus pemain,out float jarak)
    {
        if(bingkai==Time.frameCount){jarak=jarakTerpilih;return terpilih;}
        bingkai=Time.frameCount;var kaki=pemain.transform.position-Vector3.up*ArthurJalurKampus.JarakKaki;jarak=float.PositiveInfinity;NpcYangBisaDiatur hasil=null;
        foreach(var n in aktif){if(n==null||!n.isActiveAndEnabled||(n.jalurInteraksi!=null&&n.jalurInteraksi!=pemain.JalurAktif))continue;float d=(kaki-n.transform.position-n.geserTitikInteraksi).sqrMagnitude;if(d<n.jarakInteraksi*n.jarakInteraksi&&d<jarak){jarak=d;hasil=n;}}
        terpilih=hasil;jarakTerpilih=jarak;return hasil;
    }
    void Awake(){area=GetComponent<AreaInteraksi>();bubble=GetComponent<NPCBubbleTrigger>();arthur=FindFirstObjectByType<ArthurJalurKampus>();area.idTempat="NPC_"+gameObject.GetInstanceID();foreach(var c in GetComponents<Collider2D>())c.enabled=false;Terapkan();}
    void Update()
    {
        if(arthur!=null&&arthur.kamera!=null)transform.rotation=Quaternion.Euler(0,arthur.kamera.transform.eulerAngles.y,0);
        if(arthur==null)return;
        bool dekat=Terdekat(arthur,out var d)==this&&d<InteraksiJalurKampus.JarakTempatTerdekat(arthur);
        bubble?.AturPenawaranJalur(dekat);
        if(dekat)area.TawarkanInteraksi();else DialogManager.Instance?.HapusInteraksi(area);
    }
    void OnDisable(){aktif.Remove(this);bingkai=-1;if(area!=null)DialogManager.Instance?.HapusInteraksi(area);}
    void OnValidate(){Terapkan();}
    public void Terapkan()
    {
        var area=GetComponent<AreaInteraksi>();if(area==null||!dialogSederhana)return;
        area.tipeInteraksi=TipeInteraksi.NPC_Karakter;
        area.skenarioDialog=new[]{new BarisDialog{siapaYangBicara=Pembicara.Kanan_NPC,namaKarakter=namaNpc,teksDialog=dialog,teksTombolLanjut="Selesai"}};
        area.daftarOpsi=Array.Empty<OpsiInteraksi>();area.tampilkanTombolKeluar=true;
    }
}
