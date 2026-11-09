using UnityEngine;

public sealed class HewanKampus : MonoBehaviour
{
    public enum Perilaku { Diam, Jalan, Mengendus, Duduk, Mendekat, Menjauh, MembersihkanDiri }
    public Perilaku Keadaan { get; private set; }
    public bool Kucing { get; private set; }
    public bool Humanoid { get; private set; }
    public JalurKampus Jalur { get; private set; }
    public float Jarak { get; private set; }
    Transform visual,shadow;SpriteRenderer head,body,leftHand,rightHand,leftFoot,rightFoot,tail;
    ArthurJalurKampus arthur;KarakterController kontrol;
    float lateral,home,goal,next,phase,facing=1;bool ready;readonly System.Random random=new System.Random();
    public void Siapkan(bool cat,Sprite[] parts,Material material,JalurKampus route,float distance,float side,ArthurJalurKampus player,bool humanoid=false,Material shadowMaterial=null)
    {
        Kucing=cat;Humanoid=humanoid;Jalur=route;Jarak=home=goal=distance;lateral=side;arthur=player;kontrol=player.GetComponent<KarakterController>();
        visual=new GameObject("PotonganKertas").transform;visual.SetParent(transform,false);
        // Sort each paper animal as a whole, as Arthur's visual already does.
        // Individual paws/heads must never interleave with Arthur's parts.
        var group=visual.gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
        var playerGroup=player.GetComponentInChildren<UnityEngine.Rendering.SortingGroup>();
        if(playerGroup!=null){group.sortingLayerID=playerGroup.sortingLayerID;group.sortingOrder=playerGroup.sortingOrder;}
        head=Part("Kepala",parts[0],material,6);body=Part("Tubuh",parts[1],material,3);leftHand=Part("TanganKiri",parts[2],material,4);rightHand=Part("TanganKanan",parts[3],material,4);leftFoot=Part("KakiKiri",parts[4],material,2);rightFoot=Part("KakiKanan",parts[5],material,2);tail=Part("Ekor",parts[6],material,1);
        if(shadowMaterial!=null){var g=GameObject.CreatePrimitive(PrimitiveType.Capsule);g.name="BayanganKontak";g.transform.SetParent(transform,false);shadow=g.transform;shadow.localPosition=Vector3.up*(humanoid?.46f:cat?.28f:.35f);shadow.localRotation=Quaternion.Euler(0,0,humanoid?0:90);shadow.localScale=humanoid?new Vector3(.38f,.46f,.34f):cat?new Vector3(.32f,.48f,.32f):new Vector3(.4f,.60f,.4f);var c=g.GetComponent<Collider>();c.enabled=false;Destroy(c);var renderer=g.GetComponent<MeshRenderer>();renderer.sharedMaterial=shadowMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;renderer.receiveShadows=true;}
        ready=true;State(cat?Perilaku.Duduk:Perilaku.Mengendus);Position();Animate(0);
    }
    SpriteRenderer Part(string name,Sprite sprite,Material material,int order){var go=new GameObject(name);go.transform.SetParent(visual,false);var r=go.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.sortingOrder=order;return r;}
    void State(Perilaku s){Keadaan=s;next=Time.time+2+(float)random.NextDouble()*3;}
    void Update()
    {
        if(!ready||Jalur==null||arthur==null||Time.deltaTime<=0)return;
        if(KawasanKampus.Instance!=null&&KawasanKampus.Instance.SedangMemuat||DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif)){next+=Time.deltaTime;return;}
        float old=Jarak;var separation=transform.position-arthur.transform.position;separation.y=0;float distance=separation.magnitude;bool same=arthur.JalurAktif==Jalur;
        if(Kucing&&same&&distance<(kontrol.SedangBerlari?4:2.8f)){goal=Mathf.Clamp(Jarak+(Jarak>=arthur.JarakDiJalur?1:-1)*4.5f,.5f,Jalur.Panjang-.5f);State(Perilaku.Menjauh);}
        else if(!Kucing&&same&&distance<2.8f){goal=Mathf.Clamp(Jarak+(Jarak>=arthur.JarakDiJalur?1:-1)*4f,.5f,Jalur.Panjang-.5f);if(Keadaan!=Perilaku.Menjauh)State(Perilaku.Menjauh);}
        else if(!Kucing&&same&&distance<7&&distance>3.8f){goal=Mathf.Clamp(arthur.JarakDiJalur+(Jarak>=arthur.JarakDiJalur?1:-1)*3.6f,.5f,Jalur.Panjang-.5f);if(Keadaan!=Perilaku.Mendekat)State(Perilaku.Mendekat);}
        else if(!Kucing&&same&&distance<=3.8f&&Keadaan==Perilaku.Mendekat)State(Perilaku.Duduk);
        if(Time.time>next){if(random.Next(3)==0){goal=Mathf.Clamp(home+((float)random.NextDouble()*2-1)*3,.5f,Jalur.Panjang-.5f);State(Perilaku.Jalan);}else State(Kucing?(random.Next(2)==0?Perilaku.MembersihkanDiri:Perilaku.Duduk):Perilaku.Mengendus);}
        bool moving=Keadaan==Perilaku.Jalan||Keadaan==Perilaku.Mendekat||Keadaan==Perilaku.Menjauh;
        if(moving){Jarak=Mathf.MoveTowards(Jarak,goal,Time.deltaTime*(Keadaan==Perilaku.Menjauh?2.2f:Kucing?.5f:.8f));if(Mathf.Abs(Jarak-goal)<.03f)State(Kucing?Perilaku.Duduk:Perilaku.Mengendus);}
        Position();float speed=Mathf.Abs(Jarak-old)/Time.deltaTime;
        if(speed>.03f&&arthur.kamera!=null){Jalur.Sampel(Jarak,out _,out var tangent,out _);float sideways=Vector3.Dot(tangent*Mathf.Sign(Jarak-old),arthur.kamera.transform.right);if(Mathf.Abs(sideways)>.08f)facing=Mathf.Sign(sideways);}
        phase+=speed*Time.deltaTime*13;Animate(speed);
    }
    void Position(){
        Jalur.Sampel(Jarak,out var p,out var tangent,out _);var side=Vector3.Cross(Vector3.up,tangent).normalized;
        var target=p-Vector3.up*ArthurJalurKampus.JarakKaki+side*lateral;
        // Follow the shoulder's floor height rather than floating above a lower grass verge.
        if(Physics.Raycast(target+Vector3.up*.7f,Vector3.down,out var hit,4.5f,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)
            &&hit.normal.y>.65f&&!hit.collider.transform.IsChildOf(arthur.transform))target.y=hit.point.y+.015f;
        transform.position=target;
    }
    void Animate(float speed)
    {
        if(arthur.kamera!=null){visual.rotation=Quaternion.Euler(0,arthur.kamera.transform.eulerAngles.y,0);if(shadow!=null)shadow.rotation=visual.rotation*Quaternion.Euler(0,0,Humanoid?0:90);}
        if(!Humanoid){AnimateAnimal(speed);return;}
        float gait=speed>.03f?Mathf.Sin(phase):0,bob=Mathf.Abs(gait)*.025f;float size=Kucing?.86f:1;
        visual.localScale=new Vector3(size,size,size);float sit=Keadaan==Perilaku.Duduk?.1f:0;float sniff=Keadaan==Perilaku.Mengendus?.09f:0;
        Pose(head,new Vector3(0,.93f+bob-sit-sniff,-.01f),.55f,.43f,Keadaan==Perilaku.Mengendus?Mathf.Sin(Time.time*3)*7:Mathf.Sin(Time.time*.7f)*3);
        Pose(body,new Vector3(0,.51f+bob-sit,0),.32f,.40f,0);
        Pose(leftHand,new Vector3(-.25f,.50f+gait*.05f-sit,-.01f),.15f,.18f,gait*12);
        Pose(rightHand,new Vector3(.25f,.50f-gait*.05f-sit,-.01f),.15f,.18f,-gait*12);
        if(Keadaan==Perilaku.MembersihkanDiri)Pose(rightHand,new Vector3(.22f,.83f+Mathf.Sin(Time.time*7)*.035f,-.02f),.15f,.18f,-35);
        Pose(leftFoot,new Vector3(-.14f,.10f+Mathf.Max(0,gait)*.07f,0),.18f,.17f,gait*5);
        Pose(rightFoot,new Vector3(.14f,.10f+Mathf.Max(0,-gait)*.07f,0),.18f,.17f,-gait*5);
        float wag=Kucing?Mathf.Sin(Time.time*2)*8:Mathf.Sin(Time.time*8)*22;
        PoseTail(new Vector3(-.20f,.43f-sit+bob,.02f),.25f,.34f,55+wag*.55f,true);
    }
    void AnimateAnimal(float speed)
    {
        float size=Kucing?.80f:1;float gait=speed>.03f?Mathf.Sin(phase):0;float bob=Mathf.Abs(gait)*.018f;
        visual.localScale=new Vector3(size*facing,size,size);bool sit=Keadaan==Perilaku.Duduk||Keadaan==Perilaku.MembersihkanDiri;bool sniff=Keadaan==Perilaku.Mengendus;
        // The owner's cut-paper style intentionally has no knees: only short floating paws.
        Pose(body,new Vector3(0,(sit?.31f:.40f)+bob,0),.82f,sit?.29f:.34f,sit?-3:0);
        Pose(head,new Vector3(.70f,(sniff?.48f:sit?.57f:.64f)+bob,-.01f),.43f,.37f,sniff?-9:Mathf.Sin(Time.time*.7f)*1.5f);
        float legs=sit?.11f:.16f;
        float lift=.035f;
        Pose(leftHand,new Vector3(.28f-gait*.035f,.5f*legs+Mathf.Max(0,gait)*lift,-.015f),.16f,legs,gait*5);
        Pose(rightHand,new Vector3(.35f+gait*.035f,.5f*legs+Mathf.Max(0,-gait)*lift,.015f),.15f,legs,-gait*5);rightHand.sortingOrder=1;
        Pose(leftFoot,new Vector3(-.28f+gait*.035f,.5f*legs+Mathf.Max(0,-gait)*lift,-.01f),.18f,legs,-gait*5);leftFoot.sortingOrder=4;
        Pose(rightFoot,new Vector3(-.35f-gait*.035f,.5f*legs+Mathf.Max(0,gait)*lift,.02f),.17f,legs,gait*5);
        if(Keadaan==Perilaku.MembersihkanDiri)Pose(leftHand,new Vector3(.57f,.48f+Mathf.Sin(Time.time*7)*.02f,-.02f),.16f,.15f,-25);
        PoseTail(new Vector3(-.45f,(sit?.34f:.44f)+bob,.02f),.31f,.34f,Mathf.Sin(Time.time*(Kucing?2:7))*(Kucing?5:10),false);
    }
    void PoseTail(Vector3 attachment,float width,float height,float angle,bool flip)
    {
        tail.flipX=flip;Pose(tail,Vector3.zero,width,height,angle);
        var b=tail.sprite.bounds;var basePoint=new Vector3(b.min.x+b.size.x*(flip?.12f:.88f),b.min.y+b.size.y*.10f,0);
        if(flip)basePoint.x=-basePoint.x;
        // Rotate about the base, keeping the small paper-cut gap constant.
        tail.transform.localPosition=attachment-tail.transform.localRotation*Vector3.Scale(basePoint,tail.transform.localScale);
    }
    void Pose(SpriteRenderer r,Vector3 p,float w,float h,float angle){r.transform.localPosition=p;var size=r.sprite.bounds.size;r.transform.localScale=new Vector3(w/size.x,h/size.y,1);r.transform.localRotation=Quaternion.Euler(0,0,angle);}
}


