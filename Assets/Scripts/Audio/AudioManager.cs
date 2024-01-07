using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("事件监听")]
    public PlayAudioEvent fxEvent;
    public PlayAudioEvent bgmEvent;
    public AudioSource BGMSource;
    public AudioSource FXSource;
    private void OnEnable() {
        fxEvent.OnEventRaised += PlayFX;
        bgmEvent.OnEventRaised += PlayBGM;
    }
    private void OnDisable() {
        fxEvent.OnEventRaised -= PlayFX;
        bgmEvent.OnEventRaised -= PlayBGM;
    }    
    private void PlayBGM(AudioClip arg0)
    {
        BGMSource.clip = arg0;
        BGMSource.Play();
    }

    private void PlayFX(AudioClip arg0)
    {
        FXSource.PlayOneShot(arg0);
        FXSource.Play();
    }
}
