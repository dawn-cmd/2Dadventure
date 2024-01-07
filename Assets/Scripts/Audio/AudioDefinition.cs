using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDefinition : MonoBehaviour
{
    public PlayAudioEvent playAudioEvent;
    public AudioClip clip;
    public bool playOnStart;
    private void OnEnable()
    {
        if (playOnStart) PlayAudioClip();
    }
    public void PlayAudioClip()
    {
        playAudioEvent.RaiseEvent(clip);
    }
}
