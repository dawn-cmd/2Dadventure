using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("广播")]
    public FloatEventSO syncVolumeEvent;
    [Header("事件监听")]
    public PlayAudioEvent fxEvent;
    public PlayAudioEvent bgmEvent;
    public FloatEventSO volumeEvent;
    public VoidEventSO pauseEvent;
    public AudioSource BGMSource;
    public AudioSource FXSource;
    public AudioMixer mixer;
    private void OnEnable()
    {
        fxEvent.OnEventRaised += PlayFX;
        bgmEvent.OnEventRaised += PlayBGM;
        volumeEvent.OnEventRaised += OnVolumeEvent;
        pauseEvent.OnEventRaised += OnPauseEvent;
    }

    private void OnPauseEvent()
    {
        mixer.GetFloat("MasterVolume", out float amount);
        syncVolumeEvent.Raise(amount);
    }

    private void OnDisable()
    {
        fxEvent.OnEventRaised -= PlayFX;
        bgmEvent.OnEventRaised -= PlayBGM;
        volumeEvent.OnEventRaised -= OnVolumeEvent;
        pauseEvent.OnEventRaised -= OnPauseEvent;
    }

    private void OnVolumeEvent(float amount)
    {
        mixer.SetFloat("MasterVolume", amount * 100 - 80);
    }

    private void PlayBGM(AudioClip arg0)
    {
        Debug.Log("PlayBGM");
        BGMSource.clip = arg0;
        BGMSource.Play();
    }

    private void PlayFX(AudioClip arg0)
    {
        Debug.Log("PlayFX");
        FXSource.PlayOneShot(arg0);
        FXSource.Play();
    }
}
