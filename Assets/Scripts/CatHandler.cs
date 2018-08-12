using System.Collections.Generic;
using MEC;
using UnityEngine;
using Random = UnityEngine.Random;

public class CatHandler : MonoBehaviour
{
    public static CatHandler Instance;

    public Animator CatAnimator;
    public GameObject Cat;
    public GameObject CatWaves;
    public GameObject CatAttackIndicatorPrefab;

    public bool IsCatActive { get; private set; }

    public float DelayBeforeCatSpawnsAttacks = 2f;
    public float DelayBeforeCatAttacksKill = 2.5f;
    public float DelayBeforeCatCleanup = 1f;

    public int BaseAttackSpawnCount = 3;

    private int _round;
    private CoroutineHandle? _catHandler;
    private List<Animator> _attacks;

    private void Awake()
    {
        Instance = this;
        Cat.SetActive(false);
        CatWaves.SetActive(false);
        _attacks = new List<Animator>();
        for (int i = 0; i < BaseAttackSpawnCount - 1; i++)
        {
            var attackInstance = Instantiate(CatAttackIndicatorPrefab, transform).GetComponent<Animator>();
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
        AudioManager.Instance.CatTheme.StartPlaying();
        CatWaves.SetActive(true);
        yield return Timing.WaitForSeconds(AudioManager.Instance.CatTheme.StartClip.length);
        CatWaves.SetActive(false);
        Cat.SetActive(true);
        yield return Timing.WaitForSeconds(DelayBeforeCatSpawnsAttacks);
        _attacks.Add(Instantiate(CatAttackIndicatorPrefab, transform).GetComponent<Animator>());

        foreach (var attack in _attacks)
        {
            attack.transform.position = WaterHandler.Instance.GetPointInsideWater();
            attack.gameObject.SetActive(true);
        }

        yield return Timing.WaitForSeconds(DelayBeforeCatAttacksKill);

        foreach (var attack in _attacks)
        {
            attack.SetTrigger("Attack");
        }
        AudioManager.Instance.PlayCatAttack();
        AudioManager.Instance.CatTheme.EndPlaying();
        CatAnimator.SetTrigger("Despawn");

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