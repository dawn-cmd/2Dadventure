using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPoint : MonoBehaviour, IInteractable
{
    public SceneLoadEventSO LoadEventSO;
    public Vector3 teleportPosition;
    public GameSceneSO sceneToGo;
    public void TriggerAction()
    {
        Debug.Log("Teleport!!!");
        LoadEventSO.RaiseLoadRequestEvent(sceneToGo, teleportPosition, true);
    }

}
