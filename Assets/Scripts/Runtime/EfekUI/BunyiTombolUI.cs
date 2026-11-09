// PANDUAN PROYEK - Runtime / EfekUI
// Fungsi: Memutar bunyi tombol kertas-piano dan menghubungkan satu pemutar ke tombol UI.
// Panduan: Assets/Scripts/Runtime/EfekUI/README.md

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class BunyiTombolUI : MonoBehaviour
{
    public enum Varian
    {
        KertasLembut = 0,
        PianoLipatan = 1,
        KetikKertas = 2
    }

    private const string KunciVarian = "Campus.UI.ClickSoundVariant";
    private static BunyiTombolUI instance;
    private static readonly HashSet<Button> tombolTerhubung = new HashSet<Button>();
    private readonly List<Button> tombolMilik = new List<Button>();
    private AudioSource pemutar;
    private AudioSource pemutarHP;
    private AudioClip[] bunyi;
    private AudioClip bukaHP, ketukHP;
    public static int JumlahBukaHP { get; private set; }
    public static int JumlahKetukHP { get; private set; }
    public static int JumlahUmum { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatik()
    {
        instance = null;
        tombolTerhubung.Clear();
        JumlahBukaHP = JumlahKetukHP = JumlahUmum = 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void BuatPemutar()
    {
        if (instance != null) return;
        var objek = new GameObject("Suara Tombol UI");
        DontDestroyOnLoad(objek);
        instance = objek.AddComponent<BunyiTombolUI>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        pemutar = gameObject.AddComponent<AudioSource>();
        pemutar.playOnAwake = false;
        pemutar.loop = false;
        pemutar.spatialBlend = 0f;
        pemutar.volume = .22f;
        pemutar.priority = 32;
        pemutarHP=gameObject.AddComponent<AudioSource>();
        pemutarHP.playOnAwake=false;pemutarHP.loop=false;pemutarHP.spatialBlend=0;
        pemutarHP.volume=.65f;pemutarHP.priority=24;
        bunyi = new[]
        {
            Resources.Load<AudioClip>("ButtonSfx/KertasLembut"),
            Resources.Load<AudioClip>("ButtonSfx/PianoLipatan"),
            Resources.Load<AudioClip>("ButtonSfx/KetikKertas")
        };
        bukaHP = Resources.Load<AudioClip>("ArthurSfx/BukaHP");
        ketukHP = Resources.Load<AudioClip>("ArthurSfx/KetukHP");
        foreach(var clip in bunyi)if(clip!=null)clip.LoadAudioData();
        if(bukaHP!=null)bukaHP.LoadAudioData();
        if(ketukHP!=null)ketukHP.LoadAudioData();
        SceneManager.sceneLoaded += SaatSceneDimuat;
        SceneManager.sceneUnloaded += SaatSceneDilepas;
        SambungkanSemuaTombol();
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        SceneManager.sceneLoaded -= SaatSceneDimuat;
        SceneManager.sceneUnloaded -= SaatSceneDilepas;
        foreach (Button button in tombolMilik)
            if (button != null) button.onClick.RemoveListener(Mainkan);
        tombolMilik.Clear();
        tombolTerhubung.Clear();
        instance = null;
    }

    private void SaatSceneDimuat(Scene scene, LoadSceneMode mode) => SambungkanSemuaTombol();

    private void SaatSceneDilepas(Scene scene)
    {
        for (int i = tombolMilik.Count - 1; i >= 0; i--)
        {
            Button button = tombolMilik[i];
            if (button != null && button.gameObject.scene != scene) continue;
            if (button != null) button.onClick.RemoveListener(Mainkan);
            tombolTerhubung.Remove(button);
            tombolMilik.RemoveAt(i);
        }
    }

    private void SambungkanSemuaTombol()
    {
        Button[] semua = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in semua)
        {
            if (button == null || button.GetComponentInParent<RhythmJalurInput>(true) != null) continue;
            if (KontrolArthurAtauHP(button)) continue;
            if (!tombolTerhubung.Add(button)) continue;
            button.onClick.AddListener(Mainkan);
            tombolMilik.Add(button);
        }
    }

    private static bool KontrolArthurAtauHP(Button button)
    {
        if (button.name == "Btn_Kiri" || button.name == "Btn_Kanan" || button.name == "Btn_Interact") return true;
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            string metode = button.onClick.GetPersistentMethodName(i);
            if (metode.StartsWith("TekanTombolUI") || metode.StartsWith("BukaHP") ||
                metode == "JalanKeKiri" || metode == "JalanKeKanan") return true;
        }
        return false;
    }

    public static void MainkanBukaHP()
    {
        if (instance != null && instance.bukaHP != null)
        { instance.pemutarHP.PlayOneShot(instance.bukaHP, .85f); JumlahBukaHP++; }
    }

    public static void MainkanKetukHP()
    {
        if (instance != null && instance.ketukHP != null)
        { instance.pemutarHP.PlayOneShot(instance.ketukHP, .75f); JumlahKetukHP++; }
    }

    private void Mainkan()
    {
        if (bunyi == null || bunyi.Length == 0 || pemutar == null) return;
        int pilihan = Mathf.Clamp(PlayerPrefs.GetInt(KunciVarian, (int)Varian.KertasLembut), 0, bunyi.Length - 1);
        if (bunyi[pilihan] != null) { pemutar.PlayOneShot(bunyi[pilihan]); JumlahUmum++; }
    }

    public static void PilihVarian(Varian pilihan)
    {
        PlayerPrefs.SetInt(KunciVarian, Mathf.Clamp((int)pilihan, 0, 2));
        PlayerPrefs.Save();
    }
}
