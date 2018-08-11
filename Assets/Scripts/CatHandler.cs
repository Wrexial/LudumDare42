using System.Collections.Generic;
using MEC;
using UnityEngine;
using Random = UnityEngine.Random;

public class CatHandler : MonoBehaviour
{
    public static CatHandler Instance;

    public GameObject Cat;
    public GameObject CatWaves;
    public GameObject CatAttackIndicatorPrefab;

    public bool IsCatActive { get; private set; }

    public float CatWaveDuration = 2.5f;
    public float DelayBeforeCatSpawnsAttacks = 1f;
    public float DelayBeforeCatAttacksKill = 2.5f;
    public float DelayBeforeCatCleanup = 0.05f;

    private int _round;
    private CoroutineHandle? _catHandler;

    private void Awake()
    {
        Instance = this;
        Cat.SetActive(false);
        CatWaves.SetActive(false);
    }

    private void OnDestroy()
    {
        TimingHelpers.CleanlyKillCoroutine(ref _catHandler);
    }

    public void HandleCat()
    {
        _round++;
        IsCatActive = true;
        WaterHandler.Instance.StopAllWaterInteractions();
        _catHandler = Timing.RunCoroutine(HandleCatCoroutine());
    }

    private IEnumerator<float> HandleCatCoroutine()
    {
        CatWaves.SetActive(true);
        yield return Timing.WaitForSeconds(CatWaveDuration);
        CatWaves.SetActive(false);
        Cat.SetActive(true);
        yield return Timing.WaitForSeconds(DelayBeforeCatSpawnsAttacks);
        Debug.Log("Spawn death circles");
        yield return Timing.WaitForSeconds(DelayBeforeCatAttacksKill);
        Debug.Log("Actually kill fish!");
        yield return Timing.WaitForSeconds(DelayBeforeCatCleanup);
        Cat.SetActive(false);
        WaterHandler.Instance.ResumeWaterInteractions();
    }
}