using UnityEngine;
public static class BatasMalamChapter1
{
 public static bool Aktif(){var c=PerjalananChapelChapter1.Instance;return c!=null&&c.isActiveAndEnabled&&c.MalamAktif&&(c.Status==PerjalananChapelChapter1.Tahap.Pulang||c.Status==PerjalananChapelChapter1.Tahap.KeluarChapel);}
 public static bool BolehJalur(JalurKampus jalur){if(!Aktif()||jalur==null)return true;return jalur.name.StartsWith("GA_")||jalur.name.StartsWith("GH_");}
 public static bool Boleh(Transform tujuan,string label){if(!Aktif())return true;var cerita=PerjalananChapelChapter1.Instance;if(cerita!=null&&cerita.Status==PerjalananChapelChapter1.Tahap.KeluarChapel&&tujuan!=null&&tujuan.name=="TitikFoyer_DariAula")return true;var s=(label??"").ToLowerInvariant().Replace(" ","");foreach(var n in TujuanDitutup)if(s.Contains(n))return false;var p=tujuan!=null?tujuan.GetComponent<TujuanJalurKampus>():null;return BolehJalur(p!=null?p.jalur:null);}
 static readonly string[] TujuanDitutup={"gk1","gk2","dining","kantin","tamanunklab","crystal","edel","chapel"};
}


