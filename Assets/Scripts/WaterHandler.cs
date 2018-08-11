using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.UI;

public class WaterHandler : MonoBehaviour
{
    public float WaterSize = 500f;
    public float FaucetFillRate = 0.05f;
    public float FaucetFillSpeed = 1f;
    public Image WaterEdge;

    public float MinDelayForWaterChange = 0.1f;
    public float MaxDelayForWaterChange = 0.25f;
    public float MinWaterChange = 0.01f;
    public float MaxWaterChange = 0.02f;

    public float StartDelay = 3f;
    public float MinDelayForWater = 2f;

    private const float WATER_SIZE_LIMIT_MIN = 0.1f;
    private const float WATER_SIZE_LIMIT_MAX = 1.19f;
    private const float WATER_SIZE_LIMIT_MAX_ON = 1.16f;
    private const float WATER_SIZE_LIMIT_MAX_OFF = 1.16f;
    private bool _faucetOn = false;
    private CoroutineHandle? _waterLevelCoroutine;
    private CoroutineHandle? _faucetControls;
    private bool _waterEdgeOn = false;

    private const float FISH_REFRESH_RATE = 0.15f;

    private float _waterDelay;
    private CoroutineHandle? _handleWaterCoroutine;

    private void Awake()
    {
        WaterEdge.gameObject.SetActive(false);
        _handleWaterCoroutine = Timing.RunCoroutine(HandleWater());
    }

    private void OnDestroy()
    {
        TimingHelpers.CleanlyKillCoroutine(ref _handleWaterCoroutine);
    }

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
        TimingHelpers.CleanlyKillCoroutine(ref _handleWaterCoroutine);
        _handleWaterCoroutine = Timing.RunCoroutine(HandleWater());

    }

    private void DecrementWaterLevel(float value, float overTime)
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
        while (true)
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
                ClampWaterLevel();
                yield return Timing.WaitForOneFrame;
            }
        }
    }

    private IEnumerator<float> HandleWater()
    {
        _waterDelay = StartDelay;
        while (true)
        {
            _waterDelay -= Random.Range(MinDelayForWaterChange, MaxDelayForWaterChange);
            _waterDelay = Mathf.Clamp(_waterDelay, MinDelayForWater, StartDelay);
            DecrementWaterLevel(Random.Range(MinWaterChange, MaxWaterChange), _waterDelay);
            yield return Timing.WaitForSeconds(_waterDelay);
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
            ClampWaterLevel();
            yield return Timing.WaitForOneFrame;
        }
    }

    private void ClampWaterLevel()
    {
        float currentScale = transform.localScale.x;
        if (currentScale > WATER_SIZE_LIMIT_MAX_ON)
        {
            if (currentScale > WATER_SIZE_LIMIT_MAX)
            {
                transform.localScale = Vector3.one * WATER_SIZE_LIMIT_MAX;
            }

            if (!_waterEdgeOn)
            {
                WaterEdge.gameObject.SetActive(true);
                _waterEdgeOn = true;
            }
        }
        else if (currentScale < WATER_SIZE_LIMIT_MIN)
        {
            transform.localScale = Vector3.one * WATER_SIZE_LIMIT_MIN;
            Debug.Log("Game Over");
        }
        else if (_waterEdgeOn && currentScale < WATER_SIZE_LIMIT_MAX_OFF)
        {
            WaterEdge.gameObject.SetActive(false);
            _waterEdgeOn = false;
        }
    }

    public Vector3 GetPointInsideWater()
    {
        var convertedToV2 = new Vector2(transform.localPosition.x, transform.localPosition.y);
        return (Random.insideUnitCircle * WaterSize * transform.localScale) + convertedToV2;
    }
}