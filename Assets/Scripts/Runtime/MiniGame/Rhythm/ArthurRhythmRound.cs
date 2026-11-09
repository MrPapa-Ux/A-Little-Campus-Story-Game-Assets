// PANDUAN PROYEK - Runtime / MiniGame/Rhythm
// Fungsi: Logika penilaian not, skor, akurasi, energi, combo, dan hasil ronde; kelas data/logika biasa.
// Panduan: Assets/Scripts/Runtime/MiniGame/Rhythm/README.md

using System;

[Serializable]
public struct NotArthur
{
    public int jalur;
    public float waktu;
    public NotArthur(int lane,float seconds){jalur=lane;waktu=seconds;}
}

// Waktu penilaian berasal dari clock audio, bukan jumlah frame atau jumlah ketukan.
public sealed class ArthurRhythmRound
{
    public enum Nilai { Kosong, Perfect, Good, Miss }
    public readonly NotArthur[] not;
    public readonly int[] hasil;
    public readonly float jendelaGood,jendelaPerfect;
    public readonly float durasiNot;
    public float[] durasiPerNot;
    float Durasi(int index)=>durasiPerNot!=null&&index>=0&&index<durasiPerNot.Length?durasiPerNot[index]:durasiNot;
    public int Perfect {get;private set;}
    public int Good {get;private set;}
    public int Miss {get;private set;}
    public int Salah {get;private set;}
    public int Combo {get;private set;}
    public int MaxCombo {get;private set;}
    public int Skor {get;private set;}
    public int Energi {get;private set;}=100;
    public int Terkena=>Perfect+Good;
    public float Akurasi=>not.Length==0?0:100f*(Perfect+Good*.65f)/not.Length;
    public float AkurasiSaatIni=>Perfect+Good+Miss==0?100:100f*(Perfect+Good*.65f)/(Perfect+Good+Miss);
    public ArthurRhythmRound(NotArthur[] chart,float perfect=.09f,float good=.22f,float visibleSeconds=0)
    {
        not=(NotArthur[])chart.Clone();hasil=new int[not.Length];jendelaGood=Math.Max(.02f,good);jendelaPerfect=Math.Min(jendelaGood,Math.Max(.01f,perfect));durasiNot=Math.Max(0,visibleSeconds);
    }
    public Nilai Tekan(int jalur,double waktu)
    {
        int pilih=-1;double jarak=jendelaGood+1e-6;
        for(int i=0;i<not.Length;i++)if(hasil[i]==0&&not[i].jalur==jalur)
        {
            double d=Math.Abs(not[i].waktu-waktu);if(d<jarak){jarak=d;pilih=i;}
        }
        return TekanNot(pilih,waktu);
    }
    public Nilai TekanNot(int pilih,double waktu)
    {
        bool visible=durasiNot>0&&pilih>=0&&pilih<not.Length&&waktu>=not[pilih].waktu&&waktu<=not[pilih].waktu+Durasi(pilih);
        if(pilih<0||pilih>=not.Length||hasil[pilih]!=0||(durasiNot>0?!visible:Math.Abs(not[pilih].waktu-waktu)>jendelaGood+1e-6))
        {Salah++;Combo=0;Energi=Math.Max(0,Energi-8);return Nilai.Kosong;}
        bool perfect=durasiNot>0||Math.Abs(not[pilih].waktu-waktu)<=jendelaPerfect+1e-6;hasil[pilih]=perfect?1:2;
        if(perfect)Perfect++;else Good++;
        Combo++;MaxCombo=Math.Max(MaxCombo,Combo);Skor+=(perfect?1000:650)+Math.Min(Combo,50)*5;Energi=Math.Min(100,Energi+2);
        return perfect?Nilai.Perfect:Nilai.Good;
    }
    public bool Lewatkan(double waktu)
    {
        bool miss=false;
        for(int i=0;i<not.Length;i++)if(hasil[i]==0&&waktu>not[i].waktu+(durasiNot>0?Durasi(i):jendelaGood))
        {hasil[i]=3;Miss++;Combo=0;Energi=Math.Max(0,Energi-12);miss=true;}
        return miss;
    }
    public bool Lulus(float minimum)=>Energi>0&&not.Length>0&&Terkena/(float)not.Length>=minimum;
}
