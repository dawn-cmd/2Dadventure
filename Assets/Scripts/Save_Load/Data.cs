using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Data
{
    public string sceneToSave;
    public Dictionary<string, Vector3> characterPosDist = new();
    public Dictionary<string, float> floatSavedData = new();
    public void SaveGameScene(GameSceneSO scene)
    {
        sceneToSave = JsonUtility.ToJson(scene);
        Debug.Log(sceneToSave);
    }
    public GameSceneSO GetSavedScene() 
    {
        var newScene = ScriptableObject.CreateInstance<GameSceneSO>();
        JsonUtility.FromJsonOverwrite(sceneToSave, newScene);
        return newScene;
    }
}
