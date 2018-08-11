using UnityEngine;

public class CatHandler : MonoBehaviour
{
    public static CatHandler Instance;

    public GameObject Cat;
    public GameObject CatWaves;
    public GameObject CatAttackIndicatorPrefab;

    public bool IsCatActive { get { return Cat.activeSelf; } }
    private int _round;

    private void Awake()
    {
        Instance = this;
        Cat.SetActive(false);
    }

    public void HandleCat()
    {

    }
}