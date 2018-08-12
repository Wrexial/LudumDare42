using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FishHandler : MonoBehaviour
{
    public float Speed = 15f;
    public float RotationSpeed = 10f;
    public Image Head;
    public Image Tail;
    public Sprite[] HeadSprite;
    public Sprite[] TailSprite;
    public GameObject EOGUi;
    public GameObject EOGUiWin;

    private CoroutineHandle? _movingCoroutine;
    private readonly float DISTANCE_CUTOFF = 0.1f;
    private Canvas _mainCanvas;
    private Animator _animator;
    public bool IsAlive { get; private set; }
    public bool IsMoving { get { return _movingCoroutine.HasValue; } }

    private void Awake()
    {
        _mainCanvas = GetComponentInParent<Canvas>();
        _animator = GetComponent<Animator>();
        var r = Random.Range(0, HeadSprite.Length);
        Head.sprite = HeadSprite[r];
        Tail.sprite = TailSprite[r];
        IsAlive = true;

        if (EOGUi != null)
        {
            EOGUi.SetActive(false);
        }

        if (EOGUiWin != null)
        {
            EOGUiWin.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        TimingHelpers.CleanlyKillCoroutine(ref _movingCoroutine);
    }

    public void OnClick()
    {
        if (!IsAlive)
        {
            return;
        }

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_mainCanvas.transform as RectTransform, Input.mousePosition, _mainCanvas.worldCamera, out pos);
        MoveTo(pos);
    }

    public void MoveTo(Vector3 position)
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
            KillFishie();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TimingHelpers.CleanlyKillCoroutine(ref _movingCoroutine);
        _movingCoroutine = Timing.RunCoroutine(HandleGoTo(transform.localPosition));
        
        if (WaterHandler.Instance.WaterEdgeOn)
        {
            KillFishie();
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

    public void KillFishie()
    {
        IsAlive = false;
        WaterHandler.Instance.StopAllWaterInteractions();
        TimingHelpers.CleanlyKillCoroutine(ref _movingCoroutine);
        _animator.SetTrigger("Die");

        AudioManager.Instance.PlayDeath();

        Timing.CallDelayed(2f, () =>
        {
            EOGUi.SetActive(true);
            AudioManager.Instance.PlayLoseJingle();
        });
    }

    public void Victory()
    {
        IsAlive = false;

        WaterHandler.Instance.StopAllWaterInteractions();
        TimingHelpers.CleanlyKillCoroutine(ref _movingCoroutine);
        AudioManager.Instance.PlayWinJingle();

        Timing.CallDelayed(2f, () =>
        {
            EOGUiWin.SetActive(true);
        });
    }
}