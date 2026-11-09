// PANDUAN PROYEK - Runtime / Ponsel/Musik
// Fungsi: Aset data ScriptableObject yang menyimpan pemilik dan slot LaguArthur.
// Panduan: Assets/Scripts/Runtime/Ponsel/Musik/README.md

using UnityEngine;

[System.Serializable]
public class LaguArthur
{
    public string judul;
    public AudioClip audio;
    [TextArea] public string catatan;
}

[CreateAssetMenu(menuName="Kampus/Playlist karakter")]
public class PlaylistArthur : ScriptableObject
{
    public string pemilik = "Arthur";
    public LaguArthur[] lagu = new LaguArthur[5];
}
