using UnityEngine;

// A destination gate, not a replacement movement system or a second player.
public sealed class AksesAsramaJasmine : MonoBehaviour
{
    public bool keluar;
    public Transform tujuan;
    public bool BolehMasuk => keluar ||
        (FindFirstObjectByType<PertukaranTubuhArthur>() is PertukaranTubuhArthur p && p.DiTubuhPerempuan) || ModeUjiArthur.BatasDibuka;
    public string PesanTerkunci => "Ini asrama putri. Arthur perlu meminjam tubuh Naya di kelas GK2 sebelum masuk.";
}
