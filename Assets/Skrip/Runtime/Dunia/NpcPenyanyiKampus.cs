using UnityEngine;

[RequireComponent(typeof(NpcYangBisaDiatur))]
[AddComponentMenu("Arthur/NPC penyanyi kampus")]
public sealed class NpcPenyanyiKampus : MonoBehaviour
{
    public AudioClip senandung;
    [Min(.5f)] public float jarakMulai = 6;
    [Min(.5f)] public float jarakPenuh = 1.5f;
    [Range(0, 1)] public float volumeNyanyian = .48f;
    public AudioSource sumber;
    public bool SedangBernyanyi => sumber != null && sumber.isPlaying && sumber.volume > .001f;
    ArthurJalurKampus arthur;
    NpcYangBisaDiatur npc;
    float sasaran, cekBerikut;
    bool dijeda;

    void Awake()
    {
        npc = GetComponent<NpcYangBisaDiatur>();
        arthur = FindFirstObjectByType<ArthurJalurKampus>();
        if (sumber == null) sumber = gameObject.AddComponent<AudioSource>();
        sumber.playOnAwake = false;
        sumber.loop = true;
        // Jarak dihitung dari Arthur; kamera permainan berada jauh di belakangnya.
        sumber.spatialBlend = 0;
        sumber.dopplerLevel = 0;
        sumber.volume = 0;
        sumber.clip = senandung;
        sumber.Stop();
    }

    void Update()
    {
        if (sumber == null || senandung == null) return;
        if (JedaSaatTidakAktif.Dijeda)
        {
            if (!dijeda) { sumber.Pause(); dijeda = true; }
            return;
        }
        if (dijeda) { sumber.UnPause(); dijeda = false; }
        if (Time.unscaledTime >= cekBerikut)
        {
            cekBerikut = Time.unscaledTime + .1f;
            if (arthur == null) arthur = FindFirstObjectByType<ArthurJalurKampus>();
            var dialog = DialogManager.Instance;
            bool jalurSama = arthur != null && npc.jalurInteraksi != null && arthur.JalurAktif == npc.jalurInteraksi;
            bool memuat = KawasanKampus.Instance != null && KawasanKampus.Instance.SedangMemuat;
            bool diam = memuat || MalamEksplorasiArthur.NpcTidur || (dialog != null && dialog.minigameAktif);
            float jarak = jalurSama ? Vector3.Distance(arthur.transform.position - Vector3.up * ArthurJalurKampus.JarakKaki, transform.position) : float.PositiveInfinity;
            float dekat = 1 - Mathf.InverseLerp(jarakPenuh, Mathf.Max(jarakMulai, jarakPenuh + .1f), jarak);
            sasaran = jalurSama && !diam ? Mathf.SmoothStep(0, 1, dekat) * volumeNyanyian : 0;
            if (dialog != null && dialog.PercakapanAktif) sasaran *= .25f;
        }
        sumber.volume = Mathf.MoveTowards(sumber.volume, sasaran, Time.unscaledDeltaTime * volumeNyanyian * 2.5f);
        if (sasaran > 0 && !sumber.isPlaying) sumber.Play();
        if (sasaran == 0 && sumber.volume <= .001f && sumber.isPlaying) sumber.Stop();
    }

    void OnDisable()
    {
        if (sumber != null) { sumber.Stop(); sumber.volume = 0; }
        sasaran = 0;
        dijeda = false;
        cekBerikut = 0;
    }
}
