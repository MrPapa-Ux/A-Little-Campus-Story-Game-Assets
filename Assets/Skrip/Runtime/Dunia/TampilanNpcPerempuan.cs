using UnityEngine;

// Variasi pakaian tetap tertutup; sprite kepala, tangan, lanyard, dan animasi memakai aset semula.
[AddComponentMenu("Arthur/Tampilan pakaian NPC perempuan")]
public sealed class TampilanNpcPerempuan : MonoBehaviour
{
    public bool perempuan=true;
    public SpriteRenderer badan;
    public Sprite kemejaPerempuan;
    public Sprite badanSemula;
    public Color warnaSemula=Color.white;
    public Color warnaPakaian=Color.white;
    void Awake(){Terapkan();}
    void OnValidate(){Terapkan();}
    public void Terapkan()
    {
        if(badan==null||badanSemula==null)return;
        badan.sprite=perempuan&&kemejaPerempuan!=null?kemejaPerempuan:badanSemula;
        badan.color=perempuan?warnaPakaian:warnaSemula;
    }
}
