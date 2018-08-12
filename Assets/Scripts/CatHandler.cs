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
    public int BaseAttackSpawnCount = 3;

    private int _round;
    private CoroutineHandle? _catHandler;
    private List<Collider2D> _attacks;

    private void Awake()
    {
        Instance = this;
        Cat.SetActive(false);
        CatWaves.SetActive(false);
        _attacks = new List<Collider2D>();
        for (int i = 0; i < BaseAttackSpawnCount - 1; i++)
        {
            var attackInstance = Instantiate(CatAttackIndicatorPrefab, transform).GetComponent<Collider2D>();
            attackInstance.gameObject.SetActive(false);
            _attacks.Add(attackInstance);
        }
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
        _attacks.Add(Instantiate(CatAttackIndicatorPrefab, transform).GetComponent<Collider2D>());

        foreach (var attack in _attacks)
        {
            attack.transform.position = WaterHandler.Instance.GetPointInsideWater();
            attack.gameObject.SetActive(true);
        }

        yield return Timing.WaitForSeconds(DelayBeforeCatAttacksKill);

        foreach (var attack in _attacks)
        {
            attack.enabled = true;
        }

        yield return Timing.WaitForSeconds(DelayBeforeCatCleanup);

        foreach (var attack in _attacks)
        {
            attack.gameObject.SetActive(false);
        }

        Cat.SetActive(false);
        WaterHandler.Instance.ResumeWaterInteractions();
        IsCatActive = false;
    }
}