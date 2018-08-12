using MEC;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        if (collision.CompareTag("Food"))
        {
            FoodHandler.Instance.CollectFood(collision.gameObject);
        }
        else if (collision.CompareTag("Death"))
        {
            Debug.Log("Game Over - Hit by cat");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TimingHelpers.CleanlyKillCoroutine(ref _movingCoroutine);
        Debug.Log("Hit edge!");

        if (WaterHandler.Instance.WaterEdgeOn)
        {
            Debug.Log("Game Over - Hit water edge");
        }
    }

    private IEnumerator<float> HandleGoTo(Vector3 position)
    {
        while (Vector3.Distance(position, transform.localPosition) > DISTANCE_CUTOFF)
        {
            var thisSpeed = Speed;
            var vectorToTarget = position - transform.localPosition;
            var angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;

            //DOESNT REALLY WORK YET
            //if (WaterHandler.Instance.WaterEdgeOn)
            //{
            //    var closestPos = Vector3.zero;
            //    var edgeDistance = 0f;
            //    WaterHandler.Instance.GetClosestDataToEdge(transform.localPosition, out closestPos, out edgeDistance);

            //    var toPosition = (closestPos - transform.localPosition);
            //    var angleToPosition = Mathf.Atan2(toPosition.y, toPosition.x) * Mathf.Rad2Deg;

            //    if (angleToPosition < 0f)
            //    {
            //        angleToPosition += 360f;
            //    }

            //    Debug.Log("AP : " + angleToPosition + " ,D: " + edgeDistance);
            //    if (angleToPosition < 180f)
            //    {
            //        thisSpeed = Mathf.Clamp(thisSpeed - ((100 - edgeDistance) * 0.25f), 1, Speed);
            //    }
            //    else
            //    {
            //        thisSpeed = Mathf.Clamp(thisSpeed + ((100 - edgeDistance) * 0.25f), 1, Speed * 2);
            //    }
            //    //Mod thisSpeed
            //}

            transform.localPosition = Vector3.MoveTowards(transform.localPosition, position, thisSpeed * Time.deltaTime);

            var q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, RotationSpeed * Time.deltaTime);

            yield return Timing.WaitForOneFrame;
        }

        _movingCoroutine = null;
    }
}