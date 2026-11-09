using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(KarakterController),typeof(NPCJogetGoofy))]
public sealed class KetukKepalaArthur : MonoBehaviour
{
    public SpriteRenderer kepala;
    public bool SedangJoget {get;private set;}
    KarakterController arthur;NPCJogetGoofy joget;SistemHP hp;CascadeTransition transisi;
    float ketukTerakhir=-10,selesai;bool animatorAktif;
    EkspresiWajah2D wajah;bool ekspresiSebelumnyaAktif;EmosiKarakter ekspresiSebelumnya;
    void Start()
    {
        arthur=GetComponent<KarakterController>();joget=GetComponent<NPCJogetGoofy>();joget.enabled=false;
        wajah=GetComponentInChildren<EkspresiWajah2D>(true);
        hp=FindFirstObjectByType<SistemHP>();transisi=FindFirstObjectByType<DialogManager>().GetComponent<SistemPilihan>().efekTransisi;
    }
    bool AdaInteraksi()=> (DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif))||(hp!=null&&hp.hpSedangBuka)||(transisi!=null&&transisi.SedangBerjalan);
    void Update()
    {
        if(SedangJoget)
        {
            if(AdaInteraksi()||Time.time>=selesai)Berhenti();
            return;
        }
        if(Time.timeScale==0||AdaInteraksi()||arthur.kunciPergerakan){ketukTerakhir=-10;return;}
#if ENABLE_INPUT_SYSTEM
        if(Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            Ketuk(Touchscreen.current.primaryTouch.position.ReadValue(),Touchscreen.current.primaryTouch.touchId.ReadValue());
        else if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)Ketuk(Mouse.current.position.ReadValue());
#else
        if(Input.touchCount>0){foreach(var t in Input.touches)if(t.phase==TouchPhase.Began)Ketuk(t.position,t.fingerId);}
        else if(Input.GetMouseButtonDown(0))Ketuk(Input.mousePosition);
#endif
    }
    public void Ketuk(Vector2 posisi,int pointer=-1)
    {
        if(!isActiveAndEnabled||arthur==null||kepala==null||Time.timeScale==0||SedangJoget||AdaInteraksi()||arthur.kunciPergerakan)return;
        if(EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject(pointer))return;
        var camera=arthur.GetComponentInChildren<Camera>();if(camera==null)return;
        var bounds=kepala.bounds;Vector2 min=Vector2.positiveInfinity,max=Vector2.negativeInfinity;
        for(int i=0;i<8;i++)
        {
            var point=new Vector3((i&1)==0?bounds.min.x:bounds.max.x,(i&2)==0?bounds.min.y:bounds.max.y,(i&4)==0?bounds.min.z:bounds.max.z);
            var screen=camera.WorldToScreenPoint(point);if(screen.z<=0)return;
            min=Vector2.Min(min,screen);max=Vector2.Max(max,screen);
        }
        if(!new Rect(min,max-min).Contains(posisi)){ketukTerakhir=-10;return;}
        if(Time.time-ketukTerakhir<=.3f)
        {
            ketukTerakhir=-10;arthur.BerhentiBergerak();arthur.kunciPergerakan=true;
            animatorAktif=arthur.animator!=null&&arthur.animator.enabled;
            if(arthur.animator!=null)arthur.animator.enabled=false;
            if(wajah!=null)
            {
                ekspresiSebelumnyaAktif=wajah.gunakanEkspresi;ekspresiSebelumnya=wajah.ekspresi;
                wajah.gunakanEkspresi=true;wajah.ekspresi=EmosiKarakter.Senang;
            }
            joget.SimpanPoseDasar();joget.sedangJoget=true;joget.enabled=true;SedangJoget=true;selesai=Time.time+2f;
        }
        else ketukTerakhir=Time.time;
    }
    void Berhenti()
    {
        if(!SedangJoget)return;
        joget.enabled=false;SedangJoget=false;
        if(wajah!=null){wajah.gunakanEkspresi=ekspresiSebelumnyaAktif;wajah.ekspresi=ekspresiSebelumnya;}
        if(arthur.animator!=null){arthur.animator.enabled=animatorAktif;if(animatorAktif)arthur.animator.Update(0);}
        if(!AdaInteraksi())arthur.kunciPergerakan=false;
        ketukTerakhir=-10;
    }
    void OnDisable(){Berhenti();ketukTerakhir=-10;}
}
