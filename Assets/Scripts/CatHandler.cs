using System;
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
    public MeowingHandler MeowingPrefab;

    public bool IsCatActive { get; private set; }

    public float DelayBeforeCatSpawnsAttacks = 2f;
    public float DelayBeforeCatAttacksKill = 2.5f;
    public float DelayBeforeCatCleanup = 1f;

    public int BaseAttackSpawnCount = 3;

    private int _round = 0;
    private CoroutineHandle? _handleCatCoroutine;
    private List<Animator> _attacks;
    private List<MeowingHandler> _meowing;

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

        _meowing = new List<MeowingHandler>();
        for (int i = 0; i < 50; i++)
        {
            var meowInstance = Instantiate(MeowingPrefab, transform);
            meowInstance.gameObject.SetActive(false);
            _meowing.Add(meowInstance);
        }
    }

    private void OnDestroy()
    {
        TimingHelpers.CleanlyKillCoroutine(ref _handleCatCoroutine);
    }

    public void HandleCat()
    {
        _round++;
        IsCatActive = true;
        WaterHandler.Instance.StopAllWaterInteractions();
        _handleCatCoroutine = Timing.RunCoroutine(HandleCatCoroutine());
    }

    private IEnumerator<float> HandleCatCoroutine()
    {
        CoroutineHandle? meowing = null;
        if (_round == 1)
        {
            meowing = Timing.RunCoroutine(SpawnAllMeows());
        }
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

        TimingHelpers.CleanlyKillCoroutine(ref meowing);

        if (_round == 4)
        {
            WaterHandler.Instance.CurrentFish.Victory();
        }

        if (WaterHandler.Instance.CurrentFish.IsAlive)
        {
            IsCatActive = false;
        }
    }

    private IEnumerator<float> SpawnAllMeows()
    {
        foreach (var meow in _meowing)
        {
            meow.transform.localPosition = new Vector3(Random.Range(-380, 380), Random.Range(-200, 200), 0);
            meow.transform.Rotate(Vector3.back, Random.Range(0, 360));
            meow.gameObject.SetActive(true);
            meow.StartAnim();
            yield return Timing.WaitForSeconds(Random.Range(0.4f, 0.8f));
        }
    }
}