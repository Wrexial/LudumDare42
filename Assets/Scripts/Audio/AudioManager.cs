using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource Source;
    public AudioSource OneShotSource;

    public Button MuteButton;
    public Image MuteButtonImage;
    public Sprite SoundOff;
    public Sprite SoundOn;

    public AudioClip MenuMusic;
    public AudioClip GameMusic;

    public LoopableHandler CatTheme;
    public LoopableHandler FaucetEffect;
    public LoopableHandler DrainEffect;

    public AudioClip[] CatSpawn;
    public AudioClip[] Death;
    public AudioClip[] Swiming;
    public AudioClip[] Pickup;
    public AudioClip[] FinalPickup;

    private void Awake()
    {
        Instance = this;
        MuteButton.onClick.AddListener(ToggleHandle);
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene current, Scene next)
    {
        if (next.buildIndex == 3)
        {
            PlayGameMusic();
        }
        else
        {
            PlayMenuMusic();
        }
    }

    private void PlayMenuMusic()
    {
        if (Source.clip == MenuMusic)
        {
            return;
        }

        Source.clip = MenuMusic;
        Source.Play();
    }

    private void PlayGameMusic()
    {
        if (Source.clip == GameMusic)
        {
            return;
        }

        Source.clip = GameMusic;
        Source.Play();
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
}