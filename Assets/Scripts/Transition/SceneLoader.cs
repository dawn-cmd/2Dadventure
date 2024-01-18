using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Transform playerTrans;
    public Vector3 firstPos;
    public SceneLoadEventSO LoadEventSO;
    public GameSceneSO firstLoadScene;
    private GameSceneSO sceneToLoad;
    [SerializeField] private GameSceneSO currentScene;
    private Vector3 posToGo;
    private bool fadeScreen;
    public float fadeDuration;
    private bool isLoading;
    [Header("广播")]
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
        NewGame();
    }
    private void OnEnable()
    {
        LoadEventSO.LoadRequestEvent += OnLoadRequestEvent;
    }
    private void OnDisable()
    {
        LoadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
    }
    private void NewGame()
    {
        // sceneToLoad = firstLoadScene;
        Debug.Log(firstLoadScene);
        Debug.Log(firstPos);
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
        afterSceneLoadedEvent.Raise();
    }
}
