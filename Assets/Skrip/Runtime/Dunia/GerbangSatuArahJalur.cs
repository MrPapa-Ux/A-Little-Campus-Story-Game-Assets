using UnityEngine;

// A route-space gate cannot be skipped by a fast frame, unlike a narrow trigger.
public sealed class GerbangSatuArahJalur : MonoBehaviour
{
    public JalurKampus jalur;
    [Tooltip("Titik hijau batas kunci. Menggeser titik ini ikut menggeser batas satu arah.")]
    public Transform titikKunci;
    public bool majuMengikutiUrutan=true;
    [TextArea(2,4)] public string petunjuk="Setelah Arthur melewati titik ini menuju Crystal, jalan di belakang titik terkunci. Pulang memakai bubble Kembali ke bundaran di jalan depan Crystal. Status diulang saat teleportasi tiba kembali sebelum batas.";
    [SerializeField] bool terkunci;
    public bool Terkunci=>terkunci;
    public float JarakBatas=>jalur!=null&&titikKunci!=null?jalur.JarakTerdekat(titikKunci.position):0;
    public float Batasi(float dari,float tujuan)
    {
        if(!isActiveAndEnabled||jalur==null||titikKunci==null)return tujuan;
        float batas=JarakBatas;
        if(majuMengikutiUrutan){if(dari>batas+.05f||tujuan>batas+.05f)terkunci=true;return terkunci?Mathf.Max(tujuan,batas+.05f):tujuan;}
        if(dari<batas-.05f||tujuan<batas-.05f)terkunci=true;return terkunci?Mathf.Min(tujuan,batas-.05f):tujuan;
    }
    public void AturKedatangan(float jarak){terkunci=majuMengikutiUrutan?jarak>JarakBatas+.05f:jarak<JarakBatas-.05f;}
    void OnDrawGizmosSelected(){if(titikKunci==null)return;Gizmos.color=new Color(1,.45f,.05f);Gizmos.DrawWireSphere(titikKunci.position-Vector3.up*ArthurJalurKampus.JarakKaki,.55f);}
}
