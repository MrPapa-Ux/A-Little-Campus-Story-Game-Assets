using UnityEngine;
// Figuran hanya aktif di interior Dining; alur cerita dan NPC kawasan lain tetap terpisah.
public sealed class KeramaianDining : MonoBehaviour
{
 public JalurKampus jalur;
 public GameObject isi;
 ArthurJalurKampus arthur;
 void Start(){arthur=FindFirstObjectByType<ArthurJalurKampus>();}
 void Update(){if(arthur==null||isi==null)return;bool aktif=arthur.JalurAktif==jalur&&!(PerjalananChapelChapter1.Instance!=null&&PerjalananChapelChapter1.Instance.MalamAktif);if(isi.activeSelf!=aktif)isi.SetActive(aktif);}
}
