using UnityEngine;

// EditorOnly handle. The Editor tool moves the scenery and its existing gameplay points together.
[AddComponentMenu("Arthur/Pegangan world kampus")]
public sealed class PeganganWorldKampus : MonoBehaviour
{
    [TextArea(4,7)] public string petunjuk="Pilih PINDAHKAN WORLD GH, bukan potongan bangunannya. W untuk geser; E lalu tarik lingkaran HIJAU untuk memutar mendatar (sumbu Y). Model, parkiran, jalur, bubble dan kamera ikut bersama. Ctrl+Z membatalkan. Simpan kedua scene lewat tab World. Kerjakan saat Play berhenti.";
    [HideInInspector] public Vector3 posisiTersimpan;
    [HideInInspector] public Quaternion rotasiTersimpan=Quaternion.identity;
    [HideInInspector] public bool rotasiSiap;
    void OnDrawGizmos(){Gizmos.color=Color.cyan;Gizmos.DrawWireCube(transform.position+Vector3.up,new Vector3(2,2,2));}
}
