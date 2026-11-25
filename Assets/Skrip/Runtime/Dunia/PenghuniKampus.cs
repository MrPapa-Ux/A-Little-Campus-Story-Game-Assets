using UnityEngine;

public sealed class PenghuniKampus : MonoBehaviour
{
    public GameObject depanJasmine, kantin, taman;
    KelompokNpcManual[] manual;
    ArthurJalurKampus arthur;
    void Start(){manual=FindObjectsByType<KelompokNpcManual>(FindObjectsInactive.Include,FindObjectsSortMode.None);arthur=FindFirstObjectByType<ArthurJalurKampus>();}
    void Update()
    {
        if(MalamEksplorasiArthur.NpcTidur)return;
        // Hindari NPC terus diaktifkan lagi lalu dimatikan oleh event malam.
        var cerita=PerjalananChapelChapter1.Instance;
        if(cerita!=null&&cerita.isActiveAndEnabled&&cerita.MalamAktif)return;
        var manager=KawasanKampus.Instance;
        string id=manager!=null&&manager.Siap?manager.KawasanAktif:"";
        bool gh=arthur!=null&&arthur.JalurAktif!=null&&arthur.JalurAktif.name.StartsWith("GH_");
        Atur(depanJasmine,id=="JasmineGK2"&&!gh);Atur(kantin,id=="JasmineGK2"&&!gh);Atur(taman,id=="TamanUNKLAB");
        if(manual!=null)foreach(var kelompok in manual)if(kelompok!=null)Atur(kelompok.gameObject,kelompok.kawasan=="GH"?id=="JasmineGK2"&&gh:id==kelompok.kawasan&&!(id=="JasmineGK2"&&gh));
    }
    static void Atur(GameObject g,bool aktif){if(g!=null&&g.activeSelf!=aktif)g.SetActive(aktif);}
}
