using MEC;
using System.Collections.Generic;
using UnityEngine;

public class FishHandler : MonoBehaviour
{
    public float Speed = 15f;
    public float RotationSpeed = 10f;
    private CoroutineHandle? _movingCoroutine;
    private readonly float DISTANCE_CUTOFF = 0.1f;
    private Canvas _mainCanvas;

    private void Awake()
    {
        _mainCanvas = GetComponentInParent<Canvas>();
    }

    public void OnClick()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_mainCanvas.transform as RectTransform, Input.mousePosition, _mainCanvas.worldCamera, out pos);
        MoveTo(pos);
    }

    private void MoveTo(Vector3 position)
    {
        TimingHelpers.CleanlyKillCoroutine(ref _movingCoroutine);
        _movingCoroutine = Timing.RunCoroutine(HandleGoTo(position));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TimingHelpers.CleanlyKillCoroutine(ref _movingCoroutine);
    }

    private IEnumerator<float> HandleGoTo(Vector3 position)
    {
        while (Vector3.Distance(position, transform.localPosition) > DISTANCE_CUTOFF)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, position, Speed * Time.deltaTime);

            Vector3 vectorToTarget = position - transform.localPosition;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, RotationSpeed * Time.deltaTime);

            yield return Timing.WaitForOneFrame;
        }

        _movingCoroutine = null;
    }
}
