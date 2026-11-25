using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(350)]
public sealed class ArthurJalanVertikal : MonoBehaviour
{
    public Transform visualLama, rig;
    public SpriteRenderer kepalaSumber;
    public SpriteRenderer kepala, rambut, badan, tanganKiri, tanganKanan, kakiKiri, kakiKanan, telingaKiri, telingaKanan;
    public Sprite kepalaBelakang, badanBelakang, tanganBelakang, sepatuBelakangKiri, sepatuBelakangKanan;
    public Material bahanBelakang;
    public bool SedangVertikal { get; private set; }
    public bool MembelakangiKamera { get; private set; }
    public float FaseLangkah { get; private set; }
    ArthurJalurKampus jalur;KarakterController kontrol;KetukKepalaArthur ketuk;SistemHP hp;
    readonly List<Renderer> originals=new List<Renderer>();
    readonly Dictionary<Renderer,bool> hidden=new Dictionary<Renderer,bool>();
    MaterialPropertyBlock block;Vector3 last;bool started;float idle;
    Sprite badanDepan,tanganDepanKiri,tanganDepanKanan,sepatuDepanKiri,sepatuDepanKanan;Material bahanDepan;
    void Start(){jalur=GetComponent<ArthurJalurKampus>();kontrol=GetComponent<KarakterController>();ketuk=GetComponent<KetukKepalaArthur>();hp=FindFirstObjectByType<SistemHP>();block=new MaterialPropertyBlock();last=transform.position;started=true;badanDepan=badan.sprite;tanganDepanKiri=tanganKiri.sprite;tanganDepanKanan=tanganKanan.sprite;sepatuDepanKiri=kakiKiri.sprite;sepatuDepanKanan=kakiKanan.sprite;bahanDepan=kepala.sharedMaterial;if(rig!=null)rig.gameObject.SetActive(false);}
    void LateUpdate()
    {
        if(!started||jalur==null||jalur.kamera==null||rig==null)return;
        Vector3 movement=transform.position-last;last=transform.position;movement.y=0;
        bool depth=false;
        if(jalur.JalurAktif!=null){jalur.JalurAktif.Sampel(jalur.JarakDiJalur,out _,out var tangent,out _);tangent.y=0;var forward=jalur.kamera.transform.forward;forward.y=0;depth=Mathf.Abs(Vector3.Dot(tangent.normalized,forward.normalized))>(SedangVertikal?.55f:.72f);}
        depth&=!(ketuk!=null&&ketuk.SedangJoget)&&!(hp!=null&&hp.hpSedangBuka);
        if(depth&&!SedangVertikal)idle=0;
        SedangVertikal=depth;rig.gameObject.SetActive(depth);
        if(!depth){Restore();return;}
        visualLama.GetComponentsInChildren(true,originals);
        foreach(var r in originals){if(!hidden.ContainsKey(r))hidden[r]=r.forceRenderingOff;r.forceRenderingOff=true;}
        bool walking=movement.sqrMagnitude>.000001f&&movement.magnitude<.6f&&Time.deltaTime>0&&jalur.KecepatanAktual>.05f;
        if(walking){var forward=jalur.kamera.transform.forward;forward.y=0;MembelakangiKamera=Vector3.Dot(movement,forward)>0;FaseLangkah+=movement.magnitude*(kontrol.SedangBerlari?5.3f:4.5f);idle=0;}
        else idle+=Time.deltaTime;
        float amount=walking?1:Mathf.Exp(-idle*12);float phase=FaseLangkah;
        float step=Mathf.Sin(phase)*amount, left=Mathf.Max(0,step),right=Mathf.Max(0,-step);
        // Contact -> down -> passing -> up, with one paw always supporting the body.
        // Keep this small for Arthur's deliberately detached cut-paper limbs.
        float bounce=(Mathf.Cos(phase*2)*.014f)*amount;
        bool back=MembelakangiKamera&&kepalaBelakang!=null;
        kepala.sprite=back?kepalaBelakang:kepalaSumber.sprite;badan.sprite=back?badanBelakang:badanDepan;
        tanganKiri.sprite=back?tanganBelakang:tanganDepanKiri;tanganKanan.sprite=back?tanganBelakang:tanganDepanKanan;
        kakiKiri.sprite=back?sepatuBelakangKiri:sepatuDepanKiri;kakiKanan.sprite=back?sepatuBelakangKanan:sepatuDepanKanan;
        var mat=back?bahanBelakang:bahanDepan;
        kepala.sharedMaterial=badan.sharedMaterial=tanganKiri.sharedMaterial=tanganKanan.sharedMaterial=kakiKiri.sharedMaterial=kakiKanan.sharedMaterial=mat;
        rambut.enabled=telingaKiri.enabled=telingaKanan.enabled=!back;
        tanganKiri.flipX=back;tanganKanan.flipX=false;
        Pose(kepala,new Vector3(0,.05f+bounce,0),back?.72f:.61f,back?.68f:.62f,0,0);
        Pose(rambut,new Vector3(-.02f,.25f+bounce,-.01f),.72f,.50f,step*.5f,0);
        Pose(badan,new Vector3(0,-.59f+bounce,0),back?.48f:.48f,.65f,step*.6f,0);
        Pose(telingaKiri,new Vector3(-.285f,.015f+bounce,.01f),.12f,.17f,0,0);telingaKiri.flipX=true;
        Pose(telingaKanan,new Vector3(.285f,.015f+bounce,.01f),.12f,.17f,0,0);
        Pose(tanganKiri,new Vector3(-.42f,-.67f-step*.045f,0),back?.20f:.24f,back?.29f:.34f,step*3,0);
        Pose(tanganKanan,new Vector3(.42f,-.67f+step*.045f,0),back?.20f:.24f,back?.29f:.34f,-step*3,0);
        float nearLeft=(back?-step:step)*.018f;
        Pose(kakiKiri,new Vector3(-.21f,-1.535f+left*.075f,0),(back?.29f:.37f)+nearLeft,.25f+nearLeft,-step*2,0);kakiKiri.flipX=!back;
        Pose(kakiKanan,new Vector3(.21f,-1.535f+right*.075f,0),(back?.29f:.37f)-nearLeft,.25f-nearLeft,step*2,0);kakiKanan.flipX=false;
    }
    void Pose(SpriteRenderer r,Vector3 p,float width,float height,float angle,int mode)
    {
        if(r==null||r.sprite==null)return;r.transform.localPosition=p;var b=r.sprite.bounds.size;r.transform.localScale=new Vector3(width/Mathf.Max(.001f,b.x),height/Mathf.Max(.001f,b.y),1);r.transform.localRotation=Quaternion.Euler(0,0,angle);
        r.GetPropertyBlock(block);block.SetFloat("_Mode",mode);var rect=r.sprite.rect;var texture=r.sprite.texture;block.SetVector("_Rect",new Vector4(rect.x/texture.width,rect.y/texture.height,rect.width/texture.width,rect.height/texture.height));r.SetPropertyBlock(block);
    }
    void Restore(){foreach(var pair in hidden)if(pair.Key!=null)pair.Key.forceRenderingOff=pair.Value;hidden.Clear();}
    void OnDisable(){Restore();if(rig!=null)rig.gameObject.SetActive(false);SedangVertikal=false;}
}
