using UnityEngine;

[DefaultExecutionOrder(270)]
public sealed class PerlengkapanHujanArthur : MonoBehaviour
{
    SpriteRenderer payung;
    Sprite spritePayung;
    Transform tangan, visual;
    AudioSource tanah, genteng;
    bool buka;
    float pembukaan;
    Vector3 genggamanNetral;

    public void Siapkan(Transform wadahVisual)
    {
        visual = wadahVisual;
        tanah = BuatSuara("HujanTanah");
        genteng = BuatSuara("HujanGentengLogam");
        if (visual == null) return;

        tangan = visual.Find("TanganKanan");
        genggamanNetral = tangan != null ? tangan.localPosition : new Vector3(1.41f, .36f, 0);
        var tekstur = Resources.Load<Texture2D>("ArthurSfx/PayungArthur");
        if (tekstur == null) return;

        spritePayung = Sprite.Create(tekstur, new Rect(0, 0, tekstur.width, tekstur.height),
            new Vector2(187f / 256f, 38f / 256f), 82, 0, SpriteMeshType.FullRect);
        var objek = new GameObject("Payung Arthur");
        objek.SetActive(false);
        objek.transform.SetParent(visual, false);
        objek.layer = visual.gameObject.layer;
        payung = objek.AddComponent<SpriteRenderer>();
        payung.sprite = spritePayung;
        var renderTangan = tangan != null ? tangan.GetComponent<SpriteRenderer>() : null;
        if (renderTangan != null)
        {
            // Jari tetap di depan gagang payung.
            payung.sortingLayerID = renderTangan.sortingLayerID;
            payung.sortingOrder = renderTangan.sortingOrder - 1;
        }
    }

    AudioSource BuatSuara(string klip)
    {
        var sumber = gameObject.AddComponent<AudioSource>();
        sumber.playOnAwake = false;
        sumber.loop = true;
        sumber.spatialBlend = 0;
        sumber.volume = 0;
        sumber.clip = Resources.Load<AudioClip>("ArthurSfx/" + klip);
        return sumber;
    }

    public void Atur(bool hujan, bool diLuar, bool tertutup, bool dialog)
    {
        buka = hujan && diLuar && !tertutup;
        if (JedaSaatTidakAktif.Dijeda) return;

        float volume = hujan && !tertutup ? (dialog ? .35f : 1f) : 0f;
        AturSuara(tanah, diLuar ? .52f * volume : 0);
        AturSuara(genteng, diLuar ? 0 : .42f * volume);
    }

    static void AturSuara(AudioSource sumber, float sasaran)
    {
        if (sumber.clip == null) return;
        sumber.volume = Mathf.MoveTowards(sumber.volume, sasaran, Time.unscaledDeltaTime * 1.65f);
        if (sasaran > 0 && !sumber.isPlaying) sumber.Play();
        if (sasaran == 0 && sumber.volume == 0 && sumber.isPlaying) sumber.Stop();
    }

    void LateUpdate()
    {
        if (payung == null || JedaSaatTidakAktif.Dijeda) return;
        if (!buka)
        {
            if (payung.gameObject.activeSelf) payung.gameObject.SetActive(false);
            pembukaan = 0;
            return;
        }

        pembukaan = Mathf.MoveTowards(pembukaan, 1, Time.deltaTime / .26f);
        float t = Mathf.SmoothStep(0, 1, pembukaan);
        var posisi = tangan != null ? tangan.localPosition : genggamanNetral;
        posisi.z += .02f;
        payung.transform.localPosition = posisi;
        payung.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-22, 0, t));
        payung.transform.localScale = new Vector3(Mathf.Lerp(.32f, 1f, t), Mathf.Lerp(.51f, 1.4f, t), 1);
        if (!payung.gameObject.activeSelf) payung.gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        if (payung != null) Destroy(payung.gameObject);
        if (spritePayung != null) Destroy(spritePayung);
    }
}
