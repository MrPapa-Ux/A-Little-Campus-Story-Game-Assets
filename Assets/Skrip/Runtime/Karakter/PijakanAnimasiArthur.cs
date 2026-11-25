using UnityEngine;

// Receives contacts from the existing running clip, on the Animator's GameObject.
public sealed class PijakanAnimasiArthur : MonoBehaviour
{
    public KarakterController pemilik;
    public void PijakanLari(){if(pemilik!=null)pemilik.PijakanLari();}
}
