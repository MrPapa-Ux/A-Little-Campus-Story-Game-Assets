using UnityEngine;

// Both landscape directions are allowed, including when Android's rotation lock is on.
public static class OrientasiAndroidArthur
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void Terapkan()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.orientation = ScreenOrientation.AutoRotation;
#endif
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void GunakanSensorLandscape()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var go = new GameObject("Sensor Landscape Android");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<PenjagaLandscapeAndroidArthur>();
#endif
    }
}

// Unity/Android dapat mengubah requestedOrientation setelah pergantian scene atau resume.
// Pertahankan SENSOR_LANDSCAPE, bukan mengunci arah pertama yang terbaca sensor.
public sealed class PenjagaLandscapeAndroidArthur : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject activity;
    float periksaBerikut;
    void Awake()
    {
        using(var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SetelahScene;
        AturSensor();
    }
    void AturSensor()
    {
        Screen.autorotateToPortrait = Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = Screen.autorotateToLandscapeRight = true;
        if(activity!=null && activity.Call<int>("getRequestedOrientation")!=6)
            activity.Call("setRequestedOrientation",6);
    }
    void Update()
    {
        if(Time.unscaledTime<periksaBerikut)return;
        periksaBerikut=Time.unscaledTime+.25f;
        AturSensor();
    }
    void SetelahScene(UnityEngine.SceneManagement.Scene scene,UnityEngine.SceneManagement.LoadSceneMode mode){AturSensor();}
    void OnApplicationFocus(bool fokus){if(fokus)AturSensor();}
    void OnApplicationPause(bool jeda){if(!jeda)AturSensor();}
    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded-=SetelahScene;
        activity?.Dispose();
    }
#endif
}
