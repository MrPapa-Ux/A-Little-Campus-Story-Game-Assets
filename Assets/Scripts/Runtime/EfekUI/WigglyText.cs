// PANDUAN PROYEK - Runtime / EfekUI
// Fungsi: Menggoyangkan huruf TMP dengan basis mesh yang disimpan ulang saat isi teks berubah.
// Panduan: Assets/Scripts/Runtime/EfekUI/README.md

using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class WigglyText : MonoBehaviour
{
    public float kecepatanAnimasi = 7f;
    public float kekuatanGoyang = 2f;
    public int JumlahBangunBasis { get; private set; }
    TMP_Text komponenTeks;
    Vector3[][] basis;
    float timer;
    void Awake() { komponenTeks = GetComponent<TMP_Text>(); }
    void OnEnable()
    {
        if(komponenTeks==null)komponenTeks=GetComponent<TMP_Text>();
        timer=0;basis=null;komponenTeks.OnPreRenderText+=SimpanBasis;komponenTeks.SetVerticesDirty();
    }
    void OnDisable(){if(komponenTeks!=null)komponenTeks.OnPreRenderText-=SimpanBasis;basis=null;}
    // TMP owns rebuilds. Capture its fresh geometry after text, font or layout changes.
    void SimpanBasis(TMP_TextInfo info)
    {
        basis=new Vector3[info.meshInfo.Length][];
        for(int i=0;i<basis.Length;i++)basis[i]=(Vector3[])info.meshInfo[i].vertices.Clone();
        JumlahBangunBasis++;
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer < 1f / Mathf.Max(.1f,kecepatanAnimasi)) return;
        timer = 0f;
        if(basis==null||komponenTeks.havePropertiesChanged)return;
        var textInfo = komponenTeks.textInfo;
        for(int i=0;i<textInfo.meshInfo.Length&&i<basis.Length;i++)
            System.Array.Copy(basis[i],textInfo.meshInfo[i].vertices,Mathf.Min(basis[i].Length,textInfo.meshInfo[i].vertices.Length));
        for(int i=0;i<textInfo.characterCount;i++)
        {
            var c=textInfo.characterInfo[i]; if(!c.isVisible) continue;
            int material=c.materialReferenceIndex;
            if(material>=basis.Length || c.vertexIndex+3>=basis[material].Length||c.vertexIndex+3>=textInfo.meshInfo[material].vertices.Length) continue;
            var vertices=textInfo.meshInfo[material].vertices;
            for(int j=0;j<4;j++) vertices[c.vertexIndex+j]=basis[material][c.vertexIndex+j]+new Vector3(Random.Range(-kekuatanGoyang,kekuatanGoyang),Random.Range(-kekuatanGoyang,kekuatanGoyang),0f);
        }
        // Geometri font tidak dibuat ulang setiap ketukan: hanya posisi vertex diperbarui.
        komponenTeks.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}
