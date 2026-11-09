using UnityEngine;

// Silinder pipih mengikuti pijakan Arthur tanpa lampu atau raycast tambahan.
[DefaultExecutionOrder(200)]
public sealed class BayanganKontakArthur : MonoBehaviour
{
    public Transform bidang;
    public Camera kamera;
    [Min(0)] public float geserKeKamera = .18f;
    void LateUpdate()
    {
        if (bidang == null) return;
        Vector3 depan = kamera != null ? Vector3.ProjectOnPlane(-kamera.transform.forward, Vector3.up).normalized : Vector3.back;
        bidang.SetPositionAndRotation(transform.position + Vector3.down * (ArthurJalurKampus.JarakKaki - .025f) + depan * geserKeKamera, Quaternion.Euler(0,kamera!=null?kamera.transform.eulerAngles.y:0,0));
    }
}
