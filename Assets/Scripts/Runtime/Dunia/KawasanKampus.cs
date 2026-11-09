using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Arthur, routes, UI and destinations stay in CharacterLab. Only scenery streams.
[DefaultExecutionOrder(-500)]
public sealed class KawasanKampus : MonoBehaviour
{
    [Serializable] public sealed class Kawasan { public string id; public string scene; }
    public Kawasan[] kawasan;
    [Tooltip("Awal khusus scene cerita. Kosong pada mode eksplorasi.")]
    public Transform titikMulaiCerita;
#if UNITY_EDITOR
    [Header("Mulai Play langsung di tempat yang sedang dikerjakan")]
    [Tooltip("Khusus Play di Editor. Kosongkan untuk mulai dari Jasmine. Build game tetap memakai awal biasa.")]
    public Transform titikMulaiPengujian;
#endif
    public static KawasanKampus Instance { get; private set; }
    public bool SedangMemuat { get; private set; }
    public bool Siap { get; private set; }
    public string KawasanAktif { get; private set; }
    public string KesalahanTerakhir { get; private set; }
    ArthurJalurKampus arthur;
    SuasanaKampus suasana;
    Scene aktif;

    void Awake() { Instance=this;SedangMemuat=true; }
    void Start()
    {
        arthur=FindFirstObjectByType<ArthurJalurKampus>();
        suasana=FindFirstObjectByType<SuasanaKampus>();
        var pilihan=FindFirstObjectByType<SistemPilihan>();
        if(pilihan!=null&&pilihan.efekTransisi!=null)
            pilihan.efekTransisi.MulaiTransisiTeleportRutin(MuatAwal,true);
        else StartCoroutine(MuatAwal());
    }
    IEnumerator MuatAwal()
    {
        var simpanan = GetComponent<SimpananEksplorasiArthur>();
        var tujuanTerakhir = simpanan != null ? simpanan.TujuanTersimpan() : null;
        if (tujuanTerakhir != null)
        {
            yield return Pindahkan(tujuanTerakhir, () => simpanan.TerapkanTujuan(tujuanTerakhir));
            Siap = aktif.IsValid() && aktif.isLoaded;
            yield break;
        }
        if(titikMulaiCerita!=null){
            yield return Pindahkan(titikMulaiCerita,()=>arthur.PindahKe(titikMulaiCerita));
            Siap=aktif.IsValid()&&aktif.isLoaded;yield break;
        }
#if UNITY_EDITOR
        if(titikMulaiPengujian!=null&&titikMulaiPengujian.GetComponent<TujuanJalurKampus>()?.jalur!=null)
        {
            yield return Pindahkan(titikMulaiPengujian,()=>
            {
                arthur.PindahKe(titikMulaiPengujian);
                var pilihan=FindFirstObjectByType<SistemPilihan>();
                if(pilihan!=null)
                {
                    if(pilihan.grupFisikDepan!=null)pilihan.grupFisikDepan.SetActive(false);
                    if(pilihan.grupFisikBelakang!=null)pilihan.grupFisikBelakang.SetActive(false);
                    if(pilihan.grupFisikRuangan!=null)pilihan.grupFisikRuangan.SetActive(false);
                }
            });
        }
        else
#endif
            yield return Pindahkan(null,null);
        Siap=aktif.IsValid()&&aktif.isLoaded;
    }
    public static string IdUntuk(JalurKampus jalur)
    {
        string n=jalur!=null?jalur.name:"";
        if(n=="GH_Kamar201")return "GHRoom";
        if(n.StartsWith("UNK_",StringComparison.Ordinal))return "TamanUNKLAB";
        if(n.StartsWith("Pioneer_",StringComparison.Ordinal))return "Pioneer";
        if(n.StartsWith("GA_",StringComparison.Ordinal))return "GA";
        if(n.StartsWith("GK1_",StringComparison.Ordinal))return "GK1";
        // GH shares the same streamed scenery as Jasmine, GK2 and the canteen.
        if(n.StartsWith("GH_",StringComparison.Ordinal))return "JasmineGK2";
        return "JasmineGK2";
    }
    public IEnumerator Pindahkan(Transform tujuan,Action pindah)
    {
        // Caller owns the opaque fade for the entire load, move, unload operation.
        SedangMemuat=true;KesalahanTerakhir=null;
        try
        {
            var penanda=tujuan!=null?tujuan.GetComponent<TujuanJalurKampus>():null;
            string id=IdUntuk(tujuan!=null?(penanda!=null?penanda.jalur:null):(arthur!=null?arthur.JalurAktif:null));
            Kawasan data=Array.Find(kawasan??Array.Empty<Kawasan>(),k=>k!=null&&k.id==id);
            if(data==null||!Application.CanStreamedLevelBeLoaded(data.scene))
            { Gagal("Scene kawasan tidak tersedia: "+id);yield break; }
            if(aktif.IsValid()&&aktif.isLoaded&&KawasanAktif==id){pindah?.Invoke();yield break;}
            Scene berikut=SceneManager.GetSceneByPath(data.scene);
            AsyncOperation muat=null;
            if(!berikut.IsValid()||!berikut.isLoaded)
            {
                try{muat=SceneManager.LoadSceneAsync(data.scene,LoadSceneMode.Additive);}
                catch(Exception e){Gagal(e.Message);}
                if(muat==null)yield break;
                yield return muat;
                berikut=SceneManager.GetSceneByPath(data.scene);
            }
            if(!berikut.IsValid()||!berikut.isLoaded){Gagal("Gagal memuat kawasan: "+id);yield break;}
            GameObject chapel=null;
            if(id=="Pioneer")foreach(var root in berikut.GetRootGameObjects())if(root.name=="World_InteriorPioneerChapel")chapel=root;
            var chapelLama=suasana!=null?suasana.interiorPioneer:null;
            if(suasana!=null)suasana.interiorPioneer=chapel;
            if(chapel!=null)chapel.SetActive(true);
            bool berhasil=true;
            try{pindah?.Invoke();}catch(Exception e){berhasil=false;Gagal(e.ToString());}
            if(!berhasil)
            {
                if(suasana!=null)suasana.interiorPioneer=chapelLama;
                yield return SceneManager.UnloadSceneAsync(berikut);yield break;
            }
            var lama=aktif;aktif=berikut;KawasanAktif=id;Siap=true;
            if(lama.IsValid()&&lama.isLoaded&&lama!=berikut)yield return SceneManager.UnloadSceneAsync(lama);
            // Also handle entering Play with extra region previews open in the Editor.
            foreach(var other in kawasan)
            {
                if(other==null||other.id==id)continue;
                var preview=SceneManager.GetSceneByPath(other.scene);
                if(preview.IsValid()&&preview.isLoaded)yield return SceneManager.UnloadSceneAsync(preview);
            }
            // This runs at region boundaries, never per frame or between floors.
            yield return Resources.UnloadUnusedAssets();
        }
        finally { SedangMemuat=false; }
    }
    void Gagal(string pesan){KesalahanTerakhir=pesan;Debug.LogError("Perpindahan kawasan dibatalkan. "+pesan,this);}
    void OnDestroy(){if(Instance==this)Instance=null;}
}
