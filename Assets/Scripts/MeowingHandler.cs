using System.Collections.Generic;
using MEC;
using UnityEngine;

public class MeowingHandler : MonoBehaviour
{
    public void StartAnim()
    {
        var r = Random.Range(0, 4);
        Timing.RunCoroutine(HandleAnimation(r));
    }

    private IEnumerator<float> HandleAnimation(int r)
    {
        var delta = 0f;
        var timer = 0f;
        var duration = 2.5f;
        switch (r)
        {
            case 0:
                while (delta != 1)
                {
                    timer += Time.deltaTime;
                    delta = Mathf.Clamp01(timer / duration);
                    transform.Rotate(Vector3.back, Random.Range(0f, 1f));
                    yield return Timing.WaitForOneFrame;
                }
                break;

            case 1:
                while (delta != 1)
                {
                    timer += Time.deltaTime;
                    delta = Mathf.Clamp01(timer / duration);
                    transform.Translate(Random.Range(0f, 1f), 0, 0);
                    yield return Timing.WaitForOneFrame;
                }
                break;

            case 2:
                while (delta != 1)
                {
                    timer += Time.deltaTime;
                    delta = Mathf.Clamp01(timer / duration);
                    transform.Translate(0, Random.Range(0f, 1f), 0);
                    yield return Timing.WaitForOneFrame;
                }
                break;

            case 3:
                while (delta != 1)
                {
                    timer += Time.deltaTime;
                    delta = Mathf.Clamp01(timer / duration);
                    transform.localScale -= Vector3.one * Time.deltaTime;
                    yield return Timing.WaitForOneFrame;
                }
                break;
        }

        Destroy(this.gameObject);
    }
}
