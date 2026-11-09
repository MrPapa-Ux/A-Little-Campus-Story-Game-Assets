using UnityEngine;

// Pilihan membuka satu jalan secara fisik; Arthur melanjutkan berjalan sendiri.
public sealed class PengaturCabangKampus : MonoBehaviour
{
    public JalurKampus jalurLobby;
    public JalurKampus jalanDepan;
    public JalurKampus jalanBelakang;
    public BoxCollider dindingDepan;
    public BoxCollider dindingBelakang;
    [Tooltip("Buka jalur lurus saat mulai; jalur cabang tetap dipilih lewat interaksi.")]
    public bool bukaJalanDepanSaatMulai;
    public bool SudahMemilih { get; private set; }
    public bool MemilihDepan { get; private set; }
    ArthurJalurKampus arthur;
    bool dikunciMalam;

    void Start()
    {
        arthur = FindFirstObjectByType<ArthurJalurKampus>();
        SesuaikanDenganJalur();
        jalurLobby.lanjutanAkhir = null;
        dindingDepan.enabled = true;
        dindingBelakang.enabled = true;
        if (bukaJalanDepanSaatMulai) Atur(true);
    }

    void Update()
    {
        if (name=="GA_Lobby_PilihJalan" && BatasMalamChapter1.Aktif()){jalurLobby.lanjutanAkhir=null;dindingDepan.enabled=true;dindingBelakang.enabled=true;dikunciMalam=true;return;}
        if(dikunciMalam){dikunciMalam=false;SudahMemilih=false;}
        if (arthur == null || arthur.JalurAktif == null) return;
        // Kedatangan dari tangga di jalan belakang juga harus bisa kembali ke lobby.
        if (arthur.JalurAktif == jalanDepan && (!SudahMemilih || !MemilihDepan)) Atur(true);
        else if ((arthur.JalurAktif == jalanBelakang || arthur.JalurAktif == jalanBelakang.lanjutanAkhir)
                 && (!SudahMemilih || MemilihDepan)) Atur(false);
    }

    public bool Pilih(bool depan)
    {
        if(name=="GA_Lobby_PilihJalan" && BatasMalamChapter1.Aktif())return false;
        if (arthur == null) arthur = FindFirstObjectByType<ArthurJalurKampus>();
        if (arthur == null || arthur.JalurAktif != jalurLobby ||
            Vector3.Distance(arthur.transform.position, transform.position) > 1.5f) return false;
        Atur(depan);
        return true;
    }

    void Atur(bool depan)
    {
        SesuaikanDenganJalur();
        jalurLobby.lanjutanAkhir = depan ? jalanDepan : jalanBelakang;
        dindingDepan.enabled = !depan;
        dindingBelakang.enabled = depan;
        SudahMemilih = true;
        MemilihDepan = depan;
    }

    // Pembatas berada di cabangnya sendiri, bukan pada pendekatan bersama.
    // Koordinat lama tidak boleh menutup simpang setelah pemilik menggeser titik.
    public void SesuaikanDenganJalur()
    {
        if (jalurLobby == null || jalurLobby.titik == null || jalurLobby.titik.Length < 2) return;
        transform.position = jalurLobby.titik[jalurLobby.titik.Length - 1].position;
        AturPembatas(dindingDepan, jalanDepan);
        AturPembatas(dindingBelakang, jalanBelakang);
    }

    static void AturPembatas(BoxCollider pembatas, JalurKampus jalur)
    {
        if (pembatas == null || jalur == null || jalur.Panjang < .01f) return;
        jalur.Sampel(Mathf.Min(1.75f, jalur.Panjang * .5f), out var posisi, out var arah, out _);
        arah.y = 0;
        if (arah.sqrMagnitude < .001f) return;
        pembatas.transform.SetPositionAndRotation(posisi, Quaternion.LookRotation(arah));
        pembatas.center = Vector3.zero;
        pembatas.size = new Vector3(2.4f, 4, .26f);
    }
}
