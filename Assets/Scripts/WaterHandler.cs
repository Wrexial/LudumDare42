using System.Collections.Generic;
using MEC;
using UnityEngine;

public class WaterHandler : MonoBehaviour
{
    public float WaterSize = 500f;
    private CoroutineHandle? _waterLevelCoroutine;

    public void IncrementWater(float value)
    {
        TimingHelpers.CleanlyKillCoroutine(ref _waterLevelCoroutine);
        _waterLevelCoroutine = Timing.RunCoroutine(HandleWaterLevelChange(transform.localScale + (Vector3.one * value)));
    }

    private IEnumerator<float> HandleWaterLevelChange(Vector3 endScale)
    {
        var startScale = transform.localScale;
        var delta = 0f;
        var timer = 0f;
        while (delta != 1)
        {
            timer += Time.deltaTime;
            delta = Mathf.Clamp01(timer / 0.15f);
            transform.localScale = Vector3.Lerp(startScale, endScale, delta);
            yield return Timing.WaitForOneFrame;
        }
    }

    public Vector3 GetPointInsideWater()
    {
        var convertedToV2 = new Vector2(transform.localPosition.x, transform.localPosition.y);
        return (Random.insideUnitCircle * WaterSize * transform.localScale) + convertedToV2;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, WaterSize);
    }
}