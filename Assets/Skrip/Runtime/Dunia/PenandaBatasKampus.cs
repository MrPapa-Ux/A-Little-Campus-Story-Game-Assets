using UnityEngine;

public sealed class PenandaBatasKampus : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(.7f,.2f,1f);
        Gizmos.DrawWireCube(transform.position, new Vector3(.5f,8f,5f));
    }
}
