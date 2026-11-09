// PANDUAN PROYEK - Runtime / MiniGame/Rhythm
// Fungsi: Membuat AudioClip beat latihan lewat kode; kelas bantuan statis.
// Panduan: Assets/Scripts/Runtime/MiniGame/Rhythm/README.md

using System;
using UnityEngine;

public static class ArthurRhythmAudio
{
    // Beat latihan instrumental dengan tempo pasti; bukan beatmap otomatis lagu playlist.
    public static AudioClip Buat(float bpm,int beats,float awal)
    {
        const int rate=22050;double beat=60.0/bpm;int count=(int)((awal+beats*beat+1)*rate);var data=new float[count];
        var campus=Resources.Load<AudioClip>("ArthurSfx/RhythmCampus");
        if(campus!=null&&campus.LoadAudioData())
        {
            var src=new float[campus.samples*campus.channels];campus.GetData(src,0);
            for(int i=(int)(awal*rate);i<count;i++)
            {
                double t=i/(double)rate-awal;if(t>=beats*beat)break;
                double pos=Math.Min(campus.samples-1,t/(40*beat)*(campus.samples-1));int a=(int)pos,b=Math.Min(a+1,campus.samples-1);float f=(float)(pos-a),sample=0;
                for(int ch=0;ch<campus.channels;ch++)sample+=Mathf.Lerp(src[a*campus.channels+ch],src[b*campus.channels+ch],f)/campus.channels;
                data[i]=sample*Mathf.Clamp01((float)Math.Min(t*50,(beats*beat-t)*30));
            }
            var derived=AudioClip.Create("Jam Pulang - game instrumental",count,1,rate,false);derived.SetData(data,0);return derived;
        }
        double[] nada={261.6256,329.6276,391.9954,440,349.2282,391.9954,329.6276,293.6648};uint noise=9187;
        for(int i=(int)(awal*rate);i<count;i++)
        {
            double t=i/(double)rate-awal,b=t/beat,p=t%beat,half=t%(beat/2);int k=(int)b;
            if(k>=beats)continue;
            double kick=p<.16?.24*Math.Sin(2*Math.PI*(55*p+9*(1-Math.Exp(-p*28))))*Math.Exp(-p*22):0;
            noise=unchecked(noise*1664525+1013904223);double n=((noise>>8)/16777216.0*2-1);
            double hat=half<.04?n*.035*Math.Exp(-half*95):0;
            double snare=k%4==1||k%4==3?n*.09*Math.Exp(-p*40):0;
            double note=nada[k%8],keys=.10*Math.Sin(2*Math.PI*note*t)*Math.Exp(-p*6)+.035*Math.Sin(2*Math.PI*note*2*t)*Math.Exp(-p*10);
            double end=Math.Min(1,(beats*beat-t)*3);data[i]=(float)((kick+hat+snare+keys)*Math.Max(0,end));
        }
        var clip=AudioClip.Create("Campus Rhythm",count,1,rate,false);clip.SetData(data,0);return clip;
    }
}
