using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour, ISavable
{
    public Transform playerTrans;
    public Vector3 firstPos;
    public Vector3 menuPos;
    [Header("事件监听")]
    public SceneLoadEventSO LoadEventSO;
    public VoidEventSO newGame;
    public VoidEventSO backToMenuEvent;

    public GameSceneSO firstLoadScene;
    public GameSceneSO menuScene;
    private GameSceneSO sceneToLoad;
    [SerializeField] private GameSceneSO currentScene;
    private Vector3 posToGo;
    private bool fadeScreen;
    public float fadeDuration;
    private bool isLoading;
    [Header("广播")]
    public SceneLoadEventSO unloadedSceneEvent;
    public VoidEventSO afterSceneLoadedEvent;
    public FadeEventSO fadeEvent;
    private void Awake()
    {
        // Addressables.LoadSceneAsync(firstLoadScene.sceneRef, LoadSceneMode.Additive);
        // currentScene = firstLoadScene;
        // currentScene.sceneRef.LoadSceneAsync(LoadSceneMode.Additive, true);

    }
    private void Start()
    {
        // NewGame();
        LoadEventSO.RaiseLoadRequestEvent(menuScene, menuPos, true);
    }
    private void OnEnable()
    {
        LoadEventSO.LoadRequestEvent += OnLoadRequestEvent;
        newGame.OnEventRaised += NewGame;
        backToMenuEvent.OnEventRaised += OnBackToMenu;
        ISavable savable = this;
        savable.RegisterSaveDate();
    }
    private void OnDisable()
    {
        LoadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        newGame.OnEventRaised -= NewGame;
        backToMenuEvent.OnEventRaised -= OnBackToMenu;
        ISavable savable = this;
        savable.UnRegisterSaveDate();
    }
    private void OnBackToMenu()
    {
        LoadEventSO.RaiseLoadRequestEvent(menuScene, menuPos, true);
    }
    private void NewGame()
    {
        sceneToLoad = firstLoadScene;
        LoadEventSO.RaiseLoadRequestEvent(firstLoadScene, firstPos, true);
    }

    private void OnLoadRequestEvent(GameSceneSO sceneToLoad, Vector3 posToGo, bool fadeScreen)
    {
        Debug.Log("OnLoadRequestEvent");
        if (isLoading) return;
        isLoading = true;
        this.sceneToLoad = sceneToLoad;
        this.posToGo = posToGo;
        this.fadeScreen = fadeScreen;
        Debug.Log(this.sceneToLoad);
        if (currentScene == null)
        {
            LoadNewScene();
        }
        else
        {
            StartCoroutine(UnloadPreviousScene());
        }
    }
    private IEnumerator UnloadPreviousScene()
    {
        if (fadeScreen)
        {
            fadeEvent.FadeIn(fadeDuration);
        }
        yield return new WaitForSeconds(fadeDuration);
        unloadedSceneEvent.RaiseLoadRequestEvent(sceneToLoad, posToGo, true);
        yield return currentScene.sceneRef.UnLoadScene();
        playerTrans.gameObject.SetActive(false);
        LoadNewScene();
    }

    private void LoadNewScene()
    {
        var loadingOption = sceneToLoad.sceneRef.LoadSceneAsync(LoadSceneMode.Additive, true);
        loadingOption.Completed += OnLoadCompleted;
    }

    private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> handle)
    {
        currentScene = sceneToLoad;
        playerTrans.position = posToGo;
        playerTrans.gameObject.SetActive(true);
        if (fadeScreen)
        {
            fadeEvent.FadeOut(fadeDuration);
        }
        isLoading = false;
        if (currentScene.sceneType != SceneType.Menu)
            afterSceneLoadedEvent.Raise();
    }

    public DataDefinition GetDataID()
    {
        return GetComponent<DataDefinition>();
    }

    public void GetSaveData(Data data)
    {
        data.SaveGameScene(currentScene);
    }

    public void LoadData(Data data)
    {
        var playerID = playerTrans.GetComponent<DataDefinition>().ID;
        if (data.characterPosDist.ContainsKey(playerID))
        {
            posToGo = data.characterPosDist[playerID];
            sceneToLoad = data.GetSavedScene();
            OnLoadRequestEvent(sceneToLoad, posToGo, true);
        }
    }
}
