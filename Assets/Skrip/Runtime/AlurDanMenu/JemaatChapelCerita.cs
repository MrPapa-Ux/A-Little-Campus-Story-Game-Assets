using UnityEngine;

// Kerumunan digabung menjadi dua mesh. Tidak ada Animator, collider, atau AI per jemaat.
public sealed class JemaatChapelCerita : MonoBehaviour
{
 [Min(1)] public int jumlahJemaat=120;
 public Transform[] tempatDuduk;
 [HideInInspector] public Vector3[] posisiTersimpan;
 [ContextMenu("Terapkan posisi penanda duduk yang digeser")]
 public void TerapkanPosisi(){
  if(tempatDuduk==null||posisiTersimpan==null||tempatDuduk.Length!=posisiTersimpan.Length)return;
  foreach(var filter in GetComponentsInChildren<MeshFilter>(true)){
   var mesh=Application.isPlaying?filter.mesh:filter.sharedMesh;if(mesh==null||mesh.vertexCount%tempatDuduk.Length!=0)continue;
#if UNITY_EDITOR
   if(!Application.isPlaying)UnityEditor.Undo.RecordObject(mesh,"Geser jemaat Chapel");
#endif
   var v=mesh.vertices;int per=v.Length/tempatDuduk.Length;
   for(int i=0;i<tempatDuduk.Length;i++){var delta=tempatDuduk[i].localPosition-posisiTersimpan[i];for(int j=i*per;j<(i+1)*per;j++)v[j]+=delta;}
   mesh.vertices=v;mesh.RecalculateBounds();
#if UNITY_EDITOR
   if(!Application.isPlaying)UnityEditor.EditorUtility.SetDirty(mesh);
#endif
  }
  for(int i=0;i<tempatDuduk.Length;i++)posisiTersimpan[i]=tempatDuduk[i].localPosition;
 }
}
