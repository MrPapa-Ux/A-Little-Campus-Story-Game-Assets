using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Dua ketukan untuk menampilkan petunjuk, dua ketukan berikutnya untuk melewati.
public sealed class LewatiCutsceneArthur : MonoBehaviour
{
    public const float JarakKetukanCepat = .32f;
    public const float LamaPetunjuk = 2.5f;
    public bool PetunjukTerlihat { get; private set; }
    public bool SedangBerjalan { get; private set; }
    Action saatDilewati;
    TMP_Text label;
    RectTransform posisiLabel, batasAman;
    GameObject layarPetunjuk;
    float ketukanTerakhir = -10, munculSampai;
    Rect amanTerakhir;
    bool bertahap;
    float aksiBerikut=-10;
    static bool petunjukPembukaTerlihat;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetPetunjuk(){petunjukPembukaTerlihat=false;}

    public void Mulai(Action selesai, TMP_FontAsset font, bool perBagian=false)
    {
        if (label == null)
        {
            var go = new GameObject("Petunjuk Skip Cutscene", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            layarPetunjuk = go;
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, gameObject.scene);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 31000;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            batasAman = new GameObject("Batas aman", typeof(RectTransform)).GetComponent<RectTransform>();
            batasAman.SetParent(go.transform, false);
            posisiLabel = new GameObject("Skip", typeof(RectTransform)).GetComponent<RectTransform>();
            posisiLabel.SetParent(batasAman, false);
            posisiLabel.anchorMin = posisiLabel.anchorMax = new Vector2(1, 0);
            posisiLabel.pivot = new Vector2(1, 0); posisiLabel.sizeDelta = new Vector2(180, 58);
            label = posisiLabel.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = "Skip ->"; label.fontSize = 32; label.color = Color.white;
            label.alignment = TextAlignmentOptions.BottomRight; label.raycastTarget = false;
            // Root overlay: tidak ikut skala/rotasi panggung 3D atau panel cutscene.
            go.transform.localScale = Vector3.one;
        }
        if (font != null) label.font = font;
        saatDilewati = selesai; SedangBerjalan = true;
        bertahap=perBagian;aksiBerikut=-10;
        ketukanTerakhir = -10; PetunjukTerlihat = bertahap&&petunjukPembukaTerlihat; label.gameObject.SetActive(PetunjukTerlihat);
        AturAreaAman();
    }

    void AturAreaAman()
    {
        if (batasAman == null || Screen.width <= 0 || Screen.height <= 0) return;
        amanTerakhir = Screen.safeArea;
        batasAman.anchorMin = new Vector2(amanTerakhir.xMin / Screen.width, amanTerakhir.yMin / Screen.height);
        batasAman.anchorMax = new Vector2(amanTerakhir.xMax / Screen.width, amanTerakhir.yMax / Screen.height);
        batasAman.offsetMin = batasAman.offsetMax = Vector2.zero;
    }

    void Update()
    {
        if (!SedangBerjalan) return;
        if (amanTerakhir != Screen.safeArea) AturAreaAman();
        Perbarui(Time.unscaledTime);
        bool ketuk = false;
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
                if (Input.GetTouch(i).phase == TouchPhase.Began) { ketuk = true; break; }
        }
        else ketuk = Input.GetMouseButtonDown(0);
        if (ketuk) Ketuk(Time.unscaledTime);
    }

    public void Perbarui(float waktu)
    {
        if (!SedangBerjalan || !PetunjukTerlihat) return;
        if (!bertahap&&waktu >= munculSampai)
        {
            PetunjukTerlihat = false; ketukanTerakhir = -10;
            label.gameObject.SetActive(false); return;
        }
        posisiLabel.anchoredPosition = new Vector2(-36, 25 + Mathf.Sin(waktu * 5f) * (bertahap?2f:6f));
    }

    public void Ketuk(float waktu)
    {
        if (!SedangBerjalan) return;
        Perbarui(waktu);
        if(bertahap&&PetunjukTerlihat){
            if(waktu>=aksiBerikut){aksiBerikut=waktu+.25f;saatDilewati?.Invoke();}
            return;
        }
        bool cepat = waktu >= ketukanTerakhir && waktu - ketukanTerakhir <= JarakKetukanCepat;
        ketukanTerakhir = waktu;
        if (!cepat) return;
        ketukanTerakhir = -10;
        if (!PetunjukTerlihat)
        {
            PetunjukTerlihat = true; munculSampai = waktu + LamaPetunjuk;
            if(bertahap){petunjukPembukaTerlihat=true;aksiBerikut=waktu+.25f;}
            label.gameObject.SetActive(true); Perbarui(waktu);
        }
        else
        {
            var aksi = saatDilewati; Selesai(); aksi?.Invoke();
        }
    }

    public void Selesai()
    {
        SedangBerjalan = PetunjukTerlihat = false; saatDilewati = null; ketukanTerakhir = -10;
        if (label != null) label.gameObject.SetActive(false);
    }
    void OnDisable() => Selesai();
    void OnDestroy(){if(layarPetunjuk!=null)Destroy(layarPetunjuk);}
}
