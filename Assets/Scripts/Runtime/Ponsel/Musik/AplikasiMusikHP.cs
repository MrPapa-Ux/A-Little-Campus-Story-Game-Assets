// PANDUAN PROYEK - Runtime / Ponsel/Musik
// Fungsi: Menghubungkan tampilan kaset, judul, progres, volume, dan tombol ke pemutar MusikArthur.
// Panduan: Assets/Scripts/Runtime/Ponsel/Musik/README.md

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AplikasiMusikHP : MonoBehaviour
{
    public SistemHP hp;
    public MusikArthur pemutar;
    public TMP_Text judulLagu,status,waktu,labelPutar,labelAcak,labelUlang;
    public TMP_Text[] namaSlot;
    public UnityEngine.UI.Image[] latarSlot;
    public Slider progres,volume;
    public RectTransform pita;
    public Color warnaKaset = new Color(1f,.5f,0f,1f);
    public KasetCoretanUI coretan;
    float nextUpdate;
    bool tersambung;
    void OnEnable()
    {
        if(MusikArthur.Instance!=null)pemutar=MusikArthur.Instance;
        if(pemutar==null)return;
        pemutar.Berubah+=Segarkan;tersambung=true;
        volume.SetValueWithoutNotify(pemutar.Volume);Segarkan();
    }
    void OnDisable(){if(tersambung && pemutar!=null)pemutar.Berubah-=Segarkan;tersambung=false;if(coretan!=null)coretan.AturGerak(false);}
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))hp.TutupAplikasi();
        if(pemutar==null || Time.unscaledTime<nextUpdate)return;nextUpdate=Time.unscaledTime+.2f;
        progres.SetValueWithoutNotify(pemutar.Progres);
        var source=pemutar.sumber;
        waktu.text=Format(source.clip!=null?source.time:0)+" / "+Format(source.clip!=null?source.clip.length:0);
        if(coretan!=null)coretan.AturGerak(pemutar.Memutar);
    }
    static string Format(float time){int n=Mathf.FloorToInt(time);return(n/60).ToString("00")+":"+(n%60).ToString("00");}
    public void Segarkan()
    {
        if(pemutar==null)return;
        judulLagu.text=pemutar.Lagu?.audio!=null ? pemutar.Lagu.judul : "BELUM ADA LAGU";
        status.text=pemutar.Status;labelPutar.text=pemutar.Memutar?"JEDA":"PUTAR";
        labelAcak.text=pemutar.acak?"ACAK: ON":"ACAK: OFF";labelUlang.text=pemutar.ulang?"ULANG: ON":"ULANG: OFF";
        if(pita!=null)pita.GetComponent<UnityEngine.UI.Image>().color=warnaKaset;
        progres.fillRect.GetComponent<UnityEngine.UI.Image>().color=warnaKaset;
        volume.fillRect.GetComponent<UnityEngine.UI.Image>().color=warnaKaset;
        if(coretan!=null)coretan.AturGerak(pemutar.Memutar);
        for(int i=0;i<namaSlot.Length;i++)
        {
            int indeks=(pemutar.Pilihan/Mathf.Max(1,namaSlot.Length))*namaSlot.Length+i;
            var lagu=pemutar.playlist!=null && indeks<pemutar.playlist.lagu.Length?pemutar.playlist.lagu[indeks]:null;
            namaSlot[i].text=(indeks+1).ToString("00")+"   "+(lagu?.audio!=null?lagu.judul:"Lagu belum diisi");
            latarSlot[i].color=indeks==pemutar.Pilihan?Color.Lerp(warnaKaset,new Color32(239,230,211,255),.28f):new Color32(239,230,211,255);
        }
    }
    public void PilihSlot(int i){if(pemutar!=null)pemutar.Pilih((pemutar.Pilihan/Mathf.Max(1,namaSlot.Length))*namaSlot.Length+i);}
    public void Kiri(){pemutar?.Sebelumnya();}
    public void Kanan(){pemutar?.Berikutnya();}
    public void Putar(){pemutar?.PutarJeda();}
    public void Acak(){pemutar?.UbahAcak();}
    public void Ulang(){pemutar?.UbahUlang();}
    public void Volume(float v){pemutar?.UbahVolume(v);}
    public void Geser(float v){pemutar?.Geser(v);}
}
