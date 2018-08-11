using UnityEngine;

public class WaterHandler : MonoBehaviour
{
    public void IncrementWater(float value)
    {
        transform.localScale += Vector3.one * value;
    }

    public Vector3 GetPointInsideWater()
    {
        var convertedToV2 = new Vector2(transform.localPosition.x, transform.localPosition.y);
        return (Random.insideUnitCircle * transform.localScale) + convertedToV2;
    }
}