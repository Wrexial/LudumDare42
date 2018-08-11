using System.Collections.Generic;
using MEC;
using UnityEngine;

public class WaterHandler : MonoBehaviour
{
    public float WaterSize = 500f;
    public float FaucetFillRate = 0.05f;
    public float FaucetFillSpeed = 1f;

    private bool _faucetOn = false;
    private CoroutineHandle? _waterLevelCoroutine;
    private CoroutineHandle? _faucetControls;

    public void TurnOnFaucet()
    {
        _faucetOn = true;
        TimingHelpers.CleanlyKillCoroutine(ref _waterLevelCoroutine);
        _faucetControls = Timing.RunCoroutine(HandleFaucet());
    }

    public void TurnOffFaucet()
    {
        _faucetOn = false;
        TimingHelpers.CleanlyKillCoroutine(ref _faucetControls);
    }

    public void DecrementWaterLevel(float value, float overTime)
    {
        if (_faucetOn)
        {
            return;
        }

        TimingHelpers.CleanlyKillCoroutine(ref _waterLevelCoroutine);
        _waterLevelCoroutine = Timing.RunCoroutine(HandleWaterLevelChange(transform.localScale + (Vector3.one * -value), overTime));
    }

    private IEnumerator<float> HandleFaucet()
    {
        while(true)
        {
            var startScale = transform.localScale;
            var endScale = transform.localScale + (Vector3.one * FaucetFillRate);
            var delta = 0f;
            var timer = 0f;
            while (delta != 1)
            {
                timer += Time.deltaTime;
                delta = Mathf.Clamp01(timer / FaucetFillSpeed);
                transform.localScale = Vector3.Lerp(startScale, endScale, delta);
                yield return Timing.WaitForOneFrame;
            }
        }
    }

    private IEnumerator<float> HandleWaterLevelChange(Vector3 endScale, float duration)
    {
        var startScale = transform.localScale;
        var delta = 0f;
        var timer = 0f;
        while (delta != 1)
        {
            timer += Time.deltaTime;
            delta = Mathf.Clamp01(timer / duration);
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