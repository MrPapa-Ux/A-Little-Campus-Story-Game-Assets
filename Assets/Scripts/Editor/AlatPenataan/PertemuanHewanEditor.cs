using UnityEngine;using UnityEditor;
[CustomEditor(typeof(PertemuanHewanKampus))]
public sealed class PertemuanHewanEditor:Editor
{
 [MenuItem("Arthur/Uji anjing dan kucing (Play)")]
 static void Select(){var m=Object.FindFirstObjectByType<PertemuanHewanKampus>();if(m!=null)Selection.activeGameObject=m.gameObject;}
 public override void OnInspectorGUI(){var m=(PertemuanHewanKampus)target;EditorGUILayout.HelpBox("Pertemuan: 20% per kunjungan area luar. Crystal, Edel, Dining, taman, dan jalan Pioneer diundi terpisah. Varian humanoid: 1% per kunjungan (termasuk dalam 20%). Pratinjau tidak mengubah peluang game.",MessageType.Info);
 var arthur=Object.FindFirstObjectByType<ArthurJalurKampus>();bool luar=EditorApplication.isPlaying&&arthur!=null&&PertemuanHewanKampus.BolehMuncul(arthur.JalurAktif);
 if(EditorApplication.isPlaying&&!luar)EditorGUILayout.HelpBox("Masuk ke jalur luar taman/Crystal/Edel/Dining atau perjalanan Pioneer dahulu. Hewan tidak ditampilkan di interior.",MessageType.Info);
 using(new EditorGUI.DisabledScope(!luar)){
  if(GUILayout.Button("Pratinjau anjing normal"))m.Pratinjau(PertemuanHewanKampus.Hasil.Anjing);
  if(GUILayout.Button("Pratinjau kucing normal"))m.Pratinjau(PertemuanHewanKampus.Hasil.Kucing);
  if(GUILayout.Button("Pratinjau keduanya"))m.Pratinjau(PertemuanHewanKampus.Hasil.Keduanya);
  if(GUILayout.Button("Pratinjau varian humanoid langka"))m.Pratinjau(PertemuanHewanKampus.Hasil.Keduanya,true);
  if(GUILayout.Button("Hilangkan pratinjau"))m.Pratinjau(PertemuanHewanKampus.Hasil.TidakAda);
 }
 EditorGUILayout.LabelField("Zona",m.Zona);EditorGUILayout.LabelField("Hasil kunjungan",m.HasilKunjungan.ToString());DrawDefaultInspector();}
}
