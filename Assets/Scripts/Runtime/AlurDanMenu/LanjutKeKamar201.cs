using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class LanjutKeKamar201 : MonoBehaviour
{
    public string sceneKamar="Assets/Chapter1Preview/Chapter1_Kamar201.unity";
    bool memuat;
    public void Lanjut(){
        if(memuat)return;
        if(!Application.CanStreamedLevelBeLoaded(sceneKamar)){Debug.LogError("Scene kamar 201 belum tersedia.",this);return;}
        memuat=true;LoadingManager.sceneTujuan=sceneKamar;SceneManager.LoadScene("LoadingScene");
    }
}
