using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform Target;

    private void Update()
    {
        transform.position = Target.position;
    }
}