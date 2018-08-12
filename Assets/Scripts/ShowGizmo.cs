using UnityEngine;

public class ShowGizmo : MonoBehaviour {

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 25f);
    }
}