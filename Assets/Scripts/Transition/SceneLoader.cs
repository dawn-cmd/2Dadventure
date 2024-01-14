using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public SceneLoadEventSO LoadEventSO;
    public GameSceneSO firstLoadScene;
    private GameSceneSO sceneToLoad;
    [SerializeField] private GameSceneSO currentScene;
    private Vector3 posToGo;
    private bool fadeScreen;
    public float fadeDuration;
    private void Awake()
    {
        // Addressables.LoadSceneAsync(firstLoadScene.sceneRef, LoadSceneMode.Additive);
        currentScene = firstLoadScene;
        currentScene.sceneRef.LoadSceneAsync(LoadSceneMode.Additive, true);
    }
    private void OnEnable()
    {
        LoadEventSO.LoadRequestEvent += OnLoadRequestEvent;
    }
    private void OnDisable()
    {
        LoadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
    }

    private void OnLoadRequestEvent(GameSceneSO sceneToLoad, Vector3 posToGo, bool fadeScreen)
    {
        this.sceneToLoad = sceneToLoad;
        this.posToGo = posToGo;
        this.fadeScreen = fadeScreen;
        Debug.Log(sceneToLoad.sceneRef.SubObjectName);
        if (currentScene.sceneRef.SubObjectName != null)
        {
            StartCoroutine(UnloadPreviousScene());
        }
    }
    private IEnumerator UnloadPreviousScene()
    {
        if (fadeScreen)
        {

        }
        yield return new WaitForSeconds(fadeDuration);
        yield return currentScene.sceneRef.UnLoadScene();
        LoadNewScene();
    }

    private void LoadNewScene()
    {
        sceneToLoad.sceneRef.LoadSceneAsync(LoadSceneMode.Additive, true);
    }
}
