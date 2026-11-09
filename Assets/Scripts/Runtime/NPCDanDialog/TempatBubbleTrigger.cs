// PANDUAN PROYEK - Runtime / NPCDanDialog
// Fungsi: Memunculkan ikon interaksi tempat tanpa menggeser pemilik collider.
// Panduan: Assets/Scripts/Runtime/NPCDanDialog/README.md

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempatBubbleTrigger : MonoBehaviour
{
    [Header("Referensi Ikon Tempat")]
    public Transform ikonTransform;
    public float durasiAnimasi = 0.4f;
    public Vector3 ukuranMaksimal = new Vector3(0.1f, 0.1f, 0.1f);
    [Range(0,.22f)] public float tinggiBubbleDiLayar=.13f;
    Vector3 skalaAnimasi;
    SpriteRenderer gambarIkon;

    [Header("Penempatan Penanda")]
    public bool pusatkanDiAtasKepalaArthur;
    private SpriteRenderer kepalaArthur;
    private Camera kameraArthur;

    [Header("Pengaturan Goyang (Wiggly)")]
    public float jedaWiggly = 0.5f;
    public float kekuatanGoyang = 0.05f;
    public float kecepatanGoyang = 15f;

    private Coroutine animasiCoroutine;
    private Coroutine wigglyCoroutine;
    private Vector3 posisiAsliIkon;
    private bool bolehGoyang = false;
    private float timerGoyang;
    private readonly HashSet<Collider2D> pemainDalamArea = new HashSet<Collider2D>();
    private bool sedangDinonaktifkan;
    private bool dialogMenyembunyikan;
    private bool BolehAnimasi => !sedangDinonaktifkan && gameObject.activeInHierarchy && isActiveAndEnabled;

    void Awake()
    {
        // Animasi hanya boleh menyentuh gambar, bukan transform pemilik collider.
        if (ikonTransform == transform)
        {
            Transform gambar = transform.Find("IkonGambar");
            if (gambar != null)
            {
                ukuranMaksimal = Vector3.Scale(ukuranMaksimal, gambar.localScale);
                ikonTransform = gambar;
            }
            else ikonTransform = null;
        }
        if (ikonTransform != null)
        {
            posisiAsliIkon = ikonTransform.localPosition;
        }
        ResetIkon();
    }

    void OnEnable() { sedangDinonaktifkan = false; ResetIkon(); }
    void OnDisable() { sedangDinonaktifkan = true; ResetIkon(); }

    private void ResetIkon()
    {
        StopAllCoroutines();
        animasiCoroutine = null;
        wigglyCoroutine = null;
        pemainDalamArea.Clear();
        bolehGoyang = false;
        timerGoyang = 0f;
        dialogMenyembunyikan = false;
        skalaAnimasi=Vector3.zero;
        if (ikonTransform != null)
        {
            ikonTransform.localScale = Vector3.zero;
            ikonTransform.localPosition = posisiAsliIkon;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (BolehAnimasi && collision.CompareTag("Player") && pemainDalamArea.Add(collision) && pemainDalamArea.Count == 1)
        {
            bool sembunyi=DialogManager.Instance!=null&&(DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif);
            MulaiAnimasiIkon(sembunyi?Vector3.zero:ukuranMaksimal);
            if (wigglyCoroutine != null) StopCoroutine(wigglyCoroutine);
            if(!sembunyi)wigglyCoroutine = StartCoroutine(JedaDanGoyang());
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Memulihkan status jika area dinyalakan ketika pemain sudah di dalamnya.
        OnTriggerEnter2D(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // OBAT ANTI ERROR: Kalau objeknya tiba-tiba dimatikan saat teleport, batalkan semua animasi!
        if (!BolehAnimasi) { ResetIkon(); return; }

        if (collision.CompareTag("Player") && pemainDalamArea.Remove(collision) && pemainDalamArea.Count == 0)
        {
            MulaiAnimasiIkon(Vector3.zero);
            if (wigglyCoroutine != null) StopCoroutine(wigglyCoroutine);
            bolehGoyang = false;
            if (ikonTransform != null) ikonTransform.localPosition = posisiAsliIkon;
        }
    }

    private void MulaiAnimasiIkon(Vector3 skalaTarget)
    {
        if (!BolehAnimasi || ikonTransform == null) return;
        if (animasiCoroutine != null) StopCoroutine(animasiCoroutine);
        animasiCoroutine = StartCoroutine(AnimasiSkala(skalaTarget));
    }

    private IEnumerator AnimasiSkala(Vector3 targetSkala)
    {
        Vector3 skalaAwal = skalaAnimasi;
        float waktu = 0f;
        while (waktu < durasiAnimasi)
        {
            waktu += Time.deltaTime;
            skalaAnimasi = Vector3.Lerp(skalaAwal, targetSkala, waktu / durasiAnimasi);
            yield return null;
        }
        skalaAnimasi = targetSkala;
    }

    private IEnumerator JedaDanGoyang()
    {
        bolehGoyang = false;
        yield return new WaitForSeconds(jedaWiggly);
        bolehGoyang = true;
    }

    void Update()
    {
        bool dialog = DialogManager.Instance != null && (DialogManager.Instance.PercakapanAktif||DialogManager.Instance.minigameAktif);
        if(dialog != dialogMenyembunyikan)
        {
            dialogMenyembunyikan=dialog;
            if(pemainDalamArea.Count>0)
            {
                MulaiAnimasiIkon(dialog?Vector3.zero:ukuranMaksimal);
                if(wigglyCoroutine!=null)StopCoroutine(wigglyCoroutine);
                bolehGoyang=false;
                if(!dialog)wigglyCoroutine=StartCoroutine(JedaDanGoyang());
            }
        }
        if (bolehGoyang && ikonTransform != null)
        {
            timerGoyang += Time.deltaTime;
            if (timerGoyang >= (1f / Mathf.Max(1f, kecepatanGoyang)))
            {
                timerGoyang = 0f;
                float xAcak = Random.Range(-kekuatanGoyang, kekuatanGoyang);
                float yAcak = Random.Range(-kekuatanGoyang, kekuatanGoyang);
                ikonTransform.localPosition = posisiAsliIkon + new Vector3(xAcak, yAcak, 0);
            }
        }
    }

    void LateUpdate()
    {
        if(ikonTransform==null)return;
        ikonTransform.localScale=skalaAnimasi;
        if(skalaAnimasi.sqrMagnitude<.000001f)return;
        if(kameraArthur==null)kameraArthur=Camera.main;
        if(gambarIkon==null)gambarIkon=ikonTransform.GetComponentInChildren<SpriteRenderer>();
        if(kameraArthur!=null&&gambarIkon!=null&&tinggiBubbleDiLayar>0)
        {
            var b=gambarIkon.localBounds;
            float muncul=Mathf.Clamp01(Mathf.Abs(skalaAnimasi.x/ukuranMaksimal.x));
            for(int i=0;i<3;i++){
                var atas=kameraArthur.WorldToScreenPoint(gambarIkon.transform.TransformPoint(b.center+Vector3.up*b.extents.y));
                var bawah=kameraArthur.WorldToScreenPoint(gambarIkon.transform.TransformPoint(b.center-Vector3.up*b.extents.y));
                float tinggi=Mathf.Abs(atas.y-bawah.y);
                if(tinggi>.01f&&atas.z>0)ikonTransform.localScale*=kameraArthur.pixelHeight*tinggiBubbleDiLayar*muncul/tinggi;
            }
        }
        if (!pusatkanDiAtasKepalaArthur) return;
        if (kepalaArthur == null || kameraArthur == null)
        {
            var pemain = FindFirstObjectByType<KetukKepalaArthur>();
            if (pemain == null) return;
            kepalaArthur = pemain.kepala;
            kameraArthur = pemain.GetComponentInChildren<Camera>();
        }
        if (kepalaArthur == null || kameraArthur == null) return;
        var penanda = kameraArthur.WorldToScreenPoint(ikonTransform.position);
        var kepala = kameraArthur.WorldToScreenPoint(kepalaArthur.bounds.center);
        if (penanda.z <= 0 || kepala.z <= 0) return;
        penanda.x = kepala.x;
        ikonTransform.position = kameraArthur.ScreenToWorldPoint(penanda);
    }
}
