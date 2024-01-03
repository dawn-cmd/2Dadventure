using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public PlayerStateBar playerStateBar;
    [Header("事件监听")]
    public CharacterEvent healthEvent;
    private void OnEnable() {
        healthEvent.OnEventRaised += OnHealthEvent;
    }
    private void OnDisable() {
        healthEvent.OnEventRaised -= OnHealthEvent;
    }

    private void OnHealthEvent(Character character)
    {
        var healthPercentage = character.currentHealth / character.maxHealth;
        playerStateBar.OnHealthChanged(healthPercentage);
    }
}
