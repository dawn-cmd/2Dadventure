using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public PlayerStateBar playerStateBar;
    [Header("事件监听")]
    public CharacterEvent healthEvent;
    public SceneLoadEventSO loadScene;
    private void OnEnable()
    {
        healthEvent.OnEventRaised += OnHealthEvent;
        loadScene.LoadRequestEvent += OnLoadRequest;
    }
    private void OnDisable()
    {
        healthEvent.OnEventRaised -= OnHealthEvent;
        loadScene.LoadRequestEvent -= OnLoadRequest;
    }

    private void OnLoadRequest(GameSceneSO sceneToLoad, Vector3 arg1, bool arg2)
    {
        if (sceneToLoad.sceneType == SceneType.Menu)
            playerStateBar.gameObject.SetActive(false);
        else
            playerStateBar.gameObject.SetActive(true);
    }

    private void OnHealthEvent(Character character)
    {
        var healthPercentage = character.currentHealth / character.maxHealth;
        playerStateBar.OnHealthChanged(healthPercentage);
        playerStateBar.OnPowerChanged(character);
    }
}
