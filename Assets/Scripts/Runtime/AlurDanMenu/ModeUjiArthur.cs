using UnityEngine;

// Only the separately identified MOD build activates these switches.
public static class ModeUjiArthur
{
#if ALCS_MOD
    public const bool Aktif = true;
#elif UNITY_EDITOR
    public static bool Aktif { get; set; }
#else
    public const bool Aktif = false;
#endif
    public static bool BekukanTimer;
    public static bool AbaikanBatas;
    public static bool LewatiKuis;
    public static bool MenuTerbuka;
    public static bool TimerDijeda => Aktif && (BekukanTimer || MenuTerbuka);
    public static bool BatasDibuka => Aktif && AbaikanBatas;
    public static bool KuisDilewati => Aktif && LewatiKuis;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSesi(){
#if UNITY_EDITOR && !ALCS_MOD
        Aktif=false;
#endif
        BekukanTimer=AbaikanBatas=LewatiKuis=MenuTerbuka=false;
    }
}
