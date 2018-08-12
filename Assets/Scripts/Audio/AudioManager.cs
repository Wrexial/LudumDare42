using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Transform>().gameObject.AddComponent<AudioManager>();
            }

            return _instance;
        }
        private set { _instance = value; }
    }
    private static AudioManager _instance;

    public AudioSource Source;
    public AudioSource OneShotSource;

    public Button MuteButton;
    public Image MuteButtonImage;
    public Sprite SoundOff;
    public Sprite SoundOn;

    public LoopableHandler CatTheme;
    public LoopableHandler FaucetEffect;
    public LoopableHandler DrainEffect;

    public AudioClip WinJingle;
    public AudioClip LoseJingle;
    public AudioClip[] CatSpawn;
    public AudioClip[] Death;
    public AudioClip[] Swiming;
    public AudioClip[] Pickup;
    public AudioClip[] FinalPickup;
    public AudioClip[] ButtonPress;

    private void Awake()
    {
        Instance = this;
        MuteButton.onClick.AddListener(ToggleHandle);
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene current, Scene next)
    {
    }

    private void ToggleHandle()
    {
        if (AudioListener.volume == 0)
        {
            AudioListener.volume = 1;
            MuteButtonImage.sprite = SoundOn;
        }
        else
        {
            AudioListener.volume = 0;
            MuteButtonImage.sprite = SoundOff;
        }
    }

    public void PlayCatAttack()
    {
        OneShotSource.PlayOneShot(CatSpawn[Random.Range(0, CatSpawn.Length)]);
    }

    public void PlayDeath()
    {
        OneShotSource.PlayOneShot(Death[Random.Range(0, Death.Length)]);
    }

    public void PlayPickup()
    {
        OneShotSource.PlayOneShot(Pickup[Random.Range(0, Pickup.Length)]);
    }

    public void PlayFinalPickup()
    {
        OneShotSource.PlayOneShot(FinalPickup[Random.Range(0, FinalPickup.Length)]);
    }

    public void PlayWinJingle()
    {
        OneShotSource.PlayOneShot(WinJingle);
    }

    public void PlayLoseJingle()
    {
        OneShotSource.PlayOneShot(LoseJingle);
    }

    public void PlayButtonPress()
    {
        OneShotSource.PlayOneShot(ButtonPress[Random.Range(0, ButtonPress.Length)]);
    }
}