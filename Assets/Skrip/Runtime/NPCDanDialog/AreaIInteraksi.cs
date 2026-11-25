// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Mendeteksi pemain dalam area, lalu memberikan data NPC/tempat, skenario, dan opsi kepada DialogManager.
// Panduan: Assets/Skrip/Runtime/NPCDanDialog/README.md

using UnityEngine;

public class AreaInteraksi : MonoBehaviour
{
    [Header("Identitas & Tipe Interaksi")]
    public string idTempat = "NamaTempat_Atau_NPC"; 
    public TipeInteraksi tipeInteraksi = TipeInteraksi.NPC_Karakter; 

    [Tooltip("Potret NPC pada dialog. Kosongkan untuk memakai tampilan semula; gambar ini tidak mengubah sprite NPC di dunia.")]
    public Sprite potretDialog;

    [Header("Pengaturan Tombol Keluar (Kiri Bawah)")]
    [Tooltip("Centang jika ingin menampilkan tombol Keluar/Batal")]
    public bool tampilkanTombolKeluar = true;

    [Header("1. SUTRADARA CERITA (Muncul Duluan)")]
    [Tooltip("Susun adegan dialog berbalas pantun di sini!")]
    public BarisDialog[] skenarioDialog; // <--- INI YANG BARU!

    [Header("2. Daftar Panel Opsi (Muncul Terakhir)")]
    public OpsiInteraksi[] daftarOpsi;
    [Min(0),Tooltip("Nol memakai pemicu semula. Nilai positif mempersempit akses pada jalur mendatar.")]
    public float batasJarakMendatar;
    public bool DalamBatas
    {
        get {
            if(batasJarakMendatar<=0)return true;
            var p=DialogManager.Instance!=null?DialogManager.Instance.arthurScript:null;
            return p!=null&&Mathf.Abs(p.transform.position.x-transform.position.x)<=batasJarakMendatar&&Mathf.Abs(p.transform.position.y-transform.position.y)<1.25f;
        }
    }

    public void TawarkanInteraksi() { KirimDataKeManager(); }

    private void KirimDataKeManager()
    {
        if(!DalamBatas){DialogManager.Instance?.HapusInteraksi(this);return;}
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.TawarkanInteraksi(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) KirimDataKeManager();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) KirimDataKeManager();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && DialogManager.Instance != null)
        {
            DialogManager.Instance.HapusInteraksi(this);
        }
    }
    void OnDisable(){if(DialogManager.Instance!=null)DialogManager.Instance.HapusInteraksi(this);}
}
