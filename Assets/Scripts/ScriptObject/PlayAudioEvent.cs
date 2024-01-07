using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "Event/PlayAudioEventSO")]
public class PlayAudioEvent : ScriptableObject
{
    public UnityAction<AudioClip> OnEventRaised;
    public void RaiseEvent(AudioClip clip)
    {
        OnEventRaised?.Invoke(clip);
    }
}
