using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class FoodHandler : MonoBehaviour
{
    public static FoodHandler Instance;
    public GameObject FoodPrefab;

    public float FoodSpawnMinDelay = 2;
    public float FoodSpawnMaxDelay = 5;

    public int FoodToSpawnForRoundMin = 5;
    public int FoodToSpawnForRoundMax = 10;

    private int _foodCollected = 0;
    private CoroutineHandle? _foodSpawner;
    private Transform[] _spawnPoints;
    private Queue<GameObject> _foodQueue;
    private int _foodToSpawn = 0;
    private int _foodStillIngame;
    private const float TIMER_WAIT_TIMERS = 0.25f;

    private void Awake()
    {
        Instance = this;
        _spawnPoints = transform.Cast<Transform>().ToArray();
        PoolFood();
        _foodSpawner = Timing.RunCoroutine(HandleFoodSpawning());
    }

    private void OnDestroy()
    {
        TimingHelpers.CleanlyKillCoroutine(ref _foodSpawner);
    }

    public void CollectFood(GameObject foodItem)
    {
        foodItem.SetActive(false);
        _foodQueue.Enqueue(foodItem);
        _foodCollected++;
        _foodStillIngame--;
        if (_foodToSpawn == 0 && _foodStillIngame == 0)
        {
            AudioManager.Instance.PlayFinalPickup();
        }
        else
        {
            AudioManager.Instance.PlayPickup();
        }
    }

    private void PoolFood()
    {
        _foodQueue = new Queue<GameObject>();
        for (int i = 0; i < 15; i++)
        {
            var foodInstnace = Instantiate(FoodPrefab, transform);
            foodInstnace.SetActive(false);
            _foodQueue.Enqueue(foodInstnace);
        }
    }

    private IEnumerator<float> HandleFoodSpawning()
    {
        while (WaterHandler.Instance.CurrentFish.IsAlive)
        {
            Debug.Log("Spawning next round");
            _foodToSpawn = Random.Range(FoodToSpawnForRoundMin, FoodToSpawnForRoundMax);

            while (_foodToSpawn > 0)
            {
                yield return Timing.WaitForSeconds(Random.Range(FoodSpawnMinDelay, FoodSpawnMaxDelay));
                _foodToSpawn--;
                _foodStillIngame++;

                var foodItem = _foodQueue.Dequeue();
                foodItem.transform.position = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;
                foodItem.SetActive(true);
            }

            while (_foodStillIngame > 0)
            {
                yield return Timing.WaitForSeconds(TIMER_WAIT_TIMERS);
            }

            CatHandler.Instance.HandleCat();

            while (CatHandler.Instance.IsCatActive)
            {
                yield return Timing.WaitForSeconds(TIMER_WAIT_TIMERS);
            }
        }
    }
}