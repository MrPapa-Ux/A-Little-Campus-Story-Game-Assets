using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Zoom lensa menjaga kamera di lorong aman; tidak mendekatkannya menembus bangunan.
public sealed class ZoomKameraArthur : MonoBehaviour
{
    public const float BatasDekat = .68f;
    public float Faktor => faktor;
    ArthurJalurKampus arthur;
    KarakterController kontrol;
    SistemHP hp;
    float faktor=1,halus=1,fovDasar,fovTerapan,jarakLalu;
    int jariA=-1,jariB=-1;
    bool terapan,mencubit,tahanSampaiLepas;
    readonly List<RaycastResult> hasilUI=new List<RaycastResult>();
    public void Siapkan(ArthurJalurKampus pemilik){arthur=pemilik;kontrol=GetComponent<KarakterController>();hp=FindFirstObjectByType<SistemHP>();}
    bool Boleh => arthur!=null&&arthur.kamera!=null&&!arthur.KameraSinematikAktif
        &&kontrol!=null&&!kontrol.kunciPergerakan&&Time.timeScale>0
        &&!(hp!=null&&hp.MenjedaParameter)
        &&!(DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif))
        &&!(KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat);
    bool DiTombol(Touch sentuh)
    {
        if(EventSystem.current==null)return false;
        hasilUI.Clear();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=sentuh.position},hasilUI);
        foreach(var hasil in hasilUI)if(hasil.gameObject.GetComponentInParent<Selectable>()!=null)return true;
        return false;
    }
    public static float FaktorSetelahCubitan(float sekarang,float jarakSebelum,float jarakBaru)
        =>jarakSebelum<1||jarakBaru<1?sekarang:Mathf.Clamp(sekarang*jarakSebelum/jarakBaru,BatasDekat,1);
    void Update()
    {
        if(Input.touchCount==0){tahanSampaiLepas=false;mencubit=false;return;}
        if(Input.touchCount!=2||!Boleh){mencubit=false;return;}
        var a=Input.GetTouch(0);var b=Input.GetTouch(1);
        if(tahanSampaiLepas)return;
        if(a.phase==TouchPhase.Ended||a.phase==TouchPhase.Canceled||b.phase==TouchPhase.Ended||b.phase==TouchPhase.Canceled){mencubit=false;return;}
        float jarak=Vector2.Distance(a.position,b.position);
        if(!mencubit||jariA!=a.fingerId||jariB!=b.fingerId)
        {
            if(DiTombol(a)||DiTombol(b)){tahanSampaiLepas=true;return;}
            jariA=a.fingerId;jariB=b.fingerId;jarakLalu=jarak;mencubit=true;kontrol.BerhentiBergerak();return;
        }
        faktor=FaktorSetelahCubitan(faktor,jarakLalu,jarak);jarakLalu=jarak;
    }
    public void PulihkanKamera()
    {
        if(terapan&&arthur!=null&&arthur.kamera!=null&&Mathf.Abs(arthur.kamera.fieldOfView-fovTerapan)<.01f)arthur.kamera.fieldOfView=fovDasar;
        terapan=false;
    }
    public void TerapkanKamera()
    {
        if(arthur==null||arthur.kamera==null)return;
        fovDasar=arthur.kamera.fieldOfView;
        if(!Boleh){halus=1;return;}
        halus=Mathf.Lerp(halus,faktor,1-Mathf.Exp(-14*Time.unscaledDeltaTime));
        fovTerapan=Mathf.Clamp(2*Mathf.Atan(Mathf.Tan(fovDasar*Mathf.Deg2Rad*.5f)*halus)*Mathf.Rad2Deg,Mathf.Min(30,fovDasar),fovDasar);
        arthur.kamera.fieldOfView=fovTerapan;terapan=true;
    }
    void OnDisable(){PulihkanKamera();mencubit=false;}
}
