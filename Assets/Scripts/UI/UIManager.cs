using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public PlayerStateBar playerStateBar;
    [Header("事件监听")]
    public CharacterEvent healthEvent;
    public SceneLoadEventSO unloadedSceneEvent;
    public VoidEventSO loadDataEvent;
    public VoidEventSO gameOverEvent;
    public VoidEventSO backToMenuEvent;
    public FloatEventSO syncVolumeEvent;
    [Header("广播")]
    public VoidEventSO pauseEvent;
    [Header("组件")]
    public GameObject gameOverPanel;
    public GameObject restartButton;
    public GameObject mobileTouch;
    public Button settingsButton;
    public GameObject settingsPanel;
    public Slider volumeSlider;
    private void Awake()
    {
#if UNITY_STANDALONE
        // Disable mobile touch controls for standalone builds (Windows, Mac, Linux)
        mobileTouch.SetActive(false);
#endif
        settingsButton.onClick.AddListener(TogglePausePanel);
    }
    private void OnEnable()
    {
        healthEvent.OnEventRaised += OnHealthEvent;
        unloadedSceneEvent.LoadRequestEvent += OnUnLoadedSceneEvent;
        loadDataEvent.OnEventRaised += OnLoadDataEvent;
        gameOverEvent.OnEventRaised += OnGameOverEvent;
        backToMenuEvent.OnEventRaised += OnLoadDataEvent;
        syncVolumeEvent.OnEventRaised += OnSyncVolumeEvent;
    }

    private void OnSyncVolumeEvent(float amount)
    {
        volumeSlider.value = (amount + 80) / 100;
    }

    private void OnDisable()
    {
        healthEvent.OnEventRaised -= OnHealthEvent;
        unloadedSceneEvent.LoadRequestEvent -= OnUnLoadedSceneEvent;
        loadDataEvent.OnEventRaised -= OnLoadDataEvent;
        gameOverEvent.OnEventRaised -= OnGameOverEvent;
        backToMenuEvent.OnEventRaised -= OnLoadDataEvent;
        syncVolumeEvent.OnEventRaised -= OnSyncVolumeEvent;
    }
    private void TogglePausePanel()
    {
        Debug.Log(settingsPanel.activeInHierarchy);
        if (settingsPanel.activeInHierarchy)
        {
            Debug.Log("Close");
            settingsPanel.SetActive(false);
        }
        else
        {
            Debug.Log("Open");
            pauseEvent.Raise();
            settingsPanel.SetActive(true);
        }
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
