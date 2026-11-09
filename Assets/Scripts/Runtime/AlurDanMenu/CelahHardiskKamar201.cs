using UnityEngine;

// Dipertahankan agar referensi scene tetap valid. Lemari tidak lagi diangkat
// atau diberi kaki tambahan untuk quest hardisk.
public sealed class CelahHardiskKamar201 : MonoBehaviour
{
    public string namaLemari="LemariMejaTarik_1";
    [Min(0)] public float celah;
}
