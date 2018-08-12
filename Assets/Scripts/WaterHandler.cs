using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaterHandler : MonoBehaviour
{
    public static WaterHandler Instance;
    public FishHandler CurrentFish;

    public float WaterSize = 500f;
    public float FaucetFillRate = 0.05f;
    public float FaucetFillSpeed = 1f;
    public GameObject WaterEdge;
    public GameObject WaterBasedSpawnPointsParent;
    public GameObject FillingWaterAnim;
    public GameObject DrainingWaterAnim;

    public float MinDelayForWaterChange = 0.1f;
    public float MaxDelayForWaterChange = 0.25f;
    public float MinWaterChange = 0.01f;
    public float MaxWaterChange = 0.02f;

    public float StartDelay = 3f;
    public float MinDelayForWater = 2f;
    public AnimationCurve WaterFlowCurve;
    public Animator[] WaterTapAnimator;

    public bool WaterEdgeOn { get; private set; }
    public bool CanHandleWater { get; private set; }

    private float _currentWaterLevel = 1f;

    private const float WATER_SIZE_LIMIT_MIN = 0.1f;
    private const float WATER_SIZE_LIMIT_MAX = 1.25f;
    private const float WATER_SIZE_LIMIT_MAX_ON = 1.03f;
    private const float WATER_SIZE_LIMIT_MAX_OFF = 1.03f;
    private bool _faucetOn = false;
    private CoroutineHandle? _waterLevelCoroutine;
    private CoroutineHandle? _handleFaucetCoroutine;
    private Transform[] _spawnPoints;
    private EdgeCollider2D _waterCollider;
    private const float FISH_REFRESH_RATE = 0.15f;

    private float _waterDelay;
    private CoroutineHandle? _handleWaterCoroutine;

    private void Awake()
    {
        Instance = this;
        _spawnPoints = WaterBasedSpawnPointsParent.transform.Cast<Transform>().ToArray();
        CanHandleWater = true;
        WaterEdgeOn = false;
        WaterEdge.SetActive(false);
        _handleWaterCoroutine = Timing.RunCoroutine(HandleWater());
        AudioManager.Instance.DrainEffect.StartPlaying();
        _waterCollider = GetComponent<EdgeCollider2D>();
        FillingWaterAnim.SetActive(false);
        DrainingWaterAnim.SetActive(true);
    }

    private void OnDestroy()
    {
        TimingHelpers.CleanlyKillCoroutine(ref _handleFaucetCoroutine);
        TimingHelpers.CleanlyKillCoroutine(ref _handleWaterCoroutine);
        TimingHelpers.CleanlyKillCoroutine(ref _waterLevelCoroutine);
    }

    public void TurnOnFaucet()
    {
        if (!CanHandleWater || _faucetOn)
        {
            return;
        }

        _faucetOn = true;
        TimingHelpers.CleanlyKillCoroutine(ref _waterLevelCoroutine);
        _handleFaucetCoroutine = Timing.RunCoroutine(HandleFaucet());
        AudioManager.Instance.FaucetEffect.StartPlaying();
        AudioManager.Instance.DrainEffect.EndPlaying();
        FillingWaterAnim.SetActive(true);
        DrainingWaterAnim.SetActive(false);
        foreach (var item in WaterTapAnimator)
        {
            item.enabled = false;
        }
    }

    public void TurnOffFaucet()
    {
        if (!CanHandleWater || !_faucetOn)
        {
            return;
        }

        _faucetOn = false;
        TimingHelpers.CleanlyKillCoroutine(ref _handleFaucetCoroutine);
        TimingHelpers.CleanlyKillCoroutine(ref _handleWaterCoroutine);
        _handleWaterCoroutine = Timing.RunCoroutine(HandleWater());
        AudioManager.Instance.FaucetEffect.EndPlaying();
        AudioManager.Instance.DrainEffect.StartPlaying();
        FillingWaterAnim.SetActive(false);
        DrainingWaterAnim.SetActive(true);
        foreach (var item in WaterTapAnimator)
        {
            item.enabled = true;
        }
    }

    public void StopAllWaterInteractions()
    {
        _faucetOn = false;

        TimingHelpers.CleanlyKillCoroutine(ref _handleFaucetCoroutine);
        TimingHelpers.CleanlyKillCoroutine(ref _handleWaterCoroutine);
        TimingHelpers.CleanlyKillCoroutine(ref _waterLevelCoroutine);

        AudioManager.Instance.FaucetEffect.EndPlaying();
        AudioManager.Instance.DrainEffect.EndPlaying();
        FillingWaterAnim.SetActive(false);
        DrainingWaterAnim.SetActive(false);
        CanHandleWater = false;
        foreach (var item in WaterTapAnimator)
        {
            item.enabled = false;
        }
    }

    public void ResumeWaterInteractions()
    {
        if (!CurrentFish.IsAlive)
        {
            return;
        }

        _handleWaterCoroutine = Timing.RunCoroutine(HandleWater());
        CanHandleWater = true;
        foreach (var item in WaterTapAnimator)
        {
            item.enabled = true;
        }
    }

    public void GetClosestDataToEdge(Vector3 localPosition, out Vector3 closestPoint, out float closestDistance)
    {
        closestPoint = Vector3.zero;
        closestDistance = float.MaxValue;
        var dist = 0f;

        foreach (var point in _waterCollider.points)
        {
            dist = Vector3.Distance(localPosition, point);
            if (dist < closestDistance)
            {
                closestPoint = point;
                closestDistance = dist;
            }
        }
    }

    private void DecrementWaterLevel(float value, float overTime)
    {
        if (_faucetOn)
        {
            return;
        }

        TimingHelpers.CleanlyKillCoroutine(ref _waterLevelCoroutine);
        _waterLevelCoroutine = Timing.RunCoroutine(HandleWaterLevelChange(_currentWaterLevel - value, overTime));
    }

    private IEnumerator<float> HandleFaucet()
    {
        while (true)
        {
            var startWaterLevel = _currentWaterLevel;
            var endWaterLevel = _currentWaterLevel + FaucetFillRate;
            var delta = 0f;
            var timer = 0f;
            while (delta != 1)
            {
                timer += Time.deltaTime;
                delta = Mathf.Clamp01(timer / FaucetFillSpeed);
                _currentWaterLevel = Mathf.Lerp(startWaterLevel, endWaterLevel, delta);
                UpdateWaterLevel();
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

    private IEnumerator<float> HandleWaterLevelChange(float endWaterLevel, float duration)
    {
        var startWaterLevel = _currentWaterLevel;
        var delta = 0f;
        var timer = 0f;

        while (delta != 1)
        {
            timer += Time.deltaTime;
            delta = Mathf.Clamp01(timer / duration);
            _currentWaterLevel = Mathf.Lerp(startWaterLevel, endWaterLevel, delta);
            UpdateWaterLevel();
            yield return Timing.WaitForOneFrame;
        }
    }

    private void UpdateWaterLevel()
    {
        ClampWaterLevel();
        transform.localScale = Vector3.one * WaterFlowCurve.Evaluate(_currentWaterLevel);
    }

    private void ClampWaterLevel()
    {
        if (_currentWaterLevel > WATER_SIZE_LIMIT_MAX_ON)
        {
            if (_currentWaterLevel > WATER_SIZE_LIMIT_MAX)
            {
                _currentWaterLevel = WATER_SIZE_LIMIT_MAX;
            }

            if (!WaterEdgeOn)
            {
                WaterEdge.SetActive(true);
                WaterEdgeOn = true;
            }
        }
        else if (_currentWaterLevel < WATER_SIZE_LIMIT_MIN)
        {
            _currentWaterLevel = WATER_SIZE_LIMIT_MIN;
            CurrentFish.KillFishie();
        }
        else if (WaterEdgeOn && _currentWaterLevel < WATER_SIZE_LIMIT_MAX_OFF)
        {
            WaterEdge.SetActive(false);
            WaterEdgeOn = false;
        }
    }

    public Vector3 GetPointInsideWater()
    {
        return _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;
    }
}