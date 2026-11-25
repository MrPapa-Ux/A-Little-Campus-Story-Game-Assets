using UnityEngine;

// Fit the same complete 16:9 world composition to any viewport, just like the original menu video.
[RequireComponent(typeof(Camera))]
public sealed class BingkaiKameraTetap : MonoBehaviour
{
    public const float Rasio=16f/9f;
    [Range(1,150)] public float sudutAcuan=60;
    Camera kamera;int lebar=-1,tinggi=-1;
    void Awake(){kamera=GetComponent<Camera>();Atur();}
    void LateUpdate(){Atur();}
    void OnPreCull(){Atur();}
    public static Rect Area(int width,int height)=>new Rect(0,0,1,1);
    void Atur()
    {
        if(kamera==null)kamera=GetComponent<Camera>();
        if(lebar==Screen.width&&tinggi==Screen.height)return;
        lebar=Screen.width;tinggi=Screen.height;
        kamera.rect=new Rect(0,0,1,1);kamera.aspect=Rasio;
        kamera.fieldOfView=sudutAcuan;
    }
}
