using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/CharacterEvent")]
public class CharacterEvent : ScriptableObject
{
    public UnityAction<Character> OnEventRaised;
    public void Raise(Character character)
    {
        OnEventRaised?.Invoke(character);
    }
}
