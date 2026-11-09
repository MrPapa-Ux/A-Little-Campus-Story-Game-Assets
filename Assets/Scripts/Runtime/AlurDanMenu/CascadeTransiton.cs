// PANDUAN PROYEK - Runtime / AlurDanMenu
// Fade layar untuk perpindahan; API lama dipertahankan bagi SistemPilihan.
using System;
using System.Collections;
using UnityEngine;

public class CascadeTransition : MonoBehaviour
{
    [Header("Referensi transisi yang sudah ada")]
    public UnityEngine.UI.Image[] blinds;
    [Header("Durasi transisi (detik)")]
    [Min(.05f)] public float durasiTutup=.38f;
    [Min(.02f)] public float jedaGelap=.12f;
    [Min(.05f)] public float durasiBuka=.48f;
    public bool SedangBerjalan {get;private set;}
    [Header("Tampilan dari LoadingScene")]
    public GameObject tampilanLoading;
    [Min(0)] public float durasiLoadingMinimal = .65f;
    public bool LoadingTerlihat => tampilanLoading != null && tampilanLoading.activeSelf;
    CanvasGroup lapisan;

    public void MulaiTransisiTeleport(Action aksiPerpindahan)
    {
        MulaiTransisiTeleportRutin(() => AksiLangsung(aksiPerpindahan));
    }
    static IEnumerator AksiLangsung(Action aksi) { aksi?.Invoke(); yield break; }
    public void MulaiTransisiTeleportRutin(Func<IEnumerator> perpindahan,bool mulaiGelap=false)
    {
        if(SedangBerjalan)return;
        // Use the existing Canvas and one existing image; no new UI hierarchy.
        var bidang=Array.Find(blinds??Array.Empty<UnityEngine.UI.Image>(),b=>b!=null);
        if(bidang==null){Debug.LogError("Referensi bidang transisi belum tersedia.",this);return;}
        var canvas=bidang.GetComponentInParent<Canvas>(true);
        if(canvas==null){Debug.LogError("Canvas transisi belum tersedia.",this);return;}
        SedangBerjalan=true;
        foreach(var blind in blinds)if(blind!=null)blind.enabled=blind==bidang;
        var rect=bidang.rectTransform;rect.SetParent(canvas.transform,false);
        rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,.5f);
        rect.offsetMin=rect.offsetMax=Vector2.zero;rect.localScale=Vector3.one;rect.localRotation=Quaternion.identity;
        bidang.sprite=null;bidang.type=UnityEngine.UI.Image.Type.Simple;bidang.color=Color.black;bidang.raycastTarget=true;
        canvas.sortingOrder=32760;
        lapisan=bidang.GetComponent<CanvasGroup>();if(lapisan==null)lapisan=bidang.gameObject.AddComponent<CanvasGroup>();
        lapisan.alpha=mulaiGelap?1:0;lapisan.blocksRaycasts=true;lapisan.interactable=false;
        gameObject.SetActive(true);canvas.gameObject.SetActive(true);bidang.gameObject.SetActive(true);
        StartCoroutine(TransisiMasukKeluar(perpindahan,mulaiGelap));
    }
    IEnumerator TransisiMasukKeluar(Func<IEnumerator> perpindahan,bool mulaiGelap)
    {
        if(!mulaiGelap)yield return Fade(0,1,durasiTutup);
        // Commit only after a fully opaque frame has actually been rendered.
        yield return null;
        float awalLoading=Time.realtimeSinceStartup;
        if(tampilanLoading!=null)tampilanLoading.SetActive(true);
        yield return null;
        IEnumerator rutin=null;
        try{rutin=perpindahan?.Invoke();}catch(Exception e){Debug.LogException(e,this);}
        if(rutin!=null)yield return JalankanAman(rutin);
        float sisaLoading=durasiLoadingMinimal-(Time.realtimeSinceStartup-awalLoading);
        if(sisaLoading>0)yield return new WaitForSecondsRealtime(sisaLoading);
        yield return new WaitForSecondsRealtime(Mathf.Max(.02f,jedaGelap));
        if(tampilanLoading!=null)tampilanLoading.SetActive(false);
        yield return Fade(1,0,durasiBuka);
        lapisan.blocksRaycasts=false;SedangBerjalan=false;gameObject.SetActive(false);
    }
    IEnumerator JalankanAman(IEnumerator rutin)
    {
        var tumpukan=new System.Collections.Generic.Stack<IEnumerator>();tumpukan.Push(rutin);
        while(tumpukan.Count>0)
        {
            var e=tumpukan.Peek();bool lanjut=false;object hasil=null;Exception gagal=null;
            try{lanjut=e.MoveNext();if(lanjut)hasil=e.Current;}catch(Exception ex){gagal=ex;}
            if(gagal!=null){Debug.LogException(gagal,this);break;}
            if(!lanjut){tumpukan.Pop();continue;}
            if(hasil is IEnumerator anak)tumpukan.Push(anak);else yield return hasil;
        }
    }
    IEnumerator Fade(float awal,float akhir,float durasi)
    {
        float waktu=0;
        while(waktu<Mathf.Max(.05f,durasi))
        {
            float t=Mathf.Clamp01(waktu/Mathf.Max(.05f,durasi));
            // Quintic easing: zero velocity and acceleration at either end.
            float halus=t*t*t*(t*(t*6-15)+10);
            lapisan.alpha=Mathf.Lerp(awal,akhir,halus);
            yield return null;waktu+=Time.unscaledDeltaTime;
        }
        lapisan.alpha=akhir;
    }
    void OnDisable()
    {
        StopAllCoroutines();SedangBerjalan=false;
        if(tampilanLoading!=null)tampilanLoading.SetActive(false);
        if(lapisan!=null){lapisan.alpha=0;lapisan.blocksRaycasts=false;}
    }
}
