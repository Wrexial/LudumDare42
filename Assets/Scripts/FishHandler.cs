using MEC;
using System.Collections.Generic;
using UnityEngine;

public class FishHandler : MonoBehaviour
{
    public float Speed;
    private CoroutineHandle? _movingCoroutine;
    private readonly float DISTANCE_CUTOFF = 1f;

    public void MoveTo(Vector3 position)
    {
        TimingHelpers.CleanlyKillCoroutine(ref _movingCoroutine);
        _movingCoroutine = Timing.RunCoroutine(HandleGoTo(position));
    }

    private IEnumerator<float> HandleGoTo(Vector3 position)
    {
        while (Vector3.Distance(position, transform.position) < DISTANCE_CUTOFF)
        {
            var step = Speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, position, step);
            transform.LookAt(position);
            yield return Timing.WaitForOneFrame;
        }

        _movingCoroutine = null;
    }
}
