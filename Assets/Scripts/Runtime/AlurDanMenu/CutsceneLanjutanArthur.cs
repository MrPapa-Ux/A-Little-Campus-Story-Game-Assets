using System.Collections;
using UnityEngine;

// Memakai pemutar, font, safe area dan Skip yang sama dengan pembuka/Dining.
public static class CutsceneLanjutanArthur
{
    public static IEnumerator Mainkan(GameObject prefab, CeritaKamar201Arthur kamar)
    {
        if(prefab==null||kamar==null)yield break;
        var dialog=DialogManager.Instance;
        bool kunci=kamar.pemain.kunciPergerakan;
        bool mini=dialog!=null&&dialog.minigameAktif;
        bool jeda=kamar.procrastination!=null&&kamar.procrastination.JedaKhususCerita;
        GameObject go=null;
        try {
            kamar.pemain.BerhentiBergerak();kamar.pemain.kunciPergerakan=true;
            if(dialog!=null)dialog.minigameAktif=true;
            if(kamar.procrastination!=null)kamar.procrastination.JedaKhususCerita=true;
            go=Object.Instantiate(prefab);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,kamar.gameObject.scene);
            yield return go.GetComponent<KenanganPembukaArthur>().Mainkan();
        } finally {
            if(go!=null){go.SetActive(false);Object.Destroy(go);}
            if(kamar!=null&&kamar.pemain!=null)kamar.pemain.kunciPergerakan=kunci;
            if(dialog!=null)dialog.minigameAktif=mini;
            if(kamar!=null&&kamar.procrastination!=null)kamar.procrastination.JedaKhususCerita=jeda;
        }
    }
}
