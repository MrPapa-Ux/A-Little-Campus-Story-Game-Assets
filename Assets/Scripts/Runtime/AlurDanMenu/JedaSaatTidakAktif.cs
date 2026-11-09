using UnityEngine;

// Freeze simulation and audio when the game loses focus. Never changes the user's volume.
public sealed class JedaSaatTidakAktif : MonoBehaviour
{
    public static bool Dijeda {get;private set;}
#if UNITY_EDITOR
    public static bool PengujianLatarBelakang {get;set;}
#endif
    float skalaSebelumnya;bool audioSebelumnya;
    readonly System.Collections.Generic.List<AudioSource> sumberDijeda=new System.Collections.Generic.List<AudioSource>();
    bool fokus=true,ditangguhkan;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset(){Dijeda=false;
#if UNITY_EDITOR
        PengujianLatarBelakang=false;
#endif
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Buat(){var go=new GameObject("Jeda saat tidak aktif");DontDestroyOnLoad(go);go.AddComponent<JedaSaatTidakAktif>();Application.runInBackground=false;}
    void OnApplicationFocus(bool value){fokus=value;Terapkan();}
    void OnApplicationPause(bool value){ditangguhkan=value;Terapkan();}
    void Terapkan(){
#if UNITY_EDITOR
        if(PengujianLatarBelakang){AturJeda(false);return;}
#endif
        AturJeda(!fokus||ditangguhkan);
    }
    public void AturJeda(bool value)
    {
        if(value==Dijeda)return;
        if(value)
        {
            skalaSebelumnya=Time.timeScale;audioSebelumnya=AudioListener.pause;Time.timeScale=0;
            sumberDijeda.Clear();foreach(var source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                if(source.isPlaying){sumberDijeda.Add(source);source.Pause();}
            AudioListener.pause=true;
        }
        else
        {
            Time.timeScale=skalaSebelumnya;AudioListener.pause=audioSebelumnya;
            foreach(var source in sumberDijeda)if(source!=null)source.UnPause();sumberDijeda.Clear();
        }
        Dijeda=value;
    }
    void OnDestroy(){if(Dijeda){Time.timeScale=skalaSebelumnya;AudioListener.pause=audioSebelumnya;Dijeda=false;}}
}
