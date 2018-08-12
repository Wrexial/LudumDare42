using MEC;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LoopableHandler : MonoBehaviour
{
    public AudioClip StartClip;
    public AudioClip LoopClip;
    public AudioClip EndClip;
    public bool CrossFade;
    public AnimationCurve CrossFadeOnCurve;
    public AnimationCurve CrossFadeOffCurve;

    private AudioSource _source;
    private AudioSource _managerAudioSource;

    private bool _playing = false;
    private CoroutineHandle? _audioHandlingCoroutine;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        TimingHelpers.CleanlyKillCoroutine(ref _audioHandlingCoroutine);
    }

    public void StartPlaying()
    {
        if (_playing)
        {
            return;
        }

        _playing = true;
        TimingHelpers.CleanlyKillCoroutine(ref _audioHandlingCoroutine);
        _audioHandlingCoroutine = Timing.RunCoroutine(Play());
    }

    public void EndPlaying()
    {
        _playing = false;
    }

    private IEnumerator<float> Play()
    {
        if (_managerAudioSource == null)
        {
            _managerAudioSource = AudioManager.Instance.Source;
        }

        var startVolume = 0f;
        var endVolume = _source.volume;
        var managerStartVolume = 0f;
        var managerEndVolume = _managerAudioSource.volume;

        if (StartClip != null)
        {
            _source.clip = StartClip;
            _source.loop = false;
            _source.Play();

            if (CrossFade)
            {
                _source.volume = startVolume;

                var clipLength = _source.clip.length;
                var delta = 0f;
                var timer = 0f;

                while (delta != 1)
                {
                    timer += Time.deltaTime;
                    delta = Mathf.Clamp01(timer / clipLength);
                    _source.volume = Mathf.Lerp(startVolume, endVolume, CrossFadeOnCurve.Evaluate(delta));
                    _managerAudioSource.volume = Mathf.Lerp(managerStartVolume, managerEndVolume, CrossFadeOffCurve.Evaluate(delta));
                    yield return Timing.WaitForOneFrame;
                }

                _source.volume = endVolume;
                _managerAudioSource.volume = managerStartVolume;
            }
            else
            {
                yield return Timing.WaitForSeconds(_source.clip.length);
            }
        }

        _source.clip = LoopClip;
        _source.loop = true;
        _source.Play();

        while (_playing)
            yield return Timing.WaitForOneFrame;

        _source.clip = EndClip;
        _source.loop = false;
        _source.Play();

        if (CrossFade)
        {
            var clipLength = _source.clip.length;
            var delta = 0f;
            var timer = 0f;

            while (delta != 1)
            {
                timer += Time.deltaTime;
                delta = Mathf.Clamp01(timer / clipLength);
                _source.volume = Mathf.Lerp(startVolume, endVolume, CrossFadeOffCurve.Evaluate(delta));
                _managerAudioSource.volume = Mathf.Lerp(managerStartVolume, managerEndVolume, CrossFadeOnCurve.Evaluate(delta));
                yield return Timing.WaitForOneFrame;
            }

            _source.volume = endVolume;
            _managerAudioSource.volume = managerEndVolume;
        }
        else
        {
            yield return Timing.WaitForSeconds(_source.clip.length);
        }

        _source.Stop();
        _source.clip = null;
    }
}
