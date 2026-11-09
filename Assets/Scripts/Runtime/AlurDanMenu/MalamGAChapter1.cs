using System.Linq;
using UnityEngine;

// Semua aturan ini hanya untuk malam cerita, tanpa mengubah opsi asli scene.
public sealed class MalamGAChapter1 : MonoBehaviour
{
 static MalamGAChapter1 instance;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
 static void ResetInstance(){instance=null;}
 void OnEnable(){instance=this;}
 static readonly string[] BubbleDitutupTetap={"GA_Lobby_PilihJalan","GA_AksesLanjutan","GA_Teleport_JalanBelakang","GA_Taman_KeJalurDepan","GA_KeGedungKuliahSatu"};
 public Transform grupCahaya;
 public Transform batasLorong;
 public JalurKampus jalurLorong;
 [Header("Geser titik ini untuk posisi bubble malam")]
 public Transform titikBubbleLobbyMalam;
 public InteraksiJalurKampus interaksiLobby;
 public Transform kembaliDepanGA;
 Vector3 posisiLobbySiang;
 float radiusLobbySiang;
 bool posisiDisimpan;
 void Awake(){if(interaksiLobby!=null){posisiLobbySiang=interaksiLobby.transform.position;radiusLobbySiang=interaksiLobby.radius;posisiDisimpan=true;}}
 public string[] bubbleDitutup={"GA_Depan_KeTaman","GA_Lobby_TamanKiri","GA_Lobby_TamanTengah","GA_KeGedungKuliahSatu","GA_UjungBarat_NaikLantaiDua","GA_NaikLantaiDua","GA_TanggaBarat_KeLantaiDua","GA_TanggaTengah_KeLantaiDua"};
 public static bool BubbleBoleh(string nama){if(!BatasMalamChapter1.Aktif())return true;var c=instance;return (c==null||!c.bubbleDitutup.Contains(nama))&&!BubbleDitutupTetap.Contains(nama);}
 public static OpsiInteraksi[] Saring(AreaInteraksi area){
  if(!BatasMalamChapter1.Aktif()||area.daftarOpsi==null)return area.daftarOpsi;
  var opsi=area.daftarOpsi.Where(o=>BatasMalamChapter1.Boleh(o.titikTujuan,o.teksOpsi)&&!(area.name=="GA_LobbyDalam_PilihSisi"&&o.teksOpsi.ToLowerInvariant().Contains("kanan"))).ToList();
  if(area.name=="GA_LobbyDalam_KeTaman"){var c=instance;return c!=null&&c.kembaliDepanGA!=null?new[]{new OpsiInteraksi{teksOpsi="Kembali ke jalan depan GA",titikTujuan=c.kembaliDepanGA}}:System.Array.Empty<OpsiInteraksi>();}
  return opsi.ToArray();
 }
 public static float Batasi(JalurKampus jalur,float tujuan){if(jalur==null||!BatasMalamChapter1.Aktif())return tujuan;var c=instance;if(c==null||jalur.name!="GA_JalanBelakang_Lorong"||c.batasLorong==null)return tujuan;return Mathf.Min(tujuan,jalur.JarakTerdekat(c.batasLorong.position));}
 void Update(){var cerita=PerjalananChapelChapter1.Instance;bool malam=cerita!=null&&cerita.MalamAktif;bool ga=KawasanKampus.Instance!=null&&KawasanKampus.Instance.KawasanAktif=="GA";if(grupCahaya!=null&&grupCahaya.gameObject.activeSelf!=(malam&&ga))grupCahaya.gameObject.SetActive(malam&&ga);
  if(posisiDisimpan&&titikBubbleLobbyMalam!=null&&interaksiLobby!=null){bool batas=BatasMalamChapter1.Aktif();interaksiLobby.transform.position=batas?titikBubbleLobbyMalam.position:posisiLobbySiang;interaksiLobby.radius=batas?1.1f:radiusLobbySiang;}
 }
 void OnDisable(){if(instance==this)instance=null;if(grupCahaya!=null)grupCahaya.gameObject.SetActive(false);if(posisiDisimpan&&interaksiLobby!=null){interaksiLobby.transform.position=posisiLobbySiang;interaksiLobby.radius=radiusLobbySiang;}}
}


