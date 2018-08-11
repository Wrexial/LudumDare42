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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        FoodHandler.Instance.CollectFood(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TimingHelpers.CleanlyKillCoroutine(ref _movingCoroutine);
        Debug.Log("Hit edge!");

        if (WaterHandler.Instance.WaterEdgeOn)
        {
            Debug.Log("Game Over");
        }
    }

    private IEnumerator<float> HandleGoTo(Vector3 position)
    {
        while (Vector3.Distance(position, transform.localPosition) > DISTANCE_CUTOFF)
        {
            var thisSpeed = Speed;
            var vectorToTarget = position - transform.localPosition;
            var angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;

            //if (WaterHandler.Instance.WaterEdgeOn)
            //{
            //    var position = WaterHandler.Instance.GetClosestToEdge(transform.localPosition);
                  //Mod thisSpeed
            //}

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, position, thisSpeed * Time.deltaTime);

            var q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, RotationSpeed * Time.deltaTime);

            yield return Timing.WaitForOneFrame;
        }

        _movingCoroutine = null;
    }
}