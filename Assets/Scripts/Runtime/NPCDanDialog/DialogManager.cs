// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Membuka/menutup dialog, menyimpan interaksi aktif, dan mengunci atau melepas gerakan pemain.
// Panduan: Assets/Scripts/Runtime/NPCDanDialog/README.md

using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance; 
    [HideInInspector] public bool minigameAktif;

    [Header("UI Referensi")]
    public GameObject canvasDialog;
    public KarakterController arthurScript; 
    public CanvasGroup[] hudSaatPercakapan;
    public bool PercakapanAktif {get;private set;}
    public AreaInteraksi PemilikInteraksi {get;private set;}
    struct KeadaanHUD {public CanvasGroup grup;public float alpha;public bool interaksi,raycast;}
    readonly System.Collections.Generic.List<KeadaanHUD> keadaanHUD=new System.Collections.Generic.List<KeadaanHUD>();

    [Header("Status Jarak NPC & Area")]
    public bool bisaInteraksi = false; 
    public string idInteraksiAktif = ""; 
    
    [Header("Data Titipan Trigger (JANGAN DIISI MANUAL)")]
    // GANTI string[] menjadi BarisDialog[]
    public BarisDialog[] skenarioAktif; 
    
    public OpsiInteraksi[] opsiInteraksiAktif;
    public bool statusTombolKeluar = true; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        bisaInteraksi = false;
        idInteraksiAktif = "";
        skenarioAktif = null;
        opsiInteraksiAktif = null;
        statusTombolKeluar = true;
    }

    void Start()
    {
        if (canvasDialog != null) canvasDialog.SetActive(false);
    }
    public void TawarkanInteraksi(AreaInteraksi area)
    {
        if(area==null||PercakapanAktif)return;
        PemilikInteraksi=area;bisaInteraksi=true;idInteraksiAktif=area.idTempat;
        skenarioAktif=area.skenarioDialog;opsiInteraksiAktif=MalamGAChapter1.Saring(area);
        statusTombolKeluar=area.tampilkanTombolKeluar;
    }
    public void HapusInteraksi(AreaInteraksi area)
    {
        if(PercakapanAktif||PemilikInteraksi!=area)return;
        PemilikInteraksi=null;bisaInteraksi=false;idInteraksiAktif="";
        skenarioAktif=null;opsiInteraksiAktif=null;
    }

    public void BukaDialog()
    {
        if(minigameAktif)return;
        SistemHP hp = FindFirstObjectByType<SistemHP>(); 
        if (hp != null && hp.hpSedangBuka) return; 
        if(canvasDialog==null)return;
        if(!PercakapanAktif)
        {
            PercakapanAktif=true;keadaanHUD.Clear();
            // Story dialogue interrupts the phone playlist; ambient NPC dialogue does not.
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.StartsWith("Chapter1") && MusikArthur.Instance!=null)
                MusikArthur.Instance.HentikanSemua();
            if(hudSaatPercakapan!=null)foreach(var grup in hudSaatPercakapan)
            {
                if(grup==null)continue;
                keadaanHUD.Add(new KeadaanHUD{grup=grup,alpha=grup.alpha,interaksi=grup.interactable,raycast=grup.blocksRaycasts});
                grup.alpha=0;grup.interactable=false;grup.blocksRaycasts=false;
            }
        }

        if (canvasDialog != null) canvasDialog.SetActive(true);
        if (arthurScript != null)
        {
            arthurScript.BerhentiBergerak(); 
            arthurScript.kunciPergerakan = true; 
        }
    }

    public void TutupDialog()
    {
        SistemPilihan pilihan = GetComponent<SistemPilihan>();
        if (pilihan != null) pilihan.AkhiriPercakapanVisual();
        if (canvasDialog != null) canvasDialog.SetActive(false);
        PercakapanAktif=false;
        if(PemilikInteraksi!=null&&!PemilikInteraksi.isActiveAndEnabled)HapusInteraksi(PemilikInteraksi);
        foreach(var keadaan in keadaanHUD)if(keadaan.grup!=null)
        {keadaan.grup.alpha=keadaan.alpha;keadaan.grup.interactable=keadaan.interaksi;keadaan.grup.blocksRaycasts=keadaan.raycast;}
        keadaanHUD.Clear();
        if (arthurScript != null)
        {
            SistemHP hp = FindFirstObjectByType<SistemHP>(); 
            if (hp == null || !hp.hpSedangBuka) arthurScript.kunciPergerakan = false; 
        }
    }
    void OnDestroy(){if(Instance==this)Instance=null;}
}
