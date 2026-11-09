using UnityEngine;

public sealed class KupuKupuKampus : MonoBehaviour
{
    public Transform sayapKiri, sayapKanan;
    public float radiusTerbang = 1.5f, kecepatan = .65f, fase;
    Vector3 pusat;
    ArthurJalurKampus arthur;
    Renderer[] gambar;bool dekatSebelumnya=true;
    void Start() { pusat = transform.localPosition; arthur = FindFirstObjectByType<ArthurJalurKampus>();gambar=GetComponentsInChildren<Renderer>(); }
    void Update()
    {
        bool dekat=arthur==null||(arthur.transform.position-transform.position).sqrMagnitude<=1600;
        if(dekat!=dekatSebelumnya){foreach(var r in gambar)if(r!=null)r.enabled=dekat;dekatSebelumnya=dekat;}
        if(!dekat)return;
        float t = Time.time * kecepatan + fase;
        transform.localPosition = pusat + new Vector3(Mathf.Sin(t) * radiusTerbang,
            Mathf.Sin(t * 2.3f) * .22f, Mathf.Sin(t * 1.71f) * radiusTerbang * .65f);
        Vector3 arah = new Vector3(Mathf.Cos(t), .08f * Mathf.Cos(t * 2.3f), 1.11f * Mathf.Cos(t * 1.71f));
        transform.localRotation = Quaternion.LookRotation(arah, Vector3.up);
        float flutter = 18 + 55 * (.5f + .5f * Mathf.Sin(Time.time * 26 + fase * 5));
        if (sayapKiri != null) sayapKiri.localRotation = Quaternion.Euler(0, 0, flutter);
        if (sayapKanan != null) sayapKanan.localRotation = Quaternion.Euler(0, 0, -flutter);
    }
}
