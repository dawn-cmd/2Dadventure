using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/SceneLoadEventSO")]
public class SceneLoadEventSO : ScriptableObject {
    public UnityAction<GameSceneSO, Vector3, bool> LoadRequestEvent;
    public void RaiseLoadRequestEvent(GameSceneSO sceneToLoad, Vector3 teleportPosition, bool fadeScreen) {
        LoadRequestEvent?.Invoke(sceneToLoad, teleportPosition, fadeScreen);
    }
}
