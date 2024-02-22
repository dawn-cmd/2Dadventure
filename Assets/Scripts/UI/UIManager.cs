using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public PlayerStateBar playerStateBar;
    [Header("事件监听")]
    public CharacterEvent healthEvent;
    public SceneLoadEventSO unloadedSceneEvent;
    public VoidEventSO loadDataEvent;
    public VoidEventSO gameOverEvent;
    public VoidEventSO backToMenuEvent;
    [Header("组件")]
    public GameObject gameOverPanel;
    public GameObject restartButton;
    public GameObject mobileTouch;
    private void Awake()
    {
#if UNITY_STANDALONE
        // Disable mobile touch controls for standalone builds (Windows, Mac, Linux)
        mobileTouch.SetActive(false);
#endif
    }
    private void OnEnable()
    {
        healthEvent.OnEventRaised += OnHealthEvent;
        unloadedSceneEvent.LoadRequestEvent += OnUnLoadedSceneEvent;
        loadDataEvent.OnEventRaised += OnLoadDataEvent;
        gameOverEvent.OnEventRaised += OnGameOverEvent;
        backToMenuEvent.OnEventRaised += OnLoadDataEvent;
    }
    private void OnDisable()
    {
        healthEvent.OnEventRaised -= OnHealthEvent;
        unloadedSceneEvent.LoadRequestEvent -= OnUnLoadedSceneEvent;
        loadDataEvent.OnEventRaised -= OnLoadDataEvent;
        gameOverEvent.OnEventRaised -= OnGameOverEvent;
        backToMenuEvent.OnEventRaised -= OnLoadDataEvent;
    }

    private void OnGameOverEvent()
    {
        gameOverPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(restartButton);
    }

    private void OnLoadDataEvent()
    {
        gameOverPanel.SetActive(false);

    }

    private void OnUnLoadedSceneEvent(GameSceneSO sceneToLoad, Vector3 arg1, bool arg2)
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
