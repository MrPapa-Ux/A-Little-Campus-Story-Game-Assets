using UnityEditor;
using UnityEngine;

// Editor keeps rendering unscaled animations when timeScale is zero. Pause its player loop too.
[InitializeOnLoad]
public static class JedaEditorSaatTidakAktif
{
    const string Key="Campus.Editor.AutoPausedForFocus";
    static JedaEditorSaatTidakAktif(){EditorApplication.update+=Update;}
    static void Update()
    {
        if(!EditorApplication.isPlaying){if(SessionState.GetBool(Key,false))EditorApplication.isPaused=false;SessionState.SetBool(Key,false);return;}
        if(SessionState.GetBool(CampusCheckpoint19Checks.Key,false)||SessionState.GetBool("Campus.Revision20.Check",false)||SessionState.GetBool("Campus.Revision21.Check",false)||SessionState.GetBool("Campus.Revision22.Check",false)||SessionState.GetBool("Campus.Revision23.Check",false)||SessionState.GetBool("Campus.Revision26.Check",false)||SessionState.GetBool("Campus.Revision27.Check",false)||JedaSaatTidakAktif.PengujianLatarBelakang)return;
        bool active=UnityEditorInternal.InternalEditorUtility.isApplicationActive&&Application.isFocused;
        Terapkan(active);
    }
    public static void Terapkan(bool active)
    {
        var focus=Object.FindFirstObjectByType<JedaSaatTidakAktif>();
        if(!active&&!EditorApplication.isPaused){if(focus!=null)focus.AturJeda(true);SessionState.SetBool(Key,true);EditorApplication.isPaused=true;}
        else if(active&&SessionState.GetBool(Key,false)){if(focus!=null)focus.AturJeda(false);SessionState.SetBool(Key,false);EditorApplication.isPaused=false;}
    }
}
