using MEC;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const float FISH_REFRESH_RATE = 0.15f;
    public float MinDelayForWaterChange = 0.1f;
    public float MaxDelayForWaterChange = 0.25f;
    public float MinWaterChange = 0.01f;
    public float MaxWaterChange = 0.02f;

    public float StartDelay = 3f;
    public float MinDelayForWater = 2f;

    public WaterHandler Water;
    public FishHandler Fish;

    private CoroutineHandle? _handleWaterCoroutine;

    private void Awake()
    {
        _handleWaterCoroutine = Timing.RunCoroutine(HandleWater());
    }

    private void OnDestroy()
    {
        TimingHelpers.CleanlyKillCoroutine(ref _handleWaterCoroutine);
    }

    private IEnumerator<float> HandleWater()
    {
        var delay = StartDelay;
        while (true)
        {
            delay -= Random.Range(MinDelayForWaterChange, MaxDelayForWaterChange);
            delay = Mathf.Clamp(delay, MinDelayForWater, StartDelay);
            Water.DecrementWaterLevel(Random.Range(MinWaterChange, MaxWaterChange), delay);
            yield return Timing.WaitForSeconds(delay);
        }
    }
}