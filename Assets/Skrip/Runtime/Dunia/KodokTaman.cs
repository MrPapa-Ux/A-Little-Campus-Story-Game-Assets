using UnityEngine;

// Small decorative hops stay inside the planting strip, away from Arthur's path.
public sealed class KodokTaman : MonoBehaviour
{
    public float fase;
    Vector3 asal;
    void Start() { asal = transform.localPosition; }
    void Update()
    {
        float t = Mathf.Repeat(Time.time + fase, 14f);
        float hop = t < .65f ? Mathf.Sin(t / .65f * Mathf.PI) : 0;
        transform.localPosition = asal + new Vector3(.08f * hop, .16f * hop, 0);
    }
}
