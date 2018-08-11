using MEC;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float MinDelayForWaterChange = 0.1f;
    public float MaxDelayForWaterChange = 0.25f;
    public float MinDelayForWater = 2f;
    public float MaxDelayForWater = 5f;
    public float MinWaterChange = 0.01f;
    public float MaxWaterChange = 0.02f;

    public float StartDelay = 3f;

    public WaterHandler Water;
    public FishHandler Fish;

    private CoroutineHandle? _handleFishCoroutine;
    private CoroutineHandle? _handleWaterCoroutine;

    private void Awake()
    {
        _handleFishCoroutine = Timing.RunCoroutine(HandleFish());
        _handleWaterCoroutine = Timing.RunCoroutine(HandleWater());
    }

    private void OnDestroy()
    {
        TimingHelpers.CleanlyKillCoroutine(ref _handleFishCoroutine);
        TimingHelpers.CleanlyKillCoroutine(ref _handleWaterCoroutine);
    }

    private IEnumerator<float> HandleFish()
    {
        var delay = StartDelay;
        while (true)
        {
            var randomPointInWater = Water.GetPointInsideWater();
            Fish.MoveTo(randomPointInWater);
            yield return Timing.WaitForSeconds(delay);
        }
    }

    private IEnumerator<float> HandleWater()
    {
        var delay = StartDelay;
        while (true)
        {
            delay -= Random.Range(MinDelayForWaterChange, MaxDelayForWaterChange);
            delay = Mathf.Clamp(delay, MinDelayForWater, MaxDelayForWater);
            Water.DecrementWaterLevel(Random.Range(MinWaterChange, MaxWaterChange), delay);
            yield return Timing.WaitForSeconds(delay);
        }
    }
}