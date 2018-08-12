using MEC;
using System.Collections.Generic;
using UnityEngine;

public class CreditsHandler : MonoBehaviour
{
    public GameObject WaterBasedSpawnPointsParent;
    public FishHandler[] Fish;

    private Transform[] _spawnPoints;
    private List<CoroutineHandle?> _fishHandlerCoroutines = new List<CoroutineHandle?>();

    private void Awake()
    {
        _spawnPoints = WaterBasedSpawnPointsParent.GetComponentsInChildren<Transform>();
        foreach (var fishie in Fish)
        {
            _fishHandlerCoroutines.Add(Timing.RunCoroutine(FishHandler(fishie)));
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _fishHandlerCoroutines.Count; i++)
        {
            var coroutine = _fishHandlerCoroutines[i];
            TimingHelpers.CleanlyKillCoroutine(ref coroutine);
        }
    }

    private IEnumerator<float> FishHandler(FishHandler fish)
    {
        var currentPath = 5f;
        while (true)
        {
            fish.MoveTo(GetPointInsideWater());

            while (fish.IsMoving && currentPath > 0)
            {
                currentPath -= 0.1f;
                yield return Timing.WaitForSeconds(0.1f);
            }

            currentPath = 5f;
        }
    }


    public Vector3 GetPointInsideWater()
    {
        return _spawnPoints[Random.Range(0, _spawnPoints.Length)].localPosition;
    }
}
