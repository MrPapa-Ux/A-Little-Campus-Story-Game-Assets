// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Aset data ScriptableObject untuk identitas dan potongan suara kertas setiap karakter.
// Panduan: Assets/Scripts/Runtime/NPCDanDialog/README.md

using UnityEngine;

[CreateAssetMenu(menuName="Kampus/Profil suara kertas")]
public class ProfilSuaraKertas : ScriptableObject
{
    public string namaKarakter;
    public AudioClip[] potongan;
    [Range(.5f,2)] public float nada=1;
    [Range(0,.4f)] public float variasiNada=.12f;
    [Range(0,1)] public float volume=.25f;
    [Range(.035f,.3f)] public float jeda=.075f;
    [Range(1,4)] public int hurufPerBunyi=2;
}
